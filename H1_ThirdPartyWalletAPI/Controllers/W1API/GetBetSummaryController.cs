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
using H1_ThirdPartyWalletAPI.Service.Common.DB.Betlogs;
using System.Text;

namespace H1_ThirdPartyWalletAPI.Controllers.W1API
{
    [ApiController]
    [Route("w1api")]
    public class GetBetSummaryController : ControllerBase
    {
        private readonly ILogger<GetBetSummaryController> _logger;
        private readonly ICommonService _commonService;
        private readonly IRepairBetRecordService _repairBetRecordService;
        private readonly ISummaryDBService _summaryDBService;
        public GetBetSummaryController(ILogger<GetBetSummaryController> logger, IRepairBetRecordService repairBetRecordService, ICommonService commonService, ISummaryDBService summaryDBService)
        {
            _logger = logger;
            _commonService = commonService;
            _repairBetRecordService = repairBetRecordService;
            _summaryDBService = summaryDBService;
        }
        /// <summary>
        /// 取得5分鐘彙總注單
        /// 每5分鐘注單匯總在第0分鐘, EX 10:10:00~10:14:59 的注單會統計在10:10:00注單
        /// 在每10分鐘結束後過5分鐘,才可以取得該匯總單,  EX 10:10:00~10:14:59 注單最快要10:25:00才可以取得
        /// 若沒有帶入Club or Franchiser_id 搜尋條件, 起始時間與結束時間差不得超過1日
        /// 起始時間最高可以搜尋100天前資料
        /// 結束時間必須小於現在時間-5分鐘
        /// </summary>
        [HttpGet("GetBetSummary")]
        async public Task<GetBetSummary> Get([FromQuery] GetBetSummaryReq summaryRecordReq)
        {
            GetBetSummary res = new GetBetSummary();
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
                var timeinterval = 280;
                if (Config.OneWalletAPI.RCGMode == "H1" && summaryRecordReq.game_id == nameof(Platform.RSG))
                {
                    timeinterval = 20;
                }
                if (DateTime.Now.AddSeconds(-timeinterval) < summaryRecordReq.EndTime)
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
                IEnumerable<BetRecordSummary> results = await _summaryDBService.GetRecordSummary(summaryRecordReq);
                res.code = (int)ResponseCode.Success;
                res.Message = MessageCode.Message[(int)ResponseCode.Success];
                res.Data = results.ToList();
                if (Config.OneWalletAPI.RCGMode == "H1")
                {
                    if (summaryRecordReq.game_id == null)
                    {
                        res.Data = res.Data.Where(
                            x => x.Game_id != nameof(Platform.RSG) &&
                            x.Game_id != nameof(Platform.RCG)
                            ).ToList();
                    }
                }
                return res;
            }
            catch (Exception ex)
            {
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                _logger.LogError("Get Summary Record EX : {ex}  MSG : {Message} Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
                res.code = (int)ResponseCode.GetSummaryRecordFail;
                res.Message = MessageCode.Message[(int)ResponseCode.GetSummaryRecordFail] + " | " + ex.Message.ToString();
                return res;
            }
        }
        /// <summary>
        /// 取得5分鐘彙總注單
        /// 每5分鐘注單匯總在第0分鐘, EX 10:10:00~10:14:59 的注單會統計在10:10:00注單
        /// 在每10分鐘結束後過5分鐘,才可以取得該匯總單,  EX 10:10:00~10:14:59 注單最快要10:25:00才可以取得
        /// 若沒有帶入Club or Franchiser_id 搜尋條件, 起始時間與結束時間差不得超過1日
        /// 起始時間最高可以搜尋100天前資料
        /// 結束時間必須小於現在時間-5分鐘
        /// </summary>
        [HttpGet("GetBetSummaryGzip")]
        public async Task GetBetSummaryGzip([FromQuery] GetBetSummaryReq summaryRecordReq)
        {
            GetBetSummary res = new GetBetSummary();
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
                var timeinterval = 280;
                if (Config.OneWalletAPI.RCGMode == "H1" && summaryRecordReq.game_id == nameof(Platform.RSG))
                {
                    timeinterval = 20;
                }
                if (DateTime.Now.AddSeconds(-timeinterval) < summaryRecordReq.EndTime)
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

                IEnumerable<BetRecordSummary> results = await _summaryDBService.GetRecordSummary(summaryRecordReq);
                res.code = (int)ResponseCode.Success;
                res.Message = MessageCode.Message[(int)ResponseCode.Success];
                res.Data = results.ToList();
                if (Config.OneWalletAPI.RCGMode == "H1")
                {
                    if (summaryRecordReq.game_id == null)
                    {
                        res.Data = res.Data.Where(
                            x => x.Game_id != nameof(Platform.RSG) &&
                            x.Game_id != nameof(Platform.RCG)
                            ).ToList();
                    }
                }

                var responseJson = System.Text.Json.JsonSerializer.Serialize(res);
                var bytes = Encoding.UTF8.GetBytes(responseJson);
                var compress = Helpers.GzipHelper.Compress(bytes);
                await Response.Body.WriteAsync(compress);
            }
            catch (Exception ex)
            {
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                _logger.LogError("Get Summary Record EX : {ex}  MSG : {Message} Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
                res.code = (int)ResponseCode.GetSummaryRecordFail;
                res.Message = MessageCode.Message[(int)ResponseCode.GetSummaryRecordFail] + " | " + ex.Message.ToString();

                var responseJson = System.Text.Json.JsonSerializer.Serialize(res);
                var bytes = Encoding.UTF8.GetBytes(responseJson);
                var compress = Helpers.GzipHelper.Compress(bytes);
                await Response.Body.WriteAsync(compress);
            }
        }
        /// <summary>
        /// 重新取得並更新時間區段遊戲商彙總注單
        /// 起始時間最高可以修復30天前資料
        /// 結束時間必須小於現在時間15分鐘
        /// </summary>
        [HttpPut("RepairBetSummary")]
        async public Task<ResCodeBase> Repair([FromQuery] RepairBetSummaryReq summaryRecordReq)
        {
            ResCodeBase res = new ResCodeBase();
            try
            {
                if (summaryRecordReq.StartTime > summaryRecordReq.EndTime)
                {
                    throw new Exception("起始時間不得大於結束時間");
                }
                if (DateTime.Now.AddDays(-30) > summaryRecordReq.StartTime && summaryRecordReq.game_id.ToLower() != nameof(Platform.SABA2).ToLower())
                {
                    throw new Exception("起始時間必須是30天內");
                }
                if (DateTime.Now.AddSeconds(-900) < summaryRecordReq.EndTime)
                {
                    throw new Exception("結束時間必須是15分鐘前");
                }
                TimeSpan dt = summaryRecordReq.EndTime - summaryRecordReq.StartTime;
                if (dt.TotalDays > 1
                    && summaryRecordReq.game_id.ToLower() != nameof(Platform.SABA2).ToLower()
                    && summaryRecordReq.game_id.ToLower() != nameof(Platform.RCG2).ToLower()
                    && summaryRecordReq.game_id.ToLower() != nameof(Platform.BTI).ToLower()
                    )
                {
                    throw new Exception("時間最大為1天");
                }
                return await _repairBetRecordService.RepairGameRecord(summaryRecordReq, true);
            }
            catch (Exception ex)
            {
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                _logger.LogError("Put Summary Record EX : {ex}  MSG : {Message} Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
                res.code = (int)ResponseCode.GetSummaryRecordFail;
                res.Message = MessageCode.Message[(int)ResponseCode.GetSummaryRecordFail] + " | " + ex.Message.ToString();
                return res;
            }
        }
        /// <summary>
        /// 補單執行紀錄
        /// </summary>
        [HttpPut("RepairBetLog")]
        async public Task<RepairLogByPageResp> RepairLog([FromQuery] RepairLogByPageReq repairLogByPageReq)
        {
            RepairLogByPageResp res = new RepairLogByPageResp();
            try
            {
                return await _repairBetRecordService.GetRepairLog(repairLogByPageReq);

            }
            catch (Exception ex)
            {
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                _logger.LogError("RepairBetLog EX : {ex}  MSG : {Message} Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
                res.code = (int)ResponseCode.GetSummaryRecordFail;
                res.Message = MessageCode.Message[(int)ResponseCode.GetSummaryRecordFail] + " | " + ex.Message.ToString();
                return res;
            }
        }


        /// <summary>
        /// 取得匯總帳記錄總計
        /// </summary>
        /// <summary>
        /// 取得匯總帳記錄總計
        /// </summary>
        [HttpGet("BetRecordSummary/Summary")]
        public async Task<GetBetSummary_SummaryRes> GetSummary([FromQuery] GetBetSummary_SummaryReq Req)
        {
            GetBetSummary_SummaryRes res = new GetBetSummary_SummaryRes();
            try
            {
                IEnumerable<dynamic> result = await _summaryDBService.GetBetRecordSummary(Req);
                res.Count = (int)result.SingleOrDefault().count;
                return res;
            }
            catch (Exception ex)
            {
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                res.code = (int)ResponseCode.Fail;
                res.Message = MessageCode.Message[(int)ResponseCode.Fail] + "|" + ex.Message;
                _logger.LogError("get Bet Record summary exception EX : {ex}  MSG : {Message} Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
                return res;
            }
        }
        /// <summary>
        /// 取得指定ID投注彙總紀錄
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("BetRecordSummary/{id}")]
        public async Task<GetBetSummary> Get(Guid id)
        {
            GetBetSummary res = new GetBetSummary();
            try
            {
                IEnumerable<BetRecordSummary> result = await _summaryDBService.GetBetRecordSummaryById(id);
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
        [HttpPut("BetRecordSummary/{id}")]
        public async Task<ResCodeBase> Put([FromBody] PutBetSummaryReq req, Guid id)
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
                            IEnumerable<BetRecordSummary> result = await _summaryDBService.GetBetRecordSummaryByIdLock(conn, tran, id);
                            var recordData = result.Single();
                            recordData.Bet_amount = (req.bet_amount == null) ? recordData.Bet_amount : req.bet_amount.GetValueOrDefault();
                            recordData.Turnover = (req.turnover == null) ? recordData.Turnover : req.turnover.GetValueOrDefault();
                            recordData.Win = (req.win == null) ? recordData.Win : req.win.GetValueOrDefault();
                            recordData.Netwin = (req.netwin == null) ? recordData.Netwin : req.netwin.GetValueOrDefault();
                            recordData.RecordCount = (req.recordcount == null) ? recordData.RecordCount : req.recordcount.GetValueOrDefault();
                            recordData.updatedatetime = DateTime.Now;
                            if (await _summaryDBService.PutBetRecordSummary(conn, tran, recordData) != 1)
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
                _logger.LogError("put bet record summary ID : {id} exception EX : {ex}  MSG : {Message} Error Line : {errorFile}.{errorLine}", id, ex.GetType().FullName, ex.Message, errorFile, errorLine);
                return res;
            }
        }
        /// <summary>
        /// 刪除指定ID投注彙總紀錄
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        //[Authorize(Roles = "admin")]
        [HttpDelete("BetRecordSummary/{id}")]
        public async Task<ResCodeBase> Delete(Guid id)
        {
            ResCodeBase res = new ResCodeBase();
            try
            {
                if (await _summaryDBService.DeleteBetRecordSummaryById(id) != 1)
                    throw new Exception("delete bet record summary fail");
                return res;
            }
            catch (Exception ex)
            {
                res.code = (int)ResponseCode.Fail;
                res.Message = MessageCode.Message[(int)ResponseCode.Fail] + "|" + ex.Message;
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                _logger.LogError("delete bet record summary ID :{id} exception EX : {ex}  MSG : {Message} Error Line : {errorFile}.{errorLine}", id, ex.GetType().FullName, ex.Message, errorFile, errorLine);
                return res;
            }
        }
    }


}
