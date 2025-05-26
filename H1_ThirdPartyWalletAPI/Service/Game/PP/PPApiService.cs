using H1_ThirdPartyWalletAPI.Code;
using H1_ThirdPartyWalletAPI.Extensions;
using H1_ThirdPartyWalletAPI.Model.Config;
using H1_ThirdPartyWalletAPI.Model.Game.PP.Request;
using H1_ThirdPartyWalletAPI.Model.Game.PP.Responses;
using H1_ThirdPartyWalletAPI.Service.Common;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace H1_ThirdPartyWalletAPI.Service.Game.PP
{
    public class PPApiService : IPPApiService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<PPApiService> _logger;
        public PPApiService(IHttpClientFactory httpClientFactory, ILogger<PPApiService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task<PP_Responses> CreateMemberAsync(CreatePlayerRequest source)
        {
            return await ApiHandle<CreatePlayerRequest, PP_Responses>(Config.GameAPI.PP_URL + "http/CasinoGameAPI/player/account/create", source);
        }
        public async Task<StartGameResponses> StartGameAsync(StartGameRequest source)
        {
            return await ApiHandle<StartGameRequest, StartGameResponses>(Config.GameAPI.PP_URL + "http/CasinoGameAPI/game/start/", source);
        }
        public async Task<TransferResponses> TransferAsync(TransferRequest source)
        {
            return await ApiHandle<TransferRequest, TransferResponses>(Config.GameAPI.PP_URL + "http/CasinoGameAPI/balance/transfer/", source);
        }
        public async Task<GetBalanceResponses> GetBalanceAsync(GetBalanceRequest source)
        {
            return await ApiHandle<GetBalanceRequest, GetBalanceResponses>(Config.GameAPI.PP_URL + "http/CasinoGameAPI/balance/current/", source);
        }
        public async Task<GetTransferStatusResponses> GetTransferStatusAsync(GetTransferStatusRequest source)
        {
            return await ApiHandle<GetTransferStatusRequest, GetTransferStatusResponses>(Config.GameAPI.PP_URL + "http/CasinoGameAPI/balance/transfer/status/", source);
        }
        public async Task<TerminateSessionResponses> TerminateSessionAsync(TerminateSessionRequest source)
        {
            return await ApiHandle<TerminateSessionRequest, TerminateSessionResponses>(Config.GameAPI.PP_URL + "http/CasinoGameAPI/game/session/terminate/", source);
        }

        public async Task<OpenHistoryResponses> OpenHistoryAsync(OpenHistoryRequest source)
        {
            return await ApiHandle<OpenHistoryRequest, OpenHistoryResponses>(Config.GameAPI.PP_URL + "http/HistoryAPI/OpenHistory/", source);
        }
        public async Task<List<GetRecordResponses>> GetRecordAsync(GetRecordRequest source)
        {
            Dictionary<string, string> postData = new Dictionary<string, string>();
            // 轉換DIC
            postData = Helper.GetDictionary(source);
            var postDataString = string.Join("&", postData.Select(q => $"{q.Key}={q.Value}"));

            var EnvironmentData = await EnvironmentAsync(new EnvironmentRequest()
            {
                secureLogin = Config.CompanyToken.PP_SecureLogin
            });
            var DataUrlList = EnvironmentData.environments;
            
            List < GetRecordResponses > DataList= new List<GetRecordResponses>();


            foreach (var item in DataUrlList)
            {
                //string apiDomain = "";
                //switch (item.envName)
                //{
                //    case "prod-2409":
                //        apiDomain = item.apiDomain;
                //        break;
                //    case "prod-2118":
                //        apiDomain= item.apiDomain;
                //        break;
                //    case "prod-sg12":
                //        apiDomain = item.apiDomain;
                //        break;
                //    case "prerelease2":
                //        apiDomain = item.apiDomain;
                //        break;
                //    case "prerelease1":
                //        apiDomain = item.apiDomain;
                //        break;
                //}

                //if (apiDomain=="")
                //{
                //    continue;
                //}

                var url = $"https://{item.apiDomain}/IntegrationService/v3/DataFeeds/gamerounds/" + "?" + postDataString;

                var data= await GetAsync<GetRecordRequest, List<GetRecordResponses>>(url, source);

                foreach (var iten in data)
                {
                    DataList.Add(iten);
                }

            }

            return DataList;
        }

        public async Task<HealthCheckResponse> HealthCheckAsync()
        {
            using var httpClient = _httpClientFactory.CreateClient("log");
            httpClient.Timeout = TimeSpan.FromSeconds(14);
            var response = await httpClient.GetAsync(Platform.PP, Config.GameAPI.PP_URL.Replace("IntegrationService/v3/", "gs2c/livetest"));
    
            response.EnsureSuccessStatusCode();
            var body = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<HealthCheckResponse>(body);
        }



        public async Task<EnvironmentResponses> EnvironmentAsync(EnvironmentRequest source)
        {
            Dictionary<string, string> Dictionary = new Dictionary<string, string>();
            // 轉換DIC
            Dictionary = Helper.GetDictionary(source);
            // 排序
            Dictionary<string, string> Dicorder = Dictionary.OrderBy(x => x.Key).ToDictionary(obj => obj.Key, obj => obj.Value);
            //組成字串
            var Dictionaryobery = string.Join("&", Dicorder.Select(q => $"{q.Key}={q.Value}"));
            //組成MD5
            var Key = Helper.MD5encryption(Dictionaryobery, Config.CompanyToken.PP_Key, "").ToLower();
            //加入MD5加密參數
            Dictionary.Add("hash", Key);
            //組成URL
            string Datastring = string.Join("&", Dictionary.Select(q => $"{q.Key}={q.Value}"));


            using var httpClient = _httpClientFactory.CreateClient("log");
            httpClient.Timeout = TimeSpan.FromSeconds(14);
            var response = await httpClient.GetAsync(Platform.PP, Config.GameAPI.PP_URL + "http/SystemAPI/environments" + "?" + Datastring);
            response.EnsureSuccessStatusCode();
            var body = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<EnvironmentResponses>(body);
        }

        private async Task<TResponse> ApiHandle<TRequest, TResponse>(string url, TRequest source)
        {

            Dictionary<string, string> Dictionary = new Dictionary<string, string>();
            // 轉換DIC
            Dictionary = Helper.GetDictionary(source);
            // 排序
            Dictionary<string, string> Dicorder = Dictionary.OrderBy(x => x.Key).ToDictionary(obj => obj.Key, obj => obj.Value);
            //組成字串
            var Dictionaryobery = string.Join("&", Dicorder.Select(q => $"{q.Key}={q.Value}"));
            //組成MD5
            var Key = Helper.MD5encryption(Dictionaryobery, Config.CompanyToken.PP_Key, "").ToLower();
            //加入MD5加密參數
            Dictionary.Add("hash", Key);
            //組成URL
            string Datastring = string.Join("&", Dictionary.Select(q => $"{q.Key}={q.Value}"));

            var headers = new Dictionary<string, string>
            {
                {"Content-Type", "application/x-www-form-urlencoded"},
            };

            var postData = new Dictionary<string, string>
            {
            };
            var responseData = await Post(url + "?" + Datastring, postData, headers);

            return JsonConvert.DeserializeObject<TResponse>(responseData);

        }

        private async Task<string> Post(string url, Dictionary<string, string> postData, Dictionary<string, string> headers = null, int retry = 3)
        {

            HttpResponseMessage response = null;
            var apiResInfo = new ApiResponseData();
            try
            {
                _logger.LogInformation("PP Post RequestPath: {RequestPath}", url);
                using (var request = _httpClientFactory.CreateClient("log"))
                {
                    if (headers != null)
                    {
                        foreach (var item in headers)
                        {
                            request.DefaultRequestHeaders.TryAddWithoutValidation(item.Key, item.Value);
                        }
                    }
                    request.Timeout = TimeSpan.FromSeconds(14);
                    var content = new FormUrlEncodedContent(postData);
                    var sw = System.Diagnostics.Stopwatch.StartNew();
                    response = await request.PostAsync(Platform.PP, url, content);
                    sw.Stop();
                    apiResInfo.ElapsedMilliseconds = sw.ElapsedMilliseconds;
                    response.EnsureSuccessStatusCode();
                    var body = await response.Content.ReadAsStringAsync();

                    var dics = new Dictionary<string, object>
                    {
                        { "request", postData },
                        { "response", body }
                    };

                    using (var scope = _logger.BeginScope(dics))
                    {
                        _logger.LogInformation("Get RequestPath: {RequestPath} | ResponseHttpStatus:{Status} | exeTime:{exeTime} ms", url, response.StatusCode, sw.Elapsed.TotalMilliseconds);
                    }

                    return body;
                }

            }
            catch (HttpRequestException ex)
            {
                if (retry == 0)
                {
                    throw new Exception(string.Format("Call PPApi Failed:{0} Message:{1}", url, ex.Message));
                }

                return await Post(url, postData, headers, retry - 1);
            }
        }

        protected async Task<List<GetRecordResponses>> GetAsync<TRequest, TResponse>(string url, TRequest source, int retry = 3)
        {
            HttpResponseMessage response = null;
            var apiResInfo = new ApiResponseData();
            try
            {
                using (var request = _httpClientFactory.CreateClient("log"))
                {

                    request.Timeout = TimeSpan.FromSeconds(14);

                    var sw = System.Diagnostics.Stopwatch.StartNew();
                    response = await request.GetAsync(Platform.PP, url);
                    sw.Stop();
                    apiResInfo.ElapsedMilliseconds = sw.ElapsedMilliseconds;
                    _logger.LogInformation("PP GET RequestPath: {RequestPath}", url);
                    response.EnsureSuccessStatusCode();
                    var body = await response.Content.ReadAsStringAsync();

                    var dics = new Dictionary<string, object>
                    {
                        { "request", source },
                        { "response", body }
                    };

                    using (var scope = _logger.BeginScope(dics))
                    {
                        _logger.LogInformation("Get PP RequestPath: {RequestPath} | ResponseHttpStatus:{Status} | exeTime:{exeTime} ms", url, response.StatusCode, sw.Elapsed.TotalMilliseconds);
                    }
                    var models = new List<GetRecordResponses>();
                    var lineCount = 0;
                    using (var reader = new StringReader(body))
                    {
                        while (reader.Peek() != -1)
                        {
                            var line = reader.ReadLine();
                            lineCount++;
                            // Skip first two header rows
                            if (lineCount <= 2) continue;
                            var values = line.Split(',');
                            var model = new GetRecordResponses
                            {
                                PlayerID = values[0],
                                ExtPlayerID = values[1],
                                GameID = values[2],
                                PlaySessionID = Int64.Parse(values[3]),
                                ParentSessionID = values[4],
                                StartDate = DateTime.Parse(values[5]),
                                EndDate = DateTime.TryParse(values[6], out var endDate) ? endDate : null,
                                Status = values[7],
                                Type = values[8],
                                Bet = decimal.Parse(values[9]),
                                Win = decimal.Parse(values[10]),
                                Currency = values[11],
                                Jackpot = decimal.Parse(values[12])
                            };
                            models.Add(model);
                        }
                    }
                    return models;
                }

            }
            catch (HttpRequestException ex)
            {
                if (retry == 0)
                {
                    throw new Exception(string.Format("Call PPApi Failed! url:{0} status:{1}", url, ex.Message.ToString()));
                }

                return await GetAsync<TRequest, TResponse>(url, source, retry - 1);
            }
        }
    }
}
