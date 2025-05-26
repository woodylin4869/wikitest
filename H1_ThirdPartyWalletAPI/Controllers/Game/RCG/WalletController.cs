using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using Microsoft.AspNetCore.Authorization;
using H1_ThirdPartyWalletAPI.Service;
using H1_ThirdPartyWalletAPI.Service.Common;
using H1_ThirdPartyWalletAPI.Model.OneWalletGame;
using System.Security.Claims;
using H1_ThirdPartyWalletAPI.Model.DataModel;
using H1_ThirdPartyWalletAPI.Code;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace H1_ThirdPartyWalletAPI.Controllers.Game
{

    [Route("/rcg/api/[controller]")]
    [ApiController]
    [Authorize(Roles = "RCG")]
    public class WalletController : ControllerBase
    {
        private readonly ILogger<WalletController> _logger;
        private readonly ICommonService _commonService;
        private readonly ITransferWalletService _transferWalletService;
        private readonly ISingleWalletService _singleWalletService;
        public string SystemCode { get; set; }
        public string WebId { get; set; }
        public string MemberAccount { get; set; }
        public int MemberId { get; set; }
        public WalletController(ILogger<WalletController> logger, 
            ICommonService commonService, ITransferWalletService transferWalletService, ISingleWalletService singleWalletService)
        {
            _logger = logger;
            _commonService = commonService;
            _transferWalletService = transferWalletService;
            _singleWalletService = singleWalletService;
        }
        [HttpPost]
        [Route("Balance")]
        public async Task<ResponseBaseMessage<GetBalanceResponse>> GetBalance(GetBalanceRequest request)
        {
            var identity = User.Identity as ClaimsIdentity;
            WebId = identity.FindFirst("webid").Value;
            SystemCode = identity.FindFirst("SystemCode").Value;
            MemberAccount = identity.FindFirst("memberaccount").Value;
            var tokentype = identity.FindFirst("tokentype").Value;

            if(tokentype != OW_RCG.TokenType.SessionToken.ToString())
            {
                throw new ExceptionMessage((int)ResponseCode.TokenTypeFail, MessageCode.Message[(int)ResponseCode.TokenTypeFail]);
            }
            try
            {
                //取得餘額
                Wallet results  = await _singleWalletService.GetWallet(MemberAccount);
                if (results == null)
                {
                    throw new ExceptionMessage((int)ResponseCode.UserNotFound, MessageCode.Message[(int)ResponseCode.UserNotFound]);
                }
                GetBalanceResponse result = new GetBalanceResponse();
                result.RequestId = request.RequestId;
                result.Balance = results.Credit;
                result.Account = MemberAccount;
                return new ResponseBaseMessage<GetBalanceResponse>(result);
            }
            catch (ExceptionMessage ex)
            {
                int errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                _logger.LogError("RCG Balance exception EX : {ex}  MSG : {Message} Error Line : {errorLine}", ex.GetType().FullName.ToString(), ex.Message.ToString(), errorLine);
                _logger.LogError("RCG Balance exception EX : {ex}  MSG : {Message} ",ex, ex.Message.ToString());

                HttpContext.Response.StatusCode = 400;
                return new ResponseBaseMessage<GetBalanceResponse>(
                new GetBalanceResponse()
                {
                    Account = MemberAccount,
                    Status = 0,
                    Balance = 0,
                    RequestId = request.RequestId
                });
            }
            catch (Exception ex)
            {
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                _logger.LogError("RCG Balance exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
                HttpContext.Response.StatusCode = 400;
                return new ResponseBaseMessage<GetBalanceResponse>(
                new GetBalanceResponse()
                {
                    Account = MemberAccount,
                    Status = 0,
                    Balance = 0,
                    RequestId = request.RequestId
                });
            }
        }
        // POST api/<WalletController>/Debit
        [HttpPost]
        [Route("Debit")]
        public async Task<ResponseBaseMessage<DebitResponse>> Debit(DebitRequest request)
        {
            var identity = User.Identity as ClaimsIdentity;
            WebId = identity.FindFirst("webid").Value;
            SystemCode = identity.FindFirst("SystemCode").Value;
            MemberAccount = identity.FindFirst("memberaccount").Value;
            var tokentype = identity.FindFirst("tokentype").Value;
            try
            {
                if (tokentype != OW_RCG.TokenType.SessionToken.ToString())
                {
                    throw new ExceptionMessage((int)ResponseCode.JwtTokenTypeFail, MessageCode.Message[(int)ResponseCode.JwtTokenTypeFail]);
                }
                WalletTransactionBaseMessage<dynamic> tranData = new WalletTransactionBaseMessage<dynamic>();
                tranData.Data = new RcgWalletTransaction();
                tranData.source = Platform.RCG;
                tranData.Club_id = MemberAccount;
                tranData.Data.tran_id = Guid.Parse(request.Transaction.Id);
                tranData.Data.tran_rid = request.Transaction.ReferenceId;
                tranData.Data.req_id = Guid.Parse(request.RequestId);
                tranData.Data.desk_id = request.Game.DeskId;
                tranData.Data.game_name = request.Game.GameName;
                tranData.Data.shoe_no = request.Game.Shoe;
                tranData.Data.round_no = request.Game.Run;
                tranData.Data.create_datetime = DateTime.Now;
                tranData.Data.club_id = MemberAccount;
                tranData.Data.amount = request.Transaction.Amount;
                tranData.Data.tran_type = "debit";
                long unixTimestamp = (long)DateTime.UtcNow.Subtract(tranData.Data.create_datetime).TotalSeconds;
                decimal after_balance = await _singleWalletService.WalletTransaction(tranData);
                if(after_balance < 0)
                {
                    //ransactionId_Duplicate  1  
                    //Amount_over_balance     2
                    //ReferenceId_not_found   3
                    //Other_fail              4
                    throw new ExceptionMessage(Math.Abs((int)after_balance), "wallet transaction fail");
                }
                DebitResponse response = new DebitResponse(request.RequestId, MemberAccount, request.Transaction.Id, after_balance);
                return new ResponseBaseMessage<DebitResponse>(response, unixTimestamp);
            }
            catch (ExceptionMessage ex)
            {
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                _logger.LogError("RCG debit exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
                HttpContext.Response.StatusCode = 400;
                DebitResponse response = new DebitResponse();
                return new ResponseBaseMessage<DebitResponse>(response, ex.MsgId, ex.Message.ToString());
            }
            catch (Exception ex)
            {
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                _logger.LogError("RCG debit exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
                HttpContext.Response.StatusCode = 400;
                DebitResponse response = new DebitResponse();
                return new ResponseBaseMessage<DebitResponse>(response, ex.Message.ToString());
            }
        }
        // POST api/<WalletController>/Credit
        [HttpPost]
        [Route("Credit")]
        public async Task<ResponseBaseMessage<CreditResponse>> Credit(CreditRequest request)
        {
            var identity = User.Identity as ClaimsIdentity;
            WebId = identity.FindFirst("webid").Value;
            SystemCode = identity.FindFirst("SystemCode").Value;
            MemberAccount = identity.FindFirst("memberaccount").Value;
            var tokentype = identity.FindFirst("tokentype").Value;

            try
            {
                if (tokentype != OW_RCG.TokenType.SessionToken.ToString())
                {
                    throw new ExceptionMessage((int)ResponseCode.JwtTokenTypeFail, MessageCode.Message[(int)ResponseCode.JwtTokenTypeFail]);
                }
                WalletTransactionBaseMessage<dynamic> tranData = new WalletTransactionBaseMessage<dynamic>();
                tranData.Data = new RcgWalletTransaction();
                tranData.Club_id = MemberAccount;
                tranData.source = Platform.RCG;
                tranData.Data.tran_id = Guid.Parse(request.Transaction.Id);
                tranData.Data.tran_rid = request.Transaction.ReferenceId;
                tranData.Data.req_id = Guid.Parse(request.RequestId);
                tranData.Data.desk_id = request.Game.DeskId;
                tranData.Data.game_name = request.Game.GameName;
                tranData.Data.shoe_no = request.Game.Shoe;
                tranData.Data.round_no = request.Game.Run;
                tranData.Data.create_datetime = DateTime.Now;
                tranData.Data.club_id = MemberAccount;
                tranData.Data.amount = request.Transaction.Amount;
                tranData.Data.tran_type = "credit";
                long unixTimestamp = (long)DateTime.UtcNow.Subtract(tranData.Data.create_datetime).TotalSeconds;
                decimal after_balance = await _singleWalletService.WalletTransaction(tranData);
                if (after_balance < 0)
                {
                    //ransactionId_Duplicate  1  
                    //Amount_over_balance     2
                    //ReferenceId_not_found   3
                    //Other_fail              4
                    throw new ExceptionMessage(Math.Abs((int)after_balance), "wallet transaction fail");
                }
                CreditResponse response = new CreditResponse(request.RequestId, MemberAccount, request.Transaction.Id, after_balance);
                return new ResponseBaseMessage<CreditResponse>(response, unixTimestamp);
            }
            catch (ExceptionMessage ex)
            {
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                _logger.LogError("RCG Credit exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
                HttpContext.Response.StatusCode = 400;
                CreditResponse response = new CreditResponse();
                return new ResponseBaseMessage<CreditResponse>(response, ex.MsgId, ex.Message.ToString());
            }
            catch (Exception ex)
            {
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                _logger.LogError("RCG Credit exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
                HttpContext.Response.StatusCode = 400;
                CreditResponse response = new CreditResponse();
                return new ResponseBaseMessage<CreditResponse>(response, ex.Message.ToString());
            }


        }
        // POST api/<WalletController>/Cancel
        [HttpPost]
        [Route("Cancel")]
        public async Task<ResponseBaseMessage<CancelResponse>> Cancel([FromBody] CancelRequest request)
        {
            var identity = User.Identity as ClaimsIdentity;
            WebId = identity.FindFirst("webid").Value;
            SystemCode = identity.FindFirst("SystemCode").Value;
            MemberAccount = identity.FindFirst("memberaccount").Value;
            var tokentype = identity.FindFirst("tokentype").Value;
            
            try
            {
                if (tokentype != OW_RCG.TokenType.SessionToken.ToString())
                {
                    throw new ExceptionMessage((int)ResponseCode.JwtTokenTypeFail, MessageCode.Message[(int)ResponseCode.JwtTokenTypeFail]);
                }
                WalletTransactionBaseMessage<dynamic> tranData = new WalletTransactionBaseMessage<dynamic>();

                IEnumerable<RcgWalletTransaction> reuslt = await _commonService._serviceDB.GetRcgTransaction(Guid.Parse(request.Transaction.TargetId));
                if (reuslt.Count() != 1)
                {
                    throw new ExceptionMessage(3,"target id not find transaction");

                }
                tranData.Data = new RcgWalletTransaction();
                tranData.Club_id = MemberAccount;
                tranData.source = Platform.RCG;
                tranData.Data.tran_id = Guid.Parse(request.Transaction.Id);
                tranData.Data.tran_rid = request.Transaction.TargetId;
                tranData.Data.req_id = Guid.Parse(request.RequestId);
                tranData.Data.desk_id = request.Game.DeskId;
                tranData.Data.game_name = request.Game.GameName;
                tranData.Data.shoe_no = request.Game.Shoe;
                tranData.Data.round_no = request.Game.Run;
                tranData.Data.create_datetime = DateTime.Now;
                tranData.Data.club_id = MemberAccount;
                tranData.Data.amount = reuslt.SingleOrDefault().amount;
                tranData.Data.tran_type = "cancel";
                long unixTimestamp = (long)DateTime.UtcNow.Subtract(tranData.Data.create_datetime).TotalSeconds;
                decimal after_balance = await _singleWalletService.WalletTransaction(tranData);
                if (after_balance < 0)
                {
                    //ransactionId_Duplicate  1  
                    //Amount_over_balance     2
                    //ReferenceId_not_found   3
                    //Other_fail              4
                    throw new ExceptionMessage(Math.Abs((int)after_balance), "wallet transaction fail");
                }
                CancelResponse response = new CancelResponse(request.RequestId, MemberAccount, request.Transaction.Id, after_balance);
                return new ResponseBaseMessage<CancelResponse>(response, unixTimestamp);
            }
            catch (ExceptionMessage ex)
            {
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                _logger.LogError("RCG Cancel exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
                HttpContext.Response.StatusCode = 400;
                CancelResponse response = new CancelResponse();
                return new ResponseBaseMessage<CancelResponse>(response, ex.MsgId, ex.Message.ToString());
            }
            catch (Exception ex)
            {
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                _logger.LogError("RCG Cancel exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
                HttpContext.Response.StatusCode = 400;
                CancelResponse response = new CancelResponse();
                return new ResponseBaseMessage<CancelResponse>(response, ex.Message.ToString());
            }

        }
    }
}
