using Dapper;
using H1_ThirdPartyWalletAPI.Model.Config;
using H1_ThirdPartyWalletAPI.Utility;
using Microsoft.Extensions.Logging;
using Npgsql;
using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using ThirdPartyWallet.Share.Model.Game.VA.Response;

namespace H1_ThirdPartyWalletAPI.Service.Common.DB.Betlogs
{
    public interface IVADBService
    {
        Task<int> PostVARecord(NpgsqlConnection conn, IDbTransaction tran, IEnumerable<Betlog> betLogs);
        Task<List<Betlog>> GetVARecordsBytime(DateTime createtime, DateTime report_time, string club_id);
        Task<(int totalCount, decimal totalBetValid, decimal totalWin, decimal totalnetwin)> SumVABetRecordByBetTime(DateTime start, DateTime end);
        Task<List<Betlog>> GetVARecordsBycreatetime(DateTime starttime, DateTime endtime);

        Task<IEnumerable<(int count, decimal win, decimal bet, decimal jackpot, string userid, int game_type, DateTime createtime)>>
            SummaryGameRecord(DateTime reportTime, DateTime startTime, DateTime endTime);
    }
    public class VADBService : BetlogsDBServiceBase, IVADBService
    {
        public VADBService(ILogger<VADBService> logger, IBetLogsDbConnectionStringManager connectionStringManager) : base(logger, connectionStringManager)
        {
        }
        /// <summary>
        /// 寫入資料
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="tran"></param>
        /// <param name="betLogs"></param>
        /// <returns></returns>
        public async Task<int> PostVARecord(NpgsqlConnection conn, IDbTransaction tran, IEnumerable<Betlog> betLogs)
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
            var sql = $@"CREATE TEMPORARY TABLE temp_t_va_bet_record_{tableGuid:N} 
                            ( LIKE t_va_bet_record INCLUDING ALL );";
            return tran.Connection.ExecuteAsync(sql, transaction: tran);
        }
        private async Task<ulong> BulkInsertToTempTable(IDbTransaction tran, Guid tableGuid, IEnumerable<Betlog> records)
        {
            if (tran is not NpgsqlTransaction npTran)
                throw new ArgumentException("Must be NpgsqlTransaction", nameof(tran));

            if (npTran.Connection == null)
                throw new ArgumentNullException(nameof(tran.Connection));

            await using var writer =
                await npTran.Connection.BeginBinaryImportAsync(
                    $@"COPY  temp_t_va_bet_record_{tableGuid:N} (version_key,bet_id,channel_id,account,currency,game_id,bet,payout,win_lose,free_game,status,bet_mode,bet_time,create_time,settle_time,report_time,partition_time,jackpotwin) " +
                    $"FROM STDIN WITH (FORMAT BINARY);");

            foreach (var mapping in records)
            {
                await writer.StartRowAsync();
                await writer.WriteAsync(mapping.VersionKey, NpgsqlDbType.Bigint);  // long
                await writer.WriteAsync(mapping.BetId, NpgsqlDbType.Varchar);      // string
                await writer.WriteAsync(mapping.ChannelId, NpgsqlDbType.Integer);  // int
                await writer.WriteAsync(mapping.Account, NpgsqlDbType.Varchar);    // string
                await writer.WriteAsync(mapping.Currency, NpgsqlDbType.Varchar);   // string
                await writer.WriteAsync(mapping.GameId, NpgsqlDbType.Integer);    // int
                await writer.WriteAsync(mapping.Bet, NpgsqlDbType.Numeric);       // decimal
                await writer.WriteAsync(mapping.Payout, NpgsqlDbType.Numeric);    // decimal
                await writer.WriteAsync(mapping.WinLose, NpgsqlDbType.Numeric);   // decimal
                await writer.WriteAsync(mapping.FreeGame, NpgsqlDbType.Integer);  // int
                await writer.WriteAsync(mapping.Status, NpgsqlDbType.Integer);    // int
                await writer.WriteAsync(mapping.BetMode, NpgsqlDbType.Varchar);   // string
                await writer.WriteAsync(mapping.BetTime, NpgsqlDbType.Timestamp); // DateTime
                await writer.WriteAsync(mapping.CreateTime, NpgsqlDbType.Timestamp); // DateTime
                await writer.WriteAsync(mapping.SettleTime, NpgsqlDbType.Timestamp); // DateTime
                await writer.WriteAsync(mapping.report_time, NpgsqlDbType.Timestamp); // DateTime
                await writer.WriteAsync(mapping.partition_time, NpgsqlDbType.Timestamp); // DateTime
                await writer.WriteAsync(mapping.jackpotwin, NpgsqlDbType.Numeric);    // decimal

            }

            return await writer.CompleteAsync();
        }
        private async Task<int> MergeFromTempTable(IDbTransaction tran, Guid tableGuid)
        {
            var sql = @$"INSERT INTO t_va_bet_record
                        SELECT version_key,bet_id,channel_id,account,currency,game_id,bet,payout,win_lose,free_game,status,bet_mode,bet_time,create_time,settle_time,report_time,partition_time,jackpotwin 
                        FROM temp_t_va_bet_record_{tableGuid:N} AS  temp
                        WHERE NOT EXISTS 
                        (
                            SELECT NULL
                            FROM public.t_va_bet_record
                            WHERE bet_id = temp.bet_id 
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
                        SUM(payout) AS win,
                        SUM(bet) AS bet,
                        SUM(jackpotwin) as jackpot,
                        account as userid, 
                        3 as game_type,
                        Date(partition_time) as createtime
                        FROM t_va_bet_record
                        WHERE partition_time BETWEEN @start_time AND @end_time
                        AND report_time = @report_time
                        GROUP BY account,Date(partition_time)
                        ";

            var par = new DynamicParameters();
            par.Add("@start_time", startTime);
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
        //public async Task<List<Betlog>> GetVARecords(string id, DateTime time)
        //{
        //    var sql = @"
        //            SELECT sn,s_tm,member_id,gid,bet,win,pre_betamount,pre_wonamount,pre_turnover,pre_winlose,partition_time FROM t_va_bet_record
        //            WHERE sn = @id and
        //                  partition_time >= @startTime 
        //                AND partition_time < @endTime";

        //    var par = new DynamicParameters();
        //    par.Add("@id", id);
        //    par.Add("@startTime", time.AddDays(-3));
        //    par.Add("@endTime", time.AddDays(1));

        //    await using var conn = new NpgsqlConnection(await PGRead);
        //    var result = await conn.QueryAsync<Betlog>(sql, par);
        //    return result.ToList();
        //}
        /// <summary>
        /// report_time 及時間區間
        /// </summary>
        /// <param name="createtime"></param>
        /// <param name="report_time"></param>
        /// <returns></returns>
        public async Task<List<Betlog>> GetVARecordsBytime(DateTime createtime, DateTime report_time, string club_id)
        {
            try
            {
                var sql = @"SELECT bet_id AS BetId,
                           game_id AS GameId,
                           win_lose AS winLose,
                           free_game AS freeGame,
                           bet_mode AS betMode,
                           bet_time AS betTime,
                           settle_time AS settleTime,
                           account,
                           bet,
                           payout,
                           status,
                           report_time,
                           jackpotwin
                    FROM public.t_va_bet_record
                    WHERE partition_time BETWEEN @starttime AND @endtime
                      AND report_time = @reporttime
                      AND account = @club_id;";
                var parameters = new DynamicParameters();
                parameters.Add("@starttime", createtime);
                parameters.Add("@endtime", createtime.AddDays(1).AddMilliseconds(-1));
                parameters.Add("@reporttime", report_time);
                parameters.Add("@club_id", Config.OneWalletAPI.Prefix_Key + club_id);

                await using var conn = new NpgsqlConnection(await PGRead);
                var result = await conn.QueryAsync<Betlog>(sql, parameters);
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
        public async Task<(int totalCount, decimal totalBetValid, decimal totalWin, decimal totalnetwin)> SumVABetRecordByBetTime(DateTime start, DateTime end)
        {
            var sql = @"SELECT 
                    COUNT(bet_id) AS totalCount
                    , CASE WHEN SUM(bet) IS NULL THEN 0 ELSE SUM(bet) END  AS totalBetValid
                    , CASE WHEN SUM(payout) IS NULL THEN 0 ELSE SUM(payout) END AS totalWin
                    , CASE WHEN SUM(payout - bet) IS NULL THEN 0 ELSE SUM(payout - bet) END AS totalnetwin
                    FROM t_va_bet_record
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
        public async Task<List<Betlog>> GetVARecordsBycreatetime(DateTime starttime, DateTime endtime)
        {
            try
            {
                var sql = @"SELECT bet_id AS BetId,bet_time AS betTime
                    FROM public.t_va_bet_record 
                        WHERE partition_time BETWEEN @starttime AND @endtime
                       ";
                var parameters = new DynamicParameters();
                parameters.Add("@starttime", starttime);
                parameters.Add("@endtime", endtime);


                await using var conn = new NpgsqlConnection(await PGRead);
                var result = await conn.QueryAsync<Betlog>(sql, parameters);
                return result.ToList();
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
