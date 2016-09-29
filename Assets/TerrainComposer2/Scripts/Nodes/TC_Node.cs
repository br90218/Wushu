using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

namespace TerrainComposer2
{
    public class TC_Node : TC_ItemBehaviour
    {
        public InputKind inputKind;
        public InputTerrain inputTerrain;
        public InputNoise inputNoise;
        public InputShape inputShape;
        public InputFile inputFile;
        public InputCurrent inputCurrent;
        public InputPortal inputPortal;

        public NodeGroupType nodeType;
        public CollisionMode collisionMode;
        public CollisionDirection collisionDirection;
        
        public bool clamp;
        public float radius = 300;

        public TC_RawImage rawImage;
        public bool square;
        public ImageSettings imageSettings;
        public int splatSelectIndex;
        public Noise noise;
        public Shapes shapes;
        public int iterations = 1;
        public int mipmapLevel = 1;
        public float convexityStrength;

        public Texture stampTex;

        public Image image;
        public TCImage tcImage;

        public float posYOld;
        public int collisionMask = -1;
        public bool useConstant;

        public Vector3 size = new Vector3(2048, 0, 2048);

        [Serializable]
        public class Shapes
        {
            public Vector2 topSize = new Vector2(500, 500);
            public Vector2 bottomSize = new Vector2(1000,1000);
            public float size = 500;
        }

        public override void Awake()
        {
            base.Awake();
            if (rawImage != null) rawImage.referenceCount++;
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            // Debug.Log("Node " + UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode + " --- "+ UnityEditor.EditorApplication.isPlaying);

            #if UNITY_EDITOR
                if (UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode || UnityEditor.EditorApplication.isPlaying) return;
            #endif

            if (rawImage != null) rawImage.UnregisterReference();
        }

        public void SetDefaultSettings()
        {
            size = TC_Settings.instance.global.defaultTerrainSize;

            if (transform.parent.GetSiblingIndex() == 0)
            {
                inputKind = InputKind.Shape;
                inputShape = InputShape.Circle;
            }
            else if (outputId == TC.heightOutput)
            {
                inputKind = InputKind.File;
                inputFile = InputFile.RawImage;
                clamp = true;
            }
        }

        public void CheckTransformChange()
        {
            // Debug.Log("check tansform change");
            if (ctOld.hasChanged(this))
            {
                TC.AutoGenerate();
                // Debug.Log(t.name);
                ctOld.Copy(this);
            }
        }

        public override void ChangeYPosition(float y)
        {
            #if UNITY_EDITOR
                // UnityEditor.Undo.RecordObject(t, "Edit Transform");
                UnityEditor.Undo.RecordObject(this, "Edit Transform");
            #endif
            posY += y / t.lossyScale.y;
        }

        public void CalcBounds()
        {
            // if (bounds == null) bounds = new Bounds();
            bounds.center = t.position;
            bounds.size = Vector3.Scale(new Vector3(1000, 100, 1000), t.lossyScale);
        }

        public bool OutOfBounds()
        {
            // Debug.Log(bounds + " " + Area2D.current.bounds);
            if (bounds.Intersects(TC_Area2D.current.bounds)) return false;
            else
            {
                TC_Reporter.Log(name + " Out of bounds!");
                return true;
            }
        }

        public Enum GetInputPopup()
        {
            if (inputKind == InputKind.Terrain)
            {
                if (outputId == TC.heightOutput)
                {
                    InputTerrainHeight inputTerrainHeight = InputTerrainHeight.Collision;
                    return inputTerrainHeight;
                }
                else return inputTerrain;
            }
            else if (inputKind == InputKind.Noise) return inputNoise;
            else if (inputKind == InputKind.Shape) return inputShape;
            else if (inputKind == InputKind.File) return inputFile;
            else if (inputKind == InputKind.Current) return inputCurrent;
            else if (inputKind == InputKind.Portal) return inputPortal;

            return null;
        }

        public void SetInputPopup(Enum popup)
        {
            if (inputKind == InputKind.Terrain) inputTerrain = (InputTerrain)popup;
            else if (inputKind == InputKind.Noise) inputNoise = (InputNoise)popup;
            else if (inputKind == InputKind.Shape) inputShape = (InputShape)popup;
            else if (inputKind == InputKind.File) inputFile = (InputFile)popup;
            else if (inputKind == InputKind.Current) inputCurrent = (InputCurrent)popup;
            else if (inputKind == InputKind.Portal) inputPortal = (InputPortal)popup;
        }
        
