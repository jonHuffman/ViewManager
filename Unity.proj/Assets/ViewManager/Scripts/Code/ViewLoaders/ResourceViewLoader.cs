namespace Copper.ViewManager.Code.ViewLoaders
{
    using Interfaces;
    using ScriptableObjects;
    using UnityEngine;

    public class ResourceViewLoader : IViewLoader
    {
        private const string RESOURCE_FOLDER_NAME = "Resources/";

        public IView CreateView(ViewInfo viewInfo)
        {
            string viewPath = PreparePath(viewInfo.ViewPath);
            GameObject newView = Object.Instantiate(Resources.Load<GameObject>(viewPath));
            Object.DontDestroyOnLoad(newView);

            return (IView)newView.GetComponent(typeof(IView));
        }

        private static string PreparePath(string viewPath)
        {
            if (viewPath.Contains(RESOURCE_FOLDER_NAME))
            {
                int startIndex = viewPath.IndexOf(RESOURCE_FOLDER_NAME);
                int resourcePathLength = startIndex + RESOURCE_FOLDER_NAME.Length;
                viewPath = viewPath.Substring(resourcePathLength, viewPath.Length - resourcePathLength);
                viewPath = viewPath.Substring(0, viewPath.IndexOf('.'));
            }

            return viewPath;
        }
    }
}