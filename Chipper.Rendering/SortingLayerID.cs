using UnityEngine;

public struct SortingLayerID 
{
    public static int Background;
    public static int Ground;
    public static int Corpse;
    public static int Default;
    public static int Projectiles;
    public static int Effects;
    
    public static void InitializeIDs()
    {
        Background      = SortingLayer.NameToID("Background");
        Ground          = SortingLayer.NameToID("Ground");
        Corpse          = SortingLayer.NameToID("Corpse");
        Default         = SortingLayer.NameToID("Default");
        Projectiles     = SortingLayer.NameToID("Projectiles");
        Effects         = SortingLayer.NameToID("Effects");
    }
}
