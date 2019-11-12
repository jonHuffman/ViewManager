#pragma warning disable 649
using Copper.ViewManager;
using Copper.ViewManager.Code;
using Copper.ViewManager.Code.ScriptableObjects;
using UnityEngine;

public class GameInit : MonoBehaviour
{
    [SerializeField]
    private Canvas viewCanvas;
    [SerializeField]
    private ViewRegistrar registrar;

    private void Start()
    {
        if (viewCanvas == null)
        {
            Debug.LogError("A canvas must be linked in the GameInit object in order for the demo to work!");
        }

        ViewManager.Instantiate(viewCanvas, registrar);
        ViewManager.Instance.SetDebugMethods(Debug.Log, Debug.LogWarning, Debug.LogError);

        ViewManager.Instance.AddView(View.MainMenu);
    }
}
