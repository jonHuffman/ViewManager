namespace Copper.ViewManager.Code.Utils {
    using UnityEngine;

    /// <summary>
    /// A collection of extension methods for RectTransform that make it easier to work with.
    /// </summary>
    /// <remarks>Some of the functions here have been gathered from online resources, others have been written when needed.</remarks>
    public static class RectTransformExtensions
    {
        /// <summary>
        /// The anchor modes supported RectTransform.Reset()
        /// </summary>
        public enum AnchorModes
        {
            Center,
            Stretch
        }

        /// <summary>
        /// Resets the RectTransform to default values: 
        /// sets position and rotation to 0, 
        /// sets scale to 1, 
        /// sets pivot to (0.5, 0.5)
        /// updates size and anchor values based on the given anchor mode (Center by default)
        /// </summary>
        /// <param name="mode">The Anchor Mode to reset to</param>
        public static void Reset(this RectTransform trans, RectTransformExtensions.AnchorModes mode = RectTransformExtensions.AnchorModes.Center)
        {
            trans.localPosition = Vector3.zero;
            trans.localRotation = Quaternion.identity;
            trans.localScale = Vector3.one;
            trans.pivot = new Vector2(0.5f, 0.5f);

            if (mode == RectTransformExtensions.AnchorModes.Center)
            {
                trans.SetSize(new Vector2(100f, 100f));
                trans.anchorMin = new Vector2(0.5f, 0.5f);
                trans.anchorMax = new Vector2(0.5f, 0.5f);
            }
            else
            {
                trans.SetSize(Vector2.zero);
                trans.anchorMin = new Vector2(0f, 0f);
                trans.anchorMax = new Vector2(1f, 1f);
            }
        }

        /// <summary>
        /// Get the width of the RectTransform
        /// </summary>
        /// <returns>Width of the object</returns>
        public static float GetWidth(this RectTransform trans)
        {
            return trans.rect.width;
        }

        /// <summary>
        /// Get the height of the RectTransform
        /// </summary>
        /// <returns>Height of the object</returns>
        public static float GetHeight(this RectTransform trans)
        {
            return trans.rect.height;
        }

        /// <summary>
        /// Updates the width of the RectTransform to the specified size
        /// </summary>
        /// <param name="width">Width to set the RectTransform to</param>
        public static void SetWidth(this RectTransform trans, float width)
        {
            trans.SetSize(new Vector2(width, trans.rect.size.y));
        }


        /// <summary>
        /// Updates the height of the RectTransform to the specified size
        /// </summary>
        /// <param name="height">Height to set the RectTransform to</param>
        public static void SetHeight(this RectTransform trans, float height)
        {
            trans.SetSize(new Vector2(trans.rect.size.x, height));
        }

        /// <summary>
        /// Resizes the RectTransform with respect for the existing pivot.
        /// </summary>
        /// <param name="newSize">The size to change the RectTransform to</param>
        public static void SetSize(this RectTransform trans, Vector2 newSize)
        {
            Vector2 size = trans.rect.size;
            Vector2 vector = newSize - size;
            trans.offsetMin = trans.offsetMin - new Vector2(vector.x * trans.pivot.x, vector.y * trans.pivot.y);
            trans.offsetMax = trans.offsetMax + new Vector2(vector.x * (1f - trans.pivot.x), vector.y * (1f - trans.pivot.y));
        }

        /// <summary>
        /// Get the parent object's RectTransform
        /// </summary>
        /// <returns>The RectTransform of this object's parent</returns>
        public static RectTransform RectParent(this RectTransform trans)
        {
            return trans.parent as RectTransform;
        }

        /// <summary>
        /// Sets the anchors for the RectTransform
        /// </summary>
        /// <param name="AnchorPositionMin">The minimum anchor values represented as a Vector2</param>
        /// <param name="AnchorPositionsMax">The maximum anchor values represented as a Vector2</param>
        public static void SetAnchors(this RectTransform trans, Vector2 AnchorPositionMin, Vector2 AnchorPositionsMax)
        {
            trans.anchorMin = AnchorPositionMin;
            trans.anchorMax = AnchorPositionsMax;
        }
    }
}