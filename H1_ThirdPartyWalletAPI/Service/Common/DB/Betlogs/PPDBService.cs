using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using H1_ThirdPartyWalletAPI.Model.Config;
using H1_ThirdPartyWalletAPI.Model.DB.PP;
using H1_ThirdPartyWalletAPI.Model.DB.PP.Response;
using H1_ThirdPartyWalletAPI.Model.Game.PP.Responses;
using H1_ThirdPartyWalletAPI.Model.W1API;
using H1_ThirdPartyWalletAPI.Utility;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;
using static H1_ThirdPartyWalletAPI.Model.Game.NEXTSPIN.Response.GetBetHistoryResponse;

namespace H1_ThirdPartyWalletAPI.Service.Common.DB.Betlogs
{
    public interface IPPDBService
    {
        Task<int> PostppRecord(NpgsqlConnection conn, IDbTransaction tran, IEnumerable<GetRecordResponses> betLogs);
        Task<List<GetPPRecordsBySummaryReponse>> GetppRecordsBytime(DateTime createtime, DateTime report_time, string club_id);
        Task<(int totalCount, decimal totalBetValid, decimal totalWin)> SumppBetRecordByBetTime(DateTime start, DateTime end);
        Task<List<GetPPRecordsBySummaryReponse>> GetppRecordsBySummary(GetBetRecordReq RecordReq);
        Task<List<GetPPRecordResponse>> GetppRecord(string playsessionid, DateTime ReportTime);
        Task<List<GetPPRecordResponse>> GetppRecordNew(string playsessionid, DateTime sDateTime);
        Task<int> PostppRunningRecord(NpgsqlConnection conn, IDbTransaction tran, IEnumerable<GetRecordResponses> betLogs);

        Task<int> DeleteppRunningRecord(NpgsqlConnection conn, IDbTransaction tran, GetRecordResponses record_data);

        Task<IEnumerable<GetRecordResponses>> GetppRunningRecord(GetBetRecordUnsettleReq RecordReq);

        Task<int> Postppv2Record(NpgsqlConnection conn, IDbTransaction tran, IEnumerable<GetRecordResponses> betLogs);

        Task<IEnumerable<(int count, decimal win, decimal bet, decimal jackpot, string userid, int game_type, DateTime createtime)>>
    SummaryGameRecord(DateTime reportTime, DateTime startTime, DateTime endTime);
        Task<List<GetPPRecordResponse>> Getppv2Record(string playsessionid, DateTime ReportTime);

        Task<List<GetPPRecordsBySummaryReponse>> GetppRecordsv1(DateTime startTime, DateTime endTime);

        Task<List<GetPPRecordsBySummaryReponse>> GetppV2RecordsBytime(DateTime starttime, DateTime endtime);
    }
    public class PPDBService : BetlogsDBServiceBase, IPPDBService
    {
        public PPDBService(ILogger<PPDBService> logger, IBetLogsDbConnectionStringManager connectionStringManager) : base(logger, connectionStringManager)
        {
        }
        /// <summary>
        /// 寫入資料
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="tran"></param>
        /// <param name="betLogs"></param>
        /// <returns></returns>
        public async Task<int> PostppRecord(NpgsqlConnection conn, IDbTransaction tran, IEnumerable<GetRecordResponses> betLogs)
        {
            var sql = @"INSERT INTO public.t_pp_bet_record
                    ( playerid
                     ,extplayerid
                     ,gameid
                     ,playsessionid
                     ,parentsessionid
                     ,startdate
                     ,enddate
                     ,status
                     ,""type""
                     ,bet
                     ,win
                     ,currency
                     ,jackpot
                     ,summary_id
                     ,pre_bet
                     ,pre_win)
                     VALUES
                     ( @playerid
                      ,@extplayerid
                      ,@gameid
                      ,@playsessionid
                      ,@parentsessionid
                      ,@startdate
                      ,@enddate
                      ,@status
                      ,@type
                      ,@bet
                      ,@win
                      ,@currency
                      ,@jackpot
                      ,@summary_id
                      ,@pre_bet
                      ,@pre_win )
                    ";

            return await conn.ExecuteAsync(sql, betLogs, tran);
        }

