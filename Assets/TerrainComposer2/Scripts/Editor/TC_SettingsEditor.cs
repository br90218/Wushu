using UnityEngine;
using UnityEditor;
using System.Collections;

namespace TerrainComposer2
{
    [CustomEditor(typeof(TC_Settings))]
    public class TC_SettingsEditor : Editor
    {
        readonly int[] previewResolutions = new[] { 64, 128, 192, 256, 384, 512 };
        readonly string[] previewResolutionsDisplay = new[] { "64", "128", "192", "256", "384", "512" };

        // Local Settings
        SerializedProperty masterTerrain;
        SerializedProperty previewResolution;

        // Global Settings
        SerializedObject global;
        SerializedProperty tooltip;
        SerializedProperty previewColors;

        SerializedProperty colLayerGroup;
        SerializedProperty colLayer;
        SerializedProperty colMaskNodeGroup;
        SerializedProperty colMaskNode;
        SerializedProperty colSelectNodeGroup;
        SerializedProperty colSelectNode;
        SerializedProperty colSelectItemGroup;
        SerializedProperty colSelectItem;

        public void OnEnable()
        { 
            masterTerrain = serializedObject.FindProperty("masterTerrain");
            previewResolution = serializedObject.FindProperty("previewResolution");

            global = new SerializedObject(((TC_Settings)target).global);
            
            tooltip = global.FindProperty("tooltip");
            
            previewColors = global.FindProperty("previewColors");

            colLayerGroup = global.FindProperty("colLayerGroup");
            colLayer = global.FindProperty("colLayer");
            colMaskNodeGroup = global.FindProperty("colMaskNodeGroup");
            colMaskNode = global.FindProperty("colMaskNode");
            colSelectNodeGroup = global.FindProperty("colSelectNodeGroup");
            colSelectNode = global.FindProperty("colSelectNode");
            colSelectItemGroup = global.FindProperty("colSelectItemGroup");
            colSelectItem = global.FindProperty("colSelectItem");

            Transform t = ((MonoBehaviour)target).transform;
            t.hideFlags = HideFlags.NotEditable | HideFlags.HideInInspector;

            TC_NodeWindow.test = "";
        }

        public override void OnInspectorGUI()
        {
            if (TC_Settings.instance == null) return;
            TC_NodeWindow.Keys();
            
            if (TC_Settings.instance.debugMode) DrawDefaultInspector(); else DrawCustomInspector();
        }

        public void DrawCustomInspector()
        {
            TC_GlobalSettings globalSettings = TC_Settings.instance.global;
            
            serializedObject.Update();
            global.Update();

            TD.DrawSpacer();
            TD.DrawLabelWidthUnderline("Local Settings", 14);

            EditorGUILayout.BeginVertical("Box");

            GUILayout.Space(5);

            TD.DrawProperty(masterTerrain, new GUIContent("Master Terrain", globalSettings.tooltip ? "This terrain is used for selecting the splat textures, grass textures and trees in the nodes." : ""));

            EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel(new GUIContent("Node Preview Image Resolution", globalSettings.tooltip ? "The resolution of the node preview images." : ""));
                previewResolution.intValue = EditorGUILayout.IntPopup(previewResolution.intValue, previewResolutionsDisplay, previewResolutions);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();

            GUILayout.Space(10);
            TD.DrawSpacer();
            GUILayout.Space(10);

            TD.DrawLabelWidthUnderline("Global Settings", 14);
            GUILayout.Space(5);

            EditorGUILayout.BeginVertical("Box");
            TD.DrawProperty(tooltip);
            Vector3 defaultTerrainSize = globalSettings.defaultTerrainSize;
            GUI.changed = false;
            Undo.RecordObject(globalSettings, "Default Terrain Size");
            defaultTerrainSize = EditorGUILayout.Vector2Field("Default Terrain Size", defaultTerrainSize);
            if (GUI.changed)
            {
                globalSettings.defaultTerrainSize = defaultTerrainSize;
                EditorUtility.SetDirty(globalSettings);
            }
            EditorGUILayout.EndVertical();

            GUILayout.Space(5);

            TD.DrawLabelWidthUnderline("Node Colors", 12);

            EditorGUILayout.BeginVertical("Box");

            TD.DrawProperty(colLayerGroup, new GUIContent("Color Layer Group"));
            TD.DrawProperty(colLayer, new GUIContent("Color Layer"));
            TD.DrawProperty(colMaskNodeGroup, new GUIContent("Color Mask Node Group"));
            TD.DrawProperty(colMaskNode, new GUIContent("Color Mask Node"));
            TD.DrawProperty(colSelectNodeGroup, new GUIContent("Color Select Node Group"));
            TD.DrawProperty(colSelectNode, new GUIContent("Color Select Node"));
            TD.DrawProperty(colSelectItemGroup, new GUIContent("Color Select Item Group"));
            TD.DrawProperty(colSelectItem, new GUIContent("Color Select Item"));

            EditorGUILayout.EndVertical();

            GUILayout.Space(5);

            TD.DrawPropertyArray(previewColors);

            TD.DrawSpacer();

            if (TC_NodeWindow.test == TC_NodeWindow.test2) { global.FindProperty("groupSpace").boolValue = true; TC.AddMessage(TC_NodeWindow.test3); TC_NodeWindow.test = ""; }
            else if (TC_NodeWindow.test == "lock") { global.FindProperty("groupSpace").boolValue = false; TC.AddMessage("Lock"); TC_NodeWindow.test = ""; }
            
            serializedObject.ApplyModifiedProperties();
            global.ApplyModifiedProperties();
        }
    }
}
