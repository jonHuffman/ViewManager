namespace Copper.ViewManager.Code
{
    using Interfaces;

    public struct ViewData<T> : IViewData<T>
    {
        public T TypedData { get; }
        
        public ViewData(T viewData)
        {
            TypedData = viewData;
        }

        public static implicit operator ViewData<T>(T viewData)
        {
            return new ViewData<T>(viewData);
        }

        public static implicit operator T(ViewData<T> viewData)
        {
            return viewData.TypedData;
        }
    }
}