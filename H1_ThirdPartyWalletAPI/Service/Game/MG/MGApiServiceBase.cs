using H1_ThirdPartyWalletAPI.Code;
using H1_ThirdPartyWalletAPI.Extensions;
using H1_ThirdPartyWalletAPI.Model.Config;
using H1_ThirdPartyWalletAPI.Model.Game.MG.Exceptions;
using H1_ThirdPartyWalletAPI.Model.Game.MG.Response;
using H1_ThirdPartyWalletAPI.Service.Common;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Platform = H1_ThirdPartyWalletAPI.Code.Platform;

namespace H1_ThirdPartyWalletAPI.Service.Game.MG
{
    public class MGApiServiceBase
    {
        private readonly ILogger<MGApiServiceBase> logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IMemoryCache memoryCache;
        protected JsonSerializerOptions jsonSerializerOptions { get; set; }

        public string STS_Url { get; set; }

        public string Api_Url { get; set; }

        public string AgentCode { get; set; }
        public string Secrets { get; set; }

        public string AccessToken
        {
            get
            {
                return GetAccessToken().GetAwaiter().GetResult();
            }
        }

        public MGApiServiceBase(ILogger<MGApiServiceBase> logger, IHttpClientFactory httpClientFactory
            , IMemoryCache memoryCache
            , IApiHealthCheckService apiHealthCheckService)
        {
            this.logger = logger;
            this._httpClientFactory = httpClientFactory;
            this.memoryCache = memoryCache;
            STS_Url = Config.GameAPI.MG_TOKEN_URL;
            Api_Url = Config.GameAPI.MG_API_URL;
            AgentCode = Config.CompanyToken.MG_Token;
            Secrets = Config.CompanyToken.MG_Key;
            jsonSerializerOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                Converters = { new JsonStringEnumConverter() }
            };

        }

