using H1_ThirdPartyWalletAPI.Code;
using H1_ThirdPartyWalletAPI.Extensions;
using H1_ThirdPartyWalletAPI.Service.Common;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace H1_ThirdPartyWalletAPI.Middleware
{
    public class ApiHealthHandler : DelegatingHandler
    {
        private readonly IApiHealthCheckService _apiHealthCheckService;
        public ApiHealthHandler(IApiHealthCheckService apiHealthCheckService)
        {
            _apiHealthCheckService = apiHealthCheckService;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var url = request.RequestUri?.ToString();
            var platform = request.TryGetPlatform().ToString();
            var sw = new Stopwatch();
            var timeout = false;
            try
            {
                sw.Start();

                return await base.SendAsync(request, cancellationToken);
            }
            catch (TaskCanceledException)
            {
                timeout = true;
                throw;
            }
            finally
            {
                sw.Stop();

                var platformEnum = (Platform)Enum.Parse(typeof(Platform), platform);

                var time = sw.ElapsedMilliseconds;
                if (timeout)
                    time = 99999;

                _apiHealthCheckService.SetResponseData(platformEnum, new ApiResponseData()
                {
                    ElapsedMilliseconds = time
                });
            }
        }

    }
}
