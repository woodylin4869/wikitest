using Dapper;
using H1_ThirdPartyWalletAPI.Model.Config;
using H1_ThirdPartyWalletAPI.Model.DataModel;
using H1_ThirdPartyWalletAPI.Model.Game.WM.Response;
using H1_ThirdPartyWalletAPI.Model.W1API;
using H1_ThirdPartyWalletAPI.Utility;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;
using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using ThirdPartyWallet.Share.Model.Game.PS.Response;

namespace H1_ThirdPartyWalletAPI.Service.Common.DB.Betlogs
{
    public interface IWMDBService
    {
        Task<int> PostWMRecord(NpgsqlConnection conn, IDbTransaction tran, IEnumerable<WMDataReportResponse.Result> betLogs);
        Task<List<WMDataReportResponse.Result>> GetWMRecordsBytime(DateTime start, DateTime end);
        Task<List<WMDataReportResponse.Result>> GetWMRecords(string id, DateTime bettime);
        Task<List<WMDataReportResponse.Result>> GetWMRecordsV2(string id, DateTime bettime);
        Task<List<WMDataReportResponse.Result>> GetWMRecords(IDbTransaction tran, string id, DateTime bettime);
        Task<List<WMDataReportResponse.Result>> GetWMRecordsV2(IDbTransaction tran, string id, DateTime bettime);
        Task<(int totalCount, decimal totalBetValid, decimal totalWin)> SumWMBetRecordByBetTime(DateTime start, DateTime end);
        Task<List<WMDataReportResponse.Result>> GetWMRecordsBySummary(GetBetRecordReq RecordReq);
        Task<IEnumerable<(int count, decimal netwin, decimal bet, string userid, int game_type, DateTime partitionTime)>>
            SummaryGameRecord(DateTime reportTime, DateTime startTime, DateTime endTime);
        Task<List<WMDataReportResponse.Result>> GetRecordByReportTime(BetRecordSummary RecordReq, DateTime startTime, DateTime endTime, string username);
    }
    public class WMDBService:BetlogsDBServiceBase, IWMDBService
    {
        public WMDBService(ILogger<WMDBService> logger, IBetLogsDbConnectionStringManager connectionStringManager) : base(logger, connectionStringManager)
        {
        }
        /// <summary>
        /// 寫入資料
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="tran"></param>
        /// <param name="betLogs"></param>
        /// <returns></returns>
        public async Task<int> PostWMRecord(NpgsqlConnection conn, IDbTransaction tran, IEnumerable<WMDataReportResponse.Result> betLogs)
        {
            if (tran == null) throw new ArgumentNullException(nameof(tran));
            if (betLogs == null) throw new ArgumentNullException(nameof(betLogs));
            if (!betLogs.Any()) return 0;

            var tableGuid = Guid.NewGuid();
            //建立暫存表
            await CreateBetRecordTempTable(tran, tableGuid);
            //將資料倒進暫存表
            await BulkInsertToTempTable(tran, tableGuid, betLogs);
            //將資料由暫存表倒回主表(過濾重複)
            return await MergeFromTempTable(tran, tableGuid);
        }
        private Task<int> CreateBetRecordTempTable(IDbTransaction tran, Guid tableGuid)
        {
            var sql = $@"CREATE TEMPORARY TABLE temp_t_wm_bet_record_v2_{tableGuid:N} 
                            ( LIKE t_wm_bet_record_v2  INCLUDING ALL );";
            return tran.Connection.ExecuteAsync(sql, transaction: tran);
        }
        private async Task<ulong> BulkInsertToTempTable(IDbTransaction tran, Guid tableGuid, IEnumerable<WMDataReportResponse.Result> records)
        {
            if (tran is not NpgsqlTransaction npTran)
                throw new ArgumentException("Must be NpgsqlTransaction", nameof(tran));

            if (npTran.Connection == null)
                throw new ArgumentNullException(nameof(tran.Connection));

            await using var writer =
                await npTran.Connection.BeginBinaryImportAsync(
                    $@"COPY temp_t_wm_bet_record_v2_{tableGuid:N} (""user"", betid, bettime, beforecash, bet, validbet, water, ""result"", betcode, waterbet, winloss,
                                gid, settime, ""reset"", betresult, gameresult, gname, ip, ""event"", eventchild, round, subround,
                                tableid, commission, report_time, partition_time, pre_bet, pre_validbet, pre_winloss ) FROM STDIN (FORMAT BINARY)");

            foreach (var mapping in records)
            {
                await writer.StartRowAsync();
                await writer.WriteAsync(mapping.user, NpgsqlDbType.Varchar);  // varchar(30)
                await writer.WriteAsync(mapping.betId, NpgsqlDbType.Varchar);  // varchar
                await writer.WriteAsync(mapping.betTime, NpgsqlDbType.Timestamp);  // timestamp
                await writer.WriteAsync(mapping.beforeCash, NpgsqlDbType.Numeric);  // numeric(19, 4)
                await writer.WriteAsync(mapping.bet, NpgsqlDbType.Numeric);  // numeric(19, 4)
                await writer.WriteAsync(mapping.validbet, NpgsqlDbType.Numeric);  // numeric(19, 4)
                await writer.WriteAsync(mapping.water, NpgsqlDbType.Numeric);  // numeric(19, 4)
                await writer.WriteAsync(mapping.result, NpgsqlDbType.Varchar);  // varchar(30)
                await writer.WriteAsync(mapping.betCode, NpgsqlDbType.Varchar);  // varchar(30)
                await writer.WriteAsync(mapping.waterbet, NpgsqlDbType.Numeric);  // numeric(19, 4)
                await writer.WriteAsync(mapping.winLoss, NpgsqlDbType.Numeric);  // numeric(19, 4)
                await writer.WriteAsync(mapping.gid, NpgsqlDbType.Smallint);  // int2
                await writer.WriteAsync(mapping.settime, NpgsqlDbType.Timestamp);  // timestamp
                await writer.WriteAsync(mapping.reset, NpgsqlDbType.Varchar);  // varchar(10)
                await writer.WriteAsync(mapping.betResult, NpgsqlDbType.Varchar);  // varchar(30)
                await writer.WriteAsync(mapping.gameResult, NpgsqlDbType.Varchar);  // varchar
                await writer.WriteAsync(mapping.gname, NpgsqlDbType.Varchar);  // varchar(30)
                await writer.WriteAsync(mapping.ip, NpgsqlDbType.Varchar);  // varchar(30)
                await writer.WriteAsync(mapping.Event, NpgsqlDbType.Varchar);  // varchar(30)
                await writer.WriteAsync(mapping.eventChild, NpgsqlDbType.Varchar);  // varchar(30)
                await writer.WriteAsync(mapping.round, NpgsqlDbType.Varchar);  // varchar(30)
                await writer.WriteAsync(mapping.subround, NpgsqlDbType.Varchar);  // varchar(30)
                await writer.WriteAsync(mapping.tableId, NpgsqlDbType.Varchar);  // varchar(30)
                await writer.WriteAsync(mapping.commission, NpgsqlDbType.Varchar);  // varchar(10)
                await writer.WriteAsync(mapping.report_time, NpgsqlDbType.Timestamp);  // timestamp
                await writer.WriteAsync(mapping.partition_time, NpgsqlDbType.Timestamp);  // timestamp
                await writer.WriteAsync(mapping.pre_bet, NpgsqlDbType.Numeric);  // numeric(19, 4)
                await writer.WriteAsync(mapping.pre_validbet, NpgsqlDbType.Numeric);  // numeric(19, 4)
                await writer.WriteAsync(mapping.pre_winLoss, NpgsqlDbType.Numeric);  // numeric(19, 4)
            }


            return await writer.CompleteAsync();
        }
        private async Task<int> MergeFromTempTable(IDbTransaction tran, Guid tableGuid)
        {
            var sql = @$"INSERT INTO t_wm_bet_record_v2
                        SELECT  ""user"", betid, bettime, beforecash, bet, validbet, water, ""result"", betcode, waterbet, winloss,
                                gid, settime, ""reset"", betresult, gameresult, gname, ip, ""event"", eventchild, round, subround,
                                tableid, commission, report_time, partition_time, create_time, pre_bet, pre_validbet, pre_winloss
                        FROM temp_t_wm_bet_record_v2_{tableGuid:N} AS  temp
                        WHERE NOT EXISTS 
                        (
                            SELECT NULL
                            FROM public.t_wm_bet_record_v2
                            WHERE  partition_time = temp.partition_time
                                AND betid = temp.betid
                                AND settime = temp.settime
                        )";
            return await tran.Connection.ExecuteAsync(sql, tran);
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
                        coalesce(SUM(winLoss),0) AS netwin,
                        coalesce(SUM(bet),0) AS bet,
                        ""user"" as userid,
                        gid	 as game_type,
                        Date(partition_time) as partition_time
                        FROM t_wm_bet_record_v2
                        WHERE partition_time BETWEEN @start_time AND @end_time
                        AND report_time = @report_time
                        GROUP BY ""user"",Date(partition_time),gid
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
        //public async Task<int> PostWMRecord(NpgsqlConnection conn, IDbTransaction tran, IEnumerable<WMDataReportResponse.Result> betLogs)
        //{

        //    var sql = @"INSERT INTO public.t_wm_bet_record
        //            (""user"",betid, bettime, beforecash, bet, validbet, water, result, betcode, waterbet, winloss, gid, settime, reset, betresult, gameresult, summary_id, pre_bet, pre_validbet, pre_winloss,gname, ip, ""event"", eventchild, round, subround, tableid, commission)
        //            VALUES
        //            ( 
        //              @user, 
        //              @betid, 
        //              @bettime, 
        //              @beforecash, 
        //              @bet, 
        //              @validbet, 
        //              @water, 
        //              @result,
        //              @betcode,
        //              @waterbet, 
        //              @winloss, 
        //              @gid, 
        //              @settime, 
        //              @reset,
        //              @betresult, 
        //              @gameresult, 
        //              @summary_id,
        //              @pre_bet, 
        //              @pre_validbet, 
        //              @pre_winloss,
        //              @gname,
        //              @ip,
        //              @event,
        //              @eventchild,
        //              @round,
        //              @subround,
        //              @tableid,
        //              @commission)";


        //    try
        //    {
        //        return await conn.ExecuteAsync(sql, betLogs, tran);
        //    }
        //    catch (Exception ex)
        //    {
        //        throw;
        //    }
        //}

        /// <summary>
        /// 注單號取資料
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<List<WMDataReportResponse.Result>> GetWMRecords(string id, DateTime bettime)
        {
            var sql = @"
                    SELECT ""user"", betid, bettime, beforecash, bet, validbet, water, ""result"", betcode, waterbet, winloss, gid, settime,
                           ""reset"", betresult, gameresult, summary_id, pre_bet, pre_validbet, pre_winloss, gname, ip,
                           ""event"", eventchild, round, subround, tableid, commission
                    FROM t_wm_bet_record
                    WHERE betid = @id 
                        and bettime >= @start 
                        and bettime < @end";

            var par = new DynamicParameters();
            par.Add("@id", id);
            par.Add("@start", bettime.AddDays(-3));
            par.Add("@end", bettime.AddDays(1));

            await using var conn = new NpgsqlConnection(await PGRead);
            var result = await conn.QueryAsync<WMDataReportResponse.Result>(sql, par);
            return result.ToList();
        }
        /// <summary>
        /// V2注單號取資料
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<List<WMDataReportResponse.Result>> GetWMRecordsV2(string id, DateTime bettime)
        {
            var sql = @"
                    SELECT ""user"", betid, bettime, beforecash, bet, validbet, water, ""result"", betcode, waterbet, winloss, gid, settime,
                           ""reset"", betresult, gameresult, gname, ip, ""event"", eventchild, round, subround, tableid, commission, report_time,
                           partition_time, create_time, pre_bet, pre_validbet, pre_winloss
                    FROM t_wm_bet_record_v2
                    WHERE betid = @id 
                        and partition_time >= @start 
                        and partition_time < @end";

            var par = new DynamicParameters();
            par.Add("@id", id);
            par.Add("@start", bettime);
            par.Add("@end", bettime.AddDays(1).AddMilliseconds(-1));

            await using var conn = new NpgsqlConnection(await PGRead);
            var result = await conn.QueryAsync<WMDataReportResponse.Result>(sql, par);
            return result.ToList();
        }
        /// <summary>
        /// 注單號取資料
        /// </summary>
        /// <param name="tran"></param>
        /// <param name="id"></param>
        /// <param name="bettime"></param>
        /// <returns></returns>
        public async Task<List<WMDataReportResponse.Result>> GetWMRecords(IDbTransaction tran, string id, DateTime bettime)
        {
            var sql = @"
                    SELECT betId,settime,betTime FROM t_wm_bet_record
                    WHERE betid = @id 
                        and bettime = @bettime";

            var par = new DynamicParameters();
            par.Add("@id", id);
            par.Add("@bettime", bettime);

            var result = await tran.Connection.QueryAsync<WMDataReportResponse.Result>(sql, par, tran);
            return result.ToList();
        }
        /// <summary>
        /// V2注單號取資料
        /// </summary>
        /// <param name="tran"></param>
        /// <param name="id"></param>
        /// <param name="bettime"></param>
        /// <returns></returns>
        public async Task<List<WMDataReportResponse.Result>> GetWMRecordsV2(IDbTransaction tran, string id, DateTime bettime)
        {
            var sql = @"
                    SELECT betId,settime,betTime FROM t_wm_bet_record_v2
                    WHERE partition_time = @partition_time  
                        and betid = @id";

            var par = new DynamicParameters();
            par.Add("@id", id);
            par.Add("@partition_time", bettime);

            var result = await tran.Connection.QueryAsync<WMDataReportResponse.Result>(sql, par, tran);
            return result.ToList();
        }

        /// <summary>
        /// 每小時匯總
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public async Task<(int totalCount, decimal totalBetValid, decimal totalWin)> SumWMBetRecordByBetTime(DateTime start, DateTime end)
        {
            var sql = @"SELECT 
                    COUNT(betId) AS totalCount
                    , CASE WHEN SUM(validbet) IS NULL THEN 0 ELSE SUM(validbet) END  AS totalBetValid
                    , CASE WHEN SUM(winLoss) IS NULL THEN 0 ELSE SUM(winLoss) END AS totalWin
                    FROM t_wm_bet_record
                    WHERE bettime >= @startTime 
                        AND bettime < @endTime";

            var par = new DynamicParameters();
            par.Add("@startTime", start);
            par.Add("@endTime", end.AddMilliseconds(-1));

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
        public async Task<List<WMDataReportResponse.Result>> GetWMRecordsBytime(DateTime start, DateTime end)
        {

            var sql = @"SELECT *
                FROM public.t_wm_bet_record 
                WHERE bettime >= @startTime 
                    AND bettime < @endTime";
            var par = new DynamicParameters();
            par.Add("@startTime", start);
            par.Add("@endTime", end);

            await using var conn = new NpgsqlConnection(await PGRead);
            var result = await conn.QueryAsync<WMDataReportResponse.Result>(sql, par);
            return result.ToList();
        }

        /// <summary>
        /// 取得GUID資料
        /// </summary>
        /// <param name="RecordReq"></param>
        /// <returns></returns>
        public async Task<List<WMDataReportResponse.Result>> GetWMRecordsBySummary(GetBetRecordReq RecordReq)
        {
            var sql = @"SELECT * 
                    FROM public.t_wm_bet_record 
                    WHERE bettime >= @start 
                        AND bettime <= @end
                        AND summary_id = @summaryId::uuid";

            var param = new
            {
                summaryId = RecordReq.summary_id,
                start = RecordReq.ReportTime.AddDays(-3),
                end = RecordReq.ReportTime.AddDays(1),
            };

            await using var conn = new NpgsqlConnection(await PGRead);
            var result = await conn.QueryAsync<WMDataReportResponse.Result>(sql, param);
            return result.ToList();
        }

        public async Task<List<WMDataReportResponse.Result>> GetRecordByReportTime(BetRecordSummary RecordReq, DateTime startTime, DateTime endTime, string username)
        {
            var sql = @"SELECT ""user"", betid, bettime, beforecash, bet, validbet, water, ""result"", betcode, waterbet,
                               winloss, gid, settime, ""reset"", betresult, gameresult, gname, ip, ""event"", eventchild, round, subround, tableid, commission,
                               report_time, partition_time , create_time , pre_bet, pre_validbet, pre_winloss
                        FROM t_wm_bet_record_v2
                        WHERE partition_time BETWEEN @starttime AND @endtime
                        AND report_time = @reporttime
                        AND ""user"" = @userid";
            var parameters = new DynamicParameters();
            parameters.Add("@starttime", startTime);
            parameters.Add("@endtime", endTime);
            parameters.Add("@reporttime", RecordReq.ReportDatetime);
            parameters.Add("@userid", username);

            using (var conn = new NpgsqlConnection(await PGRead))
            {
                var result = await conn.QueryAsync<WMDataReportResponse.Result>(sql, parameters);
                return result.ToList();
            }
        }
    }
}
