namespace Copper.ViewManager
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using ScriptableObjects;
    using UnityEngine;

    public class LayerCollection : IEnumerable
    {
        private Transform viewContainer;
        private Dictionary<int, Layer> layers;

        public bool ContainsOverlayLayers { get; private set; }

        public Layer this[int layerID]
        {
            get => layers[layerID];
        }

        private LayerCollection() { }

        public LayerCollection(Transform viewContainer)
        {
            this.viewContainer = viewContainer;
            layers = new Dictionary<int, Layer>();
            ContainsOverlayLayers = false;
        }

        /// <summary>
        /// Registers a layer for use in the View Manager. Also creates the layer if it does not already exist.
        /// </summary>
        /// <param name="layerId">ID of the layer to register</param>
        /// <param name="name">Name that the layer should go by in the Hierarchy</param>
        /// <param name="shaderChannels">Any additional shader channels that you want this layer to support</param>
        //private void RegisterLayer(IComparable layerID, string name, AdditionalCanvasShaderChannels shaderChannels = AdditionalCanvasShaderChannels.None)
        public void RegisterLayer(LayerRecord layerRecord, int layerPosition)
        {
            if (layers.ContainsKey(layerPosition) == false)
            {
                layers.Add(layerPosition, new Layer());
            }

            layers[layerPosition].layerID = layerPosition;
            layers[layerPosition].name = string.IsNullOrEmpty(layerRecord.LayerName) ? $"Layer {layerPosition}" : layerRecord.LayerName;
            layers[layerPosition].shaderChannels = layerRecord.AdditionalShaderChannels;
            layers[layerPosition].isOverlay = layerRecord.IsOverlay;

            if (layerRecord.IsOverlay)
            {
                ContainsOverlayLayers = true;
            }
        }

        /// <summary>
        /// Creates and sorts all layers to ensure they fall in the proper order within the hierarchy
        /// </summary>
        /// <param name="shaderChannels">Any additional shader channels that you want this layer to support</param>
        public void CreateLayers()
        {
            List<int> sortedLayers = new List<int>(layers.Keys);
            sortedLayers.Sort();

            foreach (int layerKey in sortedLayers)
            {
                layers[layerKey].CreateInHierarchy(viewContainer);
            }
        }

        public Layer GetTopOccupiedOverlayLayer(params int[] ignoreViews)
        {
            List<int> sortedLayers = new List<int>(layers.Keys);
            sortedLayers.Sort();

            for (int i = sortedLayers.Count - 1; i >= 0; i--)
            {
                Layer layer = layers[sortedLayers[i]];
                if (layer.IsOccupied && layer.isOverlay && !Array.Exists(ignoreViews, viewID => viewID.Equals(layer.activeView.ViewID)))
                {
                    return layer;
                }
            }

            return null;
        }

        public IEnumerator GetEnumerator()
        {
            return layers.Values.GetEnumerator();
        }
    }
}