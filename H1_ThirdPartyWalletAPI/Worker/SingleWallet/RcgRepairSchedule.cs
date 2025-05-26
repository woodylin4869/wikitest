using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Coravel.Invocable;
using Microsoft.Extensions.Logging;
using System.Linq;
using Npgsql;
using H1_ThirdPartyWalletAPI.Model.Config;
using H1_ThirdPartyWalletAPI.Service.Common;
using H1_ThirdPartyWalletAPI.Model.DataModel;
using H1_ThirdPartyWalletAPI.Code;

namespace H1_ThirdPartyWalletAPI.Worker
{
    public class RcgRepairSchedule : IInvocable
    {
        private readonly ILogger<RcgRepairSchedule> _logger;
        private readonly IDBService _serviceDB;
        public RcgRepairSchedule(ILogger<RcgRepairSchedule> logger, IDBService serviceDB)
        {
            _logger = logger;
            _serviceDB = serviceDB;
        }
        public async Task Invoke()
        {
            _logger.LogInformation("Invoke RcgRepairSchedule on time : {time}", DateTime.Now);
            try
            {
                DateTime TimeNow = DateTime.Now.AddMinutes(-30);
                DateTime EndTime = new DateTime(TimeNow.Year, TimeNow.Month, TimeNow.Day, TimeNow.Hour, TimeNow.Minute / 5 * 5, 0);
                DateTime StartTime = EndTime.AddDays(-3);
                IEnumerable<RcgWalletTransaction> result = await _serviceDB.GetRcgTransaction(StartTime, EndTime);
                if (result.Count() == 0) //未找到漏單直接離開
                {
                    await Task.CompletedTask;
                    return;
                }
                //一次處理一筆漏單
                RcgWalletTransaction TranData = result.FirstOrDefault();
                StartTime = new DateTime(TranData.create_datetime.Year, TranData.create_datetime.Month, TranData.create_datetime.Day, TranData.create_datetime.Hour, TranData.create_datetime.Minute / 5 * 5, 0);
                EndTime = StartTime.AddMinutes(5);
                //取得該會員5分鐘所有注單
                IEnumerable<RcgWalletTransaction> RepairResult = await _serviceDB.GetRcgMemberTransaction(TranData.club_id, StartTime, EndTime);

                WalletTransferRecord recordDate = new WalletTransferRecord();
                recordDate.id = Guid.NewGuid();
                recordDate.create_datetime = StartTime;
                recordDate.success_datetime = EndTime;
                recordDate.status = WalletTransferRecord.TransferStatus.success.ToString();
                recordDate.before_balance = RepairResult.FirstOrDefault().before_balance;
                recordDate.after_balance = RepairResult.LastOrDefault().after_balance;
                foreach (RcgWalletTransaction r in RepairResult)//loop club id bet detail
                {
                    recordDate.Club_id = r.club_id;
                    recordDate.Franchiser_id = r.franchiser_id;

                    if (r.tran_type.ToLower() == "debit")
                    {
                        recordDate.amount -= r.amount;
                    }
                    else if (r.tran_type.ToLower() == "credit" || r.tran_type.ToLower() == "cancel")
                    {
                        recordDate.amount += r.amount;
                    }
                    else
                    {
                        throw new Exception("unknow type");
                    }

                    r.summary_id = recordDate.id;
                }
                if (recordDate.amount >= 0)
                {
                    recordDate.source = nameof(Platform.RCG);
                    recordDate.target = nameof(Platform.W1);
                }
                else
                {
                    recordDate.source = nameof(Platform.W1);
                    recordDate.target = nameof(Platform.RCG);
                }
                recordDate.amount = Math.Abs(recordDate.amount);
                recordDate.type = nameof(WalletTransferRecord.TransferType.RCG);

                using (NpgsqlConnection conn = new NpgsqlConnection(Config.OneWalletAPI.DBConnection.Wallet.Master))
                {
                    await conn.OpenAsync();
                    using (var tran = conn.BeginTransaction())
                    {
                        try
                        {
                            await _serviceDB.DeleteTransferRecord(conn, tran, recordDate);
                            if (await _serviceDB.PostTransferRecord(conn, tran, recordDate) != 1)
                            {
                                throw new Exception("PostTransferRecord fail");
                            }
                            foreach (RcgWalletTransaction r in RepairResult)
                            {
                                if (await _serviceDB.PutRcgTransaction(conn, tran, r) != 1)
                                {
                                    throw new Exception("PutRcgTransaction fail");
                                }
                            }
                            await tran.CommitAsync();
                        }
                        catch (Exception ex)
                        {
                            var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                            var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                            _logger.LogError("repair user  record schedule exception EX : {ex}  MSG : {Message} Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
                            await tran.RollbackAsync();
                        }
                    }
                    await conn.CloseAsync();
                }
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                _logger.LogError("Run rcg repair schedule exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
            }
        }
    }
}
