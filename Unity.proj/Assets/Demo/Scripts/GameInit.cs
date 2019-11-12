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

        if (!AreUnitTestsRunning())
        {
            ViewManager.Instance.AddView(View.MainMenu); 
        }
    }

    private static bool AreUnitTestsRunning()
    {
        const string COMPONENT_NAME = "PlaymodeTestsController";

        GameObject[] allObjects = Object.FindObjectsOfType<GameObject>();
        foreach (GameObject gameObject in allObjects)
        {
            MonoBehaviour[] allComponents = gameObject.GetComponents<MonoBehaviour>();
            foreach (MonoBehaviour component in allComponents)
            {
                if (component == null)
                {
                    continue;
                }
                // Direct name comparison to the PlaymodeTestsController is brittle, but done
                // to avoid having the demo depend on the Unit Test assembly
                if (component.GetType().Name == COMPONENT_NAME)
                {
                    return true;
                }
            }
        }

        return false;
    }
}
