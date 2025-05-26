using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using H1_ThirdPartyWalletAPI.Code;
using H1_ThirdPartyWalletAPI.Model.W1API;
using H1_ThirdPartyWalletAPI.Service;
using H1_ThirdPartyWalletAPI.Service.Common;
using System.Threading.Tasks;
using H1_ThirdPartyWalletAPI.Model.DataModel;
using H1_ThirdPartyWalletAPI.Model;

namespace H1_ThirdPartyWalletAPI.Controllers.W1API
{
    [ApiController]
    [Route("w1api/[controller]")]
    public class H1WalletTransferFundController : ControllerBase
    {
        private readonly ILogger<H1WalletTransferFundController> _logger;
        private readonly ITransferService _transferService;
        private readonly ITransferWalletService _transferWalletService;
        private readonly ICommonService _commonService;

        public H1WalletTransferFundController(ILogger<H1WalletTransferFundController> logger,
            ITransferService transferService,
            ITransferWalletService transferWalletService,
            ICommonService commonService
            )
        {
            _logger = logger;
            _transferService = transferService;
            _transferWalletService = transferWalletService;
            _commonService = commonService;
        }


        /// <summary>
        /// H1 and W1 開分/洗分
        /// </summary>
        [HttpPost]
        public async Task<ResCodeBase> Post(H1TransferFundReq request)
        {
            ResCodeBase res = new();

            var expiry = TimeSpan.FromSeconds(15);
            var wait = TimeSpan.FromSeconds(3);
            var retry = TimeSpan.FromMilliseconds(500);
            var lockKey = $"{LockRedisCacheKeys.W1WalletLock}:{request.Club_id}";
            try
            {
                return await _commonService._cacheDataService.LockAsyncRegular(lockKey,
                    () => H1TransferFundReqCore(request)
                    , expiry, wait, retry);
            }
            catch (CacheLockingException ex)
            {
                return new()
                {
                    code = (int)ResponseCode.FundTransferW1Fail,
                    Message = ex.Message
                };
            }
            catch (Exception ex)
            {
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                res.code = (int)ResponseCode.FundTransferW1Fail;
                res.Message = MessageCode.Message[(int)ResponseCode.FundTransferW1Fail] + " | " + ex.Message.ToString();

                _logger.LogError("TransferFund exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
                return res;
            }
        }

        /// <summary>
        /// H1 and W1 開分/洗分
        /// </summary>
        private async Task<ResCodeBase> H1TransferFundReqCore(H1TransferFundReq request)
        {
            ResCodeBase res = new ResCodeBase();
            try
            {
                await _transferWalletService.DeleteMemberWalletBalanceCache(request.Club_id);
                var userWallet = await _transferWalletService.GetWalletCache(request.Club_id);
                if (userWallet == null)
                {
                    throw new Exception("No User Data");
                }
                switch (request.Action.ToLower())
                {
                    case "in":
                        res = await _transferService.H1TransferFundIn(request.Session_id, request.Club_id, request.Amount);
                        break;
                    case "out":
                        res = await _transferService.H1TransferFundOut(request.Session_id, request.Club_id);
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

                _logger.LogError("TransferFund exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
                return res;
            }
        }
    }
}
