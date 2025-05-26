using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using H1_ThirdPartyWalletAPI.Code;
using H1_ThirdPartyWalletAPI.Model.W1API;
using System.Threading.Tasks;
using System.Collections.Generic;
using H1_ThirdPartyWalletAPI.Service.Common;
using System.Linq;
using H1_ThirdPartyWalletAPI.Service.Game;
using H1_ThirdPartyWalletAPI.Model.Config;
using H1_ThirdPartyWalletAPI.Model.DataModel;
using H1_ThirdPartyWalletAPI.Service.W1API;

namespace H1_ThirdPartyWalletAPI.Controllers.W1API
{
    [ApiController]
    [Route("w1api/[controller]")]
    public class GetOnlineUserController : ControllerBase
    {
        private readonly ILogger<GetOnlineUserController> _logger;
        private readonly IDBService _serviceDB;
        private readonly IOnlineUserService _onlineUser;
        public GetOnlineUserController(ILogger<GetOnlineUserController> logger
            , IDBService serviceDB
            , IGameApiService gameApi
            , IOnlineUserService onlineUser)
        {
            _logger = logger;
            _serviceDB = serviceDB;
            _onlineUser = onlineUser;
        }
        /// <summary>
        /// 取得未洗分玩家清單
        /// </summary>
        [HttpGet]
        public async Task<GetOnlineUser> Get([FromQuery] GetOnlineUserReq getonlineUserReq)
        {
            GetOnlineUser res = new GetOnlineUser();
            try
            {
                if (getonlineUserReq.Platform != null && !Enum.IsDefined(typeof(Platform), getonlineUserReq.Platform))
                {
                    throw new Exception(MessageCode.Message[(int)ResponseCode.UnknowPlatform]);
                }
                else
                {
                    var result = Enumerable.Empty<t_wallet_last_platform>();
                    if (getonlineUserReq.Platform is null)
                        result = await _serviceDB.GetWalletLastPlatform(); //取得全部資料
                    else
                        result = await _serviceDB.GetWalletLastPlatformByPlatform(getonlineUserReq.Platform); 

                    res.code = (int)ResponseCode.Success;
                    res.Message = MessageCode.Message[(int)ResponseCode.Success];
                    res.Data = result.Select(w => new GetOnlineUserData()
                    {
                        club_id = w.club_id,
                        last_platform = w.last_platform
                    }).ToList();

                    return res;
                }
                    
            }
            catch (Exception ex)
            {
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                _logger.LogError("Get GetOnlineUser EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
                res.code = (int)ResponseCode.GetOnlineUserFail;
                res.Message = MessageCode.Message[(int)ResponseCode.GetOnlineUserFail] + " | " + ex.Message.ToString();
                return res;
            }
        }
        /// <summary>
        /// 取得遊戲中玩家數量
        /// </summary>
        [HttpGet("Count")]
        public async Task<GetOnlineUserCount> Count([FromQuery] GetOnlineUserReq getonlineUserReq)
        {
            GetOnlineUserCount res = new GetOnlineUserCount();
            try
            {
                if (getonlineUserReq.Platform != null && !Enum.IsDefined(typeof(Platform), getonlineUserReq.Platform))
                {
                    throw new Exception(MessageCode.Message[(int)ResponseCode.UnknowPlatform]);
                }
                var getonlineUserReqData = new GetOnlineUserListReq();
                var onlineUserData =  await _onlineUser.GetOnlineUser(getonlineUserReqData);
                res.Data = new List<OnlinUserCount>();
                List<string> openGame = new List<string>(Config.OneWalletAPI.OpenGame.Split(','));

                res.Data = onlineUserData.UserList
                    .GroupBy(x => x.Platform)
                    .Where(x => openGame.Contains(x.Key))
                    .Select(x => new OnlinUserCount()
                    {
                        Platform = x.Key,
                        Count = x.Count()
                    })
                    .ToList();

                res.code = (int)ResponseCode.Success;
                res.Message = MessageCode.Message[(int)ResponseCode.Success];
                return res;
            }
            catch (Exception ex)
            {
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                _logger.LogError("Get GetOnlineUserCount EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
                res.code = (int)ResponseCode.GetOnlineUserFail;
                res.Message = MessageCode.Message[(int)ResponseCode.GetOnlineUserFail] + " | " + ex.Message.ToString();
                return res;
            }
        }
        /// <summary>
        /// 取得遊戲中玩家清單
        /// </summary>
        [HttpGet("List")]
        public async Task<GetOnlineUserListRes> List([FromQuery] GetOnlineUserListReq getonlineUserReq)
        {
            GetOnlineUserListRes res = new GetOnlineUserListRes();
            try
            {
                if (getonlineUserReq.Platform != null && !Enum.IsDefined(typeof(Platform), getonlineUserReq.Platform))
                {
                    throw new Exception(MessageCode.Message[(int)ResponseCode.UnknowPlatform]);
                }
                return await _onlineUser.GetOnlineUser(getonlineUserReq);
            }
            catch (Exception ex)
            {
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                _logger.LogError("Get GetOnlineUserListReq EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
                res.code = (int)ResponseCode.GetOnlineUserFail;
                res.Message = MessageCode.Message[(int)ResponseCode.GetOnlineUserFail] + " | " + ex.Message.ToString();
                return res;
            }
        }
    }
}
