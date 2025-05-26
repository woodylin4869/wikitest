using System;
using H1_ThirdPartyWalletAPI.Model.W1API;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using H1_ThirdPartyWalletAPI.Model.Config;
using Npgsql;
using H1_ThirdPartyWalletAPI.Model.Game.FC.Response;
using H1_ThirdPartyWalletAPI.Utility;
using H1_ThirdPartyWalletAPI.Model.DataModel;
using H1_ThirdPartyWalletAPI.Model.DB.JDB.Response;
using NpgsqlTypes;
using static H1_ThirdPartyWalletAPI.Model.Game.TP.Response.BetLogResponse;
using System.Reflection.Emit;

namespace H1_ThirdPartyWalletAPI.Service.Common.DB.Betlogs
{
    public interface IFCDBService
    {
        Task<int> PostfcRecord(NpgsqlConnection conn, IDbTransaction tran, IEnumerable<Record> betLogs);
        Task<(int totalcount, decimal totalbetvalid, decimal totalnetwin)> SumfcBetRecordByBetTime(DateTime start, DateTime end);
        Task<IEnumerable<(int count, decimal win, decimal bet, decimal jackpot, string userid, decimal netwin, DateTime bettime)>> SummaryGameRecord(DateTime reportTime, DateTime startTime, DateTime endTime);
        Task<dynamic> GetFcRecordByReportTime(GetBetDetailReq RecordReq, string recordid);
        Task<List<Record>> GetfcRecordsByBetTime(DateTime start, DateTime end);
        Task<List<Record>> GetFcBetRecords(BetRecordSummary RecordReq, DateTime start, DateTime end);
    }
    public class FCDBService : BetlogsDBServiceBase, IFCDBService
    {
        private readonly string _prefixKey;
        public FCDBService(ILogger<FCDBService> logger, IBetLogsDbConnectionStringManager connectionStringManager) : base(logger, connectionStringManager)
        {
            _prefixKey = Config.OneWalletAPI.Prefix_Key;
        }
        public async Task<IEnumerable<(int count, decimal win, decimal bet, decimal jackpot, string userid, decimal netwin, DateTime bettime)>> SummaryGameRecord(DateTime reportTime, DateTime startTime, DateTime endTime)
        {
            // TODO 後匯總fc更名
            var sql = @"SELECT CAST(count(1) AS INTEGER) AS count,
                               COALESCE (SUM(prize),0) AS win,
                               COALESCE (SUM(bet),0) AS bet,
                               COALESCE (SUM(jppoints),0) AS jackpot,
                               COALESCE (SUM(winlose),0) AS netwin,
                               account AS userid,
                               DATE(partition_time) as bettime
                        FROM t_fc_bet_record_v2
                        WHERE partition_time BETWEEN @start_time AND @end_time
                        AND report_time = @report_time
                        GROUP BY partition_time,account
                        ";

            var par = new DynamicParameters();
            par.Add("@start_time", startTime);
            par.Add("@end_time", endTime);
            par.Add("@report_time", reportTime);
            using (NpgsqlConnection conn = new NpgsqlConnection(await PGRead))
            {
                var result = await conn.QueryAsync(sql, par, commandTimeout: 270);

                return result.Select(x => ((int)x.count, (decimal)x.win, (decimal)x.bet, (decimal)x.jackpot, (string)x.userid, (decimal)x.netwin, (DateTime)x.bettime)).ToList();
            }
        }
        /// <summary>
        /// 寫入資料
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="tran"></param>
        /// <param name="betLogs"></param>
        /// <returns></returns>
        public async Task<int> PostfcRecord(NpgsqlConnection conn, IDbTransaction tran, IEnumerable<Record> betLogs)
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
            var sql = $@"CREATE TEMPORARY TABLE temp_t_fc_bet_record_v2_{tableGuid:N} 
                            ( LIKE t_fc_bet_record_v2  INCLUDING ALL );";
            return tran.Connection.ExecuteAsync(sql, transaction: tran);
        }
        private async Task<ulong> BulkInsertToTempTable(IDbTransaction tran, Guid tableGuid, IEnumerable<Record> records)
        {
            if (tran is not NpgsqlTransaction npTran)
                throw new ArgumentException("Must be NpgsqlTransaction", nameof(tran));

            if (npTran.Connection == null)
                throw new ArgumentNullException(nameof(tran.Connection));

            await using var writer =
                await npTran.Connection.BeginBinaryImportAsync(
                    $"COPY temp_t_fc_bet_record_v2_{tableGuid:N} (bet, prize, winlose, before, after, jptax, jppoints, recordid, account, gameid, gametype, "
                      + " jpmode, bdate, isbuyfeature, report_time, partition_time) FROM STDIN (FORMAT BINARY)");

            foreach (var mapping in records)
            {
                await writer.StartRowAsync();
                // 数值字段需要转换
                await writer.WriteAsync(mapping.bet, NpgsqlDbType.Numeric);
                await writer.WriteAsync(mapping.prize, NpgsqlDbType.Numeric);
                await writer.WriteAsync(mapping.winlose, NpgsqlDbType.Numeric);
                await writer.WriteAsync(mapping.before, NpgsqlDbType.Numeric);
                await writer.WriteAsync(mapping.after, NpgsqlDbType.Numeric);
                await writer.WriteAsync(mapping.jptax, NpgsqlDbType.Numeric);
                await writer.WriteAsync(mapping.jppoints, NpgsqlDbType.Numeric);

                await writer.WriteAsync(mapping.recordID.ToString(), NpgsqlDbType.Varchar);
                await writer.WriteAsync(mapping.account.ToString(), NpgsqlDbType.Varchar);

                await writer.WriteAsync(mapping.gameID, NpgsqlDbType.Integer);
                await writer.WriteAsync(mapping.gametype, NpgsqlDbType.Integer);
                await writer.WriteAsync(mapping.jpmode, NpgsqlDbType.Integer);

                await writer.WriteAsync(mapping.bdate, NpgsqlDbType.Timestamp);
                await writer.WriteAsync(mapping.isBuyFeature, NpgsqlDbType.Boolean);
                await writer.WriteAsync(mapping.report_time, NpgsqlDbType.Timestamp);
                await writer.WriteAsync(mapping.partition_time, NpgsqlDbType.Timestamp);
            }

            return await writer.CompleteAsync();
        }
        private async Task<int> MergeFromTempTable(IDbTransaction tran, Guid tableGuid)
        {
            var sql = @$"INSERT INTO t_fc_bet_record_v2
                        SELECT bet, prize, winlose, ""before"", ""after"", jptax, jppoints, recordid, account, gameid, gametype,
                               jpmode, bdate, isbuyfeature, report_time, create_time, partition_time
                        FROM temp_t_fc_bet_record_v2_{tableGuid:N} AS  temp
                        WHERE NOT EXISTS 
                        (
                            SELECT NULL
                            FROM public.t_fc_bet_record_v2
                            WHERE recordid = temp.recordid 
                                AND partition_time = temp.partition_time
                        )";
            return await tran.Connection.ExecuteAsync(sql, tran);
        }
        /// <summary>
        /// 依下注時間取得注單
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public async Task<List<Record>> GetfcRecordsByBetTime(DateTime start, DateTime end)
        {
            var sql = @"SELECT  recordid, bdate
                        FROM t_fc_bet_record_v2
                        WHERE partition_time BETWEEN @starttime AND @endtime ";
            var par = new DynamicParameters();
            par.Add("@startTime", start);
            par.Add("@endTime", end);

            await using var conn = new NpgsqlConnection(await PGRead);
            var result = await conn.QueryAsync<Record>(sql, par);
            return result.ToList();
        }
        /// <summary>
        /// 每小時匯總
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public async Task<(int totalcount, decimal totalbetvalid, decimal totalnetwin)> SumfcBetRecordByBetTime(DateTime start, DateTime end)
        {
            //排除gameid=99999 活動注單
            var sql = @"SELECT 
                    COUNT(recordid) AS totalcount
                    , coalesce(SUM(bet),0) AS totalbetvalid
                    , coalesce(SUM(winlose),0) AS totalnetwin
                    FROM t_fc_bet_record_v2
                    WHERE partition_time BETWEEN @startTime AND @endTime
                        and gameid<>'99999'";

            var par = new DynamicParameters();
            par.Add("@startTime", start);
            par.Add("@endTime", end);

            await using var conn = new NpgsqlConnection(await PGRead);
            var result = (await conn.QueryAsync<dynamic>(sql, par)).FirstOrDefault();
            return ((int)result.totalcount, (decimal)result.totalbetvalid, (decimal)result.totalnetwin);
        }
        /// <summary>
        /// 取得第二層明細
        /// </summary>
        /// <param name="RecordReq"></param>
        /// <returns></returns>
        public async Task<List<Record>> GetFcBetRecords(BetRecordSummary RecordReq, DateTime start, DateTime end)
        {
            var sql = @"SELECT
                    bet, prize, winlose, ""before"", ""after"", jptax, jppoints, recordid, account, gameid, gametype,
                    jpmode, bdate, isbuyfeature, report_time, partition_time
                FROM t_fc_bet_record_v2
                WHERE partition_time BETWEEN @starttime AND @endtime
                AND report_time = @report_time
                AND account = @account";

            var parameters = new DynamicParameters();
            parameters.Add("@starttime", start);
            parameters.Add("@endtime", end);
            parameters.Add("@report_time", RecordReq.ReportDatetime);
            parameters.Add("@account",_prefixKey + RecordReq.Club_id);

            {
                await using var conn = new NpgsqlConnection(await PGRead);
                var result = await conn.QueryAsync<Record>(sql, parameters);
                return result.ToList();
            }
        }
        /// <summary>
        /// 依照BetRecordSummary、遊戲序號找到明細資料
        /// </summary>
        /// <param name="RecordReq"></param>
        /// <param name="historyid"></param>
        /// <returns></returns>
        public async Task<dynamic> GetFcRecordByReportTime(GetBetDetailReq RecordReq, string recordid)
        {
            var par = new DynamicParameters();
            string strSql = @"SELECT account
                    FROM t_fc_bet_record_v2
                    WHERE partition_time BETWEEN @start_date AND @end_date
                          AND recordid = @recordid 
                          Limit 1 ";
            par.Add("@recordid", recordid);
            par.Add("@start_date", RecordReq.ReportTime.AddDays(-3));
            par.Add("@end_date", RecordReq.ReportTime.AddDays(1));
            using (NpgsqlConnection conn = new NpgsqlConnection(await PGRead))
            {
                return await conn.QuerySingleOrDefaultAsync<dynamic>(strSql, par);
            }
        }
    }
}
