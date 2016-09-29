using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;

namespace TerrainComposer2
{
    static public class TC_NodeGroupGUI
    {
        static public int Draw(TC_NodeGroup nodeGroup, ref Vector2 pos, Color colGroupNode, Color colNode, Color colBracket, float activeMulti, bool nodeFoldout, bool drawMethod, bool colorPreviewTex, bool hideSelectNodes)
        {
            if (nodeGroup == null) return 0;
            // bool select = false;
            // Draw total node

            Rect dropDownRect;
            float activeMultiOld = activeMulti;
            activeMulti *= nodeGroup.active ? 1 : 0.75f;
            
            bool isCulled = false;
            TC_GlobalSettings g = TC_Settings.instance.global;

            // if ((nodeGroup.foldout == 1 && nodeGroup.itemList.Count == 1) || nodeGroup.itemList.Count == 0) nodeGroup.foldout = 0;
            
            // Closing Bracket
            TD.DrawBracket(ref pos, nodeFoldout, true, colBracket * activeMultiOld, ref nodeGroup.foldout, true, nodeGroup.itemList.Count > 0);
            
            if (nodeGroup.foldout > 0)
            {
                if ((nodeGroup.itemList.Count != 1 || nodeGroup.foldout != 1) && nodeGroup.itemList.Count != 0 && !hideSelectNodes) pos.x -= TD.texCardBody.width;
                
                if (nodeGroup.itemList.Count > 1 && !hideSelectNodes)
                {
                    dropDownRect = TD.DrawNode(nodeGroup, pos, colGroupNode, Color.white, ref isCulled, activeMulti, nodeFoldout, drawMethod, false);
                    if (nodeGroup.foldout == 2) pos.x -= TD.texCardBody.width + g.nodeHSpace;
                    DropDownMenu(dropDownRect, nodeGroup);
                }

                //if (colorPreviewTex)
                //{
                //    startOffset.x -= TD.nodeWidth;
                //    TD.DrawNode(nodeGroup, drawMethod, ref startOffset, true, color, click, Color.white, ref isCulled);
                //    startOffset.x -= TD.nodeWidth;
                //}

                // Draw ()
                //if (nodeGroup.itemList.Count > 1)
                //{
                //    startOffset.x += 10;

                //    Draw ItemList Count
                //    if (!nodeGroup.foldout)
                //    {
                //        TD.DrawRect(new Rect(startOffset.x - 9, (startOffset.y + TD.nodeHeight / 2) - 5, 18, 10), TC_Settings.instance.global.colTextBackGround);
                //        TD.DrawText(new Vector2(startOffset.x + 2, startOffset.y + TD.nodeHeight / 2), nodeGroup.itemList.Count.ToString(), 8, FontStyle.Bold, Color.white, HorTextAlign.Center, VerTextAlign.Center);
                //    }
                //    startOffset.x -= TD.nodeWidthSpace;
                //}
                
                if (nodeGroup.foldout == 2)
                {
                    if (nodeFoldout && !hideSelectNodes)
                    {
                        float startOffsetXOld = pos.x;
                        pos.x -= TD.nodeWidthHSpace * (nodeGroup.itemList.Count - 1);
                        for (int i = 1; i < nodeGroup.itemList.Count; ++i)
                        {
                            TC_Node node = nodeGroup.itemList[i].node;
                            if (node != null)
                            {
                                if (node.inputKind != InputKind.Current && node.inputKind != InputKind.Portal)
                                {
                                    TD.DrawMethod(node, pos + new Vector2(TD.texCardBody.width - 18, 187), false, colNode, (node.active ? 1 : 0.5f) * activeMulti);
                                }
                                pos.x += TD.nodeWidthHSpace;
                            }
                        }
                        if (nodeGroup.itemList.Count > 1) TD.DrawMethod(nodeGroup, pos + new Vector2(TD.texCardBody.width - 18, 187), true, colNode, activeMulti);
                        pos.x = startOffsetXOld;
                    }

                    if (!hideSelectNodes)
                    {
                        for (int i = nodeGroup.itemList.Count - 1; i >= 0; --i)
                        {
                            TC_Node node = nodeGroup.itemList[i].node;

                            if (node != null)
                            {
                                TC_NodeGUI.Draw(node, nodeFoldout, i == 0 ? false : true, pos, colNode, activeMulti);
                                if (i != 0) pos.x -= TD.nodeWidthHSpace;
                            }
                        }
                    }
                }
            }

            if (nodeFoldout)
            {
                int mouseClick = TD.DrawNodeCount(nodeGroup, ref pos, nodeGroup.itemList.Count, nodeFoldout, ref nodeGroup.foldout, (nodeGroup.foldout == 1 && nodeGroup.itemList.Count != 1 ? colGroupNode * 0.75f : colBracket) * activeMulti);
                if (mouseClick == 0 && nodeGroup.itemList.Count == 0) nodeGroup.Add<TC_Node>("", false, false, true);
            }

            // Opening Bracket
            TD.DrawBracket(ref pos, nodeFoldout, false, colBracket * activeMultiOld, ref nodeGroup.foldout, true, nodeGroup.itemList.Count > 0);

            return 0;
        }
        
        static public void DropDownMenu(Rect rect, TC_NodeGroup nodeGroup)
        {
            if (TD.ClickRect(rect) != 1) return;

            GenericMenu menu = new GenericMenu();

            // menu.AddItem(new GUIContent("Add Layer"), false, LeftClickMenu, "Add Layer");
            string instanceID = nodeGroup.GetInstanceID().ToString();

            menu.AddItem(new GUIContent("Clear Nodes"), false, LeftClickMenu, instanceID + ":Clear Nodes");
            
            menu.ShowAsContext();
        }

        static public void LeftClickMenu(object obj)
        {
            int instanceID;
            string command = TD.ObjectToCommandAndInstanceID(obj, out instanceID);

            TC_NodeGroup nodeGroup = EditorUtility.InstanceIDToObject(instanceID) as TC_NodeGroup;

            if (nodeGroup != null)
            {
                if (command == "Clear Nodes")
                {
                    nodeGroup.Clear(true);
                }
            }
        }

    }
}