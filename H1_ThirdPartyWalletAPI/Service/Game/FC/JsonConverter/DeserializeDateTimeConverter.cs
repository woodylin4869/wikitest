using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace H1_ThirdPartyWalletAPI.Service.Game.FC.JsonConverter
{
    public class SerializeDateTimeConverter : JsonConverter<DateTime>
    {
        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var target = reader.GetString();
            return DateTime.Parse(target);
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString("yyyy-MM-dd HH:mm:ss"));
        }
    }
}
