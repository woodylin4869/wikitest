using H1_ThirdPartyWalletAPI.Code;
using H1_ThirdPartyWalletAPI.Model;
using H1_ThirdPartyWalletAPI.Model.DataModel;
using H1_ThirdPartyWalletAPI.Model.W1API;
using H1_ThirdPartyWalletAPI.Service;
using H1_ThirdPartyWalletAPI.Service.Common;
using H1_ThirdPartyWalletAPI.Service.Game;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace H1_ThirdPartyWalletAPI.Controllers.W1API
{




    [ApiController]
    [Route("w1api/[controller]")]
    public class SetLimitController : ControllerBase
    {
        private readonly ILogger<SetLimitController> _logger;
        private readonly ISaba2ApiService _Saba2API;
        private readonly ICommonService _commonService;
        private readonly ITransferWalletService _transferWalletService;
        private readonly ISingleWalletService _singleWalletService;
        private readonly IGameApiService _gameApiService;
        private readonly IGameInterfaceService _gameInterfaceService;


        public SetLimitController(ILogger<SetLimitController> logger,
            ICommonService commonService,
            ITransferWalletService transferWalletService,
            ISingleWalletService singleWalletService,
            IGameApiService gameaApiService,
            IGameInterfaceService gameInterfaceService,
            ISaba2ApiService Saba2_API
        )
        {
            _logger = logger;
            _commonService = commonService;
            _transferWalletService = transferWalletService;
            _singleWalletService = singleWalletService;
            _gameApiService = gameaApiService;
            _gameInterfaceService = gameInterfaceService;
            _Saba2API = Saba2_API;
        }

        private async Task<ResCodeBase> SetLimit(SetLimitReq request)
        {
            ResCodeBase res = new ResCodeBase();
            try
            {

                Wallet memberWalletData = await _transferWalletService.GetWalletCache(request.Club_id);
                if (memberWalletData == null)
                    throw new ExceptionMessage((int)ResponseCode.UserNotFound,
                        MessageCode.Message[(int)ResponseCode.UserNotFound]);


                if (!Enum.TryParse<Platform>(request.Platform, out var platform))
                    throw new ExceptionMessage(ResponseCode.UnknowPlatform);
                var gameUser =
                    await _commonService._gamePlatformUserService.GetSingleGamePlatformUserAsync(request.Club_id,
                        platform);

                res = await _gameInterfaceService.SetLimit(request, gameUser, memberWalletData);
                return res;

            }
            catch (ExceptionMessage ex)
            {
                res.code = (int)ResponseCode.SetLimitFail;
                res.Message = MessageCode.Message[(int)ResponseCode.SetLimitFail] + " | " + ex.Message;
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                _logger.LogInformation(
                    "Set limit exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}",
                    ex.GetType().FullName, ex.Message, errorFile, errorLine);
                return res;
            }
            catch (Exception ex)
            {
                res.code = (int)ResponseCode.SetLimitFail;
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                res.Message = MessageCode.Message[(int)ResponseCode.SetLimitFail] + " | " + ex.Message;
                _logger.LogError("Set limit exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}",
                    ex.GetType().FullName, ex.Message, errorFile, errorLine);
                return res;
            }
        }

        /// <summary>
        /// 設定會員限紅 
        /// </summary>
        [HttpPost]
        public async Task<ResCodeBase> Post(SetLimitReq request)
        {
            var expiry = TimeSpan.FromSeconds(15);
            var wait = TimeSpan.FromSeconds(3);
            var retry = TimeSpan.FromMilliseconds(500);
            var lockKey = $"{LockRedisCacheKeys.W1WalletLock}:{request.Club_id}";
            try
            {
                return await _commonService._cacheDataService.LockAsyncRegular(lockKey,
                    () => SetLimit(request)
                    , expiry, wait, retry);

            }
            catch (CacheLockingException ex)
            {
                return new()
                {
                    code = 9999,
                    Message = ex.Message
                };
            }
        }
    }
}