        public async Task<int> Postppv2Record(NpgsqlConnection conn, IDbTransaction tran, IEnumerable<GetRecordResponses> betLogs)
        {

            var tempTableName = $"t_pp_bet_record_v2_{Guid.NewGuid():N}";
            try
            {
                await CreateTempTable(conn, tran, tempTableName);
                await BulkInsertToTempTable(conn, tran, tempTableName, betLogs);
                return await MergeRecordFromTempTable(conn, tran, tempTableName);
            }
            finally
            {
                await RemovePostnextspinRecordTempTable(conn, tran, tempTableName);
            }

            //var sql = @"INSERT INTO public.t_pp_bet_record_v2
            //        ( playerid
            //         ,extplayerid
            //         ,gameid
            //         ,playsessionid
            //         ,parentsessionid
            //         ,startdate
            //         ,enddate
            //         ,status
            //         ,""type""
            //         ,bet
            //         ,win
            //         ,currency
            //         ,jackpot
            //         ,pre_bet
            //         ,pre_win
            //        ,report_time
            //            )
            //         VALUES
            //         ( 
            //            @playerid
            //          ,@extplayerid
            //          ,@gameid
            //          ,@playsessionid
            //          ,@parentsessionid
            //          ,@startdate
            //          ,@enddate
            //          ,@status
            //          ,@type
            //          ,@bet
            //          ,@win
            //          ,@currency
            //          ,@jackpot
            //          ,@pre_bet
            //          ,@pre_win 
            //          ,@report_time
            //            )
            //        ";

            //return await conn.ExecuteAsync(sql, betLogs, tran);
        }


        /// <summary>
        /// 寫入未結單
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="tran"></param>
        /// <param name="betLogs"></param>
        /// <returns></returns>
        public async Task<int> PostppRunningRecord(NpgsqlConnection conn, IDbTransaction tran, IEnumerable<GetRecordResponses> betLogs)
        {
            var sql = @"INSERT INTO public.t_pp_bet_record_running 
                    ( playerid
                     ,extplayerid
                     ,gameid
                     ,playsessionid
                     ,parentsessionid
                     ,startdate
                     ,enddate
                     ,status
                     ,""type""
                     ,bet
                     ,win
                     ,currency
                     ,jackpot
                     ,summary_id
                     ,club_id
                     ,franchiser_id)
                     VALUES
                     ( @playerid
                      ,@extplayerid
                      ,@gameid
                      ,@playsessionid
                      ,@parentsessionid
                      ,@startdate
                      ,@enddate
                      ,@status
                      ,@type
                      ,@bet
                      ,@win
                      ,@currency
                      ,@jackpot
                      ,@summary_id
                      ,@club_id
                      ,@franchiser_id)
                    ";

            return await conn.ExecuteAsync(sql, betLogs, tran);
        }
        /// <summary>
        /// 每小時匯總
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public async Task<(int totalCount, decimal totalBetValid, decimal totalWin)> SumppBetRecordByBetTime(DateTime start, DateTime end)
        {
            var sql = @"SELECT 
                    COUNT(playsessionid) AS totalCount
                    , CASE WHEN SUM(bet) IS NULL THEN 0 ELSE SUM(bet) END  AS totalBetValid
                    , CASE WHEN SUM(win) IS NULL THEN 0 ELSE SUM(win) END AS totalWin
                    FROM t_pp_bet_record_v2
                    WHERE startdate >= @startTime 
                        AND startdate < @endTime";

            var par = new DynamicParameters();
            par.Add("@startTime", start);
            par.Add("@endTime", start.AddHours(1));

            await using var conn = new NpgsqlConnection(await PGRead);
            var result = (await conn.QueryAsync<dynamic>(sql, par)).FirstOrDefault();
            return ((int)result.totalcount, (decimal)result.totalbetvalid, (decimal)result.totalwin);
        }
        /// <summary>
        /// 取得時間內的住單
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public async Task<List<GetPPRecordsBySummaryReponse>> GetppRecordsBytime(DateTime createtime, DateTime report_time, string club_id)
        {
            var sql = @"SELECT PlaySessionID,StartDate,Status,GameID,EndDate,Bet,Win,Currency,Jackpot,ExtPlayerID
                    FROM public.t_pp_bet_record_v2
                    WHERE startdate >= @startTime 
                        AND startdate <= @endTime
                        AND report_time = @reporttime
                        AND extplayerid=@club_id";
            var par = new DynamicParameters();
            par.Add("@startTime", createtime);
            par.Add("@endTime", createtime.AddDays(1).AddMilliseconds(-1));
            par.Add("@reporttime", report_time);
            par.Add("@club_id", Config.OneWalletAPI.Prefix_Key + club_id);

            await using var conn = new NpgsqlConnection(await PGRead);
            var result = await conn.QueryAsync<GetPPRecordsBySummaryReponse>(sql, par);
            return result.ToList();
        }

