﻿namespace Copper.ViewManager.Code
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

        void IView.SetViewInfo(ViewInfo viewInfo)
        {
            this.viewInfo = viewInfo;
        }

        void IView.SetParentTransform(RectTransform parent)
        {
            transform.SetParent(parent, false);
        }

        public virtual void Initialize() { }

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