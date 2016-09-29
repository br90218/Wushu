using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace TerrainComposer2
{
    public class TC_NodeGroup : TC_GroupBehaviour
    {
        [NonSerialized] public List<NodeGroupItem> itemList = new List<NodeGroupItem>();
        public NodeGroupType type;
        public RenderTexture rtColorPreview;
        public bool useConstant;
        
        public override void Awake()
        {
            rtColorPreview = null;
            base.Awake();
        }

        public override void OnDestroy()
        {
            TC_Compute.DisposeRenderTexture(ref rtColorPreview);

            base.OnDestroy();
        }

        public void LinkClone(TC_NodeGroup nodeGroupS)
        {
            preview = nodeGroupS.preview;

            for (int i = 0; i < itemList.Count; i++)
            {
                if (itemList[i].node != null)
                {
                    itemList[i].node.preview = nodeGroupS.itemList[i].node.preview;
                    itemList[i].node.Init();
                }
            }
        }

        public override void SetLockChildrenPosition(bool lockPos)
        {
            lockPosParent = lockPos;
            
            for (int i = 0; i < itemList.Count; i++)
            {
                itemList[i].node.lockPosParent = lockPosParent || lockPosChildren;
            }
        }

        public override void UpdateTransforms()
        {
            // ct.CopySpecial(this);

            for (int i = 0; i < itemList.Count; i++) itemList[i].node.ct.CopySpecial(itemList[i].node);
        }

        public ComputeBuffer ComputeValue()
        {
            TC_Compute compute = TC_Compute.instance;
            if (compute == null) Debug.Log("Compute is null");

            ComputeBuffer nodeBuffer = null;
            ComputeBuffer totalBuffer = null;
            
            bool inputCurrent;

            if (totalActive > 1) InitPreviewRenderTexture(true, name);

            int length = useConstant ? 1 : itemList.Count;

            for (int i = 0; i < length; i++)
            {
                NodeGroupItem item = itemList[i];

                if (item.node != null)
                {
                    TC_Node node = item.node;
                    if (!node.active) continue;

                    if (node.clamp)
                    {
                        // if (node.OutOfBounds()) continue;
                    }

                    inputCurrent = (node.inputKind == InputKind.Current);
                    node.InitPreviewRenderTexture(true, node.name);

                    if (totalBuffer == null && !inputCurrent)
                    {
                        totalBuffer = compute.RunNodeCompute(this, node);
                    }
                    else
                    {
                        if (!inputCurrent) nodeBuffer = compute.RunNodeCompute(this, node, totalBuffer, false);
                        else
                        {
                            for (int j = 0; j < node.iterations; j++) totalBuffer = compute.RunNodeCompute(this, node, totalBuffer, true);
                            // if (preview && totalBuffer != null) { compute.DisposeBuffer(ref totalBuffer); }
                        }

                        // if (preview && nodeBuffer != null) { compute.DisposeBuffer(ref nodeBuffer); }
                    }
                    if (totalBuffer != null && nodeBuffer != null && !inputCurrent) compute.RunComputeMethod(this, node, totalBuffer, ref nodeBuffer, itemList.Count, i == lastActive ? rtPreview : null);
                }
            }

            if (totalActive == 1)
            {
                TC_Compute.DisposeRenderTexture(ref rtPreview);
                rtDisplay = itemList[firstActive].node.rtDisplay;
            }

            return totalBuffer;
        }

        public override void ChangeYPosition(float y) { for (int i = 0; i < itemList.Count; i++) itemList[i].node.ChangeYPosition(y); }

        public override void SetFirstLoad(bool active)
        {
            base.SetFirstLoad(active);
            for (int i = 0; i < itemList.Count; i++) itemList[i].node.SetFirstLoad(active);
        }

        public override void GetItems(bool refresh)
        {
            int childCount = transform.childCount;
            // Init();
            itemList.Clear();

            active = visible;
            
            firstActive = lastActive = -1;
            totalActive = 0;

            bool newBounds = true;
            int listIndex = 0;

            for (int i = childCount - 1; i >= 0; i--)
            {
                Transform child = transform.GetChild(i);

                // TC_NodeGroup nodeGroup = child.GetComponent<TC_NodeGroup>();
                //if (nodeGroup != null)
                //{
                //    nodeGroup.outputId = outputId;
                //    nodeGroup.active = nodeGroup.go.activeSelf;
                //    itemList.Add(new nodeGroupItem(nodeGroup));
                //    nodeGroup.GetItems();
                //}
                //else
                //{
                TC_Node node = child.GetComponent<TC_Node>();

                if (node == null) TC.MoveToDustbin(child);
                else
                {
                    node.active = node.visible;
                    node.SetParameters(this, listIndex);
                    node.nodeType = type;

                    node.Init();

                    if (node.active)
                    {
                        if (node.clamp) node.CalcBounds();
                        if (newBounds) { bounds = node.bounds; newBounds = false; }
                        else bounds.Encapsulate(node.bounds);

                        lastActive = listIndex;
                        if (firstActive == -1) firstActive = lastActive;
                        ++totalActive;
                    }

                    if (i == childCount - 1) // TODO: Consider hide and do in calculation
                    {
                        if (node.method != Method.Add && node.method != Method.Subtract) node.method = Method.Add;
                    }

                    itemList.Add(new NodeGroupItem(node));
                    listIndex++;
                }
            }

            if (itemList.Count == 1)
            {
                if (itemList[0].node.active) active = visible = true;
            }

            if (!active) totalActive = 0; 
            if (totalActive == 0) active = false;
        }
    }
    
    [Serializable]
    public class NodeGroupItem
    {
        public TC_NodeGroup nodeGroup;
        public TC_Node node;

        public NodeGroupItem(TC_NodeGroup nodeGroup)
        {
            this.nodeGroup = nodeGroup;
        }

        public NodeGroupItem(TC_Node node)
        {
            this.node = node;
        }
    }
}
