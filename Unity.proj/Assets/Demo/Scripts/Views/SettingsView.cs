#pragma warning disable 649
using Copper.ViewManager;
using Copper.ViewManager.Constants;
using UnityEngine;
using UnityEngine.UI;

public class SettingsView : BaseView
{
    [SerializeField]
    private Button exitButton;

    private void Awake()
    {
        exitButton.onClick.AddListener(OpenConfirmationDialog);
    }

    private void OpenConfirmationDialog()
    {
        ViewManager.Instance.AddView(View.ConfirmationDialog, new ConfirmationDialog.ConfirmationDialogData(OpenMenu));
    }

    /// <summary>
    ///     Returns the player to the main menu upon confirmation
    /// </summary>
    private void OpenMenu()
    {
        ViewManager.Instance.AddView(View.MainMenu);
        Close();
    }
}