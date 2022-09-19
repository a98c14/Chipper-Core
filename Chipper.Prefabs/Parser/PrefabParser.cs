using Chipper.Prefabs.Data;
using Chipper.Prefabs.Data.Response;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace Chipper.Prefabs.Parser
{        
    public static class PrefabParser
    {
        /// <summary>
        /// For each prefab in detailed prefabs, convert the modules
        /// of the prefab to JSON format
        /// </summary>
        /// <example>
        /// Initial -> 
        /// {
        ///    "id": 273,
        ///    "name": "Life",
        ///    "parentId": 159,
        ///    "arrayIndex": 0,
        ///    "valueType": 11,
        ///    "value": 1,
        ///    "children": null
        ///  },
        ///  {
        ///    "id": 291,
        ///    "name": "Armor",
        ///    "parentId": 159,
        ///    "arrayIndex": 0,
        ///    "valueType": 11,
        ///    "value": 5,
        ///    "children": null
        ///  },
        ///  {
        ///    "id": 280,
        ///    "name": "Shield",
        ///    "parentId": 159,
        ///    "arrayIndex": 0,
        ///    "valueType": 11,
        ///    "value": 3,
        ///    "children": null
        ///  }
        ///  Result -> { \"Life\": 1, \"Shield\": 3, \"Armor\": 5 }
        /// </example>
        /// <param name="prefab">Prefab to serialize</param>
        /// <returns>Name and Serialized Json of prefab</returns>
        internal static IEnumerable<(string name, string value)> SerializePrefabModules(Prefab prefab)
        {
            prefab.Name = prefab.Name.ToLower().Replace(' ', '_');
            var moduleMap = new Dictionary<string, object>();
            var stack = new Stack<PrefabModulePart>();

            // Push all the root modules to processing stack and 
            // create maps. Since these are root modules we know for sure
            // that they will have children and not values.
            foreach (var module in prefab.Modules)
            {
                stack.Push(module);
                moduleMap[module.Name] = new Dictionary<string, object>();
            }

            while (stack.Count > 0)
            {
                var module = stack.Pop();

                // Get the map for the module
                var d = (Dictionary<string, object>)moduleMap[module.Name];

                if(module.Children == null)
                    continue;

                foreach (var child in module.Children)
                {
                    // If module has children link their maps with parent map
                    if (child.Children != null && child.Children.Length > 0)
                    {
                        var c = new Dictionary<string, object>();
                        d[child.Name] = c;
                        moduleMap[child.Name] = c;
                        stack.Push(child);
                    }
                    // If the module has no children just save the value without adding
                    // to the processing stack.
                    else
                    {
                        d[child.Name] = child.Value;
                    }
                }
            }

            return prefab.Modules.Select(x => (x.Name, JsonConvert.SerializeObject(moduleMap[x.Name])));
        }
    }
}
