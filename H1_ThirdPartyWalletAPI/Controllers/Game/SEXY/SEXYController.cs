using H1_ThirdPartyWalletAPI.Code;
using H1_ThirdPartyWalletAPI.Model.Game.SEXY.Request;
using H1_ThirdPartyWalletAPI.Model.Game.SEXY.Response;
using H1_ThirdPartyWalletAPI.Service;
using H1_ThirdPartyWalletAPI.Service.Common;
using H1_ThirdPartyWalletAPI.Service.Game.SEXY;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using static H1_ThirdPartyWalletAPI.Model.Game.SEXY.SEXY;

namespace H1_ThirdPartyWalletAPI.Controllers.Game.SEXY
{
    /// <summary>
    /// SEXY API
    /// </summary>
    [Route("/[controller]")]
    [ApiController]
    public class SEXYController : ControllerBase
    {

        private readonly IDBService _dbIdbService;
        private readonly ITransferWalletService _transferWalletService;
        private readonly ISEXYApiService _iSEXYapiservice;
        public SEXYController(IDBService dbIdbService, ITransferWalletService transferWalletService, ISEXYApiService iSEXYapiservice)
        {
            _dbIdbService = dbIdbService;
            _transferWalletService = transferWalletService;
            _iSEXYapiservice = iSEXYapiservice;
        }

