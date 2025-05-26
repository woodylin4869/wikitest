using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Primitives;
using System.Net;
using H1_ThirdPartyWalletAPI.Model.OneWalletGame;
using H1_ThirdPartyWalletAPI.Service.Common;

namespace H1_ThirdPartyWalletAPI.Controllers.Game.RCG
{
    [Route("RCG/api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService authService;

        public AuthController(IAuthService authService)
        {
            this.authService = authService;
        }
        [AllowAnonymous]
        [HttpPost]
        [Route("CheckUser")]
        public async Task<ResponseBaseMessage<CheckUserResponse>> CheckUser(CheckUserRequest request)
        {
            try
            {
                var result = await authService.CheckUser(request);
                return new ResponseBaseMessage<CheckUserResponse>(result);
            }
            catch (Exception)
            {
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                var result = new CheckUserResponse();
                return new ResponseBaseMessage<CheckUserResponse>(result);
            }
        }

        [HttpPost]
        [Route("RequestExtendToken")]
        [Authorize]
        public async Task<ResponseBaseMessage<RequestExtendTokenResponse>> RequestExtendToken(RequestExtendTokenRequest request)
        {
            try
            {
                var result = await authService.RequestExtendToken(HttpContext, request);
                return new ResponseBaseMessage<RequestExtendTokenResponse>(result);
            }
            catch (Exception)
            {
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                var result = new RequestExtendTokenResponse();
                return new ResponseBaseMessage<RequestExtendTokenResponse>(result);
            }
        }


    }
}
