using System;

namespace Chipper.Prefabs.Attributes
{

    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public class EditorTooltipAttribute : Attribute
    {
        public string Tooltip;

        public EditorTooltipAttribute(string tooltip)
        {
            Tooltip = tooltip;
        }
    }
}
