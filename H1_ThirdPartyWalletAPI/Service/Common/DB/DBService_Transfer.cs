using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Dapper;
using Npgsql;
using H1_ThirdPartyWalletAPI.Model.DataModel;
using H1_ThirdPartyWalletAPI.Model.Config;
using System.Data;
using System.Linq;
using H1_ThirdPartyWalletAPI.Model.W1API;
using H1_ThirdPartyWalletAPI.Code;
using H1_ThirdPartyWalletAPI.Model;
using System.Diagnostics.Metrics;
using H1_ThirdPartyWalletAPI.Utility;

namespace H1_ThirdPartyWalletAPI.Service.Common
{
    public partial interface IDBService
    {
        Task<int> PostTransferRecord(NpgsqlConnection conn, IDbTransaction tran, WalletTransferRecord transferRecord);
        Task<int> PostTransferRecord(WalletTransferRecord transferRecord);
        Task<int> PutTransferRecord(NpgsqlConnection conn, IDbTransaction tran, WalletTransferRecord transferRecord);
        Task<int> DeleteTransferRecord(NpgsqlConnection conn, IDbTransaction tran, WalletTransferRecord transferRecord);
        Task<IEnumerable<WalletTransferRecord>> GetTransferRecord(WalletTransferRecord transferRecord, DateTime startTime, DateTime endTime);
        Task<IEnumerable<WalletTransferRecord>> GetTransferRecord(GetTransactionSummaryReq SummaryRecordReq);

