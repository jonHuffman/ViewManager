#pragma warning disable 649
using Copper.ViewManager;
using Copper.ViewManager.Constants;
using Copper.ViewManager.Interfaces;
using UnityEngine;
using UnityEngine.UI;

public class GameHUD : BaseView, IViewDataReceiver<GameHUD.GameHUDData>
{
    [SerializeField]
    private Animator anim;
    [SerializeField]
    private Text healthText;

    private GameHUDData gameHudData;

    private ViewTransitionComplete onInComplete;
    private ViewTransitionComplete onOutComplete;

    public void SetViewData(IViewData<GameHUDData> viewData)
    {
        gameHudData = viewData.TypedData;
    }

    public override void Initialize()
    {
        base.Initialize();
        healthText.text = "HP: " + gameHudData.health;
    }

    public override void TransitionIn()
    {
        anim.SetTrigger("TransitionIn");
    }

    public override void TransitionOut(ViewTransitionComplete onOutComplete)
    {
        //In this case the onOutComplete callback is stored until the animation completes and is then called ANIM_OnTransitionOutComplete
        this.onOutComplete = onOutComplete;
        anim.SetTrigger("TransitionOut");
    }

    /// <summary>
    /// Handles the transition out complete dispatched by the transition animation
    /// </summary>
    /// <remarks>
    /// Linked in inspector
    /// </remarks>
    public void ANIM_OnTransitionOutComplete()
    {
        //Complete the transition by calling the callback
        if (onOutComplete != null)
        {
            onOutComplete();
        }
    }

    /// <summary>
    /// Handles the on back pressed button
    /// </summary>
    /// <remarks>
    /// Linked in inspector
    /// </remarks>
    public void UI_OnSettingsPressed()
    {
        ViewManager.Instance.AddView(View.SettingsView);
    }

    /// <summary>
    /// A basic example of storing View data in a class to be passed through UpdateView
    /// </summary>
    public class GameHUDData : IViewData<GameHUDData>
    {
        public int health;

        public GameHUDData(int health)
        {
            this.health = health;
        }

        public GameHUDData TypedData
        {
            get => this;
        }
    }
}
