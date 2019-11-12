namespace Copper.ViewManager.Code.Interfaces
{
    public interface IViewData<out T>
    {
        T TypedData { get; }
    }
}