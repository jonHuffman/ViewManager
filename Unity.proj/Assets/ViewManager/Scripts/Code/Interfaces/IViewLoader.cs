namespace Copper.ViewManager.Code.Interfaces
{
    using ScriptableObjects;

    public interface IViewLoader
    {
        IView CreateView(ViewInfo viewInfo);
    }
}