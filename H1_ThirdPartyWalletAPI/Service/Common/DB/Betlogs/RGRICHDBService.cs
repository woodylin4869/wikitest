using Dapper;
using H1_ThirdPartyWalletAPI.Model.Config;
using H1_ThirdPartyWalletAPI.Model.DataModel;
using H1_ThirdPartyWalletAPI.Model.Game.WS168.Response;
using H1_ThirdPartyWalletAPI.Model.W1API;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using H1_ThirdPartyWalletAPI.Utility;
using ThirdPartyWallet.Share.Model.Game.RGRICH.Response;

namespace H1_ThirdPartyWalletAPI.Service.Common.DB.Betlogs
{
    public interface IRGRICHDBService
    {
        Task<int> PostRGRICHRecord(NpgsqlConnection conn, IDbTransaction tran, IEnumerable<BetRecordResponse> betLogs);

        Task<List<BetRecordResponse>> GetRGRICHRecordsBytime(DateTime partitionTime, DateTime report_time, string gamePlatformUserId);

        Task<List<BetRecordResponse>> GetRGRICHRecords(string id, DateTime time);

        Task<(int TotalCount, decimal TotalBetValid, decimal TotalNetWin)> SumRGRICHBetRecordByPartitionTime(DateTime start, DateTime end);

        Task<int> PostRGRICHRunningRecord(NpgsqlConnection conn, IDbTransaction tran, IEnumerable<BetRecordResponse> betLogs);

        Task<int> DeleteRGRICHRunningRecord(NpgsqlConnection conn, IDbTransaction tran, BetRecordResponse record_data);

        Task<IEnumerable<BetRecordResponse>> GetRGRICHRunningRecord(GetBetRecordUnsettleReq RecordReq);

        Task<List<BetRecordResponse>> GetRGRICHRecordsByPartition(DateTime starttime, DateTime endtime);

        Task<IEnumerable<(int count, decimal netWin, decimal bet, decimal betValidBet, decimal jackpot, string userid, int game_type, DateTime partitionTime)>>
            SummaryGameRecord(DateTime reportTime, DateTime startTime, DateTime endTime);
    }

    public class RGRICHDBService : BetlogsDBServiceBase, IRGRICHDBService
    {
        public RGRICHDBService(ILogger<RGRICHDBService> logger, IBetLogsDbConnectionStringManager connectionStringManager) : base(logger, connectionStringManager)
        {
        }

