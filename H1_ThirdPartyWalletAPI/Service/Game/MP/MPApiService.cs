using H1_ThirdPartyWalletAPI.Code;
using H1_ThirdPartyWalletAPI.Extensions;
using H1_ThirdPartyWalletAPI.Model;
using H1_ThirdPartyWalletAPI.Model.Config;
using H1_ThirdPartyWalletAPI.Model.Game.MP.Request;
using H1_ThirdPartyWalletAPI.Model.Game.MP.Response;
using H1_ThirdPartyWalletAPI.Service.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using ThirdPartyWallet.Common;


namespace H1_ThirdPartyWalletAPI.Service.Game.MP
{
    public class MPApiService : IMPApiService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IApiHealthCheckService _apiHealthCheckService;
        private readonly LogHelper<MPApiService> _logger;
        public MPApiService(IHttpClientFactory httpClientFactory, LogHelper<MPApiService> logger, IApiHealthCheckService apiHealthCheckService)
        {
            _httpClientFactory = httpClientFactory;
            _apiHealthCheckService = apiHealthCheckService;
            _logger = logger;
        }
        /// <summary>
        /// 登入-建立會員
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public async Task<LoginToPlatformResponse> LoginToPlatformAsync(LoginToPlatformParam source)
        {
            var datetime = DateTimeOffset.UtcNow;
            source.orderid = Config.CompanyToken.MP_Id + datetime.ToString("yyyyMMddHHmmsss") + source.account;
            source.lineCode = "RoyalOnline";
            source.s = "0";
            var Dic = Helper.GetDictionary(source);
            var DataString = String.Join("&", Dic.Select(x => x.Key + "=" + x.Value));

            var Request = new MPRequest();
            Request.agent = Config.CompanyToken.MP_Id;
            Request.timestamp = datetime.ToUnixTimeMilliseconds().ToString();
            Request.param = Helper.AesEncrypt(DataString, Config.CompanyToken.MP_Deskey);
            Request.key = Helper.MD5encryption(Config.CompanyToken.MP_Id, Request.timestamp, Config.CompanyToken.MP_Md5key);
            var url = Config.GameAPI.MP_URL;
            return await ApiHandle<MPRequest, LoginToPlatformResponse>(url, Request, DataString);
        }
        /// <summary>
        /// 踢線
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public async Task<KickPlayerOfflineResponse> KickPlayerOfflineAsync(KickPlayerOfflineParam source)
        {
            var datetime = DateTimeOffset.UtcNow;
            source.s = "8";
            var Dic = Helper.GetDictionary(source);
            var DataString = String.Join("&", Dic.Select(x => x.Key + "=" + x.Value));

            var Request = new MPRequest();
            Request.agent = Config.CompanyToken.MP_Id;
            Request.timestamp = datetime.ToUnixTimeMilliseconds().ToString();
            Request.param = Helper.AesEncrypt(DataString, Config.CompanyToken.MP_Deskey);
            Request.key = Helper.MD5encryption(Config.CompanyToken.MP_Id, Request.timestamp, Config.CompanyToken.MP_Md5key);
            var url = Config.GameAPI.MP_URL;
            return await ApiHandle<MPRequest, KickPlayerOfflineResponse>(url, Request, DataString);
        }
        /// <summary>
        /// 查詢金額
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public async Task<LnquiryScoreStatusResponse> LnquiryScoreStatusAsync(LnquiryScoreStatusParam source)
        {
            var datetime = DateTimeOffset.UtcNow;
            source.s = "7";
            source.AESKey = Config.CompanyToken.MP_Deskey;
            var Dic = Helper.GetDictionary(source);
            var DataString = String.Join("&", Dic.Select(x => x.Key + "=" + x.Value));

            var Request = new MPRequest();
            Request.agent = Config.CompanyToken.MP_Id;
            Request.timestamp = datetime.ToUnixTimeMilliseconds().ToString();
            Request.param = Helper.AesEncrypt(DataString, Config.CompanyToken.MP_Deskey);
            Request.key = Helper.MD5encryption(Config.CompanyToken.MP_Id, Request.timestamp, Config.CompanyToken.MP_Md5key);
            var url = Config.GameAPI.MP_URL;
            return await ApiHandle<MPRequest, LnquiryScoreStatusResponse>(url, Request, DataString);

        }
        /// <summary>
        /// 上分
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public async Task<FundInResponse> FundInAsync(KFundInParam source)
        {
            var datetime = DateTimeOffset.UtcNow;
            source.s = "2";
            source.AESKey = Config.CompanyToken.MP_Deskey;
            //source.orderid = Config.CompanyToken.MP_Id + datetime.ToString("yyyyMMddHHmmsss") + source.account;
            var Dic = Helper.GetDictionary(source);
            var DataString = String.Join("&", Dic.Select(x => x.Key + "=" + x.Value));

            var Request = new MPRequest();
            Request.agent = Config.CompanyToken.MP_Id;
            Request.timestamp = datetime.ToUnixTimeMilliseconds().ToString();
            Request.param = Helper.AesEncrypt(DataString, Config.CompanyToken.MP_Deskey);
            Request.key = Helper.MD5encryption(Config.CompanyToken.MP_Id, Request.timestamp, Config.CompanyToken.MP_Md5key);
            var url = Config.GameAPI.MP_URL;
            return await ApiHandle<MPRequest, FundInResponse>(url, Request, DataString);
        }
        /// <summary>
        /// 下分
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>，
        public async Task<FundOutResponse> FundOutAsync(KFundOutParam source)
        {
            var datetime = DateTimeOffset.UtcNow;
            source.s = "3";
            source.AESKey = Config.CompanyToken.MP_Deskey;
            //source.orderid = Config.CompanyToken.MP_Id + datetime.ToString("yyyyMMddHHmmsss") + source.account;
            var Dic = Helper.GetDictionary(source);
            var DataString = String.Join("&", Dic.Select(x => x.Key + "=" + x.Value));

            var Request = new MPRequest();
            Request.agent = Config.CompanyToken.MP_Id;
            Request.timestamp = datetime.ToUnixTimeMilliseconds().ToString();
            Request.param = Helper.AesEncrypt(DataString, Config.CompanyToken.MP_Deskey);
            Request.key = Helper.MD5encryption(Config.CompanyToken.MP_Id, Request.timestamp, Config.CompanyToken.MP_Md5key);
            var url = Config.GameAPI.MP_URL;
            return await ApiHandle<MPRequest, FundOutResponse>(url, Request, DataString);
        }

