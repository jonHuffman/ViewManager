namespace Copper.ViewManager.Editor.CodeGeneration
{
    using System.Collections.Generic;
    using System.IO;
    using ScriptableObjects;
    using UnityEditor;
    using UnityEngine;

    public class ViewConstantsGeneratorInterface : ViewConstantsGenerator
    {
        public static void GenerateConstants(ViewRegistrar registrar)
        {
            string outputPath = EditorUtility.SaveFilePanelInProject("Save Location", "ViewStatics", "cs", "Where do you want to save this constants file?", string.Format("{0}/Core/Modules/ViewManager/Scripts/Constants", Application.dataPath));

            ViewConstantsGenerator generator = new ViewConstantsGenerator();
            generator.Session = new Dictionary<string, object>();

            generator.Session["className"] = "View";
            SetViewIDs(generator, registrar);

            generator.Initialize();

            string classDef = generator.TransformText();
            File.WriteAllText(outputPath, classDef);

            AssetDatabase.Refresh();
        }

        private static void SetViewIDs(ViewConstantsGenerator generator, ViewRegistrar registrar)
        {
            List<string> viewIDs = new List<string>();

            for (int i = 0; i < registrar.ViewRecords.Count; i++)
            {
                //If any of the ViewRecords in the registrar do not have a name defined we will not create an entry for them. Additionally if there are duplicates they will be ignored.
                if (registrar.ViewRecords[i].HasName && !viewIDs.Contains(registrar.ViewRecords[i].Name))
                {
                    viewIDs.Add(registrar.ViewRecords[i].Name);
                }
            }

            generator.Session["viewIDs"] = viewIDs.ToArray();
        }
    }
}