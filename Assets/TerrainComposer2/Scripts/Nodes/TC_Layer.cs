using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace TerrainComposer2
{
    public class TC_Layer : TC_ItemBehaviour
    {
        [NonSerialized] public TC_SelectItemGroup selectItemGroup;
        [NonSerialized] public TC_NodeGroup maskNodeGroup;
        [NonSerialized] public TC_NodeGroup selectNodeGroup;
        // public new TC_LayerGroupResult parentItem;

        public bool doNormalize;
        public float placeLimit = 0.5f;
        public float selectValue;
        public float maskValue;

        float splatTotal;
        float x, y;

        public void ComputeHeight(ref ComputeBuffer layerBuffer, ref ComputeBuffer maskBuffer, bool first = false)
        {
            TC_Compute compute = TC_Compute.instance;

            layerBuffer = selectNodeGroup.ComputeValue();

            if (layerBuffer != null)
            {
                if (maskNodeGroup.active) maskBuffer = maskNodeGroup.ComputeValue();

                if (maskBuffer != null)
                {
                    if (method != Method.Lerp || first)
                    {
                        InitPreviewRenderTexture(true, "rtPreview_Layer_" + TC.outputNames[outputId]);
                        compute.RunComputeMethod(null, null, layerBuffer, ref maskBuffer, 0, rtPreview);
                    }
                }
                else rtDisplay = selectNodeGroup.rtDisplay;
            }
            else TC_Reporter.Log("Layerbuffer " + listIndex + " = null, reporting from layer");
        }

        public bool ComputeMulti(ref RenderTexture[] renderTextures, ref ComputeBuffer maskBuffer, bool first = false)
        {
            TC_Compute compute = TC_Compute.instance;
            bool didCompute = false;

            ComputeBuffer layerBuffer = selectNodeGroup.ComputeValue();

            if (layerBuffer != null)
            {
                didCompute = true;

                TC_Compute.InitPreviewRenderTexture(ref rtPreview, "rtPreview_Layer");
                TC_Compute.InitRenderTextures(ref renderTextures, "rts" + TC.outputNames[outputId], outputId == TC.colorOutput ? 1 : 2);

                if (maskNodeGroup.active) maskBuffer = maskNodeGroup.ComputeValue();

                TC_Compute.InitPreviewRenderTexture(ref selectNodeGroup.rtColorPreview, "rtNodeGroupPreview_" + TC.outputNames[outputId]);

                if (outputId == TC.colorOutput) compute.RunColorCompute(selectNodeGroup, selectItemGroup, ref renderTextures[0], ref layerBuffer);
                else compute.RunSplatCompute(selectNodeGroup, selectItemGroup, ref renderTextures, ref layerBuffer); 

                compute.DisposeBuffer(ref layerBuffer);

                if (maskBuffer != null)
                {
                    TC_Reporter.Log("Run layer select * mask");
                    if (method != Method.Lerp || first)
                    {
                        if (outputId == TC.colorOutput) compute.RunComputeColorMethod(this, ref renderTextures[0], maskBuffer, rtPreview);
                        else compute.RunComputeMultiMethod(this, doNormalize, ref renderTextures, maskBuffer, rtPreview);
                    }
                    rtDisplay = rtPreview;
                }
                else
                {
                    TC_Reporter.Log("No mask buffer assign colorPreviewTex to layer");
                    rtDisplay = selectNodeGroup.rtColorPreview;
                }
            }

            return didCompute;
        }

        public bool ComputeItem(ref ComputeBuffer itemMapBuffer, ref ComputeBuffer maskBuffer, bool first = false)
        {
            TC_Compute compute = TC_Compute.instance;
            bool didCompute = false;

            ComputeBuffer selectBuffer = selectNodeGroup.ComputeValue();

            if (selectBuffer != null)
            {
                didCompute = true;

                TC_Compute.InitPreviewRenderTexture(ref rtPreview, "rtPreview_Layer_" + TC.outputNames[outputId]);
                rtDisplay = rtPreview;
                TC_Compute.InitPreviewRenderTexture(ref selectNodeGroup.rtColorPreview, "rtColorPreview");
                compute.RunItemCompute(this, ref itemMapBuffer, ref selectBuffer);
                compute.DisposeBuffer(ref selectBuffer);

                if (maskNodeGroup.active) maskBuffer = maskNodeGroup.ComputeValue();

                if (maskBuffer != null)
                {
                    TC_Reporter.Log("Run layer select * mask");
                    if (method != Method.Lerp || first)
                    {
                        compute.RunItemComputeMask(this, ref rtPreview, selectNodeGroup.rtColorPreview, ref itemMapBuffer, ref maskBuffer);
                    }
                }
            }

            return didCompute;
        }

        public void LinkClone(TC_Layer layerS)
        {
            preview = layerS.preview;
            maskNodeGroup.LinkClone(layerS.maskNodeGroup);
            selectNodeGroup.LinkClone(layerS.selectNodeGroup);
        }

        public override void GetItems(bool refresh)
        {
            active = visible;
            // Init();
            // InitPreview(ref rtPreview);
            bool newBounds = true;

            maskNodeGroup = GetGroup<TC_NodeGroup>(0, refresh);
            if (maskNodeGroup != null) 
            {
                maskNodeGroup.type = NodeGroupType.Mask;
                if (maskNodeGroup.totalActive > 0)
                {
                    bounds = maskNodeGroup.bounds;
                    newBounds = false;
                }
            }
            
            selectNodeGroup = GetGroup<TC_NodeGroup>(1, refresh);
            if (selectNodeGroup != null)
            {
                selectNodeGroup.type = NodeGroupType.Select;
                if (selectNodeGroup.totalActive == 0) { TC_Reporter.Log("SelectNodeGroup 0 active"); active = false; }
                else
                {
                    if (newBounds) bounds = selectNodeGroup.bounds;
                    else bounds.Encapsulate(selectNodeGroup.bounds);
                }
            }
            else active = false;
            
            if (outputId != TC.heightOutput)
            {
                selectItemGroup = GetGroup<TC_SelectItemGroup>(2, refresh);
                if (selectItemGroup != null)
                {
                    if (selectItemGroup.totalActive == 0) { TC_Reporter.Log("itemGroup 0 active"); active = false; }
                    else if (selectItemGroup.itemList.Count <= 1)
                    {
                        // TODO: Make better solution for this
                        selectNodeGroup.useConstant = true;
                        if (selectNodeGroup.itemList.Count > 0)
                        {
                            selectNodeGroup.itemList[0].node.visible = true;
                            active = visible;
                            GetGroup<TC_NodeGroup>(1, true);
                        }
                    }
                    else selectNodeGroup.useConstant = false;
                }
                else active = false;
            }
        }

        public override void SetLockChildrenPosition(bool lockPos)
        {
            // Debug.Log("lockPos " + lockPos);
            lockPosParent = lockPos;
            // Debug.Log("lockPosParent " + lockPosParent);
            if (maskNodeGroup != null) maskNodeGroup.SetLockChildrenPosition(lockPosParent || lockPosChildren);
            if (selectNodeGroup != null) selectNodeGroup.SetLockChildrenPosition(lockPosParent || lockPosChildren);
        }

        public override void UpdateTransforms()
        {
            // ct.CopySpecial(this);

            maskNodeGroup.UpdateTransforms();
            selectNodeGroup.UpdateTransforms();
        }

        public override void ChangeYPosition(float y) { selectNodeGroup.ChangeYPosition(y); }

        public override void SetFirstLoad(bool active)
        {
            base.SetFirstLoad(active);
            maskNodeGroup.SetFirstLoad(active);
            selectNodeGroup.SetFirstLoad(active);
            selectItemGroup.SetFirstLoad(active);
        }
    }
}