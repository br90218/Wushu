using UnityEngine;
using UnityEditor;
using System.Collections;


namespace TerrainComposer2
{
    [CustomEditor(typeof(TC_ItemBehaviour), true)]
    [CanEditMultipleObjects]
    public class TC_ItemBehaviourEditor : Editor {

        SerializedProperty posOffset, positionMode;

        SerializedProperty lockTransform, lockPosX, lockPosY, lockPosZ, lockRotY, lockScaleX, lockScaleY, lockScaleZ, lockPosChildren;
        SerializedProperty overlay, posY, size;
        SerializedProperty previewEdit;
        SerializedProperty method;
        SerializedProperty doNormalize;

        SerializedProperty treeResolutionPM, objectResolutionPM, objectAreaSize, objectTransform, colormapResolution, meshResolution;

        SerializedProperty inputKind, inputTerrain, inputNoise, inputShape, inputFile, inputCurrent;
        SerializedProperty splatSelectIndex;

        SerializedProperty noise, lacunarity, octaves, persistence, seed; // frequency
        
        SerializedProperty clamp, radius;
        SerializedProperty stampTex, collisionMask, collisionMode;// collisionDirection;
        
        SerializedProperty shapes, topSize, bottomSize, shapeSize;
        SerializedProperty iterations, mipmapLevel, convexityStrength;

        // SelectItemGroup
        SerializedProperty mix, scaleMulti, scaleMinMaxMulti, linkScaleToMask;

        // Item
        SerializedProperty selectIndex, splatCustom, splatCustomValues;

        // Tree
        SerializedProperty tree, scaleRange, nonUniformScale, scaleCurve, randomPosition, heightOffset;

        // SpawnObject
        SerializedProperty spawnObject, go, rotRangeX, rotRangeY, rotRangeZ, lookAtTarget, lookAtX, heightRange, includeTerrainHeight;

        SPCurve localCurve = new SPCurve();
        SPCurve worldCurve = new SPCurve();

        TC_ItemBehaviour item;
        TC_TerrainLayer layerLevel;
        TC_LayerGroup layerGroup;
        TC_LayerGroupResult groupResult;
        TC_Layer layer;
        TC_NodeGroup nodeGroup;
        TC_Node node;
        TC_SelectItemGroup selectItemGroup;
        TC_SelectItem selectItem;

        Event eventCurrent;

        float scaleYOld;

        string[] layers;
        
        public class SPCurve
        {
            public SerializedProperty curveEntry;
            public SerializedProperty active;
            public SerializedProperty range;
            public SerializedProperty curve;
            public SerializedProperty type;

            public void Init(TC_ItemBehaviourEditor editor, string name)
            {
                curveEntry = editor.serializedObject.FindProperty(name);
                active = curveEntry.FindPropertyRelative("active");
                range = curveEntry.FindPropertyRelative("range");
                curve = curveEntry.FindPropertyRelative("curve");
                type = curveEntry.FindPropertyRelative("type");
            }
        }

        void OnEnable()
        {
            // ((TC_ItemBehaviour)target).t.hideFlags = HideFlags.None;
            item = (TC_ItemBehaviour)target;
            if (item == null) return;

            Undo.undoRedoPerformed += Repaint;
            TC_ItemBehaviour.DoRepaint += Repaint;
            
            layerLevel = target as TC_TerrainLayer;
            layerGroup = target as TC_LayerGroup;
            groupResult = target as TC_LayerGroupResult;
            layer = target as TC_Layer;
            nodeGroup = target as TC_NodeGroup;
            node = target as TC_Node;
            selectItemGroup = target as TC_SelectItemGroup;
            selectItem = target as TC_SelectItem;

            localCurve.Init(this, "localCurve");
            worldCurve.Init(this, "worldCurve");

            lockTransform = serializedObject.FindProperty("lockTransform");
            lockPosChildren = serializedObject.FindProperty("lockPosChildren");

            lockPosX = serializedObject.FindProperty("lockPosX");
            lockPosY = serializedObject.FindProperty("lockPosY");
            lockPosZ = serializedObject.FindProperty("lockPosZ");
            lockRotY = serializedObject.FindProperty("lockRotY");
            lockScaleX = serializedObject.FindProperty("lockScaleX");
            lockScaleY = serializedObject.FindProperty("lockScaleY");
            lockScaleZ = serializedObject.FindProperty("lockScaleZ");
            
            posOffset = serializedObject.FindProperty("posOffset");
            positionMode = serializedObject.FindProperty("positionMode");
            posY = serializedObject.FindProperty("posY");
            size = serializedObject.FindProperty("size");
            overlay = serializedObject.FindProperty("overlay");
            method = serializedObject.FindProperty("method");
            doNormalize = serializedObject.FindProperty("doNormalize");

            TC_Reporter.Log("OnEnable");

            if (layerLevel != null)
            {
                treeResolutionPM = serializedObject.FindProperty("treeResolutionPM");
                objectResolutionPM = serializedObject.FindProperty("objectResolutionPM");
                objectAreaSize = serializedObject.FindProperty("objectAreaSize");
                objectTransform = serializedObject.FindProperty("objectTransform");
                colormapResolution = serializedObject.FindProperty("colormapResolution");
                meshResolution = serializedObject.FindProperty("meshResolution");
            }
            else if (node != null)
            {
                inputKind = serializedObject.FindProperty("inputKind");
                inputTerrain = serializedObject.FindProperty("inputTerrain");
                inputNoise = serializedObject.FindProperty("inputNoise");
                inputShape = serializedObject.FindProperty("inputShape");
                inputFile = serializedObject.FindProperty("inputFile");
                inputCurrent = serializedObject.FindProperty("inputCurrent");

                splatSelectIndex = serializedObject.FindProperty("splatSelectIndex");

                iterations = serializedObject.FindProperty("iterations");
                mipmapLevel = serializedObject.FindProperty("mipmapLevel");
                convexityStrength = serializedObject.FindProperty("convexityStrength");

                noise = serializedObject.FindProperty("noise");

                if (noise != null)
                {
                    // frequency = noise.FindPropertyRelative("frequency");
                    lacunarity = noise.FindPropertyRelative("lacunarity");
                    octaves = noise.FindPropertyRelative("octaves");
                    persistence = noise.FindPropertyRelative("persistence");
                    seed = noise.FindPropertyRelative("seed");
                }
                else TC_Reporter.Log("No noise init");

                clamp = serializedObject.FindProperty("clamp");
                radius = serializedObject.FindProperty("radius");

                stampTex = serializedObject.FindProperty("stampTex");
                collisionMask = serializedObject.FindProperty("collisionMask");
                collisionMode = serializedObject.FindProperty("collisionMode");
                // collisionDirection = serializedObject.FindProperty("collisionDirection");

                shapes = serializedObject.FindProperty("shapes");

                if (shapes != null)
                {
                    topSize = shapes.FindPropertyRelative("topSize");
                    bottomSize = shapes.FindPropertyRelative("bottomSize");
                    shapeSize = shapes.FindPropertyRelative("size");
                }
            }
            else if (selectItemGroup != null)
            {
                mix = serializedObject.FindProperty("mix");
                scaleMinMaxMulti = serializedObject.FindProperty("scaleMinMaxMulti");
                scaleMulti = serializedObject.FindProperty("scaleMulti");
                linkScaleToMask = serializedObject.FindProperty("linkScaleToMask");
            }
            else if (selectItem != null)
            {
                selectIndex = serializedObject.FindProperty("selectIndex");
                splatCustom = serializedObject.FindProperty("splatCustom");
                splatCustomValues = serializedObject.FindProperty("splatCustomValues");
                
                if (selectItem.outputId == TC.treeOutput)
                {
                    tree = serializedObject.FindProperty("tree");

                    heightOffset = tree.FindPropertyRelative("heightOffset");
                    randomPosition = tree.FindPropertyRelative("randomPosition");
                     
                    scaleRange = tree.FindPropertyRelative("scaleRange");
                    scaleMulti = tree.FindPropertyRelative("scaleMulti");
                    nonUniformScale = tree.FindPropertyRelative("nonUniformScale");
                    scaleCurve = tree.FindPropertyRelative("scaleCurve");
                }
                else if (selectItem.outputId == TC.objectOutput)
                {
                    spawnObject = serializedObject.FindProperty("spawnObject");
                    go = spawnObject.FindPropertyRelative("go");

                    heightRange = spawnObject.FindPropertyRelative("heightRange");
                    heightOffset = spawnObject.FindPropertyRelative("heightOffset");

                    randomPosition = spawnObject.FindPropertyRelative("randomPosition");
                    includeTerrainHeight = spawnObject.FindPropertyRelative("includeTerrainHeight");
                    
                    rotRangeX = spawnObject.FindPropertyRelative("rotRangeX");
                    rotRangeY = spawnObject.FindPropertyRelative("rotRangeY");
                    rotRangeZ = spawnObject.FindPropertyRelative("rotRangeZ");

                    scaleRange = spawnObject.FindPropertyRelative("scaleRange");
                    scaleMulti = spawnObject.FindPropertyRelative("scaleMulti");
                    nonUniformScale = spawnObject.FindPropertyRelative("nonUniformScale");
                    scaleCurve = spawnObject.FindPropertyRelative("scaleCurve");

                    lookAtTarget = spawnObject.FindPropertyRelative("lookAtTarget");
                    lookAtX = spawnObject.FindPropertyRelative("lookAtX");
                }
            }
        }

