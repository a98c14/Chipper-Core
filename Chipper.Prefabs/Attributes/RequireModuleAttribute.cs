using System;

namespace Chipper.Prefabs.Attributes
{
    public class RequireModuleAttribute : Attribute
    {
        public IPrefabModule[] Modules { get; private set; }

        public RequireModuleAttribute(params IPrefabModule[] modules)
        {
            Modules = modules;
        }
    }
}
