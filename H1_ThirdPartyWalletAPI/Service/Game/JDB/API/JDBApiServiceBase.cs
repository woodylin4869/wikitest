using H1_ThirdPartyWalletAPI.Code;
using H1_ThirdPartyWalletAPI.Extensions;
using H1_ThirdPartyWalletAPI.Model.Config;
using H1_ThirdPartyWalletAPI.Model.Game.JDB.Exceptions;
using H1_ThirdPartyWalletAPI.Model.Game.JDB.Request;
using H1_ThirdPartyWalletAPI.Model.Game.JDB.Response;
using H1_ThirdPartyWalletAPI.Service.Common;
using H1_ThirdPartyWalletAPI.Service.Game.JDB.API.Utility;
using H1_ThirdPartyWalletAPI.Service.Game.JDB.JsonConverter;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Web;

namespace H1_ThirdPartyWalletAPI.Service.Game.JDB.API
{
    public class JDBApiServiceBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<JDBApiServiceBase> logger;
        private readonly AESHelper aESHelper;
        protected JsonSerializerOptions DeserializejsonSerializerOptions { get; set; }
        protected JsonSerializerOptions SerializejsonSerializerOptions { get; set; }

        public string Url { get; set; }
        public string Key { get; set; }
        public string Iv { get; set; }
        public string Parent { get; set; }
        public string ProvideCode { get; set; }
        protected int Action { get; set; }

        public JDBApiServiceBase(IHttpClientFactory httpClientFactory, ILogger<JDBApiServiceBase> logger)
        {
            this._httpClientFactory = httpClientFactory;
            this.logger = logger;
            this.aESHelper = new AESHelper();
            //TODO key, iv, url
            this.Url = Config.GameAPI.JDB_URL;
            this.Key = Config.CompanyToken.JDB_Key;
            this.Iv = Config.CompanyToken.JDB_IV;
            this.ProvideCode = Config.CompanyToken.JDB_DC;
            this.Parent = Config.CompanyToken.JDB_Parents;
            this.DeserializejsonSerializerOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                NumberHandling = JsonNumberHandling.AllowReadingFromString,
                Converters = {
                    new DeserializeDateTimeConverter()
                }
            };
            this.SerializejsonSerializerOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                Converters = {
                    new SerializeDateTimeConverter()
                },
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };
        }
        public async Task<HttpResponseMessage> PostAysnc<T>(T request) where T : RequestBaseModel
        {
            var apiResInfo = new ApiResponseData();
            try
            {
                Action = request.Action;
                request.Parent = Parent;
                var client = _httpClientFactory.CreateClient("log");

                if (Action == 64)
                {
                    client.Timeout = TimeSpan.FromSeconds(300);
                }
                else if (Action == 29)
                {
                    client.Timeout = TimeSpan.FromSeconds(60);
                }
                else
                {
                    client.Timeout = TimeSpan.FromSeconds(14);
                }
                var json = JsonSerializer.Serialize(request, SerializejsonSerializerOptions);
                var chiphertext = aESHelper.StartEncode(json, Key, Iv);
                var dic = new Dictionary<string, string>();
                dic.Add("dc", ProvideCode);
                dic.Add("x", chiphertext);

                HttpContent content = new FormUrlEncodedContent(dic);
                var sw = System.Diagnostics.Stopwatch.StartNew();
                var Response = await client.PostAsync(Platform.JDB, string.Format("{0}", Url), content);
                sw.Stop();
                apiResInfo.ElapsedMilliseconds = sw.ElapsedMilliseconds;
                var result = await Response.Content.ReadAsStringAsync();
                try
                {
                    var dics = new Dictionary<string, object>();
                    //拉注單log過長截斷
                    if (Action == 64 || Action == 29)
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
                        dics.Add("response", response);
                    }
                    else
                    {
                        dics.Add("response", result);
                    }
                    dics.Add("request", json);
                    dics.Add("chipherText", chiphertext);

                    using (var scope = logger.BeginScope(dics))
                    {
                        logger.LogInformation("Action: Action{Action} | ResponseHttpStatus:{Status} | exeTime:{exeTime} ms", request.Action, Response.StatusCode, sw.Elapsed.TotalMilliseconds);
                    }
                }
                catch (Exception ex)
                {
                    var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                    var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                    logger.LogError("log exception EX : {ex}  MSG : {Message} Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
                }


                if (!Response.IsSuccessStatusCode)
                {
                    throw new Exception($"呼叫 JDB APi 失敗：Action{request.Action.ToString()}");
                }
                //logger.LogInformation("Action: Action{Action} | Request:{Request} | AES Encrypt : {chipherText} | ResponseHttpStatus:{Status}", request.Action, json, chiphertext, Response.StatusCode);
                return Response;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

        public async Task<string> ResultHandler(HttpResponseMessage message)
        {
            var result = await message.Content.ReadAsStringAsync();
            var desDecode = aESHelper.StartDecode(result, Key, Iv);
            return desDecode;
        }

        public async Task<T> ResultHandler<T>(HttpResponseMessage message)
        {

            var result = await message.Content.ReadAsStringAsync();
            var response = JsonSerializer.Deserialize<ResponseBaseModel>(result, DeserializejsonSerializerOptions);
            if (response.Status != "0000")
            {
                var error = JsonSerializer.Deserialize<ResponseErrorModel>(result, DeserializejsonSerializerOptions);
                var dics = new Dictionary<string, object>();
                dics.Add("response", result);
                using (var scope = logger.BeginScope(dics))
                {
                    if (response.Status == "7405") //降低已知status code Log層級
                        logger.LogInformation("Action: Action{Action}", Action);
                    else
                        logger.LogWarning("Action: Action{Action}", Action);
                }
                throw new JDBBadRequestException(error.Status, error.Err_text);
            }
            else
            {
                var dics = new Dictionary<string, object>();
                dics.Add("response", result);
                using (var scope = logger.BeginScope(dics))
                {
                    logger.LogInformation("Action: Action{Action}", Action);
                }
                var target = JsonSerializer.Deserialize<T>(result, DeserializejsonSerializerOptions);
                return target;
            }

        }


    }
}
