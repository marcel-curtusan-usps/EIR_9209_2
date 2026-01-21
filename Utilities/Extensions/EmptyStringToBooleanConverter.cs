using Newtonsoft.Json;
using System;

namespace EIR_9209_2.Utilities.Extensions
{
    /// <summary>
    /// Converter that treats empty string or null as false when deserializing booleans.
    /// Accepts boolean values and string representations ("true", "false", "").
    /// </summary>
    public class EmptyStringToBooleanConverter : JsonConverter<bool>
    {
        public override bool ReadJson(JsonReader reader, Type objectType, bool existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            try
            {
                if (reader.Value == null)
                    return false;

                var s = reader.Value.ToString();
                if (string.IsNullOrEmpty(s))
                    return false;

                if (bool.TryParse(s, out var b))
                    return b;

                // handle numeric 0/1
                if (int.TryParse(s, out var i))
                    return i != 0;

                throw new JsonSerializationException($"Cannot convert value '{s}' to boolean.");
            }
            catch (Exception e)
            {
                throw new JsonSerializationException($"Exception during boolean deserialization: {e}");
            }
        }

        public override void WriteJson(JsonWriter writer, bool value, JsonSerializer serializer)
        {
            writer.WriteValue(value);
        }

        public override bool CanRead => true;
        public override bool CanWrite => true;
    }
}