        public void Init()
        {
            if (inputKind == InputKind.Terrain)
            {
                if (inputTerrain == InputTerrain.Normal) { active = false; }
            }
            if (inputKind == InputKind.Noise)
            {
                if (noise == null)
                {
                    noise = new Noise();
                    noise.SetDefault(inputNoise, true);
                }
            }
            else if (inputKind == InputKind.Shape)
            {
                if (shapes == null) shapes = new Shapes();
            }
            else if (inputKind == InputKind.File)
            {
                if (inputFile == InputFile.RawImage)
                {
                    bool dropStampTex = false;

                    if (rawImage == null)
                    {
                        if (stampTex != null) dropStampTex = true; else { active = false; return; }
                    }
                    else if (rawImage.tex == null && stampTex != null) dropStampTex = true;
                    else if (stampTex == null) { active = false; return; }

                    if (dropStampTex)
                    {
                        DropTextureEditor(stampTex);
                        if (rawImage == null)
                        {
                            active = false; stampTex = null;
                        }
                    }
                }
                if (inputFile == InputFile.Image)
                {
                    if (image != null)
                    {
                        if (image.index != -1)
                        {
                            tcImage = TC_Settings.instance.imageList[image.index];
                            tcImage.LoadTexture();
                        }
                    }
                }
            }
            else if (inputKind == InputKind.Portal) { active = false; }
        }

        public bool DropTextureEditor(Texture tex)
        {
            #if UNITY_EDITOR
            if (tex != null)
            {
                string path = Application.dataPath.Replace("Assets", "") + UnityEditor.AssetDatabase.GetAssetPath(tex);
                int index = path.LastIndexOf("/");
                path = path.Insert(index, "/RawFiles");
                path = path.Remove(path.Length - 3, 3) + "raw";

                TC_RawImage oldRawImage = rawImage;
                if (oldRawImage) oldRawImage.UnregisterReference();

                rawImage = TC_Settings.instance.AddRawFile(path);
                if (rawImage != null)
                {
                    stampTex = tex;
                    
                    TC.RefreshOutputReferences(outputId, true);

                    TC_Reporter.Log(path);
                    TC_Reporter.Log("Node index " + rawImage.name);

                    return true;
                }
                else
                {
                    TC.AddMessage("This is not a stamp preview image.\n\nThe raw heightmap file needs to be placed in a 'RawFiles' folder, then TC2 will automatically make a preview image one folder before it.\nThis image needs to be used for dropping on the node.", 0, 4);
                }
            }
            #endif
            return false;
        }

    }

    [Serializable]
    public class Noise
    {
        public float frequency = 100f;
        public float lacunarity = 2.0f;
        public int octaves = 6;
        public float persistence = 0.5f;
        public float seed;

        public int firstSelected;

        public void SetDefault(InputNoise inputNoise, bool onlyFirst)
        {
            if (inputNoise == InputNoise.Perlin) DefaultPerlin(onlyFirst);
            else if (inputNoise == InputNoise.RidgedMultifractal) DefaultMultiFractal(onlyFirst);
            else if (inputNoise == InputNoise.Billow) DefaultBillow(onlyFirst);
            else if (inputNoise == InputNoise.Voronoi) DefaultVonoroi(onlyFirst);
        }
        
        public void DefaultPerlin(bool first)
        {
            if (first)
            {
                if (Mathw.BitSwitch(firstSelected, 0)) return;
                Mathw.SetBitSwitch(firstSelected, 0);
            }


        }

        public void DefaultMultiFractal(bool first)
        {
            if (first)
            {
                if (Mathw.BitSwitch(firstSelected, 1)) return;
                Mathw.SetBitSwitch(firstSelected, 1);
            }

        }

        public void DefaultBillow(bool first)
        {
            if (first)
            {
                if (Mathw.BitSwitch(firstSelected, 2)) return;
                Mathw.SetBitSwitch(firstSelected, 2);
            }

        }

        public void DefaultVonoroi(bool first)
        {
            if (first)
            {
                if (Mathw.BitSwitch(firstSelected, 3)) return;
                Mathw.SetBitSwitch(firstSelected, 3);
            }
        }
    }

    [Serializable]
    public class ImageSettings
    {
        public float repeatRotation;
    }

    [Serializable]
    public class Image
    {
        public enum PixelReadMode { ColorRange, Channel };
        static public readonly string[] channels = { "Red", "Green", "Blue", "Alpha" };

        public int index = -1;
        public PixelReadMode pixelReadMode;
        public int channel;
        public List<ColorRange> colorRanges = new List<ColorRange>();

        [Serializable]
        public class ColorRange
        {
            public Color colorStart = Color.black;
            public Color colorEnd = Color.white;
            public bool one;

            public AnimationCurve curve = AnimationCurve.Linear(0, 1, 1, 1);
        }
    }

    [Serializable]
    public class TCImage
    {
        public Texture2D tex;
        // public Vector2 resolution;
        // [NonSerialized] public byte[] bytes;

        public TCImage(Texture2D tex)
        {
            this.tex = tex;
        }

        public void LoadTexture()
        {
            // int res = tex.width * tex.height * 4;
            // if (bytes == null) bytes = tex.GetRawTextureData();
            // else if (bytes.Length != res) bytes = tex.GetRawTextureData(); 
            // Debug.Log(bytes.Length + " width " + tex.width + " height " + tex.height + " res " + tex.width * tex.height * 4);  
            // resolution = new Vector2(tex.width, tex.height); 
        }
    }

}