        /// <summary>
        /// 查詢交易狀態
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public async Task<InquiryaboutOrderStatusResponse> InquiryaboutOrderStatusAsync(InquiryaboutOrderparam source)
        {
            var datetime = DateTimeOffset.UtcNow;
            source.s = "4";
            source.AESKey = Config.CompanyToken.MP_Deskey;
            var Dic = Helper.GetDictionary(source);
            var DataString = String.Join("&", Dic.Select(x => x.Key + "=" + x.Value));

            var Request = new MPRequest();
            Request.agent = Config.CompanyToken.MP_Id;
            Request.timestamp = datetime.ToUnixTimeMilliseconds().ToString();
            Request.param = Helper.AesEncrypt(DataString, Config.CompanyToken.MP_Deskey);
            Request.key = Helper.MD5encryption(Config.CompanyToken.MP_Id, Request.timestamp, Config.CompanyToken.MP_Md5key);
            var url = Config.GameAPI.MP_URL;
            return await ApiHandle<MPRequest, InquiryaboutOrderStatusResponse>(url, Request, DataString);
        }
        /// <summary>
        /// 拉匯總
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public async Task<CheckSummaryResponse> CheckSummaryAsync(CheckSummaryparam source)
        {
            var datetime = DateTimeOffset.UtcNow;
            source.s = "61";
            var Dic = Helper.GetDictionary(source);
            var DataString = String.Join("&", Dic.Select(x => x.Key + "=" + x.Value));

            var Request = new MPRequest();
            Request.agent = Config.CompanyToken.MP_Id;
            Request.timestamp = datetime.ToUnixTimeMilliseconds().ToString();
            Request.param = Helper.AesEncrypt(DataString, Config.CompanyToken.MP_Deskey);
            Request.key = Helper.MD5encryption(Config.CompanyToken.MP_Id, Request.timestamp, Config.CompanyToken.MP_Md5key);
            var url = Config.GameAPI.MP_Record_URL;
            var data = await ApiHandle<MPRequest, CheckSummaryResponse>(url, Request, DataString);
            data.d.Transactions[0].totalBetAmount = data.d.Transactions[0].totalBetAmount / 100;
            data.d.Transactions[0].playerPL = data.d.Transactions[0].playerPL / 100;
            data.d.Transactions[0].totalWinAmount = data.d.Transactions[0].totalWinAmount / 100;
            return data;
        }
        /// <summary>
        /// 拉單
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<List<MPData>> PullGameBettingSlipAsync(PullGameBettingSlipParam source)
        {
            var datetime = DateTimeOffset.UtcNow;
            source.s = "6";
            source.AESKey = Config.CompanyToken.MP_Deskey;
            var Dic = Helper.GetDictionary(source);
            var DataString = String.Join("&", Dic.Select(x => x.Key + "=" + x.Value));

            var Request = new MPRequest();
            Request.agent = Config.CompanyToken.MP_Id;
            Request.timestamp = datetime.ToUnixTimeMilliseconds().ToString();
            Request.param = Helper.AesEncrypt(DataString, Config.CompanyToken.MP_Deskey);
            Request.key = Helper.MD5encryption(Config.CompanyToken.MP_Id, Request.timestamp, Config.CompanyToken.MP_Md5key);
            var url = Config.GameAPI.MP_Record_URL;
            var resdata = await ApiHandle<MPRequest, PullGameBettingSlipResponse>(url, Request, DataString);
            List<MPData> MPdatalist = new List<MPData>();
            if (resdata.d.code == 16)
            {
                return MPdatalist;
            }
            int[] arrayLengths = new int[]
             {
                resdata.d.list.GameID.Length,
                resdata.d.list.Accounts.Length,
                resdata.d.list.ServerID.Length,
                resdata.d.list.KindID.Length,
                resdata.d.list.TableID.Length,
                resdata.d.list.ChairID.Length,
                resdata.d.list.UserCount.Length,
                resdata.d.list.CellScore.Length,
                resdata.d.list.AllBet.Length,
                resdata.d.list.Profit.Length,
                resdata.d.list.Revenue.Length,
                resdata.d.list.NewScore.Length,
                resdata.d.list.GameStartTime.Length,
                resdata.d.list.GameEndTime.Length,
                resdata.d.list.CardValue.Length,
                resdata.d.list.ChannelID.Length,
                resdata.d.list.LineCode.Length
             };
            if (!arrayLengths.All(x => x == arrayLengths[0]))
            {
                throw new Exception("MP格式錯誤，屬性長度不同");
            }

            for (int i = 0; i < arrayLengths[0]; i++)
            {
                MPData mPData = new MPData()
                {
                    GameID = resdata.d.list.GameID[i],
                    Accounts = resdata.d.list.Accounts[i],
                    ServerID = resdata.d.list.ServerID[i],
                    KindID = resdata.d.list.KindID[i],
                    TableID = resdata.d.list.TableID[i],
                    ChairID = resdata.d.list.ChairID[i],
                    UserCount = resdata.d.list.UserCount[i],
                    CellScore = resdata.d.list.CellScore[i],
                    AllBet = resdata.d.list.AllBet[i],
                    Profit = resdata.d.list.Profit[i],
                    Revenue = resdata.d.list.Revenue[i],
                    NewScore = resdata.d.list.NewScore[i],
                    GameStartTime = DateTime.Parse(resdata.d.list.GameStartTime[i]),
                    GameEndTime = DateTime.Parse(resdata.d.list.GameEndTime[i]),
                    CardValue = resdata.d.list.CardValue[i],
                    ChannelID = resdata.d.list.ChannelID[i],
                    LineCode = resdata.d.list.LineCode[i]
                };
                MPdatalist.Add(mPData);
            }
            return MPdatalist;
        }

