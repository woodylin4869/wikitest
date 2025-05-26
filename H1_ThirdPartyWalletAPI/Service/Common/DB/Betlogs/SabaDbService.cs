using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Dapper;
using Npgsql;
using H1_ThirdPartyWalletAPI.Model.DataModel;
using H1_ThirdPartyWalletAPI.Model.Game;
using Microsoft.Extensions.Configuration;
using System.Data;
using H1_ThirdPartyWalletAPI.Model.W1API;
using System.Linq;
using H1_ThirdPartyWalletAPI.Model.Game.JDB.Response;
using H1_ThirdPartyWalletAPI.Code;
using H1_ThirdPartyWalletAPI.Model.Game.AE.Response;
using Microsoft.Extensions.Options;
using H1_ThirdPartyWalletAPI.Model.Config;
using H1_ThirdPartyWalletAPI.Utility;
using NpgsqlTypes;
using ThirdPartyWallet.Share.Model.Game.PS.Response;

namespace H1_ThirdPartyWalletAPI.Service.Common.DB.Betlogs
{
    public interface ISabaDbService
    {
        Task<IEnumerable<dynamic>> GetSabaRecordBySummaryOld(GetBetRecordReq RecordReq);
        Task<List<SABA_BetDetails>> GetSabaRecordBySummary(GetBetRecordReq RecordReq);
        Task<IEnumerable<SABA_BetDetails>> GetSabaRecord(SABA_BetDetails record_data);

        Task<IEnumerable<SABA_BetDetails>> GetSabaRecord(NpgsqlConnection conn, IDbTransaction tran, SABA_BetDetails record_data);
        Task<IEnumerable<SABA_BetDetails>> PutSabaRecord(SABA_BetDetails record_data);
        Task<IEnumerable<SABA_BetDetails>> GetSabaRunningRecord(NpgsqlConnection conn, IDbTransaction tran, SABA_BetDetails record_data);
        Task<IEnumerable<SABA_BetDetails>> GetSabaRunningRecord(GetBetRecordUnsettleReq RecordReq);
        Task<int> DeleteSabaRunningRecord(NpgsqlConnection conn, IDbTransaction tran, SABA_BetDetails record_data);
        Task<long> GetSabaLastVersionKey(string operator_id);
        Task<dynamic> SumSabaBetRecordDaily(DateTime reportDate, string operator_id);
        Task<IEnumerable<SabaBetType>> GetSabaBetType();
        Task<int> PostSabaReport(SABA_GetFinancialReportData report_data);
        Task<int> DeleteSabaReport(SABA_GetFinancialReportData report_data);


        Task<IEnumerable<SABA_BetDetails>> GetSabaV2Record(NpgsqlConnection conn, IDbTransaction tran, SABA_BetDetails record_data);
        Task<IEnumerable<SABA_BetDetails>> GetSabaV2Record(SABA_BetDetails record_data);
        Task<int> PostSabaRunningRecord(NpgsqlConnection conn, IDbTransaction tran, IEnumerable<SABA_BetDetails> betLogs);
        Task<int> PostSabaRecord(NpgsqlConnection conn, IDbTransaction tran, IEnumerable<SABA_BetDetails> betLogs);
        Task<IEnumerable<(int count, decimal netWin, decimal bet, decimal betValidBet, decimal jackpot, string userid, int game_type, DateTime partitionTime)>> SummaryGameRecord
            (DateTime reportTime, DateTime startTime, DateTime endTime);
        Task<List<SABA_BetDetails>> GetSabaRecordV2BySummary(DateTime partitionTime, DateTime ReportTime, string ClubId);
    }

    public class SabaDbService : BetlogsDBServiceBase, ISabaDbService
    {
        public SabaDbService(ILogger<SabaDbService> logger, IBetLogsDbConnectionStringManager connectionStringManager) : base(logger, connectionStringManager)
        {
        }

