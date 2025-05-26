using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace H1_ThirdPartyWalletAPI.Service.Game.META
{
    public static class AesUtil
    {
        public static string Encrypt(string toEncrypt, string key, string iv)
        {
            byte[] keyArray = UTF8Encoding.UTF8.GetBytes(key);
            byte[] ivArray = UTF8Encoding.UTF8.GetBytes(iv);
            byte[] toEncryptArray = UTF8Encoding.UTF8.GetBytes(toEncrypt);

            RijndaelManaged rDel = new RijndaelManaged();
            rDel.KeySize = 256;
            var key16 = keyArray.Take(32).ToArray();
            var _iv16 = ivArray.Take(16).ToArray();
            rDel.Key = key16;
            rDel.IV = _iv16;  // 初始化向量 initialization vector (IV)
            rDel.Mode = CipherMode.CBC; // 密碼分組連結（CBC，Cipher-block chaining）模式
            rDel.Padding = PaddingMode.PKCS7;

            ICryptoTransform cTransform = rDel.CreateEncryptor();
            byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
            return Convert.ToBase64String(resultArray, 0, resultArray.Length);
        }

        public static string Decrypt(string toDecrypt, string key, string iv)
        {
            byte[] keyArray = UTF8Encoding.UTF8.GetBytes(key);
            byte[] ivArray = UTF8Encoding.UTF8.GetBytes(iv);
            byte[] toEncryptArray = Convert.FromBase64String(toDecrypt);

            RijndaelManaged rDel = new RijndaelManaged();

            var key16 = keyArray.Take(32).ToArray();
            var iv16 = ivArray.Take(16).ToArray();

            rDel.KeySize = 256;
            rDel.Key = key16;
            rDel.IV = iv16;
            rDel.Mode = CipherMode.CBC;
            rDel.Padding = PaddingMode.PKCS7;

            ICryptoTransform cTransform = rDel.CreateDecryptor();
            byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
            return UTF8Encoding.UTF8.GetString(resultArray);
        }
    }
}

