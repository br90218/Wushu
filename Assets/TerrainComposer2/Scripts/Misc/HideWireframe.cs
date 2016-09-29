using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class HideWireframe : MonoBehaviour {

#if UNITY_EDITOR
    // Use this for initialization
    void Start()
    {
        MeshRenderer mr = GetComponent<MeshRenderer>();
        UnityEditor.EditorUtility.SetSelectedWireframeHidden(mr, true);
    }
#endif   

}
