using H1_ThirdPartyWalletAPI.Code;
using H1_ThirdPartyWalletAPI.Extensions;
using H1_ThirdPartyWalletAPI.Model.Config;
using H1_ThirdPartyWalletAPI.Model.Game.PME.Request;
using H1_ThirdPartyWalletAPI.Model.Game.PME.Response;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace H1_ThirdPartyWalletAPI.Service.Game.PME
{
    public interface IPMEApiService
    {
        /// <summary>
        /// 玩家余额获取
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellation"></param>
        /// <returns></returns>
        Task<GetBalanceResponse> GetBalanceAsync(GetBalanceRequest request, CancellationToken cancellation = default);

        /// <summary>
        /// 玩家登录
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellation"></param>
        /// <returns></returns>
        Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellation = default);

        /// <summary>
        /// 玩家密码修改
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellation"></param>
        /// <returns></returns>
        Task<ModifyResponse> ModifyAsync(ModifyRequest request, CancellationToken cancellation = default);

        /// <summary>
        /// 玩家注单拉取
        /// 1. 只能查询当前时间30天前至当前时间区间
        /// 2. 查询截至时间为当前时间5分钟前
        /// 3. 每次查询时间区间为30分钟以内
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellation"></param>
        /// <returns></returns>
        Task<QueryScrollResponse> QueryScrollAsync(QueryScrollRequest request, CancellationToken cancellation = default);

        /// <summary>
        /// 玩家注册
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellation"></param>
        /// <returns></returns>
        Task<RegisterResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellation = default);

        /// <summary>
        /// 英雄召唤注单拉取
        /// 1. 只能查询当前时间30天前至当前时间区间
        /// 2. 查询截至时间为当前时间5分钟前
        /// 3. 每次查询时间区间为30分钟以内
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellation"></param>
        /// <returns></returns>
        Task<TicketOrderQueryResponse> TicketOrderQueryAsync(TicketOrderQueryRequest request, CancellationToken cancellation = default);

        /// <summary>
        /// 玩家资金转入/转出
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellation"></param>
        /// <returns></returns>
        Task<TransferResponse> TransferAsync(TransferRequest request, CancellationToken cancellation = default);

        /// <summary>
        /// 玩家转账查询
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellation"></param>
        /// <returns></returns>
        Task<TransferQueryResponse> TransferQueryAsync(TransferQueryRequest request, CancellationToken cancellation = default);

        /// <summary>
        /// 踢線
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellation"></param>
        /// <returns></returns>
        Task<KickResponse> KickAsync(KickRequest request, CancellationToken cancellation = default);
    }

    public class PMEApiService : IPMEApiService
    {
        private readonly ILogger<PMEApiService> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly long Merchant;

        public PMEApiService(ILogger<PMEApiService> logger, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            Merchant = long.Parse(Config.CompanyToken.PME_Merchant);
        }

        /// <summary>
        /// 玩家注册
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellation"></param>
        /// <returns></returns>
        public Task<RegisterResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellation = default)
        {
            var url = Config.GameAPI.PME_URL + "/api/member/register";
            //var url = Config.GameAPI.PME_URL + "/api/version/member/register";
            return GetAsync<RegisterRequest, RegisterResponse>(url, request, cancellation);
        }
        /// <summary>
        /// 踢線
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellation"></param>
        /// <returns></returns>
        public Task<KickResponse> KickAsync(KickRequest request, CancellationToken cancellation = default)
        {
            var url = Config.GameAPI.PME_URL + "/api/member/offline";
            return GetAsync<KickRequest, KickResponse>(url, request, cancellation);
        }

        /// <summary>
        /// 玩家密码修改
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellation"></param>
        /// <returns></returns>
        public Task<ModifyResponse> ModifyAsync(ModifyRequest request, CancellationToken cancellation = default)
        {
            var url = Config.GameAPI.PME_URL + "/api/member/modify";
            //var url = Config.GameAPI.PME_URL + "/api/version/member/modify";
            return GetAsync<ModifyRequest, ModifyResponse>(url, request, cancellation);
        }

        /// <summary>
        /// 玩家登录
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellation"></param>
        /// <returns></returns>
        public Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellation = default)
        {
            var url = Config.GameAPI.PME_URL + "/api/v2/member/login";
            //var url = Config.GameAPI.PME_URL + "/api/version/v2/member/login";
            return GetAsync<LoginRequest, LoginResponse>(url, request, cancellation);
        }

        /// <summary>
        /// 玩家余额获取
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellation"></param>
        /// <returns></returns>
        public Task<GetBalanceResponse> GetBalanceAsync(GetBalanceRequest request, CancellationToken cancellation = default)
        {
            var url = Config.GameAPI.PME_URL + "/api/fund/getBalance";
            //var url = Config.GameAPI.PME_URL + "/api/version/fund/getBalance";
            return GetAsync<GetBalanceRequest, GetBalanceResponse>(url, request, cancellation);
        }

        /// <summary>
        /// 玩家资金转入/转出
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellation"></param>
        /// <returns></returns>
        public Task<TransferResponse> TransferAsync(TransferRequest request, CancellationToken cancellation = default)
        {
            var url = Config.GameAPI.PME_URL + "/api/fund/transfer";
            //var url = Config.GameAPI.PME_URL + "/api/version/fund/transfer";
            return GetAsync<TransferRequest, TransferResponse>(url, request, cancellation);
        }

        /// <summary>
        /// 玩家转账查询
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellation"></param>
        /// <returns></returns>
        public Task<TransferQueryResponse> TransferQueryAsync(TransferQueryRequest request, CancellationToken cancellation = default)
        {
            var url = Config.GameAPI.PME_URL + "/api/fund/transferQuery";
            //var url = Config.GameAPI.PME_URL + "/api/version/fund/transferQuery";
            return GetAsync<TransferQueryRequest, TransferQueryResponse>(url, request, cancellation);
        }

        /// <summary>
        /// 玩家注单拉取
        /// 1. 只能查询当前时间30天前至当前时间区间
        /// 2. 查询截至时间为当前时间5分钟前
        /// 3. 每次查询时间区间为30分钟以内
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellation"></param>
        /// <returns></returns>
        public Task<QueryScrollResponse> QueryScrollAsync(QueryScrollRequest request, CancellationToken cancellation = default)
        {
            var url = Config.GameAPI.PME_PULL_URL + "/v2/pull/order/queryScroll";
            return GetAsync<QueryScrollRequest, QueryScrollResponse>(url, request, cancellation);
        }

        /// <summary>
        /// 英雄召唤注单拉取
        /// 1. 只能查询当前时间30天前至当前时间区间
        /// 2. 查询截至时间为当前时间5分钟前
        /// 3. 每次查询时间区间为30分钟以内
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellation"></param>
        /// <returns></returns>
        public Task<TicketOrderQueryResponse> TicketOrderQueryAsync(TicketOrderQueryRequest request, CancellationToken cancellation = default)
        {
            var url = Config.GameAPI.PME_PULL_URL + "/pull/ticketOrder/query";
            return GetAsync<TicketOrderQueryRequest, TicketOrderQueryResponse>(url, request, cancellation);
        }

        private async Task<TResponse> GetAsync<TRequest, TResponse>(string url, TRequest request, CancellationToken cancellation = default)
            where TRequest : BaseRequest
            where TResponse : BaseResponse
        {
            try
            {
                return await GetCoreAsync<TRequest, TResponse>(url, request, cancellation);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "{Platform} {StatusCode} {Message}", Platform.PME, ex.StatusCode, ex.Message);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{Platform} {Message}", Platform.PME, ex.Message);
                throw;
            }
        }

        private async Task<TResponse> GetCoreAsync<TRequest, TResponse>(string url, TRequest request, CancellationToken cancellation = default)
            where TRequest : BaseRequest
            where TResponse : BaseResponse
        {
            var client = _httpClientFactory.CreateClient("log");
            client.Timeout = TimeSpan.FromSeconds(14);
            if (url.EndsWith("/pull/order/queryScroll") || url.EndsWith("/pull/ticketOrder/query"))
            {
                client.Timeout = TimeSpan.FromSeconds(60);
            }

            if (request.merchant == default) request.merchant = Merchant;
            request.time = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            var queryStr = BuildQueryString(request);
            if (url.EndsWith("/pull/order/queryScroll") || url.EndsWith("/pull/ticketOrder/query"))
                queryStr = BuildPullQueryString(request);

            var response = await client.GetAsync(Platform.PME, $"{url}?{queryStr}", cancellation);
            response.EnsureSuccessStatusCode();
            var body = await response.Content.ReadAsStringAsync(cancellation);
            var result = JsonSerializer.Deserialize<TResponse>(body);
            if (!result.IsSuccess) throw new ExceptionMessage(ResponseCode.Fail, result.data);
            return result;
        }

        private string BuildQueryString<TRequest>(TRequest request) where TRequest : BaseRequest
        {
            var parDic = GetDictionary(request);
            parDic.Add("key", Config.CompanyToken.PME_Key);

            var signStr = ParseDicToQueryString(parDic);
            using var md5 = MD5.Create();
            byte[] plainByteArray = md5.ComputeHash(Encoding.UTF8.GetBytes($"{signStr}"));
            string sign = Convert.ToHexString(plainByteArray).ToLower();

            parDic = GetDictionary(request);
            parDic.Add("sign", sign);
            return ParseDicToQueryString(parDic);
        }

        /// <summary>
        /// 產生查詢注單的查詢字串
        /// </summary>
        /// <typeparam name="TRequest"></typeparam>
        /// <param name="request"></param>
        /// <returns></returns>
        private string BuildPullQueryString<TRequest>(TRequest request) where TRequest : BaseRequest
        {
            var parDic = GetDictionary(request);
            parDic.Add("key", Config.CompanyToken.PME_Key);

            var signStr = ParseDicToQueryString(parDic);
            using var md5 = MD5.Create();
            byte[] plainByteArray = md5.ComputeHash(Encoding.UTF8.GetBytes($"{signStr}"));
            string sign = Convert.ToHexString(plainByteArray).ToLower();

            parDic = GetDictionary(request);
            parDic.Add("sign", $"pm{sign[..9]}pm{sign[9..17]}pm{sign[17..]}pm");
            return ParseDicToQueryString(parDic);
        }

        private string ParseDicToQueryString(Dictionary<string, string> dic)
        {
            return string.Join('&', dic.OrderBy(p => p.Key, StringComparer.Ordinal).Select(p => $"{p.Key}={p.Value ?? string.Empty}"));
        }

        private static Dictionary<string, string> GetDictionary<TRequest>(TRequest request, bool KeepNullField = false)
        {
            var props = typeof(TRequest).GetProperties();
            var param = new Dictionary<string, string>();

            foreach (var prop in props)
            {
                var propName = prop.Name;
                string propValue = prop.GetValue(request)?.ToString();

                if (KeepNullField || propValue is not null)
                    param.Add(propName, propValue);
            }

            return param;
        }
    }
}
