using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace TerrainComposer2
{
    public class TC_LayerGroup : TC_ItemBehaviour
    {
        [NonSerialized] public TC_NodeGroup maskNodeGroup;
        [NonSerialized] public TC_LayerGroupResult groupResult;
        
        public bool doNormalize;
        public float placeLimit = 0.5f;
        
        public ComputeBuffer ComputeSingle(ref ComputeBuffer totalBuffer, bool first = false)
        {
            if (!groupResult.active) return null;

            TC_Compute compute = TC_Compute.instance;
            
            totalBuffer = groupResult.ComputeSingle(first);

            // Debug.Log("layerMaskBuffer " + layerMaskBuffer == null);
            ComputeBuffer maskBuffer = null;
            if (maskNodeGroup.active) maskBuffer = maskNodeGroup.ComputeValue();

            if (maskBuffer != null)
            {
                TC_Compute.InitPreviewRenderTexture(ref rtPreview, "rtPreview_LayerGroup");

                if (method != Method.Lerp || first)
                {
                    if (outputId == TC.heightOutput) compute.RunComputeMethod(null, null, totalBuffer, ref maskBuffer, 0, rtPreview);
                    else compute.RunItemComputeMask(this, ref rtPreview, groupResult.rtDisplay, ref totalBuffer, ref maskBuffer);
                }

                rtDisplay = rtPreview;
            }
            else
            {
                if (outputId == TC.heightOutput || level == 0 || groupResult.totalActive == 1) rtDisplay = groupResult.rtDisplay;
                else rtDisplay = rtPreview;
            }

            if (totalBuffer == null) TC_Reporter.Log("Layer buffer null");

            return maskBuffer;
        }

        public bool ComputeMulti(ref RenderTexture[] renderTextures, ref ComputeBuffer maskBuffer, bool first = false)
        {
            TC_Compute compute = TC_Compute.instance;
            
            bool computed = groupResult.ComputeMulti(ref renderTextures, doNormalize, first);

            if (maskNodeGroup.active) maskBuffer = maskNodeGroup.ComputeValue();

            if (maskBuffer != null)
            {
                TC_Compute.InitPreviewRenderTexture(ref rtPreview, "rtPreview_LayerGroup_" + TC.outputNames[outputId]);
                if (method != Method.Lerp || first)
                {
                    if (outputId == TC.colorOutput) compute.RunComputeColorMethod(this, ref renderTextures[0], maskBuffer, groupResult.rtDisplay);
                    else compute.RunComputeMultiMethod(this, doNormalize, ref renderTextures, maskBuffer, groupResult.rtDisplay);
                }
                rtDisplay = rtPreview;
            }
            else rtDisplay = groupResult.rtDisplay;
            
            return computed;
        }
        
        public void LinkClone(TC_LayerGroup layerGroupS)
        {
            preview = layerGroupS.preview;
            maskNodeGroup.LinkClone(layerGroupS.maskNodeGroup);
            groupResult.LinkClone(layerGroupS.groupResult);
        }

        public override void SetLockChildrenPosition(bool lockPos)
        {
            lockPosParent = lockPos;
            groupResult.SetLockChildrenPosition(lockPosParent || lockPosChildren);
            maskNodeGroup.SetLockChildrenPosition(lockPosParent || lockPosChildren);
        }

        public override void UpdateTransforms()
        {
            ct.Copy(this);

            groupResult.UpdateTransforms();
        }

        public override void SetFirstLoad(bool active)
        {
            base.SetFirstLoad(active);
            maskNodeGroup.SetFirstLoad(active);
            groupResult.SetFirstLoad(active);
        }

        public override void GetItems(bool refresh)
        {
            bool newBounds = true;

            active = visible;
            
            maskNodeGroup = GetGroup<TC_NodeGroup>(0, refresh);

            if (maskNodeGroup == null) active = false;
            else 
            {
                maskNodeGroup.type = NodeGroupType.Mask;
                if (maskNodeGroup.active)
                {
                    if (newBounds) bounds = maskNodeGroup.bounds;
                    else bounds.Encapsulate(maskNodeGroup.bounds);
                }
            }

            if (t.childCount <= 1) active = false;
            else
            {
                Transform child = t.GetChild(1);
                groupResult = child.GetComponent<TC_LayerGroupResult>();

                if (groupResult == null)
                {
                    TC.MoveToDustbin(child);
                    active = false;
                }
                else
                {
                    groupResult.SetParameters(this, 1);
                    groupResult.GetItems(refresh);
                    if (!groupResult.active) active = false;
                }
            }
        }

        public override void ChangeYPosition(float y)
        {
            if (groupResult != null) groupResult.ChangeYPosition(y);
        }
    }
    
    [Serializable]
    public class LayerGroupItem
    {
        public TC_LayerGroup layerGroup;
        public TC_Layer layer;
        
        public LayerGroupItem(TC_LayerGroup layerGroup, TC_Layer layer)
        {
            this.layer = layer;
            this.layerGroup = layerGroup;
        }
    }
}