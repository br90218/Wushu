using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;


// TODO: optimization convert all lists to arrays, so if that loops faster
// TODO: Use dice shader normal calculation for multi terrain angle

// Select the transform node and on the transform go put an editor script for the node
// The node itself is not selectable and switches to transform node

// 2 transform scales
// 1 on the nodes, 1 on the area (terrain) itself

namespace TerrainComposer2
{
    public class TC_TerrainLayer : TC_ItemBehaviour
    {
        // [NonSerialized] static public TC_TerrainLayer current;
        [NonSerialized] public TC_LayerGroup[] layerGroups = new TC_LayerGroup[6];
        // [NonSerialized]
        public List<TC_SelectItem> objectSelectItems;
        // [NonSerialized]
        public List<TC_SelectItem> treeSelectItems;
        
        public int treeResolutionPM = 128;
        public int objectResolutionPM = 128;
        public Vector2 objectAreaSize;
        public Transform objectTransform; // stream around camera

        public int colormapResolution = 128;
        public int meshResolution = 2048;

        public TC_LayerGroup Clone()
        {
            GameObject obj = (GameObject)Instantiate(gameObject, t.position, t.rotation);
            obj.transform.parent = TC_Area2D.current.transform;
            return obj.GetComponent<TC_LayerGroup>();
        }

        public void LinkClone(TC_TerrainLayer terrainLayerS)
        {
            TC_LayerGroup layerGroup;

            for (int i = 0; i < layerGroups.Length; i++)
            {
                layerGroup = layerGroups[i];

                if (layerGroup != null)
                {
                    layerGroup.LinkClone(terrainLayerS.layerGroups[i]);
                }
            }
        }

        public void New(bool undo)
        {
            //#if UNITY_EDITOR
            //if (undo) UnityEditor.Undo.RecordObject(gameObject, "Undo New TerrainComposer Project");
            //#endif
            bool autoGenerateOld = TC_Generate.instance.autoGenerate;
            TC_Generate.instance.autoGenerate = false;

            Reset();
            CreateLayerGroups();

            TC_Generate.instance.autoGenerate = autoGenerateOld;
        }

        public void CreateLayerGroups()
        {
            TC_ItemBehaviour item;

            for (int i = 0; i < 6; i++)
            {
                item = (TC_ItemBehaviour)Add<TC_LayerGroup>(TC.outputNames[i] + " Output", false, false, false, i);
                item.visible = false;
            }
        }

        public void Reset()
        {
            TC_LayerGroup layerGroup;
        
            for (int i = 0; i < layerGroups.Length; i++)
            {
                layerGroup = layerGroups[i];
                if (layerGroup != null) DestroyImmediate(layerGroup.gameObject);
            }
        }

        public override void GetItems(bool refresh)
        {
            // current = this;
            if (TC_Settings.instance == null) return;
            TC_Settings.instance.HasMasterTerrain();

            for (int i = 0; i < layerGroups.Length; i++) GetItem(i);
        }

        public void GetItem(int outputId) 
        {
            // Debug.Log("Terrain Layer GetItem " + TC.outputNames[outputId]);
            active = visible;

            if (t.childCount < 6)
            {
                active = false;
                return;
            }

            if (outputId == TC.objectOutput)
            {
                if (objectSelectItems == null) objectSelectItems = new List<TC_SelectItem>();
                else objectSelectItems.Clear();
            }
            else if (outputId == TC.treeOutput)
            {
                if (treeSelectItems == null) treeSelectItems = new List<TC_SelectItem>();
                else treeSelectItems.Clear();
            }
            
            Transform child = t.GetChild(outputId); 
            TC_LayerGroup layerGroup = child.GetComponent<TC_LayerGroup>();
            if (layerGroup != null)
            {
                layerGroup.level = 0;
                layerGroup.outputId = outputId;
                layerGroup.listIndex = outputId;
                layerGroup.parentItem = this;
                
                layerGroup.GetItems(true);
                layerGroups[outputId] = layerGroup;
            }
        }
    }
}