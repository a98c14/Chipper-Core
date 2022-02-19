using System;

namespace Chipper.Prefabs.Attributes
{
    public class FormerNameAttribute : Attribute
    {
        public string Name;
        public FormerNameAttribute(string name)
        {
            Name = name;
        }
    }
}