        void OnDisable()
        {
            if (item != null)
            {
                if (item.controlDown)
                {
                    if (item.lockPosChildren)
                    {
                        item.lockPosChildren = false;
                        item.SetLockChildrenPosition(false);
                    }
                    item.controlDown = false;
                }
                TC_ItemBehaviour.DoRepaint -= Repaint;
            }
            Tools.hidden = false;
            Undo.undoRedoPerformed -= Repaint; 
        }
         
        void OnDestroy()
        {
            Undo.undoRedoPerformed -= Repaint;
            item = (TC_ItemBehaviour)target;
            if (item != null) TC_ItemBehaviour.DoRepaint -= Repaint; 
        }

        bool keyUp, cmdDuplicate, cmdDelete;
        
        void OnSceneGUI()
        {
            eventCurrent = Event.current;

            if (eventCurrent.type == EventType.KeyUp) keyUp = true;
            else if (eventCurrent.type == EventType.KeyDown) keyUp = false;

            if (eventCurrent.commandName == "Delete" || eventCurrent.commandName == "SoftDelete") { eventCurrent.Use(); cmdDelete = true; }
            else if (eventCurrent.commandName == "Duplicate") { eventCurrent.Use(); cmdDuplicate = true; }
            
            if (cmdDelete && keyUp) { cmdDelete = keyUp = false; TC_NodeWindow.DeleteKey(); return; }
            else if (cmdDuplicate && keyUp) { cmdDuplicate = keyUp = false; TC_NodeWindow.DuplicateKey(); return; }
            
            if (Tools.current == Tool.Rotate || Tools.current == Tool.Move || Tools.current == Tool.Scale) Tools.hidden = true;
            else { Tools.hidden = false; return; }

            if (selectItem != null || selectItemGroup != null) return;

            if (Tools.current == Tool.Move)
            {
                Undo.RecordObject(item.transform, "Move");
                Vector3 posOld;

                Vector3 pos = posOld = item.t.position;

                // if (GUIUtility.hotControl != 0 && node != null) pos.y *= node.t.lossyScale.y;
                
                GUI.changed = false;
                Undo.RecordObject(item.t, "Edit Transform");
                Undo.RecordObject(item, "Edit Transform");
                
                pos = Handles.PositionHandle(pos, Quaternion.identity);

                if (item.lockTransform)
                {
                    if (item.lockPosX && pos.x != posOld.x) { TC.AddMessage(item.name + " position X is locked."); pos.x = posOld.x; }
                    if (item.lockPosY && pos.y != posOld.y) { TC.AddMessage(item.name + " position Y is locked."); pos.y = posOld.y; }
                    if (item.lockPosZ && pos.z != posOld.z) { TC.AddMessage(item.name + " position Z is locked."); pos.z = posOld.z; }
                }
                
                if (!item.lockPosParent)
                {
                    if (item.t.position != pos) item.t.position = pos;

                    bool posYChanged = false;

                    if (!(item.lockTransform && item.lockPosY))
                    {
                        if (node == null)
                        {
                            float deltaY = pos.y - posOld.y;

                            if (deltaY != 0)
                            {
                                item.ChangeYPosition(deltaY);
                                posYChanged = true;
                                item.posY = pos.y;
                            }
                            if (GUIUtility.hotControl == 0) item.posY = 0;
                        }
                        else
                        {
                            if (GUIUtility.hotControl != 0)
                            {
                                float newPosY = pos.y / node.t.lossyScale.y;
                                if (node.posY != node.posYOld + newPosY) posYChanged = true;
                                node.posY = node.posYOld + newPosY;
                            }
                            else node.posYOld = node.posY;
                        }

                        if (GUIUtility.hotControl == 0)
                        {
                            item.t.position = new Vector3(pos.x, 0, pos.z);
                            item.t.hasChanged = false;
                        }
                    }

                    if ((pos.x != posOld.x || pos.z != posOld.z) || posYChanged)
                    {
                        item.UpdateTransforms();
                        item.t.hasChanged = false;
                        AutoGenerate();
                        DoRepaint();
                    }
                }
                else if (GUI.changed) TC.AddMessage(item.name + " positioning is locked.");
            }
            
            else if (Tools.current == Tool.Rotate)
            {
                Undo.RecordObject(item.transform, "Rotate");
                Handles.color = Color.blue;
                Handles.Slider(item.t.position, item.t.localRotation * new Vector3(0, 0, 1));
                Handles.SphereCap(0, item.t.position, Quaternion.identity, 0.15f * HandleUtility.GetHandleSize(item.t.position));
                Handles.color = Color.white; 
                GUI.changed = false;
                
                Quaternion rot = Handles.Disc(item.t.rotation, item.t.position, new Vector3(0, 1, 0), 1.5f * HandleUtility.GetHandleSize(item.t.position), false, 0);

                if (!(item.lockTransform && item.lockRotY))
                {
                    if (item.t.rotation != rot)
                    {
                        item.t.rotation = rot;
                        item.UpdateTransforms();
                        item.t.hasChanged = false;
                        AutoGenerate();
                        DoRepaint();
                    }
                }
                else { TC.AddMessage(item.name + " rotation Y is locked."); }
            }
            else if (Tools.current == Tool.Scale)
            {
                Undo.RecordObject(item.transform, "Scale");
                Vector3 scaleOld = item.t.localScale;
                GUI.changed = false;
                Vector3 scale = Handles.ScaleHandle(scaleOld, item.t.position, node != null ? item.t.rotation : Quaternion.identity, HandleUtility.GetHandleSize(item.t.position));

                bool freezeScaleY = false;

                if (node != null)
                {
                    if (node.nodeType == NodeGroupType.Mask) freezeScaleY = true;
                }

                if (item.outputId != TC.heightOutput) freezeScaleY = true;

                if (freezeScaleY)
                {
                    if (scale.x != scaleOld.x || scale.z != scaleOld.z) scale.y = scaleOld.y;
                }

                if (node == null)
                {
                    if (scale.x != scaleOld.x) scale.z = scale.x;
                    else if (scale.z != scaleOld.z) scale.x = scale.z;
                }
                
                if (item.lockTransform)
                {
                    if (item.lockScaleX && scale.x != scaleOld.x) { TC.AddMessage(item.name + " scale X is locked."); scale.x = scaleOld.x; }
                    if (item.lockScaleY && scale.y != scaleOld.y) { TC.AddMessage(item.name + " scale Y is locked."); scale.y = scaleOld.y; }
                    if (item.lockScaleZ && scale.z != scaleOld.z) { TC.AddMessage(item.name + " scale Z is locked."); scale.z = scaleOld.z; }
                }
                
                if (scale.x != scaleOld.x || scale.y != scaleOld.y || scale.z != scaleOld.z)
                {
                    item.t.localScale = scale;
                    item.UpdateTransforms();
                    item.t.hasChanged = false;
                    AutoGenerate();
                    DoRepaint();
                }
            }
        }

