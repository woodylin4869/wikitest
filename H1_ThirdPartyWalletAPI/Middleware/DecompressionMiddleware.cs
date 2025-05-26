using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using System.Threading;
using System.IO;
using System.IO.Compression;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;

namespace H1_ThirdPartyWalletAPI.Middleware
{
    public class DecompressionMiddleware
    {
        private readonly RequestDelegate _next;
        public DecompressionMiddleware(RequestDelegate next)
        {
            _next = next;
        }
        public async Task Invoke(HttpContext context)
        //protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            context.Request.EnableBuffering();

            var request = context.Request;
            var requestBody = request.Body;

            var requestHeaders = request.Headers;

            var encodingNames = requestHeaders.GetCommaSeparatedValues(HeaderNames.ContentEncoding);

            if (request.Method == "POST")
            {
                bool isGzip = encodingNames.Contains("gzip");
                bool isDeflate = !isGzip && encodingNames.Contains("deflate");
                if (isGzip || isDeflate)
                {
                    Stream decompressedStream = new MemoryStream();
                    if (isGzip)
                    {
                        using (var gzipStream = new GZipStream(requestBody, CompressionMode.Decompress))
                        {   
                            await gzipStream.CopyToAsync(decompressedStream);
                        }
                    }
                    else if (isDeflate)
                    {
                        using (var gzipStream = new DeflateStream(requestBody, CompressionMode.Decompress))
                        {
                            await gzipStream.CopyToAsync(decompressedStream);
                        }
                    }

                    decompressedStream.Seek(0, SeekOrigin.Begin);

                    context.Request.ContentType = "application/json";
                    context.Request.Body = decompressedStream;

                }
            }


            await _next(context);

        }
    }
}
