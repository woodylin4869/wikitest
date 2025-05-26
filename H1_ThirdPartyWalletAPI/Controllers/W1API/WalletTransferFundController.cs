using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System;
using Microsoft.Extensions.Configuration;
using H1_ThirdPartyWalletAPI.Code;
using H1_ThirdPartyWalletAPI.Model.W1API;
using H1_ThirdPartyWalletAPI.Service;
using H1_ThirdPartyWalletAPI.Service.Common;
using System.Threading.Tasks;
using H1_ThirdPartyWalletAPI.Model.Config;
using H1_ThirdPartyWalletAPI.Model.DataModel;
using System.Linq;

namespace H1_ThirdPartyWalletAPI.Controllers.W1API
{
    [ApiController]
    [Route("w1api/[controller]")]
    public class WalletTransferFundController : ControllerBase
    {
        private readonly ILogger<WalletTransferFundController> _logger;
        private readonly ITransferService _transferService;
        private readonly ICommonService _commonService;
        private readonly ITransferWalletService _transferWalletService;
        public WalletTransferFundController(ILogger<WalletTransferFundController> logger, 
            ITransferService transferService, 
            ICommonService commonService,
            ITransferWalletService transferWalletService
            )
        {
            _logger = logger;
            _transferService = transferService;
            _commonService = commonService;
            _transferWalletService = transferWalletService;
        }
        /// <summary>
        /// H1 and W1 全額轉帳功能 
        /// </summary>
        [HttpPost]
        async public Task<TransferFund> Post(TransferFundReq request)
        {
            TransferFund res = new TransferFund();
            try
            {
                if (Config.OneWalletAPI.RCGMode == "H1")
                {
                    throw new Exception("illegal api function");
                }
                if (request.Amount <= 0 && request.Action.ToLower() == "in" || request.Amount <= 0 && !request.CashOutAll && request.Action.ToLower() == "out")
                {
                    throw new Exception("can't transfer in zero credit");
                }
                await _transferWalletService.DeleteMemberWalletBalanceCache(request.Club_id);
                var platform_user = await _commonService._gamePlatformUserService.GetGamePlatformUserAsync(request.Club_id);
                if (platform_user == null)
                {
                    throw new Exception("No User Data");
                }
                switch (request.Action.ToLower())
                {
                    case "in":
                        res = await _transferService.TransferFund(request.id, request.Club_id, Platform.H1, Platform.W1, request.Amount, request.CashOutAll);                
                        break;
                    case "out":
                        if (Config.OneWalletAPI.WalletMode == "SingleWallet")
                        {
                            //改單一錢包不從遊戲取得餘額
                        }
                        else
                        {
                            List<string> openGame = new List<string>(Config.OneWalletAPI.OpenGame.Split(','));                           
                            var KicktaskList = new List<Task<bool>>();
                            foreach (string r in openGame)
                            {
                                var gameUser = platform_user.FirstOrDefault(x => x.game_platform == r);
                                if (gameUser == null)
                                {
                                    KicktaskList.Add(_transferService.KickUser(gameUser, r));
                                }
                                
                            }
                            await Task.WhenAll(KicktaskList);

                            var CashoutTaskList = new List<Task<string>>();
                            foreach (string r in openGame)
                            {
                                var gameUser = platform_user.FirstOrDefault(x => x.game_platform == r);
                                if (gameUser != null)
                                {
                                    Platform source = (Platform)Enum.Parse(typeof(Platform), r, true);
                                    if (source == Platform.RCG && Config.OneWalletAPI.RCGMode == "W2")
                                    {
                                        CashoutTaskList.Add(_transferService.WithDrawGameWalletV2(gameUser, source, Platform.W1, request.Amount));
                                    }
                                    else
                                    {
                                        CashoutTaskList.Add(_transferService.WithDrawGameWalletV2(gameUser, source, Platform.W1));
                                    }
                                }
                       
                            }
                            await Task.WhenAll(CashoutTaskList);
                        }
                        res = await _transferService.TransferFund(request.id, request.Club_id, Platform.W1, Platform.H1, request.Amount, request.CashOutAll);                    
                        break;
                    default:
                        throw new Exception("illegal Action");
                }
                return res;
            }
            catch (Exception ex)
            {
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                res.code = (int)ResponseCode.FundTransferW1Fail;
                res.Message = MessageCode.Message[(int)ResponseCode.FundTransferW1Fail] + " | " + ex.Message.ToString();
                if(ex.Message == "can't transfer in zero credit")
                {
                    _logger.LogInformation("TransferFund exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
                }
                else
                {
                    _logger.LogError("TransferFund exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
                }
                return res;
            }
        }
        /// <summary>
        /// 取得指定ID轉帳紀錄
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        public async Task<GetTransactionSummary> Get(Guid id)
        {
            GetTransactionSummary res = new GetTransactionSummary();
            try
            {
                var result = await _commonService._serviceDB.GetTransferRecordById(id);
                res.Data = new List<WalletTransferRecord>();
                res.Data.Add(result);
                return res;
            }
            catch (Exception ex)
            {
                res.code = (int)ResponseCode.Fail;
                res.Message = MessageCode.Message[(int)ResponseCode.Fail] + "|" + ex.Message;
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                _logger.LogError("get WalletTransferFund id exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
                return res;
            }
        }

    }
}
