namespace TerrainComposer2
{
    // GlobalManager
    public enum PresetMode { Default, StampMode }

    // ItemBehaviour
    public enum PositionMode { Transform, Offset, Locked }

    // Node
    public enum InputKind { Terrain = 0, Noise = 1, Shape = 2, File = 3, Current = 4, Portal = 5 };

    public enum InputTerrainHeight { Collision = 4};
    public enum InputTerrain { Height = 0, Convexity = 5, Angle = 1, Normal = 2, Splatmap = 3, Collision = 4};
    public enum InputNoise { Perlin, RidgedMultifractal, Billow, Voronoi, Random };
    public enum InputShape { Circle, Gradient, Rectangle, Constant };
    public enum InputFile { Image, RawImage };
    public enum InputCurrent { Blur, Expand, Shrink, Distortion };
    public enum InputPortal { Result, PortalList };

    public enum Method { Add = 0, Subtract = 1, Lerp = 2, Multiply = 3, Divide = 4, Difference = 5, Average = 6, Max = 7, Min = 8 }
    public enum MethodItem { Overlay = 2, Max = 7, Min = 8 }
    public enum NodeGroupType { Select, Mask };
    public enum CollisionMode { Height, Mask };
    public enum CollisionDirection { Down, Up};
    public enum ConvexityMode { Convex, Concave };

    // itemGroup Mix Slider
    public enum MixModeEnum { Group, Single }
    
    // GUI Text 
    public enum HorTextAlign { Left, Right, Center }
    public enum VerTextAlign { Top, Center, Bottom }

    // Global
    public enum StretchMode { None, Left, Right, Screen };

    public enum DropPosition { None, Top, Bottom, Left, Right, Center}

    
}