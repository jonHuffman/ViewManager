namespace Copper.ViewManager.Code.Interfaces
{
    using ScriptableObjects;
    using UnityEngine;

    public delegate void ViewTransitionComplete();

    public interface IView
    {
        /// <summary>
        /// Used to retrieve the View's assigned ID
        /// </summary>
        int ViewID { get; }

        bool IsDialog { get; }

        int LayerID { get; }

        void SetViewInfo(ViewInfo viewInfo);

        void SetParentTransform(RectTransform parent);

        /// <summary>
        /// An initialization point for your view. This will be called before TransitionIn.
        /// </summary>
        void Initialize();

        /// <summary>
        /// Plays any in transition that you may want to set up. onInComplete must be called regardless of whether there is a transition or not.
        /// </summary>
        /// <param name="onInComplete">The function that notifies that the View is does transitioning.</param>
        void TransitionIn();

        /// <summary>
        /// Plays any out transition that you may want to set up. onOutComplete must be called regardless of whether there is a transition or not.
        /// </summary>
        /// <param name="onOutComplete">The function that notifies that the View is does transitioning.</param>
        void TransitionOut(ViewTransitionComplete onOutComplete);

        /// <summary>
        /// The place to clean up your view before it is destroyed.
        /// </summary>
        void DestroyView();
    }
}
