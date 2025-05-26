using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Web;
using System.Security.Cryptography;
using System.Text;
using System.IO;
using H1_ThirdPartyWalletAPI.Model.Config;
using H1_ThirdPartyWalletAPI.Code;
using H1_ThirdPartyWalletAPI.Extensions;

namespace H1_ThirdPartyWalletAPI.Service.Common
{

    public interface IHttpService
    {
        Task<T> GetAsync<T>(HttpService.GameProvider gameProvider, HttpService.API_Type apiType, Dictionary<string, string> argus);
        Task<T> PostAsnyc<T>(HttpService.GameProvider gameProvider, HttpService.API_Type apiType, string postBody = null, Dictionary<string, string> queryString = null);
        Task<T> PostAsnyc_urlencoded<T>(HttpService.GameProvider gameProvider, HttpService.API_Type apiType, string postBody = null, Dictionary<string, string> queryString = null);
        Task<T> PostAsnyc_rcg<T>(HttpService.GameProvider gameProvider, HttpService.API_Type apiType, string queryString = "");
        Task<T> PostAsnyc_streamer<T>(HttpService.GameProvider gameProvider, HttpService.API_Type apiType, string queryString = "");
        Task<T> PostAsnyc_FormUrlEncoded<T>(HttpService.GameProvider gameProvider, HttpService.API_Type apiType, Dictionary<string, string> postBody = null, Dictionary<string, string> queryString = null);
    }
    public class HttpService : IHttpService
    {
        private readonly IHttpClientFactory _httpFactory;
        private readonly ILogger<HttpService> _logger;
        private readonly IConfiguration _configuration;
        private readonly IApiHealthCheckService _apiHealthCheckService;
        public HttpService(IHttpClientFactory httpFactory
            , ILogger<HttpService> logger
            , IConfiguration configuration
            , IApiHealthCheckService apiHealthCheckService)
        {
            _httpFactory = httpFactory;
            _logger = logger;
            _configuration = configuration;
            _apiHealthCheckService = apiHealthCheckService;
        }
        public async Task<T> GetAsync<T>(GameProvider gameProvider, API_Type apiType, Dictionary<string, string> argus)
        {
            var url = this.API_TypeToUrl(gameProvider, apiType) + (argus != null ? this.DictionaryToQueryString(argus) : "");

            var httpClient = this._httpFactory.CreateClient();
            var sw = System.Diagnostics.Stopwatch.StartNew();
            var result = await httpClient.GetStringAsync(url);
            sw.Stop();
            this._logger.LogDebug("api：{url} , result ：{result} , {exeTime} ms", url, result, sw.Elapsed.TotalMilliseconds);

            var tType = typeof(T); // string 不是實值型別 https://stackoverflow.com/questions/636932/in-c-why-is-string-a-reference-type-that-behaves-like-a-value-type
            if (tType.IsValueType || tType.Equals(typeof(string)))
                return (T)Convert.ChangeType(result, tType);
            else
                return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(result);
        }
        public async Task<T> PostAsnyc<T>(GameProvider gameProvider, API_Type apiType, string postBody = null, Dictionary<string, string> queryString = null)
        {
            var apiResInfo = new ApiResponseData();
            try
            {
                string url = this.API_TypeToUrl(gameProvider, apiType);
                if (queryString != null)
                    url += this.DictionaryToQueryString(queryString);

                var httpClient = this._httpFactory.CreateClient();
                httpClient.Timeout = TimeSpan.FromSeconds(15);
                StringContent requestBody;
                if (string.IsNullOrEmpty(postBody) == false)
                    requestBody = new StringContent(postBody, System.Text.Encoding.UTF8, "application/json");
                else
                    requestBody = new StringContent("");

                var sw = System.Diagnostics.Stopwatch.StartNew();
                var httpResult = await httpClient.PostAsync(url, requestBody);
                sw.Stop();
                apiResInfo.ElapsedMilliseconds = sw.ElapsedMilliseconds;
                _apiHealthCheckService.SetResponseData(Platform.H1, apiResInfo);
                var result = await httpResult.Content.ReadAsStringAsync();

                var dic = new Dictionary<string, object>();
                dic.Add("request", postBody);
                dic.Add("response", result);
                using (var scope = this._logger.BeginScope(dic))
                {
                    this._logger.LogInformation("api：{url}, {exeTime} ms", url, sw.Elapsed.TotalMilliseconds);
                }

                if (httpResult.IsSuccessStatusCode == false)
                    throw new Exception($"呼叫 APi 失敗：{apiType.ToString()}");

                var tType = typeof(T);
                if (tType.IsValueType || tType.Equals(typeof(string)))
                    return (T)Convert.ChangeType(result, tType);
                else
                    return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(result);
            }
            catch (TaskCanceledException ex)
            {
                apiResInfo.ElapsedMilliseconds = 99999;
                _apiHealthCheckService.SetResponseData(Platform.H1, apiResInfo);
                throw new TaskCanceledException(ex.ToString());
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }
        public async Task<T> PostAsnyc_FormUrlEncoded<T>(GameProvider gameProvider, API_Type apiType, Dictionary<string, string> postBody = null, Dictionary<string, string> queryString = null)
        {
            var apiResInfo = new ApiResponseData();
            try
            {
                string url = this.API_TypeToUrl(gameProvider, apiType);
                if (queryString != null)
                    url += this.DictionaryToQueryString(queryString);

                var formData = new FormUrlEncodedContent(postBody);

                var httpClient = this._httpFactory.CreateClient();
                httpClient.Timeout = TimeSpan.FromSeconds(14);
                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/x-www-form-urlencoded");

                var sw = System.Diagnostics.Stopwatch.StartNew();
                var httpResult = await httpClient.PostAsync(url, formData);
                sw.Stop();
                apiResInfo.ElapsedMilliseconds = sw.ElapsedMilliseconds;
                _apiHealthCheckService.SetResponseData(Platform.SABA, apiResInfo);
                var result = await httpResult.Content.ReadAsStringAsync();
                var dic = new Dictionary<string, object>();
                dic.Add("request", postBody);
                dic.Add("response", result);

                using (var scope = this._logger.BeginScope(dic))
                {
                    this._logger.LogInformation("api：{url}, {exeTime} ms", url, sw.Elapsed.TotalMilliseconds);
                }
                if (httpResult.IsSuccessStatusCode == false)
                    throw new Exception($"呼叫 APi 失敗：{apiType.ToString()}");
                var tType = typeof(T);
                if (tType.IsValueType || tType.Equals(typeof(string)))
                    return (T)Convert.ChangeType(result, tType);
                else
                    return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(result);
            }
            catch (TaskCanceledException ex)
            {
                apiResInfo.ElapsedMilliseconds = 99999;
                _apiHealthCheckService.SetResponseData(Platform.SABA, apiResInfo);
                throw new TaskCanceledException(ex.ToString());
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }
        public async Task<T> PostAsnyc_rcg<T>(GameProvider gameProvider, API_Type apiType, string queryString = "")
        {
            string RCG_Key = Config.CompanyToken.RCG_Key;
            string RCG_IV = Config.CompanyToken.RCG_IV;
            string RCG_Clinet_id = Config.CompanyToken.RCG_Token;
            string RCG_Secret = Config.CompanyToken.RCG_Secret;
            var unixTimestamp = DateTimeOffset.Now.ToUnixTimeSeconds().ToString();
            if (queryString != null)
            {
                queryString = desEncryptBase64(queryString, RCG_Key, RCG_IV);
            }
            string MD5String = RCG_Clinet_id + RCG_Secret + unixTimestamp.ToString() + queryString;
            string MD5CheckSum = GetMd5Hash(MD5String);
            string url = this.API_TypeToUrl(gameProvider, apiType);
            var httpClient = this._httpFactory.CreateClient("log");
            httpClient.Timeout = TimeSpan.FromSeconds(14);
            httpClient.DefaultRequestHeaders.Add("X-API-ClientID", RCG_Clinet_id);
            httpClient.DefaultRequestHeaders.Add("X-API-Signature", MD5CheckSum);
            httpClient.DefaultRequestHeaders.Add("X-API-Timestamp", unixTimestamp);

            StringContent requestBody;
            requestBody = new StringContent($"{HttpUtility.UrlEncode(queryString)}", Encoding.UTF8, "application/json");

            var sw = System.Diagnostics.Stopwatch.StartNew();
            var httpResult = await httpClient.PostAsync(Platform.RCG, url, requestBody);
            sw.Stop();
            var result = await httpResult.Content.ReadAsStringAsync();
            result = desDecryptBase64(result, RCG_Key, RCG_IV);
            try
            {
                var dic = new Dictionary<string, object>();
                //截斷過長注單log
                if (apiType == API_Type.GetBetRecordList)
                {
                    var response = "";
                    if (result.Length > 10000)
                    {
                        response = result.Substring(0, 9999);
                    }
                    else
                    {
                        response = result;
                    }
                    dic.Add("response", response);
                }
                else
                {
                    dic.Add("response", result);
                }
                dic.Add("request", queryString);

                using (var scope = this._logger.BeginScope(dic))
                {
                    this._logger.LogInformation("api：{url}, {exeTime} ms", url, sw.Elapsed.TotalMilliseconds);
                }
            }
            catch (Exception ex)
            {
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                _logger.LogError(ex, "log exception EX : {ex}  MSG : {Message} Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
            }

            if (httpResult.IsSuccessStatusCode == false && apiType != API_Type.GetOpenList)
                throw new Exception($"呼叫 APi 失敗：{apiType.ToString()} StatusCode: {httpResult.StatusCode}");
            var tType = typeof(T);
            if (tType.IsValueType || tType.Equals(typeof(string)))
                return (T)Convert.ChangeType(result, tType);
            else
                return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(result);
        }
        public async Task<T> PostAsnyc_streamer<T>(GameProvider gameProvider, API_Type apiType, string queryString = "")
        {
            var apiResInfo = new ApiResponseData();
            try
            {
                string RCG_Key = Config.CompanyToken.STREAMER_Key;
                string RCG_IV = Config.CompanyToken.STREAMER_IV;
                string RCG_Clinet_id = Config.CompanyToken.STREAMER_Token;
                string RCG_Secret = Config.CompanyToken.STREAMER_Secret;
                var unixTimestamp = DateTimeOffset.Now.ToUnixTimeSeconds().ToString();
                if (queryString != null)
                {
                    queryString = desEncryptBase64(queryString, RCG_Key, RCG_IV);
                }
                string MD5String = RCG_Clinet_id + RCG_Secret + unixTimestamp.ToString() + queryString;
                string MD5CheckSum = GetMd5Hash(MD5String);
                string url = this.API_TypeToUrl(gameProvider, apiType);
                var httpClient = this._httpFactory.CreateClient();
                httpClient.Timeout = TimeSpan.FromSeconds(14);
                httpClient.DefaultRequestHeaders.Add("X-API-ClientID", RCG_Clinet_id);
                httpClient.DefaultRequestHeaders.Add("X-API-Signature", MD5CheckSum);
                httpClient.DefaultRequestHeaders.Add("X-API-Timestamp", unixTimestamp);

                StringContent requestBody;
                requestBody = new StringContent($"{HttpUtility.UrlEncode(queryString)}", Encoding.UTF8, "application/json");

                var sw = System.Diagnostics.Stopwatch.StartNew();
                var httpResult = await httpClient.PostAsync(url, requestBody);
                sw.Stop();
                apiResInfo.ElapsedMilliseconds = sw.ElapsedMilliseconds;
                _apiHealthCheckService.SetResponseData(Platform.STREAMER, apiResInfo);
                var result = await httpResult.Content.ReadAsStringAsync();
                result = desDecryptBase64(result, RCG_Key, RCG_IV);
                var dic = new Dictionary<string, object>();
                dic.Add("request", queryString);
                dic.Add("response", result);
                using (var scope = this._logger.BeginScope(dic))
                {
                    this._logger.LogInformation("api：{url}, {exeTime} ms", url, sw.Elapsed.TotalMilliseconds);
                }
                if (httpResult.IsSuccessStatusCode == false)
                    throw new Exception($"呼叫 APi 失敗：{apiType.ToString()}");
                var tType = typeof(T);
                if (tType.IsValueType || tType.Equals(typeof(string)))
                    return (T)Convert.ChangeType(result, tType);
                else
                    return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(result);
            }
            catch (TaskCanceledException ex)
            {
                apiResInfo.ElapsedMilliseconds = 99999;
                if (apiType != API_Type.GetBetRecordList)
                {
                    _apiHealthCheckService.SetResponseData(Platform.STREAMER, apiResInfo);
                }
                throw new TaskCanceledException(ex.ToString());
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }
        public async Task<T> PostAsnyc_urlencoded<T>(GameProvider gameProvider, API_Type apiType, string postBody = null, Dictionary<string, string> queryString = null)
        {
            string url = this.API_TypeToUrl(gameProvider, apiType);

            string QueryString = this.DictionaryToQueryString(queryString);

            if (queryString != null)
                url += HttpUtility.UrlEncode(QueryString);

            var httpClient = this._httpFactory.CreateClient();
            httpClient.DefaultRequestHeaders.Add("X-API-ClientID", "");
            httpClient.DefaultRequestHeaders.Add("X-API-Signature", "");
            httpClient.DefaultRequestHeaders.Add("X-API-Timestamp", "");

            StringContent requestBody;
            if (string.IsNullOrEmpty(postBody) == false)
                requestBody = new StringContent(postBody, System.Text.Encoding.UTF8, "application/json");
            else
                requestBody = new StringContent("");

            var sw = System.Diagnostics.Stopwatch.StartNew();
            var httpResult = await httpClient.PostAsync(url, requestBody);
            sw.Stop();


            if (httpResult.IsSuccessStatusCode == false)
                throw new Exception($"呼叫 APi 失敗：{apiType.ToString()}");

            var result = await httpResult.Content.ReadAsStringAsync();

            this._logger.LogDebug("api：{url} , result ：{result} , {exeTime} ms", url, result, sw.Elapsed.TotalMilliseconds);

            var tType = typeof(T);
            if (tType.IsValueType || tType.Equals(typeof(string)))
                return (T)Convert.ChangeType(result, tType);
            else
                return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(result);
        }
        private string desEncryptBase64(string source, string key, string iv)
        {
            DESCryptoServiceProvider des = new DESCryptoServiceProvider();
            byte[] bkey = Encoding.ASCII.GetBytes(key);
            byte[] biv = Encoding.ASCII.GetBytes(iv);
            byte[] dataByteArray = Encoding.UTF8.GetBytes(source);

            des.Key = bkey;
            des.IV = biv;
            string encrypt = "";
            using (MemoryStream ms = new MemoryStream())
            using (CryptoStream cs = new CryptoStream(ms, des.CreateEncryptor(), CryptoStreamMode.Write))
            {
                cs.Write(dataByteArray, 0, dataByteArray.Length);
                cs.FlushFinalBlock();
                encrypt = Convert.ToBase64String(ms.ToArray());
            }
            return encrypt;
        }
        private string desDecryptBase64(string encrypt, string key, string iv)
        {
            DESCryptoServiceProvider des = new DESCryptoServiceProvider();
            byte[] bkey = Encoding.ASCII.GetBytes(key);
            byte[] biv = Encoding.ASCII.GetBytes(iv);
            des.Key = bkey;
            des.IV = biv;

            byte[] dataByteArray = Convert.FromBase64String(encrypt);
            using (MemoryStream ms = new MemoryStream())
            {
                using (CryptoStream cs = new CryptoStream(ms, des.CreateDecryptor(), CryptoStreamMode.Write))
                {
                    cs.Write(dataByteArray, 0, dataByteArray.Length);
                    cs.FlushFinalBlock();
                    return Encoding.UTF8.GetString(ms.ToArray());
                }
            }
        }
        private string GetMd5Hash(string input)
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
        private string API_TypeToUrl(GameProvider gameProvider, API_Type apiType)
        {
            string apiURL = "GameAPI:";

            switch (gameProvider)
            {
                case GameProvider.SABA:
                    apiURL += "SABA_URL";
                    break;
                case GameProvider.SBO:
                    apiURL += "SBO_URL";
                    break;
                case GameProvider.RCG_PLAYER:
                    apiURL += "RCG_PLAYER_URL";
                    break;
                case GameProvider.RCG_RECORD:
                    apiURL += "RCG_RECORD_URL";
                    break;
                case GameProvider.RCG_H1:
                    apiURL += "RCG_H1_URL";
                    break;
                case GameProvider.PG_Login:
                    apiURL += "PG_LOGIN_URL";
                    break;
                case GameProvider.PG_Cash:
                    apiURL += "PG_CASH_URL";
                    break;
                case GameProvider.PG_Player:
                    apiURL += "PG_PLAYER_URL";
                    break;
                case GameProvider.PG_Create:
                    apiURL += "PG_CREATE_URL";
                    break;
                case GameProvider.PG_SOFT_API:
                    apiURL += "PG_SoftAPIDomain";
                    break;
                case GameProvider.PG_DATA_GRAB:
                    apiURL += "PG_DataGrabAPIDomain";
                    break;
                case GameProvider.PG_SOFT_PUBLIC:
                    apiURL += "PG_SoftPublicDomain";
                    break;
                case GameProvider.PG_HISTORY_INTERPRETER:
                    apiURL += "PG_HistoryInterpreter";
                    break;
                case GameProvider.PG_LAUNCH:
                    apiURL += "PG_LaunchURL";
                    break;
                case GameProvider.H1:
                    apiURL += "H1_URL";
                    break;
                case GameProvider.H1_HEALTH:
                    apiURL += "H1_URL_Health";
                    break;
                case GameProvider.STREAMER:
                    apiURL += "STREAMER_URL";
                    break;
                case GameProvider.SABA2:
                    apiURL += "SABA2_URL";
                    break;
                default:
                    throw new NotSupportedException(string.Format("GameProvider:{0}", gameProvider));

            }
            var url = this._configuration.GetValue<string>(apiURL);
            url = System.IO.Path.Combine(url, $"{apiType.ToString().Replace("_", @"/")}");
            return url;
        }
        private string DictionaryToQueryString(Dictionary<string, string> dicArgus)
        {
            var ar = (from item in dicArgus
                      select $"{item.Key}={item.Value}").ToArray();

            if (ar.Length > 0)
                return "?" + string.Join('&', ar);
            else
                return string.Empty;
        }
        public enum GameProvider
        {
            //SBO
            SBO,
            //SBA
            SABA,
            //RCG
            RCG_PLAYER,
            RCG_RECORD,
            RCG_H1,
            //PG
            PG_Login,
            PG_Cash,
            PG_Player,
            PG_Create,
            PG_SOFT_API,
            PG_DATA_GRAB,
            PG_SOFT_PUBLIC,
            PG_HISTORY_INTERPRETER,
            PG_LAUNCH,
            //JDB
            JDB,
            //H1
            H1,
            H1_HEALTH,
            //STREAMER
            STREAMER,
            SABA2
        }
        public enum API_Type
        {
            //SABA
            CreateMember = 01001,//建立會員
            UpdateMember, //更新會員資訊
            KickUser, //踢出會員
            CheckIsOnline, //檢查會員在線
            CheckUserBalance,//取得會員餘額
            FundTransfer, //遊戲商轉帳
            CheckFundTransfer, //確認轉帳狀態
            GetBetDetail, //取得注單
            SetMemberBetSetting, //設定會員限額
            SetMemberBetSettingBySubsidiary, //設定下線會員限額
            GetSabaUrl, //取得登入連結
            GetBetDetailByTimeframe, //依日期時間取得注單
            GetBetDetailByTransID, //依注單號碼取得注單
            GetMaintenanceTime,//取得維護時間
            GetOnlineUserCount,
            GetFinancialReport,
            GetBetSettingLimit,
            //RCG
            Login = 02001, //取得登入遊戲連結
            CreateOrSetUser, //建立或編輯會員
            KickOut, //踢出會員
            KickOutByCompany,
            GetBetLimit, //設定會員限注
            GetBalance, //取得餘額
            GetPlayerOnlineList,
            Deposit,
            Withdraw,
            GetBetRecordList, //取得注單
            GetBetRecordListByDateRange, //取得注單 /api/H1/GetBetRecordListByDateRange
            GetGameDeskList,
            GetChangeRecordList, //取得改牌紀錄
            GetTransactionLog, //取得轉帳紀錄
            GetOpenList, //取得開牌紀錄
            GetMaintenanceInfo, //取得維護時間
            //PG
            LoginGame,
            GetPlayerWallet,
            TransferIn,
            TransferOut,
            Create,
            Kick,
            Suspend,
            Reinstate,
            Check,
            TransferAllOut,
            GetSingleTransaction,
            LoginProxy,
            GetPlayersWallet,
            //H1
            RefundAmount,
            SettleBetRecord,
            HealthCheck,
            WalletTransferOut,
            //STREAMER
            H1CreateOrSetUser,
            ForwardGameURL
        }
        public virtual Dictionary<string, string> GetRequestDictionary<T>(T request)
        {
            var Properties = typeof(T).GetProperties();

            //var dic = new Dictionary<string, string>();
            //foreach (var item in Properties)
            //{
            //    dic.Add(item.Name, item.GetValue(request).ToString());
            //}
            var dic = Properties.ToDictionary(x => { return x.Name.Substring(0, 1).ToLower() + x.Name.Substring(1); }, y => y.GetValue(request).ToString());
            return dic;
        }
    }
}
