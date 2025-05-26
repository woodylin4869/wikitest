using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.OpenApi.Any;
using System;
using Microsoft.Extensions.Hosting;

namespace H1_ThirdPartyWalletAPI.Attributes
{

    /// <summary>
    /// Env 環境類型
    /// </summary>
    [Flags]
    public enum EnvType
    {
        /// <summary>
        /// Local
        /// </summary>
        Local = 1,

        /// <summary>
        /// Dev
        /// </summary>
        DEV = 2,

        /// <summary>
        /// Stage
        /// </summary>
        UAT = 4,

        /// <summary>
        /// PRD
        /// </summary>
        PRD = 8,
    }

    public class ApiAllowAttribute : Attribute, IResourceFilter
    {
        // private readonly IWebHostEnvironment _Environment;
        /// <summary>
        /// 字串條件
        /// </summary>
        public EnvType EnumCondition { get; }

        public ApiAllowAttribute(EnvType enumCondition)
        {
            EnumCondition = enumCondition;
        }



        /// <summary>
        /// 執行後
        /// </summary>
        /// <param name="context"></param>
        public void OnResourceExecuted(ResourceExecutedContext context)
        {
            
        }

        /// <summary>
        /// 執行前
        /// </summary>
        /// <param name="context"></param>
        /// <exception cref="NotImplementedException"></exception>
        public void OnResourceExecuting(ResourceExecutingContext context)
        {
            var environment = (IWebHostEnvironment)context.HttpContext.RequestServices.GetService(typeof(IWebHostEnvironment));
            if (environment.IsEnvironment("Local") && EnumCondition.HasFlag(EnvType.Local) == false)
            {
                context.Result = new ForbidResult();
            }
            else if (environment.IsEnvironment("DEV") && EnumCondition.HasFlag(EnvType.DEV) == false)
            {
                context.Result = new ForbidResult();
            }
            else if (environment.IsEnvironment("UAT") && EnumCondition.HasFlag(EnvType.UAT) == false)
            {
                context.Result = new ForbidResult();
            }
            else if (environment.IsEnvironment("PRD") && EnumCondition.HasFlag(EnvType.PRD) == false)
            {
                context.Result = new ForbidResult();
            }
        }
    }
}
