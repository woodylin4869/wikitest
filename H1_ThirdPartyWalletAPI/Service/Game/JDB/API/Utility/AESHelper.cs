using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace H1_ThirdPartyWalletAPI.Service.Game.JDB.API.Utility
{
    public class AESHelper
    {
        /// <summary>
        /// 加密
        /// </summary>
        /// <param name="PlainText"></param>
        /// <param name="Key"></param>
        /// <param name="IV"></param>
        /// <returns></returns>
        public string StartEncode(string PlainText, string Key, string IV)
        {
            string result = "";
            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(Key);
                aes.IV = Encoding.UTF8.GetBytes(IV);
                byte[] plainByteArray = Encoding.UTF8.GetBytes(PlainText);
                byte[] cipherByteArray = aes.CreateEncryptor().TransformFinalBlock(plainByteArray, 0, plainByteArray.Length);

                result = Convert.ToBase64String(cipherByteArray).TrimEnd('=').Replace('+','-').Replace('/','_');
            }
            return result;
        }

        /// <summary>
        /// 解密
        /// </summary>
        /// <param name="PlainText"></param>
        /// <param name="Key"></param>
        /// <param name="IV"></param>
        /// <returns></returns>
        public string StartDecode(string PlainText, string Key,string IV)
        {
            string result = "";
            using (Aes aes = Aes.Create())
            {
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.None;
                var chiperText = PlainText.Replace('-', '+').Replace('_', '/');
                int addPaddingCounts = (4 - (chiperText.Length % 4)) % 4;
                for (int i = 0; i < addPaddingCounts; i++)
                {
                    chiperText += "=";
                }
                aes.Key = Encoding.UTF8.GetBytes(Key);
                aes.IV = Encoding.UTF8.GetBytes(IV);
                byte[] cipherByteArray = Convert.FromBase64String(chiperText);
                byte[] plainByteArray = aes.CreateDecryptor().TransformFinalBlock(cipherByteArray, 0, cipherByteArray.Length);
                string plainText = Encoding.UTF8.GetString(plainByteArray);
                result = plainText;
            }
            return result;
        }
    }
}
