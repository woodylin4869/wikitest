using H1_ThirdPartyWalletAPI.Utility;
using Microsoft.Extensions.Logging;
using Npgsql;
using NpgsqlTypes;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using System;
using Dapper;
using System.Linq;
using ThirdPartyWallet.Share.Model.Game.SPLUS.Response;
using H1_ThirdPartyWalletAPI.Model.Config;

namespace H1_ThirdPartyWalletAPI.Service.Common.DB.Betlogs
{
    public interface ISPLUSDBService
    {
        Task<int> PostSPLUSRecord(NpgsqlConnection conn, IDbTransaction tran, IEnumerable<BetlogResponse.Page_Info> betLogs);
        Task<List<BetlogResponse.Page_Info>> GetSPLUSRecordsBytime(DateTime createtime, DateTime report_time, string club_id);

        Task<IEnumerable<(int count, decimal win, decimal bet, decimal jackpot, string userid, int game_type, DateTime createtime)>> SummaryGameRecord(DateTime reportTime, DateTime startTime, DateTime endTime);
            Task<(int totalCount, decimal totalBetValid, decimal totalWin, decimal totalnetwin)> SumSPLUSBetRecordByBetTime(DateTime start, DateTime end);
    }
    public class SPLUSDBService : BetlogsDBServiceBase, ISPLUSDBService
    {
        public SPLUSDBService(ILogger<SPLUSDBService> logger, IBetLogsDbConnectionStringManager connectionStringManager) : base(logger, connectionStringManager)
        {
        }

        #region 寫單
        /// <summary>
        /// 寫入資料
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="tran"></param>
        /// <param name="betLogs"></param>
        /// <returns></returns>
        public async Task<int> PostSPLUSRecord(NpgsqlConnection conn, IDbTransaction tran, IEnumerable<BetlogResponse.Page_Info> betLogs)
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
            var sql = $@"CREATE TEMPORARY TABLE temp_t_SPLUS_bet_record_{tableGuid:N} 
                            ( LIKE t_SPLUS_bet_record  INCLUDING ALL );";
            return tran.Connection.ExecuteAsync(sql, transaction: tran);
        }
        private async Task<ulong> BulkInsertToTempTable(IDbTransaction tran, Guid tableGuid, IEnumerable<BetlogResponse.Page_Info> records)
        {
            if (tran is not NpgsqlTransaction npTran)
                throw new ArgumentException("Must be NpgsqlTransaction", nameof(tran));

            if (npTran.Connection == null)
                throw new ArgumentNullException(nameof(tran.Connection));

            await using var writer =
                await npTran.Connection.BeginBinaryImportAsync(
                    @$"COPY temp_t_SPLUS_bet_record_{tableGuid:N} (bet_id, round, gamecode, account, currency, bet_amount, bet_valid_amount, pay_off_amount,  
                            jp_win, freegame, bet_time, pay_off_time, status, report_time, partition_time ) FROM STDIN (FORMAT BINARY)");

            foreach (var mapping in records)
            {
                await writer.StartRowAsync();
                await writer.WriteAsync(mapping.bet_id.ToString(), NpgsqlDbType.Varchar); //  varchar
                await writer.WriteAsync(mapping.round.ToString(), NpgsqlDbType.Varchar); //  varchar
                await writer.WriteAsync(mapping.gamecode.ToString(), NpgsqlDbType.Varchar); //  varchar
                await writer.WriteAsync(mapping.account.ToString(), NpgsqlDbType.Varchar); //  varchar
                await writer.WriteAsync(mapping.currency.ToString(), NpgsqlDbType.Varchar); //  varchar
                await writer.WriteAsync(mapping.bet_amount , NpgsqlDbType.Numeric); //  numeric(19, 4)
                await writer.WriteAsync(mapping.bet_valid_amount , NpgsqlDbType.Numeric); //  numeric(19, 4)
                await writer.WriteAsync(mapping.pay_off_amount, NpgsqlDbType.Numeric); //  numeric(19, 4)
                await writer.WriteAsync(mapping.jp_win , NpgsqlDbType.Numeric); //  numeric(19, 4)
                await writer.WriteAsync(mapping.freegame , NpgsqlDbType.Smallint); //  int2
                await writer.WriteAsync(mapping.bet_time, NpgsqlDbType.Timestamp); //  timestamp
                await writer.WriteAsync(mapping.pay_off_time, NpgsqlDbType.Timestamp); //  timestamp
                await writer.WriteAsync(mapping.status.ToString(), NpgsqlDbType.Varchar); //  varchar
                await writer.WriteAsync(mapping.report_time, NpgsqlDbType.Timestamp); //  timestamp
                await writer.WriteAsync(mapping.partition_time, NpgsqlDbType.Timestamp); //  timestamp
            }

            return await writer.CompleteAsync();
        }
        private async Task<int> MergeFromTempTable(IDbTransaction tran, Guid tableGuid)
        {
            var sql = @$"INSERT INTO t_SPLUS_bet_record
                        SELECT bet_id, round, gamecode, account, currency, bet_amount, bet_valid_amount, pay_off_amount, jp_win,
                               freegame, bet_time, pay_off_time, status, report_time, partition_time,createtime 
                        FROM temp_t_SPLUS_bet_record_{tableGuid:N} AS  temp
                        WHERE NOT EXISTS 
                        (
                            SELECT NULL
                            FROM public.t_SPLUS_bet_record
                            WHERE bet_id = temp.bet_id 
                                AND partition_time = temp.partition_time
                        )";
            return await tran.Connection.ExecuteAsync(sql, tran);
        }

