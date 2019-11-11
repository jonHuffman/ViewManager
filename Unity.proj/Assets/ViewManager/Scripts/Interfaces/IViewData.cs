namespace Copper.ViewManager.Interfaces
{
    using System;

    public interface IViewData<out T>
    {
        T TypedData { get; }
    }
}