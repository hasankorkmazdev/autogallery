using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FatihTasci.Core.Converters;


public class BoolConverter : JsonConverter<bool>
{
    public override bool Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            var stringValue = reader.GetString();
            if (bool.TryParse(stringValue, out var result))
            {
                return result;
            }
            if (stringValue == "1") // "1" -> true
            {
                return true;
            }
            if (stringValue == "0") // "0" -> false
            {
                return false;
            }
        }
        else if (reader.TokenType == JsonTokenType.Number)
        {
            var numericValue = reader.GetInt32();
            return numericValue == 1; // 1 -> true, diğer sayılar -> false
        }
        else if (reader.TokenType == JsonTokenType.True)
        {
            return true; // JSON'dan gelen saf true değeri
        }
        else if (reader.TokenType == JsonTokenType.False)
        {
            return false; // JSON'dan gelen saf false değeri
        }

        throw new JsonException($"Değer bir boolean'a dönüştürülemedi: {reader.GetString()}");
    }

    public override void Write(Utf8JsonWriter writer, bool value, JsonSerializerOptions options)
    {
        // Doğrudan bool değeri yaz
        writer.WriteBooleanValue(value);
    }
}
