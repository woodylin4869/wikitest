using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using H1_ThirdPartyWalletAPI.Code;
using H1_ThirdPartyWalletAPI.Model;
using H1_ThirdPartyWalletAPI.Model.W1API;
using H1_ThirdPartyWalletAPI.Service.Game;
using H1_ThirdPartyWalletAPI.Service.Common;
using System.Threading.Tasks;
using H1_ThirdPartyWalletAPI.Model.Config;
using H1_ThirdPartyWalletAPI.Service;

namespace H1_ThirdPartyWalletAPI.Controllers.W1API
{
    [ApiController]
    [Route("w1api/[controller]")]
    public class GetUserBalanceController : ControllerBase
    {
        private readonly ILogger<GetUserBalanceController> _logger;
        private readonly ITransferWalletService _transferWalletService;
        private readonly ISingleWalletService _singleWalletService;

        public GetUserBalanceController(ILogger<GetUserBalanceController> logger, 
            ITransferWalletService transferWalletService,
            ISingleWalletService singleWalletService
        )
        {
            _logger = logger;
            _transferWalletService = transferWalletService;
            _singleWalletService = singleWalletService;
        }
        /// <summary>
        /// 取得所有錢包餘額
        /// </summary>
        [HttpGet]
        async public Task<GetMemberBalance> Get([FromQuery] GetMemberBalanceReq request)
        {
            GetMemberBalance res = new GetMemberBalance();
            try
            {
                if (Config.OneWalletAPI.WalletMode == "SingleWallet")
                {
                    res = await _singleWalletService.GetMemberWalletBalance(request.Club_id);
                }
                else
                {
                    res = await _transferWalletService.GetMemberWalletBalanceCache(request.Club_id);
                }                
                return res;
            }
            catch (Exception ex)
            {
                res.code = (int)ResponseCode.GetBalanceFail;
                res.Message = MessageCode.Message[(int)ResponseCode.GetBalanceFail] + " | " + ex.Message.ToString();
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                _logger.LogError("Get Member Balance exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
                return res;
            }
        }
    }
}