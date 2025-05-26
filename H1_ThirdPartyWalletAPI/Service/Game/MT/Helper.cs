using System.Security.Cryptography;
using System.Text;
using System;
using Microsoft.AspNetCore.Components.Forms;
using System.Collections.Generic;

namespace H1_ThirdPartyWalletAPI.Service.Game.MT
{
    public static class Helper
    {
        public static string MD5encryption(string parameter, string parametertwo, string parameterthree)
        {
            using var md5 = MD5.Create();
            byte[] plainByteArray = md5.ComputeHash(Encoding.UTF8.GetBytes($"{parameter}{parametertwo}{parameterthree}"));
            string cipherText = Convert.ToHexString(plainByteArray).ToLower();

            return cipherText;
        }

        public static string Bast64(string parameter)
        {
            Byte[] bytesEncode = System.Text.Encoding.UTF8.GetBytes(parameter); //取得 UTF8 2進位 Byte
            string resultEncode = Convert.ToBase64String(bytesEncode);
            return resultEncode;
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

        public static string ReplaceBetween(string input, string start, string end, string replacement)
        {
            int startIndex = input.IndexOf(start);
            if (startIndex == -1) return input;

            startIndex += start.Length;
            int endIndex = input.IndexOf(end, startIndex);
            if (endIndex == -1) return input;

            string before = input.Substring(0, startIndex);
            string after = input.Substring(endIndex);

            return before + replacement + after;
        }
    }
}
