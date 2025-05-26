using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace H1_ThirdPartyWalletAPI.Service.Game.PP
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
