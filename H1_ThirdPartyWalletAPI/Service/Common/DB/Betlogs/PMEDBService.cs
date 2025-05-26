using Dapper;
using H1_ThirdPartyWalletAPI.Model.Config;
using H1_ThirdPartyWalletAPI.Model.DataModel;
using H1_ThirdPartyWalletAPI.Model.W1API;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using H1_ThirdPartyWalletAPI.Model.DB.PME.Response;
using H1_ThirdPartyWalletAPI.Model.Game.META.Response;
using H1_ThirdPartyWalletAPI.Model.Game.PME.Response;
using H1_ThirdPartyWalletAPI.Utility;
using NpgsqlTypes;
using ThirdPartyWallet.Share.Model.Game.PS.Response;

namespace H1_ThirdPartyWalletAPI.Service.Common.DB.Betlogs;

public interface IPMEDBService
{
    Task<int> PostPMERecord(IDbTransaction tran, IEnumerable<QueryScrollResponse.Bet> betInfos);
    Task<int> PostPMERecordRunning(NpgsqlConnection conn, IDbTransaction tran, IEnumerable<QueryScrollResponse.Bet> betLogs);
    Task<int> DeletePMERecordRunning(IDbTransaction tran, long id, DateTime betTime);
    Task<List<GetPMERecordsBySummaryResponse>> GetPMERecordsBySummary(GetBetRecordReq RecordReq);
    Task<List<GetPMERecordsPKByBetTimeResponse>> GetPMERecordsPKByBetTime(DateTime start, DateTime end);
    Task<List<GetPMERecordsPreAmountByIdResponse>> GetPMERecordsPreAmountById(IDbTransaction tran, long id, DateTime bettime);
    Task<List<GetPMERunningRecordResponse>> GetPMERunningRecord(GetBetRecordUnsettleReq RecordReq);
    Task<(int totalCount, decimal totalBetValid, decimal totalNetWin)> SumPMEBetRecordByBetTime(DateTime start, DateTime end);
    Task<IEnumerable<(int count, decimal win, decimal bet, decimal jackpot, string userid, decimal netwin, DateTime bettime)>> SummaryGameRecord(DateTime reportTime, DateTime startTime, DateTime endTime);
    Task<int> PostPMERecord_V2(NpgsqlConnection conn, IDbTransaction tran, IEnumerable<QueryScrollResponse.Bet> betLogs);
    Task<List<GetPMERecordsBySummaryResponse>> GetPmeRecordsBytime(DateTime createtime, DateTime report_time, string club_id);
    Task<List<GetPMERecordsPreAmountByIdResponse>> GetPMEV2RecordsPreAmountById(IDbTransaction tran, long id, DateTime bettime);

}

public class PMEDBService : BetlogsDBServiceBase, IPMEDBService
{
    public PMEDBService(ILogger<PMEDBService> logger, IBetLogsDbConnectionStringManager connectionStringManager) : base(logger, connectionStringManager)
    {
    }

    public Task<int> PostPMERecord(IDbTransaction tran, IEnumerable<QueryScrollResponse.Bet> betInfos)
    {
        var sql = @"INSERT INTO public.t_pme_bet_record
                    (id
                    , member_id
                    , member_account
                    , merchant_id
                    , merchant_account
                    , parent_merchant_id
                    , parent_merchant_account
                    , tester
                    , order_type
                    , parley_type
                    , game_id
                    , tournament_id
                    , tournament
                    , match_id
                    , match_type
                    , market_id
                    , market_cn_name
                    , team_id
                    , team_names
                    , team_cn_names
                    , team_en_names
                    , odd_id
                    , odd_name
                    , round
                    , odd
                    , bet_amount
                    , win_amount
                    , is_live
                    , bet_status
                    , confirm_type
                    , bet_time
                    , settle_time
                    , match_start_time
                    , update_time
                    , settle_count
                    , device
                    , bet_ip
                    , score_benchmark
                    , currency_code
                    , exchange_rate
                    , summary_id
                    , pre_bet_amount
                    , pre_win_amount)
                    VALUES
                    (@id
                    , @member_id
                    , @member_account
                    , @merchant_id
                    , @merchant_account
                    , @parent_merchant_id
                    , @parent_merchant_account
                    , @tester
                    , @order_type
                    , @parley_type
                    , @game_id
                    , @tournament_id
                    , @tournament
                    , @match_id
                    , @match_type
                    , @market_id
                    , @market_cn_name
                    , @team_id
                    , @team_names
                    , @team_cn_names
                    , @team_en_names
                    , @odd_id
                    , @odd_name
                    , @round
                    , @odd
                    , @bet_amount
                    , @win_amount
                    , @is_live
                    , @bet_status
                    , @confirm_type
                    , @BetTimeFormatted
                    , @SettleTimeFormatted
                    , @MatchStartTimeFormatted
                    , @UpdateTimeFormatted
                    , @settle_count
                    , @device
                    , @bet_ip
                    , @score_benchmark
                    , @currency_code
                    , @exchange_rate
                    , @summary_id
                    , @pre_bet_amount
                    , @pre_win_amount);";

        return tran.Connection.ExecuteAsync(sql, betInfos, tran);
    }

 

