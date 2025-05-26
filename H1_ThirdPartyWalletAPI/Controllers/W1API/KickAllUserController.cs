using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using H1_ThirdPartyWalletAPI.Code;
using System.Threading.Tasks;
using H1_ThirdPartyWalletAPI.Model.Game;
using H1_ThirdPartyWalletAPI.Service.Game;
using H1_ThirdPartyWalletAPI.Model;
using H1_ThirdPartyWalletAPI.Model.Game.JDB.Request;
using H1_ThirdPartyWalletAPI.Model.Game.JDB.Response;
using H1_ThirdPartyWalletAPI.Model.Game.JDB.Exceptions;
using H1_ThirdPartyWalletAPI.Service.W1API;
using System.Collections.Generic;
using H1_ThirdPartyWalletAPI.Model.Config;

namespace H1_ThirdPartyWalletAPI.Controllers.W1API
{
    [ApiController]
    [Route("w1api/[controller]")]
    public class KickAllUserController : ControllerBase
    {
        private readonly ILogger<KickAllUserController> _logger;
        private IKickAllUserService _kickAllUserService;


        public KickAllUserController(ILogger<KickAllUserController> logger,
            IKickAllUserService kickAllUserService
            )
        {
            _logger = logger;
            _kickAllUserService = kickAllUserService;
        }
        /// <summary>
        /// 踢出所有在遊戲中的玩家
        /// </summary>
        [HttpPost]
        async public Task<ResCodeBase> Post()
        {
            ResCodeBase res = new ResCodeBase();
            try
            {

                List<string> openGame = new List<string>(Config.OneWalletAPI.OpenGame.Split(','));
                var KicktaskList = new List<Task<ResCodeBase>>();
                foreach (string r in openGame)
                {
                    var platform = (Platform)Enum.Parse(typeof(Platform), r.ToUpper());
                    KicktaskList.Add(_kickAllUserService.KickAllUser(platform));
                }
                await Task.WhenAll(KicktaskList);                
                res.code = (int)ResponseCode.Success;
                res.Message = MessageCode.Message[(int)ResponseCode.Success];
                return res;
            }
            catch (Exception ex)
            {
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                res.code = (int)ResponseCode.KickUserFail;
                res.Message = MessageCode.Message[(int)ResponseCode.KickUserFail];
                _logger.LogError("Get Maintenance Info exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
                return res;
            }
        }
    }
}
