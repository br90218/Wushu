using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace TerrainComposer2
{
    public class TC_LayerGroupResult : TC_GroupBehaviour
    {
        // [NonSerialized]
        public List<LayerGroupItem> itemList = new List<LayerGroupItem>();
        
        public ComputeBuffer ComputeSingle(bool first = false)
        {
            TC_Compute compute = TC_Compute.instance;

            ComputeBuffer totalBuffer = null;
            ComputeBuffer layerBuffer = null;
            ComputeBuffer layerMaskBuffer = null;

            RenderTexture[] rtsPreview = null;
            RenderTexture rtRightPreview = null;
            RenderTexture rtLeftPreview = null;

            if (outputId != TC.heightOutput) rtsPreview = new RenderTexture[2];

            SetPreviewTextureBefore();

            int even = 0;
            
            for (int i = 0; i < itemList.Count; i++)
            {
                TC_Layer layer = itemList[i].layer;
                
                if (layer != null)
                {
                    if (!layer.active) { TC_Reporter.Log("Inactive layer " + i); continue; }
                    
                    if (totalBuffer == null)
                    {
                        if (outputId == TC.heightOutput) layer.ComputeHeight(ref totalBuffer, ref layerMaskBuffer, i == firstActive);
                        else
                        {
                            layer.ComputeItem(ref totalBuffer, ref layerMaskBuffer, i == firstActive);
                            if (totalBuffer != null) rtLeftPreview = layer.rtDisplay;
                        }

                        TC_Area2D.current.layerGroupBuffer = totalBuffer; // Portal

                        compute.DisposeBuffer(ref layerMaskBuffer);
                    }
                    else
                    {
                        if (outputId == TC.heightOutput) layer.ComputeHeight(ref layerBuffer, ref layerMaskBuffer);
                        else layer.ComputeItem(ref layerBuffer, ref layerMaskBuffer);

                        if (layerBuffer != null)
                        {
                            if (outputId == TC.heightOutput) compute.RunComputeMethod(this, layer, totalBuffer, ref layerBuffer, totalActive, i == lastActive ? rtPreview : null, layerMaskBuffer);
                            else
                            {
                                rtRightPreview = layer.rtDisplay;
                                compute.RunComputeObjectMethod(this, layer, totalBuffer, ref layerBuffer, layerMaskBuffer, rtPreview, ref rtsPreview[even++ % 2], ref rtLeftPreview, rtRightPreview);
                            }
                            TC_Area2D.current.layerGroupBuffer = totalBuffer;
                        }
                        compute.DisposeBuffer(ref layerMaskBuffer);
                    }
                }
                else
                {
                    TC_LayerGroup layerGroup = itemList[i].layerGroup;
                    if (layerGroup == null) continue;
                    if (!layerGroup.active) continue;

                    if (totalBuffer == null)
                    {
                        layerMaskBuffer = layerGroup.ComputeSingle(ref totalBuffer, i == firstActive);
                        if (totalBuffer != null) rtLeftPreview = layerGroup.rtDisplay;
                        
                        TC_Area2D.current.layerGroupBuffer = totalBuffer;

                        compute.DisposeBuffer(ref layerMaskBuffer);
                    }
                    else
                    {
                        layerMaskBuffer = layerGroup.ComputeSingle(ref layerBuffer);

                        if (layerBuffer != null)
                        {
                            if (outputId == TC.heightOutput) compute.RunComputeMethod(this, layerGroup, totalBuffer, ref layerBuffer, totalActive, i == lastActive ? rtPreview : null, layerMaskBuffer);
                            else
                            {
                                rtRightPreview = layerGroup.rtDisplay;
                                compute.RunComputeObjectMethod(this, layerGroup, totalBuffer, ref layerBuffer, layerMaskBuffer, rtPreview, ref rtsPreview[even++ % 2], ref rtLeftPreview, rtRightPreview);
                            }
                            TC_Area2D.current.layerGroupBuffer = totalBuffer;
                        }
                        compute.DisposeBuffer(ref layerMaskBuffer);
                    }
                }
            }

            SetPreviewTextureAfter();
            
            if (outputId != TC.heightOutput) TC_Compute.DisposeRenderTextures(ref rtsPreview);
            compute.DisposeBuffer(ref layerMaskBuffer);
            
            if (totalBuffer == null) TC_Reporter.Log("Layer buffer null");
            return totalBuffer;
        }

        public bool ComputeMulti(ref RenderTexture[] renderTextures, bool doNormalize, bool first = false)
        {
            TC_Compute compute = TC_Compute.instance;
            RenderTexture[] rtsLayer = null;
            RenderTexture rtRightPreview = null;
            RenderTexture rtLeftPreview = null;

            RenderTexture[] rtsPreview = null;
            // RenderTexture rtPreview2 = null;
            ComputeBuffer layerMaskBuffer = null;
            TC_LayerGroup layerGroup;
            TC_Layer layer;
            bool firstCompute = false;
            bool lastCompute = false;

            int even = 0;
            
            rtsPreview = new RenderTexture[2];
            
            SetPreviewTextureBefore();

            for (int i = 0; i < itemList.Count; i++)
            {
                layer = itemList[i].layer;

                if (layer != null)
                {
                    if (!layer.active) continue;
                    // InitPreview(ref layer.previewRenderTex);

                    if (!firstCompute)
                    {
                        firstCompute = layer.ComputeMulti(ref renderTextures, ref layerMaskBuffer, i == firstActive);
                        
                        if (firstCompute)
                        {
                            rtLeftPreview = layer.rtDisplay;
                            TC_Reporter.Log("firt compute " + layer.maskNodeGroup.totalActive);
                            compute.DisposeBuffer(ref layerMaskBuffer);
                        }
                    }
                    else
                    {
                        lastCompute = layer.ComputeMulti(ref rtsLayer, ref layerMaskBuffer);

                        if (lastCompute)
                        {
                            TC_Reporter.Log("Run layer method multi");
                            rtRightPreview = (layer.method == Method.Lerp) ? layer.selectNodeGroup.rtColorPreview : layer.rtDisplay;
                            // Debug.Log(rtRight.name+ " "+ (layer.maskNodeGroup.activeTotal == 0 || layer.method == Method.Lerp));
                            
                            if (outputId == TC.colorOutput) compute.RunComputeColorMethod(layer, layer.method, ref renderTextures[0], ref rtsLayer[0], layerMaskBuffer, rtPreview, ref rtsPreview[even++ % 2], ref rtLeftPreview, rtRightPreview);
                            else compute.RunComputeMultiMethod(layer, layer.method, i == lastActive && doNormalize, ref renderTextures, ref rtsLayer, layerMaskBuffer, rtPreview, ref rtsPreview[even++ % 2], ref rtLeftPreview, rtRightPreview);
                            
                            compute.DisposeBuffer(ref layerMaskBuffer);
                        }
                    }
                }
                else 
                {
                    layerGroup = itemList[i].layerGroup;
                    if (!layerGroup.active) continue;

                    if (!firstCompute)
                    {
                        firstCompute = layerGroup.ComputeMulti(ref renderTextures, ref layerMaskBuffer, i == firstActive);
                        if (firstCompute)
                        {
                            rtLeftPreview = layerGroup.rtDisplay;
                            compute.DisposeBuffer(ref layerMaskBuffer);
                            TC_Reporter.Log("LayerGroup did first compute");
                        }
                    }
                    else
                    {
                        lastCompute = layerGroup.ComputeMulti(ref rtsLayer, ref layerMaskBuffer);
                        if (lastCompute)
                        {
                            // if (layerGroup.groupResult.activeTotal == 1) rtRight = layerGroup.rtDisplay; else rtRight = layerGroup.rtPreview;
                            rtRightPreview = (layerGroup.method == Method.Lerp) ? layerGroup.groupResult.rtDisplay : layerGroup.rtDisplay;

                            if (outputId == TC.colorOutput) compute.RunComputeColorMethod(layerGroup, layerGroup.method, ref renderTextures[0], ref rtsLayer[0], layerMaskBuffer, rtPreview, ref rtsPreview[even++ % 2], ref rtLeftPreview, rtRightPreview);
                            else compute.RunComputeMultiMethod(layerGroup, layerGroup.method, i == lastActive && doNormalize, ref renderTextures, ref rtsLayer, layerMaskBuffer, rtPreview, ref rtsPreview[even++ % 2], ref rtLeftPreview, rtRightPreview);
                            
                            compute.DisposeBuffer(ref layerMaskBuffer);
                        }
                    }
                }
            }

            SetPreviewTextureAfter();
            
            if (layerMaskBuffer != null) { compute.DisposeBuffer(ref layerMaskBuffer); TC_Reporter.Log("Dispose layerMaskBuffer"); }
            
            TC_Compute.DisposeRenderTextures(ref rtsPreview);
            TC_Compute.DisposeRenderTextures(ref rtsLayer);

            return firstCompute;
        }
        
        public void SetPreviewTextureBefore()
        {

            // Debug.Log("no " + maskNodeGroup.itemList.Count + " " + itemList.Count);
            if (totalActive == 0)
            {
                active = false;
                rtDisplay = null;

                TC_Compute.DisposeRenderTexture(ref rtPreview);
            }
            else if (totalActive != 1)
            { 
                TC_Compute.InitPreviewRenderTexture(ref rtPreview, "rtGroupResult");
                rtDisplay = rtPreview;
            }
        }

        public void SetPreviewTextureAfter()
        {
            if (totalActive == 1)
            {
                TC_Compute.DisposeRenderTexture(ref rtPreview);

                TC_Layer layer = itemList[firstActive].layer;
                if (layer != null) { rtDisplay = layer.rtDisplay; }
                else { rtDisplay = itemList[firstActive].layerGroup.rtDisplay; }
            }
        }

        public void LinkClone(TC_LayerGroupResult resultLayerGroup)
        {
            preview = resultLayerGroup.preview;
         
            for (int i = 0; i < itemList.Count; i++)
            {
                if (itemList[i].layerGroup != null) itemList[i].layerGroup.LinkClone(resultLayerGroup.itemList[i].layerGroup);
                else itemList[i].layer.LinkClone(resultLayerGroup.itemList[i].layer);
            }
        }

        public override void SetLockChildrenPosition(bool lockPos)
        {
            lockPosParent = lockPos;
            for (int i = 0; i < itemList.Count; i++)
            {
                if (itemList[i].layer != null) itemList[i].layer.SetLockChildrenPosition(lockPosParent || lockPosChildren);
                else if (itemList[i].layerGroup != null) itemList[i].layerGroup.SetLockChildrenPosition(lockPosParent || lockPosChildren);
            }
        }

        public override void UpdateTransforms()
        {
            // ct.CopySpecial(this);

            for (int i = 0; i < itemList.Count; i++)
            {
                if (itemList[i].layer != null) itemList[i].layer.UpdateTransforms();
                else if (itemList[i].layerGroup != null) itemList[i].layerGroup.UpdateTransforms();
            }
        }
        
        public override void ChangeYPosition(float y)
        {
            for (int i = 0; i < itemList.Count; i++)
            {
                if (itemList[i].layer != null) itemList[i].layer.ChangeYPosition(y);
                else if (itemList[i].layerGroup != null) itemList[i].layerGroup.ChangeYPosition(y);
            }
        }

        public override void SetFirstLoad(bool active)
        {
            base.SetFirstLoad(active);

            for (int i = 0; i < itemList.Count; i++)
            {
                if (itemList[i].layer != null) itemList[i].layer.SetFirstLoad(active);
                else if (itemList[i].layerGroup != null) itemList[i].layerGroup.SetFirstLoad(active);
            }
        }

        public override void GetItems(bool refresh)
        {
            active = visible;

            itemList.Clear();

            firstActive = lastActive = -1;
            totalActive = 0; 

            bool newBounds = true;
            int listIndex = 0;

            // Debug.Log(name + " GetItems");

            for (int i = t.childCount - 1; i >= 0; i--)
            {
                Transform child = transform.GetChild(i);
                TC_Layer layer = child.GetComponent<TC_Layer>();

                if (layer != null)
                {
                    layer.SetParameters(this, listIndex);

                    layer.GetItems(refresh);
                    if (layer.active)
                    {
                        ++totalActive;
                        lastActive = listIndex;
                        if (firstActive == -1) firstActive = lastActive;
                    }
                    itemList.Add(new LayerGroupItem(null, layer));
                    ++listIndex;

                    if (newBounds) { bounds = layer.bounds; newBounds = false; }
                    else bounds.Encapsulate(layer.bounds);
                }
                else
                {
                    TC_LayerGroup layerGroup = child.GetComponent<TC_LayerGroup>();

                    if (layerGroup == null) TC.MoveToDustbin(child);
                    else
                    {
                        layerGroup.SetParameters(this, listIndex);
                        layerGroup.GetItems(refresh);

                        if (layerGroup.active)
                        {
                            ++totalActive;
                            lastActive = listIndex;
                            if (firstActive == -1) firstActive = lastActive;
                        }

                        if (layerGroup.groupResult == null) TC.MoveToDustbin(child);
                        else
                        {
                            itemList.Add(new LayerGroupItem(layerGroup, null));
                            listIndex++;
                        }
                        if (newBounds) { bounds = layerGroup.bounds; newBounds = false; }
                        else bounds.Encapsulate(layerGroup.bounds);
                    }
                }
            }

            TC_Reporter.Log(TC.outputNames[outputId] + " Level " + level + " activeTotal " + totalActive);

            if (!active) totalActive = 0;
            else if (totalActive == 0) active = false; 
        }
    }
}