using H1_ThirdPartyWalletAPI.Code;
using H1_ThirdPartyWalletAPI.Extensions;
using H1_ThirdPartyWalletAPI.Model.Config;
using H1_ThirdPartyWalletAPI.Model.Game.DS.Request;
using H1_ThirdPartyWalletAPI.Service.Common;
using H1_ThirdPartyWalletAPI.Service.Game.DS.JsonConverter;
using H1_ThirdPartyWalletAPI.Service.Game.DS.Utility;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace H1_ThirdPartyWalletAPI.Service.Game.DS
{
    public class DSApiServiceBase
    {
        private readonly ILogger<DSApiServiceBase> logger;
        private readonly IHttpClientFactory _httpClientFactory;

        protected JsonSerializerOptions jsonSerializerOptions { get; set; }
        protected JsonSerializerOptions DeserializejsonSerializerOptions { get; set; }
        private string Channel { get; set; }
        private string Agent_Code { get; set; }
        private string AES_Key { get; set; }

        private string MD5_Key { get; set; }
        private string API_URL { get; set; }

        private AESHelper aESHelper { get; set; }
        private MD5Helper MD5Helper { get; set; }

        public DSApiServiceBase(ILogger<DSApiServiceBase> logger, IHttpClientFactory httpClientFactory)
        {
            this.logger = logger;
            this._httpClientFactory = httpClientFactory;
            Channel = Config.CompanyToken.DS_CHANNEL_CODE;
            Agent_Code = Config.CompanyToken.DS_AGENT;
            AES_Key = Config.CompanyToken.DS_AES;
            MD5_Key = Config.CompanyToken.DS_MD5;
            aESHelper = new AESHelper();
            MD5Helper = new MD5Helper();
            API_URL = Config.GameAPI.DS_URL;
            jsonSerializerOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                Converters = {
                    new SerializeDateTimeConverter()
                }
            };
            DeserializejsonSerializerOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                Converters = {
                    new DecimalDeserializeConverter()
                }
            };
        }
        public async Task<TResponse> PostAsync<TRequest, TResponse>(TRequest request, string RequestPath) where TRequest : RequestBaseModel
        {
            var apiResInfo = new ApiResponseData();
            try
            {
                request.agent = Agent_Code;
                var client = _httpClientFactory.CreateClient("log");
                client.Timeout = TimeSpan.FromSeconds(14);
                var json = JsonSerializer.Serialize(request, jsonSerializerOptions);
                var chiphertext = aESHelper.Encrypt(json, AES_Key);
                var sign = MD5Helper.Encrypt(string.Format("{0}{1}", chiphertext, MD5_Key));
                var _request = new RequestModel(Channel, chiphertext, sign);
                var content = new StringContent(JsonSerializer.Serialize(_request, jsonSerializerOptions), Encoding.UTF8, "application/json");
                var sw = System.Diagnostics.Stopwatch.StartNew();

                var FullRequestPath = string.Format("{0}/{1}/{2}", API_URL.TrimEnd('/'), "v1", RequestPath);
                var response = await client.PostAsync(Platform.DS, FullRequestPath, content);
                sw.Stop();
                apiResInfo.ElapsedMilliseconds = sw.ElapsedMilliseconds;
                if (!response.IsSuccessStatusCode)
                    throw new Exception(string.Format("Call DSApi Failed:{0}", RequestPath));
                else
                {
                    var body = await response.Content.ReadAsStringAsync();
                    var dics = new Dictionary<string, object>();
                    dics.Add("request", json);
                    dics.Add("response", body);
                    dics.Add("chiphertext", chiphertext);
                    dics.Add("AES_Key", AES_Key);
                    dics.Add("MD5_Key", MD5_Key);
                    using (var scope = logger.BeginScope(dics))
                    {
                        logger.LogInformation("DS Post RequestPath: {RequestPath} | ResponseHttpStatus:{Status} | exeTime:{exeTime} ms", FullRequestPath, response.StatusCode, sw.Elapsed.TotalMilliseconds);
                    }
                    var result = JsonSerializer.Deserialize<TResponse>(body, DeserializejsonSerializerOptions);
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
        public async Task<TResponse> PostWithOutBaseRequestAsync<TRequest, TResponse>(TRequest request, string RequestPath)
        {
            var apiResInfo = new ApiResponseData();
            try
            {
                var client = _httpClientFactory.CreateClient("log");
                client.Timeout = TimeSpan.FromSeconds(14);
                var json = JsonSerializer.Serialize(request, jsonSerializerOptions);
                var chiphertext = aESHelper.Encrypt(json, AES_Key);
                var sign = MD5Helper.Encrypt(string.Format("{0}{1}", chiphertext, MD5_Key));
                var _request = new RequestModel(Channel, chiphertext, sign);
                var content = new StringContent(JsonSerializer.Serialize(_request, jsonSerializerOptions), Encoding.UTF8, "application/json");
                var sw = System.Diagnostics.Stopwatch.StartNew();
                var response = await client.PostAsync(Platform.DS, string.Format("{0}/{1}/{2}", API_URL.TrimEnd('/'), "v1", RequestPath), content);
                sw.Stop();
                apiResInfo.ElapsedMilliseconds = sw.ElapsedMilliseconds;
                if (!response.IsSuccessStatusCode)
                    throw new Exception(string.Format("Call DSApi Failed:{0}", RequestPath));
                else
                {
                    var body = await response.Content.ReadAsStringAsync();

                    try
                    {
                        var dics = new Dictionary<string, object>();
                        //拉注單log過長截斷
                        if (body.Length > 10000)
                        {
                            var response_log = "";
                            response_log = body.Substring(0, 9999);
                            dics.Add("response", response_log);
                        }
                        else
                        {
                            dics.Add("response", body);
                        }
                        dics.Add("request", json);
                        dics.Add("chiphertext", chiphertext);
                        dics.Add("AES_Key", AES_Key);
                        dics.Add("MD5_Key", MD5_Key);
                        using (var scope = logger.BeginScope(dics))
                        {
                            logger.LogInformation("DS Post RequestPath: {RequestPath} | ResponseHttpStatus:{Status} | exeTime:{exeTime} ms", RequestPath, response.StatusCode, sw.Elapsed.TotalMilliseconds);
                        }
                    }
                    catch (Exception ex)
                    {
                        var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                        var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                        logger.LogError("log exception EX : {ex}  MSG : {Message} Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
                    }
                    var result = JsonSerializer.Deserialize<TResponse>(body, DeserializejsonSerializerOptions);
                    return result;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public async Task<TResponse> PostAsync<TResponse>(string RequestPath)
        {
            var apiResInfo = new ApiResponseData();
            try
            {
                var client = _httpClientFactory.CreateClient("log");
                client.Timeout = TimeSpan.FromSeconds(14);
                var chiphertext = aESHelper.Encrypt("{}", AES_Key);
                var sign = MD5Helper.Encrypt(string.Format("{0}{1}", chiphertext, MD5_Key));
                var _request = new RequestModel(Channel, chiphertext, sign);
                var content = new StringContent(JsonSerializer.Serialize(_request, jsonSerializerOptions), Encoding.UTF8, "application/json");
                var sw = System.Diagnostics.Stopwatch.StartNew();
                var response = await client.PostAsync(Platform.DS, string.Format("{0}/{1}/{2}", API_URL.TrimEnd('/'), "v1", RequestPath), content);
                sw.Stop();
                apiResInfo.ElapsedMilliseconds = sw.ElapsedMilliseconds;
                if (!response.IsSuccessStatusCode)
                    throw new Exception(string.Format("Call DSApi Failed:{0}", RequestPath));
                else
                {
                    var body = await response.Content.ReadAsStringAsync();
                    var dics = new Dictionary<string, object>();
                    dics.Add("response", body);
                    using (var scope = logger.BeginScope(dics))
                    {
                        logger.LogInformation("DS Post RequestPath: {RequestPath} | ResponseHttpStatus:{Status} | exeTime:{exeTime} ms", RequestPath, response.StatusCode, sw.Elapsed.TotalMilliseconds);
                    }
                    var result = JsonSerializer.Deserialize<TResponse>(body, DeserializejsonSerializerOptions);
                    return result;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }
    }
}