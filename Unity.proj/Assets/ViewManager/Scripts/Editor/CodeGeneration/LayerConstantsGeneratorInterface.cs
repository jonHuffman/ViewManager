namespace Copper.ViewManager.Editor.CodeGeneration
{
    using Code.ScriptableObjects;
    using System.Collections.Generic;
    using System.IO;
    using UnityEditor;
    using UnityEngine;

    public class LayerConstantsGeneratorInterface : LayerConstantsGenerator
    {
        public static void GenerateConstants(ViewRegistrar registrar, string outputPath)
        {
            outputPath = Path.Combine(new[]
            {
                outputPath, "LayerStatics.cs"
            });

            LayerConstantsGenerator generator = new LayerConstantsGenerator
            {
                Session = new Dictionary<string, object>
                {
                    ["className"] = "Layer"
                }
            };

            SetLayerIDs(generator, registrar);

            generator.Initialize();

            string classDef = generator.TransformText();
            File.WriteAllText(outputPath, classDef);

            AssetDatabase.Refresh();
        }

        private static void SetLayerIDs(LayerConstantsGenerator generator, ViewRegistrar registrar)
        {
            List<string> layerIDs = new List<string>();

            foreach (LayerRecord layerRecord in registrar.LayerRecords)
            {
                // If any of the layerRecords in the registrar do not have a name defined we will not create an entry for them.
                // Additionally if there are duplicates they will be ignored.
                if (layerRecord.HasName && !layerIDs.Contains(layerRecord.LayerName))
                {
                    layerIDs.Add(layerRecord.LayerName); 
                }
            }

            generator.Session["layerIDs"] = layerIDs.ToArray();
        }
    }
}