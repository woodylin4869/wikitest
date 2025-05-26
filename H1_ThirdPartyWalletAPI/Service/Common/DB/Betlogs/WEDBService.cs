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
using ThirdPartyWallet.Share.Model.Game.WE.Response;

namespace H1_ThirdPartyWalletAPI.Service.Common.DB.Betlogs
{
    public interface IWEDBService
    {
        Task<int> PostWERecord(NpgsqlConnection conn, IDbTransaction tran, IEnumerable<BetRecordResponse.Datum> betLogs);
        Task<List<BetRecordResponse.W1Datum>> GetWERecordsBytime(DateTime createtime, DateTime report_time, string club_id, int gametype);
        Task<List<BetRecordResponse.W1Datum>> GetWERecords(string id, DateTime time);
        Task<(int totalCount, decimal totalBetValid, decimal totalnetwin)> SumWEBetRecordByBetTime(DateTime start, DateTime end);
        Task<int> PostWERunningRecord(NpgsqlConnection conn, IDbTransaction tran, IEnumerable<BetRecordResponse.Datum> betLogs);
        Task<int> DeleteWERunningRecord(NpgsqlConnection conn, IDbTransaction tran, BetRecordResponse.Datum record_data);

        //Task<IEnumerable<BetlistResponse.W1Datalist>> GetWERunningRecord(GetBetRecordUnsettleReq RecordReq);
        Task<List<BetRecordResponse.W1Datum>> GetWERecordsBycreatetime(DateTime starttime, DateTime endtime);