        public void DoRepaint()
        {
            TC.repaintNodeWindow = true;
            Repaint();
        }

        public override void OnInspectorGUI()
        {
            if (TC_Settings.instance == null) return;
            serializedObject.Update();
            
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space((Screen.width / 2) - 130);

            string control = GUI.GetNameOfFocusedControl();
            if (control == "PreviewEdit") { GUI.color = Color.red; TD.PreviewEdit(item); }

            GUI.SetNextControlName("PreviewEdit");
            if (GUILayout.Button("", GUILayout.Width(260), GUILayout.Height(260))) GUI.FocusControl("PreviewEdit");
            GUI.color = Color.white;
            Rect rect = GUILayoutUtility.GetLastRect();
            EditorGUILayout.EndHorizontal();

            Rect previewTexRect = new Rect((Screen.width / 2) - 111, rect.y + 5f, 250, 250);

            //bool splatCustom = false;
            //if (selectItem != null)
            //{
            //    if (selectItem.splatCustom) splatCustom = true;
            //}
            
            if (item.outputId == TC.colorOutput && selectItem != null)
            {
                GUI.color = selectItem.color;
                EditorGUI.DrawPreviewTexture(previewTexRect, Texture2D.whiteTexture);
                GUI.color = Color.white;
            }
            // else if (splatCustom) TC_SelectItemGUI.DrawSplatCustomPreview(selectItem, previewTexRect);
            else if (item.rtDisplay != null) EditorGUI.DrawPreviewTexture(previewTexRect, item.rtDisplay);
            else if (item.preview.tex != null) EditorGUI.DrawPreviewTexture(previewTexRect, item.preview.tex);

            if (node != null)
            {
                TD.DrawSpacer();
                GUI.color = Color.red * TD.editorSkinMulti;
                EditorGUILayout.BeginVertical("Box");
                GUI.color = Color.white;
                TD.DrawProperty(posY, new GUIContent("Height"));
                TD.DrawProperty(size);
                
                EditorGUILayout.EndVertical();
                DrawImageSettings();
                TD.DrawSpacer();
            }

            if (layerGroup != null)
            {
                if (layerGroup.level == 0)
                {
                    if (layerGroup.outputId == TC.heightOutput) DrawTerrainHeightSlider(new Rect(previewTexRect.x + 265, previewTexRect.y, 25, previewTexRect.height));
                    else if (layerGroup.outputId == TC.treeOutput || layerGroup.outputId == TC.objectOutput || layerGroup.outputId == TC.colorOutput) DrawLayerGroupOutput();
                }
            }

            if (layerGroup != null || groupResult != null || layer != null || nodeGroup != null)
            {
                GUI.color = Color.red * TD.editorSkinMulti;
                EditorGUILayout.BeginVertical("Box");
                GUI.color = Color.white;
                
                string tooltip = "The pivot can be moved without affecting the position of all the children.\nWhen rescaling the scale will be taken from each node seperately.\n\nPress 'F' key to toggle.\nHold 'Control' key to enable.";

                TD.DrawProperty(lockPosChildren, new GUIContent("Lock Position Children",tooltip));

                if (GUI.changed)
                {
                    item.lockPosChildren = lockPosChildren.boolValue;
                    item.SetLockChildrenPosition(false);
                }

                EditorGUILayout.EndVertical();
            }

            if (layerLevel != null)
            {
                TD.DrawProperty(treeResolutionPM, new GUIContent("Tree resolution per meter"));
                TD.DrawProperty(objectResolutionPM);
                TD.DrawProperty(objectAreaSize);
                TD.DrawProperty(objectTransform);
                TD.DrawProperty(colormapResolution);
                TD.DrawProperty(meshResolution);
            }

            if (item.outputId == TC.splatOutput)
            {
                if (layer != null || layerGroup != null)
                {
                    GUI.color = Color.red;
                    EditorGUILayout.BeginVertical("Box");
                    GUI.color = Color.white;
                        TD.DrawProperty(doNormalize, new GUIContent("Normalize"));
                    EditorGUILayout.EndVertical();
                }
            }

            if (node != null)
            {
                GUI.color = Color.white;
                TD.DrawProperty(inputKind);
                if (GUI.changed) node.Init();

                if (node.inputKind == InputKind.Terrain)
                {
                    if (node.outputId == TC.heightOutput)
                    {
                        InputTerrainHeight popup = InputTerrainHeight.Collision;
                        EditorGUILayout.EnumPopup("Input Terrain", popup);
                        inputTerrain.enumValueIndex = (int)popup;
                    }
                    else TD.DrawProperty(inputTerrain);

                    if (node.inputTerrain == InputTerrain.Convexity)
                    {
                        TD.DrawProperty(convexityStrength, new GUIContent("Convexity Strength","Positive strength is Convex. Negative strength is Concave."));
                        TD.DrawProperty(mipmapLevel, new GUIContent("Radius"));
                        if (mipmapLevel.intValue < 1) mipmapLevel.intValue = 1;
                        else if (mipmapLevel.intValue > 8) mipmapLevel.intValue = 8;
                    }
                    else if (node.inputTerrain == InputTerrain.Splatmap)
                    {
                        if (TC_Settings.instance.hasMasterTerrain)
                        {
                            EditorGUILayout.BeginHorizontal();
                            DrawIntSlider(splatSelectIndex, 0, TC_Settings.instance.masterTerrain.terrainData.splatPrototypes.Length - 1, new GUIContent("Splat Index"));
                            EditorGUILayout.EndHorizontal();
                            EditorGUILayout.BeginHorizontal();
                            GUILayout.Space(5);
                            EditorGUILayout.PrefixLabel(" ");
                            DrawPreviewTexture(TC_Settings.instance.masterTerrain.terrainData.splatPrototypes[splatSelectIndex.intValue].texture, Color.white, Color.white, 150, 150);
                            EditorGUILayout.EndHorizontal();
                        }
                    }
                    else if (node.inputTerrain == InputTerrain.Normal)
                    {
                        DrawCommingSoon("The normal node will be added in another beta release.");
                    }
                    else if (node.inputTerrain == InputTerrain.Collision)
                    {
                        if (layers == null) layers = new string[32];
                        else if (layers.Length != 32) layers = new string[32];

                        for (int i = 0; i < 32; i++)
                        {
                            layers[i] = LayerMask.LayerToName(i);
                            if (layers[i] == "") layers[i] = "Default";
                        }

                        GUI.changed = false;
                        int mask = collisionMask.intValue;
                        mask = EditorGUILayout.MaskField(new GUIContent("Collision Mask"), mask, layers);
                        if (GUI.changed)
                        {
                            collisionMask.intValue = mask;
                            AutoGenerate();
                        }

                        GUILayout.Space(5);

                        TD.DrawProperty(collisionMode);
                        // TD.DrawProperty(collisionDirection);
                    }
                }
                else if (node.inputKind == InputKind.Noise) { DrawNoise(); }
                else if (node.inputKind == InputKind.Shape) { DrawShape(); }
                else if (node.inputKind == InputKind.File)
                {
                    TD.DrawProperty(inputFile);

                    if (node.inputFile == InputFile.RawImage) DrawRawImage();
                    else if (node.inputFile == InputFile.Image) DrawCommingSoon("The image node will be added in another beta release.");
                }
                else if (node.inputKind == InputKind.Current)
                {
                    TD.DrawProperty(inputCurrent);

                    DrawCurrentBlur();

                    if (node.inputCurrent == InputCurrent.Distortion)
                    {
                        TD.DrawProperty(radius);
                        DrawNoise();
                    }
                }
                else if (node.inputKind == InputKind.Portal)
                {
                    DrawCommingSoon("Portals will be added in another beta release.");
                }
            }
            else if (selectItemGroup != null)
            {
                if (selectItemGroup.outputId != TC.heightOutput) DrawSelectItemGroup();
            }
            else if (selectItem != null)
            {
                if (selectItem.outputId == TC.colorOutput) DrawColorSelectItem();
                else if (selectItem.outputId == TC.splatOutput) DrawSelectItem();
                else if (selectItem.outputId == TC.grassOutput) DrawSelectItem();
                else if (selectItem.outputId == TC.treeOutput) DrawTreeSelectItem();
                else if (selectItem.outputId == TC.objectOutput) DrawObjectSelectItem();
            }

            if (nodeGroup != null || node != null)
            {
                TD.DrawSpacer();
                TD.DrawLabelWidthUnderline("Curves", 14);
                DrawCurve(localCurve, "Local Height Curve");
                GUILayout.Space(5);
                DrawCurve(worldCurve, "Global Height Curve");
            }

            if (TC_Generate.instance.generateDone != TC_Generate.instance.generateDoneOld)
            {
                // Debug.Log("Generate Done");
                Repaint();
                TC_Generate.instance.generateDoneOld = TC_Generate.instance.generateDone;
            }

            // DrawMethod();
            // Draw Inspector
            // if (node == null)
            if (TC_Settings.instance.drawDefaultInspector) base.OnInspectorGUI();  
            // TD.Spacer();
            serializedObject.ApplyModifiedProperties();

            if (selectItem == null && selectItemGroup == null)
            {
                if (item.t.hasChanged)
                {
                    item.t.hasChanged = false;
                    if (node == null) item.t.localScale = new Vector3(item.t.localScale.x, item.t.localScale.y, item.t.localScale.x);
                    // Debug.Log(item.name);
                    AutoGenerate();
                }
            }
        }

