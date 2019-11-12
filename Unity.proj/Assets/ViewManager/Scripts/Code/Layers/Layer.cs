namespace Copper.ViewManager.Code.Layers
{
    using Interfaces;
    using UnityEngine;
    using UnityEngine.UI;
    using Utils;

    internal class Layer
    {
        public int layerID;
        public string name;
        public AdditionalCanvasShaderChannels shaderChannels;
        public IView activeView;
        public bool isOverlay;

        public RectTransform RectTransform { get; private set; }
        public bool IsOccupied
        {
            get => activeView != null;
        }
        
        private static int uiLayerMask = -1;

        private static int UiLayerMaskMask
        {
            get
            {
                uiLayerMask = LayerMask.NameToLayer("UI");
                if (uiLayerMask == -1)
                {
                    Debug.LogError("Could not find UI layer.");
                }

                return uiLayerMask;
            }
        }

        public void CreateInHierarchy(Transform viewContainer)
        {
            GameObject gameObject = new GameObject(name);
            gameObject.layer = UiLayerMaskMask;

            RectTransform rectTransform = gameObject.AddComponent<RectTransform>();
            rectTransform.SetParent(viewContainer);
            rectTransform.Reset(RectTransformExtensions.AnchorModes.Stretch);
            rectTransform.SetAsLastSibling();

            gameObject.AddComponent<Canvas>().additionalShaderChannels = shaderChannels;
            gameObject.AddComponent<GraphicRaycaster>();

            RectTransform = rectTransform;
        }

        public void SetActiveView(IView view)
        {
            activeView = view;
        }

        public void ClearActiveView()
        {
            activeView = null;
        }
    }
}