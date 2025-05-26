using H1_ThirdPartyWalletAPI.Model.Game.JILI.Request;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace H1_ThirdPartyWalletAPI.Service.Game.JILI
{
    public static class Helper
    {
        /// <summary>
        /// 加密
        /// </summary>
        public static string MD5encryption(string parameter, string parametertwo,string parameterthree)
        {
            using var md5 = MD5.Create();
            byte[] plainByteArray = md5.ComputeHash(Encoding.UTF8.GetBytes($"{parameter}{parametertwo}{parameterthree}"));
            string cipherText = Convert.ToHexString(plainByteArray).ToLower();

            return cipherText;
        }
        /// <summary>
        /// MD5參數組成
        /// </summary>
        /// <typeparam name="TRequest"></typeparam>
        /// <param name="request"></param>
        /// <param name="KeepNullField"></param>
        /// <returns></returns>
        public static Dictionary<string, string> GetMD5Dictionary<TRequest>(TRequest request, bool KeepNullField = false)
        {
            var props = typeof(TRequest).GetProperties();
            var param = new Dictionary<string, string>();

            foreach (var prop in props)
            {
                if (Attribute.IsDefined(prop, typeof(System.ComponentModel.DataAnnotations.RequiredAttribute)))
                    continue;

                var propName = prop.Name;
                string propValue = prop.PropertyType == typeof(DateTime) ? ((DateTime)prop.GetValue(request)).ToString("yyyy-MM-ddTHH:mm:ss") : prop.GetValue(request)?.ToString();

                if (KeepNullField || propValue is not null)
                    param.Add(propName, propValue);
            }

            return param;
        }

        /// <summary>
        /// URL參數組成
        /// </summary>
        /// <typeparam name="TRequest"></typeparam>
        /// <param name="request"></param>
        /// <param name="KeepNullField"></param>
        /// <returns></returns>
        public static Dictionary<string, string> GetURLDictionary<TRequest>(TRequest request, bool KeepNullField = false)
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
