namespace Copper.ViewManager
{
    using Interfaces;
    using ScriptableObjects;
    using UnityEngine;

    public class BaseView : MonoBehaviour, IView
    {
        private ViewInfo viewInfo;

        int IView.ViewID
        {
            get => viewInfo.ViewID;
        }

        bool IView.IsDialog
        {
            get => viewInfo.IsDialog;
        }

        int IView.LayerID
        {
            get => viewInfo.LayerID;
        }

        public virtual void UpdateView(object data) { }

        void IView.SetParentTransform(RectTransform parent)
        {
            transform.SetParent(parent, false);
        }

        public virtual void Initialize(ViewInfo viewInfo)
        {
            this.viewInfo = viewInfo;
        }

        public virtual void TransitionIn() { }

        public virtual void TransitionOut(ViewTransitionComplete onOutComplete)
        {
            onOutComplete?.Invoke();
        }

        public virtual void DestroyView() { }

        protected void Close()
        {
            ViewManager.Instance.RemoveView(viewInfo.ViewID);
        }
    }
}