    public async Task<int> DeletePMERecordRunning(IDbTransaction tran, long id, DateTime betTime)
    {
        var sql = "Delete from public.t_pme_bet_record_running where id = @id and bet_time = @betTime";

        return await tran.Connection.ExecuteAsync(sql, new { id, betTime }, tran);
    }

    public async Task<List<GetPMERecordsBySummaryResponse>> GetPMERecordsBySummary(GetBetRecordReq RecordReq)
    {
        var sql = @"SELECT id
                        , bet_status
                        , bet_time
                        , update_time
                        , game_id
                        , tournament
                        , split_part(team_en_names,',', 1) as hometeam
                        , split_part(team_en_names,',', 2) as awayteam
                        , settle_time
                        , bet_amount
                        , odd_name
                        , odd
                        , win_amount
                    FROM public.t_pme_bet_record 
                    WHERE bet_time >= @start 
                        AND bet_time < @end
                        AND summary_id = @summaryId::uuid";
        var param = new
        {
            summaryId = RecordReq.summary_id,
            start = RecordReq.ReportTime.AddDays(-1),
            end = RecordReq.ReportTime,
        };

        await using var conn = new NpgsqlConnection(await PGRead);
        var result = await conn.QueryAsync<GetPMERecordsBySummaryResponse>(sql, param);
        return result.ToList();
    }
    /// <summary>
    /// report_time 及時間區間
    /// </summary>
    /// <param name="createtime"></param>
    /// <param name="report_time"></param>
    /// <returns></returns>
    public async Task<List<GetPMERecordsBySummaryResponse>> GetPmeRecordsBytime(DateTime createtime, DateTime report_time, string club_id)
    {
        try
        {
            var sql = @"SELECT id
                        , bet_status
                        , bet_time
                        , update_time
                        , game_id
                        , tournament
                        , split_part(team_en_names,',', 1) as hometeam
                        , split_part(team_en_names,',', 2) as awayteam
                        , settle_time
                        , bet_amount
                        , odd_name
                        , odd
                        , win_amount
                        FROM public.t_pme_bet_record_v2 
                        WHERE partition_time BETWEEN @starttime AND @endtime
                        AND report_time = @reporttime
                        AND member_account=@club_id";
            var parameters = new DynamicParameters();
            parameters.Add("@starttime", createtime);
            parameters.Add("@endtime", createtime.AddDays(1).AddMilliseconds(-1));
            parameters.Add("@reporttime", report_time);
            parameters.Add("@club_id", Config.OneWalletAPI.Prefix_Key + club_id);

            await using var conn = new NpgsqlConnection(await PGRead);
            var result = await conn.QueryAsync<GetPMERecordsBySummaryResponse>(sql, parameters);
            return result.ToList();
        }
        catch (Exception ex)
        {
            throw;
        }
    }
    /// <summary>
    /// 依下注時間取得注單PK(id, bet_status, bet_time, update_time)
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <returns></returns>
    public async Task<List<GetPMERecordsPKByBetTimeResponse>> GetPMERecordsPKByBetTime(DateTime start, DateTime end)
    {
        var sql = @"
                    SELECT id, bet_status, bet_time, update_time
                    FROM t_pme_bet_record
                    WHERE bet_time >= @startTime 
                        AND bet_time <= @endTime";

        var par = new DynamicParameters();
        par.Add("@startTime", start);
        par.Add("@endTime", end);

        await using var conn = new NpgsqlConnection(await PGRead);
        var result = await conn.QueryAsync<GetPMERecordsPKByBetTimeResponse>(sql, par);
        return result.ToList();
    }

