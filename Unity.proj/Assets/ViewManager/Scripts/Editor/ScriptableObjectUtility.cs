namespace Copper.ViewManager.Editor
{
    using System.IO;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// A generic utility that facilitates the easy creation of scriptable objects.
    /// </summary>
    /// <remarks>
    /// This open source solution was taken from the Unity3D wiki: http://wiki.unity3d.com/index.php?title=CreateScriptableObjectAsset
    /// </remarks>
    public static class ScriptableObjectUtility
    {
        /// <summary>
        ///	This is a method to easily create a new asset file instance of a ScriptableObject-derived class. 
        /// The asset is uniquely named and placed in the currently selected project path
        /// </summary>
        public static void CreateAsset<T>() where T : ScriptableObject
        {
            T asset = ScriptableObject.CreateInstance<T>();

            string path = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (path == "")
            {
                path = "Assets";
            }
            else if (Path.GetExtension(path) != "")
            {
                path = path.Replace(Path.GetFileName(AssetDatabase.GetAssetPath(Selection.activeObject)), "");
            }

            string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(path + "/New " + typeof(T).Name + ".asset");

            AssetDatabase.CreateAsset(asset, assetPathAndName);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = asset;
        }
    }
}