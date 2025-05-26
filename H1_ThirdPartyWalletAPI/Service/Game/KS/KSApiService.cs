using H1_ThirdPartyWalletAPI.Code;
using H1_ThirdPartyWalletAPI.Extensions;
using H1_ThirdPartyWalletAPI.Model.Config;
using H1_ThirdPartyWalletAPI.Model.Game.KS.Request;
using H1_ThirdPartyWalletAPI.Model.Game.KS.Response;
using H1_ThirdPartyWalletAPI.Service.Common;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace H1_ThirdPartyWalletAPI.Service.Game.KS
{
    public class KSApiService : IKSApiService
    {
        private string ContentLanguage = "ENG";
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<KSApiService> _logger;
        public KSApiService(IHttpClientFactory httpClientFactory, ILogger<KSApiService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }


        /// <summary>
        /// 設定語系
        /// </summary>
        /// <param name="language"></param>
        public void SetContentLanguage(string language)
        {
            this.ContentLanguage = language;
        }


        private async Task<KSBaseRespones<TResponse>> ApiHandle<TRequest, TResponse>(string url, TRequest source)
        {
            IsoDateTimeConverter converter = new IsoDateTimeConverter
            {
                DateTimeFormat = "yyyy-MM-dd HH:mm:ss"
            };
            var json = JsonConvert.SerializeObject(source, converter);
            try
            {
                _logger.LogDebug("ContentLanguage :{ContentLanguage}", ContentLanguage);

                var headers = new Dictionary<string, string>
                {
                 {"Authorization", Config.CompanyToken.KS_key},
                };
                var postData = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);

                var responseJson = await Post(url, postData, json, headers);

                KSBaseRespones<TResponse> kSBaseRespones = new KSBaseRespones<TResponse>();
                var resultResponse = JsonConvert.DeserializeObject<KSBaseRespones<object>>(responseJson);

                if (resultResponse.success == 1)
                {

                    if (resultResponse.info != null)
                    {
                        TResponse info = JsonConvert.DeserializeObject<TResponse>(resultResponse.info.ToString());
                        kSBaseRespones.info = info;
                    }
                }
                else
                {
                    Info info = JsonConvert.DeserializeObject<Info>(resultResponse.info.ToString());
                    kSBaseRespones.Error = info.Error;
                }
                kSBaseRespones.success = resultResponse.success;
                kSBaseRespones.msg = resultResponse.msg;



                return kSBaseRespones;
            }
            catch (Exception ex)
            {
                KSBaseRespones<TResponse> kSBaseRespones = new KSBaseRespones<TResponse>();
                kSBaseRespones.Error = ex.Message;

                _logger.LogDebug("ApiHandle: {RequestPath} | json:{json}, {StackTrace}", url, json, ex.StackTrace);

                return kSBaseRespones;
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

                    if (url.Contains("api/log/get"))
                    {
                        request.Timeout = TimeSpan.FromSeconds(60);
                    }
                    else
                    {
                        request.Timeout = TimeSpan.FromSeconds(14);
                    }

                    var content = new FormUrlEncodedContent(postData);
                    content.Headers.Add("Content-Language", ContentLanguage);
                    response = await request.PostAsync(Platform.KS, url, content);


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
                            { "request", postData },
                            { "response", responselog }
                        };
                        using (var scope = _logger.BeginScope(dics))
                        {
                            _logger.LogInformation("Get RequestPath: {RequestPath} | ResponseHttpStatus:{Status} ", url, response.StatusCode);
                        }


                        //try
                        //{
                        //    var _KSBaseStatusRespones = JsonConvert.DeserializeObject<KSBaseRespones>(body);
                        //    if (_KSBaseStatusRespones.status == (int)ErrorCodeEnum.Unable_to_proceed_please_try_again_later)
                        //    {
                        //        //api建議20~30秒爬一次
                        //        await Task.Delay(20010);
                        //        return await Post(url, postData, reqJson, headers, 0);
                        //    }
                        //}
                        //catch
                        //{
                        //    _logger.LogError("Post KSBaseStatusResponesError: {url} | body:{body} | reqJson:{reqJson}", url, body, reqJson);
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
                _logger.LogError("Post KS HttpRequestException Error: {url} | reqJson:{reqJson}", url, reqJson);
                if (retry == 0)
                {
                    throw new Exception(string.Format("Call KSApi Failed:{0} | Msg:{1}| reqJson:{2}", url, ex.Message, reqJson));
                }
                return await Post(url, postData, reqJson, headers, retry - 1);
            }
        }

        /// <summary>
        /// 2.2 register / 会员注册
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public async Task<KSBaseRespones<UserRegisterResponse>> UserRegister(UserRegisterRequest source)
        {
            source.Password = "a123456";
            source.Currency = Model.Game.KS.KS.Currency["THB"];
            var url = Config.GameAPI.KS_URL + "api/user/register";
            var resultResponse = await ApiHandle<UserRegisterRequest, UserRegisterResponse>(url, source);
            return resultResponse;
        }


        /// <summary>
        /// 2.3 login / 会员登录
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public async Task<KSBaseRespones<UserLoginResponse>> UserLogin(UserLoginRequest source)
        {
            var url = Config.GameAPI.KS_URL + "api/user/login";
            var resultResponse = await ApiHandle<UserLoginRequest, UserLoginResponse>(url, source);
            return resultResponse;
        }


        /// <summary>
        /// 2.4 logout / 会员下线
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public async Task<KSBaseRespones<UserLogoutResponse>> UserLogout(UserLogoutRequest source)
        {
            var url = Config.GameAPI.KS_URL + "api/user/logout";
            var resultResponse = await ApiHandle<UserLogoutRequest, UserLogoutResponse>(url, source);
            return resultResponse;
        }

        /// <summary>
        /// 2.5.1 balance / 查询余额
        /// <param name="source"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<KSBaseRespones<UserBalanceResponse>> UserBalance(UserBalanceRequest source)
        {
            var url = Config.GameAPI.KS_URL + "api/user/balance";
            var resultResponse = await ApiHandle<UserBalanceRequest, UserBalanceResponse>(url, source);
            return resultResponse;
        }

        /// <summary>
        /// 2.5.2 translate / 转账
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public async Task<KSBaseRespones<UserTransferResponse>> UserTransfer(UserTransferRequest source)
        {
            var url = Config.GameAPI.KS_URL + "api/user/transfer";
            var resultResponse = await ApiHandle<UserTransferRequest, UserTransferResponse>(url, source);
            return resultResponse;
        }


        /// <summary>
        /// 2.5.3 translateinfo / 转账查询
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public async Task<KSBaseRespones<UserTransferInfoResponse>> UserTransferInfo(UserTransferInfoRequest source)
        {
            var url = Config.GameAPI.KS_URL + "api/user/transferinfo";
            var resultResponse = await ApiHandle<UserTransferInfoRequest, UserTransferInfoResponse>(url, source);
            return resultResponse;
        }

        /// <summary>
        /// 2.7 / group / 分组调整
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public async Task<KSBaseRespones<UserGroupResponse>> UserGroup(UserGroupRequest source)
        {
            var url = Config.GameAPI.KS_URL + "api/user/group";
            var resultResponse = await ApiHandle<UserGroupRequest, UserGroupResponse>(url, source);
            return resultResponse;
        }

        /// <summary>
        /// 3.3 Get / 订单拉取
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public async Task<KSBaseRespones<LogGetResponse>> LogGet(LogGetRequest source)
        {
            var url = Config.GameAPI.KS_URL + "api/log/get";
            var resultResponse = await ApiHandle<LogGetRequest, LogGetResponse>(url, source);
            return resultResponse;
        }


        /// <summary>
        /// 3.7 SiteReport / 商户报表  /日帳
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public async Task<KSBaseRespones<SiteReportResponse>> SiteReport(SiteReportRequest source)
        {
            var url = Config.GameAPI.KS_URL + "api/log/SiteReport";
            var resultResponse = await ApiHandle<SiteReportRequest, SiteReportResponse>(url, source);
            return resultResponse;
        }

        /// <summary>
        /// 3.8 BillReport / 商户账单  /日帳
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public async Task<KSBaseRespones<LogBillReprotResponse>> LogBillReprot(LogBillReprotRequest source)
        {
            var url = Config.GameAPI.KS_URL + "api/log/BillReprot";
            var resultResponse = await ApiHandle<LogBillReprotRequest, LogBillReprotResponse>(url, source);
            return resultResponse;
        }


    }
}
