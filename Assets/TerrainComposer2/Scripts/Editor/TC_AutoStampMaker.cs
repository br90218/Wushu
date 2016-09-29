using UnityEngine;
using UnityEditor;
using System.Collections;
using System.IO;

namespace TerrainComposer2
{
    class TC_AutoStampMaker : AssetPostprocessor
    {
        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            string path;

            for (int i = 0; i < importedAssets.Length; i++)
            {
                path = importedAssets[i];

                if (path.Contains("RawFiles"))
                {
                    string extension = path.Substring(path.Length - 3);
                    if (extension.Contains("raw") || extension.Contains("Raw")) ConvertToImage(path);
                }
            }
        }

        static void ConvertToImage(string path)
        {
            byte[] newBytes;

            string newPath = Application.dataPath.Replace("Assets", "") + path;
            byte[] bytes = File.ReadAllBytes(newPath);

            // Debug.Log(bytes.Length);

            int resolution = (int)Mathf.Sqrt(bytes.Length / 2);

            Texture2D tex = new Texture2D(resolution, resolution, TextureFormat.RGBA32, false);
            newBytes = new byte[resolution * resolution * 4];

            float v;

            int length = newBytes.Length / 4;
            int index;

            for (int i = 0; i < length; i++)
            {
                v = Mathf.Round(((bytes[i * 2] + (bytes[(i * 2) + 1] * 255)) / 65535f) * 255f);
                index = i * 4;
                newBytes[index] = (byte)v;
                newBytes[index + 1] = newBytes[index];
                newBytes[index + 2] = newBytes[index];
                newBytes[index + 3] = newBytes[index];
            }

            tex.LoadRawTextureData(newBytes);
            
            index = newPath.LastIndexOf("/");

            string file = newPath.Substring(index + 1);
            file = file.Remove(file.Length - 3);
            file += "Jpg";

            newPath = newPath.Substring(0, index + 1);
            
            newPath = newPath.Replace("RawFiles/", "") + file;
            File.WriteAllBytes(newPath, tex.EncodeToJPG());
            
            newPath = newPath.Replace(Application.dataPath, "Assets");
            AssetDatabase.ImportAsset(newPath);
        }
    }
}