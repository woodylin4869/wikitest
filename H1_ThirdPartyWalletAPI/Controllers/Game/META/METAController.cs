using H1_ThirdPartyWalletAPI.Model.Game.META.Request;
using H1_ThirdPartyWalletAPI.Model.Game.META.Response;
using H1_ThirdPartyWalletAPI.Service;
using H1_ThirdPartyWalletAPI.Service.Common;
using H1_ThirdPartyWalletAPI.Service.Game.META;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace H1_ThirdPartyWalletAPI.Controllers.Game.META
{
    /// <summary>
    /// META API
    /// </summary>
    [Route("/[controller]")]
    [ApiController]
    public class METAController : ControllerBase
    {

        private readonly IDBService _dbIdbService;
        private readonly ITransferWalletService _transferWalletService;
        private readonly IMETAApiService _iMETAapiservice;
        public METAController(IDBService dbIdbService, ITransferWalletService transferWalletService, IMETAApiService iMETAapiservice)
        {
            _dbIdbService = dbIdbService;
            _transferWalletService = transferWalletService;
            _iMETAapiservice = iMETAapiservice;
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
            return await _iMETAapiservice.CreateMember(source);
        }

        /// <summary>
        /// 遊戲清單
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("GetGameList")]
        public async Task<GetGameListResponse> GetGameTableListAsync([FromBody] GetGameListRequest source)
        {
            return await _iMETAapiservice.GetGameTableList(source);
        }

        /// <summary>
        /// 查詢會員狀態
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("CheckPoint")]
        public async Task<CheckPointResponse> CheckPoint([FromBody] CheckPointRequest source)
        {
            return await _iMETAapiservice.CheckPoint(source);
        }
        /// <summary>
        /// 額度轉移
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("TransPoint")]
        public async Task<TransPointResponse> TransPoint([FromBody] TransPointRequest source)
        {
            source.TradeOrder = Guid.NewGuid().ToString();
            return await _iMETAapiservice.TransPoint(source);
        }

        /// <summary>
        /// 取得遊戲帳務
        /// </summary>
        [HttpPost]
        [Route("BetOrderRecord")]
        public async Task<BetOrderRecordResponse> BetOrderRecord([FromBody] BetOrderRecordRequest source)
        {
            //var nowDateTime = DateTime.UtcNow;
            //var timestamp = nowDateTime.AddHours(-4).ToString("yyMMd");


            //DateTime dt = DateTime.Now.AddMinutes(-10).AddHours(-3);
            //long _UnixTime = (long)dt.ToUniversalTime().Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds + 1;

            DateTime date = DateTime.Now;
            var dt2 = new DateTimeOffset(date);
            long _UnixTime = dt2.ToUnixTimeSeconds();
            source.Date = _UnixTime;
            //source.Account = null;
            //source.Limit = null;
            //source.LastSerial = 115641;
            //source.Collect = null;
            return await _iMETAapiservice.BetOrderRecord(source);
        }
        /// <summary>
        /// 查詢玩家交易記錄
        /// </summary>
        [HttpPost]
        [Route("TransactionLog")]
        public async Task<TransactionLogResponse> TransactionLog([FromBody] TransactionLogRequest source)
        {
            //var nowDateTime = DateTime.UtcNow;
            //var timestamp = nowDateTime.AddHours(-4).ToString("yyMMd");


            //DateTime dt = DateTime.Now.AddMinutes(-10).AddHours(-3);
            //long _UnixTime = (long)dt.ToUniversalTime().Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds + 1;

            DateTime date = new DateTime(2023, 1, 10, 0, 0, 0, DateTimeKind.Local);
            //DateTime date = DateTime.Now;
            //DateTime date = Convert.ToDateTime("2023-01-11T13:48:03");
            var dt2 = new DateTimeOffset(date);
            long _UnixTime = dt2.ToUnixTimeSeconds();
            source.Date = _UnixTime;
            // "QdNPQ-dev10003";
            //source.Account = "QdNPQ-dev10003";
            //source.Limit = 1;
            //source.TranOrder = null;
            //source.TradeOrder = "ce7e59c5-f1d3-44b6-ae75-774488800d49";
            return await _iMETAapiservice.TransactionLog(source);
        }
    }
}
