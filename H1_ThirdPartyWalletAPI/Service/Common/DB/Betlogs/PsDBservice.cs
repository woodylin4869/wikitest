using Dapper;
using H1_ThirdPartyWalletAPI.Model.Config;
using H1_ThirdPartyWalletAPI.Model.W1API;
using H1_ThirdPartyWalletAPI.Utility;
using Microsoft.Extensions.Logging;
using Npgsql;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using System;
using ThirdPartyWallet.Share.Model.Game.PS.Response;
using System.Linq;
using NpgsqlTypes;
using static H1_ThirdPartyWalletAPI.Model.Game.TP.Response.BetLogResponse;

namespace H1_ThirdPartyWalletAPI.Service.Common.DB.Betlogs
{
    public interface IPSDBService
    {
        Task<int> PostPsRecord(NpgsqlConnection conn, IDbTransaction tran, IEnumerable<GetorderResponse.BetRecord> betLogs);
        Task<List<GetorderResponse.BetRecord>> GetPsRecordsBytime(DateTime createtime, DateTime report_time, string club_id);
        Task<List<GetorderResponse.BetRecord>> GetPsRecords(string id, DateTime time);
        Task<(int totalCount, decimal totalBetValid, decimal totalWin, decimal totalnetwin)> SumPsBetRecordByBetTime(DateTime start, DateTime end);
        Task<List<GetorderResponse.BetRecord>> GetPsRecordsBycreatetime(DateTime starttime, DateTime endtime);

