using Dapper;
using H1_ThirdPartyWalletAPI.Model.W1API;
using H1_ThirdPartyWalletAPI.Utility;
using Microsoft.Extensions.Logging;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using ThirdPartyWallet.Share.Model.Game.CR.Response;

namespace H1_ThirdPartyWalletAPI.Service.Common.DB.Betlogs
{
    public interface ICRDBService
    {
        Task<int> PostCRRecord(NpgsqlConnection conn, IDbTransaction tran, IEnumerable<Wager_Data> betLogs);

        Task<List<Wager_Data>> GetCRRecordsBytime(DateTime partitionTime, DateTime report_time, string gamePlatformUserId);

        Task<List<Wager_Data>> GetCRRecords(string id, DateTime time);

        Task<(int TotalCount, decimal TotalBetValid, decimal TotalNetWin)> SumCRBetRecordByPartitionTime(DateTime start, DateTime end);

        Task<int> PostCRRunningRecord(NpgsqlConnection conn, IDbTransaction tran, IEnumerable<Wager_Data> betLogs);

        Task<int> DeleteCRRunningRecord(NpgsqlConnection conn, IDbTransaction tran, Wager_Data record_data);

        Task<IEnumerable<Wager_Data>> GetCRRunningRecord(GetBetRecordUnsettleReq RecordReq);

        Task<List<Wager_Data>> GetCRRecordsByPartition(DateTime starttime, DateTime endtime);

        Task<IEnumerable<(int count, decimal netWin, decimal bet, decimal betValidBet, decimal jackpot, string userid, int game_type, DateTime partitionTime)>>
            SummaryGameRecord(DateTime reportTime, DateTime startTime, DateTime endTime);
    }

    public class CRDBService : BetlogsDBServiceBase, ICRDBService
    {
        public CRDBService(ILogger<CRDBService> logger, IBetLogsDbConnectionStringManager connectionStringManager) : base(logger, connectionStringManager)
        {
        }

