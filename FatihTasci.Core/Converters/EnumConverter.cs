using System.Text.Json;
using System.Text.Json.Serialization;

namespace FatihTasci.Core.Converters;

public class FlexibleEnumConverter<T> : JsonConverter<T> where T : struct, Enum
{
    public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            // Anahtar olarak string geldiğinde
            var enumString = reader.GetString();
            return (T)Enum.Parse(typeof(T), enumString);
        }
        else if (reader.TokenType == JsonTokenType.Number)
        {
            // Değer olarak int geldiğinde
            var enumValue = reader.GetInt32();
            return (T)Enum.ToObject(typeof(T), enumValue);
        }

        throw new JsonException("Invalid token type for enum conversion.");
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        // Yanıt olarak int değerini yazar
        writer.WriteNumberValue(Convert.ToInt32(value));
    }
}
public class FlexibleEnumConverterFactory : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
    {
        return typeToConvert.IsEnum; // Enum türlerini kontrol et
    }

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var converterType = typeof(FlexibleEnumConverter<>).MakeGenericType(typeToConvert);
        return (JsonConverter)Activator.CreateInstance(converterType);
    }
}