        Task<IEnumerable<(int count, decimal win, decimal bet, decimal jackpot, string userid, int game_type, DateTime createtime)>>
            SummaryGameRecord(DateTime reportTime, DateTime startTime, DateTime endTime);
    }
    public class PSDBService : BetlogsDBServiceBase, IPSDBService
    {
        public PSDBService(ILogger<PSDBService> logger, IBetLogsDbConnectionStringManager connectionStringManager) : base(logger, connectionStringManager)
        {
        }
        /// <summary>
        /// 寫入資料
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="tran"></param>
        /// <param name="betLogs"></param>
        /// <returns></returns>
        public async Task<int> PostPsRecord(NpgsqlConnection conn, IDbTransaction tran, IEnumerable<GetorderResponse.BetRecord> betLogs)
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
            var sql = $@"CREATE TEMPORARY TABLE temp_t_Ps_bet_record_{tableGuid:N} 
                            ( LIKE t_Ps_bet_record  INCLUDING ALL );";
            return tran.Connection.ExecuteAsync(sql, transaction: tran);
        }
        private async Task<ulong> BulkInsertToTempTable(IDbTransaction tran, Guid tableGuid, IEnumerable<GetorderResponse.BetRecord> records)
        {
            if (tran is not NpgsqlTransaction npTran)
                throw new ArgumentException("Must be NpgsqlTransaction", nameof(tran));

            if (npTran.Connection == null)
                throw new ArgumentNullException(nameof(tran.Connection));

            await using var writer =
                await npTran.Connection.BeginBinaryImportAsync(
                    $"COPY temp_t_Ps_bet_record_{tableGuid:N} (gt,gid,member_id,sn,s_tm,bet,win,betamt,winamt,report_time," +
                    $"partition_time,pre_betamount,pre_wonamount,pre_turnover,pre_winlose,jp) FROM STDIN (FORMAT BINARY)");

            foreach (var mapping in records)
            {
                await writer.StartRowAsync();
                await writer.WriteAsync(mapping.gt.ToString(), NpgsqlDbType.Varchar);
                await writer.WriteAsync(mapping.gid.ToString(), NpgsqlDbType.Varchar);
                await writer.WriteAsync(mapping.member_id.ToString(), NpgsqlDbType.Varchar);
                await writer.WriteAsync(mapping.sn.ToString(), NpgsqlDbType.Varchar);
                await writer.WriteAsync(mapping.s_tm, NpgsqlDbType.Timestamp);
                await writer.WriteAsync(mapping.bet, NpgsqlDbType.Numeric);
                await writer.WriteAsync(mapping.win, NpgsqlDbType.Numeric);
                await writer.WriteAsync(mapping.betamt, NpgsqlDbType.Numeric);
                await writer.WriteAsync(mapping.winamt, NpgsqlDbType.Numeric);
                await writer.WriteAsync(mapping.report_time, NpgsqlDbType.Timestamp);
                await writer.WriteAsync(mapping.partition_time, NpgsqlDbType.Timestamp);
                await writer.WriteAsync(mapping.pre_betamount, NpgsqlDbType.Numeric);
                await writer.WriteAsync(mapping.pre_wonamount, NpgsqlDbType.Numeric);
                await writer.WriteAsync(mapping.pre_turnover, NpgsqlDbType.Numeric);
                await writer.WriteAsync(mapping.pre_winlose, NpgsqlDbType.Numeric);
                await writer.WriteAsync(mapping.jp, NpgsqlDbType.Numeric);
            }

            return await writer.CompleteAsync();
        }
        private async Task<int> MergeFromTempTable(IDbTransaction tran, Guid tableGuid)
        {
            var sql = @$"INSERT INTO t_Ps_bet_record
                        SELECT gt, gid, member_id, sn, s_tm, bet, win, betamt, winamt, report_time, partition_time, pre_betamount,
                                pre_wonamount, pre_turnover, pre_winlose, create_time, jp
                        FROM temp_t_Ps_bet_record_{tableGuid:N} AS  temp
                        WHERE NOT EXISTS 
                        (
                            SELECT NULL
                            FROM public.t_Ps_bet_record
                            WHERE sn = temp.sn 
                                AND partition_time = temp.partition_time
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
        public async Task<IEnumerable<(int count, decimal win, decimal bet, decimal jackpot, string userid, int game_type, DateTime createtime)>> SummaryGameRecord(DateTime reportTime, DateTime startTime, DateTime endTime)
        {
            var sql = @"SELECT CAST(count(1) AS INTEGER) AS count,
                        SUM(win) AS win,
                        SUM(bet) AS bet,
                        SUM(jp) as jackpot,
                        member_id as userid, 
                        3 as game_type,
                        Date(s_tm) as createtime
                        FROM t_Ps_bet_record
                        WHERE partition_time BETWEEN @start_time AND @end_time
                        AND report_time = @report_time
                        GROUP BY member_id,Date(s_tm)
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
        public async Task<List<GetorderResponse.BetRecord>> GetPsRecords(string id, DateTime time)
        {
            var sql = @"
                    SELECT sn,s_tm,member_id,gid,bet,win,pre_betamount,pre_wonamount,pre_turnover,pre_winlose,partition_time FROM t_ps_bet_record
                    WHERE sn = @id and
                          partition_time >= @startTime 
                        AND partition_time < @endTime";

            var par = new DynamicParameters();
            par.Add("@id", id);
            par.Add("@startTime", time.AddDays(-3));
            par.Add("@endTime", time.AddDays(1));

            await using var conn = new NpgsqlConnection(await PGRead);
            var result = await conn.QueryAsync<GetorderResponse.BetRecord>(sql, par);
            return result.ToList();
        }
        /// <summary>
        /// report_time 及時間區間
        /// </summary>
        /// <param name="createtime"></param>
        /// <param name="report_time"></param>
        /// <returns></returns>
        public async Task<List<GetorderResponse.BetRecord>> GetPsRecordsBytime(DateTime createtime, DateTime report_time, string club_id)
        {
            try
            {
                var sql = @"SELECT sn,s_tm,gt,gid,bet,win,jp  FROM public.t_Ps_bet_record 
                        WHERE partition_time BETWEEN @starttime AND @endtime
                        AND report_time = @reporttime
                        AND member_id=@club_id";
                var parameters = new DynamicParameters();
                parameters.Add("@starttime", createtime);
                parameters.Add("@endtime", createtime.AddDays(1).AddMilliseconds(-1));
                parameters.Add("@reporttime", report_time);
                parameters.Add("@club_id", Config.OneWalletAPI.Prefix_Key + club_id);

                await using var conn = new NpgsqlConnection(await PGRead);
                var result = await conn.QueryAsync<GetorderResponse.BetRecord>(sql, parameters);
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
        public async Task<(int totalCount, decimal totalBetValid, decimal totalWin, decimal totalnetwin)> SumPsBetRecordByBetTime(DateTime start, DateTime end)
        {
            var sql = @"SELECT 
                    COUNT(sn) AS totalCount
                    , CASE WHEN SUM(bet) IS NULL THEN 0 ELSE SUM(bet) END  AS totalBetValid
                    , CASE WHEN SUM(win) IS NULL THEN 0 ELSE SUM(win) END AS totalWin
                    , CASE WHEN SUM(win - bet) IS NULL THEN 0 ELSE SUM(win - bet) END AS totalnetwin
                    FROM t_ps_bet_record
                    WHERE partition_time >= @startTime 
                        AND partition_time < @endTime";

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
        public async Task<List<GetorderResponse.BetRecord>> GetPsRecordsBycreatetime(DateTime starttime, DateTime endtime)
        {
            try
            {
                var sql = @"SELECT sn,s_tm
                    FROM public.t_ps_bet_record 
                        WHERE partition_time BETWEEN @starttime AND @endtime
                       ";
                var parameters = new DynamicParameters();
                parameters.Add("@starttime", starttime);
                parameters.Add("@endtime", endtime);


                await using var conn = new NpgsqlConnection(await PGRead);
                var result = await conn.QueryAsync<GetorderResponse.BetRecord>(sql, parameters);
                return result.ToList();
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
