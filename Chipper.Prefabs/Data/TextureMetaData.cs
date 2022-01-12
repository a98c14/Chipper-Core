using System.Collections.Generic;
using UnityEngine;

namespace Chipper.Prefabs.Data
{
    public class TextureMetaData 
    {
        public string Name { get ; set;}
        public string Guid { get; set;}
        public byte[] Texture { get; set;}
        public List<Sprite> Sprites { get; set ;}
    }
}
