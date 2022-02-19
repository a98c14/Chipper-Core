using Chipper.Prefabs.Attributes;
using Chipper.Prefabs.Data;
using Chipper.Prefabs.Types;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Chipper.Prefabs.Parser
{
    public static class TypeParser
    {
        internal static Dictionary<string, ModulePart> GetParts(Type rootType)
        {
            var proccessQueue = new Queue<PartProccessItem>();
            var root = new PartProccessItem
            {
                Type = rootType,
                Children = new Dictionary<string, ModulePart>(),
            };
            proccessQueue.Enqueue(root);
            while (proccessQueue.Count > 0)
            {
                var currentElement = proccessQueue.Dequeue();
                var fields = currentElement.Type.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                for (int i = 0; i < fields.Length; i++)
                {
                    var fieldType = fields[i].FieldType;
                    var typeAttr = fields[i].GetCustomAttribute<EditorTypeAttribute>();
                    var tooltipAttr = fields[i].GetCustomAttribute<EditorTooltipAttribute>();
                    var tooltip = tooltipAttr != null ? tooltipAttr.Tooltip : "";
                    var isArray = typeof(IEnumerable).IsAssignableFrom(fieldType) && fieldType.IsGenericType;

                    // Use the generic argument of fieldType if its a generic collection
                    if(isArray)
                        fieldType = fieldType.GetGenericArguments()[0];

                    var type = fieldType switch
                    {
                        var t when t == typeof(Vector2)  => EditorType.Vec2,
                        var t when t == typeof(Vector3)  => EditorType.Vec3,
                        var t when t == typeof(Vector4)  => EditorType.Vec4,
                        var t when t == typeof(bool)     => EditorType.Bool,
                        var t when t == typeof(int)      => EditorType.Number,
                        var t when t == typeof(float)    => EditorType.Number,
                        var t when t == typeof(double)   => EditorType.Number,
                        var t when t == typeof(decimal)  => EditorType.Number,
                        var t when t == typeof(string)   => EditorType.Text,
                        var t when t == typeof(Color)    => EditorType.Color,
                        var t when t == typeof(Material) => EditorType.Material,
                        var t when t.IsEnum              => EditorType.Enum,
                        var t when t == typeof(UnityEngine.Sprite) => EditorType.Sprite,
                        _ => EditorType.Object
                    };
                    type = typeAttr != null ? typeAttr.ValueType : type;

                    if (type == EditorType.Object)
                    {
                        var children = new Dictionary<string, ModulePart>();
                        currentElement.Children.Add(fields[i].Name, new ModulePart
                        {
                            Tooltip = tooltip,
                            IsArray = isArray,
                            ValueType = type,
                            Children = children,
                        });

                        proccessQueue.Enqueue(new PartProccessItem
                        {
                            Type = fieldType,
                            Children = children,
                        });
                    }
                    else
                    {
                        currentElement.Children.Add(fields[i].Name, new ModulePart
                        {
                            Tooltip = tooltip, 
                            IsArray = isArray,
                            ValueType = type,
                            Children = null,
                        });
                    }
                }
            }

            return root.Children;
        }
    }
}