    public async Task<List<GetPMERecordsPreAmountByIdResponse>> GetPMERecordsPreAmountById(IDbTransaction tran, long id, DateTime bettime)
    {
        var sql = @"
                    SELECT id, bet_status, bet_time, update_time, pre_bet_amount, pre_win_amount 
                    FROM t_pme_bet_record
                    WHERE bet_time = @bettime
                        AND id = @id";

        var par = new DynamicParameters();
        par.Add("@id", id);
        par.Add("@bettime", bettime);

        var result = await tran.Connection.QueryAsync<GetPMERecordsPreAmountByIdResponse>(sql, par, tran);
        return result.ToList();
    }


    public async Task<List<GetPMERecordsPreAmountByIdResponse>> GetPMEV2RecordsPreAmountById(IDbTransaction tran, long id, DateTime bettime)
    {
        var sql = @"
                    SELECT id, bet_status, bet_time, update_time, pre_bet_amount, pre_win_amount 
                    FROM t_pme_bet_record_v2
                    WHERE partition_time = @partition_time
                        AND id = @id";

        var par = new DynamicParameters();
        par.Add("@id", id);
        par.Add("@partition_time", bettime);

        var result = await tran.Connection.QueryAsync<GetPMERecordsPreAmountByIdResponse>(sql, par, tran);
        return result.ToList();
    }

    public async Task<List<GetPMERunningRecordResponse>> GetPMERunningRecord(GetBetRecordUnsettleReq RecordReq)
    {
        var par = new DynamicParameters();
        var sql = @"SELECT id
                        , bet_status
                        , bet_time
                        , update_time
                        , game_id
                        , tournament
                        , split_part(team_en_names,',', 1) as hometeam
                        , split_part(team_en_names,',', 2) as awayteam
                        , settle_time
                        , bet_amount
                        , odd_name
                        , odd
                        , win_amount
                        , club_id
                        , franchiser_id
                    FROM public.t_pme_bet_record_running 
                    WHERE bet_time >= @start 
                        AND bet_time < @end";

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
        var result = await conn.QueryAsync<GetPMERunningRecordResponse>(sql, par);
        return result.ToList();
    }

    public async Task<(int totalCount, decimal totalBetValid, decimal totalNetWin)> SumPMEBetRecordByBetTime(DateTime start, DateTime end)
    {
        var sql = @"SELECT 
                    COUNT(Distinct id) AS totalCount
                    , CASE WHEN SUM(bet_amount) IS NULL THEN 0 ELSE SUM(bet_amount) END  AS totalBetValid
                    , CASE WHEN SUM(win_amount) IS NULL THEN 0 ELSE SUM(win_amount) END AS totalNetWin
                    FROM t_pme_bet_record
                    WHERE bet_time >= @startTime 
                        AND bet_time <= @endTime";

        var par = new DynamicParameters();
        par.Add("@startTime", start);
        par.Add("@endTime", end);

        await using var conn = new NpgsqlConnection(await PGRead);
        var result = (await conn.QueryAsync<dynamic>(sql, par)).FirstOrDefault();
        return ((int)result.totalcount, (decimal)result.totalbetvalid, (decimal)result.totalnetwin);
    }

