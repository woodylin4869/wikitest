using H1_ThirdPartyWalletAPI.Extensions;
using H1_ThirdPartyWalletAPI.Model;
using H1_ThirdPartyWalletAPI.Service.Common;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using ThirdPartyWallet.Common;

namespace H1_ThirdPartyWalletAPI.Middleware;

public class HttpLogHandler : DelegatingHandler
{
    private readonly LogHelper<HttpLogHandler> _logHelper;
    private readonly ICacheDataService _cacheDataService;
    private int _cacheSeconds = 60 * 12;
    public HttpLogHandler(LogHelper<HttpLogHandler> logHelper, ICacheDataService cacheDataService)
    {
        _logHelper = logHelper;
        _cacheDataService = cacheDataService;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {

        // 蒐集紀錄 Log 需要的資料
        var url = request.RequestUri?.ToString();
        var platform = request.TryGetPlatform().ToString();
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

            if (response.IsSuccessStatusCode)
            {
                _logHelper.APILog(platform,
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
                    platform,
                    url,
                    method,
                    CheckStringLength(requestData),
                    CheckStringLength(responseData),
                    httpStatusCode,
                    sw.ElapsedMilliseconds);
            }
        }
        catch (TaskCanceledException ex)
            when (cancellationToken.IsCancellationRequested )
        {

            _logHelper.APIErrorLog(ex,
                platform,
                url,
                method,
                CheckStringLength(requestData),
                CheckStringLength(responseData),
                httpStatusCode,
                sw.ElapsedMilliseconds);

            var exceptionData= new
            {
                url= url,
                requestData= requestData
            };
            var exceptionMessage = JsonConvert.SerializeObject(exceptionData);


            throw new TaskCanceledException(exceptionMessage,ex);
        }
        catch (Exception ex)
        {
            _logHelper.APIErrorLog(ex,
                platform,
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

