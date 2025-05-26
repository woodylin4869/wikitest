using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using H1_ThirdPartyWalletAPI.Code;
using H1_ThirdPartyWalletAPI.Extensions;
using H1_ThirdPartyWalletAPI.Model.Config;
using H1_ThirdPartyWalletAPI.Model.Game.PG.Request;
using H1_ThirdPartyWalletAPI.Model.Game.PG.Response;
using H1_ThirdPartyWalletAPI.Service.Common;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace H1_ThirdPartyWalletAPI.Service.Game.PG.Service
{
    /// <summary>
    /// PG API
    /// </summary>
    public class PGApiService : IPGApiService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<PGApiService> _logger;

        public PGApiService(IHttpClientFactory httpClientFactory, ILogger<PGApiService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        /// <summary>
        /// 创建玩家账号
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public async Task<CreateResponse> CreateAsync(CreateRequest source)
        {
            var path = "Player/v1/Create";
            var query = $"?trace_id={Guid.NewGuid()}";
            var url = Config.GameAPI.PG_SoftAPIDomain + path + query;

            var dictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(JsonConvert.SerializeObject(source));
            var response = await Post(url, dictionary);
            return JsonConvert.DeserializeObject<CreateResponse>(response);
        }

        /// <summary>
        /// 查询玩家的钱包余额
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public async Task<GetPlayerWalletResponse> GetPlayerWalletAsync(GetPlayerWalletRequest source)
        {
            var path = "Cash/v3/GetPlayerWallet";
            var query = $"?trace_id={Guid.NewGuid()}";
            var url = Config.GameAPI.PG_SoftAPIDomain + path + query;

            var dictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(JsonConvert.SerializeObject(source));
            var response = await Post(url, dictionary);
            return JsonConvert.DeserializeObject<GetPlayerWalletResponse>(response);
        }

        /// <summary>
        /// 充值
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public async Task<TransferInResponse> TransferInAsync(TransferInRequest source)
        {
            var path = "Cash/v3/TransferIn";
            var query = $"?trace_id={Guid.NewGuid()}";
            var url = Config.GameAPI.PG_SoftAPIDomain + path + query;

            var dictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(JsonConvert.SerializeObject(source));
            var response = await Post(url, dictionary);
            return JsonConvert.DeserializeObject<TransferInResponse>(response);
        }

        /// <summary>
        /// 转出余额
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public async Task<TransferOutResponse> TransferOutAsync(TransferOutRequest source)
        {
            var path = "Cash/v3/TransferOut";
            var query = $"?trace_id={Guid.NewGuid()}";
            var url = Config.GameAPI.PG_SoftAPIDomain + path + query;

            var dictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(JsonConvert.SerializeObject(source));
            var response = await Post(url, dictionary);
            return JsonConvert.DeserializeObject<TransferOutResponse>(response);
        }

        /// <summary>
        /// 转出所有余额
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public async Task<TransferAllOutResponse> TransferAllOutAsync(TransferAllOutRequest source)
        {
            var path = "Cash/v3/TransferAllOut";
            var query = $"?trace_id={Guid.NewGuid()}";
            var url = Config.GameAPI.PG_SoftAPIDomain + path + query;

            var dictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(JsonConvert.SerializeObject(source));
            var response = await Post(url, dictionary);
            return JsonConvert.DeserializeObject<TransferAllOutResponse>(response);
        }

        /// <summary>
        /// 取得遊戲連結
        /// operator_player_session 請帶 pg_id
        /// </summary>
        /// <param name="gameCode"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        public string GetGameUrl(string gameCode, GetGameUrlRequest source)
        {
            var url = Config.GameAPI.PG_LaunchURL + $"{gameCode}/index.html";
            var dic = JsonConvert.DeserializeObject<Dictionary<string, string>>(JsonConvert.SerializeObject(source));
            var query = "?" + string.Join("&", dic.Where(x => !string.IsNullOrEmpty(x.Value)).Select(x => x.Key + "=" + x.Value));
            return url + query;
        }

        /// <summary>
        /// 令牌验证
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        //public async Task<VerifySessionResponse> VerifySession(VerifySessionRequest source)
        //{
        //    var outputModel = new VerifySessionResponse();

        //    if (source.operator_token != Config.CompanyToken.PG_Token)
        //    {
        //        outputModel.error = new VerifySessionResponse.Error
        //        {
        //            code = "1034"
        //        };
        //        return outputModel;
        //    }

        //    if (source.secret_key != Config.CompanyToken.PG_Key)
        //    {
        //        outputModel.error = new VerifySessionResponse.Error
        //        {
        //            code = "1034"
        //        };
        //        return outputModel;
        //    }


        //    var target = await _dbIdbService.GetPlatformPGUser(source.operator_player_session); // operator_player_session 為 pg_id

        //    // 驗證發出去的 PGId
        //    if (target == null)
        //    {
        //        outputModel.error = new VerifySessionResponse.Error
        //        {
        //            code = "1034"
        //        };
        //        return outputModel;
        //    }

        //    var wallet = await _transferWalletService.GetWalletCache(target.club_id);

        //    return new VerifySessionResponse
        //    {
        //        data = new VerifySessionResponse.Data
        //        {
        //            player_name = source.operator_player_session,
        //            nickname = wallet.Club_Ename,
        //            currency = wallet.Currency
        //        }
        //    };
        //}

        /// <summary>
        /// 踢出玩家
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public async Task<KickResponse> KickAsync(KickRequest source)
        {
            var path = "Player/v1/Kick";
            var query = $"?trace_id={Guid.NewGuid()}";
            var url = Config.GameAPI.PG_SoftAPIDomain + path + query;

            var dictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(JsonConvert.SerializeObject(source));
            var response = await Post(url, dictionary);
            return JsonConvert.DeserializeObject<KickResponse>(response);
        }

        /// <summary>
        /// 冻结玩家
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public async Task<SuspendResponse> SuspendAsync(SuspendRequest source)
        {
            var path = "Player/v1/Suspend";
            var query = $"?trace_id={Guid.NewGuid()}";
            var url = Config.GameAPI.PG_SoftAPIDomain + path + query;

            var dictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(JsonConvert.SerializeObject(source));
            var response = await Post(url, dictionary);
            return JsonConvert.DeserializeObject<SuspendResponse>(response);
        }

        /// <summary>
        /// 恢复玩家账号
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public async Task<ReinstateResponse> ReinstateAsync(ReinstateRequest source)
        {
            var path = "Player/v1/Reinstate";
            var query = $"?trace_id={Guid.NewGuid()}";
            var url = Config.GameAPI.PG_SoftAPIDomain + path + query;

            var dictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(JsonConvert.SerializeObject(source));
            var response = await Post(url, dictionary);
            return JsonConvert.DeserializeObject<ReinstateResponse>(response);
        }

        /// <summary>
        /// 查看玩家状态
        /// 该 API 并非检查在线玩家的状态，而是检查在 PG 的状态。对于在线的活跃玩家，请在后台查询
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public async Task<CheckResponse> CheckAsync(CheckRequest source)
        {
            var path = "Player/v1/Check";
            var query = $"?trace_id={Guid.NewGuid()}";
            var url = Config.GameAPI.PG_SoftAPIDomain + path + query;

            var dictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(JsonConvert.SerializeObject(source));
            var response = await Post(url, dictionary);
            return JsonConvert.DeserializeObject<CheckResponse>(response);
        }

        /// <summary>
        /// 获取单个交易记录
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public async Task<GetSingleTransactionResponse> GetSingleTransactionAsync(GetSingleTransactionRequest source)
        {
            var path = "Cash/v3/GetSingleTransaction";
            var query = $"?trace_id={Guid.NewGuid()}";
            var url = Config.GameAPI.PG_SoftAPIDomain + path + query;

            var dictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(JsonConvert.SerializeObject(source));
            var response = await Post(url, dictionary);
            return JsonConvert.DeserializeObject<GetSingleTransactionResponse>(response);
        }

        /// <summary>
        /// 获取运营商令牌
        /// </summary>
        /// <returns></returns>
        public async Task<LoginProxyResponse> LoginProxyAsync()
        {
            var path = "Login/v1/LoginProxy";
            var query = $"?trace_id={Guid.NewGuid()}";
            var url = Config.GameAPI.PG_SoftAPIDomain + path + query;

            var dictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(JsonConvert.SerializeObject(new LoginProxyRequest()
            {
                operator_token = Config.CompanyToken.PG_Token,
                secret_key = Config.CompanyToken.PG_Key
            }));
            var response = await Post(url, dictionary);
            return JsonConvert.DeserializeObject<LoginProxyResponse>(response);
        }

        /// <summary>
        /// 投注详情页面
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public async Task<string> RedirectToBetDetail(RedirectToBetDetailRequest source)
        {
            var loginProxyResult = await LoginProxyAsync();
            source.t = loginProxyResult.data.operator_session;
            source.trace_id = Guid.NewGuid().ToString();

            var url = Config.GameAPI.PG_HistoryInterpreter;
            var dic = JsonConvert.DeserializeObject<Dictionary<string, string>>(JsonConvert.SerializeObject(source));
            var query = "?" + string.Join("&", dic.Where(x => !string.IsNullOrEmpty(x.Value)).Select(x => x.Key + "=" + x.Value));
            return url + query;
        }

        /// <summary>
        /// 获取多个玩家钱包余额
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public async Task<GetPlayersWalletResponse> GetPlayersWalletAsync(GetPlayersWalletRequest source)
        {
            var path = "Cash/v3/GetPlayersWallet";
            var query = $"?trace_id={Guid.NewGuid()}";
            var url = Config.GameAPI.PG_SoftAPIDomain + path + query;

            var dictionary = new Dictionary<string, string>
            {
                {"operator_token", source.operator_token},
                {"secret_key", source.secret_key},
            };

            for (var index = 0; index < source.player_names.Count; index++)
            {
                dictionary.Add($"player_names[{index}]", source.player_names[index]);
            };

            var response = await Post(url, dictionary);
            return JsonConvert.DeserializeObject<GetPlayersWalletResponse>(response);
        }

        /// <summary>
        /// 获取历史记录
        /// 运营商可获得最近 60 天的投注历史记录。
        /// 运营商必须为每个请求提取至少 1500 条记录。
        /// 建議5分鐘調用一次API
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public async Task<GetHistoryResponse> GetHistoryAsync(GetHistoryRequest source)
        {
            var path = "Bet/v4/GetHistory";
            var query = $"?trace_id={Guid.NewGuid()}";
            var url = Config.GameAPI.PG_DataGrabAPIDomain + path + query;

            var dictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(JsonConvert.SerializeObject(source));
            var response = await Post(url, dictionary);
            return JsonConvert.DeserializeObject<GetHistoryResponse>(response);
        }

        /// <summary>
        /// 获取特定时间内的历史记录
        /// 运营商可获得最近 60 天的投注历史记录。
        /// 运营商必须为每个请求提取至少 1500 条记录。
        /// 建議5分鐘調用一次API
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public async Task<GetHistoryForSpecificTimeRangeResponse> GetHistoryForSpecificTimeRangeAsync(GetHistoryForSpecificTimeRangeRequest source)
        {
            var path = "Bet/v4/GetHistoryForSpecificTimeRange";
            var query = $"?trace_id={Guid.NewGuid()}";
            var url = Config.GameAPI.PG_DataGrabAPIDomain + path + query;

            var dictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(JsonConvert.SerializeObject(source));
            var response = await Post(url, dictionary);
            return JsonConvert.DeserializeObject<GetHistoryForSpecificTimeRangeResponse>(response);
        }

        /// <summary>
        /// 获取单一玩家的历史记录
        /// 运营商可获得最近 60 天的投注历史记录。
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public async Task<GetPlayerHistoryResponse> GetPlayerHistoryAsync(GetPlayerHistoryRequest source)
        {
            var path = "Bet/v4/GetPlayerHistory";
            var query = $"?trace_id={Guid.NewGuid()}";
            var url = Config.GameAPI.PG_DataGrabAPIDomain + path + query;

            var dictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(JsonConvert.SerializeObject(source));
            var response = await Post(url, dictionary);
            return JsonConvert.DeserializeObject<GetPlayerHistoryResponse>(response);
        }

        /// <summary>
        /// 获取游戏列表
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public async Task<GetGameListResponse> GetGameListAsync(GetGameListRequest source)
        {
            var path = "Game/v2/Get";
            var query = $"?trace_id={Guid.NewGuid()}";
            var url = Config.GameAPI.PG_SoftAPIDomain + path + query;

            var dictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(JsonConvert.SerializeObject(source));
            var response = await Post(url, dictionary);
            return JsonConvert.DeserializeObject<GetGameListResponse>(response);
        }

        /// <summary>
        /// 获取每小时投注汇总
        /// 运营商可以获取最近 60 天的投注记录
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public async Task<GetHandsSummaryHourlyResponse> GetHandsSummaryHourlyAsync(GetHandsSummaryHourlyRequest source)
        {
            var path = "Bet/v4/GetHandsSummaryHourly";
            var query = $"?trace_id={Guid.NewGuid()}";
            var url = Config.GameAPI.PG_DataGrabAPIDomain + path + query;

            var dictionary = new Dictionary<string, string>
            {
                {"operator_token", source.operator_token},
                {"secret_key", source.secret_key},
                {"from_time", source.from_time.ToString()},
                {"to_time", source.to_time.ToString()},
                {"currency", source.currency},
            };

            for (var index = 0; index < source.transaction_types?.Count; index++)
            {
                dictionary.Add($"transaction_types[{index}]", source.transaction_types[index].ToString());
            };

            var response = await Post(url, dictionary);
            return JsonConvert.DeserializeObject<GetHandsSummaryHourlyResponse>(response);
        }
        /// <summary>
        /// 获取在线玩家列表
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public async Task<GetOnlinePlayersResponse> GetOnlinePlayersAsync(GetOnlinePlayersRequest source)
        {
            var path = "Player/v3/GetOnlinePlayers";
            var query = $"?trace_id={Guid.NewGuid()}";
            var url = Config.GameAPI.PG_SoftAPIDomain + path + query;

            var dictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(JsonConvert.SerializeObject(source));
            var response = await Post(url, dictionary);
            return JsonConvert.DeserializeObject<GetOnlinePlayersResponse>(response);
        }
        public async Task<string> Post(string url, Dictionary<string, string> postData, int retry = 3)
        {
            HttpResponseMessage response = null;
            var apiResInfo = new ApiResponseData();
            try
            {
                using (var request = _httpClientFactory.CreateClient("log"))
                {
                    request.Timeout = TimeSpan.FromSeconds(14);
                    var content = new FormUrlEncodedContent(postData);
                    var sw = System.Diagnostics.Stopwatch.StartNew();
                    response = await request.PostAsync(Platform.PG, url, content);
                    sw.Stop();
                    apiResInfo.ElapsedMilliseconds = sw.ElapsedMilliseconds;
                    response.EnsureSuccessStatusCode();
                    var body = await response.Content.ReadAsStringAsync();
                    try
                    {
                        //拉注單log過長截斷
                        if (url.Contains("Bet/v4/GetHistory") && url.Length > 10000)
                        {
                            var reponse = body.Substring(0, 9999);
                            var dics = new Dictionary<string, object>
                            {
                                { "request", JsonConvert.SerializeObject(postData) },
                                { "response", reponse }
                            };
                            using (var scope = _logger.BeginScope(dics))
                            {
                                _logger.LogInformation("Get RequestPath: {RequestPath} | ResponseHttpStatus:{Status} | exeTime:{exeTime} ms", url, response.StatusCode, sw.Elapsed.TotalMilliseconds);
                            }
                        }
                        else
                        {
                            var dics = new Dictionary<string, object>
                            {
                                { "request", JsonConvert.SerializeObject(postData) },
                                { "response", body }
                            };
                            using (var scope = _logger.BeginScope(dics))
                            {
                                _logger.LogInformation("Get RequestPath: {RequestPath} | ResponseHttpStatus:{Status} | exeTime:{exeTime} ms", url, response.StatusCode, sw.Elapsed.TotalMilliseconds);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                        var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                        _logger.LogError("log exception EX : {ex}  MSG : {Message} Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
                    }

                    return body;
                }

            }
            catch (HttpRequestException ex)
            {
                if (retry == 0)
                {
                    throw new Exception(string.Format("Call PgApi Failed:{0}", url));
                }

                return await Post(url, postData, retry - 1);
            }
        }

    }
}
