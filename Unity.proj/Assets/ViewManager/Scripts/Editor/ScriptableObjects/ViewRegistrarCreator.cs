namespace Copper.ViewManager.Editor.ScriptableObjects
{
    using Code.ScriptableObjects;
    using Editor;
    using UnityEditor;
    using UnityEngine;

    public class ViewRegistrarCreator : MonoBehaviour
    {
        [MenuItem("Tools/View Manager/Create Registrar")]
        public static void CreateViewRegistrar()
        {
            ScriptableObjectUtility.CreateAsset<ViewRegistrar>();
        }
    }
}