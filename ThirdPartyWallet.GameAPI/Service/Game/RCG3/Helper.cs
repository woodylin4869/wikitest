using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ThirdPartyWallet.GameAPI.Service.Game.RCG3
{
    public static class Helper
    {
        public static string MD5encryption(string parameter)
        {
            using var md5 = MD5.Create();
            byte[] plainByteArray = md5.ComputeHash(Encoding.UTF8.GetBytes($"{parameter}"));
            string cipherText = Convert.ToBase64String(plainByteArray);

            return cipherText;
        }

        public static Dictionary<string, object> GetDictionary<TRequest>(TRequest request, bool KeepNullField = false)
        {
            var props = typeof(TRequest).GetProperties();
            var param = new Dictionary<string, object>();

            foreach (var prop in props)
            {
                var propName = prop.Name;
                var propValue = prop.PropertyType == typeof(DateTime) ? ((DateTime)prop.GetValue(request)).ToString("yyyy-MM-ddTHH:mm:ss") : prop.GetValue(request);

                if (KeepNullField || propValue is not null)
                    param.Add(propName, propValue);
            }

            return param;
        }

        public static string desEncryptBase64(string source, string key, string iv)
        {
            DESCryptoServiceProvider des = new DESCryptoServiceProvider();
            byte[] bkey = Encoding.ASCII.GetBytes(key);
            byte[] biv = Encoding.ASCII.GetBytes(iv);
            byte[] dataByteArray = Encoding.UTF8.GetBytes(source);

            des.Key = bkey;
            des.IV = biv;
            string encrypt = "";
            using (MemoryStream ms = new MemoryStream())
            using (CryptoStream cs = new CryptoStream(ms, des.CreateEncryptor(), CryptoStreamMode.Write))
            {
                cs.Write(dataByteArray, 0, dataByteArray.Length);
                cs.FlushFinalBlock();
                encrypt = Convert.ToBase64String(ms.ToArray());
            }
            return encrypt;
        }
        public static string desDecryptBase64(string encrypt, string key, string iv)
        {
            DESCryptoServiceProvider des = new DESCryptoServiceProvider();
            byte[] bkey = Encoding.ASCII.GetBytes(key);
            byte[] biv = Encoding.ASCII.GetBytes(iv);
            des.Key = bkey;
            des.IV = biv;

            byte[] dataByteArray = Convert.FromBase64String(encrypt);
            using (MemoryStream ms = new MemoryStream())
            {
                using (CryptoStream cs = new CryptoStream(ms, des.CreateDecryptor(), CryptoStreamMode.Write))
                {
                    cs.Write(dataByteArray, 0, dataByteArray.Length);
                    cs.FlushFinalBlock();
                    return Encoding.UTF8.GetString(ms.ToArray());
                }
            }
        }
    }
}
