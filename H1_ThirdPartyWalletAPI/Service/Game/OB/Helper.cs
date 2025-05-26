using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace H1_ThirdPartyWalletAPI.Service.Game.OB
{
    public class Helper
    {
        public static string MD5encryption(string parameter)
        {
            using var md5 = MD5.Create();
            byte[] plainByteArray = md5.ComputeHash(Encoding.UTF8.GetBytes(parameter));
            string cipherText = Convert.ToHexString(plainByteArray).ToLower();

            return cipherText;
        }


        public static string Encrypt(string toEncrypt, string key)
        {
            byte[] bytes1 = Encoding.UTF8.GetBytes(key);
            byte[] bytes2 = Encoding.UTF8.GetBytes(toEncrypt);
            RijndaelManaged rijndaelManaged = new RijndaelManaged();
            rijndaelManaged.Key = bytes1;
            rijndaelManaged.Mode = CipherMode.ECB;
            rijndaelManaged.Padding = PaddingMode.PKCS7;
            byte[] inArray = rijndaelManaged.CreateEncryptor().TransformFinalBlock(bytes2, 0, bytes2.Length);
            return Convert.ToBase64String(inArray, 0, inArray.Length);
        }



        public static string StartEncode(string PlainText, string Key)
        {
            string result = "";
            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(Key);
                byte[] plainByteArray = Encoding.UTF8.GetBytes(PlainText);
                byte[] cipherByteArray = aes.CreateEncryptor().TransformFinalBlock(plainByteArray, 0, plainByteArray.Length);

                result = Convert.ToBase64String(cipherByteArray).Replace('/', '_');
                //result = Convert.ToBase64String(cipherByteArray).TrimEnd('=').Replace('+', '-');
            }
            return result;
        }
        public static Dictionary<string, string> GetDictionary<TRequest>(TRequest request, bool KeepNullField = false)
        {
            var props = typeof(TRequest).GetProperties();
            var param = new Dictionary<string, string>();

            foreach (var prop in props)
            {
                var propName = prop.Name;
                string propValue = prop.PropertyType == typeof(DateTime) ? ((DateTime)prop.GetValue(request)).ToString("yyyy-MM-ddTHH:mm:ss") : prop.GetValue(request)?.ToString();

                if (KeepNullField || propValue is not null)
                    param.Add(propName, propValue);
            }

            return param;
        }

    }
}