        Task<(int ResultCount, IEnumerable<WalletTransferRecord> ResultData)> GetTransferRecordByPage(GetTransactionSummaryByPageReq summaryByPageReq);
        Task<IEnumerable<WalletTransferRecord>> GetSessionTransferRecord(GetTransactionSummaryReq SummaryRecordReq);
        Task<WalletTransferRecord> GetTransferRecordById(Guid id);
        Task<WalletTransferRecord> GetTransferRecordByIdLock(NpgsqlConnection conn, IDbTransaction tran, Guid id, DateTime create_datetime);
        Task<IEnumerable<WalletTransferRecord>> GetTransferRecordBySession(string club_id);
        Task<dynamic> GetWalletTransferRecordSummary(GetTransactionSummary_SummaryReq req);
        Task<int> DeleteTransferRecordById(Guid id);
    }
    public partial class DBService : IDBService
    {
        #region t_wallet_transfer_record
        public async Task<int> PostTransferRecord(NpgsqlConnection conn, IDbTransaction tran, WalletTransferRecord transferRecord)
        {
            string strSql = @"INSERT INTO t_wallet_transfer_record
                            (
                                id,
                                source,
                                target,
                                create_datetime,
                                success_datetime,
                                before_balance,
                                after_balance,
                                status,
                                club_id,
                                amount,
                                franchiser_id,
                                type
                            )
                            VALUES
                            (
                                @id,
                                @source,
                                @target,
                                @create_datetime,
                                @success_datetime,
                                @before_balance,
                                @after_balance,
                                @status,
                                @club_id,
                                @amount,
                                @franchiser_id,
                                @type
                            )";
            return await conn.ExecuteAsync(strSql, transferRecord, tran);
        }
        public async Task<int> PostTransferRecord(WalletTransferRecord transferRecord)
        {
            string strSql = @"INSERT INTO t_wallet_transfer_record
                            (
                                id,
                                source,
                                target,
                                create_datetime,
                                success_datetime,
                                before_balance,
                                after_balance,
                                status,
                                club_id,
                                amount,
                                franchiser_id,
                                type
                            )
                            VALUES
                            (
                                @id,
                                @source,
                                @target,
                                @create_datetime,
                                @success_datetime,
                                @before_balance,
                                @after_balance,
                                @status,
                                @club_id,
                                @amount,
                                @franchiser_id,
                                @type
                            )";
            using (var conn = new NpgsqlConnection(PGMaster))
            {
                return await conn.ExecuteAsync(strSql, transferRecord);
            }
        }
        public async Task<int> PutTransferRecord(NpgsqlConnection conn, IDbTransaction tran, WalletTransferRecord transferRecord)
        {
            var par = new DynamicParameters();
            string strSql = @"UPDATE t_wallet_transfer_record
                                    SET success_datetime = @success_datetime,
                                    status = @status,
                                    before_balance = @before_balance,
                                    after_balance = @after_balance,
                                    type = @type,
                                    source = @source,
                                    target = @target,
                                    amount = @amount
                                    WHERE id = @id
                                    AND create_datetime = @create_datetime";

            par.Add("@success_datetime", transferRecord.success_datetime);
            par.Add("@status", transferRecord.status);
            par.Add("@before_balance", transferRecord.before_balance);
            par.Add("@after_balance", transferRecord.after_balance);
            par.Add("@type", transferRecord.type);
            par.Add("@source", transferRecord.source);
            par.Add("@target", transferRecord.target);
            par.Add("@amount", transferRecord.amount);
            par.Add("@id", transferRecord.id);
            par.Add("@create_datetime", transferRecord.create_datetime);
            return await conn.ExecuteAsync(strSql, par, tran);
        }
        public async Task<int> DeleteTransferRecord(NpgsqlConnection conn, IDbTransaction tran, WalletTransferRecord transferRecord)
        {
            var par = new DynamicParameters();
            string strSql = @"DELETE FROM t_wallet_transfer_record
                               WHERE club_id = @club_id
                               AND create_datetime = @create_datetime
                               AND success_datetime = @success_datetime";
            par.Add("@club_id", transferRecord.Club_id);
            par.Add("@create_datetime", transferRecord.create_datetime);
            par.Add("@success_datetime", transferRecord.success_datetime);

            return await conn.ExecuteAsync(strSql, par, tran);
        }
        public async Task<IEnumerable<WalletTransferRecord>> GetTransferRecord(WalletTransferRecord transferRecord, DateTime startTime, DateTime endTime)
        {
            var par = new DynamicParameters();
            string strSql = @"SELECT 
                                    id
	                                ,source
	                                ,target
	                                ,create_datetime
	                                ,success_datetime
	                                ,before_balance
	                                ,after_balance
	                                ,STATUS
	                                ,club_id
	                                ,amount
	                                ,franchiser_id
	                                ,type
                            FROM t_wallet_transfer_record
                            WHERE create_datetime BETWEEN @startTime and @endTime";
            if (transferRecord.status != null)
            {
                strSql += " AND status = @status";
                par.Add("@status", transferRecord.status);
            }
            //限制1000筆，避免爆量
            strSql += " ORDER BY create_datetime LIMIT 1000";
            par.Add("@startTime", startTime);
            par.Add("@endTime", endTime);
            using (NpgsqlConnection conn = new NpgsqlConnection(await PGRead))
            {
                return await conn.QueryAsync<WalletTransferRecord>(strSql, par);
            }
        }
        public async Task<IEnumerable<WalletTransferRecord>> GetTransferRecord(GetTransactionSummaryReq SummaryRecordReq)
        {
            var par = new DynamicParameters();
            string strSql = @"SELECT 
                                    id
	                                ,source
	                                ,target
	                                ,create_datetime
	                                ,success_datetime
	                                ,before_balance
	                                ,after_balance
	                                ,STATUS
	                                ,club_id
	                                ,amount
	                                ,franchiser_id
	                                ,type
                    FROM t_wallet_transfer_record
                    WHERE create_datetime BETWEEN @StartTime AND @EndTime
                    ";
            if (SummaryRecordReq.Club_id != null)
            {
                par.Add("@Club_id", SummaryRecordReq.Club_id);
                strSql += " AND Club_id = @Club_id";
            }
            if (SummaryRecordReq.Franchiser_id != null)
            {
                par.Add("@Franchiser_id", SummaryRecordReq.Franchiser_id);
                strSql += " AND Franchiser_id = @Franchiser_id";
            }
            if (SummaryRecordReq.status != null)
            {
                par.Add("@status", SummaryRecordReq.status.ToLower());
                strSql += " AND status = @status";
            }
            if (SummaryRecordReq.type != null)
            {
                par.Add("@type", SummaryRecordReq.type.ToUpper());
                strSql += " AND type = @type";
            }
            if (SummaryRecordReq.Page != null && SummaryRecordReq.Count != null)
            {
                strSql += @" OFFSET @offset
                        LIMIT @limit";
                par.Add("@offset", SummaryRecordReq.Page * SummaryRecordReq.Count);
                par.Add("@limit", SummaryRecordReq.Count);
            }
            par.Add("@StartTime", SummaryRecordReq.StartTime);
            par.Add("@EndTime", SummaryRecordReq.EndTime);


            using (NpgsqlConnection conn = new NpgsqlConnection(await PGRead))
            {
                return await conn.QueryAsync<WalletTransferRecord>(strSql, par);
            }

        }