        #region t_saba_bet_record + t_saba_bet_running_record
        public async Task<IEnumerable<dynamic>> GetSabaRecordBySummaryOld(GetBetRecordReq RecordReq)
        {
            var par = new DynamicParameters();
            string strSql = @"SELECT trans_id, last_version_key, vendor_member_id, operator_id, league_id, match_id, sport_type, bet_type, parlay_ref_no,
                            settlement_time, transaction_time, winlost_datetime, stake, winlost, winlost_amount, islive, after_amount, balancechange, currency,
                            odds, odds_type, ticket_status, summary_id, bet_team, leaguename_en, hometeamname_en, awayteamname_en, pre_stake, pre_winlost_amount
                    FROM t_saba_bet_record
                    WHERE summary_id = @summary_id
                    AND winlost_datetime > @start_date
                    ;
                    SELECT trans_id, last_version_key, vendor_member_id, operator_id, league_id, match_id, sport_type, bet_type, parlay_ref_no, settlement_time,
                           transaction_time, winlost_datetime, stake, winlost, winlost_amount, islive, after_amount, balancechange, currency, odds, odds_type,
                           ticket_status, summary_id, bet_team, leaguename_en, hometeamname_en, awayteamname_en, club_id, franchiser_id, turnover
                    FROM t_saba_bet_record_running
                    WHERE summary_id = @summary_id;
                    ";
            par.Add("@summary_id", Guid.Parse(RecordReq.summary_id));
            par.Add("@start_date", RecordReq.ReportTime.AddDays(-3));

            using (NpgsqlConnection conn = new NpgsqlConnection(await PGRead))
            {
                List<dynamic> res = new List<dynamic>();
                var results = await conn.QueryMultipleAsync(strSql, par);
                while (!results.IsConsumed)
                {
                    List<dynamic> result = results.Read().ToList();
                    foreach (dynamic r in result)
                    {
                        res.Add(r);
                    }
                }
                return res;
            }
        }
        public async Task<List<SABA_BetDetails>> GetSabaRecordBySummary(GetBetRecordReq RecordReq)
        {
            var par = new DynamicParameters();
            string strSql = @"SELECT  trans_id, last_version_key, vendor_member_id, operator_id, league_id, match_id, sport_type, bet_type, parlay_ref_no,
                            settlement_time, transaction_time, winlost_datetime, stake, winlost, winlost_amount, islive, after_amount, balancechange, currency,
                            odds, odds_type, ticket_status, summary_id, bet_team, leaguename_en, hometeamname_en, awayteamname_en, pre_stake, pre_winlost_amount
                    FROM t_saba_bet_record
                    WHERE summary_id = @summary_id
                    AND winlost_datetime > @start_date
                    ";
            par.Add("@summary_id", Guid.Parse(RecordReq.summary_id));
            par.Add("@start_date", RecordReq.ReportTime.AddDays(-3));

            using (NpgsqlConnection conn = new NpgsqlConnection(await PGRead))
            {
                var res =  await conn.QueryAsync<SABA_BetDetails>(strSql, par);
                return res.ToList();
            }
        }
        public async Task<IEnumerable<SABA_BetDetails>> GetSabaRecord(SABA_BetDetails record_data)
        {
            var par = new DynamicParameters();
            string strSql = @"SELECT trans_id, last_version_key, vendor_member_id, operator_id, league_id, match_id, sport_type, bet_type, parlay_ref_no,
                            settlement_time, transaction_time, winlost_datetime, stake, winlost, winlost_amount, islive, after_amount, balancechange, currency,
                            odds, odds_type, ticket_status, summary_id, bet_team, leaguename_en, hometeamname_en, awayteamname_en, pre_stake, pre_winlost_amount
                    FROM t_saba_bet_record
                    WHERE trans_id = @trans_id
                    AND ticket_status = @ticket_status
                    AND winlost_datetime > @winlost_datetime
                    Limit 1";

            par.Add("@trans_id", record_data.trans_id);
            par.Add("@ticket_status", record_data.ticket_status);
            par.Add("@winlost_datetime", record_data.winlost_datetime.GetValueOrDefault().AddDays(-2));

            using (NpgsqlConnection conn = new NpgsqlConnection(await PGRead))
            {
                return await conn.QueryAsync<SABA_BetDetails>(strSql, par);
            }
        }

