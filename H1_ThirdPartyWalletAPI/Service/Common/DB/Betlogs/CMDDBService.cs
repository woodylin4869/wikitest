using Dapper;
using H1_ThirdPartyWalletAPI.Model.Config;
using H1_ThirdPartyWalletAPI.Model.DataModel;
using H1_ThirdPartyWalletAPI.Model.DB.CMD.Response;
using H1_ThirdPartyWalletAPI.Model.Game.CMD368.Response;
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

    public interface ICMDDBService
    {
        Task<int> PostCMDRecord(NpgsqlConnection conn, IDbTransaction tran, IEnumerable<BetRecordResponse.Daet> betLogs);
        Task<int> PostCMDRecordRunning(NpgsqlConnection conn, IDbTransaction tran, IEnumerable<BetRecordResponse.Daet> betLogs);
        Task<int> DeleteCMDRecordRunning(IDbTransaction tran, string referenceno, DateTime transDate);
        Task<List<GetCMDRecordsBySummaryResponse>> GetCMDRecordsBySummary(GetBetRecordReq RecordReq);
        Task<List<GetCMDRecordsPKByBetTimeResponse>> GetCMDRecordsPKByBetTime(DateTime start, DateTime end);
        Task<List<GetCMDRecordsPreAmountByIdResponse>> GetCMDRecordsPreAmountById(IDbTransaction tran, string referenceno, DateTime transdate);
        Task<List<GetCMDRecordsPreAmountByIdResponse>> GetCMDRecordsV2PreAmountById(IDbTransaction tran, string referenceno, DateTime transdate);
        Task<List<GetCMDRunningRecordResponse>> GetCMDRunningRecord(GetBetRecordUnsettleReq RecordReq);
        Task<(int totalCount, decimal totalBetValid, decimal totalNetWin)> SumCMDBetRecordByBetTime(DateTime start, DateTime end);

        Task<IEnumerable<(int count, decimal netWin, decimal bet, decimal betValidBet, decimal jackpot, string userid, int game_type, DateTime partitionTime)>>
            SummaryGameRecord(DateTime reportTime, DateTime startTime, DateTime endTime);
    }
    public class CMDDBService: BetlogsDBServiceBase,ICMDDBService
    {
        public CMDDBService(ILogger<CMDDBService> logger, IBetLogsDbConnectionStringManager connectionStringManager) : base(logger, connectionStringManager)
        {
        }

        //public Task<int> PostCMDRecordback(IDbTransaction tran, IEnumerable<BetRecordResponse.Daet> betInfos)
        //{
        //    var sql = @"INSERT INTO public.t_cmd_bet_record
        //            (id
        //            ,sourcename
        //            ,referenceno
        //            ,soctransid
        //            ,isfirsthalf
        //            ,transdate
        //            ,ishomegive
        //            ,isbethome
        //            ,pre_betamount
        //            ,outstanding
        //            ,hdp
        //            ,odds
        //            ,currency
        //            ,pre_winamount
        //            ,exchangerate
        //            ,winlosestatus
        //            ,transtype
        //            ,dangerstatus
        //            ,memcommissionset
        //            ,memcommission
        //            ,betip
        //            ,homescore
        //            ,awayscore
        //            ,runhomescore
        //            ,runawayscore
        //            ,isrunning
        //            ,rejectreason
        //            ,sporttype
        //            ,choice
        //            ,workingdate
        //            ,oddstype
        //            ,matchdate
        //            ,hometeamid
        //            ,awayteamid
        //            ,leagueid
        //            ,specialid
        //            ,iscashout
        //            ,cashouttotal
        //            ,cashouttakeback
        //            ,cashoutwinloseamount
        //            ,betsource
        //            ,statuschange
        //            ,stateupdatets
        //            ,aosexcluding
        //            ,mmrpercent
        //            ,matchid
        //            ,matchgroupid
        //            ,betremarks
        //            ,isspecial
        //            ,summary_id
        //            ,betamount
        //            ,winamount)
        //            VALUES
        //            ( @id
        //            , @sourcename
        //            , @referenceno
        //            , @soctransid
        //            , @isfirsthalf
        //            , @TransDateFormatted
        //            , @ishomegive
        //            , @isbethome
        //            , @pre_betamount
        //            , @outstanding
        //            , @hdp
        //            , @odds
        //            , @currency
        //            , @pre_winamount
        //            , @exchangerate
        //            , @winlosestatus
        //            , @transtype
        //            , @dangerstatus
        //            , @memcommissionset
        //            , @memcommission
        //            , @betip
        //            , @homescore
        //            , @awayscore
        //            , @runhomescore
        //            , @runawayscore
        //            , @isrunning
        //            , @rejectreason
        //            , @sporttype
        //            , @choice
        //            , @workingdate
        //            , @oddstype
        //            , @matchdate
        //            , @hometeamid
        //            , @awayteamid
        //            , @leagueid
        //            , @specialid
        //            , @iscashout
        //            , @cashouttotal
        //            , @cashouttakeback
        //            , @cashoutwinloseamount
        //            , @betsource
        //            , @statuschange
        //            , @StateUpdateTsFormatted
        //            , @aosexcluding
        //            , @mmrpercent
        //            , @matchid
        //            , @matchgroupid
        //            , @betremarks
        //            , @isspecial
        //            , @summary_id
        //            , @betamount
        //            , @winamount);";

        //    return tran.Connection.ExecuteAsync(sql, betInfos, tran);
        //}

        //public Task<int> PostCMDRecordRunningback(IDbTransaction tran, IEnumerable<BetRecordResponse.Daet> betInfos)
        //{
        //    var sql = @"INSERT INTO public.t_cmd_bet_record_running
        //            (id
        //            ,sourcename
        //            ,referenceno
        //            ,soctransid
        //            ,isfirsthalf
        //            ,transdate
        //            ,ishomegive
        //            ,isbethome
        //            ,pre_betamount
        //            ,outstanding
        //            ,hdp
        //            ,odds
        //            ,currency
        //            ,pre_winamount
        //            ,exchangerate
        //            ,winlosestatus
        //            ,transtype
        //            ,dangerstatus
        //            ,memcommissionset
        //            ,memcommission
        //            ,betip
        //            ,homescore
        //            ,awayscore
        //            ,runhomescore
        //            ,runawayscore
        //            ,isrunning
        //            ,rejectreason
        //            ,sporttype
        //            ,choice
        //            ,workingdate
        //            ,oddstype
        //            ,matchdate
        //            ,hometeamid
        //            ,awayteamid
        //            ,leagueid
        //            ,specialid
        //            ,iscashout
        //            ,cashouttotal
        //            ,cashouttakeback
        //            ,cashoutwinloseamount
        //            ,betsource
        //            ,statuschange
        //            ,stateupdatets
        //            ,aosexcluding
        //            ,mmrpercent
        //            ,matchid
        //            ,matchgroupid
        //            ,betremarks
        //            ,isspecial
        //            ,summary_id
        //            ,club_id
        //            ,franchiser_id
        //            ,betamount
        //            ,winamount)
        //            VALUES
        //            ( @id
        //            , @sourcename
        //            , @referenceno
        //            , @soctransid
        //            , @isfirsthalf
        //            , @TransDateFormatted
        //            , @ishomegive
        //            , @isbethome
        //            , @pre_betamount
        //            , @outstanding
        //            , @hdp
        //            , @odds
        //            , @currency
        //            , @pre_winamount
        //            , @exchangerate
        //            , @winlosestatus
        //            , @transtype
        //            , @dangerstatus
        //            , @memcommissionset
        //            , @memcommission
        //            , @betip
        //            , @homescore
        //            , @awayscore
        //            , @runhomescore
        //            , @runawayscore
        //            , @isrunning
        //            , @rejectreason
        //            , @sporttype
        //            , @choice
        //            , @workingdate
        //            , @oddstype
        //            , @matchdate
        //            , @hometeamid
        //            , @awayteamid
        //            , @leagueid
        //            , @specialid
        //            , @iscashout
        //            , @cashouttotal
        //            , @cashouttakeback
        //            , @cashoutwinloseamount
        //            , @betsource
        //            , @statuschange
        //            , @StateUpdateTsFormatted
        //            , @aosexcluding
        //            , @mmrpercent
        //            , @matchid
        //            , @matchgroupid
        //            , @betremarks
        //            , @isspecial
        //            , @summary_id
        //            , @club_id
        //            , @franchiser_id
        //            , @betamount
        //            , @winamount);";

        //    return tran.Connection.ExecuteAsync(sql, betInfos, tran);
        //}

        public async Task<int> DeleteCMDRecordRunning(IDbTransaction tran, string referenceno, DateTime transDate)
        {
            var sql = "Delete from public.t_cmd_bet_record_running where referenceno = @referenceno and transdate = @transDate";

            return await tran.Connection.ExecuteAsync(sql, new { referenceno, transDate }, tran);
        }

        public async Task<List<GetCMDRecordsBySummaryResponse>> GetCMDRecordsBySummary(GetBetRecordReq RecordReq)
        {
            var sql = @"SELECT referenceno as Referenceno
                        , soctransid as SocTransId
                        , winlosestatus as WinLoseStatus
                        , transdate as TransDate
                        , sporttype as SportType
                        , leagueId as LeagueId
                        , stateupdatets as StateUpdateTs
                        , isbethome as IsBetHome
                        , betamount as BetAmount
                        , transtype as TransType
                        , odds as Odds
                        , oddstype as OddsType
                        , winamount as WinAmount
                        , hometeamid as HomeTeamId
                        , awayteamid as AwayTeamId
                    FROM public.t_cmd_bet_record 
                    WHERE transdate >= @start 
                        AND transdate < @end
                        AND summary_id = @summaryId::uuid";
            var parm = new
            {
                summaryId = RecordReq.summary_id,
                start = RecordReq.ReportTime,
                end = RecordReq.ReportTime.AddDays(1),
            };

            await using var conn = new NpgsqlConnection(await PGRead);
            var result = await conn.QueryAsync<GetCMDRecordsBySummaryResponse>(sql, parm);
            return result.ToList();
        }

        /// <summary>
        /// 依下注時間取得注單PK(id, winlosestatus, transdate, stateupdatets)
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public async Task<List<GetCMDRecordsPKByBetTimeResponse>> GetCMDRecordsPKByBetTime(DateTime start, DateTime end)
        {
            var sql = @"
                    SELECT referenceno, winlosestatus, transdate, stateupdatets
                    FROM t_cmd_bet_record
                    WHERE transdate >= @startTime 
                        AND transdate <= @endTime";

            var par = new DynamicParameters();
            par.Add("@startTime", start);
            par.Add("@endTime", end);

            await using var conn = new NpgsqlConnection(await PGRead);
            var result = await conn.QueryAsync<GetCMDRecordsPKByBetTimeResponse>(sql, par);
            return result.ToList();
        }

        public async Task<List<GetCMDRecordsPreAmountByIdResponse>> GetCMDRecordsPreAmountById(IDbTransaction tran, string referenceno, DateTime transdate)
        {
            var sql = @"
                    SELECT referenceno, winlosestatus, transdate, stateupdatets, pre_betamount, pre_winamount 
                    FROM t_cmd_bet_record
                    WHERE transdate = @transdate
                        AND referenceno = @referenceno";

            var par = new DynamicParameters();
            par.Add("@referenceno", referenceno);
            par.Add("@transdate", transdate);

            var result = await tran.Connection.QueryAsync<GetCMDRecordsPreAmountByIdResponse>(sql, par, tran);
            return result.ToList();
        }

        public async Task<List<GetCMDRunningRecordResponse>> GetCMDRunningRecord(GetBetRecordUnsettleReq RecordReq)
        {
            var par = new DynamicParameters();
            var sql = @"SELECT referenceno as Referenceno
                        , winlosestatus as WinLoseStatus
                        , transdate as TransDate
                        , sporttype as SportType
                        , leagueId as LeagueId
                        , stateupdatets as StateUpdateTs
                        , isbethome as IsBetHome
                        , betamount as BetAmount
                        , transtype as TransType
                        , odds as Odds
                        , oddstype as OddsType
                        , winamount as WinAmount
                        , hometeamid as HomeTeamId
                        , awayteamid as AwayTeamId
                        , club_id as Club_id
                        , soctransid as SocTransId
                        , franchiser_id as Franchiser_id
                    FROM public.t_cmd_bet_record_running
                    WHERE transdate >= @start 
                        AND transdate < @end";

            if (RecordReq.Club_id != null)
            {
                par.Add("@Club_id", RecordReq.Club_id);
                sql += " AND Club_id = @Club_id";
            }
            if (RecordReq.Franchiser_id != null)
            {
                par.Add("@Franchiser_id", RecordReq.Franchiser_id);
                sql += " AND Franchiser_id = @Franchiser_id";
            }

            par.Add("@start", RecordReq.StartTime != null ? RecordReq.StartTime : DateTime.Now.AddDays(-100));
            par.Add("@end", RecordReq.EndTime != null ? RecordReq.EndTime : DateTime.Now);

            using NpgsqlConnection conn = new NpgsqlConnection(await PGRead);
            var result = await conn.QueryAsync<GetCMDRunningRecordResponse>(sql, par);
            return result.ToList();
        }

        public async Task<(int totalCount, decimal totalBetValid, decimal totalNetWin)> SumCMDBetRecordByBetTime(DateTime start, DateTime end)
        {
            var sql = @"SELECT 
                    COUNT(Distinct referenceno) AS totalCount
                    , CASE WHEN SUM(betamount) IS NULL THEN 0 ELSE SUM(betamount) END  AS totalBetValid
                    , CASE WHEN SUM(winamount) IS NULL THEN 0 ELSE SUM(winamount) END AS totalNetWin
                    FROM t_cmd_bet_record
                    WHERE transdate >= @startTime 
                        AND transdate < @endTime";

            var par = new DynamicParameters();
            par.Add("@startTime", start);
            par.Add("@endTime", end);

            await using var conn = new NpgsqlConnection(await PGRead);
            var result = (await conn.QueryAsync<dynamic>(sql, par)).FirstOrDefault();
            return ((int)result.totalcount, (decimal)result.totalbetvalid, (decimal)result.totalnetwin);
        }

        #region back

        /// <summary>
        /// 寫入注單資料
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="tran"></param>
        /// <param name="betLogs"></param>
        /// <returns></returns>
        public async Task<int> PostCMDRecord(NpgsqlConnection conn, IDbTransaction tran, IEnumerable<BetRecordResponse.Daet> betLogs)
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
            var sql = $@"CREATE TEMPORARY TABLE temp_t_cmd_bet_record_v2_{tableGuid:N} 
                            ( LIKE t_cmd_bet_record_v2  INCLUDING ALL );";
            return tran.Connection.ExecuteAsync(sql, transaction: tran);
        }
        private async Task<ulong> BulkInsertToTempTable(IDbTransaction tran, Guid tableGuid, IEnumerable<BetRecordResponse.Daet> records)
        {
            if (tran is not NpgsqlTransaction npTran)
                throw new ArgumentException("Must be NpgsqlTransaction", nameof(tran));

            if (npTran.Connection == null)
                throw new ArgumentNullException(nameof(tran.Connection));

            await using var writer =
                await npTran.Connection.BeginBinaryImportAsync(
                    $@"COPY temp_t_cmd_bet_record_v2_{tableGuid:N} (id, sourcename, referenceno, soctransid, isfirsthalf, transdate, 
                            ishomegive, isbethome, betamount, outstanding, hdp, odds, currency, winamount, exchangerate, winlosestatus, transtype,
                            dangerstatus, memcommissionset, memcommission, betip, homescore, awayscore, runhomescore, runawayscore, isrunning, rejectreason,
                            sporttype, choice, workingdate, oddstype, matchdate, hometeamid, awayteamid, leagueid, specialid, iscashout, cashouttotal,
                            cashouttakeback, cashoutwinloseamount, betsource, statuschange, stateupdatets, aosexcluding, mmrpercent, matchid, matchgroupid, betremarks,
                            isspecial, pre_betamount, pre_winamount, report_time, partition_time,validbet) FROM STDIN (FORMAT BINARY)");

            foreach (var mapping in records)
            {
                await writer.StartRowAsync();
                await writer.WriteAsync(mapping.Id, NpgsqlDbType.Bigint);  // int8
                await writer.WriteAsync(mapping.SourceName, NpgsqlDbType.Varchar);  // varchar(20)
                await writer.WriteAsync(mapping.ReferenceNo, NpgsqlDbType.Varchar);  // varchar(50)
                await writer.WriteAsync(mapping.SocTransId ,  NpgsqlDbType.Bigint);  // int8
                await writer.WriteAsync(mapping.IsFirstHalf, NpgsqlDbType.Boolean);  // bool
                await writer.WriteAsync(mapping.TransDateFormatted, NpgsqlDbType.Timestamp);  // timestamp
                await writer.WriteAsync(mapping.IsHomeGive, NpgsqlDbType.Boolean);  // bool
                await writer.WriteAsync(mapping.IsBetHome, NpgsqlDbType.Boolean);  // bool
                await writer.WriteAsync(mapping.BetAmount, NpgsqlDbType.Numeric);  // numeric(18, 2)
                await writer.WriteAsync(mapping.Outstanding, NpgsqlDbType.Numeric);  // numeric(18, 4)
                await writer.WriteAsync(mapping.Hdp, NpgsqlDbType.Numeric);  // numeric(18, 2)
                await writer.WriteAsync(mapping.Odds, NpgsqlDbType.Numeric);  // numeric(18, 2)
                await writer.WriteAsync(mapping.Currency, NpgsqlDbType.Varchar);  // varchar(5)
                await writer.WriteAsync(mapping.WinAmount, NpgsqlDbType.Numeric);  // numeric(18, 2)
                await writer.WriteAsync(mapping.ExchangeRate, NpgsqlDbType.Numeric);  // numeric(18, 2)
                await writer.WriteAsync(mapping.WinLoseStatus, NpgsqlDbType.Varchar);  // varchar(5)
                await writer.WriteAsync(mapping.TransType, NpgsqlDbType.Varchar);  // varchar(5)
                await writer.WriteAsync(mapping.DangerStatus, NpgsqlDbType.Varchar);  // varchar(2)
                await writer.WriteAsync(mapping.MemCommissionSet, NpgsqlDbType.Numeric);  // numeric(18, 4)
                await writer.WriteAsync(mapping.MemCommission, NpgsqlDbType.Numeric);  // numeric(18, 2)
                await writer.WriteAsync(mapping.BetIp, NpgsqlDbType.Varchar);  // varchar(50)
                await writer.WriteAsync(mapping.HomeScore, NpgsqlDbType.Integer);  // int4
                await writer.WriteAsync(mapping.AwayScore, NpgsqlDbType.Integer);  // int4
                await writer.WriteAsync(mapping.RunHomeScore, NpgsqlDbType.Integer);  // int4
                await writer.WriteAsync(mapping.RunAwayScore, NpgsqlDbType.Integer);  // int4
                await writer.WriteAsync(mapping.IsRunning, NpgsqlDbType.Boolean);  // bool
                await writer.WriteAsync(mapping.RejectReason, NpgsqlDbType.Varchar);  // varchar(100)
                await writer.WriteAsync(mapping.SportType, NpgsqlDbType.Varchar);  // varchar(5)
                await writer.WriteAsync(mapping.Choice, NpgsqlDbType.Varchar);  // varchar
                await writer.WriteAsync(mapping.WorkingDate, NpgsqlDbType.Integer);  // int4
                await writer.WriteAsync(mapping.OddsType, NpgsqlDbType.Varchar);  // varchar(2)
                await writer.WriteAsync(mapping.MatchDate, NpgsqlDbType.Bigint);  // int8
                await writer.WriteAsync(mapping.HomeTeamId, NpgsqlDbType.Bigint);  // int8
                await writer.WriteAsync(mapping.AwayTeamId, NpgsqlDbType.Bigint);  // int8
                await writer.WriteAsync(mapping.LeagueId, NpgsqlDbType.Bigint);  // int8
                await writer.WriteAsync(mapping.SpecialId, NpgsqlDbType.Varchar);  // varchar(10)
                await writer.WriteAsync(mapping.IsCashOut, NpgsqlDbType.Boolean);  // bool
                await writer.WriteAsync(mapping.CashOutTotal, NpgsqlDbType.Numeric);  // numeric(18, 4)
                await writer.WriteAsync(mapping.CashOutTakeBack, NpgsqlDbType.Numeric);  // numeric(18, 4)
                await writer.WriteAsync(mapping.CashOutWinLoseAmount, NpgsqlDbType.Numeric);  // numeric(18, 4)
                await writer.WriteAsync(mapping.BetSource, NpgsqlDbType.Integer);  // int4
                await writer.WriteAsync(mapping.StatusChange, NpgsqlDbType.Integer);  // int4
                await writer.WriteAsync(mapping.StateUpdateTsFormatted, NpgsqlDbType.Timestamp);  // timestamp
                await writer.WriteAsync(mapping.AOSExcluding, NpgsqlDbType.Varchar);  // varchar(512)
                await writer.WriteAsync(mapping.MMRPercent, NpgsqlDbType.Numeric);  // numeric(18, 4)
                await writer.WriteAsync(mapping.MatchID, NpgsqlDbType.Integer);  // int4
                await writer.WriteAsync(mapping.MatchGroupID, NpgsqlDbType.Varchar);  // varchar(50)
                await writer.WriteAsync(mapping.BetRemarks, NpgsqlDbType.Varchar);  // varchar(128)
                await writer.WriteAsync(mapping.IsSpecial, NpgsqlDbType.Boolean);  // bool
                await writer.WriteAsync(mapping.pre_betamount, NpgsqlDbType.Numeric);  // numeric(18, 2)
                await writer.WriteAsync(mapping.pre_winamount, NpgsqlDbType.Numeric);  // numeric(18, 4)
                await writer.WriteAsync(mapping.report_time, NpgsqlDbType.Timestamp);  // timestamp
                await writer.WriteAsync(mapping.partition_time, NpgsqlDbType.Timestamp);  // timestamp
                await writer.WriteAsync(mapping.validbet, NpgsqlDbType.Numeric);  // numeric(18, 2)
            }
            return await writer.CompleteAsync();
        }
        private async Task<int> MergeFromTempTable(IDbTransaction tran, Guid tableGuid)
        {
            var sql = @$"INSERT INTO t_cmd_bet_record_v2
                        SELECT id, sourcename, referenceno, soctransid, isfirsthalf, transdate, 
                                ishomegive, isbethome, betamount, outstanding, hdp, odds, currency, winamount, exchangerate, winlosestatus, transtype,
                                dangerstatus, memcommissionset, memcommission, betip, homescore, awayscore, runhomescore, runawayscore, isrunning, rejectreason,
                                sporttype, choice, workingdate, oddstype, matchdate, hometeamid, awayteamid, leagueid, specialid, iscashout, cashouttotal,
                                cashouttakeback, cashoutwinloseamount, betsource, statuschange, stateupdatets, aosexcluding, mmrpercent, matchid, matchgroupid, betremarks,
                                isspecial, pre_betamount, pre_winamount, create_time, report_time, partition_time,validbet
                        FROM temp_t_cmd_bet_record_v2_{tableGuid:N} AS  temp
                        WHERE NOT EXISTS 
                        (
                            SELECT NULL
                            FROM public.t_cmd_bet_record_v2
                            WHERE partition_time = temp.partition_time
                                AND stateupdatets = temp.stateupdatets
                                AND winlosestatus = temp.winlosestatus
                                AND referenceno = temp.referenceno 
                        )";
            return await tran.Connection.ExecuteAsync(sql, tran);
        }

        /// <summary>
        /// 寫入注單資料
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="tran"></param>
        /// <param name="betLogs"></param>
        /// <returns></returns>
        public async Task<int> PostCMDRecordRunning(NpgsqlConnection conn, IDbTransaction tran, IEnumerable<BetRecordResponse.Daet> betLogs)
        {
            if (tran == null) throw new ArgumentNullException(nameof(tran));
            if (betLogs == null) throw new ArgumentNullException(nameof(betLogs));
            if (!betLogs.Any()) return 0;

            var tableGuid = Guid.NewGuid();
            //建立暫存表
            await CreateBetRecordRunningTempTable(tran, tableGuid);
            //將資料倒進暫存表
            await BulkInsertToRunningTempTable(tran, tableGuid, betLogs);
            //將資料由暫存表倒回主表(過濾重複)
            return await MergeFromRunningTempTable(tran, tableGuid);
        }
        private Task<int> CreateBetRecordRunningTempTable(IDbTransaction tran, Guid tableGuid)
        {
            var sql = $@"CREATE TEMPORARY TABLE temp_t_cmd_bet_record_running_{tableGuid:N} 
                            ( LIKE t_cmd_bet_record_running  INCLUDING ALL );";
            return tran.Connection.ExecuteAsync(sql, transaction: tran);
        }
        private async Task<ulong> BulkInsertToRunningTempTable(IDbTransaction tran, Guid tableGuid, IEnumerable<BetRecordResponse.Daet> records)
        {
            if (tran is not NpgsqlTransaction npTran)
                throw new ArgumentException("Must be NpgsqlTransaction", nameof(tran));

            if (npTran.Connection == null)
                throw new ArgumentNullException(nameof(tran.Connection));

            await using var writer =
                await npTran.Connection.BeginBinaryImportAsync(
                    $@"COPY temp_t_cmd_bet_record_running_{tableGuid:N} (id, sourcename, referenceno, soctransid, isfirsthalf, transdate, ishomegive, 
                            isbethome, betamount, outstanding, hdp, odds, currency, winamount, exchangerate, winlosestatus, transtype, dangerstatus,
                            memcommissionset, memcommission, betip, homescore, awayscore, runhomescore, runawayscore, isrunning, rejectreason, sporttype,
                            choice, workingdate, oddstype, matchdate, hometeamid, awayteamid, leagueid, specialid, iscashout, cashouttotal, cashouttakeback,
                            cashoutwinloseamount, betsource, statuschange, stateupdatets, aosexcluding, mmrpercent, matchid, matchgroupid, betremarks, isspecial,
                            summary_id, pre_betamount, pre_winamount, club_id, franchiser_id,validbet
                            ) FROM STDIN (FORMAT BINARY)");

            foreach (var mapping in records)
            {
                await writer.StartRowAsync();

                await writer.WriteAsync(mapping.Id, NpgsqlDbType.Bigint);  // int8
                await writer.WriteAsync(mapping.SourceName, NpgsqlDbType.Varchar);  // varchar(20)
                await writer.WriteAsync(mapping.ReferenceNo, NpgsqlDbType.Varchar);  // varchar(50)
                await writer.WriteAsync(mapping.SocTransId, NpgsqlDbType.Bigint);  // int8
                await writer.WriteAsync(mapping.IsFirstHalf, NpgsqlDbType.Boolean);  // bool
                await writer.WriteAsync(mapping.TransDateFormatted, NpgsqlDbType.Timestamp);  // timestamp
                await writer.WriteAsync(mapping.IsHomeGive, NpgsqlDbType.Boolean);  // bool
                await writer.WriteAsync(mapping.IsBetHome, NpgsqlDbType.Boolean);  // bool
                await writer.WriteAsync(mapping.BetAmount, NpgsqlDbType.Numeric);  // numeric(18, 2)
                await writer.WriteAsync(mapping.Outstanding, NpgsqlDbType.Numeric);  // numeric(18, 4)
                await writer.WriteAsync(mapping.Hdp, NpgsqlDbType.Numeric);  // numeric(18, 2)
                await writer.WriteAsync(mapping.Odds, NpgsqlDbType.Numeric);  // numeric(18, 2)
                await writer.WriteAsync(mapping.Currency, NpgsqlDbType.Varchar);  // varchar(5)
                await writer.WriteAsync(mapping.WinAmount, NpgsqlDbType.Numeric);  // numeric(18, 2)
                await writer.WriteAsync(mapping.ExchangeRate, NpgsqlDbType.Numeric);  // numeric(18, 2)
                await writer.WriteAsync(mapping.WinLoseStatus, NpgsqlDbType.Varchar);  // varchar(5)
                await writer.WriteAsync(mapping.TransType, NpgsqlDbType.Varchar);  // varchar(5)
                await writer.WriteAsync(mapping.DangerStatus, NpgsqlDbType.Varchar);  // varchar(2)
                await writer.WriteAsync(mapping.MemCommissionSet, NpgsqlDbType.Numeric);  // numeric(18, 4)
                await writer.WriteAsync(mapping.MemCommission, NpgsqlDbType.Numeric);  // numeric(18, 2)
                await writer.WriteAsync(mapping.BetIp, NpgsqlDbType.Varchar);  // varchar(50)
                await writer.WriteAsync(mapping.HomeScore, NpgsqlDbType.Integer);  // int4
                await writer.WriteAsync(mapping.AwayScore, NpgsqlDbType.Integer);  // int4
                await writer.WriteAsync(mapping.RunHomeScore, NpgsqlDbType.Integer);  // int4
                await writer.WriteAsync(mapping.RunAwayScore, NpgsqlDbType.Integer);  // int4
                await writer.WriteAsync(mapping.IsRunning, NpgsqlDbType.Boolean);  // bool
                await writer.WriteAsync(mapping.RejectReason, NpgsqlDbType.Varchar);  // varchar(100)
                await writer.WriteAsync(mapping.SportType, NpgsqlDbType.Varchar);  // varchar(5)
                await writer.WriteAsync(mapping.Choice, NpgsqlDbType.Varchar);  // varchar
                await writer.WriteAsync(mapping.WorkingDate, NpgsqlDbType.Integer);  // int4
                await writer.WriteAsync(mapping.OddsType, NpgsqlDbType.Varchar);  // varchar(2)
                await writer.WriteAsync(mapping.MatchDate, NpgsqlDbType.Bigint);  // int8
                await writer.WriteAsync(mapping.HomeTeamId, NpgsqlDbType.Bigint);  // int8
                await writer.WriteAsync(mapping.AwayTeamId, NpgsqlDbType.Bigint);  // int8
                await writer.WriteAsync(mapping.LeagueId, NpgsqlDbType.Bigint);  // int8
                await writer.WriteAsync(mapping.SpecialId, NpgsqlDbType.Varchar);  // varchar(10)
                await writer.WriteAsync(mapping.IsCashOut, NpgsqlDbType.Boolean);  // bool
                await writer.WriteAsync(mapping.CashOutTotal, NpgsqlDbType.Numeric);  // numeric(18, 4)
                await writer.WriteAsync(mapping.CashOutTakeBack, NpgsqlDbType.Numeric);  // numeric(18, 4)
                await writer.WriteAsync(mapping.CashOutWinLoseAmount, NpgsqlDbType.Numeric);  // numeric(18, 4)
                await writer.WriteAsync(mapping.BetSource, NpgsqlDbType.Integer);  // int4
                await writer.WriteAsync(mapping.StatusChange, NpgsqlDbType.Integer);  // int4
                await writer.WriteAsync(mapping.StateUpdateTsFormatted, NpgsqlDbType.Timestamp);  // timestamp
                await writer.WriteAsync(mapping.AOSExcluding, NpgsqlDbType.Varchar);  // varchar(512)
                await writer.WriteAsync(mapping.MMRPercent, NpgsqlDbType.Numeric);  // numeric(18, 4)
                await writer.WriteAsync(mapping.MatchID, NpgsqlDbType.Integer);  // int4
                await writer.WriteAsync(mapping.MatchGroupID, NpgsqlDbType.Varchar);  // varchar(50)
                await writer.WriteAsync(mapping.BetRemarks, NpgsqlDbType.Varchar);  // varchar(128)
                await writer.WriteAsync(mapping.IsSpecial, NpgsqlDbType.Boolean);  // bool
                await writer.WriteAsync(mapping.summary_id, NpgsqlDbType.Uuid);  // varchar(128)
                await writer.WriteAsync(mapping.pre_betamount, NpgsqlDbType.Numeric);  // numeric(18, 2)
                await writer.WriteAsync(mapping.pre_winamount, NpgsqlDbType.Numeric);  // numeric(18, 4)
                await writer.WriteAsync(mapping.club_id, NpgsqlDbType.Varchar);  // varchar(50)
                await writer.WriteAsync(mapping.franchiser_id, NpgsqlDbType.Varchar);  // varchar(50)
                await writer.WriteAsync(mapping.validbet, NpgsqlDbType.Numeric);  // numeric(18, 2)
            }

            return await writer.CompleteAsync();
        }
        private async Task<int> MergeFromRunningTempTable(IDbTransaction tran, Guid tableGuid)
        {
            var sql = @$"INSERT INTO t_cmd_bet_record_running
                        SELECT id, sourcename, referenceno, soctransid, isfirsthalf, transdate, ishomegive, 
                            isbethome, betamount, outstanding, hdp, odds, currency, winamount, exchangerate, winlosestatus, transtype, dangerstatus,
                            memcommissionset, memcommission, betip, homescore, awayscore, runhomescore, runawayscore, isrunning, rejectreason, sporttype,
                            choice, workingdate, oddstype, matchdate, hometeamid, awayteamid, leagueid, specialid, iscashout, cashouttotal, cashouttakeback,
                            cashoutwinloseamount, betsource, statuschange, stateupdatets, aosexcluding, mmrpercent, matchid, matchgroupid, betremarks, isspecial,
                            summary_id, pre_betamount, pre_winamount, club_id, franchiser_id,validbet
                        FROM temp_t_cmd_bet_record_running_{tableGuid:N} AS  temp
                        WHERE NOT EXISTS 
                        (
                            SELECT NULL
                            FROM public.t_cmd_bet_record_running
                            WHERE referenceno = temp.referenceno 
                                AND stateupdatets = temp.stateupdatets
                                AND winlosestatus = temp.winlosestatus
                                AND transdate = temp.transdate
                        )"; 
            return await tran.Connection.ExecuteAsync(sql, tran);
        }

        public async Task<List<GetCMDRecordsPreAmountByIdResponse>> GetCMDRecordsV2PreAmountById(IDbTransaction tran, string referenceno, DateTime transdate)
        {
            var sql = @"
                    SELECT referenceno, winlosestatus, transdate, stateupdatets, pre_betamount, pre_winamount 
                    FROM t_cmd_bet_record_v2
                    WHERE partition_time = @partition_time
                        AND referenceno = @referenceno";

            var par = new DynamicParameters();
            par.Add("@referenceno", referenceno);
            par.Add("@partition_time", transdate);

            var result = await tran.Connection.QueryAsync<GetCMDRecordsPreAmountByIdResponse>(sql, par, tran);
            return result.ToList();
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
                        coalesce(SUM(winamount),0) AS netWin,
                        coalesce(SUM(betamount),0) AS bet,
                        coalesce(SUM(validbet),0) AS betValidBet,
                        0 as jackpot,
                        sourcename as userid,
                        0 as game_type,
                        Date(partition_time) as partition_time
                        FROM t_cmd_bet_record_v2
                        WHERE partition_time BETWEEN @start_time AND @end_time
                        AND report_time = @report_time
                        GROUP BY sourcename,Date(partition_time)";

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

        #endregion
    }
}
