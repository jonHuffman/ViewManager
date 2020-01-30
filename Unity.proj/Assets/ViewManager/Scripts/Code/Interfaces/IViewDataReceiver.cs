namespace Copper.ViewManager.Code.Interfaces
{
    public interface IViewDataReceiver<T>
    {
        /// <summary>
        /// A data object may be provided to your view, should you wish to refer to it multiple times
        /// during the view life-cycle the IViewData object should be cached.
        /// <para><b>Note:</b> This method will be called before <see cref="IView.Initialize"/></para>
        /// </summary>
        /// <param name="viewData">The data to make available to this view</param>
        void SetViewData(ViewData<T> viewData);
    }
}