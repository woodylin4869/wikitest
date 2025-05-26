using Serilog;
using System.Security.Cryptography;
using System.Text;

namespace ThirdPartyWallet.GameAPI.Service.Game.EGSlot;
public static class Helper
{
    /// <summary>
    /// 轉換成 Key=Value
    /// </summary>
    public static string ConvertToKeyValue<T>(T source, params string[] excludedProperties) where T : class
    {
        var type = source.GetType();
        var properties = type.GetProperties()
            .Where(prop => !excludedProperties.Contains(prop.Name));  // Filter out excluded properties

        var keyValuePairs = properties
            .Select(prop => prop.Name + "=" + prop.GetValue(source, null))
            .ToList();
        return string.Join("&", keyValuePairs);
    }
    /// <summary>
    /// 進線hash組成
    /// </summary>
    /// <param name="data"></param>
    /// <param name="secretKey"></param>
    /// <returns></returns>
    public static string CreateHMACSHA256(string data, string secretKey)
    {
        using (HMACSHA256 hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secretKey)))
        {
            byte[] dataBytes = Encoding.UTF8.GetBytes(data);
            byte[] hashBytes = hmac.ComputeHash(dataBytes);

            // 將結果轉換為16進制字符串
            StringBuilder hex = new StringBuilder(hashBytes.Length * 2);
            foreach (byte b in hashBytes)
            {
                hex.AppendFormat("{0:x2}", b);
            }
            return hex.ToString();
        }
    }

    public static string ReplaceDomain(string originalUrl, string newDomain)
    {
        try
        {
            Uri uri = new Uri(originalUrl);
            string pathAndQuery = uri.PathAndQuery;
            string updatedUrl = newDomain.TrimEnd('/') + pathAndQuery;
            return updatedUrl;
        }
        catch (UriFormatException)
        {
            Log.Warning("Invalid URL format: {OriginalUrl}", originalUrl);
            return null;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Unexpected error while replacing domain.");
            return null;
        }
    }
}
