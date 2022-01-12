using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Chipper.Prefabs.Parser
{
    public class PrefabModuleContractResolver : DefaultContractResolver
    {
        private Type m_StopAtBaseType;

        public PrefabModuleContractResolver(Type stopAtBaseType)
        {
            m_StopAtBaseType = stopAtBaseType;
        }

        protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
        {
            var defaultProperties = base.CreateProperties(type, memberSerialization);
            var includedProperties = ParserUtils.GetPropertyNames(type, m_StopAtBaseType);
            var selectedProperties = defaultProperties
                .Where(p => includedProperties.Contains(p.PropertyName))
                .ToList();

            return selectedProperties;
        }

        protected override JsonContract CreateContract(Type objectType)
        {
            var contract = base.CreateContract(objectType);
            contract.Converter = new PrefabModuleJsonConverter();
            return contract;
        }


        public static string FirstCharToLowerCase(string str)
        {
            if (string.IsNullOrEmpty(str) || char.IsLower(str[0]))
                return str;

            return char.ToLower(str[0]) + str.Substring(1);
        }
    }
}
