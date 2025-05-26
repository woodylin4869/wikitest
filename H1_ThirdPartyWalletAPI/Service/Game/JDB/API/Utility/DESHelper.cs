using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace H1_ThirdPartyWalletAPI.Service.Game.JDB.API.Utility
{
    public class DESHelper
    {
        /// <summary>加密</summary>
        public static string Encrypt(string plainText, string key, string iv)
        {
            DES des = null;

            try
            {
                des = DES.Create();

                des.Mode = CipherMode.CBC;
                des.Padding = PaddingMode.PKCS7;
                des.Key = Encoding.UTF8.GetBytes(key);
                des.IV = Encoding.UTF8.GetBytes(iv);

                byte[] plainByteArray = Encoding.UTF8.GetBytes(plainText);
                byte[] cipherByteArray = des.CreateEncryptor().TransformFinalBlock(plainByteArray, 0, plainByteArray.Length);

                string cipherText = Convert.ToBase64String(cipherByteArray);

                return cipherText;
            }
            catch
            {
                throw;
            }
            finally
            {
                if (des != null)
                {
                    des.Dispose();
                }
            }
        }

        /// <summary>解密</summary>
        public static string Decrypt(string cipherText, string key, string iv)
        {
            DES des = null;
            try
            {
                des = DES.Create();
                des.Mode = CipherMode.CBC;
                des.Padding = PaddingMode.PKCS7;
                des.Key = Encoding.UTF8.GetBytes(key);
                des.IV = Encoding.UTF8.GetBytes(iv);

                byte[] cipherByteArray = Convert.FromBase64String(cipherText);
                byte[] plainByteArray = des.CreateDecryptor().TransformFinalBlock(cipherByteArray, 0, cipherByteArray.Length);

                string plainText = Encoding.UTF8.GetString(plainByteArray);

                return plainText;
            }
            catch
            {
                throw;
            }
            finally
            {
                if (des != null)
                {
                    des.Dispose();
                }
            }
        }
    }
}
