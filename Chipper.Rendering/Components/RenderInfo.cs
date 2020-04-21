using Unity.Entities;
using UnityEngine;

public struct RenderInfo : IComponentData
{
    public int     SortingLayer;
    public int     Layer;
    public bool    IsDefaultDirectionRight;
    public Color32 Color;
}
