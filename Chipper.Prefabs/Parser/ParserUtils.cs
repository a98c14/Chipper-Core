using System;
using System.Collections.Generic;
using System.Reflection;

namespace Chipper.Prefabs.Parser
{
    public class ParserUtils
    {
        /// <summary>
        /// Gets a list of all public instance properties of a given class type
        /// excluding those belonging to or inherited by the given base type.
        /// </summary>
        /// <param name="type">The Type to get property names for</param>
        /// <param name="stopAtType">A base type inherited by type whose properties should not be included.</param>
        /// <returns></returns>
        public static List<string> GetPropertyNames(Type type, Type stopAtBaseType)
        {
            var propertyNames = new List<string>();
            if (type == null || type == stopAtBaseType) return propertyNames;
            var currentType = type;

            do
            {
                var fields = currentType.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

                foreach (var field in fields)
                    if (!propertyNames.Contains(field.Name))
                        propertyNames.Add(field.Name);

                currentType = currentType.BaseType;
            } while (currentType != null && currentType != stopAtBaseType);

            return propertyNames;
        }
    }
}