        void AutoGenerate()
        {
            TC.AutoGenerate();
            DoRepaint();
        }
        
        static public void CheckKeyLockOnSelection(Event eventCurrent)
        {
            for (int i = 0; i < Selection.transforms.Length; i++)
            {
                TC_ItemBehaviour item = Selection.transforms[i].GetComponent<TC_ItemBehaviour>();

                if (item != null)
                {
                    if (item.GetType() != typeof(TC_SelectItem) && item.GetType() != typeof(TC_SelectItemGroup))
                    {
                        if (item.GetType() != typeof(TC_Node))
                        {
                            if (eventCurrent.control)
                            {
                                if (!item.lockPosChildren) item.controlDown = true;
                                if (!item.lockPosChildren)
                                {
                                    item.lockPosChildren = true;
                                    item.SetLockChildrenPosition(false);
                                }
                            }
                            else
                            {
                                if (item.lockPosChildren && item.controlDown)
                                {
                                    item.controlDown = false;
                                    item.lockPosChildren = false;
                                    item.SetLockChildrenPosition(false);
                                }
                            }
                        }

                        if (eventCurrent.keyCode == KeyCode.L && eventCurrent.type == EventType.KeyDown)
                        {
                            if (item.GetType() != typeof(TC_Node))
                            {
                                item.lockPosChildren = !item.lockPosChildren;
                                item.SetLockChildrenPosition(false);
                            }
                            else item.lockTransform = !item.lockTransform;

                            // EditorUtility.SetDirty(item);
                        }
                    }
                }
            }
        }

