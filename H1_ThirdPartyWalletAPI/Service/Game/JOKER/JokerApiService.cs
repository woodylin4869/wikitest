using H1_ThirdPartyWalletAPI.Code;
using H1_ThirdPartyWalletAPI.Extensions;
using H1_ThirdPartyWalletAPI.Middleware;
using H1_ThirdPartyWalletAPI.Model.Config;
using H1_ThirdPartyWalletAPI.Model.Game.JOKER.Request;
using H1_ThirdPartyWalletAPI.Model.Game.JOKER.Response;
using H1_ThirdPartyWalletAPI.Service.Common;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace H1_ThirdPartyWalletAPI.Service.Game.JOKER;

public class JokerApiService : IJokerApiService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<JokerApiService> _logger;
    private static readonly SemaphoreSlim recordLock = new(1);

    public JokerApiService(IHttpClientFactory httpClientFactory, ILogger<JokerApiService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    /// <summary>
    /// 获取游戏列表  健康度
    /// </summary>
    /// <returns></returns>
    public async Task<GetGameListResponse> GetGameListAsync()
    {
        var response = await ApiHandler1(new GetGameListRequest());
        return JsonConvert.DeserializeObject<GetGameListResponse>(response);
    }

    /// <summary>
    /// 取得遊戲 Token
    /// </summary>
    public async Task<GetGameTokenResponse> GetGameTokenAsync(GetGameTokenRequest source)
    {
        var response = await ApiHandler(source);
        return JsonConvert.DeserializeObject<GetGameTokenResponse>(response);
    }

    /// <summary>
    /// 取得遊戲 Url
    /// </summary>
    public string GetGameUrl(GetGameUrlRequest source)
    {
        var parameters = new Dictionary<string, object>
        {
            {"token", source.Token},
            {"game", source.GameCode},
            {"redirectUrl", source.RedirectUrl},
            {"mobile", source.Mobile},
            {"lang", source.Lang},
        };

        var keyValueList = parameters
            .Where(x => !string.IsNullOrEmpty(x.Value.ToString()))
            .Select(x => x.Key + "=" + x.Value).ToList();

        return Config.GameAPI.JOKER_FORWARDGAME_URL + "?" + string.Join("&", keyValueList);
    }

    /// <summary>
    /// 获取信用
    /// </summary>
    public async Task<GetCreditResponse> GetCreditAsync(GetCreditRequest source)
    {
        var response = await ApiHandler(source);
        return JsonConvert.DeserializeObject<GetCreditResponse>(response);
    }

    /// <summary>
    /// 转移信用
    /// </summary>
    public async Task<TransferCreditResponse> TransferCreditAsync(TransferCreditRequest source)
    {
        var response = await ApiHandler(source);
        return JsonConvert.DeserializeObject<TransferCreditResponse>(response);
    }

    /// <summary>
    /// 验证转移信用
    /// 响应 - 成功：HTTP / 1.1 200 OK
    /// 响应-失败：HTTP/1.1 404 Not Found 表示 requestId 不存在
    /// </summary>
    public async Task<ValidTransferCreditResponse> ValidTransferCreditAsync(ValidTransferCreditRequest source)
    {
        var response = await ApiHandler(source);
        return JsonConvert.DeserializeObject<ValidTransferCreditResponse>(response);
    }

    /// <summary>
    /// 提款所有信用
    /// </summary>
    public async Task<TransferOutAllCreditResponse> TransferOutAllCreditAsync(TransferOutAllCreditRequest source)
    {
        var response = await ApiHandler(source);
        return JsonConvert.DeserializeObject<TransferOutAllCreditResponse>(response);
    }

    /// <summary>
    /// 创建用户
    /// </summary>
    public async Task<CreatePlayerResponse> CreatePlayerAsync(CreatePlayerRequest source)
    {
        var response = await ApiHandler(source);
        return JsonConvert.DeserializeObject<CreatePlayerResponse>(response);
    }

    /// <summary>
    /// 注销用户
    /// </summary>
    public async Task<KickPlayerResponse> KickPlayerAsync(KickPlayerRequest source)
    {
        var response = await ApiHandler(source);
        return JsonConvert.DeserializeObject<KickPlayerResponse>(response);
    }

    /// <summary>
    /// 取得注單明細
    /// </summary>
    public async Task<GetBetDetailResponse> GetBetDetailAsync(GetBetDetailRequest source)
    {
        await recordLock.WaitAsync();
        _ = Task.Run(async () =>
        {
            await Task.Delay(TimeSpan.FromSeconds(3));
            recordLock.Release();
        });

        var response = await ApiHandler(source);
        return JsonConvert.DeserializeObject<GetBetDetailResponse>(response);
    }

    /// <summary>
    /// 取得小時帳
    /// </summary>
    public async Task<GethourBetResponse> GethourBetAsync(GethourBetRequest source)
    {
        await recordLock.WaitAsync();
        _ = Task.Run(async () =>
        {
            await Task.Delay(TimeSpan.FromSeconds(3));
            recordLock.Release();
        });

        var response = await ApiHandler(source);
        return JsonConvert.DeserializeObject<GethourBetResponse>(response);
    }

    /// <summary>
    /// 检索历史 URL
    /// </summary>
    public async Task<GetGameHistoryUrlResponse> GetGameHistoryUrlAsync(GetGameHistoryUrlRequest source)
    {
        var response = await ApiHandler(source);
        return JsonConvert.DeserializeObject<GetGameHistoryUrlResponse>(response);
    }

    /// <summary>
    /// 检索输赢
    /// </summary>
    public async Task<GetWinLoseSummaryResponse> GetWinLoseSummaryAsync(GetWinLoseSummaryRequest source)
    {
        var response = await ApiHandler(source);
        return JsonConvert.DeserializeObject<GetWinLoseSummaryResponse>(response);
    }

    /// <summary>
    /// API 請求
    /// </summary>
    private async Task<string> ApiHandler(object source)
    {
        var sw = new Stopwatch();
        sw.Start();

        var apiResInfo = new ApiResponseData();

        try
        {
            var rowData = Helper.ConvertToKeyValue(source);
            var signature = Helper.GetHMACSHA1Signature(rowData, Config.CompanyToken.JOKER_Secret);

            var apiUrl = Config.GameAPI.JOKER_API_URL + $"?appid={Config.CompanyToken.JOKER_AppId}&signature={Uri.EscapeDataString(signature)}";

            var response = await Post(apiUrl, source);
            apiResInfo.ElapsedMilliseconds = sw.ElapsedMilliseconds;
            sw.Stop();

            var dics = new Dictionary<string, object>
            {
                { "request", JsonConvert.SerializeObject(source) },
                { "response", response.body.Length > 10000 ? response.body.Substring(0, 9999):response }
            };
            using (var scope = _logger.BeginScope(dics))
            {
                _logger.LogInformation("Get RequestPath: {RequestPath} | ResponseHttpStatus:{Status} | exeTime:{exeTime} ms", apiUrl, response.statusCode, sw.Elapsed.TotalMilliseconds);
            }

            return response.body;
        }
        catch (HttpRequestException ex)
        {
            // 响应-失败：HTTP/1.1 404 Not Found 表示 requestId 不存在
            if (ex.StatusCode == HttpStatusCode.NotFound)
            {
                throw new Exception(HttpStatusCode.NotFound.ToString());
            }

            throw new Exception($"Call JokerApi Failed:{ex}");
        }
    }

    private async Task<string> ApiHandler1(object source)
    {
        var sw = new Stopwatch();
        sw.Start();

        var apiResInfo = new ApiResponseData();

        try
        {
            var rowData = Helper.ConvertToKeyValue(source);
            var signature = Helper.GetHMACSHA1Signature(rowData, Config.CompanyToken.JOKER_Secret);

            var apiUrl = Config.GameAPI.JOKER_API_URL + $"?appid={Config.CompanyToken.JOKER_AppId}&signature={Uri.EscapeDataString(signature)}";

            var response = await Post(apiUrl, source);
            apiResInfo.ElapsedMilliseconds = sw.ElapsedMilliseconds;
            sw.Stop();



            //521是遊戲商伺服器直接報錯誤 
            if (response.statusCode.ToString() == "521")
                throw new TaskCanceledException("HTTP Status Code 521 - Server is down");

            var dics = new Dictionary<string, object>
            {
                { "request", JsonConvert.SerializeObject(source) },
                { "response", response.body.Length > 10000 ? response.body.Substring(0, 9999):response }
            };
            using (var scope = _logger.BeginScope(dics))
            {
                _logger.LogInformation("Get RequestPath: {RequestPath} | ResponseHttpStatus:{Status} | exeTime:{exeTime} ms", apiUrl, response.statusCode, sw.Elapsed.TotalMilliseconds);
            }

            return response.body;
        }
        catch (HttpRequestException ex)
        {
            // 响应-失败：HTTP/1.1 404 Not Found 表示 requestId 不存在
            if (ex.StatusCode == HttpStatusCode.NotFound)
            {
                throw new Exception(HttpStatusCode.NotFound.ToString());
            }

            throw new Exception($"Call JokerApi Failed:{ex}");
        }
    }

    /// <summary>
    /// Post
    /// </summary>
    /// <param name="url"></param>
    /// <param name="postData"></param>
    /// <param name="headers"></param>
    /// <returns></returns>
    private async Task<(string body, HttpStatusCode statusCode)> Post(string url, object? postData, Dictionary<string, string> headers = null)
    {
        using (var request = _httpClientFactory.CreateClient("log"))
        {

            if (headers != null)
            {
                foreach (var item in headers)
                {
                    request.DefaultRequestHeaders.Add(item.Key, item.Value);
                }
            }
            request.Timeout = TimeSpan.FromSeconds(14);
            var response = await request.PostAsync(Platform.JOKER, url, new StringContent(JsonConvert.SerializeObject(postData), Encoding.UTF8, "application/json"));
            response.EnsureSuccessStatusCode();
            var body = await response.Content.ReadAsStringAsync();
            return (body, response.StatusCode);
        }
    }
}