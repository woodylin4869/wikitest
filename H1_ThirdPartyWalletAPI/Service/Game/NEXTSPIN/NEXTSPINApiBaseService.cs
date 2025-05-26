using H1_ThirdPartyWalletAPI.Code;
using H1_ThirdPartyWalletAPI.Extensions;
using H1_ThirdPartyWalletAPI.Model;
using H1_ThirdPartyWalletAPI.Model.Config;
using H1_ThirdPartyWalletAPI.Service.Common;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using ThirdPartyWallet.Common;

namespace H1_ThirdPartyWalletAPI.Service.Game.NEXTSPIN
{
    public partial class NEXTSPINApiService
    {
        private readonly LogHelper<NEXTSPINApiService> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string MerchantCode;

        public NEXTSPINApiService(LogHelper<NEXTSPINApiService> logger, IHttpClientFactory httpClientFactory)
        {
            this._logger = logger;
            this._httpClientFactory = httpClientFactory;

            this.MerchantCode = Config.CompanyToken.NEXTSPIN_MerchantCode;
        }

        private async Task<TResponse> PostAsync<TRequest, TResponse>(string apiName, TRequest request, Func<string, string> responseLogFormat = null)
            where TRequest : Model.Game.NEXTSPIN.Request.BaseRequest
            where TResponse : Model.Game.NEXTSPIN.Response.BaseResponse
        {
            var apiResInfo = new ApiResponseData();
            try
            {
                var client = _httpClientFactory.CreateClient();
                client.Timeout = TimeSpan.FromSeconds(14);
                client.DefaultRequestHeaders.TryAddWithoutValidation("API", apiName);
                client.DefaultRequestHeaders.TryAddWithoutValidation("DataType", "JSON");

                request.merchantCode = MerchantCode;
                var jsonRequest = JsonConvert.SerializeObject(request);
                _logger.APILog(Platform.NEXTSPIN.ToString()
                    , Config.GameAPI.NEXTSPIN_API_URL + $" API:{apiName}"
                    , "POST"
                    , jsonRequest
                    , string.Empty
                    , 200
                    , 0L);

                var sw = Stopwatch.StartNew();
                var response = await client.PostAsync(Platform.NEXTSPIN, Config.GameAPI.NEXTSPIN_API_URL, new StringContent(jsonRequest));
                sw.Stop();
                apiResInfo.ElapsedMilliseconds = sw.ElapsedMilliseconds;

                response.EnsureSuccessStatusCode();
                //521是遊戲商伺服器直接報錯誤 
                if (response.StatusCode.ToString() == "521")
                    throw new ExceptionMessage((int)response.StatusCode, Enum.GetName(typeof(Model.Game.NEXTSPIN.NEXTSPIN.ErrorCode), (int)response.StatusCode));
                var body = await response.Content.ReadAsStringAsync();
                _logger.APILog(Platform.NEXTSPIN.ToString()
                    , Config.GameAPI.NEXTSPIN_API_URL + $" API:{apiName}"
                    , "POST"
                    , jsonRequest
                    , responseLogFormat is not null ? responseLogFormat(body) : body
                    , (int)response.StatusCode
                    , (long)sw.Elapsed.TotalMilliseconds);

                var result = JsonConvert.DeserializeObject<TResponse>(body);

                if (!result.IsSuccess)
                    throw new ExceptionMessage(result.code, Enum.GetName(typeof(Model.Game.NEXTSPIN.NEXTSPIN.ErrorCode), result.code));

                return result;

            }
            catch (Exception ex)
            {

                var httpStatusCode = 500;
                if (ex is HttpRequestException requestException)
                {
                    httpStatusCode = (int)requestException.StatusCode;
                }

                _logger.APIErrorLog(ex
                    , Platform.NEXTSPIN.ToString()
                    , Config.GameAPI.NEXTSPIN_API_URL + $" API:{apiName}"
                    , "POST"
                    , JsonConvert.SerializeObject(request)
                    , ex.ToString()
                    , httpStatusCode
                    , 0L);

                throw;
            }
        }
        /// <summary>
        /// 521伺服器爛線專用
        /// </summary>
        /// <typeparam name="TRequest"></typeparam>
        /// <typeparam name="TResponse"></typeparam>
        /// <param name="apiName"></param>
        /// <param name="request"></param>
        /// <param name="responseLogFormat"></param>
        /// <returns></returns>
        private async Task<TResponse> PostAsync1<TRequest, TResponse>(string apiName, TRequest request, Func<string, string> responseLogFormat = null)
          where TRequest : Model.Game.NEXTSPIN.Request.BaseRequest
          where TResponse : Model.Game.NEXTSPIN.Response.BaseResponse
        {
            var apiResInfo = new ApiResponseData();
            try
            {
                var client = _httpClientFactory.CreateClient();
                client.Timeout = TimeSpan.FromSeconds(14);
                client.DefaultRequestHeaders.TryAddWithoutValidation("API", apiName);
                client.DefaultRequestHeaders.TryAddWithoutValidation("DataType", "JSON");

                request.merchantCode = MerchantCode;
                var jsonRequest = JsonConvert.SerializeObject(request);
                _logger.APILog(Platform.NEXTSPIN.ToString()
                    , Config.GameAPI.NEXTSPIN_API_URL + $" API:{apiName}"
                    , "POST"
                    , jsonRequest
                    , string.Empty
                    , 200
                    , 0L);

                var sw = Stopwatch.StartNew();
                var response = await client.PostAsync(Platform.NEXTSPIN, Config.GameAPI.NEXTSPIN_API_URL, new StringContent(jsonRequest));
                sw.Stop();
                apiResInfo.ElapsedMilliseconds = sw.ElapsedMilliseconds;

                response.EnsureSuccessStatusCode();
                //521是遊戲商伺服器直接報錯誤 
                if (response.StatusCode.ToString() == "521")
                    throw new TaskCanceledException("HTTP Status Code 521 - Server is down");
                var body = await response.Content.ReadAsStringAsync();
                _logger.APILog(Platform.NEXTSPIN.ToString()
                    , Config.GameAPI.NEXTSPIN_API_URL + $" API:{apiName}"
                    , "POST"
                    , jsonRequest
                    , responseLogFormat is not null ? responseLogFormat(body) : body
                    , (int)response.StatusCode
                    , (long)sw.Elapsed.TotalMilliseconds);

                var result = JsonConvert.DeserializeObject<TResponse>(body);

                if (!result.IsSuccess)
                    throw new ExceptionMessage(result.code, Enum.GetName(typeof(Model.Game.NEXTSPIN.NEXTSPIN.ErrorCode), result.code));

                return result;

            }
            catch (Exception ex)
            {

                var httpStatusCode = 500;
                if (ex is HttpRequestException requestException)
                {
                    httpStatusCode = (int)requestException.StatusCode;
                }

                _logger.APIErrorLog(ex
                    , Platform.NEXTSPIN.ToString()
                    , Config.GameAPI.NEXTSPIN_API_URL + $" API:{apiName}"
                    , "POST"
                    , JsonConvert.SerializeObject(request)
                    , ex.ToString()
                    , httpStatusCode
                    , 0L);

                throw;
            }
        }
    }
}
