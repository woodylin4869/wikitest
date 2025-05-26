using System.Security.Cryptography;
using System.Text;
using ThirdPartyWallet.Share.Model.Game.WE;

namespace ThirdPartyWallet.GameAPI.Service.Game.WE;
public static class Halper
{
    /// <summary>
    /// MD5加密
    /// </summary>
    /// <param name="parameter"></param>
    /// <returns></returns>
    public static string MD5encryption(string parameter)
    {
        using (MD5 md5 = MD5.Create())
        {
            // 將輸入字符串轉換為字節數組並計算其哈希值
            byte[] data = md5.ComputeHash(Encoding.UTF8.GetBytes(parameter));
            // 創建一個StringBuilder來收集字節並創建字符串
            StringBuilder sBuilder = new StringBuilder();
            // 遍歷每個字節的數據並格式化為十六進制字符串
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }
            // 返回十六進制字符串
            return sBuilder.ToString();
        }
    }

    // 通過WEConfig模型獲取appSecret
    private static string GetAppSecret()
    {
        WEConfig config = new WEConfig();  // 假定在某處已實例化並設定好
        return config.WE_appSecret;  // 從WEConfig獲取appSecret
    }
    /// <summary>
    /// 数据模型对象转换为可以用于例如 API 调用的键值对形式
    /// </summary>
    /// <typeparam name="TRequest"></typeparam>
    /// <param name="request"></param>
    /// <param name="KeepNullField"></param>
    /// <returns></returns>
    public static Dictionary<string, string> GetDictionary<TRequest>(TRequest request, bool KeepNullField = false)
    {
        var props = typeof(TRequest).GetProperties();
        var param = new Dictionary<string, string>();

        foreach (var prop in props)
        {
            var propName = prop.Name;
            string propValue = prop.PropertyType == typeof(DateTime) ? ((DateTime)prop.GetValue(request)).ToString("yyyy-MM-ddTHH:mm:ss") : prop.GetValue(request)?.ToString();

            if (KeepNullField || propValue is not null)
                param.Add(propName, propValue);
        }

        return param;
    }

    /// <summary>
    /// SHA256
    /// </summary>
    /// <param name="rawData"></param>
    /// <returns></returns>
    public static string ComputeSha256Hash(string rawData)
    {
        // 使用 SHA256 創建哈希
        using (SHA256 sha256Hash = SHA256.Create())
        {
            // 計算哈希值
            byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));

            // 將字節數組轉換為十六進制字符串
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < bytes.Length; i++)
            {
                builder.Append(bytes[i].ToString("x2"));
            }
            return builder.ToString();
        }
    }



}
