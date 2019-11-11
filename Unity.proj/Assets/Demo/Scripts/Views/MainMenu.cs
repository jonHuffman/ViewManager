using Copper.ViewManager;
using Copper.ViewManager.Constants;
using UnityEngine;

public class MainMenu : BaseView
{
    /// <summary>
    /// On play handler for the Main Menu's play button
    /// </summary>
    /// <remarks>
    /// Linked in the inspector
    /// </remarks>
    public void UI_OnPlayPressed()
    {
        //Since the GameHUD is on the same layer as MainMenu, the MainMenu will automatically be transitioned out before the GameHUD is brought in
        ViewManager.Instance.AddView(View.GameHUD, new GameHUD.GameHUDData(UnityEngine.Random.Range(0, 100)));
    }

    /// <summary>
    /// On quit handler for the Main Menu's play button
    /// </summary>
    /// <remarks>
    /// Linked in the inspector
    /// </remarks>
    public void UI_OnQuitPressed()
    {
        ViewManager.Instance.AddView(View.ConfirmationDialog, new ConfirmationDialog.ConfirmationDialogData(ApplicationQuit));
    }

    /// <summary>
    /// Quits the application
    /// </summary>
    private void ApplicationQuit()
    {
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
