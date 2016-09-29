using UnityEngine;
using UnityEditor;
using System.Collections;

namespace TerrainComposer2
{
    [CustomEditor(typeof(TC_RawImage))]
    public class TC_RawImageEditor : Editor
    {
        void OnEnable()
        {
            Transform t = ((MonoBehaviour)target).transform;
            t.hideFlags = HideFlags.NotEditable | HideFlags.HideInInspector;
        }

        public override void OnInspectorGUI()
        {
            if (TC_Settings.instance == null) return;
            if (TC_Settings.instance.debugMode) DrawDefaultInspector(); else DrawCustomInspector();
        }

        void DrawCustomInspector()
        {
            TC_RawImage rawImage = (TC_RawImage)target;

            TD.DrawSpacer();

            TD.DrawLabelWidthUnderline("Loaded stamp texture for GPU", 14);

            GUILayout.Space(25);
            Rect rect = GUILayoutUtility.GetLastRect();

            float width = Screen.width - (rect.x * 2);

            TD.DrawTexture(new Rect(rect.x, rect.y, width, width), rawImage.tex, Color.white);
            GUILayout.Space(width - 50);

            TD.DrawSpacer();

            TD.DrawLabelWidthUnderline("Details", 14);
            
            EditorGUILayout.BeginVertical("Box");
            
            EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel("Path");
                EditorGUILayout.LabelField(rawImage.fullPath);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel("Resolution");
                EditorGUILayout.LabelField(rawImage.resolution.ToString());
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel("Node References");
                EditorGUILayout.LabelField(rawImage.referenceCount.ToString());
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();

            TD.DrawSpacer();
        }
    }
}