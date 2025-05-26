using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;

namespace ThirdPartyWallet.Common;

public class LogHelper<T>
{
    private readonly ILogger<T> _logger;
    private readonly IHostEnvironment _environment;

    public LogHelper(ILogger<T> logger, IHostEnvironment environment)
    {
        this._logger = logger;
        _environment = environment;
    }

    public ILogger<T> GetLogger => _logger;

    /// <summary>
    /// 用於 Middleware，紀錄 API Request/Response，
    /// </summary>
    /// <param name="context"></param>
    /// <param name="type"></param>
    /// <param name="requestContent"></param>
    /// <param name="elapsedMilliseconds"></param>
    public void HttpLog(HttpContext context, string type, string requestContent, long? elapsedMilliseconds)
    {
        var projectName = Assembly.GetCallingAssembly().GetName().Name ?? "";
        _logger.LogInformation("{system} - {source}, {type}, {httpMethod}, {apiPath}, {query}, {body}, {environment}, {executionTime}",
            projectName,
            context.Connection.RemoteIpAddress?.ToString(),
            type,
            context.Request.Method,
            context.Request.Path,
            context.Request.QueryString,
            requestContent,
            _environment.EnvironmentName,
            elapsedMilliseconds);
    }

    /// <summary>
    /// 用於 Middleware，紀錄 API 例外錯誤
    /// </summary>
    /// <param name="context"></param>
    /// <param name="requestContent"></param>
    /// <param name="responseContent"></param>
    /// <param name="ex"></param>
    /// <param name="elapsedMilliseconds"></param>
    public void HttpErrorLog(HttpContext context, string requestContent, string responseContent, Exception ex, long elapsedMilliseconds)
    {
        var projectName = Assembly.GetCallingAssembly().GetName().Name ?? "";
        _logger.LogError(ex, "{system} - {source}, {type}, {httpMethod}, {apiPath}, {query}, {body}, {environment}, {response}, {executionTime}",
            projectName,
            context.Connection.RemoteIpAddress?.ToString(),
            "Exception",
            context.Request.Method,
            context.Request.Path,
            context.Request.QueryString,
            requestContent,
            _environment.EnvironmentName,
            responseContent,
            elapsedMilliseconds);
    }

    /// <summary>
    /// 用於資料層，紀錄 DB 查詢與回傳
    /// </summary>
    /// <param name="dbName"></param>
    /// <param name="commandType"></param>
    /// <param name="sqlOrSPName"></param>
    /// <param name="parameters"></param>
    /// <param name="response"></param>
    /// <param name="elapsedMilliseconds"></param>
    public void DBLog(string dbName, string commandType, string sqlOrSPName, string? parameters, string response, long elapsedMilliseconds)
    {
        var projectName = Assembly.GetCallingAssembly().GetName().Name ?? "";
        _logger.LogInformation("{system} - {type}, {dbName}, {commandType}, {sqlOrSPName}, {parameters}, {response}, {executionTime}, {environment}",
            projectName,
            "DB",
            dbName,
            commandType,
            sqlOrSPName,
            parameters,
            response,
            elapsedMilliseconds,
            _environment.EnvironmentName);
    }

    /// <summary>
    /// 用於資料層，紀錄呼叫外部 API Request/Response
    /// </summary>
    /// <param name="platform"></param>
    /// <param name="url"></param>
    /// <param name="method"></param>
    /// <param name="request"></param>
    /// <param name="response"></param>
    /// <param name="httpStatusCode"></param>
    /// <param name="elapsedMilliseconds"></param>
    public void APILog(string platform, string url, string method, string? request, string response, int httpStatusCode, long elapsedMilliseconds)
    {
        var projectName = Assembly.GetCallingAssembly().GetName().Name ?? "";

        _logger.LogInformation("{system} - {type}, {platform}, {url}, {method}, {request}, {response}, {httpStatusCode}, {executionTime}, {environment}",
            projectName,
            "API",
            platform,
            url,
            method,
            request,
            response,
            httpStatusCode,
            elapsedMilliseconds,
            _environment.EnvironmentName);
    }

    /// <summary>
    /// 用於資料層，紀錄呼叫外部 API Request/Response 例外錯誤
    /// </summary>
    /// <param name="ex"></param>
    /// <param name="platform"></param>
    /// <param name="url"></param>
    /// <param name="method"></param>
    /// <param name="request"></param>
    /// <param name="response"></param>
    /// <param name="httpStatusCode"></param>
    /// <param name="elapsedMilliseconds"></param>
    public void APIErrorLog(Exception ex, string platform, string url, string method, string? request, string response, int httpStatusCode, long elapsedMilliseconds)
    {
        var projectName = Assembly.GetCallingAssembly().GetName().Name ?? "";
        _logger.LogError(ex, "{system} - {type}, {platform}, {url}, {method}, {request}, {response}, {httpStatusCode}, {executionTime}, {environment}",
            projectName,
            "API",
            platform,
            url,
            method,
            request,
            response,
            httpStatusCode,
            elapsedMilliseconds,
            _environment.EnvironmentName);
    }

    /// <summary>
    /// 用於排程，紀錄排程啟動與結束
    /// </summary>
    /// <param name="scheduleName"></param>
    /// <param name="data"></param>
    /// <param name="message"></param>
    /// <param name="elapsedMilliseconds"></param>
    public void ScheduleLog(string scheduleName, string? data, string message, long? elapsedMilliseconds = null)
    {
        var projectName = Assembly.GetCallingAssembly().GetName().Name ?? "";
        _logger.LogInformation("{system} - {type}, {scheduleName}, {data}, {message}, {environment}, {executionTime}",
            projectName,
            "Schedule",
            scheduleName,
            data,
            message,
            _environment.EnvironmentName,
            elapsedMilliseconds);
    }

    /// <summary>
    /// 用於排程，記錄排程例外錯誤
    /// </summary>
    /// <param name="ex"></param>
    /// <param name="scheduleName"></param>
    /// <param name="data"></param>
    /// <param name="message"></param>
    /// <param name="elapsedMilliseconds"></param>
    public void ScheduleErrorLog(Exception ex, string scheduleName, string data, string message, long? elapsedMilliseconds = null)
    {
        var projectName = Assembly.GetCallingAssembly().GetName().Name ?? "";
        _logger.LogError(ex, "{system} - {type}, {scheduleName}, {data}, {message}, {environment}, {executionTime}",
            projectName,
            "ScheduleException",
            scheduleName,
            data,
            message,
            _environment.EnvironmentName,
            elapsedMilliseconds);
    }
}

public enum CorrelationId
{
    X_ClubId = 1,
    X_Platform = 2,
}

/// <summary>
/// req參數中要串聯的屬性名稱
/// </summary>
public enum ReqCorrelationId
{
    club_id = 1,
    clubId = 2,
    Platform = 3

}

public class Correlation
{
    public string Club_Id { get; set; }

    //[JsonExtensionData]
    //private Dictionary<string, JsonElement> AdditionalData { get; set; }
}