        /// <summary>
        /// 取得GUID資料
        /// </summary>
        /// <param name="RecordReq"></param>
        /// <returns></returns>
        public async Task<List<GetPPRecordsBySummaryReponse>> GetppRecordsBySummary(GetBetRecordReq RecordReq)
        {
            var sql = @"SELECT  PlaySessionID,StartDate,Status,GameID,EndDate,Bet,Win,Currency,Jackpot,ExtPlayerID
                    FROM public.t_pp_bet_record 
                    WHERE startdate >= @start 
                        AND startdate <= @end
                        AND summary_id = @summaryId::uuid";
            var param = new
            {
                summaryId = RecordReq.summary_id,
                start = RecordReq.ReportTime.AddDays(-3),
                end = RecordReq.ReportTime.AddDays(1),
            };

            await using var conn = new NpgsqlConnection(await PGRead);
            var result = await conn.QueryAsync<GetPPRecordsBySummaryReponse>(sql, param);
            return result.ToList();
        }
        /// <summary>
        /// 時間區間
        /// </summary>
        /// <param name="playsessionid"></param>
        /// <param name="ReportTime"></param>
        /// <returns></returns>
        public async Task<List<GetPPRecordResponse>> GetppRecord(string playsessionid, DateTime ReportTime)
        {
            string strSql = @"SELECT PlaySessionID,ExtPlayerID,pre_Bet,pre_Win
                    FROM t_pp_bet_record
                    WHERE playsessionid = @playsessionid
                         AND startdate >= @start 
                        AND startdate <= @end ";
            var parameters = new DynamicParameters();
            parameters.Add("@playsessionid", Int64.Parse(playsessionid));
            parameters.Add("@start", ReportTime.AddDays(-3));
            parameters.Add("@end", ReportTime.AddDays(1));

            using (var conn = new NpgsqlConnection(await PGRead))
            {
                var result = await conn.QueryAsync<GetPPRecordResponse>(strSql, parameters);
                return result.ToList();
            }
        }

        public async Task<List<GetPPRecordResponse>> Getppv2Record(string playsessionid, DateTime ReportTime)
        {
            string strSql = @"SELECT PlaySessionID,ExtPlayerID,pre_Bet,pre_Win
                    FROM t_pp_bet_record_v2
                    WHERE playsessionid = @playsessionid
                         AND startdate >= @start 
                        AND startdate <= @end ";
            var parameters = new DynamicParameters();
            parameters.Add("@playsessionid", Int64.Parse(playsessionid));
            parameters.Add("@start", ReportTime.AddDays(-3));
            parameters.Add("@end", ReportTime.AddDays(1));

            using (var conn = new NpgsqlConnection(await PGRead))
            {
                var result = await conn.QueryAsync<GetPPRecordResponse>(strSql, parameters);
                return result.ToList();
            }
        }

        /// <summary>
        /// 時間區間
        /// </summary>
        /// <param name="playsessionid"></param>
        /// <param name="ReportTime"></param>
        /// <returns></returns>
        public async Task<List<GetPPRecordResponse>> GetppRecordNew(string playsessionid, DateTime StartDate)
        {
            string strSql = @"SELECT PlaySessionID,StartDate,ExtPlayerID,status,pre_Bet,pre_Win
                    FROM t_pp_bet_record_v2
                    WHERE playsessionid = @playsessionid
                         AND startdate = @StartDate";
            var parameters = new DynamicParameters();
            parameters.Add("@playsessionid", Int64.Parse(playsessionid));
            parameters.Add("@StartDate", StartDate);

            using (var conn = new NpgsqlConnection(await PGRead))
            {
                var result = await conn.QueryAsync<GetPPRecordResponse>(strSql, parameters);
                return result.ToList();
            }
        }

        public async Task<int> DeleteppRunningRecord(NpgsqlConnection conn, IDbTransaction tran, GetRecordResponses record_data)
        {
            string strSqlDel = @"DELETE FROM t_pp_bet_record_running
                               WHERE playsessionid=@playsessionid";
            return await conn.ExecuteAsync(strSqlDel, record_data, tran);
        }

