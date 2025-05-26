using H1_ThirdPartyWalletAPI.Code;
using H1_ThirdPartyWalletAPI.Extensions;
using H1_ThirdPartyWalletAPI.Model.Config;
using H1_ThirdPartyWalletAPI.Model.Game.FC.Enum;
using H1_ThirdPartyWalletAPI.Model.Game.FC.Request;
using H1_ThirdPartyWalletAPI.Model.Game.FC.Response;
using H1_ThirdPartyWalletAPI.Service.Common;
using H1_ThirdPartyWalletAPI.Service.Game.FC.Utility;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace H1_ThirdPartyWalletAPI.Service.Game.FC
{
    public class FCApiService : IFCApiService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<FCApiService> _logger;
        public FCApiService(IHttpClientFactory httpClientFactory, ILogger<FCApiService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        private async Task<TResponse> ApiHandle<TRequest, TResponse>(string url, TRequest source)
        {
            IsoDateTimeConverter converter = new IsoDateTimeConverter
            {
                DateTimeFormat = "yyyy-MM-dd HH:mm:ss"
            };
            var json = JsonConvert.SerializeObject(source, converter);
            try
            {

                var headers = new Dictionary<string, string>
            {
                {"Content-Type", "application/x-www-form-urlencoded"},
            };
                var postData = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);

                var responseJson = await Post(url, postData, json, headers);
                //var responseJson = Helper.DESDecrypt(responseData, Config.CompanyToken.FC_Key, Config.CompanyToken.FC_IV);
                return JsonConvert.DeserializeObject<TResponse>(responseJson);
            }
            catch (Exception ex)
            {
                _logger.LogDebug("ApiHandle: {RequestPath} | json:{json}", url, json);
                throw;
            }

        }
        /// <summary>
        /// POST
        /// </summary>
        /// <param name="url"></param>
        /// <param name="postData"></param>
        /// <param name="reqJson"></param>
        /// <param name="headers"></param>
        /// <param name="retry"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private async Task<string> Post(string url, Dictionary<string, string> postData, string reqJson, Dictionary<string, string> headers = null, int retry = 0)
        {
            HttpResponseMessage response = null;
            var apiResInfo = new ApiResponseData();
            try
            {
                //HttpClientHandler handler = new HttpClientHandler()
                //{
                //    AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
                //};
                using (var request = _httpClientFactory.CreateClient("log"))
                {
                    if (headers != null)
                    {
                        foreach (var item in headers)
                        {
                            request.DefaultRequestHeaders.TryAddWithoutValidation(item.Key, item.Value);
                        }
                    }
                    
                    if (url.Contains("GetRecordList") || url.Contains("GetHistoryRecordList"))
                    {
                        request.Timeout = TimeSpan.FromSeconds(60);
                    }
                    else
                    {
                        request.Timeout = TimeSpan.FromSeconds(14);
                    }


                    var chiphertext = AESHelper.AesEncrypt(reqJson, Config.CompanyToken.FC_AgentKey);
                    var sign = MD5Helper.Encrypt(reqJson);
                    var _request = new RequestModel(Config.CompanyToken.FC_AgentCode, "THB", chiphertext, sign);

                    IsoDateTimeConverter converter = new IsoDateTimeConverter
                    {
                        DateTimeFormat = "yyyy-MM-dd HH:mm:ss"
                    };
                    var json = JsonConvert.SerializeObject(_request, converter);
                    var requestData = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
                    var content = new FormUrlEncodedContent(requestData);
                    var sw = System.Diagnostics.Stopwatch.StartNew();
                    response = await request.PostAsync(Platform.FC, url, content);
                    sw.Stop();
                    apiResInfo.ElapsedMilliseconds = sw.ElapsedMilliseconds;
                    response.EnsureSuccessStatusCode();
                    string body = "";
                    IEnumerable<string> values;
                    if (response.Content.Headers.TryGetValues("Content-Encoding", out values) && values.Contains("gzip"))
                    {
                        var gzip = new GZipStream(await response.Content.ReadAsStreamAsync(), CompressionMode.Decompress);
                        var sr = new StreamReader(gzip, Encoding.UTF8);

                        body = await sr.ReadToEndAsync();
                    }
                    else
                    {
                        body = await response.Content.ReadAsStringAsync();
                    }
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
                            { "request", json },
                            { "response", responselog }
                        };
                        using (var scope = _logger.BeginScope(dics))
                        {
                            _logger.LogInformation("Get RequestPath: {RequestPath} | ResponseHttpStatus:{Status} | exeTime:{exeTime} ms", url, response.StatusCode, sw.Elapsed.TotalMilliseconds);
                        }


                        //try
                        //{
                        //    var _FCBaseStatusRespones = JsonConvert.DeserializeObject<FCBaseStatusRespones>(body);
                        //    if (_FCBaseStatusRespones.status == (int)ErrorCodeEnum.Unable_to_proceed_please_try_again_later)
                        //    {
                        //        //api建議20~30秒爬一次
                        //        await Task.Delay(20010);
                        //        return await Post(url, postData, reqJson, headers, 0);
                        //    }
                        //}
                        //catch
                        //{
                        //    _logger.LogError("Post FCBaseStatusResponesError: {url} | body:{body} | reqJson:{reqJson}", url, body, reqJson);
                        //}
                    }
                    catch
                    {

                    }
                    return body;
                }

            }
            catch (HttpRequestException ex)
            {
                _logger.LogError("Post FC HttpRequestException Error: {url} | reqJson:{reqJson}", url, reqJson);
                if (retry == 0)
                {
                    throw new Exception(string.Format("Call FCApi Failed:{0} | Msg:{1}| reqJson:{2}", url, ex.Message, reqJson));
                }
                return await Post(url, postData, reqJson, headers, retry - 1);
            }
        }

        /// <summary>
        /// 建立會員
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public async Task<CreateMemberResponse> CreateMember(CreateMemberRequest source)
        {
            var url = Config.GameAPI.FC_URL + "AddMember";
            var resultResponse = await ApiHandle<CreateMemberRequest, CreateMemberResponse>(url, source);
            return resultResponse;
        }

        /// <summary>
        /// 3-9、取得游戏列表
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public async Task<GetGameListResponse> GetGameList(GetGameListRequest source)
        {
            var url = Config.GameAPI.FC_URL + "GetGameList";
            var resultResponse = await ApiHandle<GetGameListRequest, GetGameListResponse>(url, source);
            return resultResponse;
        }

        /// <summary>
        /// 3-6、查询玩家基本信息
        /// 查询玩家信息时使用。
        /// 包含在线资讯、持有点数、语系。
        /// 当玩家处于鱼机大厅时，参数 OnlineType 将回传 1。
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<SearchMemberResponse> SearchMember(SearchMemberRequest source)
        {
            var url = Config.GameAPI.FC_URL + "SearchMember";
            var resultResponse = await ApiHandle<SearchMemberRequest, SearchMemberResponse>(url, source);
            return resultResponse;
        }
        /// <summary>
        /// 3-7、玩家钱包充提
        /// 当玩家要需充值或提款时使用，由此功能来操作玩家在 FC 内的钱包，替玩家 在 FC 内的
        /// 钱包进行充值，或是将 FC 钱包内的的金额转回商户的钱包。
        /// 充提点数上限为 13 位数 (不含小數)。
        /// TrsID、BankID 为唯一值
        /// 若呼叫该 API 未收到响应，需再次执行时，请带入相同的 TrsID，避免重复执行。
        /// 当设定参数 AllOut 为 1 时，可不必带入 points 参数。
        /// 当设定参数 AllOut 为 1 时，不论是否带入 points，皆直接领出现有金额（包含小数字后两位）
        /// 玩家在进行 SLOT 游戏时，存提款不受影响
        /// 玩家在进行鱼机、推币机游戏时，存款不受影响，不得提款会收到错误讯息 503。
        /// 当玩家提款点数大于玩家持有点数时不得提款，会收到错误讯息 203。
        /// 当提供之对应单号重复时，会收到错误讯息 205 ，并同时回传单号。
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>

        public async Task<SetPointsResponse> SetPoints(SetPointsRequest source)
        {
            var url = Config.GameAPI.FC_URL + "SetPoints";
            var resultResponse = await ApiHandle<SetPointsRequest, SetPointsResponse>(url, source);

            //若存取款回應會員在線則延遲0.5S後重新發送API一次
            if (resultResponse.Result == (int)ErrorCodeEnum.Member_Is_Online)
            {
                await Task.Delay(500);
                resultResponse = await ApiHandle<SetPointsRequest, SetPointsResponse>(url, source);
            }
            return resultResponse;
        }

        /// <summary>
        /// 3-3、登录游戏
        /// 玩家使用平台帐号进入 FC 游戏时使用
        /// 如果该玩家在 FC 游戏没有账号，会自动创建后再进行登录
        /// GameID 与 LoginGamHall，两者都带入会先优判断 LoginGameHall。
        /// 当 LoginGameHall 为 true，GameHallGameType 没有带入时，预设 GameType 全开
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public async Task<LoginResponse> Login(LoginRequest source)
        {
            var url = Config.GameAPI.FC_URL + "Login";
            var resultResponse = await ApiHandle<LoginRequest, LoginResponse>(url, source);
            return resultResponse;
        }

        /// <summary>
        /// 3-4、踢出玩家 0 504 
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public async Task<KickOutResponse> KickOut(KickOutRequest source)
        {
            var url = Config.GameAPI.FC_URL + "KickOut";
            var resultResponse = await ApiHandle<KickOutRequest, KickOutResponse>(url, source);
            return resultResponse;
        }

        /// <summary>
        /// 3-5、踢出全部玩家 0 999
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<KickoutAllResponse> KickoutAll(KickoutAllRequest source)
        {
            var url = Config.GameAPI.FC_URL + "KickoutAll";
            var resultResponse = await ApiHandle<KickoutAllRequest, KickoutAllResponse>(url, source);
            return resultResponse;
        }

        /// <summary>
        /// 3-8、交易纪录单笔查询
        /// 以交易单号或对应单号查询单笔交易记录时使用
        /// 可择一带入，若同时带入两参数，则必须完全比对成功，才会响应该交易单。
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public async Task<GetSingleBillResponse> GetSingleBill(GetSingleBillRequest source)
        {
            var url = Config.GameAPI.FC_URL + "GetSingleBill";
            var resultResponse = await ApiHandle<GetSingleBillRequest, GetSingleBillResponse>(url, source);
            return resultResponse;
        }

        /// <summary>
        /// 3-10、取得玩家报表
        /// 查询玩家游戏报表
        /// 未带入 RecordID 会进入报表主页
        /// 有带入 GameType 则会导向对应的游戏类别之报表主页
        /// 有带入 RecordID 会直接进入该游戏的详细讯息页面，并且不参考 GameType
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public async Task<GetPlayerReportResponse> GetPlayerReport(GetPlayerReportRequest source)
        {
            var url = Config.GameAPI.FC_URL + "GetPlayerReport";
            var resultResponse = await ApiHandle<GetPlayerReportRequest, GetPlayerReportResponse>(url, source);
            return resultResponse;
        }


        /// <summary>
        /// 3-14、取得游戏纪录列表
        /// 查询在特定时间内的游戏记录。
        /// 每次查询时间范围最多为 15 分钟。
        /// 仅能查询 2 小时以內的游戏纪录。
        /// 仅提供 60 天内的游戏纪录。
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public async Task<GetRecordListResponse> GetRecordList(GetRecordListRequest source)
        {
            var url = Config.GameAPI.FC_URL + "GetRecordList";
            var resultResponse = await ApiHandle<GetRecordListRequest, GetRecordListResponse>(url, source);
            return resultResponse;
        }


        /// <summary>
        /// 3-15、取得充提交易纪录
        /// 查询交易记录时使用。
        /// 每次查询时间范围最多为 15 分钟。
        /// 建议每次查询范围不超过 1 分钟，以获得较佳效果。
        /// 仅提供 60 天内的交易结果。
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public async Task<GetBillListResponse> GetBillList(GetBillListRequest source)
        {
            var url = Config.GameAPI.FC_URL + "GetBillList";
            var resultResponse = await ApiHandle<GetBillListRequest, GetBillListResponse>(url, source);
            return resultResponse;
        }


        /// <summary>
        ///  3-16、每日会员游戏报表
        ///  查询单一代理在特定日期内的游戏详细交易信息
        ///  带入日期仅参考 年-月-日
        ///  仅提供 60 天内的交易结果
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public async Task<GetMemberGameReportResponse> GetMemberGameReport(GetMemberGameReportRequest source)
        {
            var url = Config.GameAPI.FC_URL + "GetMemberGameReport";
            var resultResponse = await ApiHandle<GetMemberGameReportRequest, GetMemberGameReportResponse>(url, source);
            return resultResponse;
        }

        /// <summary>
        /// 3-17、取得游戏缩图清单
        /// 取得游戏缩图与清单时使用
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public async Task<GetGameIconListResponse> GetGameIconList(GetGameIconListRequest source)
        {
            GetGameIconListResponse resultResponse;
            var url = Config.GameAPI.FC_URL + "GetGameIconList";
            //   var resultResponse = await ApiHandle<GetGameIconListRequest, GetGameIconListResponse>(url, source);
            IsoDateTimeConverter converter = new IsoDateTimeConverter
            {
                DateTimeFormat = "yyyy-MM-dd HH:mm:ss"
            };
            var json = JsonConvert.SerializeObject(source, converter);
            try
            {

                var headers = new Dictionary<string, string>
            {
                {"Content-Type", "application/x-www-form-urlencoded"},
            };
                var postData = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);

                var responseJson = await Post(url, postData, json, headers);

                resultResponse = JsonConvert.DeserializeObject<GetGameIconListResponse>(responseJson);

                //JsonConvert.DeserializeXNode
                ////var responseJson = Helper.DESDecrypt(responseData, Config.CompanyToken.FC_Key, Config.CompanyToken.FC_IV);
                //resultResponse = JsonConvert.DeserializeObject<GetGameIconListResponse>(responseJson);
            }
            catch (Exception ex)
            {
                _logger.LogDebug("ApiHandle: {RequestPath} | json:{json}", url, json);
                throw;
            }

            return resultResponse;
        }

        /// <summary>
        /// 3-20、取得历史游戏纪录列表
        /// 查询在特定时间内的游戏记录。
        /// 每次查询时间范围最多为 15 分钟。
        /// 仅能查询 2 小时以前的游戏纪录。
        /// 仅提供 60 天内的游戏纪录。
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public async Task<GetHistoryRecordListResponse> GetHistoryRecordList(GetHistoryRecordListRequest source)
        {
            var url = Config.GameAPI.FC_URL + "GetHistoryRecordList";
            var resultResponse = await ApiHandle<GetHistoryRecordListRequest, GetHistoryRecordListResponse>(url, source);
            return resultResponse;
        }

        public async Task<GetCurrencyReportResponse> GetCurrencyReport(GetCurrencyReportRequest source)
        {
            var url = Config.GameAPI.FC_URL + "GetCurrencyReport";
            var resultResponse = await ApiHandle<GetCurrencyReportRequest, GetCurrencyReportResponse>(url, source);
            return resultResponse;
        }


    }
}