        /// <summary>
        /// 寫入資料
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="tran"></param>
        /// <param name="betLogs"></param>
        /// <returns></returns>
        public async Task<int> PostRGRICHRecord(NpgsqlConnection conn, IDbTransaction tran, IEnumerable<BetRecordResponse> betLogs)
        {
            var sql = @"INSERT INTO t_rgrich_bet_record (
	                        id
	                        ,uid
	                        ,username
	                        ,game_type
	                        ,game_code
	                        ,bet_no
	                        ,bet_total
	                        ,bet_real
	                        ,payoff
	                        ,jackpot
	                        ,jackpot_contribute
	                        ,STATUS
	                        ,bet_time
	                        ,payout_time
	                        ,created_at
	                        ,updated_at
	                        ,pre_bet_total
	                        ,pre_bet_real
	                        ,pre_payoff
	                        ,report_time
	                        ,partition_time
	                        )
                        VALUES (
	                        @Id
	                        ,@Uid
	                        ,@Username
	                        ,@Game_type
	                        ,@Game_code
	                        ,@Bet_no
	                        ,@Bet_total
	                        ,@Bet_real
	                        ,@Payoff
	                        ,@Jackpot
	                        ,@Jackpot_contribute
	                        ,@Status
	                        ,@Bet_time
	                        ,@Payout_time
	                        ,@Created_at
	                        ,@Updated_at
	                        ,@Pre_Bet_total
	                        ,@Pre_Bet_real
	                        ,@Pre_Payoff
	                        ,@Report_time
	                        ,@Bet_time
	                        );
";
            try
            {
                return await conn.ExecuteAsync(sql, betLogs, tran);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<int> PostRGRICHRunningRecord(NpgsqlConnection conn, IDbTransaction tran, IEnumerable<BetRecordResponse> betLogs)
        {
            var sql = @"INSERT INTO t_rgrich_bet_record_running (
	                        id
	                        ,uid
	                        ,username
	                        ,game_type
	                        ,game_code
	                        ,bet_no
	                        ,bet_total
	                        ,bet_real
	                        ,payoff
	                        ,jackpot
	                        ,jackpot_contribute
	                        ,STATUS
	                        ,bet_time
	                        ,payout_time
	                        ,created_at
	                        ,updated_at
	                        ,club_id
	                        ,franchiser_id
	                        )
                        VALUES (
	                        @Id
	                        ,@Uid
	                        ,@Username
	                        ,@Game_type
	                        ,@Game_code
	                        ,@Bet_no
	                        ,@Bet_total
	                        ,@Bet_real
	                        ,@Payoff
	                        ,@Jackpot
	                        ,@Jackpot_contribute
	                        ,@Status
	                        ,@Bet_time
	                        ,@Payout_time
	                        ,@Created_at
	                        ,@Updated_at
	                        ,@Club_id
	                        ,@Franchiser_id
	                        )";
            try
            {
                return await conn.ExecuteAsync(sql, betLogs, tran);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        /// <summary>
        /// 刪除未結算
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="tran"></param>
        /// <param name="record_data"></param>
        /// <returns></returns>
        public async Task<int> DeleteRGRICHRunningRecord(NpgsqlConnection conn, IDbTransaction tran, BetRecordResponse record_data)
        {
            string strSqlDel = @"DELETE FROM t_rgrich_bet_record_running
                               WHERE bet_no=@bet_no";
            return await conn.ExecuteAsync(strSqlDel, record_data, tran);
        }

        /// <summary>
        /// 五分鐘會總
        /// </summary>
        /// <param name="reportTime"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        public async Task<IEnumerable<(int count, decimal netWin, decimal bet, decimal betValidBet, decimal jackpot, string userid, int game_type, DateTime partitionTime)>> SummaryGameRecord(DateTime reportTime, DateTime startTime, DateTime endTime)
        {
            var sql = @"SELECT CAST(count(1) AS INTEGER) AS count,
                        coalesce(SUM(payoff),0) AS netWin,
                        coalesce(SUM(bet_total),0) AS bet,
                        coalesce(SUM(bet_real),0) AS betValidBet,
                        0 as jackpot,
                        username as userid,
                        3 as game_type,
                        Date(partition_time) as partition_time
                        FROM t_rgrich_bet_record
                        WHERE partition_time BETWEEN @start_time AND @end_time
                        AND report_time = @report_time
                        GROUP BY username,Date(partition_time)
                        ";

            var par = new DynamicParameters();
            par.Add("@start_time", startTime.AddDays(-2));
            par.Add("@end_time", endTime);
            par.Add("@report_time", reportTime);
            using (NpgsqlConnection conn = new NpgsqlConnection(await PGRead))
            {
                var result = await conn.QueryAsync(sql, par, commandTimeout: 270);
                var a = result.Select(x => ((int)x.count, (decimal)x.netwin, (decimal)x.bet, (decimal)x.betvalidbet, (decimal)x.jackpot, (string)x.userid, (int)x.game_type, (DateTime)x.partition_time)).ToList();
                return a;
            }
        }

        /// <summary>
        /// 注單號取資料
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<List<BetRecordResponse>> GetRGRICHRecords(string id, DateTime time)
        {
            // TODO:問問先進
            var sql = @"
                    SELECT id
	                    ,uid
	                    ,username
	                    ,game_type
	                    ,game_code
	                    ,bet_no
	                    ,bet_total
	                    ,bet_real
	                    ,payoff
	                    ,jackpot
	                    ,jackpot_contribute
	                    ,STATUS
	                    ,bet_time
	                    ,payout_time
	                    ,created_at
	                    ,updated_at
	                    ,pre_bet_total
	                    ,pre_bet_real
	                    ,pre_payoff
	                    ,report_time
	                    ,partition_time
                    FROM t_rgrich_bet_record
                    WHERE bet_no = @id and
                          partition_time >= @startTime
                        AND partition_time < @endTime";

            var par = new DynamicParameters();
            par.Add("@id", id);
            par.Add("@startTime", time.AddDays(-3));
            par.Add("@endTime", time.AddDays(1));

            await using var conn = new NpgsqlConnection(await PGRead);
            var result = await conn.QueryAsync<BetRecordResponse>(sql, par);
            return result.ToList();
        }

        /// <summary>
        /// 查詢第2層明細(5分鐘彙總帳)需要資訊
        /// </summary>
        /// <param name="partitionTime">BetTime</param>
        /// <param name="report_time">ReportTime</param>
        /// <param name="gamePlatformUserId">遊戲商使用者ID</param>
        /// <returns></returns>
        public async Task<List<BetRecordResponse>> GetRGRICHRecordsBytime(DateTime partitionTime, DateTime report_time, string gamePlatformUserId)
        {
            try
            {
                var sql = @"SELECT
                             bet_no
                            ,bet_time
                            ,game_type
	                        ,game_code
	                        ,bet_total
	                        ,bet_real
	                        ,payoff
	                        ,jackpot
	                        ,STATUS
	                        ,payout_time
                        FROM public.t_rgrich_bet_record
                            WHERE partition_time BETWEEN @starttime AND @endtime
                            AND report_time = @reporttime
                            AND username = @username ";
                var parameters = new DynamicParameters();
                parameters.Add("@starttime", partitionTime);
                parameters.Add("@endtime", partitionTime.AddDays(1).AddMilliseconds(-1));
                parameters.Add("@reporttime", report_time);
                parameters.Add("@username", gamePlatformUserId);

                await using var conn = new NpgsqlConnection(await PGRead);
                var result = await conn.QueryAsync<BetRecordResponse>(sql, parameters);
                return result.ToList();
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        /// <summary>
        /// 每小時匯總
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public async Task<(int TotalCount, decimal TotalBetValid, decimal TotalNetWin)> SumRGRICHBetRecordByPartitionTime(DateTime start, DateTime end)
        {
            var sql = @"SELECT
                    COUNT(1) AS totalcount
                    , coalesce(SUM(bet_real),0) AS totalbetvalid
                    , coalesce(SUM(payoff),0) AS totalnetwin
                    FROM t_rgrich_bet_record
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
        public async Task<List<BetRecordResponse>> GetRGRICHRecordsByPartition(DateTime starttime, DateTime endtime)
        {
            try
            {
                var sql = @"SELECT bet_no,status,updated_at
                    FROM public.t_rgrich_bet_record
                        WHERE partition_time BETWEEN @starttime AND @endtime
                       ";
                var parameters = new DynamicParameters();
                parameters.Add("@starttime", starttime);
                parameters.Add("@endtime", endtime);

                await using var conn = new NpgsqlConnection(await PGRead);
                var result = await conn.QueryAsync<BetRecordResponse>(sql, parameters);
                return result.ToList();
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<IEnumerable<BetRecordResponse>> GetRGRICHRunningRecord(GetBetRecordUnsettleReq RecordReq)
        {
            var par = new DynamicParameters();
            string strSql = @"SELECT id
	                                ,uid
	                                ,username
	                                ,game_type
	                                ,game_code
	                                ,bet_no
	                                ,bet_total
	                                ,bet_real
	                                ,payoff
	                                ,jackpot
	                                ,jackpot_contribute
	                                ,STATUS
	                                ,bet_time
	                                ,payout_time
	                                ,created_at
	                                ,updated_at
	                                ,club_id
	                                ,franchiser_id
	                                ,create_time
                                FROM PUBLIC.t_rgrich_bet_record_running
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
                return await conn.QueryAsync<BetRecordResponse>(strSql, par);
            }
        }
    }
}