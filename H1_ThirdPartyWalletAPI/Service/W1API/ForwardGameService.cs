using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Linq;
using H1_ThirdPartyWalletAPI.Service.Game;
using H1_ThirdPartyWalletAPI.Model.DataModel;
using H1_ThirdPartyWalletAPI.Model.W1API;
using H1_ThirdPartyWalletAPI.Code;
using System.Data;
using H1_ThirdPartyWalletAPI.Model;
using H1_ThirdPartyWalletAPI.Service.Common;
using H1_ThirdPartyWalletAPI.Model.Config;
using Microsoft.AspNetCore.Hosting;
namespace H1_ThirdPartyWalletAPI.Service
{
    public interface IForwardGameService
    {
        public Task<ForwardGame> ForwardGame(ForwardGameReq request);
    }
    public class ForwardGameService : IForwardGameService
    {
        private readonly ILogger<ForwardGameService> _logger;
        private readonly IGameApiService _gameaApiService;
        private readonly IGameInterfaceService _gameInterfaceService;
        private readonly ICommonService _commonService;
        private readonly ITransferService _transferService;
        private readonly IWebHostEnvironment _env;
        private int _cacheSeconds = 600;
        private readonly ITransferWalletService _transferWalletService;

        public ForwardGameService(ILogger<ForwardGameService> logger
        , ICommonService commonService
        , IGameApiService gameaApiService
        , IGameInterfaceService gameInterfaceService
        , ITransferService transferService
        , IWebHostEnvironment env,
        ITransferWalletService transferWalletService)
        {
            _logger = logger;
            _gameaApiService = gameaApiService;
            _gameInterfaceService = gameInterfaceService;
            _commonService = commonService;
            _transferService = transferService;
            _env = env;
            _transferWalletService = transferWalletService;
        }

        public async Task<ForwardGame> ForwardGame(ForwardGameReq request)
        {
            var expiry = TimeSpan.FromSeconds(15);
            var wait = TimeSpan.FromSeconds(3);
            var retry = TimeSpan.FromMilliseconds(500);
            var lockKey = $"{LockRedisCacheKeys.W1WalletLock}:{request.Club_id}";
            try
            {
                return await _commonService._cacheDataService.LockAsyncRegular(lockKey,
                    () => ForwardGameCore(request)
                    , expiry, wait, retry);
            }
            catch (CacheLockingException ex)
            {
                return new()
                {
                    Url = "",
                    code = 9999,
                    Message = ex.Message
                };
            }
        }

