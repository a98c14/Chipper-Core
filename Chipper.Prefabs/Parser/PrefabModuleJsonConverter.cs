using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace Chipper.Prefabs.Parser
{
    public class PrefabModuleJsonConverter : JsonConverter
    {
        public PrefabModuleJsonConverter() {}

        public class CustomValueTest
        {
            public string Message;

            public CustomValueTest(string value)
            {
                Message = value;
            }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var t = JToken.FromObject(value);
            if (t.Type != JTokenType.Object)
            {
                t.WriteTo(writer);
            }
            else
            {
                var o = (JObject)t;
                var newToken = JToken.FromObject(new CustomValueTest("Test"));
                foreach(var property in o.Properties())
                {
                    var v = o.GetValue(property.Name);
                }
                o.Add("CustomValue", newToken);
                o.WriteTo(writer);
            }
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException("Unnecessary because CanRead is false. The type will skip the converter.");
        }

        public override bool CanRead
        {
            get { return false; }
        }

        public override bool CanConvert(Type objectType)
        {
            return true;
        }
    }

}
