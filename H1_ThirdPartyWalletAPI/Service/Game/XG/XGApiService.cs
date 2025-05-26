using H1_ThirdPartyWalletAPI.Code;
using H1_ThirdPartyWalletAPI.Extensions;
using H1_ThirdPartyWalletAPI.Model.Config;
using H1_ThirdPartyWalletAPI.Model.Game.XG.Request;
using H1_ThirdPartyWalletAPI.Model.Game.XG.Response;
using H1_ThirdPartyWalletAPI.Service.Game.XG.JsonConverter;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using H1_ThirdPartyWalletAPI.Extensions;
using System.Threading.Tasks;

namespace H1_ThirdPartyWalletAPI.Service.Game.XG
{
    public class XGApiService : IXGApiService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<XGApiService> _logger;

        public XGApiService(IHttpClientFactory httpClientFactory, ILogger<XGApiService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        /// <summary>
        /// 會員 創建會員帳號 /api/keno-api/xg-casino/CreateMember
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<CreateMemberResponse> CreateMember(CreateMemberRequest request)
        {
            var url = Config.GameAPI.XG_URL + "api/keno-api/xg-casino/CreateMember";
            return await PostAsync<CreateMemberRequest, CreateMemberResponse>(url, request);
        }

        /// <summary>
        /// 會員 取得登入連結 /api/keno-api/xg-casino/Login
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<LoginResponse> Login(LoginRequest request)
        {
            var url = Config.GameAPI.XG_URL + "api/keno-api/xg-casino/Login";
            return await GetAsync<LoginRequest, LoginResponse>(url, request);
        }

        /// <summary>
        /// 會員 踢線 /api/keno-api/xg-casino/KickMember
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<KickMemberResponse> KickMember(KickMemberRequest request)
        {
            var url = Config.GameAPI.XG_URL + "api/keno-api/xg-casino/KickMember";
            return await PostAsync<KickMemberRequest, KickMemberResponse>(url, request);
        }

        /// <summary>
        /// 會員 取得會員資料 /api/keno-api/xg-casino/Account
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<AccountResponse> Account(AccountRequest request)
        {
            var url = Config.GameAPI.XG_URL + "api/keno-api/xg-casino/Account";
            return await GetAsync<AccountRequest, AccountResponse>(url, request);
        }

        /// <summary>
        /// 會員 取得會員限注 /api/keno-api/xg-casino/Template
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<GetTemplateResponse> GetTemplate(GetTemplateRequest request)
        {
            var url = Config.GameAPI.XG_URL + "api/keno-api/xg-casino/Template";
            return await GetAsync<GetTemplateRequest, GetTemplateResponse>(url, request);
        }

        /// <summary>
        /// 會員 設定會員限注 /api/keno-api/xg-casino/Template
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<SetTemplateResponse> SetTemplate(SetTemplateRequest request)
        {
            var url = Config.GameAPI.XG_URL + "api/keno-api/xg-casino/Template";
            return await PostAsync<SetTemplateRequest, SetTemplateResponse>(url, request);
        }

        /// <summary>
        /// 轉帳 會員轉帳 /api/keno-api/xg-casino/Transfer
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<TransferResponse> Transfer(TransferRequest request)
        {
            var url = Config.GameAPI.XG_URL + "api/keno-api/xg-casino/Transfer";
            return await PostAsync<TransferRequest, TransferResponse>(url, request);
        }

        /// <summary>
        /// 轉帳 取得單筆轉帳資料 /api/keno-api/xg-casino/CheckTransfer
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<CheckTransferResponse> CheckTransfer(CheckTransferRequest request)
        {
            var url = Config.GameAPI.XG_URL + "api/keno-api/xg-casino/CheckTransfer";
            return await PostAsync<CheckTransferRequest, CheckTransferResponse>(url, request);
        }

        /// <summary>
        /// 注單 取得會員下注內容 /api/keno-api/xg-casino/GetBetRecordByTime
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<GetBetRecordByTimeResponse> GetBetRecordByTime(GetBetRecordByTimeRequest request)
        {
            var url = Config.GameAPI.XG_URL + "api/keno-api/xg-casino/GetBetRecordByTime";
            return await PostAsync<GetBetRecordByTimeRequest, GetBetRecordByTimeResponse>(url, request);
        }

        /// <summary>
        /// 注單 注單編號查詢會員下注內容  /api/keno-api/xg-casino/GetGameDetailUrl
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<GetGameDetailUrlResponse> GetGameDetailUrl(GetGameDetailUrlRequest request)
        {
            var url = Config.GameAPI.XG_URL + "api/keno-api/xg-casino/GetGameDetailUrl";
            return await PostAsync<GetGameDetailUrlRequest, GetGameDetailUrlResponse>(url, request);
        }

        /// <summary>
        /// 注單 取得會員下注內容統計 /api/keno-api/xg-casino/GetGameDetailUrl
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<GetApiReportUrlResponse> GetApiReportUrl(GetApiReportUrlRequest request)
        {
            var url = Config.GameAPI.XG_URL + "api/keno-api/xg-casino/GetApiReportUrl";
            return await PostAsync<GetApiReportUrlRequest, GetApiReportUrlResponse>(url, request);
        }

        /// <summary>
        /// API健康檢查 /api/keno-api/xg-casino/Health 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<HealthResponse> Health(HealthRequest request)
        {
            var url = Config.GameAPI.XG_URL + "api/keno-api/xg-casino/Health";
            return await GetAsync<HealthRequest, HealthResponse>(url, request);
        }

        #region ApiHandle

        /// <summary>
        /// GetAsync
        /// </summary>
        /// <typeparam name="TRequest"></typeparam>
        /// <typeparam name="TResponse"></typeparam>
        /// <param name="url"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        /// <exception cref="TaskCanceledException"></exception>
        private async Task<TResponse> GetAsync<TRequest, TResponse>(string url, TRequest request)
        {
            try
            {
                using (var client = _httpClientFactory.CreateClient("log"))
                {
                    string queryStr = BuildQueryString(GetDictionary(request));
                    url += "?" + queryStr + "&Key=" + ComputeKey(queryStr);
                    client.Timeout = TimeSpan.FromSeconds(14);
                    var sw = System.Diagnostics.Stopwatch.StartNew();
                    var response = await client.GetAsync(Platform.XG, url);
                    sw.Stop();

                    response.EnsureSuccessStatusCode();
                    var body = await response.Content.ReadAsStringAsync();
                    try
                    {
                        var responselog = "";
                        if (body.Length > 10000)
                        {
                            responselog = body.Substring(0, 9999);
                        }
                        else
                        {
                            responselog = body;
                        }
                        var dics = new Dictionary<string, object>
                        {
                            { "request", request },
                            { "response", responselog }
                        };
                        using (var scope = _logger.BeginScope(dics))
                        {
                            _logger.LogInformation("Get RequestPath: {RequestPath} | ResponseHttpStatus:{Status} | exeTime:{exeTime} ms", url, response.StatusCode, sw.Elapsed.TotalMilliseconds);
                        }
                    }
                    catch
                    {
                    }

                    var result = JsonSerializer.Deserialize<TResponse>(body);
                    return result;
                }
            }
            catch (HttpRequestException ex)
            {
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                _logger.LogError("XG Get exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
                /*if (retry == 0)
                {
                    throw new Exception(string.Format("Call XGApi Failed:{0},reqJson:{1}", url, reqJson));
                }
                return await Post(url, httpMethod, reqJson, headers, retry - 1);*/
                throw ex;
            }
        }

        /// <summary>
        /// PostAsync
        /// </summary>
        /// <typeparam name="TRequest"></typeparam>
        /// <typeparam name="TResponse"></typeparam>
        /// <param name="url"></param>
        /// <param name="request"></param>
        /// <param name="responseLogFormat"></param>
        /// <returns></returns>
        protected async Task<TResponse> PostAsync<TRequest, TResponse>(string url, TRequest request, Func<string, string> responseLogFormat = null)
            where TRequest : Model.Game.XG.Request.BaseRequest
            where TResponse : Model.Game.XG.Response.BaseResponse
        {
            var client = _httpClientFactory.CreateClient("log");
            client.Timeout = TimeSpan.FromSeconds(14);
            request.Key = null;
            var formdata = GetDictionary(request);
            string queryStr = BuildQueryString(formdata);
            string key = ComputeKey(queryStr);
            request.Key = key;

            var jsonRequest = JsonSerializer.Serialize(request, new JsonSerializerOptions
            {
                Converters = {
                new SerializeDateTimeConverter()
            },
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            });
            _logger.LogInformation("XG Post RequestPath: {RequestPath} Body:{body}", url, jsonRequest);

            var sw = System.Diagnostics.Stopwatch.StartNew();
            var response = await client.PostAsync(Platform.XG, url, new StringContent(jsonRequest, Encoding.UTF8, "application/json"));
            sw.Stop();

            var body = await response.Content.ReadAsStringAsync();
            var dics = new Dictionary<string, object>();
            dics.Add("request", jsonRequest);
            dics.Add("response", responseLogFormat == null ? body : responseLogFormat(body));
            dics.Add("queryStr", queryStr);
            dics.Add("Key", key);

            using (var scope = _logger.BeginScope(dics))
            {
                _logger.LogInformation("XG Post RequestPath: {RequestPath} | ResponseHttpStatus:{Status} | exeTime:{exeTime} ms", url, response.StatusCode, sw.Elapsed.TotalMilliseconds);
            }
            var result = JsonSerializer.Deserialize<TResponse>(body);
            return result;
        }

        /// <summary>
        /// 每次呼叫都必須在請求參數內新加上一個Key參數
        /// 
        /// Key 產生方式
        /// Key = {6個任意字元} + MD5(所有請求參數串 + KeyG) + {6個任意字元}
        /// KeyG = MD5(DateTime.now().setZone("UTC-4").toString("yyMMd") + 公鑰(AgentId) + 私鑰(AgentKey))
        ///
        /// 請求參數串 依各 API 方法參數列表，務必按請求參數順序以 parameter1=value1&parameter2=value2&... 格式串起，API 端將會檢查請求參數串內容及順序，以確保請求參數未被竄改
        /// Key 任意字元 任意填入，前後各 6 字元不需相同，驗證時會去頭尾後比對中間加密部份
        /// KeyG 日期為當下 UTC-4 時間 & 日期格式為 yyMMd，例如： 2018/2/7 => 18027, 7 號是 7 而不是 07 2018/2/18 => 180218
        /// </summary>
        /// <param name="requestStr"></param>
        /// <returns></returns>
        private string ComputeKey(string requestStr)
        {
            using var md5 = MD5.Create();
            string date = DateTime.Now.AddHours(-12).ToString("yyMMd");
            byte[] plainByteArray = md5.ComputeHash(Encoding.UTF8.GetBytes(date + Config.CompanyToken.XG_AgentID + Config.CompanyToken.XG_AgentKey));
            string keyG = Convert.ToHexString(plainByteArray).ToLower();
            plainByteArray = md5.ComputeHash(Encoding.UTF8.GetBytes($"{requestStr}" + keyG));
            string key = "oooooo" + Convert.ToHexString(plainByteArray).ToLower() + "oooooo";

            return key;
        }

        /// <summary>
        /// 組合請求參數字串
        /// </summary>
        /// <typeparam name="TRequest"></typeparam>
        /// <param name="request"></param>
        /// <returns></returns>
        private string BuildQueryString<TRequest>(TRequest request)
        {
            Dictionary<string, string> props = request as Dictionary<string, string>;

            if (request is null)
                props = GetDictionary(request);

            return string.Join("&", props.Select(q => $"{q.Key}={q.Value}"));
        }

        /// <summary>
        /// 重整輸入參數
        /// </summary>
        /// <typeparam name="TRequest"></typeparam>
        /// <param name="request"></param>
        /// <param name="KeepNullField"></param>
        /// <returns></returns>
        private static Dictionary<string, string> GetDictionary<TRequest>(TRequest request, bool KeepNullField = false)
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

        #endregion
    }
}