    public async Task<IEnumerable<(int count, decimal win, decimal bet, decimal jackpot, string userid, decimal netwin, DateTime bettime)>> SummaryGameRecord(DateTime reportTime, DateTime startTime, DateTime endTime)
    {
        // TODO 後匯總PME更名
        var sql = @"SELECT CAST(count(1) AS INTEGER) AS count,
                               COALESCE (SUM(win_amount),0) AS win,
                               COALESCE (SUM(bet_amount),0) AS bet,
                               COALESCE (SUM(win_amount-bet_amount),0) AS netwin,
                               member_account AS userid,
                               DATE(partition_time) as bettime
                        FROM t_pme_bet_record_v2
                        WHERE partition_time BETWEEN @start_time AND @end_time
                        AND report_time = @report_time
                        GROUP BY Date(partition_time),member_account
                        ";

        var par = new DynamicParameters();
        par.Add("@start_time", startTime);
        par.Add("@end_time", endTime);
        par.Add("@report_time", reportTime);
        using (NpgsqlConnection conn = new NpgsqlConnection(await PGRead))
        {
            var result = await conn.QueryAsync(sql, par, commandTimeout: 270);

            return result.Select(x => ((int)x.count, (decimal)x.win, (decimal)x.bet, (decimal)0, (string)x.userid, (decimal)x.netwin, (DateTime)x.bettime)).ToList();
        }
    }
    #region V2

