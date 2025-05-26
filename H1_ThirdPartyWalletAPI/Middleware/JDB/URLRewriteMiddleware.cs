using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Threading.Tasks;
using System.Web;

namespace H1_ThirdPartyWalletAPI.Middlewares.JDB
{
    public class URLRewriteMiddleware
    {
        private readonly RequestDelegate _next;



        /// <summary>Log</summary>
        private readonly ILogger<URLRewriteMiddleware> _Logger;

        /// <summary>
        ///  Memory Cache
        /// </summary>
        protected IMemoryCache _memoryCache;

        /// <summary>
        /// Memory Cache Option
        /// </summary>
        protected MemoryCacheEntryOptions _memoryCacheOption;

       
        /// <summary>
        /// Organization Service
        /// </summary>

        

        public URLRewriteMiddleware(RequestDelegate next, ILogger<URLRewriteMiddleware> logger)
        {
            _next = next;
            this._Logger = logger;
        }



        public async Task Invoke(HttpContext context)
        {
            context.Request.EnableBuffering();

     

            using MemoryStream RequestBody = new MemoryStream();

            var querystring = HttpUtility.UrlDecode( context.Request.QueryString.Value);

            var body = querystring.Substring(querystring.IndexOf("=") + 1);

            if (body.Length == 0)
            {
                using var reader = new StreamReader(context.Request.Body);

                body = await reader.ReadToEndAsync();

                context.Request.Body.Seek(0, SeekOrigin.Begin);

                context.Items["Body"] = body;

                await _next(context);
                return;
            }
            
            if (context.Request.Method =="POST")
            {
                //context.Request.Path = ActionFactory(body);
            }
            context.Items["Body"] = body;

            var sw = new StreamWriter(RequestBody);
            await sw.WriteAsync(body);
            await sw.FlushAsync();
            RequestBody.Seek(0, SeekOrigin.Begin);

            context.Request.ContentType = "application/json";
            context.Request.ContentLength = RequestBody.Length;
            context.Request.Body = RequestBody;
            

            //context.Request.Body.Seek(0, SeekOrigin.Begin);

            await _next(context);
            
        }
    }

   
}