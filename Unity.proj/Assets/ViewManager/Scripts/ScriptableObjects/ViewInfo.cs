namespace Copper.ViewManager.ScriptableObjects
{
    using System;
    using AssetPathAttribute;
    using UnityEngine;

    [Serializable]
    public class ViewInfo
    {
        [SerializeField, Tooltip("A unique name that you will reference this view by")]
        private string viewID = string.Empty;
        [SerializeField, Tooltip("The layer that this view belongs to")]
        private int layerID = -1;
        [SerializeField, AssetPath.Attribute(typeof(RectTransform)), Tooltip("Reference to a view prefab")]
        private string view = string.Empty;
        [SerializeField, Tooltip("A View marked as a Dialog will ignore the Layer and instead be spawned above all existing layers")]
        private bool isDialog = false;
        [SerializeField, Tooltip("A View marked persistent will not be destroyed when closed. It is instead disabled")]
        private bool persistent = false;
        [SerializeField, Tooltip("The load method that this view should make use of.")]
        private string loadType = string.Empty;

        #region Properties
        /// <summary>
        /// A unique name that you will reference this view by
        /// </summary>
        public int ViewID { get => viewID.GetHashCode(); }

        /// <summary>
        /// The layer that this view belongs to
        /// </summary>
        public int LayerID { get => layerID; }

        /// <summary>
        /// Reference to a view prefab
        /// </summary>
        public string ViewPath { get => view; }

        /// <summary>
        /// Whether or not this View is considered a Dialog
        /// </summary>
        public bool IsDialog { get => isDialog; }

        /// <summary>
        /// Enable this if the view should not be destroyed when removed
        /// </summary>
        public bool Persistent { get => persistent; }

        /// <summary>
        /// An Assembly Qualified name that refers to the IViewLoader implementation used to load this view
        /// </summary>
        public string LoadType { get => loadType; }

        public bool HasName { get => !string.IsNullOrEmpty(viewID); }
        public string Name { get => viewID; }
        #endregion
    }
}
