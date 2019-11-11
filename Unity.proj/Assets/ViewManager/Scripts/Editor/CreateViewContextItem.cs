using UnityEditor;
using UnityEngine;

namespace Copper.ViewManager.Editor {
    using Utils;

    public class CreateViewContextItem
    {
        /// <summary>
        /// Creates an empty game object with Anchors and Offsets set up for a View
        /// </summary>
        [MenuItem("GameObject/Create Empty View", false, 0)]
        private static void CreateView()
        {
            RectTransform viewObj = (new GameObject("View")).AddComponent<RectTransform>();
            viewObj.transform.SetParent(Selection.activeTransform);
            viewObj.Reset(RectTransformExtensions.AnchorModes.Stretch);
        }
    }
}
