using H1_ThirdPartyWalletAPI.Code;
using H1_ThirdPartyWalletAPI.Model;
using H1_ThirdPartyWalletAPI.Model.Config;
using H1_ThirdPartyWalletAPI.Model.DataModel;
using H1_ThirdPartyWalletAPI.Model.W1API;
using H1_ThirdPartyWalletAPI.Service.Common;
using H1_ThirdPartyWalletAPI.Service.Game;
using Microsoft.Extensions.Logging;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace H1_ThirdPartyWalletAPI.Service
{
    public interface ITransferWalletService
    {
        public Task<string> CreateMemberWallet(string Club_id, string Club_Name, string currency, string Franchiser_id);
        public Task<GetMemberBalance> GetMemberWalletBalance(string Club_id);
        public Task<GetMemberBalance> GetMemberWalletBalanceCache(string Club_id);
        public Task<bool> DeleteMemberWalletBalanceCache(string Club_id);
        //public Task<string> GetMemberWallet();
        public Task<string> UpdateRcgAuthToken(string Club_id, string token);
        public Task<Wallet> GetWallet(NpgsqlConnection conn, IDbTransaction tran, string Club_id);
        public Task<Wallet> GetWalletCache(string Club_id);
        public Task<bool> SetWallet(NpgsqlConnection conn, IDbTransaction tran, Wallet walletData);
        //public Task<List<GamePlatformUser>> GetPlatformUserCache(string Club_id);
        //Task<bool> SetPlatformUser(GamePlatformUser platform_user);
        public Task<MemberBalance> GetGameCredit(Platform platform, GamePlatformUser platform_user);

    }
    public class TransferWalletService : ITransferWalletService
    {
        private readonly ILogger<TransferWalletService> _logger;
        //private readonly IGameApiService _gameaApiService;
        private readonly IGameInterfaceService _gameInterfaceService;
        private readonly ICacheDataService _cacheDataService;
        private readonly IDBService _dBService;
        private readonly IApiHealthCheckService _apiHealthCheckService;
        private readonly IGamePlatformUserService _gamePlatformUserService;
        //private readonly IMemoryCache _memoryCache;
        private int _cacheSeconds = 600;
        const int _BalanceCacheSeconds = 10;


        public TransferWalletService(
            ILogger<TransferWalletService> logger,
            ICacheDataService cacheDataService,
            IDBService dBService,
            IGameInterfaceService gameInterfaceService,
            IApiHealthCheckService apiHealthCheckService,
            IGamePlatformUserService gamePlatformUserService)
        {
            _logger = logger;
            _gameInterfaceService = gameInterfaceService;
            _cacheDataService = cacheDataService;
            _dBService = dBService;
            _apiHealthCheckService = apiHealthCheckService;
            _gamePlatformUserService = gamePlatformUserService;
        }
        public async Task<string> CreateMemberWallet(string Club_id, string Club_Name, string Currency, string Franchiser_id)
        {
            try
            {
                string conncetionString = Config.OneWalletAPI.DBConnection.Wallet.Master;
                using (NpgsqlConnection conn = new NpgsqlConnection(conncetionString))
                {
                    await conn.OpenAsync();
                    using (var tran = conn.BeginTransaction())
                    {
                        IEnumerable<dynamic> results = await _dBService.GetWalletLock(conn, tran, Club_id);
                        results = results.ToList();
                        if (results.Count() != 1)
                        {
                            Wallet memberWallet = new Wallet();
                            memberWallet.Club_id = Club_id;
                            memberWallet.Club_Ename = Club_Name;
                            memberWallet.Credit = 0;
                            memberWallet.Lock_credit = 0;
                            memberWallet.Currency = Currency;
                            memberWallet.Franchiser_id = Franchiser_id;
                            if (await _dBService.PostWallet(conn, tran, memberWallet) != 1)
                            {
                                return "fail";
                            }
                            await _cacheDataService.StringSetAsync($"{RedisCacheKeys.WalletTransaction}/wallet/{Club_id}", memberWallet, _cacheSeconds);
                        }
                        await tran.CommitAsync();
                    }
                    await conn.CloseAsync();
                    return "success";
                }
            }
            catch (ExceptionMessage ex)
            {
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                _logger.LogError("CreateMemberWallet exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
                return "fail";
            }
            catch (Exception ex)
            {
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                _logger.LogError("CreateMemberWallet exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
                return "fail";
            }
        }
        public async Task<GetMemberBalance> GetMemberWalletBalance(string Club_id)
        {
            GetMemberBalance resData = new GetMemberBalance();
            try
            {
                resData.Data = new List<MemberBalance>();
                //取得中心錢包餘額
                Wallet results = await GetWalletCache(Club_id);
                if (results == null)
                {
                    throw new ExceptionMessage((int)ResponseCode.UserNotFound, MessageCode.Message[(int)ResponseCode.UserNotFound]);
                }
                MemberBalance W1Balance = new MemberBalance();
                W1Balance.Wallet = nameof(Platform.W1);
                W1Balance.Amount = results.Credit;
                resData.Data.Add(W1Balance);

                var platform_user = await _gamePlatformUserService.GetGamePlatformUserAsync(Club_id);
                List<string> openGame = new List<string>(Config.OneWalletAPI.OpenGame.Split(','));
                var taskList = new List<Task<MemberBalance>>();
                foreach (string r in openGame)
                {
                    Platform platformid = (Platform)Enum.Parse(typeof(Platform), r.ToUpper());
                    var gameUser = platform_user.FirstOrDefault(x => x.game_platform == r);
                    //確認遊戲館狀態
                    var GammeProviderInfoList = await _apiHealthCheckService.GetAllHealthInfo();
                    var GammeProvider = GammeProviderInfoList.Where(x => x.Platform.ToLower() == r.ToLower()).ToList();
                    if (GammeProvider.Count > 0)
                    {
                        switch (GammeProvider.FirstOrDefault().Status)
                        {
                            case Status.TIMEOUT:
                            case Status.MAINTAIN:
                                break;
                            default:
                                if (gameUser != null)
                                {
                                    taskList.Add(_gameInterfaceService.GetGameCredit(platformid, gameUser));
                                }
                                break;
                        }
                    }
                    else
                    {
                        if (gameUser != null)
                        {
                            taskList.Add(_gameInterfaceService.GetGameCredit(platformid, gameUser));
                        }
                    }
                }
                var countResult = await Task.WhenAll(taskList);
                foreach (MemberBalance r in countResult)
                {
                    if (r.Wallet != null)
                        resData.Data.Add(r);
                }
                return resData;
            }
            catch (ExceptionMessage ex)
            {
                resData.code = ex.MsgId;
                resData.Message = ex.Message;
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                _logger.LogError(ex, "GetMemberWalletBalance exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
                return resData;
            }
            catch (Exception ex)
            {
                resData.code = (int)ResponseCode.GetBalanceFail;
                resData.Message = MessageCode.Message[(int)ResponseCode.GetBalanceFail] + " | " + ex.Message.ToString();
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                _logger.LogError(ex, "GetMemberWalletBalance exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
                return resData;
            }
        }

        public async Task<GetMemberBalance> GetMemberWalletBalanceCache(string Club_id)
        {
            return await _cacheDataService.GetOrSetValueAsync($"{RedisCacheKeys.WalletTransaction}/balance/{Club_id}",
                async () =>
                {
                    var resData = default(GetMemberBalance);

                    var expiry = TimeSpan.FromSeconds(10);
                    var wait = TimeSpan.FromSeconds(5);
                    var retry = TimeSpan.FromMilliseconds(500);

                    await _cacheDataService.LockAsyncRegular($"{RedisCacheKeys.WalletTransaction}/balance/{Club_id}"
                        , async () =>
                        {
                            resData = await _cacheDataService.StringGetAsync<GetMemberBalance>($"{RedisCacheKeys.WalletTransaction}/balance/{Club_id}");
                            if (resData != default)
                                return;

                            resData = await GetMemberWalletBalance(Club_id);
                        }, expiry, wait, retry);

                    return resData;
                }
                , _BalanceCacheSeconds);
        }
        public async Task<bool> DeleteMemberWalletBalanceCache(string Club_id)
        {
            return await _cacheDataService.KeyDelete($"{RedisCacheKeys.WalletTransaction}/balance/{Club_id}");
        }
        public async Task<string> UpdateRcgAuthToken(string Club_id, string token)
        {
            try
            {
                if (await _dBService.PutRcgToken(Club_id, token) != 1)
                {
                    throw new Exception("更新auth token失敗");
                }
                return "success";
            }
            catch (Exception ex)
            {
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                _logger.LogError("UpdateRcgAuthToken exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
                return ex.Message.ToString();
            }
        }
        public async Task<Wallet> GetWallet(NpgsqlConnection conn, IDbTransaction tran, string Club_id)
        {
            IEnumerable<Wallet> result = await _dBService.GetWalletLock(conn, tran, Club_id);
            if (result.Count() != 1)
            {
                throw new ExceptionMessage((int)ResponseCode.UserNotFound, MessageCode.Message[(int)ResponseCode.UserNotFound]);
            }
            Wallet resData = result.SingleOrDefault();
            await _cacheDataService.StringSetAsync($"{RedisCacheKeys.WalletTransaction}/wallet/{Club_id}", resData, _cacheSeconds);
            return resData;
        }
        public async Task<Wallet> GetWalletCache(string Club_id)
        {
            Wallet walletData = await _cacheDataService.GetOrSetValueAsync($"{RedisCacheKeys.WalletTransaction}/wallet/{Club_id}",
            async () =>
            {
                try
                {
                    IEnumerable<Wallet> result = await _dBService.GetWallet(Club_id);
                    if (result.Count() != 1)
                    {
                        throw new ExceptionMessage((int)ResponseCode.UserNotFound, MessageCode.Message[(int)ResponseCode.UserNotFound]);
                    }
                    return result.SingleOrDefault();
                }
                catch
                {
                    return null;
                }
            },
            _cacheSeconds);
            return walletData;
        }
        public async Task<bool> SetWallet(NpgsqlConnection conn, IDbTransaction tran, Wallet walletData)
        {
            await _dBService.PutWallet(conn, tran, walletData);
            // 更新餘額存進redis
            await _cacheDataService.StringSetAsync(
                $"{RedisCacheKeys.WalletTransaction}/wallet/{walletData.Club_id}",
                walletData,
                _cacheSeconds);
            return true;
        }
        
        public async Task<MemberBalance> GetGameCredit(Platform platform, GamePlatformUser platform_user)
        {
            return  await _gameInterfaceService.GetGameCredit(platform, platform_user);
        }
    }
}