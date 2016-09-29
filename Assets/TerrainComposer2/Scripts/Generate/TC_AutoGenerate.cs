using UnityEngine;
using System.Collections;

namespace TerrainComposer2
{
    [ExecuteInEditMode]
    public class TC_AutoGenerate : MonoBehaviour
    {

        [HideInInspector] public CachedTransform cT = new CachedTransform();
        Transform t;
        public bool repeat;

        void Start()
        {
            t = transform;
            cT.Copy(t);
        }

        void Update()
        {
            MyUpdate();
        }

        void MyUpdate()
        {
            if (repeat) TC.AutoGenerate();

            if (cT.hasChanged(t))
            {
                // Debug.Log("Auto generate");
                cT.Copy(t);
                TC.AutoGenerate();
            }
        }

        #if UNITY_EDITOR
        void OnEnable()
        {
            TC.AutoGenerate();
            UnityEditor.EditorApplication.update += MyUpdate;
        }
        
         
        void OnDisable()
        {
            UnityEditor.EditorApplication.update -= MyUpdate;
        }

        void OnDestroy()
        {
            UnityEditor.EditorApplication.update -= MyUpdate;
        }
        #endif
    }
}