using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Net;

namespace ThirdPartyWallet.Common;

public class NamedHttpLogHandler : DelegatingHandler
{
    private readonly LogHelper<NamedHttpLogHandler> _logHelper;
    private readonly string _platform;

    public static NamedHttpLogHandler Build(IServiceProvider sp, string platform)
    {
        return new(sp.GetRequiredService<LogHelper<NamedHttpLogHandler>>(), platform);
    }

    public NamedHttpLogHandler(LogHelper<NamedHttpLogHandler> logHelper, string platform)
    {
        _logHelper = logHelper;
        _platform = platform;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // 蒐集紀錄 Log 需要的資料
        var url = request.RequestUri?.ToString();
        var method = request.Method.ToString();
        var requestData = request.Content != null ? await request.Content.ReadAsStringAsync(cancellationToken) : string.Empty;
        var responseData = string.Empty;
        var httpStatusCode = 0;
        var sw = new Stopwatch();
        HttpResponseMessage response;

        try
        {
            sw.Start();
            response = await base.SendAsync(request, cancellationToken);
            sw.Stop();

            responseData = await response.Content.ReadAsStringAsync(cancellationToken);
            httpStatusCode = (int)response.StatusCode;

            // Check if 404 is acceptable for this request
            var is404Acceptable = request.Headers.Contains("X-Accept-404") && response.StatusCode == HttpStatusCode.NotFound;
            
            if (response.IsSuccessStatusCode || is404Acceptable)
            {
                _logHelper.APILog(_platform,
                    url,
                    method,
                    CheckStringLength(requestData),
                    CheckStringLength(responseData),
                    httpStatusCode,
                    sw.ElapsedMilliseconds);
            }
            else
            {
                _logHelper.APIErrorLog(new Exception(),
                    _platform,
                    url,
                    method,
                    CheckStringLength(requestData),
                    CheckStringLength(responseData),
                    httpStatusCode,
                    sw.ElapsedMilliseconds);
            }
        }
        catch (TaskCanceledException ex)
          when (cancellationToken.IsCancellationRequested)
        {

            _logHelper.APIErrorLog(ex,
                _platform,
                url,
                method,
                CheckStringLength(requestData),
                CheckStringLength(responseData),
                httpStatusCode,
                sw.ElapsedMilliseconds);

            var exceptionData = new
            {
                url = url,
                requestData = requestData
            };
            var exceptionMessage = JsonConvert.SerializeObject(exceptionData);


            throw new TaskCanceledException(exceptionMessage, ex);
        }
        catch (Exception ex)
        {
            _logHelper.APIErrorLog(ex,
                _platform,
                url,
                method,
                CheckStringLength(requestData),
                CheckStringLength(responseData),
                httpStatusCode,
                sw.ElapsedMilliseconds);

            throw;
        }

        return response;
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
}


