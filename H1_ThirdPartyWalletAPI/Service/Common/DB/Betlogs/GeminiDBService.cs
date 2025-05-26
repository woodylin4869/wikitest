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
using ThirdPartyWallet.Share.Model.Game.Gemini.Response;

namespace H1_ThirdPartyWalletAPI.Service.Common.DB.Betlogs
{
    public interface IGeminiDBService
    {
        Task<int> PostGeminiRecord(NpgsqlConnection conn, IDbTransaction tran, IEnumerable<BetlistResponse.Datalist> betLogs);
        Task<List<BetlistResponse.W1Datalist>> GetGeminiRecordsBytime(DateTime createtime, DateTime report_time, string club_id);
        Task<List<BetlistResponse.W1Datalist>> GetGeminiRecords(string id, DateTime time);
        Task<(int totalCount, decimal totalBetValid, decimal totalWin, decimal totalnetwin)> SumGeminiBetRecordByBetTime(DateTime start, DateTime end);

        Task<int> PostGeminiRunningRecord(NpgsqlConnection conn, IDbTransaction tran, IEnumerable<BetlistResponse.Datalist> betLogs);
        Task<int> DeleteGeminiRunningRecord(NpgsqlConnection conn, IDbTransaction tran, BetlistResponse.Datalist record_data);

        Task<IEnumerable<BetlistResponse.W1Datalist>> GetGeminiRunningRecord(GetBetRecordUnsettleReq RecordReq);
        Task<List<BetlistResponse.W1Datalist>> GetGeminiRecordsBycreatetime(DateTime starttime, DateTime endtime);