        Task<IEnumerable<(int count, decimal win, decimal bet, decimal jackpot, string userid, int game_type, DateTime partitionTime, decimal turnover)>>
            SummaryGameRecord(DateTime reportTime, DateTime startTime, DateTime endTime);
    }
    public class WEDBService : BetlogsDBServiceBase, IWEDBService
    {
        public WEDBService(ILogger<WEDBService> logger, IBetLogsDbConnectionStringManager connectionStringManager) : base(logger, connectionStringManager)
        {
        }
        /// <summary>
        /// 寫入資料
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="tran"></param>
        /// <param name="betLogs"></param>
        /// <returns></returns>
        public async Task<int> PostWERecord(NpgsqlConnection conn, IDbTransaction tran, IEnumerable<BetRecordResponse.Datum> betLogs)
        {

            var sql = @"INSERT INTO public.t_we_record
                    (      betid
                          ,operatorid
                          ,playerid
                          ,weplayerid
                          ,betdatetime
                          ,settlementtime
                          ,betstatus
                          ,odds
                          ,betcode
                          ,validbetamount
                          ,gameresult
                          ,device
                          ,betamount
                          ,winlossamount
                          ,category
                          ,gametype
                          ,gameroundid
                          ,tableid
                          ,ip
                          ,trackid
                          ,resettletime
                          ,tipamount
                          ,type
                          ,report_time
                          ,partition_time
                          ,pre_bet
                          ,pre_win
                          ,pre_netwin
                          ,groupgametype
                          ,groupgametype_id
                    )
                    VALUES
                    ( 
                           @betid
                          ,@operatorid
                          ,@playerid
                          ,@weplayerid
                          ,to_timestamp(@betdatetime)
                          ,to_timestamp(@settlementtime)
                          ,@betstatus
                          ,@odds
                          ,@betcode
                          ,@validbetamount
                          ,@gameresult
                          ,@device
                          ,@betamount
                          ,@winlossamount
                          ,@category
                          ,@gametype
                          ,@gameroundid
                          ,@tableid
                          ,@ip
                          ,@trackid
                          ,to_timestamp(@resettletime)
                          ,@tipamount
                          ,@type
                     ,@report_time
                     ,to_timestamp(@betdatetime)
                     ,@pre_bet
                     ,@pre_win
                     ,@pre_netwin
                     ,@groupgametype
                        ,@groupgametype_id          
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

        public async Task<int> PostWERunningRecord(NpgsqlConnection conn, IDbTransaction tran, IEnumerable<BetRecordResponse.Datum> betLogs)
        {
            var sql = @"INSERT INTO public.t_we_record_running
                    (      betid
                          ,operatorid
                          ,playerid
                          ,weplayerid
                          ,betdatetime
                          ,settlementtime
                          ,betstatus
                          ,odds
                          ,betcode
                          ,validbetamount
                          ,gameresult
                          ,device
                          ,betamount
                          ,winlossamount
                          ,category
                          ,gametype
                          ,gameroundid
                          ,tableid
                          ,ip
                          ,trackid
                          ,resettletime
                          ,tipamount
                          ,type
                          ,club_id
                          ,franchiser_id)
                    VALUES
                    ( 
                           @betid
                          ,@operatorid
                          ,@playerid
                          ,@weplayerid
                          ,to_timestamp(@betdatetime)
                          ,to_timestamp(@settlementtime)
                          ,@betstatus
                          ,@odds
                          ,@betcode
                          ,@validbetamount
                          ,@gameresult
                          ,@device
                          ,@betamount
                          ,@winlossamount
                          ,@category
                          ,@gametype
                          ,@gameroundid
                          ,@tableid
                          ,@ip
                          ,@trackid
                          ,to_timestamp(@resettletime)
                          ,@tipamount
                          ,@type
                          ,@club_id
                          ,@franchiser_id
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
        public async Task<int> DeleteWERunningRecord(NpgsqlConnection conn, IDbTransaction tran, BetRecordResponse.Datum record_data)
        {
            string strSqlDel = @"DELETE FROM t_we_record_running
                               WHERE betid=@betid";
            return await conn.ExecuteAsync(strSqlDel, record_data, tran);
        }
        /// <summary>
        /// 五分鐘會總
        /// </summary>
        /// <param name="reportTime"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        public async Task<IEnumerable<(int count, decimal win, decimal bet, decimal jackpot, string userid, int game_type, DateTime partitionTime,decimal turnover)>> SummaryGameRecord(DateTime reportTime, DateTime startTime, DateTime endTime)
        {
            var sql = @"SELECT CAST(count(1) AS INTEGER) AS count,
                        SUM(winlossamount) AS win,
                        SUM(betamount) AS bet,
                        0 as jackpot,
                        playerid as userid, 
                        groupgametype_id as game_type,
                        Date(partition_time)  as partitionTime,
                        SUM(validbetamount) as turnover
                        FROM t_we_record
                        WHERE partition_time BETWEEN @start_time AND @end_time
                        AND report_time = @report_time
                        GROUP BY playerid,Date(partition_time),groupgametype_id
                        ";

            var par = new DynamicParameters();
            par.Add("@start_time", startTime);
            par.Add("@end_time", endTime);
            par.Add("@report_time", reportTime);
            using (NpgsqlConnection conn = new NpgsqlConnection(await PGRead))
            {
                var result = await conn.QueryAsync(sql, par, commandTimeout: 270);
                var a = result.Select(x => ((int)x.count, (decimal)x.win, (decimal)x.bet, (decimal)x.jackpot, (string)x.userid, (int)x.game_type, (DateTime)x.partitiontime,(decimal)x.turnover)).ToList();
                return a;
            }
        }

        /// <summary>
        /// 注單號取資料
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<List<BetRecordResponse.W1Datum>> GetWERecords(string id, DateTime time)
        {
            var sql = @"
                    SELECT betid,betdatetime,betstatus,resettleTime,pre_bet,pre_netwin
                    FROM t_we_record
                    WHERE betid = @id and
                          partition_time >= @starttime
                        AND partition_time < @endtime";

            var par = new DynamicParameters();
            par.Add("@id", id);
            par.Add("@starttime", time.AddDays(-3));
            par.Add("@endtime", time.AddDays(1));

            await using var conn = new NpgsqlConnection(await PGRead);

            var result = await conn.QueryAsync<BetRecordResponse.W1Datum>(sql, par);
            return result.ToList();

        }


        /// <summary>
        /// report_time 及時間區間
        /// </summary>
        /// <param name="createtime"></param>
        /// <param name="report_time"></param>
        /// <returns></returns>
        public async Task<List<BetRecordResponse.W1Datum>> GetWERecordsBytime(DateTime createtime, DateTime report_time, string club_id,int gametype)
        {
            try
            {
                var sql = @"SELECT betid
                                  ,gametype
                                  ,playerid
                                  ,betdatetime
                                  ,settlementtime
                                  ,betamount
                                  ,winlossamount
                                  ,betstatus
                                  ,category
                                  ,tableid
                    FROM public.t_we_record 
                        WHERE partition_time BETWEEN @starttime AND @endtime
                        AND report_time = @reporttime
                        AND playerid=@club_id
                        AND groupgametype_id = @groupgametype_id";
                var parameters = new DynamicParameters();
                parameters.Add("@starttime", createtime);
                parameters.Add("@endtime", createtime.AddDays(1).AddSeconds(-1));
                parameters.Add("@reporttime", report_time);
                parameters.Add("@club_id", Config.OneWalletAPI.Prefix_Key + club_id);
                parameters.Add("@groupgametype_id", gametype);


                await using var conn = new NpgsqlConnection(await PGRead);
                var result = await conn.QueryAsync<BetRecordResponse.W1Datum>(sql, parameters);
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
        public async Task<(int totalCount, decimal totalBetValid, decimal totalnetwin)> SumWEBetRecordByBetTime(DateTime start, DateTime end)
        {
            var sql = @"SELECT 
                    COUNT(betid) AS totalCount
                    , CASE WHEN SUM(betamount) IS NULL THEN 0 ELSE SUM(betamount) END  AS totalBetValid
                    , CASE WHEN SUM(winlossamount) IS NULL THEN 0 ELSE SUM(winlossamount) END AS totalnetwin
                    FROM t_we_record
                    WHERE partition_time >= @startTime 
                        AND partition_time < @endTime";

            var par = new DynamicParameters();
            par.Add("@startTime", start);
            par.Add("@endTime", end.AddSeconds(-1));

            await using var conn = new NpgsqlConnection(await PGRead);
            var result = (await conn.QueryAsync<dynamic>(sql, par)).FirstOrDefault();
            return ((int)result.totalcount, (decimal)result.totalbetvalid, (decimal)result.totalnetwin);
        }


        /// <summary>
        /// 取得createtime 區間 注單
        /// </summary>
        /// <param name="starttime"></param>
        /// <param name="endtime"></param>
        /// <returns></returns>
        public async Task<List<BetRecordResponse.W1Datum>> GetWERecordsBycreatetime(DateTime starttime, DateTime endtime)
        {
            try
            {
                var sql = @"SELECT betid,betstatus,resettletime
                    FROM public.t_we_record 
                        WHERE partition_time BETWEEN @starttime AND @endtime
                       ";
                var parameters = new DynamicParameters();
                parameters.Add("@starttime", starttime);
                parameters.Add("@endtime", endtime);


                await using var conn = new NpgsqlConnection(await PGRead);
                var result = await conn.QueryAsync<BetRecordResponse.W1Datum>(sql, parameters);
                return result.ToList();
            }
            catch (Exception ex)
            {
                throw;
            }
        }


        //public async Task<IEnumerable<BetlistResponse.W1Datalist>> GetWERunningRecord(GetBetRecordUnsettleReq RecordReq)
        //{
        //    var par = new DynamicParameters();
        //    string strSql = @"SELECT *
        //            FROM t_WE_bet_record_running
        //            WHERE createtime BETWEEN @StartTime AND @EndTime
        //            ";

        //    if (RecordReq.Club_id != null)
        //    {
        //        par.Add("@Club_id", RecordReq.Club_id);
        //        strSql += " AND Club_id = @Club_id";
        //    }
        //    if (RecordReq.Franchiser_id != null)
        //    {
        //        par.Add("@Franchiser_id", RecordReq.Franchiser_id);
        //        strSql += " AND Franchiser_id = @Franchiser_id";
        //    }
        //    par.Add("@StartTime", RecordReq.StartTime != null ? RecordReq.StartTime : DateTime.Now.AddDays(-100));
        //    par.Add("@EndTime", RecordReq.EndTime != null ? RecordReq.EndTime : DateTime.Now);
        //    using (NpgsqlConnection conn = new NpgsqlConnection(await PGRead))
        //    {
        //        return await conn.QueryAsync<BetlistResponse.W1Datalist>(strSql, par);
        //    }
        //}



    }
}
