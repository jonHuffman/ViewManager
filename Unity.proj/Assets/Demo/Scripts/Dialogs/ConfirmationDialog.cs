﻿using DG.Tweening;
using System;
using Copper.ViewManager;
using Copper.ViewManager.Interfaces;
#pragma warning disable 649
using UnityEngine;

/// <summary>
/// Its called a dialog but in the end everything is a View
/// </summary>
public class ConfirmationDialog : BaseView
{
    [SerializeField]
    private RectTransform dialogBody;
    [SerializeField]
    private Vector3 inPos;
    [SerializeField]
    private float transitionLength = 1f;

    private Vector3 outPos;
    private ConfirmationDialogData data;

    private void Awake()
    {
        Debug.Assert(dialogBody != null, "dialogBody is Null!");
    }
    
    public override void TransitionIn()
    {
        outPos = dialogBody.transform.localPosition;

        //Use a basic tween to transition the dialog
        dialogBody.DOLocalMove(inPos, transitionLength);
    }

    public override void TransitionOut(ViewTransitionComplete onOutComplete)
    {
        //Use a basic tween to transition the dialog
        dialogBody.DOLocalMove(outPos, transitionLength).OnComplete(() => { onOutComplete(); });
    }

    public override void UpdateView(object data)
    {
        //Store the data object so that we can call the callbacks later
        this.data = (ConfirmationDialogData)data;
    }

    /// <summary>
    /// Handles the user confirming their action
    /// </summary>
    public void UI_OnConfirm()
    {
        data.onConfirmCallback?.Invoke();

        Close();
    }

    /// <summary>
    /// Handles the user cancelling their action
    /// </summary>
    public void UI_OnCancel()
    {
        data.onCancelCallback?.Invoke();

        Close();
    }

    /// <summary>
    /// Stores the callbacks for the confirmation dialog
    /// </summary>
    public class ConfirmationDialogData
    {
        public Action onConfirmCallback;
        public Action onCancelCallback;

        public ConfirmationDialogData(Action confirmationCallback, Action cancelCallback = null)
        {
            onConfirmCallback = confirmationCallback;
            onCancelCallback = cancelCallback;
        }
    }

    private void OnDrawGizmos()
    {
        //If the dialog body has been linked we draw some debug gizmos to show the path of the tween for easy setup
        if (dialogBody != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere((transform as RectTransform).TransformPoint(inPos), 10f);

            Gizmos.DrawLine((transform as RectTransform).TransformPoint(dialogBody.transform.localPosition), (transform as RectTransform).TransformPoint(inPos));
        }
    }
}