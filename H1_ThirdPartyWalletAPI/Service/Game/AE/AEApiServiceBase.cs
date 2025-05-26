using H1_ThirdPartyWalletAPI.Code;
using H1_ThirdPartyWalletAPI.Extensions;
using H1_ThirdPartyWalletAPI.Model.Config;
using H1_ThirdPartyWalletAPI.Model.Game.AE.Request;
using H1_ThirdPartyWalletAPI.Service.Common;
using H1_ThirdPartyWalletAPI.Service.Game.AE.JsonConverter;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace H1_ThirdPartyWalletAPI.Service.Game.AE
{
    public class AEApiServiceBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<AEApiServiceBase> _logger;

        protected JsonSerializerOptions DeserializejsonSerializerOptions { get; set; }

        protected JsonSerializerOptions SerializejsonSerializerOptions { get; set; }

        protected int Site_id { get; set; }

        protected string private_secret_key { get; set; }

        protected string ApiUrl { get; set; }

        private string ApiAction { get; set; }

        protected void SetApiTarget(string value)
        {
            ApiAction = value;
        }

        public AEApiServiceBase(IHttpClientFactory httpClientFactory, ILogger<AEApiServiceBase> logger)
        {
            this._httpClientFactory = httpClientFactory;
            _logger = logger;
            DeserializejsonSerializerOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                Converters = {
                    new DecimalDeserializeConverter()
                }
            };
            SerializejsonSerializerOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                Converters = { }
            };
            ApiUrl = Config.GameAPI.AE_URL;
            private_secret_key = Config.CompanyToken.AE_key;
            Site_id = Config.CompanyToken.AE_SiteId;
        }
        public async Task<TResponse> PostAsync<TRequest, TResponse>(TRequest request) where TRequest : AERequestBase
        {
            var apiResInfo = new ApiResponseData();
            try
            {
                request.site_id = Site_id;
                var client = _httpClientFactory.CreateClient("log");
                client.Timeout = TimeSpan.FromSeconds(14);
                client.DefaultRequestHeaders.Add("Authorization", String.Format("Bearer {0}", GetJWTTokenFromRequest(request)));
                var content = new StringContent("");
                var sw = System.Diagnostics.Stopwatch.StartNew();
                var response = await client.PostAsync(Platform.AE, string.Format("{0}/{1}", ApiUrl.TrimEnd('/'), ApiAction), content);
                sw.Stop();
                apiResInfo.ElapsedMilliseconds = sw.ElapsedMilliseconds;
                if (!response.IsSuccessStatusCode)
                    throw new Exception(string.Format("Call AeApi Failed:{0}", ApiAction));
                else
                {
                    var body = await response.Content.ReadAsStringAsync();

                    var dics = new Dictionary<string, object>
                {
                    { "request", JsonSerializer.Serialize(request)},
                    { "response", body }
                };
                    using (var scope = _logger.BeginScope(dics))
                    {
                        _logger.LogInformation("Get RequestPath: {RequestPath} | ResponseHttpStatus:{Status} | exeTime:{exeTime} ms", string.Format("{0}/{1}", ApiUrl.TrimEnd('/'), ApiAction), response.StatusCode, sw.Elapsed.TotalMilliseconds);
                    }


                    var result = JsonSerializer.Deserialize<TResponse>(body, DeserializejsonSerializerOptions);
                    return result;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }

        }
        public async Task<TResponse> PostAsyncWithGzip<TRequest, TResponse>(TRequest request) where TRequest : AERequestBase
        {
            request.site_id = Site_id;
            var client = _httpClientFactory.CreateClient("log");
            client.DefaultRequestHeaders.Add("Authorization", String.Format("Bearer {0}", GetJWTTokenFromRequest(request)));
            client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip");
            var content = new StringContent("");
            var sw = System.Diagnostics.Stopwatch.StartNew();
            var response = await client.PostAsync(Platform.AE, string.Format("{0}/{1}", ApiUrl.TrimEnd('/'), ApiAction), content);
            sw.Stop();
            if (!response.IsSuccessStatusCode)
            {

                throw new Exception(string.Format("Call MgApi Failed:{0}", ApiAction));
            }
            else
            {
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
                    var reponse = "";
                    //拉注單log過長截斷
                    if (body.Length > 10000)
                    {
                        reponse = body.Substring(0, 9999);
                    }
                    else
                    {
                        reponse = body;
                    }
                    var dics = new Dictionary<string, object>
                    {
                        { "request", JsonSerializer.Serialize(request)},
                        { "response", reponse }
                    };
                    using (var scope = _logger.BeginScope(dics))
                    {
                        _logger.LogInformation("Get RequestPath: {RequestPath} | ResponseHttpStatus:{Status} | exeTime:{exeTime} ms", string.Format("{0}/{1}", ApiUrl.TrimEnd('/'), ApiAction), response.StatusCode, sw.Elapsed.TotalMilliseconds);
                    }
                }
                catch (Exception ex)
                {
                    var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                    var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                    _logger.LogError("log exception EX : {ex}  MSG : {Message} Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
                }

                var result = JsonSerializer.Deserialize<TResponse>(body, DeserializejsonSerializerOptions);
                return result;
            }
        }
        public string GetJWTTokenFromRequest<TRequest>(TRequest request)
        {
            var expiredDateTime = DateTime.UtcNow.AddMinutes(5);
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(private_secret_key));
            var signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Expires = expiredDateTime,
                SigningCredentials = signingCredentials,
                Claims = typeof(TRequest).GetProperties().ToDictionary(x => JsonNamingPolicy.CamelCase.ConvertName(x.Name), x => (x.PropertyType == typeof(DateTime) ? ((DateTime)x.GetValue(request)).ToString("yyyy-MM-ddTHH:mm:sszzz") : x.GetValue(request)))
            };
            var tokenHandler = new JwtSecurityTokenHandler();
            var securityToken = tokenHandler.CreateToken(tokenDescriptor);
            var serializeToken = tokenHandler.WriteToken(securityToken);
            return serializeToken;
        }

    }
}
