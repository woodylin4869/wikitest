using H1_ThirdPartyWalletAPI.Code;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace H1_ThirdPartyWalletAPI.Extensions;


/// <summary>
/// 擴充方法，GET、POST增加傳入參數 Platform
/// </summary>
public static class HttpClientExtensions
{
    private const HttpCompletionOption DefaultCompletionOption = HttpCompletionOption.ResponseContentRead;
    private static Uri? CreateUri(string? uri) =>
        string.IsNullOrEmpty(uri) ? null : new Uri(uri, UriKind.RelativeOrAbsolute);

    #region GET

    public static Task<HttpResponseMessage> GetAsync(this HttpClient httpClient, Platform platform, string? requestUri) =>
       GetAsync(httpClient, platform, CreateUri(requestUri));

    public static Task<HttpResponseMessage> GetAsync(this HttpClient httpClient, Platform platform, Uri? requestUri) =>
        GetAsync(httpClient, platform, requestUri, DefaultCompletionOption);

    public static Task<HttpResponseMessage> GetAsync(this HttpClient httpClient, Platform platform, string? requestUri, HttpCompletionOption completionOption) =>
        GetAsync(httpClient, platform, CreateUri(requestUri), completionOption);

    public static Task<HttpResponseMessage> GetAsync(this HttpClient httpClient, Platform platform, Uri? requestUri, HttpCompletionOption completionOption) =>
        GetAsync(httpClient, platform, requestUri, completionOption, CancellationToken.None);

    public static Task<HttpResponseMessage> GetAsync(this HttpClient httpClient, Platform platform, string? requestUri, CancellationToken cancellationToken) =>
        GetAsync(httpClient, platform, CreateUri(requestUri), cancellationToken);

    public static Task<HttpResponseMessage> GetAsync(this HttpClient httpClient, Platform platform, Uri? requestUri, CancellationToken cancellationToken) =>
        GetAsync(httpClient, platform, requestUri, DefaultCompletionOption, cancellationToken);

    public static Task<HttpResponseMessage> GetAsync(this HttpClient httpClient, Platform platform, string? requestUri, HttpCompletionOption completionOption, CancellationToken cancellationToken) =>
        GetAsync(httpClient, platform, CreateUri(requestUri), completionOption, cancellationToken);

    public static async Task<HttpResponseMessage> GetAsync(this HttpClient httpClient, Platform platform, Uri? requestUri,
        HttpCompletionOption completionOption, CancellationToken cancellationToken)
    {
        {
            HttpRequestMessage request = new(HttpMethod.Get, requestUri);
            request.WithPlatform(platform);
            return await httpClient.SendAsync(request, completionOption,
                cancellationToken);
        }
    }

    #endregion


    #region POST

    public static Task<HttpResponseMessage> PostAsync(this HttpClient httpClient, Platform platform, string? requestUri, HttpContent? content) =>
        PostAsync(httpClient, platform, CreateUri(requestUri), content);

    public static Task<HttpResponseMessage> PostAsync(this HttpClient httpClient, Platform platform, Uri? requestUri, HttpContent? content) =>
        PostAsync(httpClient, platform, requestUri, content, CancellationToken.None);

    public static Task<HttpResponseMessage> PostAsync(this HttpClient httpClient, Platform platform, string? requestUri, HttpContent? content, CancellationToken cancellationToken) =>
        PostAsync(httpClient, platform, CreateUri(requestUri), content, cancellationToken);

    public static async Task<HttpResponseMessage> PostAsync(this HttpClient httpClient, Platform platform, Uri? requestUri, HttpContent? content, CancellationToken cancellationToken)
    {
        HttpRequestMessage request = new(HttpMethod.Post, requestUri);
        request.WithPlatform(platform);
        request.Content = content;
        return await httpClient.SendAsync(request, cancellationToken);
    }



    #endregion



}


/// <summary>
/// 擴充方法，HttpRequestMessage 增加設定Platform方法
/// </summary>
public static class HttpRequestExtensions
{
    internal static readonly string IdKey = "GetPlatformID";
    public static HttpRequestMessage WithPlatform(this HttpRequestMessage request, Platform platform)
    {
        request.Options.Set(new HttpRequestOptionsKey<Platform>(IdKey), platform);
        return request;
    }

    public static Platform? TryGetPlatform(this HttpRequestMessage request)
    {
        if (request.Options.TryGetValue(new HttpRequestOptionsKey<Platform>(IdKey), out Platform value))
        {
            return value;
        }
        throw new Exception($"Platform is Null，請到Middleware/HttpLogHandler.cs查看");
    }
}