namespace Copper.ViewManager
{
    using Interfaces;

    internal class NullData : IViewData<NullData>
    {
        public NullData TypedData
        {
            get => this;
        }
    }
}