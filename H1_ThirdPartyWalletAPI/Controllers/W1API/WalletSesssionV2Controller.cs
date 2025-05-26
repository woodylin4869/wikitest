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
using H1_ThirdPartyWalletAPI.Service.W1API;

namespace H1_ThirdPartyWalletAPI.Controllers.W1API
{
    [ApiController]
    [Route("w1api")]
    public class WalletSessionV2Controller : ControllerBase
    {
        private readonly ILogger<WalletSessionV2Controller> _logger;
        private readonly IWalletSessionService _walletSessionService;
        private readonly ITransferWalletService _transferWalletService;
        private readonly ISingleWalletService _singleWalletService;
        public WalletSessionV2Controller(ILogger<WalletSessionV2Controller> logger,
             IWalletSessionService walletSessionService,
             ITransferWalletService transferWalletService,
             ISingleWalletService singleWalletService)
        {
            _logger = logger;
            _walletSessionService = walletSessionService;
            _transferWalletService = transferWalletService;
            _singleWalletService = singleWalletService;
        }
        /// <summary>
        /// 取得Wallet Session
        /// </summary>
        [HttpGet("WalletSessionV2")]
        async public Task<GetClubSessionV2Res> Get([FromQuery] GetWalletSessionV2Req Req)
        {
            GetClubSessionV2Res res = new GetClubSessionV2Res();
            try
            {
                if (Req.Club_id != null)
                {
                    res.Data = new List<WalletSessionClub>();
                    var walletSession = await _walletSessionService.GetWalletSessionByClub(Req.Club_id);
                    if (walletSession != null)
                    {
                        res.Data.Add(new WalletSessionClub((short)walletSession.status, walletSession.club_id));
                    }

                    return await Task.FromResult(res);
                }
                else
                {
                    var walletSession = await _walletSessionService.GetWalletSession(Req);
                    return await Task.FromResult(walletSession);
                }
            }
            catch (Exception ex)
            {
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                res.code = (int)ResponseCode.Fail;
                res.Message = MessageCode.Message[(int)ResponseCode.Fail] + " | " + ex.Message;
                _logger.LogError("get WalletSessionV2 exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
                return res;
            }
        }
        /// <summary>
        /// 取得指定ID Wallet Session
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("WalletSessionv2/{id}")]
        public async Task<GetWalletSessionV2Res> Get(Guid id)
        {
            GetWalletSessionV2Res res = new GetWalletSessionV2Res();
            res.Data = new List<WalletSessionV2>();
            try
            {
                var walletSession = await _walletSessionService.GetWalletSessionById(id);
                if (walletSession != null)
                {
                    res.Data.Add(walletSession);
                }
                else
                {
                    res.code = (int)ResponseCode.SessionNotFound;
                    res.Message = MessageCode.Message[(int)ResponseCode.SessionNotFound];
                }
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
        /// 取得指定ID Wallet Session
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("WalletSessionv2/Optimize")]
        public async Task<GetWalletSessionV2Res> GetWalletSessionv2Optimize([FromQuery] GetWalletSessionv2OptimizeReq  req)
        {
            GetWalletSessionV2Res res = new GetWalletSessionV2Res();
            res.Data = new List<WalletSessionV2>();
            try
            {
                var walletSession = await _walletSessionService.GetWalletSessionByIdAndStartTime(req.Id.Value, req.StartTime.Value);
                if (walletSession != null)
                {
                    res.Data.Add(walletSession);
                }
                else
                {
                    res.code = (int)ResponseCode.SessionNotFound;
                    res.Message = MessageCode.Message[(int)ResponseCode.SessionNotFound];
                }
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
        /// 取得Club id 之 Wallet Session與 balance
        /// </summary>
        [HttpGet("UserWalletSession")]
        async public Task<GetUserWalletSessionRes> Get([FromQuery] GetUserWalletSessionReq Req)
        {
            GetUserWalletSessionRes res = new GetUserWalletSessionRes();
            GetMemberBalance memberBalance = new GetMemberBalance();
            try
            {
                if (Config.OneWalletAPI.WalletMode == "SingleWallet")
                {                   
                    memberBalance = await _singleWalletService.GetMemberWalletBalance(Req.Club_id);
                }
                else
                {
                    try
                    {
                        memberBalance = await _transferWalletService.GetMemberWalletBalanceCache(Req.Club_id);
                        if (memberBalance.code != (int)ResponseCode.Success)
                        {
                            throw new ExceptionMessage(memberBalance.code, memberBalance.Message);
                        }
                        res.Amount = memberBalance.Data.Sum(x => x.Amount);
                    }
                    catch (CacheLockingException)
                    {
                        res.Amount = decimal.Zero;
                    }
                }
                var session = await _walletSessionService.GetWalletSessionByClub(Req.Club_id);
                if (session != null)
                {
                    res.SessionId = session.session_id.ToString();
                }               
                return res;
            }
            catch (ExceptionMessage ex)
            {
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                res.code = ex.MsgId;
                res.Message = MessageCode.Message[(int)ResponseCode.Fail] + " | " + ex.Message;
                _logger.LogInformation("get UserWalletSession exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
                return res;
            }
            catch (Exception ex)
            {
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                res.code = (int)ResponseCode.Fail;
                res.Message = MessageCode.Message[(int)ResponseCode.Fail] + " | " + ex.Message;
                _logger.LogError("get UserWalletSession exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
                return res;
            }
        }
    }
}
