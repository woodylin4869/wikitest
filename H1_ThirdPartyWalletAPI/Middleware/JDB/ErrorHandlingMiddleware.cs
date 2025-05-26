using H1_ThirdPartyWalletAPI.Code;
using H1_ThirdPartyWalletAPI.Model.Game.JDB;
using H1_ThirdPartyWalletAPI.Model.Game.JDB.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace H1_ThirdPartyWalletAPI.Middlewares.JDB
{
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate next;

        //--這個等 Nlog 設定完再處理
        private readonly ILogger<ErrorHandlingMiddleware> _logger;

        public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
        {
            this.next = next;
            this._logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await next(context);
            }
            catch (Exception ex)
            {
                if (ex is JDBBadRequestException)           
                {
                    _logger.LogWarning(ex, ex.Message);
                  
                }
                else
                {
                    _logger.LogCritical(ex, ex.Message);
                }

                if (await HandleExceptionAsync(context, ex))
                {
                    return;
                }
                else
                {
                    throw;
                }
                //--不必做任何事，就會進入下一個
            }
        }

        private static async Task<bool> HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var code = HttpStatusCode.InternalServerError; // 500 if unexpected

            //--如果不是指定的 exception ，不放出
            //ErrorResponseViewModel model = new ErrorResponseViewModel();
            //model.ErrorType = "system";
            //model.Message = "系統錯誤!!";
            var responseMessage = new ErrorResponseModel();


            if (exception is JDBBadRequestException)
            {
                code = HttpStatusCode.BadRequest;
                var ex = (JDBBadRequestException)exception;

                responseMessage.Status = ex.status;
                responseMessage.Err_text = ex.Message;

                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int)code;
                await context.Response.WriteAsync(JsonSerializer.Serialize(responseMessage, new JsonSerializerOptions() { PropertyNameCaseInsensitive = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
                return true;

            }
            else
            {
                context.Response.StatusCode = (int)code;
                return false;
                
            }

        }
    }
}