        Task<IEnumerable<(int count, decimal win, decimal bet, decimal jackpot, string userid, int game_type, DateTime createtime)>>
            SummaryGameRecord(DateTime reportTime, DateTime startTime, DateTime endTime);
    }
    public class GeminiDBService : BetlogsDBServiceBase, IGeminiDBService
    {
        public GeminiDBService(ILogger<GeminiDBService> logger, IBetLogsDbConnectionStringManager connectionStringManager) : base(logger, connectionStringManager)
        {
        }
        /// <summary>
        /// 寫入資料
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="tran"></param>
        /// <param name="betLogs"></param>
        /// <returns></returns>
        public async Task<int> PostGeminiRecord(NpgsqlConnection conn, IDbTransaction tran, IEnumerable<BetlistResponse.Datalist> betLogs)
        {

            var sql = @"INSERT INTO public.t_gemini_bet_record
                    (username
                    ,billno
                    ,billstatus
                    ,grouptype
                    ,gametype
                    ,gamecode
                    ,createtime
                    ,reckontime
                    ,playtype
                    ,currency
                    ,betamount
                    ,wonamount
                    ,turnover
                    ,winlose
                    ,pre_betamount
                    ,pre_wonamount
                    ,pre_turnover
                    ,pre_winlose
                    ,report_time
                    )
                    VALUES
                    ( 
                     @username
                    ,@billno
                    ,@billstatus
                    ,@grouptype
                    ,@gametype
                    ,@gamecode
                     ,to_timestamp(@createtime/ 1000.0)
                     ,to_timestamp(@reckontime/ 1000.0)
                    ,@playtype
                    ,@currency
                    ,@betamount
                    ,@wonamount
                    ,@turnover
                    ,@winlose
                    ,@pre_betamount
                    ,@pre_wonamount
                    ,@pre_turnover
                    ,@pre_winlose
                    ,@report_time)";
            try
            {
                return await conn.ExecuteAsync(sql, betLogs, tran);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<int> PostGeminiRunningRecord(NpgsqlConnection conn, IDbTransaction tran, IEnumerable<BetlistResponse.Datalist> betLogs)
        {
            var sql = @"INSERT INTO public.t_gemini_bet_record_running
                    (username
                     ,billno
                     ,billstatus
                     ,grouptype
                     ,gametype
                     ,gamecode
                     ,createtime
                     ,reckontime
                     ,playtype
                     ,currency
                     ,betamount
                     ,wonamount
                     ,turnover
                     ,winlose
                     ,club_id
                     ,franchiser_id)
                   VALUES
                   (    
                      @username
                     ,@billno
                     ,@billstatus
                     ,@grouptype
                     ,@gametype
                     ,@gamecode
                     ,to_timestamp(@createtime/ 1000.0)
                     ,to_timestamp(@reckontime/ 1000.0)
                     ,@playtype
                     ,@currency
                     ,@betamount
                     ,@wonamount
                     ,@turnover
                     ,@winlose
                     ,@club_id
                     ,@franchiser_id)";
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
        public async Task<int> DeleteGeminiRunningRecord(NpgsqlConnection conn, IDbTransaction tran, BetlistResponse.Datalist record_data)
        {
            string strSqlDel = @"DELETE FROM t_gemini_bet_record_running
                               WHERE billno=@billno";
            return await conn.ExecuteAsync(strSqlDel, record_data, tran);
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
                        SUM(wonamount) AS win,
                        SUM(betamount) AS bet,
                        0 as jackpot,
                        username as userid, 
                        3 as game_type,
                        Date(createtime) as createtime
                        FROM t_gemini_bet_record
                        WHERE createtime BETWEEN @start_time AND @end_time
                        AND report_time = @report_time
                        GROUP BY username,Date(createtime)
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
        /// 注單號取資料
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<List<BetlistResponse.W1Datalist>> GetGeminiRecords(string id, DateTime time)
        {
            var sql = @"
                    SELECT billNo,createtime,billstatus,username,gamecode,betamount,wonamount,winlose,turnover,pre_betamount,pre_wonamount,pre_turnover,pre_winlose FROM t_gemini_bet_record
                    WHERE billno = @id and
                          createtime >= @startTime 
                        AND createtime < @endTime";

            var par = new DynamicParameters();
            par.Add("@id", id);
            par.Add("@startTime", time.AddDays(-3));
            par.Add("@endTime", time.AddDays(1));

            await using var conn = new NpgsqlConnection(await PGRead);
            var result = await conn.QueryAsync<BetlistResponse.W1Datalist>(sql, par);
            return result.ToList();
        }


        /// <summary>
        /// report_time 及時間區間
        /// </summary>
        /// <param name="createtime"></param>
        /// <param name="report_time"></param>
        /// <returns></returns>
        public async Task<List<BetlistResponse.W1Datalist>> GetGeminiRecordsBytime(DateTime createtime, DateTime report_time,string club_id)
        {
            try
            {
                var sql = @"SELECT username,billno,billstatus,createtime,reckontime,gametype,betamount,wonamount,winlose,turnover
                    FROM public.t_gemini_bet_record 
                        WHERE createtime BETWEEN @starttime AND @endtime
                        AND report_time = @reporttime
                        AND username=@club_id";
                var parameters = new DynamicParameters();
                parameters.Add("@starttime", createtime);
                parameters.Add("@endtime", createtime.AddDays(1).AddMilliseconds(-1));
                parameters.Add("@reporttime", report_time);
                parameters.Add("@club_id", Config.OneWalletAPI.Prefix_Key+ club_id);

                await using var conn = new NpgsqlConnection(await PGRead);
                var result = await conn.QueryAsync<BetlistResponse.W1Datalist>(sql, parameters);
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
        public async Task<(int totalCount, decimal totalBetValid, decimal totalWin,decimal totalnetwin)> SumGeminiBetRecordByBetTime(DateTime start, DateTime end)
        {
            var sql = @"SELECT 
                    COUNT(billno) AS totalCount
                    , CASE WHEN SUM(betamount) IS NULL THEN 0 ELSE SUM(betamount) END  AS totalBetValid
                    , CASE WHEN SUM(wonamount) IS NULL THEN 0 ELSE SUM(wonamount) END AS totalWin
                    , CASE WHEN SUM(winlose) IS NULL THEN 0 ELSE SUM(winlose) END AS totalnetwin
                    FROM t_gemini_bet_record
                    WHERE createtime >= @startTime 
                        AND createtime < @endTime";

            var par = new DynamicParameters();
            par.Add("@startTime", start);
            par.Add("@endTime", end.AddMilliseconds(-1));

            await using var conn = new NpgsqlConnection(await PGRead);
            var result = (await conn.QueryAsync<dynamic>(sql, par)).FirstOrDefault();
            return ((int)result.totalcount, (decimal)result.totalbetvalid, (decimal)result.totalwin, (decimal)result.totalnetwin);
        }


        /// <summary>
        /// 取得createtime 區間 注單
        /// </summary>
        /// <param name="starttime"></param>
        /// <param name="endtime"></param>
        /// <returns></returns>
        public async Task<List<BetlistResponse.W1Datalist>> GetGeminiRecordsBycreatetime(DateTime starttime, DateTime endtime)
        {
            try
            {
                var sql = @"SELECT billNo,billstatus
                    FROM public.t_gemini_bet_record 
                        WHERE createtime BETWEEN @starttime AND @endtime
                       ";
                var parameters = new DynamicParameters();
                parameters.Add("@starttime", starttime);
                parameters.Add("@endtime", endtime);


                await using var conn = new NpgsqlConnection(await PGRead);
                var result = await conn.QueryAsync<BetlistResponse.W1Datalist>(sql, parameters);
                return result.ToList();
            }
            catch (Exception ex)
            {
                throw;
            }
        }


        public async Task<IEnumerable<BetlistResponse.W1Datalist>> GetGeminiRunningRecord(GetBetRecordUnsettleReq RecordReq)
        {
            var par = new DynamicParameters();
            string strSql = @"SELECT *
                    FROM t_gemini_bet_record_running
                    WHERE createtime BETWEEN @StartTime AND @EndTime
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
                return await conn.QueryAsync<BetlistResponse.W1Datalist>(strSql, par);
            }
        }

       
       
    }
}