    public async Task<int> PostPMERecord_V2(NpgsqlConnection conn, IDbTransaction tran, IEnumerable<QueryScrollResponse.Bet> betLogs)
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
        var sql = $@"CREATE TEMPORARY TABLE temp_t_pme_bet_record_v2_{tableGuid:N} 
                            ( LIKE t_pme_bet_record_v2  INCLUDING ALL );";
        return tran.Connection.ExecuteAsync(sql, transaction: tran);
    }
    private async Task<ulong> BulkInsertToTempTable(IDbTransaction tran, Guid tableGuid, IEnumerable<QueryScrollResponse.Bet> records)
    {
        if (tran is not NpgsqlTransaction npTran)
            throw new ArgumentException("Must be NpgsqlTransaction", nameof(tran));

        if (npTran.Connection == null)
            throw new ArgumentNullException(nameof(tran.Connection));

        await using var writer =
            await npTran.Connection.BeginBinaryImportAsync(
                $"COPY temp_t_pme_bet_record_v2_{tableGuid:N} ( id, member_id, member_account, merchant_id, merchant_account, parent_merchant_id, parent_merchant_account," +
                     " tester, order_type, parley_type, game_id, tournament_id, match_id, match_type, market_id, market_cn_name, team_id, team_names, team_cn_names, team_en_names," +
                     " odd_id, odd_name, round, odd, bet_amount, win_amount, is_live, bet_status, confirm_type, bet_time, settle_time, match_start_time, update_time, settle_count," +
                     " device, bet_ip, score_benchmark, currency_code, exchange_rate, pre_bet_amount, pre_win_amount, tournament, partition_time, report_time"+
                     ") FROM STDIN (FORMAT BINARY)");

        foreach (var mapping in records)
        {
            await writer.StartRowAsync();
            await writer.WriteAsync(mapping.id, NpgsqlDbType.Bigint); // int8
            await writer.WriteAsync(mapping.member_id, NpgsqlDbType.Bigint); // int8
            await writer.WriteAsync(mapping.member_account, NpgsqlDbType.Varchar); // varchar
            await writer.WriteAsync(mapping.merchant_id, NpgsqlDbType.Bigint); // int8
            await writer.WriteAsync(mapping.merchant_account, NpgsqlDbType.Varchar); // varchar
            await writer.WriteAsync(mapping.parent_merchant_id, NpgsqlDbType.Bigint); // int8
            await writer.WriteAsync(mapping.parent_merchant_account, NpgsqlDbType.Varchar); // varchar
            await writer.WriteAsync(mapping.tester, NpgsqlDbType.Smallint); // int2
            await writer.WriteAsync(mapping.order_type, NpgsqlDbType.Smallint); // int2
            await writer.WriteAsync(mapping.parley_type, NpgsqlDbType.Smallint); // int2
            await writer.WriteAsync(mapping.game_id, NpgsqlDbType.Bigint); // int8
            await writer.WriteAsync(mapping.tournament_id, NpgsqlDbType.Bigint); // int8
            await writer.WriteAsync(mapping.match_id, NpgsqlDbType.Bigint); // int8
            await writer.WriteAsync(mapping.match_type, NpgsqlDbType.Smallint); // int2
            await writer.WriteAsync(mapping.market_id, NpgsqlDbType.Bigint); // int8
            await writer.WriteAsync(mapping.market_cn_name, NpgsqlDbType.Varchar); // varchar
            await writer.WriteAsync(mapping.team_id, NpgsqlDbType.Varchar); // varchar
            await writer.WriteAsync(mapping.team_names, NpgsqlDbType.Varchar); // varchar
            await writer.WriteAsync(mapping.team_cn_names, NpgsqlDbType.Varchar); // varchar
            await writer.WriteAsync(mapping.team_en_names, NpgsqlDbType.Varchar); // varchar
            await writer.WriteAsync(mapping.odd_id, NpgsqlDbType.Bigint); // int8
            await writer.WriteAsync(mapping.odd_name, NpgsqlDbType.Varchar); // varchar
            await writer.WriteAsync(mapping.round, NpgsqlDbType.Integer); // int4
            await writer.WriteAsync(mapping.odd, NpgsqlDbType.Varchar); // varchar
            await writer.WriteAsync(mapping.bet_amount, NpgsqlDbType.Numeric); // numeric(19, 4)
            await writer.WriteAsync(mapping.win_amount, NpgsqlDbType.Numeric); // numeric(19, 4)
            await writer.WriteAsync(mapping.is_live, NpgsqlDbType.Smallint); // int2
            await writer.WriteAsync(mapping.bet_status, NpgsqlDbType.Smallint); // int2
            await writer.WriteAsync(mapping.confirm_type, NpgsqlDbType.Smallint); // int2
            await writer.WriteAsync(mapping.BetTimeFormatted, NpgsqlDbType.Timestamp); // timestamp
            await writer.WriteAsync(mapping.SettleTimeFormatted, NpgsqlDbType.Timestamp); // timestamp
            await writer.WriteAsync(mapping.MatchStartTimeFormatted, NpgsqlDbType.Timestamp); // timestamp
            await writer.WriteAsync(mapping.UpdateTimeFormatted, NpgsqlDbType.Timestamp); // timestamp
            await writer.WriteAsync(mapping.settle_count, NpgsqlDbType.Integer); // int4
            await writer.WriteAsync(mapping.device, NpgsqlDbType.Smallint); // int2
            await writer.WriteAsync(mapping.bet_ip, NpgsqlDbType.Bigint); // int8
            await writer.WriteAsync(mapping.score_benchmark, NpgsqlDbType.Varchar); // varchar
            await writer.WriteAsync(mapping.currency_code, NpgsqlDbType.Integer); // int4
            await writer.WriteAsync(mapping.exchange_rate, NpgsqlDbType.Numeric); // numeric(19, 4)
            await writer.WriteAsync(mapping.pre_bet_amount, NpgsqlDbType.Numeric); // numeric(19, 4)
            await writer.WriteAsync(mapping.pre_win_amount, NpgsqlDbType.Numeric); // numeric(19, 4)
            await writer.WriteAsync(mapping.tournament, NpgsqlDbType.Varchar); // varchar
            await writer.WriteAsync(mapping.partition_time, NpgsqlDbType.Timestamp); // timestamp
            await writer.WriteAsync(mapping.report_time, NpgsqlDbType.Timestamp); // timestamp
        }

        return await writer.CompleteAsync();
    }
    private async Task<int> MergeFromTempTable(IDbTransaction tran, Guid tableGuid)
    {
        var sql = @$"INSERT INTO t_pme_bet_record_v2
                        SELECT id, member_id, member_account, merchant_id, merchant_account, parent_merchant_id, parent_merchant_account,
                                  tester, order_type, parley_type, game_id, tournament_id, match_id, match_type, market_id, market_cn_name,
                                  team_id, team_names, team_cn_names, team_en_names, odd_id, odd_name, round, odd, bet_amount, win_amount,
                                  is_live, bet_status, confirm_type, bet_time, settle_time, match_start_time, update_time, settle_count, device,
                                  bet_ip, score_benchmark, currency_code, exchange_rate, pre_bet_amount, pre_win_amount, tournament, partition_time, create_time, report_time  
                        FROM temp_t_pme_bet_record_v2_{tableGuid:N} AS  temp
                        WHERE NOT EXISTS 
                        (
                            SELECT NULL
                            FROM public.t_pme_bet_record_v2
                            WHERE id = temp.id 
                                AND bet_status = temp.bet_status
                                AND partition_time = temp.partition_time
                                AND update_time = temp.update_time
                        )";
        return await tran.Connection.ExecuteAsync(sql, tran);
    }
    public async Task<int> PostPMERecordRunning(NpgsqlConnection conn, IDbTransaction tran, IEnumerable<QueryScrollResponse.Bet> betLogs)
    {
        if (tran == null) throw new ArgumentNullException(nameof(tran));
        if (betLogs == null) throw new ArgumentNullException(nameof(betLogs));
        if (!betLogs.Any()) return 0;

        var tableGuid = Guid.NewGuid();
        //建立暫存表
        await CreateBetRecordrunningTempTable(tran, tableGuid);
        //將資料倒進暫存表
        await BulkInsertTorunningTempTable(tran, tableGuid, betLogs);
        //將資料由暫存表倒回主表(過濾重複)
        return await MergeFromrunningTempTable(tran, tableGuid);
    }
    private Task<int> CreateBetRecordrunningTempTable(IDbTransaction tran, Guid tableGuid)
    {
        var sql = $@"CREATE TEMPORARY TABLE temp_t_pme_bet_record_running_{tableGuid:N} 
                            ( LIKE t_pme_bet_record_running  INCLUDING ALL );";
        return tran.Connection.ExecuteAsync(sql, transaction: tran);
    }
    private async Task<ulong> BulkInsertTorunningTempTable(IDbTransaction tran, Guid tableGuid, IEnumerable<QueryScrollResponse.Bet> records)
    {
        if (tran is not NpgsqlTransaction npTran)
            throw new ArgumentException("Must be NpgsqlTransaction", nameof(tran));

        if (npTran.Connection == null)
            throw new ArgumentNullException(nameof(tran.Connection));

        await using var writer =
            await npTran.Connection.BeginBinaryImportAsync(
                @$"COPY temp_t_pme_bet_record_running_{tableGuid:N} ( id, member_id, member_account, merchant_id, merchant_account, parent_merchant_id, parent_merchant_account,
                               tester, order_type, parley_type, game_id, tournament_id, match_id, match_type, market_id, market_cn_name, team_id,
                               team_names, team_cn_names, team_en_names, odd_id, odd_name, round, odd, bet_amount, win_amount, is_live, bet_status, confirm_type,
                               bet_time, settle_time, match_start_time, update_time, settle_count, device, bet_ip, score_benchmark, currency_code, exchange_rate,
                               summary_id, club_id, franchiser_id, tournament   ) FROM STDIN (FORMAT BINARY)");

        foreach (var mapping in records)
        {
            await writer.StartRowAsync();
            await writer.WriteAsync(mapping.id, NpgsqlDbType.Bigint); // int8
            await writer.WriteAsync(mapping.member_id, NpgsqlDbType.Bigint); // int8
            await writer.WriteAsync(mapping.member_account, NpgsqlDbType.Varchar); // varchar
            await writer.WriteAsync(mapping.merchant_id, NpgsqlDbType.Bigint); // int8
            await writer.WriteAsync(mapping.merchant_account, NpgsqlDbType.Varchar); // varchar
            await writer.WriteAsync(mapping.parent_merchant_id, NpgsqlDbType.Bigint); // int8
            await writer.WriteAsync(mapping.parent_merchant_account, NpgsqlDbType.Varchar); // varchar
            await writer.WriteAsync(mapping.tester, NpgsqlDbType.Smallint); // int2
            await writer.WriteAsync(mapping.order_type, NpgsqlDbType.Smallint); // int2
            await writer.WriteAsync(mapping.parley_type, NpgsqlDbType.Smallint); // int2
            await writer.WriteAsync(mapping.game_id, NpgsqlDbType.Bigint); // int8
            await writer.WriteAsync(mapping.tournament_id, NpgsqlDbType.Bigint); // int8
            await writer.WriteAsync(mapping.match_id, NpgsqlDbType.Bigint); // int8
            await writer.WriteAsync(mapping.match_type, NpgsqlDbType.Smallint); // int2
            await writer.WriteAsync(mapping.market_id, NpgsqlDbType.Bigint); // int8
            await writer.WriteAsync(mapping.market_cn_name, NpgsqlDbType.Varchar); // varchar
            await writer.WriteAsync(mapping.team_id, NpgsqlDbType.Varchar); // varchar
            await writer.WriteAsync(mapping.team_names, NpgsqlDbType.Varchar); // varchar
            await writer.WriteAsync(mapping.team_cn_names, NpgsqlDbType.Varchar); // varchar
            await writer.WriteAsync(mapping.team_en_names, NpgsqlDbType.Varchar); // varchar
            await writer.WriteAsync(mapping.odd_id, NpgsqlDbType.Bigint); // int8
            await writer.WriteAsync(mapping.odd_name, NpgsqlDbType.Varchar); // varchar
            await writer.WriteAsync(mapping.round, NpgsqlDbType.Integer); // int4
            await writer.WriteAsync(mapping.odd, NpgsqlDbType.Varchar); // varchar
            await writer.WriteAsync(mapping.bet_amount, NpgsqlDbType.Numeric); // numeric(19, 4)
            await writer.WriteAsync(mapping.win_amount, NpgsqlDbType.Numeric); // numeric(19, 4)
            await writer.WriteAsync(mapping.is_live, NpgsqlDbType.Smallint); // int2
            await writer.WriteAsync(mapping.bet_status, NpgsqlDbType.Smallint); // int2
            await writer.WriteAsync(mapping.confirm_type, NpgsqlDbType.Smallint); // int2
            await writer.WriteAsync(mapping.BetTimeFormatted, NpgsqlDbType.Timestamp); // timestamp
            await writer.WriteAsync(mapping.SettleTimeFormatted, NpgsqlDbType.Timestamp); // timestamp
            await writer.WriteAsync(mapping.MatchStartTimeFormatted, NpgsqlDbType.Timestamp); // timestamp
            await writer.WriteAsync(mapping.UpdateTimeFormatted, NpgsqlDbType.Timestamp); // timestamp
            await writer.WriteAsync(mapping.settle_count, NpgsqlDbType.Integer); // int4
            await writer.WriteAsync(mapping.device, NpgsqlDbType.Smallint); // int2
            await writer.WriteAsync(mapping.bet_ip, NpgsqlDbType.Bigint); // int8
            await writer.WriteAsync(mapping.score_benchmark, NpgsqlDbType.Varchar); // varchar
            await writer.WriteAsync(mapping.currency_code, NpgsqlDbType.Integer); // int4
            await writer.WriteAsync(mapping.exchange_rate, NpgsqlDbType.Numeric); // numeric(19, 4)
            await writer.WriteAsync(mapping.summary_id, NpgsqlDbType.Uuid); // uuid
            await writer.WriteAsync(mapping.club_id, NpgsqlDbType.Varchar); // varchar
            await writer.WriteAsync(mapping.franchiser_id, NpgsqlDbType.Varchar); // varchar
            await writer.WriteAsync(mapping.tournament, NpgsqlDbType.Varchar); // varchar
        }

        return await writer.CompleteAsync();
    }
    private async Task<int> MergeFromrunningTempTable(IDbTransaction tran, Guid tableGuid)
    {
        var sql = @$"INSERT INTO t_pme_bet_record_running
                        SELECT id, member_id, member_account, merchant_id, merchant_account, parent_merchant_id, parent_merchant_account,
                               tester, order_type, parley_type, game_id, tournament_id, match_id, match_type, market_id, market_cn_name, team_id,
                               team_names, team_cn_names, team_en_names, odd_id, odd_name, round, odd, bet_amount, win_amount, is_live, bet_status, confirm_type,
                               bet_time, settle_time, match_start_time, update_time, settle_count, device, bet_ip, score_benchmark, currency_code, exchange_rate,
                               summary_id, club_id, franchiser_id, tournament
                        FROM temp_t_pme_bet_record_running_{tableGuid:N} AS  temp
                        WHERE NOT EXISTS 
                        (
                            SELECT NULL
                            FROM public.t_pme_bet_record_running
                            WHERE id = temp.id 
                                AND bet_status = temp.bet_status
                                AND bet_time = temp.bet_time
                                AND update_time = temp.update_time
                        )";
        return await tran.Connection.ExecuteAsync(sql, tran);
    }

    #endregion V2
}