        void DrawMethod()
        {
            if (item.level > 1 && nodeGroup == null && selectItem == null && selectItemGroup == null)
            {
                if (item.t.parent != null)
                {
                    if (item.t.GetSiblingIndex() != item.t.parent.childCount - 1)
                    {
                        TD.DrawSpacer();

                        if (node != null)
                        {
                            if (node.inputKind != InputKind.Current) TD.DrawProperty(method);
                        }
                        else
                        {
                            if (item.outputId != TC.treeOutput && item.outputId != TC.objectOutput) TD.DrawProperty(method);
                            else
                            {
                                MethodItem m = (MethodItem)method.enumValueIndex;
                                m = (MethodItem)EditorGUILayout.EnumPopup(new GUIContent("Method"), m);
                                method.enumValueIndex = (int)m;
                            }
                        }

                        if (((Method)method.enumValueIndex) == Method.Lerp) DrawSlider(overlay, 0, 1);
                    }
                }
            }
        }

        public void DrawPosOffset()
        {
            EditorGUILayout.BeginVertical("Box");
            // -------
            item.t.localPosition = EditorGUILayout.Vector3Field("Position", item.t.localPosition);

            float rotY = item.t.localEulerAngles.y;
            GUI.changed = false;
            rotY = EditorGUILayout.Slider("Rotation", rotY, -180, 180);
            if (GUI.changed) item.t.localEulerAngles = new Vector3(0, rotY, 0);

            item.t.localScale = EditorGUILayout.Vector3Field("Scale", item.t.localScale);
            GUILayout.Space(5);
            TD.DrawProperty(posOffset);
            TD.DrawProperty(positionMode);
            // -------
            EditorGUILayout.EndVertical();
        }

        public void DrawNoise()
        {
            TD.DrawProperty(inputNoise);
            if (GUI.changed) node.Init();

            if (inputNoise.enumValueIndex == (int)InputNoise.Voronoi) { DrawCommingSoon("This noise will be enabled in another beta release."); return; }

            //if (inputNoise.enumValueIndex != (int)InputNoise.Random)
            //{
                // TD.DrawProperty(frequency);
                TD.DrawProperty(lacunarity);
                TD.DrawProperty(persistence);
                TD.DrawProperty(octaves);

                if (GUI.changed)
                {
                    if (octaves.intValue < 1) octaves.intValue = 1;
                    else if (octaves.intValue > 12) octaves.intValue = 12;
                }
                TD.DrawProperty(seed);
            //}
        }

        public void DrawCommingSoon(string text)
        {
            GUILayout.Space(5);
            TD.DrawLabel(text, 14);
            GUILayout.Space(5);
        }

        public void DrawCurve(SPCurve spCurve, string label)
        {
            GUI.color = Color.blue * TD.editorSkinMulti;
            EditorGUILayout.BeginVertical("Box");
            GUI.color = Color.white;
            // -------

            TD.DrawProperty(spCurve.active);
            // GUILayout.Space(3);
            bool active = spCurve.active.boolValue;

            if (!active) GUI.color = new Color(1, 1, 1, 0.35f);
            DrawMinMaxSlider(spCurve.range, 0, 1, 0.001f, new GUIContent("Curve Range"));
            // GUILayout.Space(3);

            EditorGUILayout.BeginHorizontal();
            TD.DrawProperty(spCurve.curve, new GUIContent(label));
            if (GUILayout.Button("I", EditorStyles.miniButtonMid, GUILayout.Width(25)))
            {
                spCurve.curve.animationCurveValue = Mathw.InvertCurve(spCurve.curve.animationCurveValue);
                AutoGenerate();
            }
            EditorGUILayout.EndHorizontal();

            GUI.color = Color.white;
            // TD.DrawProperty(spCurve.type);

            // -------
            EditorGUILayout.EndVertical();
        }

        public void DrawShape()
        {
            TD.DrawProperty(inputShape);

            if (shapeSize == null) TC_Reporter.Log("Shape Size = null");
            if (node.inputShape == InputShape.Circle) TD.DrawProperty(shapeSize, new GUIContent("Radius"));
            else if (node.inputShape == InputShape.Gradient) TD.DrawProperty(shapeSize, new GUIContent("Size"));
            else if (node.inputShape == InputShape.Rectangle)
            {
                GUI.changed = false;
                EditorGUILayout.PropertyField(topSize);
                EditorGUILayout.PropertyField(bottomSize);
                if (GUI.changed)
                {
                    topSize.vector2Value = new Vector2(topSize.vector2Value.x, topSize.vector2Value.x * (bottomSize.vector2Value.y / bottomSize.vector2Value.x));
                    AutoGenerate();
                }
            }
        }

