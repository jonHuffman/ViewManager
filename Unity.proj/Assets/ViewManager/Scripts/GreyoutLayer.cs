namespace Copper.ViewManager
{
    using System.Collections.Generic;
    using System.Linq;
    using Interfaces;
    using UnityEngine;
    using UnityEngine.UI;
    using Utils;

    /// <summary>
    /// A darkened layer placed behind dialogs and views on overlay layers to reduce distraction
    /// and bring focus to the UI elements above. It will also block input to the elements below.
    /// </summary>
    public class GreyoutLayer
    {
        private const float FADE_RATE = 2.4f;
        private const float DEFAULT_ALPHA = 0.5f;

        private Image greyoutImage;
        private Color greyoutColor;
        private System.Collections.IEnumerator greyoutTransition;
        private LayerCollection layerCollection;

        public GreyoutLayer(Canvas viewCanvas, LayerCollection layerCollection)
        {
            this.layerCollection = layerCollection;

            CreateGreyoutObject(viewCanvas);

            SetAlpha(DEFAULT_ALPHA);
            SetGreyoutState(false, true);
        }

        public void DisableGreyout()
        {
            SetGreyoutState(false, true);
        }

        public void SetAlpha(float alpha)
        {
            greyoutColor = Color.black;
            greyoutColor.a = alpha;
            greyoutImage.color = greyoutColor;
        }

        /// <summary>
        /// Updates the position in the ViewCanvas that the greyout will appear at. This is used to ensure that the greyout is always below the topmost overlay or dialog.
        /// </summary>
        /// <param name="updatedView">The viewinfo of the view that was updated. We only update the greyout position if an overlay layerID was updated</param>
        public void UpdateGreyoutPosition(BaseView updatedView, HashSet<int> viewsToRemoveList)
        {
            if (((IView)updatedView).IsDialog)
            {
                if (TryRepositionGreyoutInDialogHierarchy(updatedView, viewsToRemoveList))
                {
                    return;
                }
            }

            //Update the greyout position relative to overlay layers since there are no active dialogs
            if (layerCollection.ContainsOverlayLayers)
            {
                if (TryRepositionGreyoutInViewHierarchy(viewsToRemoveList))
                {
                    return;
                }
            }

            SetGreyoutState(false);
        }

        private void CreateGreyoutObject(Canvas viewCanvas)
        {
            GameObject go = new GameObject("GreyoutLayer");
            go.layer = LayerMask.NameToLayer("UI");
            go.transform.SetParent(viewCanvas.transform, false);

            go.transform.localPosition = Vector3.forward * 0.1f;

            greyoutImage = go.AddComponent<Image>();
            greyoutImage.rectTransform.SetSize(new Vector2(viewCanvas.pixelRect.width, viewCanvas.pixelRect.height));
            greyoutImage.rectTransform.SetAnchors(new Vector2(0, 0), new Vector2(1, 1));
        }

        private void SetGreyoutState(bool activeState, bool skipAnimation = false)
        {
            if (greyoutTransition != null)
            {
                greyoutImage.StopCoroutine(greyoutTransition);
            }

            if (skipAnimation == true)
            {
                Color transparentColor = greyoutColor;
                transparentColor.a = 0;
                greyoutImage.color = activeState ? greyoutColor : transparentColor;
                UpdateGreyoutActiveState();
                return;
            }

            if (activeState == true)
            {
                greyoutImage.gameObject.SetActive(activeState);
                greyoutTransition = TransitionGreyout(greyoutColor.a);
            }
            else
            {
                greyoutTransition = TransitionGreyout(0);
            }

            if (greyoutImage.isActiveAndEnabled)
            {
                greyoutImage.StartCoroutine(greyoutTransition);
            }
        }

        private bool TryRepositionGreyoutInDialogHierarchy(BaseView updatedView, HashSet<int> viewsToRemoveList)
        {
            Transform greyoutTransform = greyoutImage.transform;
            Transform dialogContainer = updatedView.transform.parent;
            bool viewIsBeingRemoved = viewsToRemoveList.Contains(((IView)updatedView).ViewID);

            for (int i = dialogContainer.childCount - 1; i >= 0; i--)
            {
                Transform dialogTransform = dialogContainer.GetChild(i);

                // We do not update the greyout position for the dialog being removed
                // If its the only dialog, then the greyout will be disabled at bottom of the function
                if (viewIsBeingRemoved && updatedView.transform == dialogTransform)
                {
                    continue;
                }

                IView dialogView = dialogTransform.gameObject.GetComponent<IView>();
                if (!dialogTransform.Equals(greyoutTransform) && dialogTransform.gameObject.activeSelf && !viewsToRemoveList.Contains(dialogView.ViewID))
                {
                    PlaceGreyoutBelowTransform(dialogTransform);
                    SetGreyoutState(true);
                    return true;
                }
            }

            return false;
        }

        private bool TryRepositionGreyoutInViewHierarchy(HashSet<int> viewsToRemoveList)
        {
            Layer topOccupiedOverlay = layerCollection.GetTopOccupiedOverlayLayer(viewsToRemoveList.ToArray());
            if (topOccupiedOverlay != null)
            {
                IView view = topOccupiedOverlay.activeView;

                PlaceGreyoutBelowTransform(((BaseView)view).transform);
                SetGreyoutState(true);
                return true;
            }

            return false;
        }

        private void PlaceGreyoutBelowTransform(Transform activeOverlay)
        {
            Transform greyoutTransform = greyoutImage.transform;
            greyoutTransform.SetParent(activeOverlay.parent);
            int siblingIndex = Mathf.Clamp(activeOverlay.GetSiblingIndex() - 1, 0, int.MaxValue);
            greyoutTransform.SetSiblingIndex(siblingIndex);
        }

        /// <summary>
        /// Enables or Disables the Greyout object depending on if its alpha value is greater than a defined epsilon value
        /// </summary>
        private void UpdateGreyoutActiveState()
        {
            const float DISABLE_EPSILON = 0.05f;
            greyoutImage.gameObject.SetActive(greyoutImage.color.a > DISABLE_EPSILON);
        }

        private System.Collections.IEnumerator TransitionGreyout(float targetAlpha)
        {
            bool isFadingIn = greyoutImage.color.a < targetAlpha;

            if (isFadingIn)
            {
                yield return FadeGreyoutIn(targetAlpha);
            }
            else
            {
                yield return FadeGreyoutOut(targetAlpha);
            }

            UpdateGreyoutActiveState();

            greyoutTransition = null;
        }

        private System.Collections.IEnumerator FadeGreyoutIn(float targetAlpha)
        {
            Color greyoutImageColor = greyoutImage.color;

            if (!greyoutImage.gameObject.activeSelf)
            {
                greyoutImage.gameObject.SetActive(true);
            }

            greyoutImage.raycastTarget = true;

            while (greyoutImage.color.a < targetAlpha)
            {
                float newAlpha = greyoutImageColor.a + FADE_RATE * Time.deltaTime;
                newAlpha = Mathf.Max(newAlpha, targetAlpha);
                greyoutImageColor.a = newAlpha;
                greyoutImage.color = greyoutImageColor;

                yield return null;
            }

            greyoutImageColor.a = targetAlpha;
            greyoutImage.color = greyoutImageColor;
        }

        private System.Collections.IEnumerator FadeGreyoutOut(float targetAlpha)
        {
            const float CAN_CLICK_THROUGH_DELTA = 0.30f;

            Color greyoutImageColor = greyoutImage.color;

            bool canClickThrough = false;
            while (greyoutImage.color.a > targetAlpha)
            {
                float newAlpha = greyoutImageColor.a - FADE_RATE * Time.deltaTime;
                newAlpha = Mathf.Min(newAlpha, targetAlpha);
                greyoutImageColor.a = newAlpha;
                greyoutImage.color = greyoutImageColor;

                if (!canClickThrough && greyoutImage.color.a <= CAN_CLICK_THROUGH_DELTA)
                {
                    greyoutImage.raycastTarget = false;
                    canClickThrough = true;
                }
                yield return null;
            }

            greyoutImageColor.a = targetAlpha;
            greyoutImage.color = greyoutImageColor;
        }
    }
}