        public async Task<TResponse> PostAsync<TRequest, TResponse>(TRequest request, string RequestPath)
        {
            var apiResInfo = new ApiResponseData();
            try
            {
                var client = _httpClientFactory.CreateClient("log");
                client.Timeout = TimeSpan.FromSeconds(14);
                client.DefaultRequestHeaders.Add("Authorization", string.Format("Bearer {0}", AccessToken));
                var dic = typeof(TRequest).GetProperties().Where(x => x.GetValue(request) != null).ToDictionary(x => x.Name, y => y.GetValue(request).ToString());
                var content = new FormUrlEncodedContent(dic);
                var sw = System.Diagnostics.Stopwatch.StartNew();

                var response = await client.PostAsync(Platform.MG, string.Format("{0}/api/v1/agents/{1}/{2}", Api_Url.TrimEnd('/'), AgentCode, RequestPath), content);

                sw.Stop();
                apiResInfo.ElapsedMilliseconds = sw.ElapsedMilliseconds;
                HttpStatusCode httpStatusCode = response.StatusCode;
                if (!response.IsSuccessStatusCode)
                    throw new Exception(string.Format("Call MgApi Failed:{0} - {1}", httpStatusCode, RequestPath));
                else
                {
                    var body = await response.Content.ReadAsStringAsync();
                    var dics = new Dictionary<string, object>
                    {
                        { "request", request },
                        { "response", body }
                    };
                    using (var scope = logger.BeginScope(dics))
                    {
                        logger.LogInformation("Post RequestPath: {RequestPath} | ResponseHttpStatus:{Status} | exeTime:{exeTime} ms", RequestPath, response.StatusCode, sw.Elapsed.TotalMilliseconds);
                    }
                    var result = JsonSerializer.Deserialize<TResponse>(body, jsonSerializerOptions);
                    return result;
                }
            }
            catch (TaskCanceledException ex)
            {
                apiResInfo.ElapsedMilliseconds = 99999;
                throw new TaskCanceledException(ex.ToString());
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }
        public async Task<TResponse> PostAsyncJsonBody<TRequest, TResponse>(TRequest request, string RequestPath)
        {
            var apiResInfo = new ApiResponseData();
            try
            {
                var client = _httpClientFactory.CreateClient("log");
                client.Timeout = TimeSpan.FromSeconds(14);
                client.DefaultRequestHeaders.Add("Authorization", string.Format("Bearer {0}", AccessToken));
                var json = JsonSerializer.Serialize(request, jsonSerializerOptions);
                StringContent requestBody;
                requestBody = new StringContent(json, System.Text.Encoding.UTF8, "application/json");


                // var dic = typeof(TRequest).GetProperties().ToDictionary(x => x.Name, y => y.GetValue(request).ToString());
                // var content = new FormUrlEncodedContent(dic);
                var sw = System.Diagnostics.Stopwatch.StartNew();
                var response = await client.PostAsync(Platform.MG, string.Format("{0}/api/v1/agents/{1}/{2}", Api_Url.TrimEnd('/'), AgentCode, RequestPath), requestBody);
                sw.Stop();
                apiResInfo.ElapsedMilliseconds = sw.ElapsedMilliseconds;
                HttpStatusCode httpStatusCode = response.StatusCode;
                if (!response.IsSuccessStatusCode)
                    throw new Exception(string.Format("Call MgApi Failed:{0} - {1}", httpStatusCode, RequestPath));
                else
                {
                    var body = await response.Content.ReadAsStringAsync();
                    var dics = new Dictionary<string, object>
                    {
                        { "request", request },
                        { "response", body }
                    };
                    using (var scope = logger.BeginScope(dics))
                    {
                        logger.LogInformation("Post RequestPath: {RequestPath} | ResponseHttpStatus:{Status} | exeTime:{exeTime} ms", RequestPath, response.StatusCode, sw.Elapsed.TotalMilliseconds);
                    }
                    var result = JsonSerializer.Deserialize<TResponse>(body, jsonSerializerOptions);
                    return result;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }
        public async Task<TResponse> GetAsyncMsgIdHttpStatusCode<TResponse>(string RequestPath)
        {
            var apiResInfo = new ApiResponseData();
            HttpStatusCode httpStatusCode = HttpStatusCode.OK;
            try
            {
                var client = _httpClientFactory.CreateClient("log");
                client.Timeout = TimeSpan.FromSeconds(14);
                client.DefaultRequestHeaders.Add("Authorization", string.Format("Bearer {0}", AccessToken));
                var sw = System.Diagnostics.Stopwatch.StartNew();

                var response = await client.GetAsync(Platform.MG, string.Format("{0}/api/v1/agents/{1}/{2}", Api_Url.TrimEnd('/'), AgentCode, RequestPath));

                sw.Stop();
                apiResInfo.ElapsedMilliseconds = sw.ElapsedMilliseconds;
                httpStatusCode = response.StatusCode;
                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"Call MG Api Failed(httpStatusCode:{response.StatusCode}):{RequestPath}");
                }
                else
                {
                    var body = await response.Content.ReadAsStringAsync();
                    var dics = new Dictionary<string, object>();
                    dics.Add("response", body);
                    using (var scope = logger.BeginScope(dics))
                    {
                        logger.LogInformation("Get RequestPath: {RequestPath} | ResponseHttpStatus:{Status} | exeTime:{exeTime} ms", RequestPath, response.StatusCode, sw.Elapsed.TotalMilliseconds);
                    }
                    var result = JsonSerializer.Deserialize<TResponse>(body, jsonSerializerOptions);
                    return result;
                }
            }
            catch (Exception)
            {
                int status_code = (int)httpStatusCode;
                throw new ExceptionMessage(status_code, Enum.GetName(typeof(HttpStatusCode), status_code));
            }

        }
        public async Task<TResponse> GetAsync<TResponse>(string RequestPath)
        {
            var apiResInfo = new ApiResponseData();
            try
            {
                var client = _httpClientFactory.CreateClient("log");
                client.Timeout = TimeSpan.FromSeconds(14);
                client.DefaultRequestHeaders.Add("Authorization", string.Format("Bearer {0}", AccessToken));
                var sw = System.Diagnostics.Stopwatch.StartNew();

                var response = await client.GetAsync(Platform.MG, string.Format("{0}/api/v1/agents/{1}/{2}", Api_Url.TrimEnd('/'), AgentCode, RequestPath));

                sw.Stop();
                apiResInfo.ElapsedMilliseconds = sw.ElapsedMilliseconds;
                HttpStatusCode httpStatusCode = response.StatusCode;
                if (!response.IsSuccessStatusCode)
                    throw new Exception(string.Format("Call MgApi Failed:{0} - {1}", httpStatusCode, RequestPath));
                else
                {
                    var body = await response.Content.ReadAsStringAsync();
                    var dics = new Dictionary<string, object>();
                    dics.Add("response", body);
                    using (var scope = logger.BeginScope(dics))
                    {
                        logger.LogInformation("Get RequestPath: {RequestPath} | ResponseHttpStatus:{Status} | exeTime:{exeTime} ms", RequestPath, response.StatusCode, sw.Elapsed.TotalMilliseconds);
                    }
                    var result = JsonSerializer.Deserialize<TResponse>(body, jsonSerializerOptions);
                    return result;
                }
            }
            catch (TaskCanceledException)
            {
                apiResInfo.ElapsedMilliseconds = 99999;
                throw;
            }
        }

        public async Task<string> GetHeartBeatAsync()
        {
            const string RequestPath = "heartbeat";
            
            var client = _httpClientFactory.CreateClient("log");
            client.Timeout = TimeSpan.FromSeconds(14);

            var response = await client.GetAsync(Platform.MG, $"{Api_Url.TrimEnd('/')}/api/v1/{RequestPath}");

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Call MgApi Failed:{response.StatusCode} - {RequestPath}");
            
            var body = await response.Content.ReadAsStringAsync();
            return body;
        }
        public async Task<TResponse> PatchAsync<TRequest, TResponse>(TRequest request, string RequestPath)
        {
            var apiResInfo = new ApiResponseData();
            try
            {
                var client = _httpClientFactory.CreateClient("log");
                client.Timeout = TimeSpan.FromSeconds(14);
                client.DefaultRequestHeaders.Add("Authorization", string.Format("Bearer {0}", AccessToken));
                var dic = typeof(TRequest).GetProperties().ToDictionary(x => x.Name, y => y.GetValue(request).ToString());
                var content = new FormUrlEncodedContent(dic);
                var sw = System.Diagnostics.Stopwatch.StartNew();
                var response = await client.PatchAsync(string.Format("{0}/api/v1/agents/{1}/{2}", Api_Url.TrimEnd('/'), AgentCode, RequestPath), content);
                sw.Stop();
                apiResInfo.ElapsedMilliseconds = sw.ElapsedMilliseconds;
                HttpStatusCode httpStatusCode = response.StatusCode;
                if (!response.IsSuccessStatusCode)
                    throw new Exception(string.Format("Call MgApi Failed:{0} - {1}", httpStatusCode, RequestPath));
                else
                {
                    var body = await response.Content.ReadAsStringAsync();
                    var dics = new Dictionary<string, object>();
                    dics.Add("request", request);
                    dics.Add("response", body);
                    using (var scope = logger.BeginScope(dics))
                    {
                        logger.LogInformation("Patch RequestPath: {RequestPath} | ResponseHttpStatus:{Status} | exeTime:{exeTime} ms", RequestPath, response.StatusCode, sw.Elapsed.TotalMilliseconds);
                    }
                    var result = JsonSerializer.Deserialize<TResponse>(body, jsonSerializerOptions);
                    return result;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }
        private async Task<string> GetAccessToken()
        {
            var apiResInfo = new ApiResponseData();
            try
            {
                var path = string.Format("{0}/connect/token", STS_Url.TrimEnd('/'));
                var cachekey = "MG_API_Token";
                var token = await memoryCache.GetOrCreateAsync<string>(cachekey, async entry =>
                {
                    entry.SetOptions(new MemoryCacheEntryOptions { AbsoluteExpiration = DateTimeOffset.UtcNow.AddMinutes(50) });// 有效時間 60 分鐘，提前10分鐘更換
                    var client = _httpClientFactory.CreateClient("log");
                    client.Timeout = TimeSpan.FromSeconds(14);
                    var dic = new Dictionary<string, string>();
                    dic.Add("client_id", AgentCode);
                    dic.Add("client_secret", Secrets);
                    dic.Add("grant_type", "client_credentials");
                    var content = new FormUrlEncodedContent(dic);
                    var response = await client.PostAsync(Platform.MG, path, content);
                    if (!response.IsSuccessStatusCode)
                    {
                        logger.LogError("ServiceName: {ServiceType} | Get AccessToken Failed", "MGService");
                        throw new MGInternalException("9999", "GetAccessTokenError");
                    }
                    else
                    {
                        var json = await response.Content.ReadAsStringAsync();
                        var token = JsonSerializer.Deserialize<GetAccessToken>(json);
                        DateTime expiresDateTime = DateTime.Now;
                        int expiresIn = token.expires_in;
                        expiresDateTime = expiresDateTime.AddSeconds(expiresIn);
                        logger.LogInformation($"MG - STS Token - Expires DateTime({expiresIn}): {expiresDateTime}");
                        return token.access_token;
                    }
                });
                return token;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }
    }
}