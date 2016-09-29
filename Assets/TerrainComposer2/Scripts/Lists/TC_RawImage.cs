using UnityEngine;
using System.Collections;
using System;
using System.IO;

namespace TerrainComposer2
{
    [ExecuteInEditMode]
    public class TC_RawImage : MonoBehaviour
    {
        public enum ByteOrder { Windows, Mac };

        public string fullPath;
        public string filename;
        public ByteOrder byteOrder;
        public bool stream;
        public int referenceCount;

        [NonSerialized] public System.IO.FileStream fileStream;
        public Int2 resolution;
        public bool squareResolution;
        public Texture2D tex;
        [NonSerialized] public byte[] bytes;

        public bool isDestroyed, callDestroy;

        void Awake()
        {
            if (isDestroyed)
            {
                LoadRawImage(fullPath);
                TC_Settings.instance.rawFiles.Add(this);
            }
            if (!callDestroy) { TC.refreshOutputReferences = 6; referenceCount = 0; }
            else callDestroy = false;
        }

        void OnDestroy()
        {
            if (!callDestroy) TC.refreshOutputReferences = 6;
        }

        void DestroyMe()
        {
            TC_Settings settings = TC_Settings.instance;
            if (settings == null) return;
            if (settings.rawFiles == null) return;

            int index = settings.rawFiles.IndexOf(this);
            if (index != -1) settings.rawFiles.RemoveAt(index);
            
            #if UNITY_EDITOR
                if (tex != null) DestroyImmediate(tex);
                UnityEditor.Undo.DestroyObjectImmediate(gameObject);
            #else
                if (tex != null) Destroy(tex);
                Destroy(gameObject);
            #endif
        }

        public void UnregisterReference()
        {
            --referenceCount;
            if (referenceCount > 0) return;
            isDestroyed = true;
            callDestroy = true;

            DestroyMe();
        }

        public bool GetFileResolution()
        {
            if (!TC.FileExists(fullPath)) return false;

            long length = TC.GetFileLength(fullPath);
            float res = Mathf.Sqrt(length / 2);

            if (res == Mathf.Floor(res)) squareResolution = true; else squareResolution = false;

            resolution = new Int2(res, res);

            return true;
        }

        //public bool LoadFile()
        //{
        //    if (bytes != null)
        //    {
        //        if (bytes.Length != 0) return true;
        //    }
        //    if (!GlobalManager.FileExists(fullPath)) return false;

        //    if (stream)
        //    {
        //        fileStream = new System.IO.FileStream(fullPath, System.IO.FileMode.Open);
        //    }
        //    else
        //    {
        //        bytes = System.IO.File.ReadAllBytes(fullPath);
        //    }

        //    return true;
        //}

        public void LoadRawImage(string fullPath)
        {
            this.fullPath = fullPath;
            
            if (tex != null) return;
            if (!TC.FileExists(fullPath)) return;

            TC_Reporter.Log("Load Raw file " + fullPath);

            GetFileResolution();

            // Debug.Log(bytes.Length);
            #if !UNITY_WEBPLAYER
                byte[] bytes = File.ReadAllBytes(fullPath);
            #else
                // TC.AddMessage("You are in Webplayer build mode, loading from disk is protected in this mode and stamp textures don't work.\nThis will be fixed.\n\nFor now another build mode in needed.", 0, 5); 
            #endif

            tex = new Texture2D(resolution.x, resolution.y, TextureFormat.R16, false);
            tex.name = "texRawHeightmap";
            tex.LoadRawTextureData(bytes);
            tex.Apply();
        }
    }
}