        /// <summary>
        /// 寫入資料
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="tran"></param>
        /// <param name="betLogs"></param>
        /// <returns></returns>
        public async Task<int> PostCRRecord(NpgsqlConnection conn, IDbTransaction tran, IEnumerable<Wager_Data> betLogs)
        {
            var sql = @"INSERT INTO public.t_cr_bet_record
                        (adddate, cashoutid, cashout, cashout_d, currency, degold, degold_d, gold, gold_d, gtype, handicap, id, ip, ioratio, league, mid, oddsformat, odds, orderdate, ""order"", ordertime, pname, report_test, rtype, rtypecode, score, settle, strong, tname_away, tname_home, username, vgold, members_vgold, resultdetail, ""result"", resultdate, result_score, wtype, wtypecode, wingold_d, wingold, report_time, partition_time, pre_gold, pre_degold, pre_wingold,create_time)
                 VALUES(@adddate, @cashoutid, @cashout, @cashout_d, @currency, @degold, @degold_d, @gold, @gold_d, @gtype, @handicap, @id, @ip, @ioratio, @league, @mid, @oddsformat, @odds, @orderdate, @order, @ordertime, @pname, @report_test, @rtype, @rtypecode, @score, @settle, @strong, @tname_away, @tname_home, @username, @vgold, @members_vgold, @resultdetail, @result, @resultdate, @result_score, @wtype, @wtypecode, @wingold_d, @wingold, @report_time, @adddate, @pre_gold, @pre_degold, @pre_wingold,@create_time);
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

        public async Task<int> PostCRRunningRecord(NpgsqlConnection conn, IDbTransaction tran, IEnumerable<Wager_Data> betLogs)
        {
            var sql = @"INSERT INTO public.t_cr_bet_record_running
(adddate, cashoutid, cashout, cashout_d, currency, degold, degold_d, gold, gold_d, gtype, handicap, id, ip, ioratio, league, mid, oddsformat, odds, orderdate, ""order"", ordertime, pname, report_test, rtype, rtypecode, score, settle, strong, tname_away, tname_home, username, vgold, members_vgold, resultdetail, ""result"", resultdate, result_score, wtype, wtypecode, wingold_d, wingold,   pre_gold, pre_degold, pre_wingold, club_id, franchiser_id,create_time)
VALUES(@adddate, @cashoutid, @cashout, @cashout_d, @currency, @degold, @degold_d, @gold, @gold_d, @gtype, @handicap, @id, @ip, @ioratio, @league, @mid, @oddsformat, @odds, @orderdate, @order, @ordertime, @pname, @report_test, @rtype, @rtypecode, @score, @settle, @strong, @tname_away, @tname_home, @username, @vgold, @members_vgold, @resultdetail, @result, @resultdate, @result_score, @wtype, @wtypecode, @wingold_d, @wingold,   @pre_gold, @pre_degold, @pre_wingold, @club_id, @franchiser_id,@create_time);";
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
        public async Task<int> DeleteCRRunningRecord(NpgsqlConnection conn, IDbTransaction tran, Wager_Data record_data)
        {
            string strSqlDel = @"DELETE FROM t_cr_bet_record_running
                               WHERE id=@id";
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
                        coalesce(SUM(wingold-degold),0) AS netWin,
                        coalesce(SUM(gold),0) AS bet,
                        coalesce(SUM(degold),0) AS betValidBet,
                        0 as jackpot,
                        username as userid,
                        0 as game_type,
                        Date(partition_time) as partition_time
                        FROM t_cr_bet_record
                        WHERE partition_time BETWEEN @start_time AND @end_time
                        AND report_time = @report_time
                        GROUP BY username,Date(partition_time)";

            var par = new DynamicParameters();
            par.Add("@start_time", startTime);
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
        /// 沖銷訂正注單號取資料
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<List<Wager_Data>> GetCRRecords(string id, DateTime time)
        {
            // TODO:問問先進
            var sql = @"SELECT adddate
                              ,cashoutid
                              ,cashout
                              ,cashout_d
                              ,currency
                              ,degold
                              ,degold_d
                              ,gold
                              ,gold_d
                              ,gtype
                              ,handicap
                              ,id
                              ,ip
                              ,ioratio
                              ,league
                              ,mid
                              ,oddsformat
                              ,odds
                              ,orderdate
                              ,""order""
                              ,ordertime
                              ,pname
                              ,report_test
                              ,rtype
                              ,rtypecode
                              ,score
                              ,settle
                              ,strong
                              ,tname_away
                              ,tname_home
                              ,username
                              ,vgold
                              ,members_vgold
                              ,resultdetail
                              ,""result""
                              ,resultdate
                              ,result_score
                              ,wtype
                              ,wtypecode
                              ,wingold_d
                              ,wingold
                              ,report_time
                              ,partition_time
                              ,pre_gold
                              ,pre_degold
                              ,pre_wingold
                    FROM t_cr_bet_record
                    WHERE id = @id and
                          partition_time >= @startTime
                        AND partition_time < @endTime";

            var par = new DynamicParameters();
            par.Add("@id", id);
            par.Add("@startTime", time.AddDays(-3));
            par.Add("@endTime", time.AddDays(1));

            await using var conn = new NpgsqlConnection(await PGRead);
            var result = await conn.QueryAsync<Wager_Data>(sql, par);
            return result.ToList();
        }

        /// <summary>
        /// 查詢第2層明細(5分鐘彙總帳)需要資訊
        /// </summary>
        /// <param name="partitionTime">BetTime</param>
        /// <param name="report_time">ReportTime</param>
        /// <param name="gamePlatformUserId">遊戲商使用者ID</param>
        /// <returns></returns>
        public async Task<List<Wager_Data>> GetCRRecordsBytime(DateTime partitionTime, DateTime report_time, string gamePlatformUserId)
        {
            try
            {
                var sql = @"SELECT
                             id
                            ,adddate
                            ,gtype
	                        ,gold
	                        ,degold
	                        ,wingold
                            ,resultdate
	                        ,league
	                        ,tname_away
                            ,tname_home
	                        ,""order""
                            ,ioratio
                            ,odds
                            ,result
                        FROM public.t_cr_bet_record
                            WHERE partition_time BETWEEN @starttime AND @endtime
                            AND report_time = @reporttime
                            AND upper(username) = @username ";
                var parameters = new DynamicParameters();
                parameters.Add("@starttime", partitionTime);
                parameters.Add("@endtime", partitionTime.AddDays(1).AddMilliseconds(-1));
                parameters.Add("@reporttime", report_time);
                parameters.Add("@username", gamePlatformUserId);

                await using var conn = new NpgsqlConnection(await PGRead);
                var result = await conn.QueryAsync<Wager_Data>(sql, parameters);
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
        public async Task<(int TotalCount, decimal TotalBetValid, decimal TotalNetWin)> SumCRBetRecordByPartitionTime(DateTime start, DateTime end)
        {
            var sql = @"SELECT
                    COUNT(1) AS totalcount
                    , coalesce(SUM(degold),0) AS totalbetvalid
                    , coalesce(SUM(wingold-degold),0) AS totalnetwin
                    FROM t_cr_bet_record
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
        public async Task<List<Wager_Data>> GetCRRecordsByPartition(DateTime starttime, DateTime endtime)
        {
            try
            {
                var sql = @"SELECT id, cashoutid,result, adddate, resultdate
                    FROM public.t_cr_bet_record
                        WHERE resultdate BETWEEN @starttime AND @endtime
                       ";
                var parameters = new DynamicParameters();
                parameters.Add("@starttime", starttime);
                parameters.Add("@endtime", endtime);

                await using var conn = new NpgsqlConnection(await PGRead);
                var result = await conn.QueryAsync<Wager_Data>(sql, parameters);
                return result.ToList();
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<IEnumerable<Wager_Data>> GetCRRunningRecord(GetBetRecordUnsettleReq RecordReq)
        {
            var par = new DynamicParameters();
            string strSql = @"SELECT adddate
                              ,cashoutid
                              ,cashout
                              ,cashout_d
                              ,currency
                              ,degold
                              ,degold_d
                              ,gold
                              ,gold_d
                              ,gtype
                              ,handicap
                              ,id
                              ,ip
                              ,ioratio
                              ,league
                              ,mid
                              ,oddsformat
                              ,odds
                              ,orderdate
                              ,""order""
                              ,ordertime
                              ,pname
                              ,report_test
                              ,rtype
                              ,rtypecode
                              ,score
                              ,settle
                              ,strong
                              ,tname_away
                              ,tname_home
                              ,username
                              ,vgold
                              ,members_vgold
                              ,resultdetail
                              ,""result""
                              ,resultdate
                              ,result_score
                              ,wtype
                              ,wtypecode
                              ,wingold_d
                              ,wingold
                              ,pre_gold
                              ,pre_degold
                              ,pre_wingold
                              ,club_id
                              ,franchiser_id
                              ,create_time
                    FROM PUBLIC.t_cr_bet_record_running
                    WHERE adddate BETWEEN @StartTime AND @EndTime";

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
                return await conn.QueryAsync<Wager_Data>(strSql, par);
            }
        }
    }
}