        /// <summary>
        /// 取得轉帳紀錄(分頁)
        /// </summary>
        /// <param name="summaryByPageReq"></param>
        /// <returns></returns>
        public async Task<(int ResultCount, IEnumerable<WalletTransferRecord> ResultData)> GetTransferRecordByPage(GetTransactionSummaryByPageReq summaryByPageReq)
        {
            var par = new DynamicParameters();
            string strSql = @"SELECT id
	                                ,source
	                                ,target
	                                ,create_datetime
	                                ,success_datetime
	                                ,before_balance
	                                ,after_balance
	                                ,STATUS
	                                ,club_id
	                                ,amount
	                                ,franchiser_id
	                                ,type
                    FROM t_wallet_transfer_record
                    WHERE create_datetime BETWEEN @StartTime AND @EndTime
                    ";
            // 統計筆數sql
            string strSqlCount = "";
            if (summaryByPageReq.Club_id != null)
            {
                par.Add("@Club_id", summaryByPageReq.Club_id);
                strSql += " AND Club_id = @Club_id";
            }
            if (summaryByPageReq.Franchiser_id != null)
            {
                par.Add("@Franchiser_id", summaryByPageReq.Franchiser_id);
                strSql += " AND Franchiser_id = @Franchiser_id";
            }
            if (summaryByPageReq.status != null)
            {
                par.Add("@status", summaryByPageReq.status.ToLower());
                strSql += " AND status = @status";
            }
            if (summaryByPageReq.type != null)
            {
                par.Add("@type", summaryByPageReq.type.ToUpper());
                strSql += " AND type = @type";
            }
            strSqlCount = $"select count(1) from  ({strSql.ToString()}) formatTable";

            // 加入order條件防止分頁錯亂
            string sort = SortUtility.SortWithIncrement("create_datetime", SortType.Desc, summaryByPageReq.SortColumnName, summaryByPageReq.SortType);
            strSql += sort;

            strSql += @" OFFSET @offset
                    LIMIT @limit";
            par.Add("@offset", summaryByPageReq.Page * summaryByPageReq.Count);
            par.Add("@limit", summaryByPageReq.Count);


            par.Add("@StartTime", summaryByPageReq.StartTime);
            par.Add("@EndTime", summaryByPageReq.EndTime);


            using (NpgsqlConnection conn = new NpgsqlConnection(await PGRead))
            {
                string combineSQL = $"{strSqlCount.ToString()};{strSql.ToString()}";
                var result = await conn.QueryMultipleAsync(combineSQL, par);
                var count = result.Read<int>().FirstOrDefault();
                var data = result.Read<WalletTransferRecord>();
                return (count, data);
            }

        }

