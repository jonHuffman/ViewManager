namespace Copper.ViewManager.Code
{
    using System;
    using System.Collections.Generic;
    using Interfaces;
    using ScriptableObjects;
    using UnityEngine.Assertions;

    public class ViewLoaderFactory
    {
        private Dictionary<string, IViewLoader> viewLoaders;

        public ViewLoaderFactory()
        {
            viewLoaders = new Dictionary<string, IViewLoader>();
        }

        public IViewLoader GetViewLoader(ViewInfo viewInfo)
        {
            if (!viewLoaders.TryGetValue(viewInfo.LoadType, out IViewLoader viewLoader))
            {
                Type loaderType = Type.GetType(viewInfo.LoadType);
                Assert.IsNotNull(loaderType);

                viewLoader = Activator.CreateInstance(loaderType) as IViewLoader;
                viewLoaders[viewInfo.LoadType] = viewLoader;
            }

            return viewLoader;
        }
    }
}