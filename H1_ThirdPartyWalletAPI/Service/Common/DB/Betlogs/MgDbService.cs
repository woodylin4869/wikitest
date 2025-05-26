using Dapper;
using H1_ThirdPartyWalletAPI.Model.DB.MG;
using H1_ThirdPartyWalletAPI.Model.DB.MG.Response;
using H1_ThirdPartyWalletAPI.Model.W1API;
using H1_ThirdPartyWalletAPI.Utility;
using Microsoft.Extensions.Logging;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace H1_ThirdPartyWalletAPI.Service.Common.DB.Betlogs
{
    public interface IMgDbService
    {
        /// <summary>
        /// 根據時間區間取得注單明細 (時間由小到大)
        /// </summary>
        /// <returns></returns>
        Task<List<GetMgRecordByBetTimeResponse>> GetMgRecordByBetTime(DateTime startTime, DateTime endTime);

        /// <summary>
        /// 根據注單編號取得注單明細
        /// </summary>
        /// <param name="betUid"></param>
        /// <param name="reportTime"></param>
        /// <returns></returns>
        Task<List<GetMgRecordByBetUidResponse>> GetMgRecordByBetUid(string betUid, DateTime reportTime);

        /// <summary>
        /// 根據注單編號取得注單明細
        /// </summary>
        /// <param name="betUid"></param>
        /// <param name="reportTime"></param>
        /// <returns></returns>
        Task<List<GetMgRecordByBetUidResponse>> GetMgRecordByBetUidV1(string betUid, DateTime reportTime);

        Task<int> PostMgRecord(NpgsqlConnection conn, IDbTransaction tran, List<Model.Game.MG.Response.BetRecord> record_data);
        Task<IEnumerable<GetMgRecordBySummaryResponse>> GetMgRecordBySummary(GetBetRecordReq RecordReq);

        /// <summary>
        /// 撈 t_mg_bet_record 注單資料
        /// MG 回饋統計時間區間為 >= 為開始的時間 
        /// </summary>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        Task<dynamic> SumMgBetRecordByBetTime(DateTime startTime, DateTime endTime);

        Task<MGRecordPrimaryKey> GetLatestMgRecord(DateTime time);

        Task<IEnumerable<(int count, decimal win, decimal bet, decimal jackpot, string userid, int game_type, DateTime createtime)>>
            SummaryGameRecord(DateTime reportTime, DateTime startTime, DateTime endTime);

        Task<List<GetMgRecordBySummaryResponse>> SumMgBetRecordByreport_time(DateTime createtime, DateTime report_time, string playerid);
    }

    public class MgDbService : BetlogsDBServiceBase, IMgDbService
    {
        public MgDbService(ILogger<MgDbService> logger, IBetLogsDbConnectionStringManager connectionStringManager) : base(logger, connectionStringManager)
        {
        }

        #region t_mg_bet_record

        /// <summary>
        /// 根據時間區間取得注單明細 (時間由小到大)
        /// </summary>
        /// <returns></returns>
        public async Task<List<GetMgRecordByBetTimeResponse>> GetMgRecordByBetTime(DateTime startTime, DateTime endTime)
        {
            string strSql = @"SELECT betuid, gameendtimeutc
                            FROM   t_mg_bet_record_v2
                            WHERE partition_time >= @startTime 
                            AND partition_time <= @endTime";
            var par = new DynamicParameters();
            par.Add("@startTime", startTime);
            par.Add("@endTime", endTime);

            using (NpgsqlConnection conn = new(await PGRead))
            {
                var result = await conn.QueryAsync<GetMgRecordByBetTimeResponse>(strSql, par);
                return result.ToList();
            }
        }
        /// <summary>
        /// 根據注單編號取得注單明細
        /// </summary>
        /// <param name="betUid"></param>
        /// <param name="reportTime"></param>
        /// <returns></returns>
        public async Task<List<GetMgRecordByBetUidResponse>> GetMgRecordByBetUid(string betUid, DateTime reportTime)
        {
            string strSql = @"SELECT betuid, gameendtimeutc, playerid
                            FROM   t_mg_bet_record_v2
                            WHERE betuid = @betUid 
                            AND partition_time >= @start_date
                            AND partition_time < @end_date
                            LIMIT 1";
            var par = new DynamicParameters();
            par.Add("@betUid", betUid);
            par.Add("@start_date", reportTime.AddDays(-3));
            par.Add("@end_date", reportTime.AddDays(1));

            using (NpgsqlConnection conn = new(await PGRead))
            {
                var result = await conn.QueryAsync<GetMgRecordByBetUidResponse>(strSql, par);
                return result.ToList();
            }
        }

        /// <summary>
        /// 根據注單編號取得注單明細
        /// </summary>
        /// <param name="betUid"></param>
        /// <param name="reportTime"></param>
        /// <returns></returns>
        public async Task<List<GetMgRecordByBetUidResponse>> GetMgRecordByBetUidV1(string betUid, DateTime reportTime)
        {
            string strSql = @"SELECT betuid, gameendtimeutc, playerid
                            FROM   t_mg_bet_record
                            WHERE betuid = @betUid 
                            AND gameendtimeutc >= @start_date
                            AND gameendtimeutc < @end_date
                            LIMIT 1";
            var par = new DynamicParameters();
            par.Add("@betUid", betUid);
            par.Add("@start_date", reportTime.AddDays(-3));
            par.Add("@end_date", reportTime.AddDays(1));

            using (NpgsqlConnection conn = new(await PGRead))
            {
                var result = await conn.QueryAsync<GetMgRecordByBetUidResponse>(strSql, par);
                return result.ToList();
            }
        }

        public async Task<int> PostMgRecord(NpgsqlConnection conn, IDbTransaction tran, List<Model.Game.MG.Response.BetRecord> record_data)
        {
            // 建立暫存表
            var tempTableName = $"temp_mg_bet_record_v2_{Guid.NewGuid():N}";
            try
            {
                await CreateTempTable(conn, tran, tempTableName);
                await BulkInsertToMgTempTable(conn, tran, tempTableName, record_data);
                return await MergeMgRecordFromTempTable(conn, tran, tempTableName);
            }
            catch (Exception ex)
            {
                _logger.LogDebug("PostMgRecord Message :{EX}", ex.Message);
                return 0;
            }
            finally
            {
                await RemovePostMgRecordTempTable(conn, tran, tempTableName);
            }
        }


        /// <summary>
        /// 建立暫存資料表
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="tran"></param>
        /// <param name="tempTableName"></param>
        /// <returns></returns>
        private async Task<string> CreateTempTable(NpgsqlConnection conn, IDbTransaction tran, string tempTableName)
        {
            var sql = "CREATE TEMPORARY TABLE IF NOT EXISTS #TempTableName ( LIKE t_mg_bet_record_v2 INCLUDING DEFAULTS INCLUDING CONSTRAINTS );";
            sql = sql.Replace("#TempTableName", tempTableName);
            // 建立temp資料表
            await conn.ExecuteAsync(sql, tran, commandTimeout: 60);

            // 建立唯一索引避免資料重複
            sql = $"CREATE UNIQUE index IF NOT EXISTS {tempTableName}_un ON {tempTableName} (betuid);";
            await conn.ExecuteAsync(sql, tran, commandTimeout: 60);

            return tempTableName;
        }

        public async Task<ulong> BulkInsertToMgTempTable(NpgsqlConnection conn, IDbTransaction tran, string tempTableName, List<Model.Game.MG.Response.BetRecord> record_data)
        {
            if (record_data == null || record_data.Count == 0)
            {
                return 0; // 沒有資料時，直接返回
            }

            string sql = @"COPY #TempTableName (
            betuid,
            createddateutc,
            gamestarttimeutc,
            gameendtimeutc,
            playerid,
            productid,
            productplayerid,
            platform,
            gamecode,
            channel,
            currency,
            betamount,
            payoutamount,
            betstatus,
            pca,
            externaltransactionid,
            jackpotwin,
            report_time,
            partition_time
        ) FROM STDIN (FORMAT BINARY)";

            sql = sql.Replace("#TempTableName", tempTableName);

            try
            {
                await using var writer = await conn.BeginBinaryImportAsync(sql);
                foreach (var record in record_data)
                {
                    await writer.StartRowAsync();
                    await writer.WriteAsync(record.BetUID, NpgsqlTypes.NpgsqlDbType.Varchar);
                    await writer.WriteAsync(record.createdDateUTC.HasValue ? (object)record.createdDateUTC.Value : (object)DBNull.Value, NpgsqlTypes.NpgsqlDbType.Timestamp);
                    await writer.WriteAsync(record.gameStartTimeUTC.HasValue ? (object)record.gameStartTimeUTC.Value : (object)DBNull.Value, NpgsqlTypes.NpgsqlDbType.Timestamp);
                    await writer.WriteAsync(record.gameEndTimeUTC.HasValue ? (object)record.gameEndTimeUTC.Value : (object)DBNull.Value, NpgsqlTypes.NpgsqlDbType.Timestamp);
                    await writer.WriteAsync(record.PlayerId, NpgsqlTypes.NpgsqlDbType.Varchar);
                    await writer.WriteAsync(record.ProductId, NpgsqlTypes.NpgsqlDbType.Varchar);
                    await writer.WriteAsync(record.ProductPlayerId, NpgsqlTypes.NpgsqlDbType.Varchar);
                    await writer.WriteAsync(record.Platform, NpgsqlTypes.NpgsqlDbType.Varchar);
                    await writer.WriteAsync(record.GameCode, NpgsqlTypes.NpgsqlDbType.Varchar);
                    await writer.WriteAsync(record.Channel, NpgsqlTypes.NpgsqlDbType.Varchar);
                    await writer.WriteAsync(record.Currency, NpgsqlTypes.NpgsqlDbType.Varchar);
                    await writer.WriteAsync(record.BetAmount.HasValue ? (object)record.BetAmount.Value : (object)DBNull.Value, NpgsqlTypes.NpgsqlDbType.Numeric);
                    await writer.WriteAsync(record.PayoutAmount.HasValue ? (object)record.PayoutAmount.Value : (object)DBNull.Value, NpgsqlTypes.NpgsqlDbType.Numeric);
                    await writer.WriteAsync(((int)record.BetStatus).ToString(), NpgsqlTypes.NpgsqlDbType.Varchar);
                    await writer.WriteAsync(record.PCA.HasValue ? (object)record.PCA.Value : (object)DBNull.Value, NpgsqlTypes.NpgsqlDbType.Numeric);
                    await writer.WriteAsync(record.ExternalTransactionId, NpgsqlTypes.NpgsqlDbType.Varchar);
                    await writer.WriteAsync(record.jackpotwin, NpgsqlTypes.NpgsqlDbType.Numeric);
                    await writer.WriteAsync(record.report_time, NpgsqlTypes.NpgsqlDbType.Timestamp);
                    await writer.WriteAsync(record.gameEndTimeUTC.HasValue ? (object)record.gameEndTimeUTC.Value : (object)DBNull.Value, NpgsqlTypes.NpgsqlDbType.Timestamp);
                }

                return await writer.CompleteAsync();
            }
            catch (Exception ex)
            {
                throw;
            }
        }


        /// <summary>
        /// 從TemapTable和主資料表做差集後，搬移資料回主注單資料表
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="tran"></param>
        /// <param name="tempTableName"></param>
        /// <returns></returns>
        private async Task<int> MergeMgRecordFromTempTable(NpgsqlConnection conn, IDbTransaction tran, string tempTableName)
        {
            var sql = @"
                    insert into t_mg_bet_record_v2 (
                            betuid,
                            createddateutc,
                            gamestarttimeutc,
                            gameendtimeutc,
                            playerid,
                            productid,
                            productplayerid,
                            platform,
                            gamecode,
                            channel,
                            currency,
                            betamount,
                            payoutamount,
                            betstatus,
                            pca,
                            externaltransactionid,
                            jackpotwin,
                            report_time,
                            partition_time
                        )
                        select betuid,
                               createddateutc,
                               gamestarttimeutc,
                               gameendtimeutc,
                               playerid,
                               productid,
                               productplayerid,
                               platform,
                               gamecode,
                               channel,
                               currency,
                               betamount,
                               payoutamount,
                               betstatus,
                               pca,
                               externaltransactionid,
                               jackpotwin,
                               report_time,
                               partition_time
                        from #TempTableName tempTable
                        where not exists (
                                select null from t_mg_bet_record_v2
		                        where partition_time = tempTable.partition_time
		                        and  betuid = tempTable.betuid
	                    );";

            sql = sql.Replace("#TempTableName", tempTableName);

            var rows = await conn.ExecuteAsync(sql, tran, commandTimeout: 60);

            return rows;
        }


        /// <summary>
        /// 移除暫存資料表
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="tran"></param>
        /// <param name="tempTableName"></param>
        /// <returns></returns>
        private async Task<int> RemovePostMgRecordTempTable(NpgsqlConnection conn, IDbTransaction tran, string tempTableName)
        {
            var sql = @"DROP TABLE IF EXISTS #TempTableName;";

            sql = sql.Replace("#TempTableName", tempTableName);

            var rows = await conn.ExecuteAsync(sql, tran, commandTimeout: 60);

            return rows;
        }


        /// <summary>
        /// 後會總不使用
        /// </summary>
        /// <param name="RecordReq"></param>
        /// <returns></returns>
        public async Task<IEnumerable<GetMgRecordBySummaryResponse>> GetMgRecordBySummary(GetBetRecordReq RecordReq)
        {
            var par = new DynamicParameters();
            string strSql = @"SELECT betuid, gameendtimeutc, createddateutc, gamecode, betamount, payoutamount,jackpotwin
                    FROM t_mg_bet_record
                    WHERE summary_id = @summary_id
                    AND gameendtimeutc >= @start_date
                    AND gameendtimeutc < @end_date
                    ";
            par.Add("@summary_id", Guid.Parse(RecordReq.summary_id));
            par.Add("@start_date", RecordReq.ReportTime.AddDays(-3));
            par.Add("@end_date", RecordReq.ReportTime.AddDays(1));

            using (NpgsqlConnection conn = new NpgsqlConnection(await PGRead))
            {
                return await conn.QueryAsync<GetMgRecordBySummaryResponse>(strSql, par);
            }
        }
        #endregion

        /// <summary>
        /// 撈 t_mg_bet_record 注單資料
        /// MG 回饋統計時間區間為 >= 為開始的時間 
        /// </summary>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        public async Task<dynamic> SumMgBetRecordByBetTime(DateTime startTime, DateTime endTime)
        {
            var par = new DynamicParameters();
            string strSql = @"SELECT 
                    SUM(payoutamount) AS total_win
                    , SUM(betAmount) AS total_bet
                    , COUNT(betuid) AS total_cont
                    FROM t_mg_bet_record_v2
                    WHERE partition_time >= @startTime
                    AND partition_time < @endTime
                    ";

            par.Add("@startTime", startTime);
            par.Add("@endTime", endTime);
            using (NpgsqlConnection conn = new(await PGRead))
            {
                return (await conn.QueryAsync<dynamic>(strSql, par)).FirstOrDefault();
            }
        }

        public async Task<MGRecordPrimaryKey> GetLatestMgRecord(DateTime time)
        {
            await using var conn = new NpgsqlConnection(await PGRead);
            var date = await GetLastNonEmptyPartition(conn, DateOnly.FromDateTime(time.AddHours(-1)));

            const string sql = @"select betuid, gameendtimeutc 
                                from t_mg_bet_record_v2
                                where partition_time >= @start and partition_time < @end
                                    and gamecode != 'EventRecord' --排除活動單
                                order by partition_time desc
                                limit 1";

            var param = new
            {
                start = date.ToDateTime(TimeOnly.MinValue),
                end = date.AddDays(1).ToDateTime(TimeOnly.MinValue) <= time
                    ? date.AddDays(1).ToDateTime(TimeOnly.MinValue)
                    : time
            };

            var result = await conn.QueryFirstAsync<MGRecordPrimaryKey>(sql, param);
            return result;
        }

        private async Task<DateOnly> GetLastNonEmptyPartition(IDbConnection conn, DateOnly date)
        {
            if (DateTime.Now - date.ToDateTime(TimeOnly.MinValue) > TimeSpan.FromDays(100))
                throw new Exception("Mg bet record not exists!");

            const string sql = @"select 1 
                                from t_mg_bet_record_v2 
                                where partition_time >= @start and partition_time < @end 
                                    and gamecode != 'EventRecord' --排除活動單
                                limit 1";

            var param = new
            {
                start = date.ToDateTime(TimeOnly.MinValue),
                end = date.AddDays(1).ToDateTime(TimeOnly.MinValue)
            };

            var result = await conn.QueryAsync<int>(sql, param);
            if (result.Any())
                return date;
            else
                return await GetLastNonEmptyPartition(conn, date.AddDays(-1));
        }


        public async Task<List<GetMgRecordBySummaryResponse>> SumMgBetRecordByreport_time(DateTime createtime, DateTime report_time, string playerid)
        {

            string strSql = @"SELECT 
                    betuid, gameendtimeutc, createddateutc, gamecode, betamount, payoutamount,jackpotwin
                    FROM t_mg_bet_record_v2
                    WHERE partition_time >= @startTime
                    AND partition_time < @endTime
                    AND report_time = @reporttime 
                    AND playerid = @playerid 
                    ";

            var parameters = new DynamicParameters();
            parameters.Add("@starttime", createtime);
            parameters.Add("@endtime", createtime.AddDays(1).AddMilliseconds(-1));
            parameters.Add("@reporttime", report_time);
            parameters.Add("@playerid", playerid);
            using (NpgsqlConnection conn = new(await PGRead))
            {
                var result = await conn.QueryAsync<GetMgRecordBySummaryResponse>(strSql, parameters);
                return result.ToList();
            }
        }



        /// <summary>
        /// 五分鐘會總
        /// </summary>
        /// <param name="reportTime"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        public async Task<IEnumerable<(int count, decimal win, decimal bet, decimal jackpot, string userid, int game_type, DateTime createtime)>> SummaryGameRecord(DateTime reportTime, DateTime startTime, DateTime endTime)
        {
            var sql = @"SELECT CAST(count(1) AS INTEGER) AS count,
                        SUM(payoutamount) AS win,
                        SUM(betamount) AS bet,
                        SUM(jackpotwin) as jackpot,
                        playerid as userid, 
                        3 as game_type,
                        Date(partition_time) as createtime
                        FROM t_mg_bet_record_v2
                        WHERE partition_time BETWEEN @start_time AND @end_time
                        AND report_time = @report_time
                        GROUP BY playerid,Date(partition_time)
                        ";

            var par = new DynamicParameters();
            par.Add("@start_time", startTime.AddDays(-2));
            par.Add("@end_time", endTime);
            par.Add("@report_time", reportTime);
            using (NpgsqlConnection conn = new NpgsqlConnection(await PGRead))
            {
                var result = await conn.QueryAsync(sql, par, commandTimeout: 270);
                var a = result.Select(x => ((int)x.count, (decimal)x.win, (decimal)x.bet, (decimal)x.jackpot, (string)x.userid, (int)x.game_type, (DateTime)x.createtime)).ToList();
                return a;
            }
        }
    }
}
