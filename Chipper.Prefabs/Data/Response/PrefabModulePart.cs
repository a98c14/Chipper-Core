using Chipper.Prefabs.Types;
using System.Collections.Generic;

namespace Chipper.Prefabs.Data
{
    public class PrefabModulePart
    {
        public int Id;
        public string Name;
        public object Value;
        public int ArrayIndex;
        public EditorType ValueType;
        public PrefabModulePart[] Children;
    }
}
