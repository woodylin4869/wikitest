using H1_ThirdPartyWalletAPI.Model.Config;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace H1_ThirdPartyWalletAPI.Service.Game.FC.Utility
{
    public class MD5Helper
    {
        public void Encrypt(string plainText, out string outCipherText, out string MD5Result)
        {
            outCipherText = null;
            MD5 md5 = null;

            try
            {
                md5 = MD5.Create();
                byte[] plainByteArray = md5.ComputeHash(Encoding.UTF8.GetBytes(plainText));

                MD5Result = BitConverter.ToString(plainByteArray).Replace("-", "");

                string cipherText = Convert.ToHexString(plainByteArray);

                outCipherText = cipherText.ToLower();

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
                string cipherText = Convert.ToHexString(plainByteArray);

                return cipherText.ToLower();
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


        public static string GetMd5Hash(string input)
        {
            using (MD5 md5Hash = MD5.Create())
            {
                // Convert the input string to a byte array and compute the hash.
                byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));
                string cipherText = Convert.ToBase64String(data);
                // Return the hexadecimal string.
                return cipherText;
            }
        }


        public static string GetFCTrsID()
        {
            string GUID = Guid.NewGuid().ToString();
            string MD5Code = GetMd5Hash(GUID);
            string strKeyWord = Regex.Replace(MD5Code, @"[^a-zA-Z0-9]", "");
            return strKeyWord;
        }


        /// <summary>
        /// 縮短GUID長度
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public static string CutGuidTo30Characters(Guid guid)
        {
            return guid.ToString("N")[2..];
        }
    }
}
