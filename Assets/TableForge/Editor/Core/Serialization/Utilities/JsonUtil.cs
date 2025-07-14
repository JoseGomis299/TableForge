using System.Collections.Generic;
using Unity.Plastic.Newtonsoft.Json;
using Unity.Plastic.Newtonsoft.Json.Linq;

namespace TableForge.Editor.Serialization
{
    internal static class JsonUtil
    {
        public static Dictionary<string, string> ToStringDictionary(string json)
        {
            var result = new Dictionary<string, string>();
            var obj = JObject.Parse(json);

            foreach (var property in obj.Properties())
            {
                var value = property.Value;
                string stringValue;

                if (value.Type == JTokenType.Object || value.Type == JTokenType.Array)
                {
                    // Serialize sub-objects and arrays back to compact JSON strings
                    stringValue = value.ToString(Formatting.None);
                }
                else
                {
                    // Convert primitives directly to string
                    stringValue = value.ToString();
                }

                result[property.Name] = stringValue;
            }

            return result;
        }
        
        public static bool IsValidJsonObject(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return false;

            input = input.Trim();

            // Must be a JSON object, not an array
            if (!input.StartsWith(SerializationConstants.JsonObjectStart) || !input.EndsWith(SerializationConstants.JsonObjectEnd))
                return false;

            try
            {
                var token = JToken.Parse(input);
                return token.Type == JTokenType.Object;
            }
            catch (JsonReaderException)
            {
                return false;
            }
        }
        
        public static List<string> JsonArrayToStringList(string jsonArray)
        {
            var result = new List<string>();

            if (string.IsNullOrWhiteSpace(jsonArray))
                return result;

            try
            {
                var token = JToken.Parse(jsonArray);

                if (token.Type != JTokenType.Array)
                    return result; // Not a valid array

                foreach (var item in token)
                {
                    // Preserve full JSON for objects/arrays, simple ToString for primitives
                    if (item.Type == JTokenType.Object || item.Type == JTokenType.Array)
                        result.Add(item.ToString(Formatting.None));
                    else
                        result.Add(item.ToString());
                }
            }
            catch (JsonReaderException)
            {
                // Invalid JSON array
            }

            return result;
        }
    }
}