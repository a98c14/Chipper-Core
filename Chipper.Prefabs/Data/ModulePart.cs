using Chipper.Prefabs.Types;
using System.Collections.Generic;

namespace Chipper.Prefabs.Data
{
    public class ModulePart
    {
        public string Tooltip;
        public bool IsArray;
        public EditorType ValueType;
        public Dictionary<string, ModulePart> Children;
    }
}
