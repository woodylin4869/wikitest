using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using H1_ThirdPartyWalletAPI.Code;
using H1_ThirdPartyWalletAPI.Model.W1API;
using System.Threading.Tasks;
using System.Collections.Generic;
using H1_ThirdPartyWalletAPI.Model.DataModel;
using H1_ThirdPartyWalletAPI.Service.Common;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using H1_ThirdPartyWalletAPI.Model;
using Npgsql;
using H1_ThirdPartyWalletAPI.Model.Config;
using static H1_ThirdPartyWalletAPI.Model.DataModel.WalletTransferRecord;
using H1_ThirdPartyWalletAPI.Service;
using Microsoft.AspNetCore.Http;

namespace H1_ThirdPartyWalletAPI.Controllers.W1API
{
    [ApiController]
    [Route("w1api")]
    public class GetTransactionSummaryController : ControllerBase
    {
        private readonly ILogger<GetTransactionSummaryController> _logger;
        private readonly ICommonService _commonService;
        private readonly ITransferService _transferService;

        public GetTransactionSummaryController(ILogger<GetTransactionSummaryController> logger
            , ICommonService commonService
            , ITransferService transferService)
        {
            _logger = logger;
            _commonService = commonService;
            _transferService = transferService;
        }
        /// <summary>
        /// 取得轉帳記錄
        /// 若沒有帶入Club or Franchiser_id 搜尋條件, 起始時間與結束時間差不得超過1日
        /// 起始時間最高可以搜尋100天前資料
        /// </summary>
        [Route("[controller]")]
        [HttpGet]
        async public Task<GetTransactionSummary> Get([FromQuery] GetTransactionSummaryReq transerRecordReq)
        {
            GetTransactionSummary res = new GetTransactionSummary();
            try
            {
                if (transerRecordReq.StartTime > transerRecordReq.EndTime)
                {
                    throw new Exception("起始時間不得大於結束時間");
                }
                if (DateTime.Now.AddDays(-100) > transerRecordReq.StartTime)
                {
                    throw new Exception("起始時間必須是100天內");
                }
                if (transerRecordReq.Club_id == null && transerRecordReq.Franchiser_id == null)
                {
                    TimeSpan dt = transerRecordReq.EndTime - transerRecordReq.StartTime;
                    if (dt.TotalDays > 1)
                    {
                        throw new Exception("無搜尋條件,時間最大為1天");
                    }
                }
                //IEnumerable<WalletTransferRecord> results = await _serviceDB.GetTransferRecord(transerRecordReq);
                IEnumerable<WalletTransferRecord> results = await _commonService._serviceDB.GetTransferRecord(transerRecordReq);
                res.code = (int)ResponseCode.Success;
                res.Message = MessageCode.Message[(int)ResponseCode.Success];
                res.Data = results.ToList();
                return res;
            }
            catch (Exception ex)
            {
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                _logger.LogError("Get Transfer Record EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
                res.code = (int)ResponseCode.GetSummaryRecordFail;
                res.Message = MessageCode.Message[(int)ResponseCode.GetSummaryRecordFail] + " | " + ex.Message.ToString();
                return res;
            }
        }

        /// <summary>
        /// 取得轉帳記錄(分頁)
        /// 若沒有帶入Club or Franchiser_id 搜尋條件, 起始時間與結束時間差不得超過1日
        /// 起始時間最高可以搜尋100天前資料
        /// </summary>
        [HttpGet("TransactionSummary/Page")]
        public async Task<GetTransactionSummaryByPageResp> GetByPage([FromQuery] GetTransactionSummaryByPageReq transferRecordReq)
        {
            GetTransactionSummaryByPageResp res = new GetTransactionSummaryByPageResp();
            try
            {
                if (transferRecordReq.StartTime > transferRecordReq.EndTime)
                {
                    throw new Exception("起始時間不得大於結束時間");
                }
                if (DateTime.Now.AddDays(-100) > transferRecordReq.StartTime)
                {
                    throw new Exception("起始時間必須是100天內");
                }
                if (transferRecordReq.Club_id == null && transferRecordReq.Franchiser_id == null)
                {
                    TimeSpan dt = transferRecordReq.EndTime - transferRecordReq.StartTime;
                    if (dt.TotalDays > 1)
                    {
                        throw new Exception("無搜尋條件,時間最大為1天");
                    }
                }
                //IEnumerable<WalletTransferRecord> results = await _serviceDB.GetTransferRecord(transferRecordReq);
                var results = await _commonService._serviceDB.GetTransferRecordByPage(transferRecordReq);
                res.code = (int)ResponseCode.Success;
                res.Message = MessageCode.Message[(int)ResponseCode.Success];
                res.Data = results.ResultData?.ToList() ?? new List<WalletTransferRecord>();
                res.TotalCount = results.ResultCount;
                return res;
            }
            catch (Exception ex)
            {
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                _logger.LogError("Get Transfer Record EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
                res.code = (int)ResponseCode.GetSummaryRecordFail;
                res.Message = MessageCode.Message[(int)ResponseCode.GetSummaryRecordFail] + " | " + ex.Message.ToString();
                return res;
            }
        }

        /// <summary>
        /// 取得轉帳記錄總計
        /// </summary>
        [HttpGet("TransactionSummary/Summary")]
        public async Task<GetTransactionSummary_SummaryRes> GetSummary([FromQuery] GetTransactionSummary_SummaryReq Req)
        {
            GetTransactionSummary_SummaryRes res = new GetTransactionSummary_SummaryRes();
            try
            {
                IEnumerable<dynamic> result = await _commonService._serviceDB.GetWalletTransferRecordSummary(Req);
                res.Count = (int)result.SingleOrDefault().count;
                return res;
            }
            catch (Exception ex)
            {
                res.code = (int)ResponseCode.Fail;
                res.Message = MessageCode.Message[(int)ResponseCode.Fail] + "|" + ex.Message;
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                _logger.LogError("get Transfer Record summary exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
                return res;
            }
        }

        /// <summary>
        /// 取得指定ID轉帳紀錄
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("TransactionSummary/{id}")]
        public async Task<GetTransactionSummary> Get(Guid id)
        {
            GetTransactionSummary res = new GetTransactionSummary();
            try
            {
                var result = await _commonService._serviceDB.GetTransferRecordById(id);

                if (result.status == TransferStatus.pending.ToString() || result.status == TransferStatus.init.ToString())
                {
                    await _transferService.CheckSingleTransferRecord(result);
                }
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
                _logger.LogError("get TransactionSummary id exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
                return res;
            }
        }

        /// <summary>
        /// 更新指定ID轉帳記錄
        /// </summary>
        /// <param name="id"></param>
        /// <param name="req"></param>
        /// <returns></returns>
        [Authorize(Roles = "admin")]
        [HttpPut("TransactionSummary/{id}")]
        public async Task<ResCodeBase> Put([FromBody] PutTransactionSummaryReq req, Guid id, DateTime create_datetime)
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
                            var transferData = await _commonService._serviceDB.GetTransferRecordByIdLock(conn, tran, id, create_datetime);
                            transferData.after_balance = (req.after_balance == null) ? transferData.after_balance : req.after_balance.GetValueOrDefault();
                            transferData.amount = (req.amount == null) ? transferData.amount : req.amount.GetValueOrDefault();
                            transferData.before_balance = (req.before_balance == null) ? transferData.before_balance : req.before_balance.GetValueOrDefault();
                            transferData.source = (req.source == null) ? transferData.source : req.source;
                            transferData.status = (req.status == null) ? transferData.status : req.status;
                            transferData.type = (req.type == null) ? transferData.type : req.type;
                            transferData.target = (req.target == null) ? transferData.target : req.target;
                            if (await _commonService._serviceDB.PutTransferRecord(conn, tran, transferData) != 1)
                            {
                                throw new Exception("update transfer record fail");
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
                _logger.LogError("put transaction summary ID : {id} exception EX : {ex}  MSG : {Message} ", id, ex.GetType().FullName.ToString(), ex.Message.ToString());
                return res;
            }
        }

        /// <summary>
        /// 刪除指定ID轉帳記錄
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize(Roles = "admin")]
        [HttpDelete("TransactionSummary/{id}")]
        public async Task<ResCodeBase> Delete(Guid id)
        {
            ResCodeBase res = new ResCodeBase();
            try
            {
                if (await _commonService._serviceDB.DeleteTransferRecordById(id) != 1)
                    throw new Exception("delete transfer record fail");
                return res;
            }
            catch (Exception ex)
            {
                res.code = (int)ResponseCode.Fail;
                res.Message = MessageCode.Message[(int)ResponseCode.Fail] + "|" + ex.Message;
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                _logger.LogError("delete transaction summary ID :{id} exception EX : {ex}  MSG : {Message} ", id, ex.GetType().FullName.ToString(), ex.Message.ToString());
                return res;
            }
        }


        /// <summary>
        /// 使用Club_id查詢會員的電子最後進入館別交易紀錄
        /// </summary>
        /// <param name="Club_id"></param>
        /// <returns></returns>
        [HttpGet("TransactionSummary/GetElectronicDepositRecord")]
        async public Task<dynamic> GetElectronicDepositRecord([FromQuery] string Club_id)
        {
            GetElectronicDepositRecordResponse res = new GetElectronicDepositRecordResponse();
            try
            {
                res.code = (int)ResponseCode.Success;
                res.Message = MessageCode.Message[(int)ResponseCode.Success];
                res.Data = await _transferService.GetElectronicDepositRecordCache(Club_id);
                return res;
            }
            catch (Exception ex)
            {
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                _logger.LogError("GetElectronicDepositRecord EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
                res.code = (int)ResponseCode.Fail;
                res.Message = MessageCode.Message[(int)ResponseCode.Fail] + " | " + ex.Message.ToString();
                return res;
            }
        }

        /// <summary>
        /// 刪除電子遊戲轉入紀錄快取
        /// </summary>
        /// <param name="Club_id"></param>
        /// <returns></returns>
        [HttpGet("TransactionSummary/DeleteElectronicDepositRecordCache")]
        public Task<ResCodeBase> DeleteElectronicDepositRecordCache([FromQuery] string Club_id)
        {
            return _transferService.DeleteElectronicDepositRecordCache(Club_id);
        }
    }
}