        public void DrawImageSettings()
        {
            // EditorGUILayout.PrefixLabel(" ");
            // EditorGUILayout.LabelField("Clamp", GUILayout.Width(40));

            GUI.color = Color.red * TD.editorSkinMulti;
            EditorGUILayout.BeginVertical("Box");
            GUI.color = Color.white;

            TD.DrawProperty(lockTransform);

            if (lockTransform.boolValue)
            {
                EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PrefixLabel(" Position");
                    if (lockPosX.boolValue) GUI.backgroundColor = Color.green;
                    if (GUILayout.Button("X", EditorStyles.miniButtonMid, GUILayout.Width(25))) lockPosX.boolValue = !lockPosX.boolValue;
                    GUI.backgroundColor = lockPosY.boolValue ? Color.green : Color.white;
                    if (GUILayout.Button("Y", EditorStyles.miniButtonMid, GUILayout.Width(25))) lockPosY.boolValue = !lockPosY.boolValue;
                    GUI.backgroundColor = lockPosZ.boolValue ? Color.green : Color.white;
                    if (GUILayout.Button("Z", EditorStyles.miniButtonMid, GUILayout.Width(25))) lockPosZ.boolValue = !lockPosZ.boolValue;
                    GUI.backgroundColor = Color.white;
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PrefixLabel(" Rotation");
                    GUI.backgroundColor = lockRotY.boolValue ? Color.green : Color.white;
                    GUILayout.Space(25);
                    if (GUILayout.Button("Y", EditorStyles.miniButtonMid, GUILayout.Width(25))) lockRotY.boolValue = !lockRotY.boolValue;
                    GUI.backgroundColor = Color.white;
                EditorGUILayout.EndHorizontal();

                
                EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PrefixLabel(" Scale");
                    if (lockScaleX.boolValue) GUI.backgroundColor = Color.green;
                    if (GUILayout.Button("X", EditorStyles.miniButtonMid, GUILayout.Width(25))) lockScaleX.boolValue = !lockScaleX.boolValue;
                    GUI.backgroundColor = lockScaleY.boolValue ? Color.green : Color.white;
                    if (GUILayout.Button("Y", EditorStyles.miniButtonMid, GUILayout.Width(25))) lockScaleY.boolValue = !lockScaleY.boolValue;
                    GUI.backgroundColor = lockScaleZ.boolValue ? Color.green : Color.white;
                    if (GUILayout.Button("Z", EditorStyles.miniButtonMid, GUILayout.Width(25))) lockScaleZ.boolValue = !lockScaleZ.boolValue;
                    GUI.backgroundColor = Color.white;
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndVertical();

            GUI.color = Color.red * TD.editorSkinMulti;
            EditorGUILayout.BeginVertical("Box");
            GUI.color = Color.white;

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Flip Scale");
            if (item.t.localScale.x < 0) GUI.backgroundColor = Color.green;
            if (GUILayout.Button("X", EditorStyles.miniButtonMid, GUILayout.Width(25))) SetTransformScale(new Vector3(-item.t.localScale.x, item.t.localScale.y, item.t.localScale.z), "Flip X");
            GUI.backgroundColor = item.t.localScale.y < 0 ? Color.green : Color.white;
            if (GUILayout.Button("Y", EditorStyles.miniButtonMid, GUILayout.Width(25))) SetTransformScale(new Vector3(item.t.localScale.x, -item.t.localScale.y, item.t.localScale.z), "Flip Y");
            GUI.backgroundColor = item.t.localScale.z < 0 ? Color.green : Color.white;
            if (GUILayout.Button("Z", EditorStyles.miniButtonMid, GUILayout.Width(25))) SetTransformScale(new Vector3(item.t.localScale.x, item.t.localScale.y, -item.t.localScale.z), "Flip Z");
            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndHorizontal();
            
            TD.DrawProperty(clamp);

            EditorGUILayout.EndVertical();
        }

        public void SetTransformScale(Vector3 scale, string undoName)
        {
            Undo.RecordObject(item.t, undoName);
            item.t.localScale = scale;
        }

        public void DrawRawImage()
        {
            if (TC_Settings.instance.presetMode == PresetMode.StampMode)
            {
                GUI.changed = false;
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel("Stamp Texture");
                stampTex.objectReferenceValue = (Texture)EditorGUILayout.ObjectField((Texture)stampTex.objectReferenceValue, typeof(Texture), false, GUILayout.Width(75), GUILayout.Height(75));
                if (stampTex.objectReferenceValue != null)
                {
                    EditorGUILayout.LabelField(stampTex.objectReferenceValue.name);
                }

                if (GUI.changed)
                {
                    Texture tex = (Texture)stampTex.objectReferenceValue;
                    if (!node.DropTextureEditor(tex))
                    {
                        stampTex.objectReferenceValue = null;
                        DoRepaint();
                    }
                    else AutoGenerate();
                    // Debug.Log(node.rawImageIndex);
                }
                EditorGUILayout.EndHorizontal();
            }
        }

        public void DrawCurrentBlur()
        {
            DrawIntSlider(iterations, 1, 30);
        }

        void DrawSelectItemGroup()
        {
            if (selectItemGroup.itemList.Count == 0) return;

            EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Mixing", GUILayout.Width(50));
                DrawSlider(mix, 0, 1.5f, new GUIContent("","Mixes " + TC.outputNames[item.outputId] + " Items to have more overlap\n 0 = no overlap, 1.5 = max overlap."));
            
                if (GUI.changed)
                {
                    serializedObject.ApplyModifiedProperties();
                    selectItemGroup.CalcPreview();
                    DoRepaint();
                }
            EditorGUILayout.EndHorizontal();
            
            if (GUILayout.Button("Reset Sliders")) selectItemGroup.ResetRanges();

            GUI.color = TD.editorSkinMulti != 1 ? new Color(0.1f, 0.0f, 0.0f, 0.5f) : Color.red;
            EditorGUILayout.BeginVertical("Box");
            GUI.color = Color.white;

            for (int i = 0; i < selectItemGroup.itemList.Count; i++)
            {
                TC_SelectItem selectItem1 = selectItemGroup.itemList[i];

                EditorGUILayout.BeginHorizontal();
                if (selectItemGroup.outputId != TC.colorOutput)
                {
                    DrawPreviewTexture(selectItem1.preview.tex, (TD.hoverItem == selectItem1 ? Color.green : Color.white) * (selectItem1.active ? 1 : 0.75f), Color.white);
                }
                else DrawPreviewTexture(Texture2D.whiteTexture, Color.white, selectItem1.color);

                DrawRangeSlider(selectItem1, true);
                EditorGUILayout.EndHorizontal();
            }
            
            EditorGUILayout.EndVertical();

            if (selectItemGroup.outputId == TC.treeOutput || selectItemGroup.outputId == TC.objectOutput)
            {
                TD.DrawSpacer();

                GUI.color = Color.blue * TD.editorSkinMulti;
                EditorGUILayout.BeginVertical("Box");
                GUI.color = Color.white;

                TD.DrawLabelWidthUnderline("Scale", 14);
                
                DrawVector2(scaleMinMaxMulti, true, "Min", "Max", new GUIContent("Scale Range Multiplier"));
                
                GUI.changed = false;
                TD.DrawProperty(scaleMulti, new GUIContent("Scale Multiplier")); 
                if (GUI.changed)
                {
                    if (scaleMulti.floatValue < 0.01f) scaleMulti.floatValue = 0.01f;
                }

                TD.DrawProperty(linkScaleToMask);
                EditorGUILayout.EndVertical();

                TD.DrawSpacer();
            }
        }

        static public void DrawPreviewTexture(Texture2D tex, Color color, Color colTex, int width = 50, int height = 50)
        {
            GUI.backgroundColor = color;
            if (GUILayout.Button("", GUILayout.Width(width), GUILayout.Height(height))) { }
            GUI.backgroundColor = Color.white;

            Rect rect = GUILayoutUtility.GetLastRect();
            rect.x += 4;
            rect.y += 4;
            rect.width -= 8;
            rect.height -= 8;
            
            if (tex != null)
            {
                GUI.color = colTex;
                EditorGUI.DrawPreviewTexture(rect, tex);
                GUI.color = Color.white;
            }
        }

        void DrawTerrainHeightSlider(Rect rect)
        {
            if (!TC_Settings.instance.hasMasterTerrain) return;

            TC_Area2D area2D = TC_Area2D.current;

            Vector3 size = area2D.currentTerrainArea.terrainSize;

            GUI.changed = false;
            size.y = (int)GUI.VerticalSlider(rect, size.y, area2D.terrainHeightRange.y, area2D.terrainHeightRange.x);

            size.y = EditorGUILayout.FloatField("Terrain Height", size.y);
            if (GUI.changed)
            {
                if (size.y < 1) size.y = 1;
                area2D.currentTerrainArea.terrainSize = size;
                area2D.currentTerrainArea.ApplySize();
                DoRepaint();
                TC.AutoGenerate(false);
            }
        }

        void DrawSelectItem()
        {
            if (!TC_Settings.instance.hasMasterTerrain) return;

            int length = selectItem.GetItemTotalFromTerrain();

            GUI.changed = false;
            DrawIntSlider(selectIndex, 0, length - 1, new GUIContent(TC.outputNames[selectItem.outputId] + " Texture"));

            if (GUI.changed) selectItem.Refresh();

            if (selectItem.outputId == TC.splatOutput)
            {
                TD.DrawProperty(splatCustom);

                if (splatCustom.boolValue)
                {
                    EditorGUILayout.BeginVertical("Box");
                    for (int i = 0; i < length; i++)
                    {
                        EditorGUILayout.BeginHorizontal();
                        GUILayout.Space(15);
                        DrawPreviewTexture(TC_Settings.instance.masterTerrain.terrainData.splatPrototypes[i].texture, Color.white, Color.white);
                        GUILayout.Space(5);
                        DrawSlider(splatCustomValues.GetArrayElementAtIndex(i), 0, 1, new GUIContent(""));
                        EditorGUILayout.EndHorizontal();
                    }
                    GUILayout.Space(5);
                    EditorGUILayout.EndVertical();
                }
            }
        }

        void DrawColorSelectItem()
        {
            // TD.DrawProperty(color);
        }

        void DrawLayerGroupOutput()
        {
            TC_TerrainLayer terrainLayer = layerGroup.parentItem as TC_TerrainLayer;

            if (terrainLayer != null)
            {
                TD.DrawSpacer();
                
                Undo.RecordObject(terrainLayer, "Resolution");
                GUI.changed = false;

                EditorGUILayout.BeginHorizontal();
                if (layerGroup.outputId == TC.treeOutput)
                {
                    terrainLayer.treeResolutionPM = EditorGUILayout.IntField(new GUIContent("Tree Resolution Per Meter"), terrainLayer.treeResolutionPM);
                    if (terrainLayer.treeResolutionPM > 89) terrainLayer.treeResolutionPM = 89;
                }
                else if (layerGroup.outputId == TC.objectOutput)
                {
                    terrainLayer.objectResolutionPM = EditorGUILayout.IntField(new GUIContent("Object Resolution Per Meter"), terrainLayer.objectResolutionPM);
                    if (terrainLayer.objectResolutionPM > 89) terrainLayer.objectResolutionPM = 89;
                }
                else if (layerGroup.outputId == TC.colorOutput) terrainLayer.colormapResolution = EditorGUILayout.IntField(new GUIContent("Colormap Resolution"), terrainLayer.colormapResolution);

                if (layerGroup.active)
                {
                    if (GUILayout.Button("Refresh", GUILayout.Width(60))) AutoGenerate();
                }
                EditorGUILayout.EndHorizontal();

                if (GUI.changed) EditorUtility.SetDirty(terrainLayer);
                
                TD.DrawSpacer();
            }
        }
        
        void DrawTreeSelectItem()
        {
            if (!TC_Settings.instance.hasMasterTerrain) return;
            TD.DrawSpacer();

            int treeLength = TC_Settings.instance.masterTerrain.terrainData.treePrototypes.Length;

            DrawIntSlider(selectIndex, 0, treeLength - 1, new GUIContent("Tree Index"));
            if (GUI.changed) selectItem.Refresh();

            TD.DrawSpacer();

            GUI.color = Color.red * TD.editorSkinMulti;
            EditorGUILayout.BeginVertical("Box");
                GUI.color = Color.white;
                TD.DrawLabelWidthUnderline("Position", 14);
                DrawSlider(randomPosition, 0, 1, new GUIContent("Random Position"));
                TD.DrawProperty(heightOffset);
            EditorGUILayout.EndVertical();

            TD.DrawSpacer();

            DrawScale();

            TD.DrawSpacer();
        }

        void DrawScale()
        {
            GUI.color = Color.blue * TD.editorSkinMulti;
            EditorGUILayout.BeginVertical("Box");
            GUI.color = Color.white;

            TD.DrawLabelWidthUnderline("Scale", 14);

            EditorGUILayout.BeginHorizontal();
            // GUILayout.Space(15);
            
            DrawVector2(scaleRange, true, "Min", "Max", new GUIContent("Scale Range"));
           
            EditorGUILayout.EndHorizontal();

            GUI.changed = false;
            TD.DrawProperty(scaleMulti);
            if (GUI.changed)
            {
                if (scaleMulti.floatValue < 0.01f) scaleMulti.floatValue = 0.01f;
            }

            DrawSlider(nonUniformScale, 0, 1);
            TD.DrawProperty(scaleCurve);

            TC_SelectItemGroup selectableItemGroup = selectItem.parentItem;
            if (selectableItemGroup != null)
            {
                if (selectableItemGroup.itemList.Count == 1)
                {
                    Undo.RecordObject(selectableItemGroup, "Link Scale To Mask");
                    GUI.changed = false;
                    selectableItemGroup.linkScaleToMask = EditorGUILayout.Toggle("Link Scale To Mask", selectableItemGroup.linkScaleToMask);
                    if (GUI.changed) AutoGenerate();
                }
            }
            else Repaint();

            GUILayout.Space(5);

            EditorGUILayout.EndVertical();
        }
        

        void DrawObjectSelectItem()
        {
            TD.DrawSpacer();

            EditorGUILayout.BeginVertical("Box");

            TD.DrawProperty(go, new GUIContent("Spawn Object"));
            if (GUI.changed) selectItem.Refresh();

            EditorGUILayout.EndVertical();

            TD.DrawSpacer();

            
            GUI.color = Color.red * TD.editorSkinMulti;
            EditorGUILayout.BeginVertical("Box");
            GUI.color = Color.white;

            TD.DrawLabelWidthUnderline("Position", 14);
            
            DrawSlider(randomPosition, 0, 1, new GUIContent("Random Position"));
            DrawVector2(heightRange, false, "Min", "Max", new GUIContent("Height Range"));
            // TD.DrawProperty(includeScale); TODO: Is this needed?
            TD.DrawProperty(heightOffset);
            TD.DrawProperty(includeTerrainHeight);
            EditorGUILayout.EndVertical();


            TD.DrawSpacer();

            GUI.color = Color.yellow * TD.editorSkinMulti;
            EditorGUILayout.BeginVertical("Box");
            GUI.color = Color.white;

            TD.DrawLabelWidthUnderline("Rotation", 14);

            DrawVector2(rotRangeX, false, "Min", "Max", new GUIContent("Rotation X Range"));
            DrawVector2(rotRangeY, false, "Min", "Max", new GUIContent("Rotation Y Range"));
            DrawVector2(rotRangeZ, false, "Min", "Max", new GUIContent("Rotation Z Range"));

            TD.DrawProperty(lookAtTarget);
            if (lookAtTarget.objectReferenceValue != null) TD.DrawProperty(lookAtX, new GUIContent("  Include X-Axis"));
            EditorGUILayout.EndVertical();

            TD.DrawSpacer();

            DrawScale();

            TD.DrawSpacer();
        }

        public void DrawRangeSlider(TC_SelectItem item, bool labelSpace)
        {
            if (labelSpace) EditorGUILayout.LabelField("");
            Rect rect = GUILayoutUtility.GetLastRect();
            
            if (item.active)
            {
                GUI.color = (TD.hoverItem == item ? Color.green : Color.white);
                item.range = TD.MinMaxSlider.Draw(rect, item.range, 0.0f, 1.0f, new Vector2(200.0f, 25.0f));

                if (TD.MinMaxSlider.changed)
                {
                    selectItemGroup.SetRanges(item);
                    DoRepaint();
                }
            }
            else
            {
                GUI.color = (TD.hoverItem == item ? new Color(0, 1, 0, 0.35f) : new Color(1, 1, 1, 0.35f));
                TD.MinMaxSlider.Draw(rect, item.range, 0.0f, 1.0f, new Vector2(200.0f, 25.0f));
            }

            // if (GUIW.changed) Repaint();

            //if (global_script.tooltip_mode != 0)
            //{
            //    tooltip_text = "Center this value to 50";
            //}
            GUILayout.Space(3);

            if (GUILayout.Button(new GUIContent("C", "Center this value"), EditorStyles.miniButtonMid, GUILayout.Width(25.0f))) selectItemGroup.CenterRange(item);
            EditorGUILayout.LabelField(item.range.x.ToString("F2") + " - " + item.range.y.ToString("F2"), GUILayout.Width(90.0f));

            GUI.color = Color.white;
        }

        public void DrawVector2(SerializedProperty property, bool useLimit, string xLabel, string yLabel, GUIContent guiContent = null, float xLabelWidth = 35, float yLabelWidth = 35)
        {
            GUI.changed = false;

            float labelWidth = EditorGUIUtility.labelWidth;

            EditorGUILayout.BeginHorizontal();
            if (guiContent != null) EditorGUILayout.PrefixLabel(guiContent);
            else EditorGUILayout.PrefixLabel(property.name);
            
            EditorGUIUtility.labelWidth = xLabelWidth;
            float x = EditorGUILayout.FloatField(xLabel, property.vector2Value.x);

            EditorGUIUtility.labelWidth = yLabelWidth;
            float y = EditorGUILayout.FloatField(yLabel, property.vector2Value.y);
            
            property.vector2Value = new Vector2(x, y);

            EditorGUILayout.EndHorizontal();

            EditorGUIUtility.labelWidth = labelWidth;

            if (GUI.changed)
            {
                if (useLimit)
                {
                    if (property.vector2Value.x < 0.001f) property.vector2Value = new Vector2(0.001f, property.vector2Value.y);
                    if (property.vector2Value.x > property.vector2Value.y) property.vector2Value = new Vector2(property.vector2Value.x, property.vector2Value.x + 0.001f);
                }
                AutoGenerate();
            }
        }

        public void DrawMinMaxSlider(SerializedProperty property, float minValue, float maxValue, float limit, GUIContent guiContent, float width = -1)
        {
            GUI.changed = false;
            EditorGUILayout.BeginHorizontal();
            Vector2 v = property.vector2Value;
            
            if (width == -1)
            {
                EditorGUILayout.PrefixLabel(guiContent);
                EditorGUILayout.MinMaxSlider(ref v.x, ref v.y, minValue, maxValue);
                //EditorGUILayout.LabelField("");
                //rect = GUILayoutUtility.GetLastRect();
                //v = GUIW.MinMaxSlider(rect, v, minValue, maxValue, new Vector2(rect.width, 50));
            }
            else
            {
                EditorGUILayout.PrefixLabel(guiContent);
                EditorGUILayout.MinMaxSlider(ref v.x, ref v.y, minValue, maxValue);
            }
            property.vector2Value = v;
            EditorGUILayout.EndHorizontal();

            if (GUI.changed)
            {
                // if (property.vector2Value.x  < limit) property.vector2Value = new Vector2(limit, property.vector2Value.y);
                if (property.vector2Value.x + limit > property.vector2Value.y) property.vector2Value = new Vector2(property.vector2Value.x, property.vector2Value.x + limit);

                AutoGenerate();
            }

        }

        public void DrawSlider(SerializedProperty property, float leftValue, float rightValue, GUIContent guiContent = null, float width = -1)
        {
            GUI.changed = false;
            if (width == -1)
            {
                if (guiContent == null) EditorGUILayout.Slider(property, leftValue, rightValue);
                else EditorGUILayout.Slider(property, leftValue, rightValue, guiContent);
            }
            else
            {
                if (guiContent == null) EditorGUILayout.Slider(property, leftValue, rightValue, GUILayout.Width(width));
                else EditorGUILayout.Slider(property, leftValue, rightValue, guiContent, GUILayout.Width(width));
            }
            if (GUI.changed) AutoGenerate();
        }

        public void DrawIntSlider(SerializedProperty property, int leftValue, int rightValue, GUIContent guiContent = null, float width = -1)
        {
            GUI.changed = false;
            if (width == -1)
            {
                if (guiContent == null) EditorGUILayout.IntSlider(property, leftValue, rightValue);
                else EditorGUILayout.IntSlider(property, leftValue, rightValue, guiContent);
            }
            else
            {
                if (guiContent == null) EditorGUILayout.IntSlider(property, leftValue, rightValue, GUILayout.Width(width));
                else EditorGUILayout.IntSlider(property, leftValue, rightValue, guiContent, GUILayout.Width(width));
            }
            if (GUI.changed) AutoGenerate();
        }
    }
}