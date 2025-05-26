using H1_ThirdPartyWalletAPI.Middleware;
using H1_ThirdPartyWalletAPI.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace H1_ThirdPartyWalletAPI.Service.Common
{
    public class RequestMonitorMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestMonitorMiddleware> _logger;
        private readonly RequestProcessingFlag _processingFlag;

        public RequestMonitorMiddleware(RequestDelegate next, ILogger<RequestMonitorMiddleware> logger, RequestProcessingFlag processingFlag)
        {
            _next = next;
            _logger = logger;
            _processingFlag = processingFlag;
        }

        public async Task Invoke(HttpContext context)
        {
            _processingFlag.Increment();
            try
            {
                await _next(context);
            }
            catch(Exception)
            {
                throw;
            }
            finally
            {
                _processingFlag.Decrement();
            }
        }
    }
}
