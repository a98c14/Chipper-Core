using Chipper.Prefabs.Types;
using System.Collections.Generic;

namespace Chipper.Prefabs.Data
{
    internal class ModulePart
    {
        public string Description;
        public bool IsArray;
        public EditorType ValueType;
        public Dictionary<string, ModulePart> Child;
    }
}
