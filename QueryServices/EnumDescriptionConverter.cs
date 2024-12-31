using Newtonsoft.Json;
using System.ComponentModel;
using System.Reflection;

public class EnumDescriptionConverter<T> : JsonConverter<T> where T : Enum
{
    public override T ReadJson(JsonReader reader, Type objectType, T existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        var desc = reader.Value.ToString();
        try
        {
            var result = Enum.GetValues(typeof(T)).Cast<T>().Single(value =>
            {
                var member = typeof(T).GetMember(value.ToString())[0];
                var attribute = (DescriptionAttribute)member.GetCustomAttribute(typeof(DescriptionAttribute), false);
                return string.Equals(desc, attribute?.Description, StringComparison.OrdinalIgnoreCase);
            });
            return result;
        }
        catch (InvalidOperationException e) when (e.Message.Equals("Sequence contains no matching element"))
        {
            throw new JsonSerializationException($"Error: Unrecognized enum description: {desc}.");
        }
    }

    public override void WriteJson(JsonWriter writer, T value, JsonSerializer serializer)
    {
        var member = typeof(T).GetMember(value.ToString())[0];
        var attribute = (DescriptionAttribute)member.GetCustomAttribute(typeof(DescriptionAttribute), false);
        writer.WriteValue(attribute.Description);
    }

    public override bool CanRead => true;
    public override bool CanWrite => true;
}