        #endregion 寫單

        #region 報表
        /// <summary>
        /// 第二層明細 查詢report_time的住單
        /// </summary>
        /// <param name="createtime"></param>
        /// <param name="report_time"></param>
        /// <returns></returns>
        public async Task<List<BetlogResponse.Page_Info>> GetSPLUSRecordsBytime(DateTime createtime, DateTime report_time, string club_id)
        {
            try
            {
                var sql = @"SELECT bet_id, round, gamecode, account, bet_amount, bet_valid_amount, pay_off_amount, jp_win,
                                bet_time, pay_off_time, status,partition_time  FROM public.t_SPLUS_bet_record 
                        WHERE partition_time BETWEEN @starttime AND @endtime
                        AND report_time = @reporttime
                        AND account=@account";
                var parameters = new DynamicParameters();
                parameters.Add("@starttime", createtime);
                parameters.Add("@endtime", createtime.AddDays(1).AddMilliseconds(-1));
                parameters.Add("@reporttime", report_time);
                parameters.Add("@account", Config.OneWalletAPI.Prefix_Key + club_id);

                await using var conn = new NpgsqlConnection(await PGRead);
                var result = await conn.QueryAsync<BetlogResponse.Page_Info>(sql, parameters);
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
        public async Task<(int totalCount, decimal totalBetValid, decimal totalWin, decimal totalnetwin)> SumSPLUSBetRecordByBetTime(DateTime start, DateTime end)
        {
            var sql = @"SELECT 
                    COUNT(bet_id) AS totalCount
                    , CASE WHEN SUM(bet_valid_amount) IS NULL THEN 0 ELSE SUM(bet_valid_amount) END  AS totalBetValid
                    , CASE WHEN SUM(pay_off_amount + bet_valid_amount) IS NULL THEN 0 ELSE SUM(pay_off_amount + bet_valid_amount) END AS totalWin
                    , CASE WHEN SUM(pay_off_amount + jp_win) IS NULL THEN 0 ELSE SUM(pay_off_amount + jp_win) END AS totalnetwin
                    FROM t_SPLUS_bet_record
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
        /// 五分鐘會總
        /// </summary>
        /// <param name="reportTime"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        public async Task<IEnumerable<(int count, decimal win, decimal bet, decimal jackpot, string userid, int game_type, DateTime createtime)>> SummaryGameRecord(DateTime reportTime, DateTime startTime, DateTime endTime)
        {
            var sql = @"SELECT CAST(count(1) AS INTEGER) AS count,
                        SUM(pay_off_amount) AS win,
                        SUM(bet_amount) AS bet,
                        SUM(jp_win) as jackpot,
                        account as userid, 
                        3 as game_type,
                        Date(partition_time) as createtime
                        FROM t_SPLUS_bet_record
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
        #endregion 報表
    }
}
