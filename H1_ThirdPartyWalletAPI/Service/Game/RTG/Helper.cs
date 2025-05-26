using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace H1_ThirdPartyWalletAPI.Service.Game.RTG
{
    public static class Helper
    {
        /// <summary>
        /// 加密
        /// </summary>
        public static string DESEncryption(string plainText, string key, string iv)
        {
            using (var des = DES.Create())
            {
                des.Mode = CipherMode.CBC;
                des.Padding = PaddingMode.PKCS7;
                des.Key = Encoding.UTF8.GetBytes(key);
                des.IV = Encoding.UTF8.GetBytes(iv);
                byte[] plainByteArray = Encoding.UTF8.GetBytes(plainText);

                string cipherText1 = Convert.ToBase64String(plainByteArray);

                byte[] cipherByteArray = des.CreateEncryptor().TransformFinalBlock(plainByteArray, 0,
                    plainByteArray.Length);
                string cipherText = Convert.ToBase64String(cipherByteArray);
                return cipherText;
            }
        }
        /// <summary>
        /// 產生簽章
        /// </summary>
        public static string CreateSignature(string clientID, string clientSecret, string timestamp, string
            encryptData)
        {
            using (MD5 md5Hash = MD5.Create())
            {
                //encryptData = HttpUtility.UrlEncode(encryptData);
                // Convert the input string to a byte array and compute the hash.
                var inputText = clientID + clientSecret + timestamp + encryptData;                
                byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(inputText));
                string cipherText = BitConverter.ToString(data)
                .Replace("-", String.Empty)
                .ToLower();
                // Return the hexadecimal string.
                return cipherText;
            }
        }
    }
}