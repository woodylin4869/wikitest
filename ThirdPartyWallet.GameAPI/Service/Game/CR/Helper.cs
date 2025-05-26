using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Security.Cryptography;
using System.Text;

namespace H1_ThirdPartyWalletAPI.Service.Game.CR
{
    public static class Helper
    {
        public static string AES_Encrypt(string input, string key)
        {
            var iv = new byte[16]; // Using an empty IV for ECB mode
            using (var aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(key);
                aes.Mode = CipherMode.ECB;
                aes.Padding = PaddingMode.PKCS7;

                using (var encryptor = aes.CreateEncryptor(aes.Key, iv))
                {
                    var buffer = Encoding.UTF8.GetBytes(input);
                    return Convert.ToBase64String(encryptor.TransformFinalBlock(buffer, 0, buffer.Length));
                }
            }
        }

        public static string AES_Decrypt(string str, string key)
        {
            if (string.IsNullOrEmpty(str)) return null;
            Byte[] toEncryptArray = Convert.FromBase64String(str);
            System.Security.Cryptography.RijndaelManaged rm = new
            System.Security.Cryptography.RijndaelManaged
            {
                Key = Encoding.UTF8.GetBytes(key),
                Mode = System.Security.Cryptography.CipherMode.ECB,
                Padding = System.Security.Cryptography.PaddingMode.PKCS7
            };
            System.Security.Cryptography.ICryptoTransform cTransform = rm.CreateDecryptor();
            Byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
            return Encoding.UTF8.GetString(resultArray);
        }
        public static long GetCurrentUnixTimestampMillis()
        {
            return (long)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalMilliseconds;
        }


        public static bool IsValidJson(string strInput)
        {
            strInput = strInput.Trim();
            if ((strInput.StartsWith("{") && strInput.EndsWith("}")) || //For object
                (strInput.StartsWith("[") && strInput.EndsWith("]"))) //For array
            {
                try
                {
                    var obj = JToken.Parse(strInput);
                    return true;
                }
                catch (Exception) // An error occurred during parsing
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }


        public class LongToStringConverter : JsonConverter<long>
        {
            public override void WriteJson(JsonWriter writer, long value, JsonSerializer serializer)
            {
                writer.WriteValue(value.ToString());
            }

            public override long ReadJson(JsonReader reader, Type objectType, long existingValue, bool hasExistingValue, JsonSerializer serializer)
            {
                return long.Parse((string)reader.Value);
            }

            public override bool CanRead => true;
            public override bool CanWrite => true;
        }
        public class IntToStringConverter : JsonConverter<int>
        {
            public override void WriteJson(JsonWriter writer, int value, JsonSerializer serializer)
            {
                writer.WriteValue(value.ToString());
            }

            public override int ReadJson(JsonReader reader, Type objectType, int existingValue, bool hasExistingValue, JsonSerializer serializer)
            {
                return int.Parse((string)reader.Value);
            }

            public override bool CanRead => true;
            public override bool CanWrite => true;
        }
        public class DecimalToStringConverter : JsonConverter<decimal>
        {
            public override void WriteJson(JsonWriter writer, decimal value, JsonSerializer serializer)
            {
                writer.WriteValue(value.ToString());
            }

            public override decimal ReadJson(JsonReader reader, Type objectType, decimal existingValue, bool hasExistingValue, JsonSerializer serializer)
            {
                return decimal.Parse((string)reader.Value);
            }

            public override bool CanRead => true;
            public override bool CanWrite => true;
        }
    }
}