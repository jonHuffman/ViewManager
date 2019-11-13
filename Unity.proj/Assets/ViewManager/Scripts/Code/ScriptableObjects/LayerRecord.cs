#pragma warning disable 649
namespace Copper.ViewManager.Code.ScriptableObjects
{
    using System;
    using UnityEngine;

    [Serializable]
    public class LayerRecord
    {
        [SerializeField]
        private string layerName;
        [SerializeField]
        private bool isOverlay;
        [SerializeField]
        private AdditionalCanvasShaderChannels additionalShaderChannels;

        /// <summary>
        /// The name of the defined layer
        /// </summary>
        public string LayerName { get { return layerName; } }

        /// <summary>
        /// Is this layer an overlay or not in respect to the greyout
        /// </summary>
        public bool IsOverlay { get { return isOverlay; } }

        /// <summary>
        /// The additional shader channels to enable for this layer
        /// </summary>
        public AdditionalCanvasShaderChannels AdditionalShaderChannels { get { return additionalShaderChannels; } }

        public bool HasName { get => !string.IsNullOrEmpty(LayerName); }
    }
}