        public async Task<IEnumerable<SABA_BetDetails>> GetSabaRecord(NpgsqlConnection conn, IDbTransaction tran, SABA_BetDetails record_data)
        {
            var par = new DynamicParameters();
            string strSql = @"SELECT trans_id, last_version_key, vendor_member_id, operator_id, league_id, match_id, sport_type, bet_type, parlay_ref_no,
                            settlement_time, transaction_time, winlost_datetime, stake, winlost, winlost_amount, islive, after_amount, balancechange, currency,
                            odds, odds_type, ticket_status, summary_id, bet_team, leaguename_en, hometeamname_en, awayteamname_en, pre_stake, pre_winlost_amount
                    FROM t_saba_bet_record
                    WHERE trans_id = @trans_id and winlost_datetime > @winlost_datetime";

            par.Add("@trans_id", record_data.trans_id);
            par.Add("@winlost_datetime", record_data.winlost_datetime.GetValueOrDefault().AddDays(-2));
            var results = await conn.QueryAsync<SABA_BetDetails>(strSql, par, tran);
            return results;
        }
        public async Task<IEnumerable<SABA_BetDetails>> PutSabaRecord(SABA_BetDetails record_data)
        {
            var par = new DynamicParameters();
            string strSql = @"UPDATE t_saba_bet_record
                    SET winlost_datetime = @winlost_datetime
                    WHERE trans_id = @trans_id
                    AND ticket_status = @ticket_status";

            par.Add("@trans_id", record_data.trans_id);
            par.Add("@ticket_status", record_data.ticket_status);
            par.Add("@winlost_datetime", record_data.winlost_datetime);

            using (NpgsqlConnection conn = new NpgsqlConnection(PGMaster))
            {
                return await conn.QueryAsync<SABA_BetDetails>(strSql, par);
            }
        }
   
        public async Task<IEnumerable<SABA_BetDetails>> GetSabaRunningRecord(NpgsqlConnection conn, IDbTransaction tran, SABA_BetDetails record_data)
        {
            var par = new DynamicParameters();
            string strSql = @"SELECT trans_id, last_version_key, vendor_member_id, operator_id, league_id, match_id, sport_type, bet_type, parlay_ref_no, settlement_time,
                           transaction_time, winlost_datetime, stake, winlost, winlost_amount, islive, after_amount, balancechange, currency, odds, odds_type,
                           ticket_status, summary_id, bet_team, leaguename_en, hometeamname_en, awayteamname_en, club_id, franchiser_id, turnover
                    FROM t_saba_bet_record_running
                    WHERE trans_id = @trans_id
                    AND ticket_status = @ticket_status
                    Limit 1";

            par.Add("@trans_id", record_data.trans_id);
            par.Add("@ticket_status", "running");
            var results = await conn.QueryAsync<SABA_BetDetails>(strSql, par, tran);
            return results;
        }
        public async Task<IEnumerable<SABA_BetDetails>> GetSabaRunningRecord(GetBetRecordUnsettleReq RecordReq)
        {
            var par = new DynamicParameters();
            string strSql = @"SELECT trans_id, last_version_key, vendor_member_id, operator_id, league_id, match_id, sport_type, bet_type, parlay_ref_no, settlement_time,
                           transaction_time, winlost_datetime, stake, winlost, winlost_amount, islive, after_amount, balancechange, currency, odds, odds_type,
                           ticket_status, summary_id, bet_team, leaguename_en, hometeamname_en, awayteamname_en, club_id, franchiser_id, turnover
                    FROM t_saba_bet_record_running
                    WHERE transaction_time BETWEEN @StartTime AND @EndTime";
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
                return await conn.QueryAsync<SABA_BetDetails>(strSql, par);
            }
        }
    
