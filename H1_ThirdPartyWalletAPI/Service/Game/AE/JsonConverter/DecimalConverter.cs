using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace H1_ThirdPartyWalletAPI.Service.Game.AE.JsonConverter
{
    public class DecimalDeserializeConverter : JsonConverter<decimal>
    {
        public override decimal Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Number)
            {
                if (reader.TryGetDecimal(out decimal value))
                {
                    return value;
                }
                else
                {
                    throw new JsonException("Failed to convert JSON number to decimal.");
                }
            }
            else if (reader.TokenType == JsonTokenType.String)
            {
                if (decimal.TryParse(reader.GetString(), out decimal value))
                {
                    return value;
                }
                else
                {
                    throw new JsonException("Failed to convert JSON string to decimal.");
                }
            }
            else
            {
                throw new JsonException($"Unexpected token type: {reader.TokenType}");
            }
        }

        public override void Write(Utf8JsonWriter writer, decimal value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }
}
