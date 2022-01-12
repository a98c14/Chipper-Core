using Chipper.Prefabs.Tags;
using System;

namespace Chipper.Prefabs.Attributes
{
    public class TagAttribute : Attribute
    {
        public IAssetTag[] Tags { get; private set; }

        public TagAttribute(params IAssetTag[] tags)
        {
            Tags = tags;
        }
    }
}
