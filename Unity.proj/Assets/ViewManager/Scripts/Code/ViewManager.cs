namespace Copper.ViewManager.Code
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using Interfaces;
    using Layers;
    using ScriptableObjects;
    using UnityEngine;
    using UnityEngine.UI;
    using Utils;

    public sealed class ViewManager
    {
        private static readonly int UI_LAYER = LayerMask.NameToLayer("UI");

        public delegate void ViewChangedMethod(IComparable ID);

        public event ViewChangedMethod onViewOpened;
        public event ViewChangedMethod onViewClosed;

        private Canvas viewCanvas;
        private CanvasGroup canvasGroup;

        private RectTransform dialogContainer;

        private LayerCollection layerCollection;

        private ViewLoaderFactory viewLoaderFactory;

        private Dictionary<int, ViewInfo> registeredViews = new Dictionary<int, ViewInfo>();
        private Dictionary<int, IView> activeViews = new Dictionary<int, IView>();
        private Dictionary<int, IView> persistentViews = new Dictionary<int, IView>();

        // Collections tracking views currently being added/removed, to allow ignoring duplicate calls
        private HashSet<int> viewsToAddList = new HashSet<int>();
        private HashSet<int> viewsToRemoveList = new HashSet<int>();

        private GreyoutLayer greyoutLayer;

        private Thread mainThread;


        #region Singleton

        private static ViewManager instance;

        public static ViewManager Instance
        {
            get
            {
                if (instance == null)
                {
                    throw new NullReferenceException("ViewManager has not been instantiated. Please call ViewManager.Instantiate before trying to access the Instance,");
                }
                return instance;
            }
        }

        public static void Instantiate(Canvas viewCanvas, ViewRegistrar registrar)
        {
            instance = new ViewManager(viewCanvas, registrar);

        }

        private ViewManager(Canvas viewCanvas, ViewRegistrar registrar)
        {
            mainThread = Thread.CurrentThread;

            if (UI_LAYER == -1)
            {
                Debug.LogError("Could not find UI layer.");
            }

            LinkCanvas(viewCanvas);

            layerCollection = new LayerCollection(CreateViewContainer("Views"));
            CreateDialogContainer();

            viewLoaderFactory = new ViewLoaderFactory();

            RegisterLayers(registrar.LayerRecords);
            RegisterViews(registrar.ViewRecords);

            CreateLayers();
        }

        #endregion

        /// <summary>
        /// Defines custom logging methods for the ViewManager to use. If not set, the ViewManager will output through Unity's Debug class.
        /// </summary>
        public void SetDebugMethods(LogMethod log, LogMethod logWarning, LogMethod logError)
        {
            Logging.SetLogMethods(log, logWarning, logError);
        }

        /// <summary>
        /// Adds a View to the canvas on its pre-defined Layer.
        /// If there is already a view occupying that Layer it will be removed first.
        /// If the view is already active, it will not not get re-added.
        /// </summary>
        /// <param name="viewID">ID of the view to add.</param>
        public void AddView(int viewID)
        {
            AddView<NullData>(viewID, null);
        }

        /// <summary>
        /// Adds a View to the canvas on its pre-defined Layer.
        /// If there is already a view occupying that Layer it will be removed first.
        /// If the view is already active, it will not not get re-added.
        /// </summary>
        /// <typeparam name="T">The type of IViewData that you wish to pass to the view</typeparam>
        /// <param name="viewID">ID of the view to add</param>
        /// <param name="viewData">The data you wish to pass to the view</param>
        public void AddView<T>(int viewID, T viewData) /*where T : IViewData<T>*/
        {
            if (!ViewCanBeAdded(viewID))
            {
                return;
            }

            if (IsViewActive(viewID))
            {
                // If the view is in the process of being removed we will speed it along so that we can re-add it
                if (viewsToRemoveList.Contains(viewID))
                {
                    FinalizeRemoval(activeViews[viewID], null, true);
                }
                else
                {
                    Logging.LogWarning(string.Format("View with viewID {0} is already active.", viewID));
                    return;
                }
            }

            ViewInfo viewInfo = registeredViews[viewID];
            if (viewInfo.IsDialog)
            {
                // Dialogs are simply added as the top-most item within the Dialogs layer
                // We do not need to worry about something else occupying the layer
                FinalizeAddView<T>(viewInfo, viewData);
                return;
            }

            Layer targetLayer = layerCollection[viewInfo.LayerID];
            if (targetLayer.IsOccupied)
            {
                viewsToAddList.Add(viewInfo.ViewID);
                RemoveView(targetLayer.activeView.ViewID, () => FinalizeAddView<T>(viewInfo, viewData));
            }
            else
            {
                FinalizeAddView<T>(viewInfo, viewData);
            }
        }

        /// <summary>
        /// Removes a view from the view canvas
        /// </summary>
        /// <param name="viewID">viewID of the view to remove.</param>
        /// <param name="completeCallback">Fired on transition out</param>
        /// <param name="forceRemove">If true, this View object will be destroyed regardless of whether it is flagged for persistence.</param>
        public void RemoveView(int viewID, System.Action completeCallback = null, bool forceRemove = false)
        {
            if (!ViewCanBeRemoved(viewID))
            {
                return;
            }

            IView view = activeViews[viewID];
            viewsToRemoveList.Add(viewID);
            view.TransitionOut(() => FinalizeRemoval(view, completeCallback, forceRemove));
        }

        /// <summary>
        /// Removes all of the active views
        /// </summary>
        /// <param name="forceRemove">If true, all View objects will be destroyed regardless of whether they are flagged for persistence.</param>
        public void RemoveAllViews(bool forceRemove = false)
        {
            viewsToAddList.Clear();

            foreach (Layer layer in layerCollection)
            {
                if (layer.IsOccupied)
                {
                    RemoveView(layer.activeView.ViewID, null, forceRemove);
                }
            }

            greyoutLayer.DisableGreyout();
        }

        /// <summary>
        /// Removes all the view except for those on a specified exempt layerID.
        /// </summary>
        /// <param name="exemptLayers">The layers that you do not want to remove views from</param>
        /// <param name="forceRemove">If true, all non-exempt View objects will be destroyed regardless of whether they are flagged for persistence.</param>
        public void RemoveAllViews(int[] exemptLayers, bool forceRemove = false)
        {
            foreach (Layer layer in layerCollection)
            {
                if (layer.IsOccupied && !Array.Exists(exemptLayers, exemptLayerID => exemptLayerID.Equals(layer.layerID)))
                {
                    RemoveView(layer.activeView.ViewID, null, forceRemove);
                }
            }
        }

        /// <summary>
        /// Removes all of the active Dialogs
        /// </summary>
        /// <param name="forceRemove">If true, all View objects will be destroyed regardless of whether they are flagged for persistence.</param>
        public void RemoveAllDialogs(bool forceRemove = false)
        {
            foreach (KeyValuePair<int, IView> keyValuePair in activeViews)
            {
                if (keyValuePair.Value.IsDialog)
                {
                    RemoveView(keyValuePair.Value.ViewID, null, forceRemove);
                }
            }
        }

        /// <summary>
        /// Gets the viewID of the current view on the specified layerID. If there is no view on the layerID -1 will be returned.
        /// </summary>
        /// <param name="layerID">The layerID viewID to try and retrieve a  viewID for.</param>
        /// <returns>The view viewID of the view on the specified layerID. Returns -1 if no view occupies the layerID.</returns>
        public int GetViewIdOnLayer(int layerID)
        {
            if (layerCollection[layerID].IsOccupied)
            {
                return layerCollection[layerID].activeView.ViewID;
            }

            return -1;
        }

        /// <summary>
        /// Gets the name of a view on a specific layerID. Typically used for debugging.
        /// </summary>
        /// <param name="layerID">The layerID viewID to try and retrieve a view name for.</param>
        /// <returns>Name of the view on the layerID. If no view occupies the layerID, an empty string is returned.</returns>
        public string GetViewNameOnLayer(int layerID)
        {
            IView view = layerCollection[layerID].activeView;
            return view != null ? ((Component)view).name : string.Empty;
        }

        /// <summary>
        /// Returns true if a view is active. A view cannot be considered active until it has been created.
        /// </summary>
        public bool IsViewActive(int viewID)
        {
            return activeViews.ContainsKey(viewID) &&
                   activeViews[viewID] != null &&
                   !viewsToRemoveList.Contains(viewID);
        }

        /// <summary>
        /// Enables input for everything managed by the ViewManager
        /// </summary>
        public void EnableInput()
        {
            GetCanvasGroup().interactable = true;
        }

        /// <summary>
        /// Disables input for everything managed by the ViewManager
        /// </summary>
        public void DisableInput(bool enabled)
        {
            GetCanvasGroup().interactable = false;
        }

        /// <summary>
        /// Sets the target alpha of the greyout
        /// </summary>
        /// <param name="alpha">Target alpha</param>
        public void SetGreyoutAlpha(float alpha)
        {
            greyoutLayer.SetAlpha(alpha);
        }

        private void LinkCanvas(Canvas canvas)
        {
            viewCanvas = canvas;
            UnityEngine.Object.DontDestroyOnLoad(viewCanvas.transform.root);
        }

        private void RegisterLayers(List<LayerRecord> layerRecords)
        {
            for (int i = 0; i < layerRecords.Count; i++)
            {
                layerCollection.RegisterLayer(layerRecords[i], i);
            }
        }

        private void CreateLayers()
        {
            layerCollection.CreateLayers();
            greyoutLayer = new GreyoutLayer(viewCanvas.rootCanvas, layerCollection);
        }

        private void RegisterViews(List<ViewInfo> viewInfo)
        {
            foreach (ViewInfo info in viewInfo)
            {
                if (!CanViewBeRegistered(info))
                {
                    continue;
                }

                registeredViews.Add(info.ViewID, info);

                if (info.Persistent)
                {
                    persistentViews.Add(info.ViewID, null);
                }
            }
        }

        private bool CanViewBeRegistered(ViewInfo viewInfo)
        {
            if (!viewInfo.HasName)
            {
                Logging.LogWarning("A View in the View Registrar has not been assigned a name. Skipping registration of this View.");
                return false;
            }

            if (registeredViews.ContainsKey(viewInfo.ViewID))
            {
                Logging.LogWarning($"A View with viewID {viewInfo.Name} has already been registered. This view will not be registered.");
                return false;
            }

            if (string.IsNullOrEmpty(viewInfo.ViewPath))
            {
                Logging.LogWarning($"The view registrar contains an empty View Path for View with viewID {viewInfo.Name}. This view will not be registered.");
                return false;
            }

            return true;
        }

        private bool ViewCanBeAdded(int viewID)
        {
            if (mainThread.Equals(Thread.CurrentThread) == false)
            {
                Logging.LogWarning($"The View with viewID {viewID} can't be added from an other thread. No View will be shown.");
                return false;
            }

            if (!registeredViews.ContainsKey(viewID))
            {
                Logging.LogWarning($"No View with viewID {viewID} has been registered. No View will be shown.");
                return false;
            }

            if (viewsToAddList.Contains(viewID))
            {
                Logging.LogWarning($"View with viewID {viewID} is already in the process of being added. You cannot re-add an existing view.");
                return false;
            }

            return true;
        }

        private bool ViewCanBeRemoved(int viewID)
        {
            if (!registeredViews.ContainsKey(viewID))
            {
                Logging.LogWarning($"No View with viewID {viewID} has been registered. No View will be removed.");
                return false;
            }

            if (!activeViews.ContainsKey(viewID) || activeViews[viewID] == null)
            {
                Logging.LogWarning($"View with viewID {viewID} is not active and does not need to be removed.");
                return false;
            }

            if (viewsToRemoveList.Contains(viewID))
            {
                Logging.LogWarning($"View with viewID {viewID} is already in the process of being removed");
                return false;
            }

            return true;
        }

        private void FinalizeAddView<T>(ViewInfo viewInfo, ViewData<T> viewData)
        {
            CreateAndAddView(viewInfo, viewData);
            DispatchViewOpened(viewInfo.ViewID);
            viewsToAddList.Remove(viewInfo.ViewID);
        }

        /// <summary>
        /// Fires the onViewOpened event with the viewID provided
        /// </summary>
        /// <param name="ID">viewID of the view that opened</param>
        private void DispatchViewOpened(IComparable ID)
        {
            onViewOpened?.Invoke(ID);
        }

        /// <summary>
        /// Fires the onViewClosed event with the viewID provided
        /// </summary>
        /// <param name="ID">viewID of the view that closed</param>
        private void DispatchViewClosed(IComparable ID)
        {
            onViewClosed?.Invoke(ID);
        }

        private RectTransform CreateViewContainer(string name)
        {
            GameObject gameObject = new GameObject(name);
            gameObject.layer = UI_LAYER;

            RectTransform rectTransform = gameObject.AddComponent<RectTransform>();
            rectTransform.SetParent(viewCanvas.transform);
            rectTransform.Reset(RectTransformExtensions.AnchorModes.Stretch);

            return rectTransform;
        }

        private void CreateDialogContainer()
        {
            // Dialogs should be the last sibling in order to render on top of all other views
            dialogContainer = CreateViewContainer("Dialogs");
            dialogContainer.gameObject.AddComponent<Canvas>();
            dialogContainer.gameObject.AddComponent<GraphicRaycaster>();
            dialogContainer.SetAsLastSibling();
        }

        /// <summary>
        /// Creates and transitions in the View. 
        /// If the view is marked as a Dialog, then a new Layer is created for it and placed above all others.
        /// If the view is marked to be kept alive, then it is activated and re-runs its initialization logic.
        /// </summary>
        /// <param name="viewInfo">The info required to create the View</param>
        /// <param name="viewData"></param>
        private void CreateAndAddView<T>(ViewInfo viewInfo, ViewData<T> viewData)
        {
            // Create or Retrieve the View object
            IView view = GetView(viewInfo);

            if (view != null)
            {
                if (viewInfo.IsDialog == false)
                {
                    Layer layer = layerCollection[viewInfo.LayerID];
                    layer.SetActiveView(view);
                    view.SetParentTransform(layer.RectTransform);
                }
                else
                {
                    view.SetParentTransform(dialogContainer);
                }

                activeViews[viewInfo.ViewID] = view;

                view.SetViewInfo(viewInfo);

                if (view is IViewDataReceiver<T> viewDataReceiver)
                {
                    viewDataReceiver.SetViewData(viewData);
                }

                view.Initialize();
                view.TransitionIn();

                greyoutLayer.UpdateGreyoutPosition((BaseView)view, viewsToRemoveList);
            }
            else
            {
                Logging.LogError(string.Format("The view object you attempted to add is not of type IView: {0}", viewInfo.ViewID));
            }
        }

        private bool IsSubclassOfRawGeneric(Type generic, Type toCheck)
        {
            while (toCheck != null && toCheck != typeof(object))
            {
                Type currentType = toCheck.IsGenericType ? toCheck.GetGenericTypeDefinition() : toCheck;
                if (generic == currentType)
                {
                    return true;
                }
                toCheck = toCheck.BaseType;
            }
            return false;
        }

        private IView GetView(ViewInfo viewInfo)
        {
            IView view;
            if (viewInfo.Persistent && persistentViews[viewInfo.ViewID] != null)
            {
                view = GetPersistentView(viewInfo);
            }
            else
            {
                IViewLoader viewLoader = viewLoaderFactory.GetViewLoader(viewInfo);
                view = viewLoader.CreateView(viewInfo);

                if (viewInfo.Persistent)
                {
                    persistentViews[viewInfo.ViewID] = view;
                }
            }

            return view;
        }

        private IView GetPersistentView(ViewInfo viewInfo)
        {
            IView view = persistentViews[viewInfo.ViewID];
            ((Component)view).gameObject.SetActive(true);
            return view;
        }

        private void FinalizeRemoval(IView view, System.Action completeCallback = null, bool forceRemove = false)
        {
            // It is possible for FinalizeRemoval to be called twice in a single frame in very rare instances.
            if (viewsToRemoveList.Contains(view.ViewID) == false)
            {
                return;
            }

            // Confirm that a new view on the same Layer has not been added in the middle
            // of processing the removal of this view before clearing the Layer's active view
            if (!view.IsDialog && view.ViewID == layerCollection[view.LayerID].activeView.ViewID)
            {
                layerCollection[view.LayerID].ClearActiveView();
            }

            // If this is a persistent view either disable it or destroy it. If it is not a persistent view, destroy it.
            if (persistentViews.ContainsKey(view.ViewID))
            {
                if (!forceRemove)
                {
                    ((Component)view).gameObject.SetActive(false);
                }
                else
                {
                    persistentViews[view.ViewID] = null;
                    UnityEngine.Object.Destroy(((Component)view).gameObject);
                }
            }
            else
            {
                UnityEngine.Object.Destroy(((Component)view).gameObject);
            }

            greyoutLayer.UpdateGreyoutPosition((BaseView)view, viewsToRemoveList);

            // Performs the destroy action of the View class, does not actually destroy the GameObject
            view.DestroyView();
            activeViews.Remove(view.ViewID);

            DispatchViewClosed(view.ViewID);
            viewsToRemoveList.Remove(view.ViewID);

            completeCallback?.Invoke();
        }

        /// <summary>
        /// Gets the view canvas' CanvasGroup component, adding one if none exists.
        /// </summary>
        private CanvasGroup GetCanvasGroup()
        {
            if (canvasGroup == null)
            {
                canvasGroup = viewCanvas.GetComponent<CanvasGroup>();

                if (canvasGroup == null)
                {
                    canvasGroup = viewCanvas.gameObject.AddComponent<CanvasGroup>();
                }
            }

            return canvasGroup;
        }
    }
}