        /// <summary>
        /// 建立帳號
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("CreateMember")]
        public async Task<CreateMemberResponse> CreateMemberAsync([FromBody] CreateMemberRequest source)
        {
            source.cert = "RrqUwBase6I9QKH8bho";
            source.agentId = "h1sexytest";
            source.userId = "testJoshua3";
            source.currency = "THB";
            source.language = "th";
            source.userName = "testJoshua3";

            BetLimitClass betLimitClass = new BetLimitClass();
            Model.Game.SEXY.SEXY.LIVE lIVE = new LIVE();
            List<int> BetLimit = new List<int>();
            BetLimit.Add(260901);
            BetLimit.Add(260902);
            BetLimit.Add(260903);
            lIVE.limitId = BetLimit;
            Model.Game.SEXY.SEXY.SEXYBCRT sEXYBCRT = new SEXYBCRT();
            sEXYBCRT.LIVE = lIVE;
            betLimitClass.SEXYBCRT = sEXYBCRT;

            source.betLimit = JsonConvert.SerializeObject(betLimitClass);
            return await _iSEXYapiservice.CreateMember(source);
        }
        /// <summary>
        /// 取得餘額
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("GetBalance")]
        public async Task<GetBalanceResponse> GetBalanceAsync([FromBody] GetBalanceRequest source)
        {
            source.cert = "RrqUwBase6I9QKH8bho";
            source.agentId = "h1sexytest";

            source.userIds = "testJoshua3";
            source.isFilterBalance = 0;
            source.alluser = 0;


            return await _iSEXYapiservice.GetBalance(source);
        }
        /// <summary>
        /// 登入
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("GameLogin")]
        public async Task<DoLoginAndLaunchGameResponse> GameLoginAsync([FromBody] DoLoginAndLaunchGameRequest source)
        {
            source.cert = "RrqUwBase6I9QKH8bho";
            source.agentId = "h1sexytest";

            source.userId = "testJoshua3";
            source.externalURL = "http://ts.bacctest.com/Home/Index";
            source.language = "th";

            BetLimitClass betLimitClass = new BetLimitClass();
            Model.Game.SEXY.SEXY.LIVE lIVE = new LIVE();
            List<int> BetLimit = new List<int>();
            BetLimit.Add(260901);
            BetLimit.Add(260902);
            BetLimit.Add(260903);
            lIVE.limitId = BetLimit;
            Model.Game.SEXY.SEXY.SEXYBCRT sEXYBCRT = new SEXYBCRT();
            sEXYBCRT.LIVE = lIVE;
            betLimitClass.SEXYBCRT = sEXYBCRT;

            source.betLimit =  JsonConvert.SerializeObject(betLimitClass);

            source.platform = "SEXYBCRT";
            source.gameType = "LIVE";
            source.hall = "SEXY";
            source.gameCode = "MX-LIVE-001";
            return await _iSEXYapiservice.DoLoginAndLaunchGame(source);
        }

        /// <summary>
        /// 登出
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("GameLogout")]
        public async Task<GameLogoutResponse> GameLogoutAsync([FromBody] GameLogoutRequest source)
        {
            source.cert = "RrqUwBase6I9QKH8bho";
            source.agentId = "h1sexytest";

            source.userIds = "testJoshua3";

            return await _iSEXYapiservice.GameLogout(source);
        }

        /// <summary>
        /// 存款
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("Deposit")]
        public async Task<DepositResponse> DepositAsync([FromBody] DepositRequest source)
        {
            source.cert = "RrqUwBase6I9QKH8bho";
            source.agentId = "h1sexytest";

            source.userId = "testJoshua3";
            source.transferAmount = 1000;
            source.txCode = Guid.NewGuid().ToString();

            return await _iSEXYapiservice.Deposit(source);
        }
        /// <summary>
        /// 取款
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("Withdraw")]
        public async Task<WithdrawResponse> WithdrawAsync([FromBody] WithdrawRequest source)
        {
            source.cert = "RrqUwBase6I9QKH8bho";
            source.agentId = "h1sexytest";
            source.userId = "testJoshua3";
            source.txCode = Guid.NewGuid().ToString();
            source.withdrawType = "0";
            source.transferAmount = 500;

            return await _iSEXYapiservice.Withdraw(source);
        }
        /// <summary>
        /// 查詢转帐紀錄
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("CheckTransferOperation")]
        public async Task<CheckTransferOperationResponse> CheckTransferOperationAsync([FromBody] CheckTransferOperationRequest source)
        {
            source.cert = "RrqUwBase6I9QKH8bho";
            source.agentId = "h1sexytest";
            source.txCode = "19df5992-ac85-4165-b223-bbd4aee1b8a0";
            return await _iSEXYapiservice.CheckTransferOperation(source);
        }

        /// <summary>
        /// 取得遊戲帳務
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("GetTransactionByUpdateDate")]
        public async Task<GetTransactionByUpdateDateResponse> GetTransactionByUpdateDateAsync([FromBody] GetTransactionByUpdateDateRequest source)
        {
            source.cert = "RrqUwBase6I9QKH8bho";
            source.agentId = "h1sexytest";
            source.timeFrom = new DateTime(2023, 3, 2, 11, 0, 44, DateTimeKind.Local);
            source.platform = "SEXYBCRT";
            source.currency = "THB";

            return await _iSEXYapiservice.GetTransactionByUpdateDate(source);
        }

        /// <summary>
        /// 取得遊戲帳務
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("GetTransactionByTxTime")]
        public async Task<GetTransactionByTxTimeResponse> GetTransactionByTxTimeAsync([FromBody] GetTransactionByTxTimeRequest source)
        {
            source.cert = "RrqUwBase6I9QKH8bho";
            source.agentId = "h1sexytest";
            source.startTime = new DateTime(2023, 2, 22, 13, 0, 0, DateTimeKind.Local);
            source.endTime = new DateTime(2023, 2, 22, 14, 0, 0, DateTimeKind.Local);
            source.platform = "SEXYBCRT";
            //source.currency = "THB";

            return await _iSEXYapiservice.GetTransactionByTxTime(source);
        }

        /// <summary>
        /// 取得遊戲帳務
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("GetSummaryByTxTimeHour")]
        public async Task<GetSummaryByTxTimeHourResponse> GetSummaryByTxTimeHourAsync([FromBody] GetSummaryByTxTimeHourRequest source)
        {
            source.cert = "RrqUwBase6I9QKH8bho";
            source.agentId = "h1sexytest";
            source.startTime = new DateTime(2023, 3, 2, 15, 0, 0, DateTimeKind.Local).ToString("yyyy-MM-ddTHHzzz", CultureInfo.InvariantCulture); 
            source.endTime = new DateTime(2023, 3, 2, 16, 0, 0, DateTimeKind.Local).ToString("yyyy-MM-ddTHHzzz", CultureInfo.InvariantCulture); 
            source.platform = "SEXYBCRT";
            //source.currency = "THB";

            return await _iSEXYapiservice.GetSummaryByTxTimeHour(source);
        }

        /// <summary>
        /// 取得交易历史纪录 遊戲注單明細
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("GetTransactionHistoryResult")]
        public async Task<GetTransactionHistoryResultResponse> GetTransactionHistoryResultAsync([FromBody] GetTransactionHistoryResultRequest source)
        {
            source.cert = "RrqUwBase6I9QKH8bho";
            source.agentId = "h1sexytest";
            source.userId = "testJoshua2";

            source.platform = "SEXYBCRT";
            source.platformTxId = "BAC-255922812";
            source.roundId = "Mexico-02-GA546770056";
            //source.currency = "THB";

            return await _iSEXYapiservice.GetTransactionHistoryResult(source);
        }
        

    }
}