        public async Task<IEnumerable<GetRecordResponses>> GetppRunningRecord(GetBetRecordUnsettleReq RecordReq)
        {
            var par = new DynamicParameters();
            string strSql = @"SELECT *
                    FROM t_pp_bet_record_running
                    WHERE startdate BETWEEN @StartTime AND @EndTime
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
                return await conn.QueryAsync<GetRecordResponses>(strSql, par);
            }
        }
        /// <summary>
        /// 五分彙總
        /// </summary>
        /// <param name="reportTime"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        public async Task<IEnumerable<(int count, decimal win, decimal bet, decimal jackpot, string userid, int game_type, DateTime createtime)>> SummaryGameRecord(DateTime reportTime, DateTime startTime, DateTime endTime)
        {
            var sql = @"SELECT CAST(count(1) AS INTEGER) AS count,
                         SUM(win) AS win,
                        SUM(bet) AS bet,
                        0 as jackpot,
                        extplayerid as userid, 
                        3 as game_type,
                        Date(startdate) as createtime
                        FROM t_pp_bet_record_v2
                        WHERE startdate BETWEEN @start_time AND @end_time
                        AND report_time = @report_time
                        GROUP BY extplayerid,Date(startdate)
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

        /// <summary>
        /// 補單需取v1表查詢
        /// </summary>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        public async Task<List<GetPPRecordsBySummaryReponse>> GetppRecordsv1(DateTime startTime, DateTime endTime)
        {
            var sql = @"SELECT playsessionid,startDate,status
                    FROM public.t_pp_bet_record
                    WHERE startdate >= @startTime 
                        AND startdate <= @endTime";
            var par = new DynamicParameters();
            par.Add("@startTime", startTime);
            par.Add("@endTime", endTime);


            await using var conn = new NpgsqlConnection(await PGRead);
            var result = await conn.QueryAsync<GetPPRecordsBySummaryReponse>(sql, par);
            return result.ToList();
        }


        /// <summary>
        /// 建立站存表
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="tran"></param>
        /// <param name="tempTableName"></param>
        /// <returns></returns>
        private async Task<string> CreateTempTable(NpgsqlConnection conn, IDbTransaction tran, string tempTableName)
        {
            //var tempTableName = $"temp_t_rsg_bet_record_{Guid.NewGuid():N}";
            var sql = "CREATE TEMPORARY TABLE IF NOT EXISTS #TempTableName ( LIKE t_pp_bet_record_v2 INCLUDING ALL );";
            sql = sql.Replace("#TempTableName", tempTableName);
            // 建立temp資料表
            await conn.ExecuteAsync(sql, tran, commandTimeout: 60);

            // 建立唯一索引避免資料重複
            sql = $"CREATE UNIQUE index IF NOT EXISTS {tempTableName}_un ON {tempTableName} (playsessionid);";
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
        private async Task<ulong> BulkInsertToTempTable(NpgsqlConnection conn, IDbTransaction tran, string tempTableName,
        IEnumerable<GetRecordResponses> record_data)
        {
            var sql = @"COPY #TempTableName (
                          playerid
                         ,extplayerid
                         ,gameid
                         ,playsessionid
                         ,parentsessionid
                         ,startdate
                         ,enddate
                         ,status
                         ,""type""
                         ,bet
                         ,win
                         ,currency
                         ,jackpot
                         ,pre_bet
                         ,pre_win
                        ,report_time
                        ) FROM STDIN (FORMAT BINARY)";


            sql = sql.Replace("#TempTableName", tempTableName);
            try
            {
                await using var writer = await conn.BeginBinaryImportAsync(sql);
                foreach (var betInfo in record_data)
                {
                    await writer.StartRowAsync();
                    // 寫入每一列的資料，請根據你的數據庫結構和類型進行調整
                    await writer.WriteAsync(betInfo.PlayerID, NpgsqlTypes.NpgsqlDbType.Varchar);
                    await writer.WriteAsync(betInfo.ExtPlayerID, NpgsqlTypes.NpgsqlDbType.Varchar);
                    await writer.WriteAsync(betInfo.GameID, NpgsqlTypes.NpgsqlDbType.Varchar);
                    await writer.WriteAsync(betInfo.PlaySessionID, NpgsqlTypes.NpgsqlDbType.Bigint);
                    await writer.WriteAsync(betInfo.ParentSessionID, NpgsqlTypes.NpgsqlDbType.Varchar);
                    await writer.WriteAsync(betInfo.StartDate, NpgsqlTypes.NpgsqlDbType.Timestamp);
                    await writer.WriteAsync(betInfo.EndDate ?? (object)DBNull.Value, NpgsqlTypes.NpgsqlDbType.Timestamp);
                    await writer.WriteAsync(betInfo.Status, NpgsqlTypes.NpgsqlDbType.Varchar);
                    await writer.WriteAsync(betInfo.Type, NpgsqlTypes.NpgsqlDbType.Varchar);
                    await writer.WriteAsync(betInfo.Bet, NpgsqlTypes.NpgsqlDbType.Numeric);
                    await writer.WriteAsync(betInfo.Win, NpgsqlTypes.NpgsqlDbType.Numeric);
                    await writer.WriteAsync(betInfo.Currency, NpgsqlTypes.NpgsqlDbType.Varchar);
                    await writer.WriteAsync(betInfo.Jackpot, NpgsqlTypes.NpgsqlDbType.Numeric);
                    await writer.WriteAsync(betInfo.pre_Bet, NpgsqlTypes.NpgsqlDbType.Numeric);
                    await writer.WriteAsync(betInfo.pre_Win, NpgsqlTypes.NpgsqlDbType.Numeric);
                    await writer.WriteAsync(betInfo.report_time, NpgsqlTypes.NpgsqlDbType.Timestamp);

                }

                // 完成寫入操作
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
        private async Task<int> MergeRecordFromTempTable(NpgsqlConnection conn, IDbTransaction tran, string tempTableName)
        {
            var sql = @"
                    insert into t_pp_bet_record_v2 (
                        playerid
                         ,extplayerid
                         ,gameid
                         ,playsessionid
                         ,parentsessionid
                         ,startdate
                         ,enddate
                         ,status
                         ,""type""
                         ,bet
                         ,win
                         ,currency
                         ,jackpot
                         ,pre_bet
                         ,pre_win
                        ,report_time
                        )
                        select            
                          playerid
                         ,extplayerid
                         ,gameid
                         ,playsessionid
                         ,parentsessionid
                         ,startdate
                         ,enddate
                         ,status
                         ,""type""
                         ,bet
                         ,win
                         ,currency
                         ,jackpot
                         ,pre_bet
                         ,pre_win
                        ,report_time
                        from #TempTableName tempTable
                        where not exists (
                                select null from t_pp_bet_record_v2
		                        where startdate = tempTable.startdate
		                        and  playsessionid = tempTable.playsessionid
                                and status = tempTable.status    
	                    );
                    ";

            sql = sql.Replace("#TempTableName", tempTableName);
            try
            {
                var rows = await conn.ExecuteAsync(sql, tran, commandTimeout: 60);

                return rows;
            }
            catch (Exception ex)
            {
                throw;
            }


        }

        /// <summary>
        /// 刪除
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="tran"></param>
        /// <param name="tempTableName"></param>
        /// <returns></returns>
        private async Task<int> RemovePostnextspinRecordTempTable(NpgsqlConnection conn, IDbTransaction tran, string tempTableName)
        {
            var sql = @"DROP TABLE IF EXISTS #TempTableName ;";

            sql = sql.Replace("#TempTableName", tempTableName);

            var rows = await conn.ExecuteAsync(sql, tran, commandTimeout: 60);

            return rows;
        }
        /// <summary>
        /// 取得時間區間去比對HASHset
        /// </summary>
        /// <param name="starttime"></param>
        /// <param name="endtime"></param>
        /// <returns></returns>

        public async Task<List<GetPPRecordsBySummaryReponse>> GetppV2RecordsBytime(DateTime starttime, DateTime endtime)
        {
            var sql = @"SELECT PlaySessionID,StartDate,Status
                    FROM public.t_pp_bet_record_v2
                    WHERE startdate >= @startTime 
                        AND startdate <= @endTime";
            var par = new DynamicParameters();
            par.Add("@startTime", starttime.AddMinutes(-30));
            par.Add("@endTime", endtime.AddMinutes(30));


            await using var conn = new NpgsqlConnection(await PGRead);
            var result = await conn.QueryAsync<GetPPRecordsBySummaryReponse>(sql, par);
            return result.ToList();
        }
    }
}