        /// <summary>
        /// 查詢全平台狀態
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public async Task<GetPlatformStatusResponse> GetPlatformStatus(GetPlatformStatusRequest source)
        {
            var datetime = DateTimeOffset.UtcNow;
            source.s = "16";
            var Dic = Helper.GetDictionary(source);
            var DataString = String.Join("&", Dic.Select(x => x.Key + "=" + x.Value));

            var Request = new MPRequest();
            Request.agent = Config.CompanyToken.MP_Id;
            Request.timestamp = datetime.ToUnixTimeMilliseconds().ToString();
            Request.param = Helper.AesEncrypt(DataString, Config.CompanyToken.MP_Deskey);
            Request.key = Helper.MD5encryption(Config.CompanyToken.MP_Id, Request.timestamp, Config.CompanyToken.MP_Md5key);
            var url = Config.GameAPI.MP_Record_URL;
            var data = await ApiHandle<MPRequest, GetPlatformStatusResponse>(url, Request, DataString);
            return data;
        }

        private async Task<TResponse> ApiHandle<TRequest, TResponse>(string url, TRequest source, string DataJson)
        {
            var Dic = Helper.GetDictionary(source);

            var DataString = String.Join("&", Dic.Select(x => x.Key + "=" + x.Value));

            url = url + "?" + DataString;
            var headers = new Dictionary<string, string>
            {
                { "Content-Type","text/plain;charset=utf-8" },
            };

            var postData = new Dictionary<string, string>
            {
            };
            var responseData = await Get(url, Dic, DataJson, headers);

            return JsonConvert.DeserializeObject<TResponse>(responseData);

        }
        private async Task<string> Get(string url, Dictionary<string, string> postData, string DataJson, Dictionary<string, string> headers = null)
        {
            var apiResInfo = new ApiResponseData();
            try
            {
                using (var request = _httpClientFactory.CreateClient())
                {

                    if (headers != null)
                    {
                        foreach (var item in headers)
                        {
                            request.DefaultRequestHeaders.TryAddWithoutValidation(item.Key, item.Value);
                        }
                    }

                    request.Timeout = TimeSpan.FromSeconds(14);

                    var sw = System.Diagnostics.Stopwatch.StartNew();


                    var response = await request.GetAsync(Platform.MP, url);
                    sw.Stop();
                    apiResInfo.ElapsedMilliseconds = sw.ElapsedMilliseconds;
                    _apiHealthCheckService.SetResponseData(Platform.MP, apiResInfo);

                    if ((int)response.StatusCode != 200)
                        throw new Exception(string.Format("Call MP Failed! url:{0} Postdata:{1} status:{2}", url, JsonConvert.SerializeObject(postData), response.StatusCode.ToString()));

                    using var res = new StreamReader(await response.Content.ReadAsStreamAsync(), Encoding.UTF8);
                    var body = await res.ReadToEndAsync();

                    var responselog = "";
                    if (body.Length > 10000)
                    {
                        responselog = body.Substring(0, 9999);
                    }
                    else
                    {
                        responselog = body;
                    }

                    _logger.APILog(
                      Platform.MP.ToString()
                     , Config.GameAPI.MP_URL
                     , "GET"
                     , JsonConvert.SerializeObject(postData) + $" ApiData:{DataJson}"
                     , responselog
                     , (int)response.StatusCode
                     , (long)sw.Elapsed.TotalMilliseconds);

                    return body;
                }
            }
            catch (TaskCanceledException ex)
            {
                apiResInfo.ElapsedMilliseconds = 99999;
                _apiHealthCheckService.SetResponseData(Platform.MP, apiResInfo);
                throw ex;
            }
            catch (Exception ex)
            {

                var httpStatusCode = 500;
                if (ex is HttpRequestException requestException)
                {
                    httpStatusCode = (int)requestException.StatusCode;
                }

                _logger.APIErrorLog(ex
                    , Platform.MP.ToString()
                    , Config.GameAPI.MP_URL
                    , "GET"
                    , JsonConvert.SerializeObject(postData) + $" ApiData:{DataJson}"
                    , string.Empty
                    , httpStatusCode
                    , 0L);

                throw;
            }
        }
    }
}
