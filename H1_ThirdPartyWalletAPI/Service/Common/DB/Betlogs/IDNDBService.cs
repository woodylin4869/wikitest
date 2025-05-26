using Dapper;
using H1_ThirdPartyWalletAPI.Model.W1API;
using H1_ThirdPartyWalletAPI.Utility;
using Microsoft.Extensions.Logging;
using Npgsql;
using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using ThirdPartyWallet.Share.Model.Game.IDN.Response;

namespace H1_ThirdPartyWalletAPI.Service.Common.DB.Betlogs
{
    public interface IIDNDBService
    {
        Task<int> PostIDNRecord(NpgsqlConnection conn, IDbTransaction tran, List<Bet_History> betLogs);

        Task<List<Bet_History>> GetIDNRecordsBytime(DateTime partitionTime, DateTime report_time, string gamePlatformUserId, int groupgametype);

        Task<List<Bet_History>> GetIDNRecords(string bet_id, DateTime time);

        Task<(int TotalCount, decimal TotalBetValid, decimal TotalNetWin)> SumIDNBetRecordByPartitionTime(DateTime start, DateTime end);

        Task<List<Bet_History>> GetIDNRecordsByPartition(DateTime starttime, DateTime endtime);

        Task<IEnumerable<(int count, decimal netwin, decimal bet, string userid, int game_type, DateTime partitionTime)>>
            SummaryGameRecord(DateTime reportTime, DateTime startTime, DateTime endTime);
    }

    public class IDNDBService : BetlogsDBServiceBase, IIDNDBService
    {
        public IDNDBService(ILogger<IDNDBService> logger, IBetLogsDbConnectionStringManager connectionStringManager) : base(logger, connectionStringManager)
        {
        }

