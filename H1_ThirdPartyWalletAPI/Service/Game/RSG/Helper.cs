using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace H1_ThirdPartyWalletAPI.Service.Game.RSG
{
    public static class Helper
    {
		/// <summary>
        /// 判斷作業系統，取得台灣標準時間
        /// </summary>
        /// <returns></returns>
        private static DateTime GetTaiwanDateTime()
        {
            var taipeiStandardTime = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ?
                "Taipei Standard Time" : "Asia/Taipei";

            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById(taipeiStandardTime));
        }

        /// <summary>
        /// 取得 Timestamp
        /// </summary>
        /// <returns></returns>
        public static long GetTimestamp()
        {
            var jan1St1970 = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var datetime = GetTaiwanDateTime();

            var timestamp = (long)(datetime - jan1St1970).TotalSeconds;
            return timestamp;
        }

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
                byte[] cipherByteArray = des.CreateEncryptor().TransformFinalBlock(plainByteArray, 0,
                    plainByteArray.Length);
                string cipherText = Convert.ToBase64String(cipherByteArray);
                return cipherText;
            }
        }
        /// <summary>
        /// 解密
        /// </summary>
        public static string DESDecrypt(string text, string key, string iv)
        {
            using (var des = DES.Create())
            {
                des.Mode = CipherMode.CBC;
                des.Padding = PaddingMode.PKCS7;
                des.Key = Encoding.UTF8.GetBytes(key);
                des.IV = Encoding.UTF8.GetBytes(iv);
                byte[] plainByteArray = Convert.FromBase64String(text);
                byte[] cipherByteArray = des.CreateDecryptor().TransformFinalBlock(plainByteArray, 0,
                    plainByteArray.Length);
                string cipherText = Encoding.UTF8.GetString(cipherByteArray);
                return cipherText;
            }
        }

        /// <summary>
        /// 產生簽章
        /// </summary>
        public static string CreateSignature(string clientID, string clientSecret, string timestamp, string
            encryptData)
        {
            using (var md5 = MD5.Create())
            {
                var inputText = clientID + clientSecret + timestamp + encryptData;
                var inputByteArray = Encoding.UTF8.GetBytes(inputText);
                var outputByteArray = md5.ComputeHash(inputByteArray);
                var outputText = ByteToHexBitFiddle(outputByteArray);
                return outputText;
            }
        }

        private static string ByteToHexBitFiddle(byte[] bytes)
        {
            char[] c = new char[bytes.Length * 2];
            int b;
            for (int i = 0; i < bytes.Length; i++)
            {
                b = bytes[i] >> 4;
                c[i * 2] = (char)(55 + b + (((b - 10) >> 31) & -7));
                b = bytes[i] & 0xF;
                c[i * 2 + 1] = (char)(55 + b + (((b - 10) >> 31) & -7));
            }
            return new string(c);
        }
    }
}