        public async Task<int> DeleteSabaRunningRecord(NpgsqlConnection conn, IDbTransaction tran, SABA_BetDetails record_data)
        {
            string strSqlDel = @"DELETE FROM t_saba_bet_record_running
                               WHERE trans_id=@trans_id";
            return await conn.ExecuteAsync(strSqlDel, record_data, tran);
        }
        public async Task<long> GetSabaLastVersionKey(string operator_id)
        {
            try
            {
                long lastVersionKey = 0;
                using (NpgsqlConnection conn = new NpgsqlConnection(await PGRead))
                {
                    string strSql = @"
                                SELECT MAX(last_version_key)
                                FROM t_saba_bet_record;
                                where operator_id=@operator_id
                                SELECT MAX(last_version_key)
                                FROM t_saba_bet_record_running
                                 where operator_id=@operator_id
                                ";
                    var parameters = new DynamicParameters();
                    parameters.Add("@operator_id", operator_id);
                    var results = await conn.QueryMultipleAsync(strSql, parameters);
                    while (!results.IsConsumed)
                    {
                        List<dynamic> result = results.Read().ToList();
                        foreach (dynamic r in result)
                        {
                            if (r.max > lastVersionKey)
                            {
                                lastVersionKey = r.max;
                            }
                        }
                    }
                }
                return lastVersionKey;
            }
            catch (Exception ex)
            {
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                _logger.LogError("SABA WriteRecord exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
                return 0;
            }
        }
        public async Task<dynamic> SumSabaBetRecordDaily(DateTime reportDate, string operator_id)
        {
            var par = new DynamicParameters();
            string strSql = @"SELECT 
                    COUNT(*) AS total_count
                    , SUM(pre_stake) AS total_bet
                    , SUM(pre_winlost_amount) AS total_netwin
                    FROM t_saba_bet_record
                    WHERE winlost_datetime = @winlost_datetime
                    AND ticket_status <> @ticket_status
                    ";

            par.Add("@winlost_datetime", reportDate);
            par.Add("@operator_id", operator_id);
            par.Add("@ticket_status", "running");
            using (NpgsqlConnection conn = new NpgsqlConnection(await PGRead))
            {
                return await conn.QueryAsync(strSql, par);
            }
        }



        #endregion

        #region V2
        public async Task<List<SABA_BetDetails>> GetSabaRecordV2BySummary(DateTime partitionTime, DateTime ReportTime, string ClubId)
        {
            var par = new DynamicParameters();
            string strSql = @"SELECT trans_id, last_version_key, vendor_member_id, operator_id, league_id, match_id, sport_type, bet_type, parlay_ref_no,
                               settlement_time, transaction_time, winlost_datetime, stake, winlost, winlost_amount, islive, after_amount, balancechange,
                               currency , odds, odds_type, ticket_status, bet_team, leaguename_en, hometeamname_en, awayteamname_en ,turnover,
                               pre_stake , pre_winlost_amount, report_time, partition_time, create_time
                    FROM t_saba_bet_record_v2
                    WHERE  partition_time >= @start_date 
                           AND partition_time < @end_date 
                           AND  report_time = @reporttime 
                           AND vendor_member_id = @vendor_member_id ";
            par.Add("@reporttime", ReportTime);
            par.Add("@start_date", partitionTime);
            par.Add("@end_date", partitionTime.AddDays(1).AddMilliseconds(-1));
            par.Add("@vendor_member_id", ClubId);

            using (NpgsqlConnection conn = new NpgsqlConnection(await PGRead))
            {

                var res = await conn.QueryAsync<SABA_BetDetails>(strSql, par);
                return res.ToList();
            }
        }
        public async Task<IEnumerable<SABA_BetDetails>> GetSabaV2Record(NpgsqlConnection conn, IDbTransaction tran, SABA_BetDetails record_data)
        {
            var par = new DynamicParameters();
            string strSql = @"SELECT trans_id, last_version_key, vendor_member_id, operator_id, league_id, match_id, sport_type, bet_type, parlay_ref_no,
                               settlement_time, transaction_time, winlost_datetime, stake, winlost, winlost_amount, islive, after_amount, balancechange,
                               currency , odds, odds_type, ticket_status, bet_team, leaguename_en, hometeamname_en, awayteamname_en ,turnover,
                               pre_stake , pre_winlost_amount, report_time, partition_time, create_time
                    FROM t_saba_bet_record_v2
                    WHERE trans_id = @trans_id and partition_time > @partition_time";

            par.Add("@trans_id", record_data.trans_id);
            par.Add("@partition_time", record_data.winlost_datetime.GetValueOrDefault().AddDays(-2));
            var results = await conn.QueryAsync<SABA_BetDetails>(strSql, par, tran);
            return results;
        }
        public async Task<IEnumerable<SABA_BetDetails>> GetSabaV2Record(SABA_BetDetails record_data)
        {
            var par = new DynamicParameters();
            string strSql = @"SELECT trans_id, last_version_key, vendor_member_id, operator_id, league_id, match_id, sport_type, bet_type, parlay_ref_no,
                               settlement_time, transaction_time, winlost_datetime, stake, winlost, winlost_amount, islive, after_amount, balancechange,
                               currency , odds, odds_type, ticket_status, bet_team, leaguename_en, hometeamname_en, awayteamname_en ,turnover,
                               pre_stake , pre_winlost_amount, report_time, partition_time, create_time
                    FROM t_saba_bet_record_v2
                    WHERE trans_id = @trans_id
                    AND ticket_status = @ticket_status
                    AND partition_time > @partition_time
                    Limit 1";

            par.Add("@trans_id", record_data.trans_id);
            par.Add("@ticket_status", record_data.ticket_status);
            par.Add("@partition_time", record_data.winlost_datetime.GetValueOrDefault().AddDays(-2));

            using (NpgsqlConnection conn = new NpgsqlConnection(await PGRead))
            {
                return await conn.QueryAsync<SABA_BetDetails>(strSql, par);
            }
        }
        /// <summary>
        /// 寫入資料
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="tran"></param>
        /// <param name="betLogs"></param>
        /// <returns></returns>
        public async Task<int> PostSabaRecord(NpgsqlConnection conn, IDbTransaction tran, IEnumerable<SABA_BetDetails> betLogs)
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
            var sql = $@"CREATE TEMPORARY TABLE temp_t_saba_bet_record_v2_{tableGuid:N} 
                            ( LIKE t_saba_bet_record_v2  INCLUDING ALL );";
            return tran.Connection.ExecuteAsync(sql, transaction: tran);
        }
        private async Task<ulong> BulkInsertToTempTable(IDbTransaction tran, Guid tableGuid, IEnumerable<SABA_BetDetails> records)
        {
            if (tran is not NpgsqlTransaction npTran)
                throw new ArgumentException("Must be NpgsqlTransaction", nameof(tran));

            if (npTran.Connection == null)
                throw new ArgumentNullException(nameof(tran.Connection));

            await using var writer =
                await npTran.Connection.BeginBinaryImportAsync(
                    $@"COPY temp_t_saba_bet_record_v2_{tableGuid:N} (trans_id, last_version_key, vendor_member_id, operator_id, league_id, match_id,
                                    sport_type, bet_type, parlay_ref_no, settlement_time, transaction_time, winlost_datetime, stake, winlost,
                                    winlost_amount, islive, after_amount, balancechange, currency, odds, odds_type, ticket_status,bet_team, 
                                    leaguename_en, hometeamname_en, awayteamname_en ,turnover, pre_stake, pre_winlost_amount, report_time, partition_time
                                    ) FROM STDIN (FORMAT BINARY)");

            foreach (var mapping in records)
            {
                await writer.StartRowAsync();

                await writer.WriteAsync(mapping.trans_id ?? 0, NpgsqlDbType.Bigint); // int8
                await writer.WriteAsync(mapping.last_version_key, NpgsqlDbType.Bigint); // int8
                await writer.WriteAsync(mapping.vendor_member_id, NpgsqlDbType.Varchar); // varchar(32)
                await writer.WriteAsync(mapping.operator_id, NpgsqlDbType.Varchar); // varchar(20)
                await writer.WriteAsync(mapping.league_id ?? 0, NpgsqlDbType.Integer); // int4
                await writer.WriteAsync(mapping.match_id ?? 0, NpgsqlDbType.Integer); // int4
                await writer.WriteAsync(mapping.sport_type ?? 0, NpgsqlDbType.Integer); // int4
                await writer.WriteAsync(mapping.bet_type ?? 0, NpgsqlDbType.Integer); // int4
                await writer.WriteAsync(mapping.parlay_ref_no ?? 0, NpgsqlDbType.Bigint); // int8
                await writer.WriteAsync(mapping.settlement_time ?? DateTime.Now, NpgsqlDbType.Timestamp); // timestamp
                await writer.WriteAsync(mapping.transaction_time ?? DateTime.Now, NpgsqlDbType.Timestamp); // timestamp
                await writer.WriteAsync(mapping.winlost_datetime ?? DateTime.Now, NpgsqlDbType.Timestamp); // timestamp
                await writer.WriteAsync(mapping.stake ?? 0, NpgsqlDbType.Numeric); // numeric(30, 4)
                await writer.WriteAsync(mapping.winlost ?? 0, NpgsqlDbType.Numeric); // numeric(30, 4)
                await writer.WriteAsync(mapping.winlost_amount ?? 0, NpgsqlDbType.Numeric); // numeric(30, 4)
                await writer.WriteAsync(mapping.islive ?? 0, NpgsqlDbType.Smallint); // int2
                await writer.WriteAsync(mapping.after_amount ?? 0, NpgsqlDbType.Numeric); // numeric(30, 4)
                await writer.WriteAsync(mapping.balancechange ?? 0, NpgsqlDbType.Smallint); // int2
                await writer.WriteAsync(mapping.currency ?? 0, NpgsqlDbType.Integer); // int4
                await writer.WriteAsync(mapping.odds ?? 0, NpgsqlDbType.Numeric); // numeric(20, 4)
                await writer.WriteAsync(mapping.odds_type ?? 0, NpgsqlDbType.Integer); // int4
                await writer.WriteAsync(mapping.ticket_status, NpgsqlDbType.Varchar); // varchar(32)
                await writer.WriteAsync(mapping.bet_team, NpgsqlDbType.Varchar); // varchar
                await writer.WriteAsync(mapping.leaguename_en, NpgsqlDbType.Varchar); // varchar
                await writer.WriteAsync(mapping.hometeamname_en, NpgsqlDbType.Varchar); // varchar
                await writer.WriteAsync(mapping.awayteamname_en, NpgsqlDbType.Varchar); // varchar
                await writer.WriteAsync(mapping.turnover ?? 0, NpgsqlDbType.Numeric); // numeric(30, 4)
                await writer.WriteAsync(mapping.pre_stake ?? 0, NpgsqlDbType.Numeric); // numeric(30, 4)
                await writer.WriteAsync(mapping.pre_winlost_amount ?? 0, NpgsqlDbType.Numeric); // numeric(30, 4)
                await writer.WriteAsync(mapping.report_time, NpgsqlDbType.Timestamp); // timestamp
                await writer.WriteAsync(mapping.partition_time, NpgsqlDbType.Timestamp); // timestamp
            }

            return await writer.CompleteAsync();
        }
        private async Task<int> MergeFromTempTable(IDbTransaction tran, Guid tableGuid)
        {
            var sql = @$"INSERT INTO t_saba_bet_record_v2
                        SELECT trans_id, last_version_key, vendor_member_id, operator_id, league_id, match_id, sport_type, bet_type, parlay_ref_no,
                               settlement_time, transaction_time, winlost_datetime, stake, winlost, winlost_amount, islive, after_amount, balancechange,
                               currency , odds, odds_type, ticket_status, bet_team, leaguename_en, hometeamname_en, awayteamname_en ,turnover,
                               pre_stake , pre_winlost_amount, report_time, partition_time, create_time
                        FROM temp_t_saba_bet_record_v2_{tableGuid:N} AS  temp
                        WHERE NOT EXISTS 
                        (
                            SELECT NULL
                            FROM public.t_saba_bet_record_v2
                            WHERE partition_time = temp.partition_time 
                                  AND last_version_key = temp.last_version_key
                                  AND trans_id = temp.trans_id
                                  AND winlost_datetime = temp.winlost_datetime
                        )";
            return await tran.Connection.ExecuteAsync(sql, tran);
        }

