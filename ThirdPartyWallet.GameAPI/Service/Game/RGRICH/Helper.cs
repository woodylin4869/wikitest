using System.Security.Cryptography;
using System.Text;
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.DataProtection.KeyManagement;

namespace H1_ThirdPartyWalletAPI.Service.Game.RGRICH
{
    public static class Helper
    {
        public static string CalculateMD5Hash(string input)
        {
            using (MD5 md5 = MD5.Create())
            {
                byte[] inputBytes = Encoding.ASCII.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("X2"));
                }
                return sb.ToString();
            }
        }

        /// <summary>
        /// aes-256加密
        /// ref：https://blog.csdn.net/zxb11c/article/details/127675490
        /// </summary>
        /// <param name="content"></param>
        /// <param name="aesKey"></param>
        /// <returns></returns>
        public static string Encrypt(string content, string aesKey)
        {
            RijndaelManaged aes = new RijndaelManaged();
            byte[] byteContent = Encoding.UTF8.GetBytes(content);
            byte[] byteKey = Encoding.UTF8.GetBytes(aesKey);
            aes.Mode = CipherMode.ECB;
            aes.Padding = PaddingMode.PKCS7;
            aes.KeySize = 256;
            //aes.Key = _key;
            aes.Key = byteKey;
            //aes.IV = _iV;

            using var crtpto = aes.CreateEncryptor();
            byte[] decrypted = crtpto.TransformFinalBlock(byteContent, 0, byteContent.Length);
            return Convert.ToBase64String(decrypted);
        }
    }
}