        async private Task<ForwardGame> ForwardGameCore(ForwardGameReq request)
        {
            Platform platformid = (Platform)Enum.Parse(typeof(Platform), request.Platform.ToUpper());
            ForwardGame res = new ForwardGame();
            res.Url = "";
            try
            {
                //確認遊戲館狀態
                var GammeProviderInfoList = await _commonService._apiHealthCheck.GetAllHealthInfo();
                var GammeProvider = GammeProviderInfoList.Where(x => x.Platform.ToLower() == request.Platform.ToLower()).ToList();
                if (GammeProvider.Count > 0)
                {
                    switch (GammeProvider.FirstOrDefault().Status)
                    {
                        case Status.TIMEOUT:
                            throw new ExceptionMessage((int)ResponseCode.GameApiTimeOut, MessageCode.Message[(int)ResponseCode.GameApiTimeOut]);
                        case Status.MAINTAIN:
                            throw new ExceptionMessage((int)ResponseCode.GameApiMaintain, MessageCode.Message[(int)ResponseCode.GameApiMaintain]);
                        default:
                            break;
                    }
                }
                List<string> openGame = new List<string>(Config.OneWalletAPI.OpenGame.Split(','));
                if (!openGame.Contains(request.Platform.ToUpper())) //只允許開放的遊戲進入
                {
                    throw new ExceptionMessage((int)ResponseCode.UnavailablePlatform, MessageCode.Message[(int)ResponseCode.UnavailablePlatform]);
                }

                //get member wallet
                Wallet userData = await _transferWalletService.GetWalletCache(request.Club_id);
                if (userData == null)
                {
                    throw new ExceptionMessage((int)ResponseCode.UserNotFound, MessageCode.Message[(int)ResponseCode.UserNotFound]);
                }
                //H1需要在Session status = Deposit 才可以進入遊戲
                if (Config.OneWalletAPI.RCGMode == "H1")
                {
                    WalletSessionV2 resData = await _commonService._cacheDataService.GetOrSetValueAsync($"{RedisCacheKeys.WalletSession}/{L2RedisCacheKeys.club}/{request.Club_id}",
                    async () =>
                    {
                        List<short> status = new List<short>{
                              (short)WalletSessionV2.SessionStatus.DEPOSIT
                        };
                        var walletSessionV2 = await _commonService._serviceDB.GetWalletSessionV2(status, request.Club_id);
                        if (walletSessionV2.Count() == 1)
                        {
                            return walletSessionV2.SingleOrDefault();
                        }
                        else if (walletSessionV2.Count() > 1)
                        {
                            throw new ExceptionMessage((int)ResponseCode.SessionMultiFail, MessageCode.Message[(int)ResponseCode.SessionMultiFail]);
                        }
                        else
                        {
                            return null;
                        }
                    },
                    _cacheSeconds);

                    if (resData == null || resData.status != WalletSessionV2.SessionStatus.DEPOSIT)
                    {
                        throw new ExceptionMessage((int)ResponseCode.SessionNotFound, MessageCode.Message[(int)ResponseCode.SessionNotFound]);
                    }
                    else
                    {
                        //update sesssion time
                        await _commonService._serviceDB.PutWalletSessionV2(resData.session_id);
                    }
                }
                //get platform user
                var platform_user = await _commonService._gamePlatformUserService.GetGamePlatformUserAsync(request.Club_id);
                //判斷有更換遊戲才轉出其它遊戲餘額
                if (userData.last_platform != request.Platform)
                {
                    //cash out from other game
                    var KicktaskList = new List<Task<bool>>();
                    foreach (string r in openGame)
                    {
                        if (r != request.Platform && r == userData.last_platform)
                        {
                            var gameUserData = platform_user.FirstOrDefault(x => x.game_platform == r);
                            if (gameUserData != null)
                            {
                                var GammeProviderInfo = GammeProviderInfoList.Where(x => x.Platform.ToLower() == r.ToLower()).ToList();
                                if (GammeProviderInfo.Count > 0)
                                {
                                    switch (GammeProviderInfo.FirstOrDefault().Status)
                                    {
                                        case Status.TIMEOUT:
                                        case Status.MAINTAIN:
                                            break;
                                        default:
                                            KicktaskList.Add(_transferService.KickUser(gameUserData, r));
                                            break;
                                    }
                                }
                                else
                                {
                                    KicktaskList.Add(_transferService.KickUser(gameUserData, r));
                                }
                            }
                        }
                    }
                    await Task.WhenAll(KicktaskList);
                    var CashoutTaskList = new List<Task<string>>();
                    foreach (string r in openGame)
                    {
                        if (r != request.Platform && r == userData.last_platform)
                        {
                            var gameUserData = platform_user.FirstOrDefault(x => x.game_platform == r);
                            if (gameUserData != null)
                            {
                                Platform source = (Platform)Enum.Parse(typeof(Platform), r, true);

                                var GammeProviderInfo = GammeProviderInfoList.Where(x => x.Platform.ToLower() == r.ToLower()).ToList();
                                if (GammeProviderInfo.Count > 0)
                                {
                                    switch (GammeProviderInfo.FirstOrDefault().Status)
                                    {
                                        case Status.TIMEOUT:
                                        case Status.MAINTAIN:
                                            break;
                                        default:
                                            CashoutTaskList.Add(_transferService.WithDrawGameWalletV2(gameUserData, source, Platform.W1));
                                            break;
                                    }
                                }
                                else
                                {
                                    CashoutTaskList.Add(_transferService.WithDrawGameWalletV2(gameUserData, source, Platform.W1));
                                }
                            }

                        }
                    }
                    await Task.WhenAll(CashoutTaskList);
                }
                //Step 1 Create Member
                if (!Enum.TryParse<Platform>(request.Platform.ToUpper(), out var platform)) throw new ExceptionMessage(ResponseCode.UnavailablePlatform);
                var gameUser = await _commonService._gamePlatformUserService.GetSingleGamePlatformUserAsync(request.Club_id, platform);
                if (gameUser == null)
                {
                    gameUser = await _gameInterfaceService.CreateGameUser(request, userData);
                    await _commonService._gamePlatformUserService.PostGamePlatformUserRetryAsync(gameUser, 3);
                }
                //Step 2 Transfer all W1 wallet credit to game                
                string deposit_result = await _transferService.DepositGameWalletV2(gameUser, Platform.W1, platformid, request.Stop_balance);
                if (deposit_result != "success")
                {
                    res.code = (int)ResponseCode.FundTransferFail;
                    res.Message = MessageCode.Message[(int)ResponseCode.FundTransferFail] + " | " + deposit_result;
                    throw new ExceptionMessage(res.code, res.Message);
                }
                //Step 3 Get Game URL
                res.Url = await _gameInterfaceService.Login(request, userData, gameUser);
                res.Club_id = request.Club_id;
                res.code = (int)ResponseCode.Success;
                res.Message = MessageCode.Message[(int)ResponseCode.Success];
                return res;
            }
            catch (ExceptionMessage ex)
            {
                res.code = ex.MsgId;
                res.Message = ex.Message.ToString();
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                _logger.LogInformation("Forward game exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
                return res;
            }
            catch (Exception ex)
            {
                res.Message = ex.Message.ToString();
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                _logger.LogError("Forward game exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
                return res;
            }
        }
    }
}
