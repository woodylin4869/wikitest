using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using Microsoft.Extensions.Configuration;
using H1_ThirdPartyWalletAPI.Code;
using H1_ThirdPartyWalletAPI.Model.W1API;
using System.Threading.Tasks;
using H1_ThirdPartyWalletAPI.Model.Game;
using System.Collections.Generic;
using H1_ThirdPartyWalletAPI.Model.DataModel;
using H1_ThirdPartyWalletAPI.Service.Common;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using H1_ThirdPartyWalletAPI.Model;
using Npgsql;
using H1_ThirdPartyWalletAPI.Model.Config;
using H1_ThirdPartyWalletAPI.Service;

namespace H1_ThirdPartyWalletAPI.Controllers.W1API
{
    [ApiController]
    [Route("w1api/[controller]")]
    public class BetRecordSessionController : ControllerBase
    {
        private readonly ILogger<BetRecordSessionController> _logger;
        private readonly ICommonService _commonService;
        public BetRecordSessionController(ILogger<BetRecordSessionController> logger, IRepairBetRecordService repairBetRecordService, ICommonService commonService)
        {
            _logger = logger;
            _commonService = commonService;
        }
        /// <summary>
        /// 取得Session彙總注單
        /// 若沒有帶入Club or Franchiser_id 搜尋條件, 起始時間與結束時間差不得超過1日
        /// 起始時間最高可以搜尋100天前資料
        /// 結束時間必須小於現在時間-5分鐘
        /// </summary>
        [HttpGet]
        async public Task<GetBetRecordSessionRes> Get([FromQuery] GetBetRecordSessionReq summaryRecordReq)
        {
            GetBetRecordSessionRes res = new GetBetRecordSessionRes();
            try
            {
                if (summaryRecordReq.StartTime > summaryRecordReq.EndTime)
                {
                    throw new Exception("起始時間不得大於結束時間");
                }
                if (DateTime.Now.AddDays(-100) > summaryRecordReq.StartTime)
                {
                    throw new Exception("起始時間必須是100天內");
                }
                if (DateTime.Now.AddSeconds(-280) < summaryRecordReq.EndTime)
                {
                    throw new Exception("結束時間必須是5分鐘前");
                }
                if (summaryRecordReq.Club_id == null && summaryRecordReq.Franchiser_id == null)
                {
                    TimeSpan dt = summaryRecordReq.EndTime - summaryRecordReq.StartTime;
                    if (dt.TotalDays > 1)
                    {
                        throw new Exception("無搜尋條件,時間最大為1天");
                    }
                }
                IEnumerable<BetRecordSession> results = await _commonService._serviceDB.GetRecordSession(summaryRecordReq);
                res.code = (int)ResponseCode.Success;
                res.Message = MessageCode.Message[(int)ResponseCode.Success];
                res.Data = results.ToList();
                return res;
            }
            catch (Exception ex)
            {
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                _logger.LogError("Get Bet Record Session EX : {ex}  MSG : {Message} Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
                res.code = (int)ResponseCode.GetSummaryRecordFail;
                res.Message = MessageCode.Message[(int)ResponseCode.GetSummaryRecordFail] + " | " + ex.Message.ToString();
                return res;
            }
        }
        /// <summary>
        /// 取得匯總帳記錄總計
        /// </summary>
        [HttpGet("Summary")]
        public async Task<GetBetRecordSession_SummaryRes> GetSummary([FromQuery] GetBetRecordSession_SummaryReq Req)
        {
            var res = new GetBetRecordSession_SummaryRes();
            try
            {
                IEnumerable<dynamic> result = await _commonService._serviceDB.GetRecordSessionSummary(Req);
                res.Count = (int)result.SingleOrDefault().count;
                return res;
            }
            catch (Exception ex)
            {
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                res.code = (int)ResponseCode.Fail;
                res.Message = MessageCode.Message[(int)ResponseCode.Fail] + "|" + ex.Message;
                _logger.LogError("get Bet Record Session summary exception EX : {ex}  MSG : {Message} Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
                return res;
            }
        }
        /// <summary>
        /// 取得指定ID投注彙總紀錄
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        public async Task<GetBetRecordSessionRes> Get(Guid id)
        {
            GetBetRecordSessionRes res = new GetBetRecordSessionRes();
            try
            {
                var result = await _commonService._serviceDB.GetRecordSessionById(id);
                res.Data = result.ToList();
                return res;
            }
            catch (Exception ex)
            {
                res.code = (int)ResponseCode.Fail;
                res.Message = MessageCode.Message[(int)ResponseCode.Fail] + "|" + ex.Message;
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                _logger.LogError("get Bet Record Summary id exception EX : {ex}  MSG : {Message} Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
                return res;
            }
        }
        /// <summary>
        /// 更新指定ID投注彙總紀錄
        /// </summary>
        /// <param name="id"></param>
        /// <param name="req"></param>
        /// <returns></returns>
        [Authorize(Roles = "admin")]
        [HttpPut("{id}")]
        public async Task<ResCodeBase> Put([FromBody] PutBetRecordSessionReq req, Guid id)
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
                            IEnumerable<BetRecordSession> result = await _commonService._serviceDB.GetRecordSessionByIdLock(conn, tran, id);
                            var recordData = result.Single();
                            recordData.Bet_amount = (req.bet_amount == null) ? recordData.Bet_amount : req.bet_amount.GetValueOrDefault();
                            recordData.Turnover = (req.turnover == null) ? recordData.Turnover : req.turnover.GetValueOrDefault();
                            recordData.Win = (req.win == null) ? recordData.Win : req.win.GetValueOrDefault();
                            recordData.Netwin = (req.netwin == null) ? recordData.Netwin : req.netwin.GetValueOrDefault();
                            recordData.RecordCount = (req.recordcount == null) ? recordData.RecordCount : req.recordcount.GetValueOrDefault();
                            recordData.UpdateDatetime = DateTime.Now;
                            if (await _commonService._serviceDB.PutRecordSession(conn, tran, recordData) != 1)
                            {
                                throw new Exception("update bet record summary fail");
                            }
                            await tran.CommitAsync();
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
                res.code = (int)ResponseCode.Fail;
                res.Message = MessageCode.Message[(int)ResponseCode.Fail] + "|" + ex.Message;
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                _logger.LogError("put bet record session ID : {id} exception EX : {ex}  MSG : {Message} Error Line : {errorFile}.{errorLine}", id, ex.GetType().FullName, ex.Message, errorFile, errorLine);
                return res;
            }
        }
        /// <summary>
        /// 刪除指定ID投注彙總紀錄
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize(Roles = "admin")]
        [HttpDelete("{id}")]
        public async Task<ResCodeBase> Delete(Guid id)
        {
            ResCodeBase res = new ResCodeBase();
            try
            {
                if (await _commonService._serviceDB.DeleteRecordSessionById(id) != 1)
                    throw new Exception("delete bet record summary fail");
                return res;
            }
            catch (Exception ex)
            {
                res.code = (int)ResponseCode.Fail;
                res.Message = MessageCode.Message[(int)ResponseCode.Fail] + "|" + ex.Message;
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                _logger.LogError("delete bet record session ID :{id} exception EX : {ex}  MSG : {Message} Error Line : {errorFile}.{errorLine}",id, ex.GetType().FullName, ex.Message, errorFile, errorLine);
                return res;
            }
        }
    }


}
