using Unity.Entities;
using UnityEngine;
using System;

[System.Serializable]
public struct MaterialInfo : ISharedComponentData
{
    public int MaterialID;
}
