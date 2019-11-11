namespace Copper.ViewManager.Interfaces
{
    using ScriptableObjects;

    public interface IViewLoader
    {
        IView CreateView(ViewInfo viewInfo);
    }
}