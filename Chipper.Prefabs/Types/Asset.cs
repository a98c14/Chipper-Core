using System;
using UnityEngine;

namespace Chipper.Prefabs.Types
{
    public class Asset 
    {
        public int Id { get; set;}
        public long InternalId { get; set;}
        public string Guid { get; set; }
        public string InternalGuid { get; set; }
        public string Name { get; set; }
        public AssetType Type { get; set;}

        public static AssetType GetAssetType(Type type) => type switch
        {
            var t when t == typeof(ParticleSystem) => AssetType.ParticleSystem,
            var t when t == typeof(TrailRenderer) => AssetType.TrailSystem,
            _ => AssetType.ParticleSystem
        };
    }
}
