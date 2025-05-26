using H1_ThirdPartyWalletAPI.Code;
using H1_ThirdPartyWalletAPI.Extensions;
using H1_ThirdPartyWalletAPI.Model.Config;
using H1_ThirdPartyWalletAPI.Model.Game.META.Enum;
using H1_ThirdPartyWalletAPI.Model.Game.META.Request;
using H1_ThirdPartyWalletAPI.Model.Game.META.Response;
using H1_ThirdPartyWalletAPI.Service.Common;
using H1_ThirdPartyWalletAPI.Service.Game.META.JsonConverter;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace H1_ThirdPartyWalletAPI.Service.Game.META
{
    public class METAApiService : IMETAApiService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<METAApiService> _logger;

        public METAApiService(IHttpClientFactory httpClientFactory, ILogger<METAApiService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        #region  API Handle
        private async Task<METABaseStatusRespones> ApiHandle<TRequest>(string url, TRequest source)
        {
            //var nowDateTime = DateTime.UtcNow;
            //var timestamp = nowDateTime.AddHours(-4).ToString("yyMMd");
            //DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            var rawData = JsonSerializer.Serialize(source, new JsonSerializerOptions
            {
                Converters = {
                    new SerializeDateTimeConverter()
                },
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            });
            var chiphertext = AesUtil.Encrypt(rawData, Config.CompanyToken.META_Key, Config.CompanyToken.META_IV);
            var postData = new Dictionary<string, string>
            {
                { "HashKey", Config.CompanyToken.META_Key },
                { "Data", chiphertext }
            };
            var headers = new Dictionary<string, string>
            {
                {"Content-Type", "application/json"},
            };
            try
            {
                var responseData = await Post(string.Format("{0}", url), postData, headers);
                var METABaseStatus = JsonSerializer.Deserialize<METABaseErroRespones>(responseData);
                if (METABaseStatus.status && METABaseStatus.code == (int)ErrorCodeEnum.Success)
                {
                    return JsonSerializer.Deserialize<METABaseStatusRespones>(responseData);
                }
                else
                {
                    METABaseStatusRespones getMetaDataResponesBase = new METABaseStatusRespones();
                    getMetaDataResponesBase.status = METABaseStatus.status;
                    getMetaDataResponesBase.code = METABaseStatus.code;
                    getMetaDataResponesBase.errMsg = METABaseStatus.errMsg;
                    return getMetaDataResponesBase;
                }
            }
            catch (TaskCanceledException ex)
            {
                throw new TaskCanceledException(ex.ToString());
            }
            catch (Exception ex)
            {
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                _logger.LogError("Meta ApiHandle exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
                METABaseStatusRespones getMetaDataResponesBase = new METABaseStatusRespones();
                getMetaDataResponesBase.status = false;
                return getMetaDataResponesBase;
            }

        }

        private async Task<string> Post(string url, Dictionary<string, string> postData, Dictionary<string, string> headers = null, int retry = 0)
        {
            HttpResponseMessage response = null;
            var apiResInfo = new ApiResponseData();
            try
            {
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
                    //var content = new FormUrlEncodedContent(postData);
                    var sw = System.Diagnostics.Stopwatch.StartNew();

                    response = await request.PostAsync(Platform.META, url, new StringContent(JsonSerializer.Serialize(postData), Encoding.UTF8));
                    sw.Stop();
                    apiResInfo.ElapsedMilliseconds = sw.ElapsedMilliseconds;
                    response.EnsureSuccessStatusCode();
                    var body = await response.Content.ReadAsStringAsync();

                    var dics = new Dictionary<string, object>
                    {
                        { "request", postData },
                        { "response", body }
                    };
                    _logger.LogInformation("Meta Post RequestPath: {RequestPath}", url);
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
                    string postDataJsonString = JsonSerializer.Serialize(postData);
                    throw new Exception(string.Format("Call METAApi Failed:{0},postData:{1}, error:{2}, error:{3}", url, postDataJsonString, ex, ex.Message));
                }

                return await Post(url, postData, headers, retry - 1);
            }
        }


        /// <summary>
        /// 共用解密
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="response"></param>
        /// <returns></returns>
        /// <exception cref="ExceptionMessage"></exception>
        public T ResultHandler<T>(METABaseStatusRespones response)
        {
            string DataDes = response.data;
            string chiphertext = AesUtil.Decrypt(DataDes, Config.CompanyToken.META_Key, Config.CompanyToken.META_IV);
            var target = JsonSerializer.Deserialize<T>(chiphertext, new JsonSerializerOptions
            {
                Converters = {
                    new SerializeDateTimeConverter()
                }
            });
            return target;
        }

        #endregion

        /// <summary>
        /// 建立會員
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public async Task<CreateMemberResponse> CreateMember(CreateMemberRequest source)
        {
            CreateMemberResponse resultResponse = new CreateMemberResponse();

            source.Password = source.Account;
            var url = Config.GameAPI.META_URL + "Member/Create";
            METABaseStatusRespones responseData = await ApiHandle(url, source);
            if (responseData.status && responseData.code == (int)ErrorCodeEnum.Success)
            {
                resultResponse = ResultHandler<CreateMemberResponse>(responseData);
                resultResponse.DecryptStatus = true;
            }
            else
            {
                resultResponse.code = responseData.code;
                resultResponse.errMsg = responseData.errMsg;
            }
            return resultResponse;
        }

        /// <summary>
        /// 取得遊戲列表
        /// </summary>
        /// <returns></returns>
        public async Task<GetGameListResponse> GetGameTableList(GetGameListRequest source)
        {
            GetGameListResponse resultResponse = new();
            var url = Config.GameAPI.META_URL + "Game/TableList";
            METABaseStatusRespones responseData = await ApiHandle<GetGameListRequest>(url, source);
            if (responseData.status && responseData.code == (int)ErrorCodeEnum.Success)
            {
                resultResponse = ResultHandler<GetGameListResponse>(responseData);
                resultResponse.DecryptStatus = true;
            }
            else
            {
                resultResponse.code = responseData.code;
                resultResponse.errMsg = responseData.errMsg;
            }
            return resultResponse;
        }


        public async Task<CheckPointResponse> CheckPoint(CheckPointRequest source)
        {
            CheckPointResponse resultResponse = new();
            var url = Config.GameAPI.META_URL + "Member/CheckPoint";
            METABaseStatusRespones responseData = await ApiHandle<CheckPointRequest>(url, source);
            if (responseData.status && responseData.code == (int)ErrorCodeEnum.Success)
            {
                resultResponse = ResultHandler<CheckPointResponse>(responseData);
                resultResponse.DecryptStatus = true;
            }
            else
            {
                resultResponse.code = responseData.code;
                resultResponse.errMsg = responseData.errMsg;
            }
            return resultResponse;
        }

        public async Task<GameLogoutResponse> GameLogout(GameLogoutRequest source)
        {
            GameLogoutResponse resultResponse = new();
            var url = Config.GameAPI.META_URL + "Game/Logout";
            METABaseStatusRespones responseData = await ApiHandle<GameLogoutRequest>(url, source);
            if (responseData.status && responseData.code == (int)ErrorCodeEnum.Success)
            {
                resultResponse = ResultHandler<GameLogoutResponse>(responseData);
                resultResponse.DecryptStatus = true;
            }
            else
            {
                resultResponse.code = responseData.code;
                resultResponse.errMsg = responseData.errMsg;
            }
            return resultResponse;
        }


        public async Task<TransPointResponse> TransPoint(TransPointRequest source)
        {
            TransPointResponse resultResponse = new();
            var url = Config.GameAPI.META_URL + "Member/TransPoint";
            try
            {
                METABaseStatusRespones responseData = await ApiHandle<TransPointRequest>(url, source);
                if (responseData.status && responseData.code == (int)ErrorCodeEnum.Success)
                {
                    resultResponse = ResultHandler<TransPointResponse>(responseData);
                    resultResponse.DecryptStatus = true;
                }
                else
                {
                    resultResponse.code = responseData.code;
                    resultResponse.errMsg = responseData.errMsg;
                }
            }
            catch (TaskCanceledException ex)
            {
                throw new TaskCanceledException(ex.ToString());
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
            return resultResponse;
        }

        public async Task<GameLoginResponse> GameLogin(GameLoginRequest source)
        {
            GameLoginResponse resultResponse = new();
            var url = Config.GameAPI.META_URL + "Game/Login";
            METABaseStatusRespones responseData = await ApiHandle<GameLoginRequest>(url, source);

            if (responseData.status && responseData.code == (int)ErrorCodeEnum.Success)
            {
                resultResponse = ResultHandler<GameLoginResponse>(responseData);
                resultResponse.DecryptStatus = true;
            }
            else
            {
                resultResponse.code = responseData.code;
                resultResponse.errMsg = responseData.errMsg;
            }
            return resultResponse;
        }

        public async Task<TransactionLogResponse> TransactionLog(TransactionLogRequest source)
        {
            TransactionLogResponse resultResponse = new();
            var url = Config.GameAPI.META_URL + "Member/TransactionLog";
            METABaseStatusRespones responseData = await ApiHandle<TransactionLogRequest>(url, source);
            if (responseData.status && responseData.code == (int)ErrorCodeEnum.Success)
            {
                resultResponse = ResultHandler<TransactionLogResponse>(responseData);
                resultResponse.DecryptStatus = true;
            }
            else
            {
                resultResponse.code = responseData.code;
                resultResponse.errMsg = responseData.errMsg;
            }
            return resultResponse;
        }

        public async Task<BetOrderRecordResponse> BetOrderRecord(BetOrderRecordRequest source)
        {
            //水果盤只拉取已對獎
            //是否已對獎 N 填寫1時只取得已對獎的單
            source.Collect = 1;

            BetOrderRecordResponse resultResponse = new();
            var url = Config.GameAPI.META_URL + "Bets/BetOrder";
            METABaseStatusRespones responseData = await ApiHandle<BetOrderRecordRequest>(url, source);
            if (responseData.status && responseData.code == (int)ErrorCodeEnum.Success)
            {
                resultResponse = ResultHandler<BetOrderRecordResponse>(responseData);
                resultResponse.DecryptStatus = true;
            }
            else
            {
                resultResponse.code = responseData.code;
                resultResponse.errMsg = responseData.errMsg;
            }
            return resultResponse;
        }
    }
}
