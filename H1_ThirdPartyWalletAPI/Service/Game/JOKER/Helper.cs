using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace H1_ThirdPartyWalletAPI.Service.Game.JOKER;

public static class Helper
{
	/// <summary>
    /// 轉換成 Key=Value
    /// </summary>
    public static string ConvertToKeyValue<T>(T source) where T : class
    {
        var type = source.GetType();
        var properties = type.GetProperties();
        var list = properties.OrderBy(x => x.Name).Select(x => x.Name + "=" + x.GetValue(source)).ToList();
        return string.Join("&", list);
    }

    /// <summary>
    /// 簽名
    /// </summary>
    public static string GetHMACSHA1Signature(string rawData, string secretKey)
    {
        using (var sha = new HMACSHA1(Encoding.UTF8.GetBytes(secretKey)))
        {
            var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(rawData));
            return Convert.ToBase64String(hash);
        }
    }

    public static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Local);
    /// <summary>
    /// 時間戳
    /// </summary>
    public static int GetCurrentTimestamp()
    {
        return (int)DateTime.UtcNow.Subtract(UnixEpoch).TotalSeconds;
    }
}