        /// <summary>
        /// 寫入資料
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="tran"></param>
        /// <param name="betLogs"></param>
        /// <returns></returns>
        public async Task<int> PostIDNRecord(NpgsqlConnection conn, IDbTransaction tran, List<Bet_History> betLogs)
        {
            // 建立暫存表
            var tempTableName = $"temp_t_idn_bet_record_{Guid.NewGuid():N}";
            try
            {
                await CreateTempTable(conn, tran, tempTableName);
                await BulkInsertToIDNTempTable(conn, tran, tempTableName, betLogs);
                return await MergeIDNRecordFromTempTable(conn, tran, tempTableName);
            }
            catch (Exception ex)
            {

                return 0;
            }
            finally
            {
                await RemovePostIDNRecordTempTable(conn, tran, tempTableName);
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
            //var tempTableName = $"temp_t_idn_bet_record_{Guid.NewGuid():N}";
            var sql = "CREATE TEMPORARY TABLE IF NOT EXISTS #TempTableName ( LIKE t_idn_bet_record INCLUDING ALL);";
            sql = sql.Replace("#TempTableName", tempTableName);
            // 建立temp資料表
            await conn.ExecuteAsync(sql, tran, commandTimeout: 60);
            return tempTableName;
        }

        /// <summary>
        /// Copy至暫存表
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="tran"></param>
        /// <param name="tempTableName"></param>
        /// <param name="record_data"></param>
        /// <returns></returns>
        private async Task<ulong> BulkInsertToIDNTempTable(NpgsqlConnection conn, IDbTransaction tran, string tempTableName,
            List<Bet_History> record_data)
        {
            var sql = @"COPY #TempTableName
                    (
                       date,
                       round_id,
                       bet_id,
                       match_id,
                       game_id,
                       bet_type,
                       bet,
                       win,
                       game_username,
                       id,
                       raw_data,
                       game_result,
                       game_name,
                       report_time,
                       pre_bet,
                       pre_win,
                       partition_time,
                       groupgametype
                    )
                    FROM STDIN (FORMAT BINARY)";

            sql = sql.Replace("#TempTableName", tempTableName);

            await using var writer = await conn.BeginBinaryImportAsync(sql);
            foreach (var log in record_data)
            {
                await writer.StartRowAsync();
                await writer.WriteAsync(log.date, NpgsqlDbType.Timestamp);
                await writer.WriteAsync(log.round_id, NpgsqlDbType.Varchar);
                await writer.WriteAsync(log.bet_id, NpgsqlDbType.Varchar);
                await writer.WriteAsync(log.match_id, NpgsqlDbType.Varchar);
                await writer.WriteAsync(log.game_id, NpgsqlDbType.Varchar);
                await writer.WriteAsync(log.bet_type, NpgsqlDbType.Varchar);
                await writer.WriteAsync(log.bet, NpgsqlDbType.Numeric);
                await writer.WriteAsync(log.win, NpgsqlDbType.Numeric);
                await writer.WriteAsync(log.game_username, NpgsqlDbType.Varchar);
                await writer.WriteAsync(log.id, NpgsqlDbType.Bigint);
                await writer.WriteAsync(log.raw_data ?? (object)DBNull.Value, NpgsqlDbType.Text);
                await writer.WriteAsync(log.game_result ?? (object)DBNull.Value, NpgsqlDbType.Text);
                await writer.WriteAsync(log.game_name ?? (object)DBNull.Value, NpgsqlDbType.Varchar);
                await writer.WriteAsync(log.Report_time ?? (object)DBNull.Value, NpgsqlDbType.Timestamp);
                await writer.WriteAsync(log.Pre_bet, NpgsqlDbType.Numeric);
                await writer.WriteAsync(log.Pre_win, NpgsqlDbType.Numeric);
                await writer.WriteAsync(log.date, NpgsqlDbType.Timestamp); // partition_time 使用 date
                await writer.WriteAsync(log.groupgametype, NpgsqlDbType.Integer);
            }
            return await writer.CompleteAsync();
        }

        /// <summary>
        /// 從TemapTable和主資料表做差集後，搬移資料回主注單資料表
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="tran"></param>
        /// <param name="tempTableName"></param>
        /// <returns></returns>
        private async Task<int> MergeIDNRecordFromTempTable(NpgsqlConnection conn, IDbTransaction tran, string tempTableName)
        {
            var sql = @"
                    insert into t_idn_bet_record (
                            date,
                            round_id,
                            bet_id,
                            match_id,
                            game_id,
                            bet_type,
                            bet,
                            win,
                            game_username,
                            id,
                            raw_data,
                            game_result,
                            game_name,
                            report_time,
                            pre_bet,
                            pre_win,
                            partition_time,
                            groupgametype
                        )
                        select date,
                               round_id,
                               bet_id,
                               match_id,
                               game_id,
                               bet_type,
                               bet,
                               win,
                               game_username,
                               id,
                               raw_data,
                               game_result,
                               game_name,
                               report_time,
                               pre_bet,
                               pre_win,
                               partition_time,
                               groupgametype
                        from #TempTableName tempTable
                        where not exists (
                                select null from t_idn_bet_record
		                        where partition_time = tempTable.partition_time
		                        and  bet_type = tempTable.bet_type
                                and  id = tempTable.id
	                    );
                    ";

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
        private async Task<int> RemovePostIDNRecordTempTable(NpgsqlConnection conn, IDbTransaction tran, string tempTableName)
        {
            var sql = @"DROP TABLE IF EXISTS #TempTableName ;";

            sql = sql.Replace("#TempTableName", tempTableName);

            var rows = await conn.ExecuteAsync(sql, tran, commandTimeout: 60);

            return rows;
        }


        /// <summary>
        /// 五分鐘會總
        /// </summary>
        /// <param name="reportTime"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        public async Task<IEnumerable<(int count, decimal netwin, decimal bet, string userid, int game_type, DateTime partitionTime)>> SummaryGameRecord(DateTime reportTime, DateTime startTime, DateTime endTime)
        {
            var sql = @"SELECT CAST(count(1) AS INTEGER) AS count,
                        coalesce(SUM(win),0) AS netwin,
                        coalesce(SUM(bet),0) AS bet,
                        game_username as userid,
                        groupgametype as game_type,
                        Date(partition_time) as partition_time
                        FROM t_idn_bet_record
                        WHERE partition_time BETWEEN @start_time AND @end_time
                        AND report_time = @report_time
                        GROUP BY game_username,Date(partition_time),groupgametype
                        ";

            var par = new DynamicParameters();
            par.Add("@start_time", startTime);
            par.Add("@end_time", endTime);
            par.Add("@report_time", reportTime);
            using (NpgsqlConnection conn = new NpgsqlConnection(await PGRead))
            {
                var result = await conn.QueryAsync(sql, par, commandTimeout: 270);
                var a = result.Select(x => ((int)x.count, (decimal)x.netwin, (decimal)x.bet, (string)x.userid, (int)x.game_type, (DateTime)x.partition_time)).ToList();
                return a;
            }
        }

        /// <summary>
        /// 查詢第2層明細(5分鐘彙總帳)需要資訊
        /// </summary>
        /// <param name="partitionTime">BetTime</param>
        /// <param name="report_time">ReportTime</param>
        /// <param name="gamePlatformUserId">遊戲商使用者ID</param>
        /// <param name="groupgametype"></param>
        /// <returns></returns>
        public async Task<List<Bet_History>> GetIDNRecordsBytime(DateTime partitionTime, DateTime report_time, string gamePlatformUserId, int groupgametype)
        {
            try
            {
                var sql = @"SELECT
                             date,
                             round_id,
                             bet_id,
                             match_id,
                             game_id,
                             bet_type,
                             bet,
                             win,
                             game_username,
                             id,
                             raw_data,
                             game_result,
                             game_name,
                             report_time,
                             pre_bet,
                             pre_win,
                             partition_time,
                             groupgametype
                        FROM public.t_idn_bet_record
                            WHERE partition_time BETWEEN @starttime AND @endtime
                            AND report_time = @reporttime
                            AND game_username = @game_username 
                            AND groupgametype = @groupgametype";
                var parameters = new DynamicParameters();
                parameters.Add("@starttime", partitionTime);
                parameters.Add("@endtime", partitionTime.AddDays(1).AddMilliseconds(-1));
                parameters.Add("@reporttime", report_time);
                parameters.Add("@game_username", gamePlatformUserId);
                parameters.Add("@groupgametype", groupgametype);

                await using var conn = new NpgsqlConnection(await PGRead);
                var result = await conn.QueryAsync<Bet_History>(sql, parameters);
                return result.ToList();
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        /// <summary>
        /// 注單號取資料
        /// </summary>
        /// <param name="bet_id"></param>
        /// <param name="report_time"></param>
        /// <returns></returns>
        public async Task<List<Bet_History>> GetIDNRecords(string bet_id, DateTime report_time)
        {
            var sql = @"
                    SELECT round_id,bet_id,date,game_id,match_id,bet,win,raw_data  FROM t_idn_bet_record
                      WHERE partition_time BETWEEN @starttime AND @endtime 
                            AND bet_id = @bet_id ";

            var par = new DynamicParameters();
            par.Add("@starttime", report_time.AddDays(-3));
            par.Add("@endtime", report_time.AddDays(1));
            par.Add("@bet_id", bet_id);

            await using var conn = new NpgsqlConnection(await PGRead);
            var result = await conn.QueryAsync<Bet_History>(sql, par);
            return result.ToList();
        }

        /// <summary>
        /// 每小時匯總
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public async Task<(int TotalCount, decimal TotalBetValid, decimal TotalNetWin)> SumIDNBetRecordByPartitionTime(DateTime start, DateTime end)
        {
            var sql = @"SELECT
                    COUNT(1) AS totalcount
                    , coalesce(SUM(bet),0) AS totalbetvalid
                    , coalesce(SUM(win),0) AS totalnetwin
                    FROM t_idn_bet_record
                    WHERE partition_time >= @startTime
                        AND partition_time < @endTime";

            var par = new DynamicParameters();
            par.Add("@startTime", start);
            par.Add("@endTime", end.AddMilliseconds(-1));

            await using var conn = new NpgsqlConnection(await PGRead);
            var result = (await conn.QueryAsync<dynamic>(sql, par)).FirstOrDefault();
            return ((int)result.totalcount, (decimal)result.totalbetvalid, (decimal)result.totalnetwin);
        }

        /// <summary>
        /// 取得partition 時間區間內 注單
        /// </summary>
        /// <param name="starttime"></param>
        /// <param name="endtime"></param>
        /// <returns></returns>
        public async Task<List<Bet_History>> GetIDNRecordsByPartition(DateTime starttime, DateTime endtime)
        {
            try
            {
                var sql = @"SELECT id,bet_type,date
                    FROM public.t_idn_bet_record
                        WHERE partition_time BETWEEN @starttime AND @endtime
                       ";
                var parameters = new DynamicParameters();
                parameters.Add("@starttime", starttime);
                parameters.Add("@endtime", endtime);

                await using var conn = new NpgsqlConnection(await PGRead);
                var result = await conn.QueryAsync<Bet_History>(sql, parameters);
                return result.ToList();
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<IEnumerable<Bet_History>> GetIDNRunningRecord(GetBetRecordUnsettleReq RecordReq)
        {
            var par = new DynamicParameters();
            string strSql = @"SELECT id 
                                FROM PUBLIC.t_idn_bet_record_running
                                WHERE bet_time BETWEEN @StartTime AND @EndTime
                    ";

            if (RecordReq.Club_id != null)
            {
                par.Add("@Club_id", RecordReq.Club_id);
                strSql += " AND Club_id = @Club_id";
            }
            if (RecordReq.Franchiser_id != null)
            {
                par.Add("@Franchiser_id", RecordReq.Franchiser_id);
                strSql += " AND Franchiser_id = @Franchiser_id";
            }
            par.Add("@StartTime", RecordReq.StartTime != null ? RecordReq.StartTime : DateTime.Now.AddDays(-100));
            par.Add("@EndTime", RecordReq.EndTime != null ? RecordReq.EndTime : DateTime.Now);
            using (NpgsqlConnection conn = new NpgsqlConnection(await PGRead))
            {
                return await conn.QueryAsync<Bet_History>(strSql, par);
            }
        }
    }
}