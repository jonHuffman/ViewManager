namespace Copper.ViewManager.Editor.CodeGeneration
{
    using Code.ScriptableObjects;
    using System.Collections.Generic;
    using System.IO;
    using UnityEditor;
    using UnityEngine;

    public class ViewConstantsGeneratorInterface : ViewConstantsGenerator
    {
        public static void GenerateConstants(ViewRegistrar registrar, string outputPath)
        {
            //string outputPath = EditorUtility.SaveFilePanelInProject("Save Location", "ViewStatics", "cs", "Where do you want to save this constants file?", string.Format("{0}/Core/Modules/ViewManager/Scripts/Constants", Application.dataPath));

            outputPath = Path.Combine(new[]
            {
                outputPath, "ViewStatics.cs"
            });

            ViewConstantsGenerator generator = new ViewConstantsGenerator
            {
                Session = new Dictionary<string, object>
                {
                    ["className"] = "View"
                }
            };

            SetViewIDs(generator, registrar);

            generator.Initialize();

            string classDef = generator.TransformText();
            File.WriteAllText(outputPath, classDef);

            AssetDatabase.Refresh();
        }

        private static void SetViewIDs(ViewConstantsGenerator generator, ViewRegistrar registrar)
        {
            List<string> viewIDs = new List<string>();

            foreach (ViewInfo viewRecord in registrar.ViewRecords)
            {
                // If any of the ViewRecords in the registrar do not have a name defined we will not create an entry for them.
                // Additionally if there are duplicates they will be ignored.
                if (viewRecord.HasName && !viewIDs.Contains(viewRecord.Name))
                {
                    viewIDs.Add(viewRecord.Name);
                }
            }

            generator.Session["viewIDs"] = viewIDs.ToArray();
        }
    }
}