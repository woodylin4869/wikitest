using H1_ThirdPartyWalletAPI.Code;
using H1_ThirdPartyWalletAPI.Model.Game;
using H1_ThirdPartyWalletAPI.Model.W1API;
using H1_ThirdPartyWalletAPI.Service;
using H1_ThirdPartyWalletAPI.Service.Common;
using H1_ThirdPartyWalletAPI.Service.Game;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using H1_ThirdPartyWalletAPI.Model.Config;
using System.Collections.Generic;
using System.Linq;
using H1_ThirdPartyWalletAPI.Model.DataModel;
using H1_ThirdPartyWalletAPI.Model;
using Npgsql;
using Microsoft.AspNetCore.Authorization;

namespace H1_ThirdPartyWalletAPI.Controllers.W1API
{
    [ApiController]
    [Route("w1api")]
    public class UserWalletController : ControllerBase
    {
        private readonly ILogger<UserWalletController> _logger;
        private readonly ICommonService _commonService;
        private readonly ITransferWalletService _transferWalletService;
        private readonly ISingleWalletService _singleWalletService;
        public UserWalletController(ILogger<UserWalletController> logger,
             ICommonService commonService,
             ITransferWalletService transferWalletService,
             ISingleWalletService singleWalletService
             )
        {
            _logger = logger;
            _commonService = commonService;
            _transferWalletService = transferWalletService;
            _singleWalletService = singleWalletService;
        }
        /// <summary>
        /// 建立使用者錢包
        /// </summary>
        //[Authorize(Roles = "admin")]
        [HttpPost]
        [Route("CreateUser")]
        async public Task<CreateUserRes> Post(CreateUserReq request)
        {
            CreateUserRes res = new CreateUserRes();
            res.Club_id = request.Club_id;
            try
            {
                //確認貨幣
                if(!Enum.IsDefined(typeof(Currency), request.Currency))
                {
                    throw new Exception("不支援的貨幣");
                }
                //Check or create member wallet
                string resCreateWallet = await _transferWalletService.CreateMemberWallet(request.Club_id, request.Club_Ename, request.Currency, request.Franchiser_id);
                if (resCreateWallet == "fail")
                {
                    throw new Exception("建立使用者失敗");
                }


                res.code = (int)ResponseCode.Success;
                res.Message = MessageCode.Message[(int)ResponseCode.Success];
                return res;
            }
            catch (Exception ex)
            {
                res.code = (int)ResponseCode.CreateMemberFail;
                res.Message = MessageCode.Message[(int)ResponseCode.CreateMemberFail] + " | " + ex.Message.ToString();
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                _logger.LogError("Create user exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
                return res;
            }
        }

        /// <summary>
        /// 設定停利額度
        /// </summary>
        //[HttpPost]
        //[Route("StopBalance")]
        //async public Task<ResCodeBase> StopBalance(StopBalanceReq request)
        //{
        //    ResCodeBase res = new ResCodeBase();
        //    try
        //    {
        //        if(request.stop_balance == 0 || request.stop_balance < -1)
        //        {
        //            throw new ExceptionMessage((int)ResponseCode.Fail, MessageCode.Message[(int)ResponseCode.Fail]);
        //        }
        //        //Check member wallet
        //        var wallet = await _commonService._transferWalletService.GetWalletCache(request.Club_id);
        //        if (wallet != null)
        //        {
        //            wallet.stop_balance = request.stop_balance;
        //            await _commonService._serviceDB.PutWalletStopBalance(wallet);
        //        }
        //        else
        //        {
        //            throw new ExceptionMessage((int)ResponseCode.UserNotFound, MessageCode.Message[(int)ResponseCode.UserNotFound]);
        //        }
        //        res.code = (int)ResponseCode.Success;
        //        res.Message = MessageCode.Message[(int)ResponseCode.Success];
        //        return res;
        //    }
        //    catch (ExceptionMessage ex)
        //    {
        //        res.code = ex.MsgId;
        //        res.Message = ex.Message;
        //        return res;
        //    }
        //    catch (Exception ex)
        //    {
        //        res.code = (int)ResponseCode.Fail;
        //        res.Message = MessageCode.Message[(int)ResponseCode.Fail] + " | " + ex.Message.ToString();
        //        var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
        //        var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
        //        _logger.LogError("StopBalance exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
        //        return res;
        //    }
        //}
        /// <summary>
        /// 取得使用者錢包清單總計
        /// </summary>
        [HttpGet("UserWallet/Summary")]
        public async Task<GetUserWalletSummaryRes> GetSummary([FromQuery] GetUserWalletSummaryReq Req)
        {
            GetUserWalletSummaryRes res = new GetUserWalletSummaryRes();
            try
            {
                IEnumerable<dynamic> result = await _commonService._serviceDB.GetWalletSummary(Req.Franchiser);
                res.Count = (int)result.SingleOrDefault().count;
                return res;
            }
            catch (Exception ex)
            {
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                res.code = (int)ResponseCode.Fail;
                res.Message = MessageCode.Message[(int)ResponseCode.Fail] + "|" + ex.Message;
                _logger.LogError("get gamelist summary exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
                return res;
            }
        }
        /// <summary>
        /// 取得使用者錢包清單
        /// </summary>
        [HttpGet("UserWallet")]
        async public Task<GetUserWalletRes> Get([FromQuery] GetUserWalletReq Req)
        {
            GetUserWalletRes res = new GetUserWalletRes();
            try
            {
                IEnumerable<Wallet> result = await _commonService._serviceDB.GetWalletList(Req);
                res.Data = result.ToList();
                return await Task.FromResult(res);
            }
            catch (Exception ex)
            {
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                res.code = (int)ResponseCode.Fail;
                res.Message = MessageCode.Message[(int)ResponseCode.Fail] + " | " + ex.Message;
                _logger.LogError("get walletlist exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
                return res;
            }
        }
        /// <summary>
        /// 取得指定ID使用者錢包
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("UserWallet/{id}")]
        public async Task<GetUserWalletRes> Get(string id)
        {
            GetUserWalletRes res = new GetUserWalletRes();
            res.Data = new List<Wallet>();
            try
            {
                Wallet userData = await _transferWalletService.GetWalletCache(id);
                res.Data.Add(userData);
                return res;
            }
            catch (Exception ex)
            {
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                res.code = (int)ResponseCode.Fail;
                res.Message = MessageCode.Message[(int)ResponseCode.Fail] + "|" + ex.Message;
                _logger.LogError("get user wallet id exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
                return res;
            }
        }
        /// <summary>
        /// 更新指定ID使用者錢包
        /// </summary>
        /// <param name="id"></param>
        /// <param name="req"></param>
        /// <returns></returns>
        [Authorize(Roles = "admin")]
        [HttpPut("UserWallet/{id}")]
        public async Task<ResCodeBase> Put([FromBody] PutUserWalletReq req, string id)
        {
            ResCodeBase res = new ResCodeBase();
            try
            {
                using (NpgsqlConnection conn = new NpgsqlConnection(Config.OneWalletAPI.DBConnection.Wallet.Master))
                {
                    await conn.OpenAsync();
                    using (var tran = conn.BeginTransaction())
                    {
                        try
                        {
                            await _commonService._cacheDataService.LockAsync(
                            $"{RedisCacheKeys.WalletTransaction}/wallet/{id}",
                            async () =>
                            {
                                Wallet walletData;
                                // 取餘額
                                if (Config.OneWalletAPI.WalletMode == "SingleWallet")
                                {
                                    walletData = await _singleWalletService.GetWallet(id);
                                }
                                else
                                {
                                    walletData = await _transferWalletService.GetWallet(conn, tran, id);
                                }
                                if (walletData == null)
                                {
                                    throw new ExceptionMessage((int)ResponseCode.UserNotFound, MessageCode.Message[(int)ResponseCode.UserNotFound]);
                                }
                                walletData.Credit = (req.credit == null) ? walletData.Credit : req.credit.GetValueOrDefault();
                                walletData.Lock_credit = (req.lock_credit == null) ? walletData.Lock_credit : req.lock_credit.GetValueOrDefault();
                                walletData.Franchiser_id = (req.franchiser_id == null) ? walletData.Franchiser_id : req.franchiser_id;

                                if (Config.OneWalletAPI.WalletMode == "SingleWallet")
                                {
                                    await _singleWalletService.SetWallet(walletData);
                                }
                                else
                                {
                                    await _transferWalletService.SetWallet(conn, tran, walletData);
                                }
                            });
                        }
                        catch (Exception ex)
                        {
                            await tran.RollbackAsync();
                            throw new Exception(ex.Message);
                        }
                    }
                    await conn.CloseAsync();
                }
                return res;
            }
            catch (Exception ex)
            {
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                res.code = (int)ResponseCode.Fail;
                res.Message = MessageCode.Message[(int)ResponseCode.Fail] + "|" + ex.Message;
                _logger.LogError("put gamelist id EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
                return res;
            }
        }
        /// <summary>
        /// 刪除指定ID使用者錢包
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize(Roles = "admin")]
        [HttpDelete("UserWallet/{id}")]
        public async Task<ResCodeBase> Delete(string id)
        {
            ResCodeBase res = new ResCodeBase();
            try
            {
                if (await _commonService._serviceDB.DeleteWallet(id) != 1)
                    throw new Exception("delete user wallet fail");
                return res;
            }
            catch (Exception ex)
            {
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                res.code = (int)ResponseCode.Fail;
                res.Message = MessageCode.Message[(int)ResponseCode.Fail] + "|" + ex.Message;
                _logger.LogError("get gamelist summary exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
                return res;
            }
        }
    }
}
