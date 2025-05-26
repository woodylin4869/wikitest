using H1_ThirdPartyWalletAPI.Code;
using H1_ThirdPartyWalletAPI.Extensions;
using H1_ThirdPartyWalletAPI.Model.Config;
using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace H1_ThirdPartyWalletAPI.Service.Game.CMD368
{
    public class CMDApiServiceBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private string API_URL { get; init; }
        public CMDApiServiceBase(IHttpClientFactory httpClientFactory)
        {
            this._httpClientFactory = httpClientFactory;
            API_URL = Config.GameAPI.CMD368_PULL_URL;
        }
        public async Task<TResponse> GetAsync<TResponse>(string requestPath, CancellationToken cancellationToken = default) where TResponse : class
        {
            var client = _httpClientFactory.CreateClient("log");
            try
            {
                var response = await client.GetAsync(Platform.CMD368, $"{API_URL}/{requestPath}", cancellationToken);
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                    throw new HttpRequestException($"Failed to call CMDApi at {requestPath}: {response.StatusCode} - {errorContent}");
                }
                var contentType = response.Content.Headers.ContentType?.MediaType;
                if (contentType != null && contentType.Contains("application/json", StringComparison.OrdinalIgnoreCase))
                {
                    var body = await response.Content.ReadAsStringAsync(cancellationToken);
                    return JsonSerializer.Deserialize<TResponse>(body) ?? throw new InvalidOperationException("Deserialized object is null");
                }
                else
                {
                    throw new InvalidOperationException($"Expected application/json response, got {contentType}");
                }
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException($"Error deserializing response to type {typeof(TResponse)}", ex);
            }
        }
    }
}