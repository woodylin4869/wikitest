using H1_ThirdPartyWalletAPI.Code;
using H1_ThirdPartyWalletAPI.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using ThirdPartyWallet.Common;

namespace H1_ThirdPartyWalletAPI.Middleware
{
    public class LogMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly LogHelper<LogMiddleware> _logger;

        public LogMiddleware(RequestDelegate next, LogHelper<LogMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            if (IsIgnoreLogPath(context.Request.Path.ToString()))
            {
                await _next(context);
                return;
            }

            var sw = new Stopwatch();
            sw.Start();

            var originalBodyStream = context.Response.Body;
            await using var fakeResponseBody = new MemoryStream();

            try
            {
                // 啟用讀取 Request
                context.Request.EnableBuffering();
                // Request Body
                var requestContent = await ReadStreamToString(context.Request.Body);
                // 設定 Stream 存放 ResponseBody
                context.Response.Body = fakeResponseBody;

                #region 在此加入想要CorrelationId 的地方

                HashSet<(string DictKey, object DictValue)> dict = new HashSet<(string, object)>();
                dict.UnionWith(await GetCorrelationId(context, requestContent, ReqCorrelationId.club_id));
                dict.UnionWith(await GetCorrelationId(context, requestContent, ReqCorrelationId.clubId));
                dict.UnionWith(await GetCorrelationId(context, requestContent, ReqCorrelationId.Platform));
                var diction = dict.ToDictionary(x => x.DictKey, y => y.DictValue);
                using var loggerScope = _logger.GetLogger.BeginScope(diction);

                #endregion 在此加入想要CorrelationId 的地方

                _logger.HttpLog(context, "Request", CheckStringLength(requestContent), null);

                // 執行 Middleware
                await _next(context);

                // 讀取 Response
                fakeResponseBody.Seek(0, SeekOrigin.Begin);
                var responseContent = await ReadStreamToString(fakeResponseBody);
                await fakeResponseBody.CopyToAsync(originalBodyStream);

                sw.Stop();
                _logger.HttpLog(context, "Response", CheckStringLength(responseContent), sw.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                var serializerSettings = new JsonSerializerSettings
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                };

                var responseContent = JsonConvert.SerializeObject(new ResCodeBase()
                {
                    code = (int)ResponseCode.Fail,
                    Message = MessageCode.Message[(int)ResponseCode.Fail]
                }, serializerSettings);

                // Request Body
                context.Request.Body.Position = 0;
                var requestContent = await ReadStreamToString(context.Request.Body);

                sw.Stop();
                _logger.HttpLog(context, "Response", CheckStringLength(responseContent), sw.ElapsedMilliseconds);
                _logger.HttpErrorLog(context, requestContent, ex.Message, ex, sw.ElapsedMilliseconds);

                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int)HttpStatusCode.OK;

                await context.Response.WriteAsync(responseContent, Encoding.UTF8);

                context.Response.Body.Seek(0, SeekOrigin.Begin);
                await fakeResponseBody.CopyToAsync(originalBodyStream);
            }
        }

        private async Task<HashSet<(string, object)>> GetCorrelationId(HttpContext context, string requestBody, ReqCorrelationId reqCorrelationId)
        {
            var result = "";
            CorrelationId? correlationId = null;
            correlationId = GetCorrelationId(reqCorrelationId);
            if (correlationId.HasValue == false)
                return new HashSet<(string, object)>();

            if (context.Request.Method.ToLower() == "post")
            {
                try
                {
                    using JsonDocument document = JsonDocument.Parse(requestBody);
                    if (document.RootElement.ValueKind == JsonValueKind.Object)
                    {
                        JsonElement? value = null;
                        foreach (var property in document.RootElement.EnumerateObject())
                        {
                            if (property.Name.Equals(reqCorrelationId.ToString(), StringComparison.OrdinalIgnoreCase))
                            {
                                value = property.Value;
                                result = property.Value.GetString();
                                break; // 找到後退出循環
                            }
                        }

                        #region backup benchmarkDotnet 效能比較差

                        //var options = new JsonSerializerOptions
                        //{
                        //    PropertyNameCaseInsensitive = true,
                        //    IncludeFields = true, // 或者 JsonIncludeAttribute
                        //    NumberHandling = JsonNumberHandling.AllowReadingFromString,
                        //    ReadCommentHandling = System.Text.Json.JsonCommentHandling.Skip,
                        //};

                        //var correlation = System.Text.Json.JsonSerializer.Deserialize<Correlation>(requestBody, options);
                        //if (correlation != null && string.IsNullOrEmpty(correlation.Club_Id) == false)
                        //{
                        //    result = correlation.Club_Id;
                        //}

                        #endregion backup benchmarkDotnet 效能比較差
                    }
                }
                catch (System.Text.Json.JsonException ex)
                {
                    _logger.GetLogger.LogError($"Error during deserialization: {ex.Message}");
                    // Console.WriteLine($"Error during deserialization: {ex.Message}");
                }
            }
            else if (context.Request.Method.ToLower() == "get")
            {
                string keyToCheck = reqCorrelationId.ToString();
                // Check if the key exists in the query parameters (case-insensitive)
                if (context.Request.Query.Keys.Any(key => string.Equals(key, keyToCheck, StringComparison.OrdinalIgnoreCase)))
                {
                    string value = context.Request.Query[keyToCheck];
                    result = value.ToString();
                }
            }

            if (string.IsNullOrEmpty(result) == true)
            {
                return new HashSet<(string, object)>();
            }

            return await Task.FromResult(new HashSet<(string, object)> { (correlationId.ToString(), result) });
        }

        private CorrelationId? GetCorrelationId(ReqCorrelationId reqCorrelationId)
        {
            return reqCorrelationId switch
            {
                ReqCorrelationId.clubId => CorrelationId.X_ClubId,
                ReqCorrelationId.club_id => CorrelationId.X_ClubId,
                ReqCorrelationId.Platform => CorrelationId.X_Platform,
                _ => null
            };
        }

        /// <summary>
        /// 要忽略的 API 路徑
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private static bool IsIgnoreLogPath(string url)
        {
            var ignoreList = new List<string>()
            {
                "/swagger",
                "/health",
                "/hc-ui",
                "/hc",
            };

            return ignoreList.Any(x => url.IndexOf(x, StringComparison.Ordinal) != -1);
        }

        /// <summary>
        /// 限制資料長度，避免 GCP Log 寫不進去
        /// </summary>
        /// <returns></returns>
        private static string CheckStringLength(string data)
        {
            data ??= string.Empty;
            var maxlength = 3000;
            if (data.Length > maxlength)
            {
                return $"Log 過長已截斷，保留最大資料長度為: {maxlength}，{data.Substring(0, maxlength)}";
            }

            return data;
        }

        private static async Task<string> ReadStreamToString(Stream stream)
        {
            var position = stream.Position;
            using var memory = new MemoryStream();
            await stream.CopyToAsync(memory);
            stream.Seek(0, SeekOrigin.Begin);
            memory.Seek(position, SeekOrigin.Begin);
            using var reader = new StreamReader(memory);
            return await reader.ReadToEndAsync();
        }
    }
}