        /// <summary>
        /// 寫入資料
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="tran"></param>
        /// <param name="betLogs"></param>
        /// <returns></returns>
        public async Task<int> PostSabaRunningRecord(NpgsqlConnection conn, IDbTransaction tran, IEnumerable<SABA_BetDetails> betLogs)
        {
            if (tran == null) throw new ArgumentNullException(nameof(tran));
            if (betLogs == null) throw new ArgumentNullException(nameof(betLogs));
            if (!betLogs.Any()) return 0;

            var tableGuid = Guid.NewGuid();
            //建立暫存表
            await CreateBetRunningRecordTempTable(tran, tableGuid);
            //將資料倒進暫存表
            await BulkInsertToRunningTempTable(tran, tableGuid, betLogs);
            //將資料由暫存表倒回主表(過濾重複)
            return await MergeFromRunningTempTable(tran, tableGuid);
        }
        private Task<int> CreateBetRunningRecordTempTable(IDbTransaction tran, Guid tableGuid)
        {
            var sql = $@"CREATE TEMPORARY TABLE temp_t_saba_bet_record_running_{tableGuid:N} 
                            ( LIKE t_saba_bet_record_running  INCLUDING ALL );";
            return tran.Connection.ExecuteAsync(sql, transaction: tran);
        }
        private async Task<ulong> BulkInsertToRunningTempTable(IDbTransaction tran, Guid tableGuid, IEnumerable<SABA_BetDetails> records)
        {
            if (tran is not NpgsqlTransaction npTran)
                throw new ArgumentException("Must be NpgsqlTransaction", nameof(tran));

            if (npTran.Connection == null)
                throw new ArgumentNullException(nameof(tran.Connection));

            await using var writer =
                await npTran.Connection.BeginBinaryImportAsync(
                    $@"COPY temp_t_saba_bet_record_running_{tableGuid:N} (trans_id, last_version_key, vendor_member_id, operator_id, league_id, match_id, sport_type,
                            bet_type, parlay_ref_no, settlement_time, transaction_time, winlost_datetime, stake, winlost, winlost_amount,
                            islive, after_amount, balancechange, currency, odds, odds_type, ticket_status, summary_id, bet_team,
                            leaguename_en, hometeamname_en, awayteamname_en, club_id, franchiser_id,turnover
                            ) FROM STDIN (FORMAT BINARY)");

            foreach (var mapping in records)
            {
                await writer.StartRowAsync();

                await writer.WriteAsync(mapping.trans_id ?? 0, NpgsqlDbType.Bigint); // int8
                await writer.WriteAsync(mapping.last_version_key, NpgsqlDbType.Bigint); // int8
                await writer.WriteAsync(mapping.vendor_member_id, NpgsqlDbType.Varchar); // varchar(32)
                await writer.WriteAsync(mapping.operator_id, NpgsqlDbType.Varchar); // varchar(20)
                await writer.WriteAsync(mapping.league_id ?? 0, NpgsqlDbType.Integer); // int4
                await writer.WriteAsync(mapping.match_id ?? 0, NpgsqlDbType.Integer); // int4
                await writer.WriteAsync(mapping.sport_type ?? 0, NpgsqlDbType.Integer); // int4
                await writer.WriteAsync(mapping.bet_type ?? 0, NpgsqlDbType.Integer); // int4
                await writer.WriteAsync(mapping.parlay_ref_no ?? 0, NpgsqlDbType.Bigint); // int8
                await writer.WriteAsync(mapping.settlement_time ?? DateTime.Now, NpgsqlDbType.Timestamp); // timestamp
                await writer.WriteAsync(mapping.transaction_time ?? DateTime.Now, NpgsqlDbType.Timestamp); // timestamp
                await writer.WriteAsync(mapping.winlost_datetime ?? DateTime.Now, NpgsqlDbType.Timestamp); // timestamp
                await writer.WriteAsync(mapping.stake ?? 0, NpgsqlDbType.Numeric); // numeric(30, 4)
                await writer.WriteAsync(mapping.winlost ?? 0, NpgsqlDbType.Numeric); // numeric(30, 4)
                await writer.WriteAsync(mapping.winlost_amount ?? 0, NpgsqlDbType.Numeric); // numeric(30, 4)
                await writer.WriteAsync(mapping.islive ?? 0, NpgsqlDbType.Smallint); // int2
                await writer.WriteAsync(mapping.after_amount ?? 0, NpgsqlDbType.Numeric); // numeric(30, 4)
                await writer.WriteAsync(mapping.balancechange ?? 0, NpgsqlDbType.Smallint); // int2
                await writer.WriteAsync(mapping.currency ?? 0, NpgsqlDbType.Integer); // int4
                await writer.WriteAsync(mapping.odds ?? 0, NpgsqlDbType.Numeric); // numeric(20, 4)
                await writer.WriteAsync(mapping.odds_type ?? 0, NpgsqlDbType.Integer); // int4
                await writer.WriteAsync(mapping.ticket_status, NpgsqlDbType.Varchar); // varchar(32)
                await writer.WriteAsync(mapping.summary_id, NpgsqlDbType.Uuid); // uuid
                await writer.WriteAsync(mapping.bet_team, NpgsqlDbType.Varchar); // varchar
                await writer.WriteAsync(mapping.leaguename_en, NpgsqlDbType.Varchar); // varchar
                await writer.WriteAsync(mapping.hometeamname_en, NpgsqlDbType.Varchar); // varchar
                await writer.WriteAsync(mapping.awayteamname_en, NpgsqlDbType.Varchar); // varchar
                await writer.WriteAsync(mapping.club_id, NpgsqlDbType.Varchar); // varchar
                await writer.WriteAsync(mapping.franchiser_id, NpgsqlDbType.Varchar); // varchar
                await writer.WriteAsync(mapping.turnover ?? 0, NpgsqlDbType.Numeric); // numeric(30, 4)
            }

            return await writer.CompleteAsync();
        }
        private async Task<int> MergeFromRunningTempTable(IDbTransaction tran, Guid tableGuid)
        {
            var sql = @$"INSERT INTO t_saba_bet_record_running
                        SELECT trans_id, last_version_key, vendor_member_id, operator_id, league_id, match_id, sport_type,
                                bet_type, parlay_ref_no, settlement_time, transaction_time, winlost_datetime, stake, winlost, winlost_amount,
                                islive, after_amount, balancechange, currency, odds, odds_type, ticket_status, summary_id, bet_team,
                                leaguename_en, hometeamname_en, awayteamname_en, club_id, franchiser_id,turnover
                        FROM temp_t_saba_bet_record_running_{tableGuid:N} AS  temp
                        WHERE NOT EXISTS 
                        (
                            SELECT NULL
                            FROM public.t_saba_bet_record_running
                            WHERE last_version_key = temp.last_version_key
                                  AND trans_id = temp.trans_id
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
        public async Task<IEnumerable<(int count, decimal netWin, decimal bet, decimal betValidBet, decimal jackpot, string userid, int game_type, DateTime partitionTime)>> SummaryGameRecord(DateTime reportTime, DateTime startTime, DateTime endTime)
        {
            var sql = @"SELECT CAST(count(1) AS INTEGER) AS count,
                        coalesce(SUM(winlost_amount),0) AS netWin,
                        coalesce(SUM(stake),0) AS bet,
                        coalesce(SUM(turnover),0) AS betValidBet,
                        0 as jackpot,
                        vendor_member_id as userid,
                        0 as game_type,
                        Date(partition_time) as partition_time
                        FROM t_saba_bet_record_v2
                        WHERE partition_time BETWEEN @start_time AND @end_time
                        AND report_time = @report_time
                        GROUP BY vendor_member_id,Date(partition_time)";

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


        #endregion V2

        #region t_Saba_bet_type
        public async Task<IEnumerable<SabaBetType>> GetSabaBetType()
        {
            var par = new DynamicParameters();
            string strSql = @"SELECT *
                    FROM t_saba_bet_type";
            using (NpgsqlConnection conn = new NpgsqlConnection(await PGRead))
            {
                return await conn.QueryAsync<SabaBetType>(strSql, par);
            }
        }
        #endregion

        #region t_saba_game_report
        public async Task<int> PostSabaReport(SABA_GetFinancialReportData report_data)
        {
            await using (var conn = new NpgsqlConnection(PGMaster))
            {
                string stSqlInsert = @"INSERT INTO t_saba_game_report
                (
	                merchant,
	                financialdate,
	                currency,
	                totalbetamount,
	                totalwinamount,
	                totalbetcount,
	                netamount,
                    membercount,
	                cancelticketcount,
	                cancelroundcount,
                    totalturnover
                )
                VALUES
                (
	                @merchant,
	                @financialdate,
	                @currency,
	                @totalbetamount,
	                @totalwinamount,
	                @totalbetcount,
	                @netamount,
                    @membercount,
	                @cancelticketcount,
	                @cancelroundcount,
                    @totalturnover
                )";
                return await conn.ExecuteAsync(stSqlInsert, report_data);
            }
        }
        public async Task<int> DeleteSabaReport(SABA_GetFinancialReportData report_data)
        {
            await using (var conn = new NpgsqlConnection(PGMaster))
            {
                string strSqlDel = @"DELETE FROM t_saba_game_report
                               WHERE financialdate=@financialdate";
                return await conn.ExecuteAsync(strSqlDel, report_data);
            }
        }
        #endregion
    }
}
