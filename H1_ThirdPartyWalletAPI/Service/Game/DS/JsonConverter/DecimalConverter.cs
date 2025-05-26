using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace H1_ThirdPartyWalletAPI.Service.Game.DS.JsonConverter
{
    public class DecimalDeserializeConverter : JsonConverter<decimal>
    {
        public override decimal Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return reader.GetDecimal();
        }

        public override void Write(Utf8JsonWriter writer, decimal value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }
}
