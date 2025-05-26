using System.Security.Cryptography;
using System.Text;

namespace H1_ThirdPartyWalletAPI.Service.Game.Ps
{
    public static class Helper
    {
        public static string MD5encryption(string parameter, string parametertwo, string parameterthree)
        {
            using var md5 = MD5.Create();
            byte[] plainByteArray = md5.ComputeHash(Encoding.UTF8.GetBytes($"{parameter}{parametertwo}{parameterthree}"));
            string cipherText = Convert.ToHexString(plainByteArray).ToLower();

            return cipherText;
        }
    }
}
