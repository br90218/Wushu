using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace TerrainComposer2
{
    static public class TC
    {
        static public int refreshOutputReferences;
        static public bool refreshPreviewImages;
        static public bool repaintNodeWindow;
        static public List<MessageCommand> messages = new List<MessageCommand>();
        static public float autoGenerateCallTimeStart;

        public const int heightOutput = 0;
        public const int splatOutput = 1;
        public const int colorOutput = 2;
        public const int treeOutput = 3;
        public const int grassOutput = 4;
        public const int objectOutput = 5;

        public const int nodeLabelLength = 19;

        static readonly public string[] outputNames = new string[6] { "Height", "Splat", "Color", "Tree", "Grass", "Object" };

        static public string installPath;
        static public string fullInstallPath;

        static public void AutoGenerate(bool waitForNextFrame = true)
        {
            if (TC_Generate.instance != null) TC_Generate.instance.cmdGenerate = true;
        }
        
        static public void RefreshOutputReferences(int outputId, bool autoGenerate = false)
        {
            // Debug.Log("Call refresh " + outputId);
            refreshOutputReferences = outputId;
            
            if (autoGenerate) AutoGenerate();
        }

        static public void Swap<T>(ref T source, ref T dest)
        {
            T temp = source;
            source = dest;
            dest = temp;
        }

        static public void Swap<T>(List<T> source, int indexS, List<T> dest, int indexD)
        {
            if (indexD < 0 || indexD >= dest.Count) return;

            T temp = source[indexS];
            source[indexS] = dest[indexD];
            dest[indexD] = temp;
        }

        static public void Swap<T>(ref T[] source, ref T[] dest)
        {
            for (int i = 0; i < source.Length; i++) Swap(ref source[i], ref dest[i]);
        }

        static public void InitArray<T>(ref T[] array, int resolution)
        {
            if (array == null) array = new T[resolution];
            else if (array.Length != resolution) array = new T[resolution];
        }

        static public void InitArray<T>(ref T[,] array, int resolutionX, int resolutionY)
        {
            if (array == null) array = new T[resolutionX, resolutionY];
            else if (array.Length != resolutionX * resolutionY) array = new T[resolutionX, resolutionY];
        }

        static public void SetTextureReadWrite(Texture2D tex)
        {
            if (tex == null) return;

            #if UNITY_EDITOR
            string path = UnityEditor.AssetDatabase.GetAssetPath(tex);
            UnityEditor.TextureImporter textureImporter = (UnityEditor.TextureImporter)UnityEditor.AssetImporter.GetAtPath(path);

            if (textureImporter != null)
            {
                if (!textureImporter.isReadable)
                {
                    textureImporter.isReadable = true;
                    UnityEditor.AssetDatabase.ImportAsset(path, UnityEditor.ImportAssetOptions.ForceUpdate);
                }
            }
            #endif
        }

        static public string GetFileName(string path)
        {
            int index = path.LastIndexOf("/");
            if (index != -1)
            {
                string file = path.Substring(index + 1);
                index = file.LastIndexOf(".");

                if (index != -1) return file.Substring(0, index);
                return "";
            }
            return "";
        }

        static public string GetPath(string path)
        {
            int index = path.LastIndexOf("/");
            if (index != -1) return path.Substring(0, index);
            return "";
        }

        static public string GetAssetDatabasePath(string path)
        {
            return path.Replace(Application.dataPath, "Assets");
        }

        static public bool FileExists(string fullPath)
        {
            if (fullPath == null) { Debug.Log("Path doesn't exists."); return false; }
            System.IO.FileInfo file_info = new System.IO.FileInfo(fullPath);
            if (file_info.Exists) return true; else return false;
        }

        static public long GetFileLength(string fullPath)
        {
            #if !UNITY_WEBPLAYER
            System.IO.FileInfo fileInfo = new System.IO.FileInfo(fullPath);
            return fileInfo.Length;
            #else
            return 0;
            #endif
        }

        static public void MoveToDustbin(Transform t)
        {
            TC_Settings settings = TC_Settings.instance;

            if (settings.dustbinT == null) settings.CreateDustbin();

            t.parent = settings.dustbinT;

            AddMessage(t.name + " is not compatible with the hierarchy of TerrainComposer\n It is moved to the 'Dustbin' GameObject.", 0);
            AddMessage("If you pressed the delete key you can undo it with control-z", 3);
        }

        static public void AddMessage(string message, float delay = 0, float duration = 2)
        {
            for (int i = 0; i < messages.Count; i++) if (messages[i].message.Contains(message)) return;
            messages.Add(new MessageCommand(message, delay, duration));
        }

        public class MessageCommand
        {
            public string message;
            public float delay;
            public float duration;
            public float startTime;
            
            public MessageCommand(string message, float delay, float duration)
            {
                this.message = message;
                this.delay = delay;
                this.duration = duration;
                startTime = Time.realtimeSinceStartup;
            }
        }
    }
}