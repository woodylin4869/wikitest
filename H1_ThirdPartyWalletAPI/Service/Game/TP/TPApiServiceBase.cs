using H1_ThirdPartyWalletAPI.Code;
using H1_ThirdPartyWalletAPI.Extensions;
using H1_ThirdPartyWalletAPI.Model.Config;
using H1_ThirdPartyWalletAPI.Model.Game.TP.Response;
using H1_ThirdPartyWalletAPI.Service.Common;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace H1_ThirdPartyWalletAPI.Service.Game.TP
{
    public class TPApiServiceBase
    {
        private readonly ILogger logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private string API_KEY { get; set; }
        private string API_TOKEN { get; set; }

        public TPApiServiceBase(ILogger logger, IHttpClientFactory httpClientFactory)
        {
            this.logger = logger;
            this._httpClientFactory = httpClientFactory;

            this.API_TOKEN = Config.CompanyToken.TP_Token;
            this.API_KEY = Config.CompanyToken.TP_Key;

        }

        protected Task<TpResponse<TResponse>> GetAsync<TResponse>(string url, Func<string, string> responseLogFormat = null)
        {
            return GetAsync<object, TResponse>(url, new object(), responseLogFormat);
        }

        protected async Task<TpResponse<TResponse>> GetAsync<TRequest, TResponse>(string url, TRequest request, Func<string, string> responseLogFormat = null)
        {
            var apiResInfo = new ApiResponseData();
            try
            {
                var client = _httpClientFactory.CreateClient("log");
                client.Timeout = TimeSpan.FromSeconds(14);
                SetTpHeader(ref client);

                var query = GetDictionary(request);
                query.Add("sign", ComputeSign(query));
                url += "?" + string.Join("&", query.Select(q => $"{q.Key}={q.Value}"));

                logger.LogInformation("Tp Get RequestPath: {RequestPath}", url);

                var sw = System.Diagnostics.Stopwatch.StartNew();
                var response = await client.GetAsync(Platform.TP, url);
                sw.Stop();
                apiResInfo.ElapsedMilliseconds = sw.ElapsedMilliseconds;

                if (!response.IsSuccessStatusCode)
                    throw new Exception(string.Format("Call TPApi Failed! url:{0} status:{1}", url, response.StatusCode.ToString()));

                var body = await response.Content.ReadAsStringAsync();
                var dics = new Dictionary<string, object>();

                dics.Add("request", string.Empty);
                dics.Add("response", responseLogFormat == null ? body : responseLogFormat(body));
                dics.Add("API_KEY", API_KEY);
                using (var scope = logger.BeginScope(dics))
                {
                    logger.LogInformation("Tp Get RequestPath: {RequestPath} | ResponseHttpStatus:{Status} | exeTime:{exeTime} ms", url, response.StatusCode, sw.Elapsed.TotalMilliseconds);
                }
                var result = JsonConvert.DeserializeObject<TpResponse<TResponse>>(body);
                return result;

            }
            catch (TaskCanceledException ex)
            {
                apiResInfo.ElapsedMilliseconds = 99999;
                throw ex;
            }
        }

        protected async Task<Stream> GetAsStreamAsync<TRequest>(string url, TRequest request)
        {
            var apiResInfo = new ApiResponseData();
            try
            {
                var client = _httpClientFactory.CreateClient("log");
                client.Timeout = TimeSpan.FromSeconds(14);
                SetTpHeader(ref client);

                var query = GetDictionary(request);
                query.Add("sign", ComputeSign(query));
                url += "?" + string.Join("&", query.Select(q => $"{q.Key}={q.Value}"));

                logger.LogInformation("Tp Get RequestPath: {RequestPath}", url);

                var sw = System.Diagnostics.Stopwatch.StartNew();
                var response = await client.GetAsync(Platform.TP, url);
                sw.Stop();
                apiResInfo.ElapsedMilliseconds = sw.ElapsedMilliseconds;

                if (!response.IsSuccessStatusCode)
                    throw new Exception(string.Format("Call TPApi Failed! url:{0} status:{1}", url, response.StatusCode.ToString()));

                var body = await response.Content.ReadAsStreamAsync();
                var dics = new Dictionary<string, object>();
                dics.Add("request", string.Empty);
                dics.Add("response", string.Empty);
                dics.Add("API_KEY", API_KEY);
                using (var scope = logger.BeginScope(dics))
                {
                    logger.LogInformation("Tp Get RequestPath: {RequestPath} | ResponseHttpStatus:{Status} | exeTime:{exeTime} ms", url, response.StatusCode, sw.Elapsed.TotalMilliseconds);
                }
                return body;

            }
            catch (TaskCanceledException ex)
            {
                apiResInfo.ElapsedMilliseconds = 99999;
                throw ex;
            }
        }

        protected async Task<TpResponse<TResponse>> PostAsync<TRequest, TResponse>(string url, TRequest request, Func<string, string> responseLogFormat = null)
        {
            var apiResInfo = new ApiResponseData();
            try
            {
                var client = _httpClientFactory.CreateClient("log");
                client.Timeout = TimeSpan.FromSeconds(14);
                SetTpHeader(ref client);

                var formdata = GetDictionary(request);
                formdata.Add("sign", ComputeSign(formdata));

                using var formContent = new MultipartFormDataContent();
                foreach (var field in formdata)
                {
                    formContent.Add(new StringContent(field.Value), $"\"{field.Key}\"");
                }

                logger.LogInformation("Tp Post RequestPath: {RequestPath} Body:{body}", url, JsonConvert.SerializeObject(formdata));

                var sw = System.Diagnostics.Stopwatch.StartNew();
                var response = await client.PostAsync(Platform.TP, url, formContent);
                sw.Stop();
                apiResInfo.ElapsedMilliseconds = sw.ElapsedMilliseconds;
                if (!response.IsSuccessStatusCode)
                    throw new Exception(string.Format("Call TPApi Failed! url:{0} status:{1}", url, response.StatusCode.ToString()));

                var body = await response.Content.ReadAsStringAsync();
                var dics = new Dictionary<string, object>();

                dics.Add("request", JsonConvert.SerializeObject(formdata));
                dics.Add("response", responseLogFormat == null ? body : responseLogFormat(body));
                dics.Add("API_KEY", API_KEY);
                using (var scope = logger.BeginScope(dics))
                {
                    logger.LogInformation("Tp Post RequestPath: {RequestPath} | ResponseHttpStatus:{Status} | exeTime:{exeTime} ms", url, response.StatusCode, sw.Elapsed.TotalMilliseconds);
                }
                var result = JsonConvert.DeserializeObject<TpResponse<TResponse>>(body);
                return result;

            }
            catch (TaskCanceledException ex)
            {
                apiResInfo.ElapsedMilliseconds = 99999;
                throw ex;
            }
        }

        private void SetTpHeader(ref HttpClient httpClient)
        {
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", API_TOKEN);
        }

        private static Dictionary<string, string> GetDictionary<TRequest>(TRequest request, bool KeepNullField = false)
        {
            var props = typeof(TRequest).GetProperties();
            var param = new Dictionary<string, string>();

            foreach (var prop in props)
            {
                var propName = prop.Name;
                string propValue = prop.PropertyType == typeof(DateTime) ? ((DateTime)prop.GetValue(request)).ToString("yyyy-MM-dd HH:mm:ss") : prop.GetValue(request)?.ToString();

                if (KeepNullField || propValue is not null)
                    param.Add(propName, WebUtility.UrlEncode(propValue));
            }

            return param;
        }

        /// <summary>
        /// 每次呼叫都必須在網址加上一個sign參數，而sign參數是以傳遞資料及API KEY產生:
        /// 先將參數陣列照key值進行升序排序
        /// => 組成query string
        /// => 後面串上api key後md5加密
        /// </summary>
        /// <typeparam name="TRequest"></typeparam>
        /// <param name="request"></param>
        /// <returns></returns>
        private string ComputeSign<TRequest>(TRequest request)
        {
            var queryString = BuildQueryString(request);

            using var md5 = MD5.Create();
            byte[] plainByteArray = md5.ComputeHash(Encoding.UTF8.GetBytes($"{queryString}{API_KEY}"));
            string cipherText = Convert.ToHexString(plainByteArray).ToLower();

            return cipherText;
        }

        private string BuildQueryString<TRequest>(TRequest request)
        {
            Dictionary<string, string> props = request as Dictionary<string, string>;

            if (request is null)
                props = GetDictionary(request);

            return string.Join('&', props.OrderBy(p => p.Key, StringComparer.Ordinal).Select(p => $"{p.Key}={p.Value ?? string.Empty}"));
        }
    }
}