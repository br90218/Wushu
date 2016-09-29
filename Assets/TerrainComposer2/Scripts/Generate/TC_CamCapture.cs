using UnityEngine;
using System.Collections;

namespace TerrainComposer2
{
    [ExecuteInEditMode]
    public class TC_CamCapture : MonoBehaviour
    {
        public Camera cam;
        public int collisionMask;
        Transform t;
        public CollisionDirection collisionDirection;

        void Start()
        {
            t = transform;
            cam = GetComponent<Camera>();
            cam.aspect = 1;
        }
        
        public void Capture(int collisionMask)
        {
            if (TC_Area2D.current.currentTerrainArea == null) return;
            this.collisionMask = collisionMask;
            // this.collisionDirection = collisionDirection;
            cam.cullingMask = collisionMask;

            SetCamera();

            cam.Render();
        }

        public void SetCamera()
        {
            if (t == null) Start();
           
            t.position = new Vector3(TC_Area2D.current.bounds.center.x, -1, TC_Area2D.current.bounds.center.z);
            t.rotation = Quaternion.Euler(-90, 0, 0);
            cam.orthographicSize = TC_Area2D.current.bounds.extents.x;

            cam.nearClipPlane = 0;
            cam.farClipPlane = TC_Area2D.current.currentTerrainArea.terrainSize.y + 1;

            // Vector3 size = area.currentTerrain.terrainData.size;
            // t.position = new Vector3(area.area.center.x, -1, area.area.center.y);
        }
    }
}