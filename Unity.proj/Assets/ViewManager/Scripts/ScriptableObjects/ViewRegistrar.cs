#pragma warning disable 649

namespace Copper.ViewManager.ScriptableObjects
{
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// A scriptable object that handles the ragistration and organization of Views. 
    /// This Object is also responsible for providing the data for ViewConstants generation
    /// </summary>
    public class ViewRegistrar : ScriptableObject
    {
        [SerializeField]
        private List<LayerRecord> layers;

        [SerializeField]
        private List<ViewInfo> viewRecords;

        public List<LayerRecord> LayerRecords
        {
            get => layers;
        }

        public List<ViewInfo> ViewRecords
        {
            get => viewRecords;
        }
    }
}