        public async Task<IEnumerable<WalletTransferRecord>> GetSessionTransferRecord(GetTransactionSummaryReq SummaryRecordReq)
        {
            var par = new DynamicParameters();
            string strSql = @"SELECT 
                                    id
	                                ,source
	                                ,target
	                                ,create_datetime
	                                ,success_datetime
	                                ,before_balance
	                                ,after_balance
	                                ,STATUS
	                                ,club_id
	                                ,amount
	                                ,franchiser_id
	                                ,type
                    FROM t_wallet_transfer_record
                    WHERE success_datetime BETWEEN @StartTime AND @EndTime
                    AND create_datetime BETWEEN @CreatStartTime AND @EndTime
                    AND type NOT IN('IN','OUT')
                    AND status = 'success'
                    ";
            if (SummaryRecordReq.Club_id != null)
            {
                par.Add("@Club_id", SummaryRecordReq.Club_id);
                strSql += " AND Club_id = @Club_id";
            }
            if (SummaryRecordReq.status != null)
            {
                par.Add("@status", SummaryRecordReq.status.ToLower());
                strSql += " AND status = @status";
            }
            par.Add("@StartTime", SummaryRecordReq.StartTime);
            par.Add("@EndTime", SummaryRecordReq.EndTime);
            //partition search
            par.Add("@CreatStartTime", SummaryRecordReq.StartTime.AddHours(-1));

            using (NpgsqlConnection conn = new NpgsqlConnection(await PGRead))
            {
                return await conn.QueryAsync<WalletTransferRecord>(strSql, par);
            }
        }
        public async Task<WalletTransferRecord> GetTransferRecordById(Guid id)
        {
            var par = new DynamicParameters();
            string strSql = @"SELECT 
                                    id
	                                ,source
	                                ,target
	                                ,create_datetime
	                                ,success_datetime
	                                ,before_balance
	                                ,after_balance
	                                ,STATUS
	                                ,club_id
	                                ,amount
	                                ,franchiser_id
	                                ,type
                    FROM t_wallet_transfer_record
                    WHERE id = @id
                    ";
            par.Add("@id", id);
            using (NpgsqlConnection conn = new NpgsqlConnection(await PGRead))
            {
                return await conn.QuerySingleOrDefaultAsync<WalletTransferRecord>(strSql, par);
            }
        }
        public async Task<IEnumerable<WalletTransferRecord>> GetTransferRecordBySession(string club_id)
        {
            var par = new DynamicParameters();
            string strSql = @"SELECT id
	                                ,source
	                                ,target
	                                ,create_datetime
	                                ,success_datetime
	                                ,before_balance
	                                ,after_balance
	                                ,STATUS
	                                ,club_id
	                                ,amount
	                                ,franchiser_id
	                                ,type
                    FROM t_wallet_transfer_record
                    WHERE club_id = @club_id
                    AND type = 'OUT'
                    ORDER BY create_datetime DESC
                    LIMIT 2
                    ";
            par.Add("@club_id", club_id);
            using (NpgsqlConnection conn = new NpgsqlConnection(await PGRead))
            {
                return await conn.QueryAsync<WalletTransferRecord>(strSql, par);
            }
        }
        public async Task<WalletTransferRecord> GetTransferRecordByIdLock(NpgsqlConnection conn, IDbTransaction tran, Guid id, DateTime create_datetime)
        {
            var par = new DynamicParameters();
            string strSql = @"SELECT id
	                                ,source
	                                ,target
	                                ,create_datetime
	                                ,success_datetime
	                                ,before_balance
	                                ,after_balance
	                                ,STATUS
	                                ,club_id
	                                ,amount
	                                ,franchiser_id
	                                ,type
                    FROM t_wallet_transfer_record
                    WHERE id = @id
                    AND create_datetime = @create_datetime
                    FOR UPDATE
                    ";
            par.Add("@id", id);
            par.Add("@create_datetime", create_datetime);
            return await conn.QuerySingleOrDefaultAsync<WalletTransferRecord>(strSql, par, tran);
        }
        public async Task<dynamic> GetWalletTransferRecordSummary(GetTransactionSummary_SummaryReq req)
        {
            var par = new DynamicParameters();
            string strSql = @"SELECT COUNT(id)
                    FROM t_wallet_transfer_record
                    WHERE create_datetime BETWEEN @StartTime AND @EndTime";
            if (req.Club_id != null)
            {
                strSql += " AND club_id = @club_id";
                par.Add("@club_id", req.Club_id);
            }
            if (req.Franchiser != null)
            {
                strSql += " AND franchiser_id = @franchiser_id";
                par.Add("@franchiser_id", req.Franchiser);
            }
            if (req.status != null)
            {
                par.Add("@status", req.status.ToLower());
                strSql += " AND status = @status";
            }
            if (req.type != null)
            {
                par.Add("@type", req.type.ToUpper());
                strSql += " AND type = @type";
            }
            par.Add("@StartTime", req.StartTime);
            par.Add("@EndTime", req.EndTime);
            using (NpgsqlConnection conn = new NpgsqlConnection(await PGRead))
            {
                return await conn.QueryAsync<dynamic>(strSql, par);
            }
        }
        public async Task<int> DeleteTransferRecordById(Guid id)
        {
            var par = new DynamicParameters();
            string strSqlDel = @"DELETE FROM t_wallet_transfer_record
                               WHERE id=@id";

            par.Add("@id", id);
            using (var conn = new NpgsqlConnection(PGMaster))
            {
                return await conn.ExecuteAsync(strSqlDel, par);
            }
        }
        #endregion
    }
}
