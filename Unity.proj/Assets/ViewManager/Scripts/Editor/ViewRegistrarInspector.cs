namespace Copper.ViewManager.Editor
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text.RegularExpressions;
    using Code.Interfaces;
    using Code.ScriptableObjects;
    using CodeGeneration;
    using ScriptableObjects;
    using UnityEditor;
    using UnityEngine;

    [CustomEditor(typeof(ViewRegistrar))]
    public class ViewRegistrarInspector : Editor
    {
        private const string RESOURCE_FOLDER_NAME = "Resources/";

        private SerializedObject _target;
        private SerializedProperty layers;
        private SerializedProperty viewRecords;

        private bool showLayers = false;
        private bool showViews = false;
        private List<bool> expandedViewInfo;
        private List<string> prevLayers;

        private List<string> fullyQualifiedLoadTypes;
        private List<string> shortenedLoadTypes;

        private string[] shaderChannelOptions = new string[]
        {
            "TexCoord1",
            "TexCoord2",
            "TexCoord3",
            "Normal",
            "Tangent"
        };

        private void OnEnable()
        {
            if (expandedViewInfo == null)
            {
                expandedViewInfo = new List<bool>();
            }

            if (fullyQualifiedLoadTypes == null)
            {
                Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
                Type iViewLoaderType = typeof(IViewLoader);
                fullyQualifiedLoadTypes = new List<string>();
                shortenedLoadTypes = new List<string>();
                foreach (Assembly assembly in assemblies)
                {
                    IEnumerable<Type> viewBuilderTypes = assembly.GetTypes().Where(type => type.IsClass &&
                                                                                           !type.IsAbstract &&
                                                                                           iViewLoaderType.IsAssignableFrom(type));

                    foreach (Type builderType in viewBuilderTypes)
                    {
                        fullyQualifiedLoadTypes.Add(builderType.AssemblyQualifiedName);
                        string shortenedLoadType = Regex.Match(builderType.AssemblyQualifiedName, "(?!\\.)\\w+(?=,)").Value;
                        shortenedLoadTypes.Add(shortenedLoadType);
                    }
                }
            }
        }

        public override void OnInspectorGUI()
        {
            SerializedObject _target = new SerializedObject(target);
            SerializedProperty _layers = _target.FindProperty("layers");
            SerializedProperty _viewRecords = _target.FindProperty("viewRecords");

            EditorGUILayout.HelpBox("All Views and Layers that you wish to use in your game may be registered here in the View Registrar.\n\n  Layers\nView layers will appear in the order that you specify here.\n\n  Views\nWhen setting up a view you must give it a name, specify its layer, and link the view.", MessageType.None);

            if (GUILayout.Button("Regenerate View Statics")) ViewConstantsGeneratorInterface.GenerateConstants(target as ViewRegistrar);

            #region Layers

            EditorGUILayout.BeginHorizontal();
            {
                showLayers = EditorGUILayout.Foldout(showLayers, "Layers");
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Add Layer"))
                {
                    _layers.InsertArrayElementAtIndex(_layers.arraySize);
                    SerializedProperty layerName = _layers.GetArrayElementAtIndex(_layers.arraySize - 1).FindPropertyRelative("layerName");
                    layerName.stringValue = string.Empty;
                    showLayers = true;
                }
            }
            EditorGUILayout.EndHorizontal();

            if (showLayers)
            {
                bool layerUsesLightingChannel = false;

                for (int i = 0; i < _layers.arraySize; i++)
                {
                    SerializedProperty layer = _layers.GetArrayElementAtIndex(i);
                    SerializedProperty layerName = layer.FindPropertyRelative("layerName");
                    SerializedProperty isOverlay = layer.FindPropertyRelative("isOverlay");
                    SerializedProperty layerShaderChannels = layer.FindPropertyRelative("additionalShaderChannels");
                    EditorGUILayout.BeginHorizontal(GUI.skin.box);
                    {
                        EditorGUILayout.BeginVertical();
                        {
                            GUILayout.Space(14);
                            EditorGUILayout.LabelField(string.Format("Layer {0}", i + 1), GUILayout.MaxWidth(80), GUILayout.MinWidth(60));
                        }
                        EditorGUILayout.EndVertical();
                        EditorGUILayout.BeginVertical(GUILayout.MaxWidth(20));
                        {
                            GUILayout.Space(10);
                            if (GUILayout.Button("▴", GUILayout.Width(20), GUILayout.Height(12))) _layers.MoveArrayElement(i, i - 1);
                            GUILayout.Space(-2);
                            if (GUILayout.Button("▾", GUILayout.Width(20), GUILayout.Height(12))) _layers.MoveArrayElement(i, i + 1);
                        }
                        EditorGUILayout.EndVertical();

                        EditorGUILayout.BeginVertical();
                        {
                            GUILayout.Space(7);
                            EditorGUILayout.BeginHorizontal();
                            {
                                layerName.stringValue = EditorGUILayout.TextField(layerName.stringValue);
                                EditorGUILayout.LabelField("Overlay", GUILayout.Width(65));
                                GUILayout.Space(-15);
                                isOverlay.boolValue = EditorGUILayout.Toggle(isOverlay.boolValue);
                            }
                            EditorGUILayout.EndHorizontal();

                            layerShaderChannels.intValue = EditorGUILayout.MaskField("Additional Shader Channels", layerShaderChannels.intValue, shaderChannelOptions, new GUILayoutOption[0]);

                            if (((layerShaderChannels.intValue & 8) | (layerShaderChannels.intValue & 16)) != 0)
                            {
                                layerUsesLightingChannel = true;
                            }
                        }
                        EditorGUILayout.EndVertical();

                        EditorGUILayout.BeginVertical();
                        {
                            GUILayout.Space(3);
                            if (GUILayout.Button("-", GUILayout.Width(24), GUILayout.Height(36))) _layers.DeleteArrayElementAtIndex(i);
                        }
                        EditorGUILayout.EndVertical();
                    }
                    EditorGUILayout.EndHorizontal();
                }

                if (layerUsesLightingChannel)
                {
                    EditorGUILayout.HelpBox("Shader channels Normal and Tangent are most often used with lighting, which an Overlay canvas does not support. Its likely these channels are not needed.", MessageType.Warning);
                }
            }
            #endregion

            #region View Records

            EditorGUILayout.BeginHorizontal();
            {
                showViews = EditorGUILayout.Foldout(showViews, "Views");
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Add View"))
                {
                    _viewRecords.InsertArrayElementAtIndex(_viewRecords.arraySize);
                    SerializedProperty view = _viewRecords.GetArrayElementAtIndex(_viewRecords.arraySize - 1);
                    SerializedProperty viewName = view.FindPropertyRelative("viewID");
                    SerializedProperty viewPrefab = view.FindPropertyRelative("view");
                    SerializedProperty isDialog = view.FindPropertyRelative("isDialog");
                    SerializedProperty persistent = view.FindPropertyRelative("persistent");
                    SerializedProperty loadType = view.FindPropertyRelative("loadType");
                    viewName.stringValue = string.Empty;
                    viewPrefab.stringValue = string.Empty;
                    isDialog.boolValue = false;
                    persistent.boolValue = false;
                    loadType.stringValue = fullyQualifiedLoadTypes[0];

                    showViews = true;
                }
            }
            EditorGUILayout.EndHorizontal();

            if (showViews)
            {
                EditorGUI.indentLevel++;

                //Copy the layer names for use in the layer field of views
                List<string> layerNames = new List<string>();
                for (int i = 0; i < _layers.arraySize; i++)
                {
                    layerNames.Add(_layers.GetArrayElementAtIndex(i).FindPropertyRelative("layerName").stringValue);
                }

                // Update the record of which sets of view records are expanded
                if (expandedViewInfo.Count <= _viewRecords.arraySize)
                {
                    for (int i = expandedViewInfo.Count; i < _viewRecords.arraySize; i++)
                    {
                        expandedViewInfo.Add(false);
                    }
                }

                for (int i = 0; i < _viewRecords.arraySize; i++)
                {
                    SerializedProperty view = _viewRecords.GetArrayElementAtIndex(i);
                    SerializedProperty viewName = view.FindPropertyRelative("viewID");
                    SerializedProperty layerID = view.FindPropertyRelative("layerID");
                    SerializedProperty viewPrefabPath = view.FindPropertyRelative("view");
                    SerializedProperty isDialog = view.FindPropertyRelative("isDialog");
                    SerializedProperty persistent = view.FindPropertyRelative("persistent");
                    SerializedProperty loadType = view.FindPropertyRelative("loadType");

                    // Set View ID if View is linked but ID is empty
                    if (string.IsNullOrEmpty(viewName.stringValue) && string.IsNullOrEmpty(viewPrefabPath.stringValue) == false)
                    {
                        viewName.stringValue = Path.GetFileNameWithoutExtension(viewPrefabPath.stringValue);

                    }

                    // Correct the layer value if needed
                    if (prevLayers != null)
                    {
                        if (layerID.intValue >= 0 && layerID.intValue < prevLayers.Count)
                        {
                            string layer = prevLayers[layerID.intValue];
                            int newIndex = layerNames.FindIndex(name => name == layer);

                            if (layerID.intValue != newIndex)
                            {
                                layerID.intValue = newIndex;
                            }
                        }
                    }

                    expandedViewInfo[i] = EditorGUILayout.Foldout(expandedViewInfo[i], viewName.stringValue);
                    if (expandedViewInfo[i])
                    {
                        EditorGUI.indentLevel++;

                        EditorGUILayout.BeginVertical(GUI.skin.box);
                        {
                            EditorGUILayout.PropertyField(viewName);
                            EditorGUILayout.PropertyField(viewPrefabPath);
                            EditorGUILayout.PropertyField(persistent);

                            int loadTypeIndex = Mathf.Clamp(fullyQualifiedLoadTypes.IndexOf(loadType.stringValue), 0, int.MaxValue);
                            loadTypeIndex = EditorGUILayout.Popup("Load Type", loadTypeIndex, shortenedLoadTypes.ToArray());
                            loadType.stringValue = fullyQualifiedLoadTypes[loadTypeIndex];

                            EditorGUILayout.PropertyField(isDialog);

                            if (!isDialog.boolValue)
                            {
                                EditorGUILayout.BeginHorizontal();
                                {
                                    layerID.intValue = EditorGUILayout.Popup("Layer", layerID.intValue, layerNames.ToArray());
                                }
                                EditorGUILayout.EndHorizontal();
                            }

                            if (GUILayout.Button("Remove"))
                            {
                                _viewRecords.DeleteArrayElementAtIndex(i);
                                expandedViewInfo.RemoveAt(i);
                            }
                        }
                        EditorGUILayout.EndVertical();

                        EditorGUI.indentLevel--;
                    }
                }

                prevLayers = new List<string>(layerNames);

                EditorGUI.indentLevel--;
            }
            #endregion

            _target.ApplyModifiedProperties();
        }
    }
}