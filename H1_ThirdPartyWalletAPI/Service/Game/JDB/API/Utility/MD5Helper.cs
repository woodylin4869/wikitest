using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;


namespace H1_ThirdPartyWalletAPI.Service.Game.JDB.API.Utility
{
    public class MD5Helper
    {
        public static void Encrypt(string plainText, out string outCipherText,  out string MD5Result)
        {
            outCipherText = null;
            MD5 md5 = null;

            try
            {
                md5 = MD5.Create();
                byte[] plainByteArray = md5.ComputeHash(Encoding.UTF8.GetBytes(plainText));
               
                MD5Result = BitConverter.ToString(plainByteArray).Replace("-", "");

                string cipherText = Convert.ToBase64String(plainByteArray);

                outCipherText = cipherText;

            }
            catch
            {
                throw;
            }
            finally
            {
                if (md5 != null)
                {
                    md5.Dispose();
                }
            }
        }

        /// <summary>加密</summary>
        public static string Encrypt(string plainText)
        {
            MD5 md5 = null;

            try
            {
                md5 = MD5.Create();
                byte[] plainByteArray = md5.ComputeHash(Encoding.UTF8.GetBytes(plainText));
                string cipherText = Convert.ToBase64String(plainByteArray);

                return cipherText;
            }
            catch
            {
                throw;
            }
            finally
            {
                if (md5 != null)
                {
                    md5.Dispose();
                }
            }
        }
    }
}
