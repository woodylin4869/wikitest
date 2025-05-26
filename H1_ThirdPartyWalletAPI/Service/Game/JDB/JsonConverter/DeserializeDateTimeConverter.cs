using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace H1_ThirdPartyWalletAPI.Service.Game.JDB.JsonConverter
{
    public class DeserializeDateTimeConverter : JsonConverter<DateTime>
    {
        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            // Damn!! JDB DateTime Formatter is dd-MM-yyyy , need convert to yyyy-MM-dd
            var target = reader.GetString();
            if (target.IndexOf(" ") != -1)
            {
                var date = target.Substring(0, target.IndexOf(" "));
                var time = target.Substring(target.IndexOf(" ") + 1);
                var units = date.Split("-").Reverse();
                return DateTime.Parse(string.Join("-", units) + " " + time);
            }
            else
            {
                var units = target.Split("-").Reverse();
                return DateTime.Parse(string.Join("-", units));
            }
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString("yyyy-MM-dd HH:mm:ss"));
        }
    }
    public class SerializeDateTimeConverter : JsonConverter<DateTime>
    {
        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var target = reader.GetString();
            return DateTime.Parse(target);
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString("dd-MM-yyyy HH:mm:ss"));
        }
    }
}
