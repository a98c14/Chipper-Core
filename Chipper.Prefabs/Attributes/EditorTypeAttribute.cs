using Chipper.Prefabs.Types;
using System;

namespace Chipper.Prefabs.Attributes
{

    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public class EditorTypeAttribute : Attribute
    {
        public EditorType ValueType;

        public EditorTypeAttribute(EditorType type)
        {
            ValueType = type;
        }
    }
}
