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
using H1_ThirdPartyWalletAPI.Model.Game.TP.Response;
using static H1_ThirdPartyWalletAPI.Model.Game.TP.Response.BetLogResponse;
using H1_ThirdPartyWalletAPI.Utility;
using H1_ThirdPartyWalletAPI.Model.DB.JDB.Response;
using H1_ThirdPartyWalletAPI.Model.DB.PP.Response;
using NpgsqlTypes;

namespace H1_ThirdPartyWalletAPI.Service.Common.DB.Betlogs;

public interface ITpDBService
{
    Task<int> PostTpRecord(NpgsqlConnection conn, IDbTransaction tran, IEnumerable<BetLog> betLogs);
    Task<List<t_tp_bet_record>> GetTpRecordsBySummary(GetBetRecordReq RecordReq);
    Task<List<t_tp_bet_record>> GetTpRecordsByBetTime(DateTime start, DateTime end);
    
    Task<(int totalCount, decimal totalBetValid, decimal totalNetWin)> SumTpBetRecordByBetTime(DateTime start, DateTime end);
    Task<int> PostTpReport(IEnumerable<StatisticsByGameResponse.Statistics> statistics);
    Task<int> DeleteTpReport(IEnumerable<StatisticsByGameResponse.Statistics> statistics);
    Task<List<t_tp_bet_record>> GetTpRecordByReportTime(BetRecordSummary RecordReq, DateTime start, DateTime end);
    Task<int> BulkInsertSummaryBetRecordMapping(IDbTransaction tran, IEnumerable<t_summary_bet_record_mapping> mappings);
    Task<ulong> BatchInsertRecordSummaryAsync(NpgsqlConnection conn, List<BetRecordSummary> summaryData);

    public Task<IEnumerable<(int RecordCount, decimal turnover, decimal netwin, decimal bet, string userid,DateTime betTime)>> SummaryGameRecord(DateTime reportTime, DateTime startTime, DateTime endTime);

     Task<List<t_tp_bet_record>> GetTpRecordV2ByBetTime(DateTime startTime, DateTime endTime);
    //真人未串接
    //Task<int> PostTpLiveRecord(NpgsqlConnection conn, IDbTransaction tran, IEnumerable<BetLog> betLogs);
    //Task<List<t_tp_live_bet_record>> GetTpLiveRecordsBySummary(GetBetRecordReq RecordReq);
    //Task<List<t_tp_live_bet_record>> GetTpLiveRecordsByBetTime(DateTime start, DateTime end);
    //Task<List<t_tp_live_bet_record>> GetTpLiveRecordsV2ByBetTime(DateTime start, DateTime end);
    //Task<List<t_tp_live_bet_record>> GetTpLiveRecordsByRowId(IDbTransaction tran, string rowId, DateTime bettime);
    //Task<(int totalCount, decimal totalBetValid, decimal totalNetWin)> SumTpLiveBetRecordByBetTime(DateTime start, DateTime end);
}

public class TpDBService : BetlogsDBServiceBase, ITpDBService
{

    private readonly string _prefixKey;
    public TpDBService(ILogger<TpDBService> logger, IBetLogsDbConnectionStringManager connectionStringManager) : base(logger, connectionStringManager)
    {
        _prefixKey = Config.OneWalletAPI.Prefix_Key;
    }
    #region t_tp_bet_record
    /// <summary>
    /// 補單查詢遊戲單號(新表)
    /// </summary>
    /// <param name="startTime"></param>
    /// <param name="endTime"></param>
    /// <returns></returns>
    public async Task<List<t_tp_bet_record>> GetTpRecordV2ByBetTime(DateTime startTime, DateTime endTime)
    {
        var par = new DynamicParameters();
        string strSql = @"SELECT 
                                rowid
                                , bettime
                                , status
                FROM t_tp_bet_record_v2
                WHERE partition_time BETWEEN @startTime and @endTime
                ";
        par.Add("@startTime", startTime);
        par.Add("@endTime", endTime);
        {
            await using var conn = new NpgsqlConnection(await PGRead);
            var result = await conn.QueryAsync<t_tp_bet_record>(strSql, par);
            return result.ToList();
        }
    }
    /// <summary>
    /// 取得第二層明細
    /// </summary>
    /// <param name="RecordReq"></param>
    /// <returns></returns>
    public async Task<List<t_tp_bet_record>> GetTpRecordByReportTime(BetRecordSummary RecordReq, DateTime start, DateTime end)
    {
        var sql = @"SELECT
                    rowid, hall, round, category, gameid, game_name,
                    casino_account, betvalid, betamount, betresult, pca_contribute, pca_win, revenue, bettime, payout_time,
                    reporttime, trace, freegame, bettype, gameresult, status, db_report_time, partition_time
                FROM t_tp_bet_record_v2
                WHERE partition_time BETWEEN @starttime AND @endtime
                AND db_report_time = @db_report_time
                AND casino_account = @casino_account";

        var parameters = new DynamicParameters();
        parameters.Add("@starttime", start);
        parameters.Add("@endtime", end);
        parameters.Add("@db_report_time", RecordReq.ReportDatetime);
        parameters.Add("@casino_account", RecordReq.Club_id);

        {
            await using var conn = new NpgsqlConnection(await PGRead);
            var result = await conn.QueryAsync<t_tp_bet_record>(sql, parameters);
            return result.ToList();
        }
    }
    /// <summary>
    /// 後匯總更名
    /// </summary>
    /// <param name="reportTime"></param>
    /// <param name="startTime"></param>
    /// <param name="endTime"></param>
    /// <returns></returns>
    public async Task<IEnumerable<(int RecordCount, decimal turnover, decimal netwin, decimal bet, string userid, DateTime betTime)>> SummaryGameRecord(DateTime reportTime, DateTime startTime, DateTime endTime)
    {
        var sql = @"SELECT CAST(count(1) AS INTEGER) AS count,
                        COALESCE (SUM(betvalid),0) AS turnover,
                        COALESCE (SUM(betresult),0) AS netwin,
                        COALESCE (SUM(betamount),0) AS bet,
                        casino_account AS userid,
                        date(partition_time) AS partition_time
                        FROM t_tp_bet_record_v2
                        WHERE partition_time BETWEEN @start_time AND @end_time
                        AND db_report_time = @db_report_time
                        GROUP BY casino_account,partition_time
                        ";

        var par = new DynamicParameters();
        par.Add("@start_time", startTime);
        par.Add("@end_time", endTime);
        par.Add("@db_report_time", reportTime);
        using (NpgsqlConnection conn = new NpgsqlConnection(await PGRead))
        {
            var result = await conn.QueryAsync(sql, par, commandTimeout: 270);
            return result.Select(x => ((int)x.count, (decimal)x.turnover, (decimal)x.netwin, (decimal)x.bet, (string)x.userid, (DateTime) x.partition_time)).ToList();
        }
    }
    public async Task<int> PostTpRecord(NpgsqlConnection conn, IDbTransaction tran, IEnumerable<BetLog> betLogs)
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
        var sql = $@"CREATE TEMPORARY TABLE temp_t_tp_bet_record_v2_{tableGuid:N} 
                            ( LIKE t_tp_bet_record_v2  INCLUDING ALL );";

        return tran.Connection.ExecuteAsync(sql, transaction: tran);
    }

    private async Task<ulong> BulkInsertToTempTable(IDbTransaction tran, Guid tableGuid,IEnumerable<BetLog> records)
    {
        if (tran is not NpgsqlTransaction npTran)
            throw new ArgumentException("Must be NpgsqlTransaction", nameof(tran));

        if (npTran.Connection == null)
            throw new ArgumentNullException(nameof(tran.Connection));

        await using var writer =
            await npTran.Connection.BeginBinaryImportAsync(
                $"COPY temp_t_tp_bet_record_v2_{tableGuid:N} (rowid, hall, round, category, gameid, game_name," +
                $" casino_account, betvalid, betamount, betresult, pca_contribute, pca_win, revenue, bettime, payout_time," +
                $" reporttime, trace, freegame, bettype, gameresult, status, db_report_time, partition_time) FROM STDIN (FORMAT BINARY)");

        foreach (var mapping in records)
        {
            await writer.StartRowAsync();
            await writer.WriteAsync(mapping.rowid.ToString(), NpgsqlDbType.Varchar);
            await writer.WriteAsync(mapping.hall.ToString(), NpgsqlDbType.Varchar);
            await writer.WriteAsync(mapping.round.ToString(), NpgsqlDbType.Varchar);
            await writer.WriteAsync(mapping.category.ToString(), NpgsqlDbType.Varchar);
            await writer.WriteAsync(mapping.gameid.ToString(), NpgsqlDbType.Varchar);
            await writer.WriteAsync(mapping.game_name.ToString(), NpgsqlDbType.Varchar);
            await writer.WriteAsync(mapping.casino_account.ToString(), NpgsqlDbType.Varchar);
            // 数值字段需要转换
            await writer.WriteAsync(decimal.Parse(mapping.betvalid), NpgsqlDbType.Numeric);
            await writer.WriteAsync(decimal.Parse(mapping.betamount), NpgsqlDbType.Numeric);
            await writer.WriteAsync(decimal.Parse(mapping.betresult), NpgsqlDbType.Numeric);
            await writer.WriteAsync(decimal.Parse(mapping.pca_contribute), NpgsqlDbType.Numeric);
            await writer.WriteAsync(decimal.Parse(mapping.pca_win), NpgsqlDbType.Numeric);
            await writer.WriteAsync(decimal.Parse(mapping.revenue), NpgsqlDbType.Numeric);
            await writer.WriteAsync(mapping.bettime, NpgsqlDbType.Timestamp);
            await writer.WriteAsync(mapping.payout_time, NpgsqlDbType.Timestamp);
            await writer.WriteAsync(mapping.reporttime, NpgsqlDbType.Timestamp);
            await writer.WriteAsync(mapping.trace, NpgsqlDbType.Boolean);
            await writer.WriteAsync(mapping.freegame ?? (object)DBNull.Value, NpgsqlDbType.Smallint);
            await writer.WriteAsync(mapping.bettype ?? (object)DBNull.Value, NpgsqlDbType.Varchar);
            await writer.WriteAsync(mapping.gameresult ?? (object)DBNull.Value, NpgsqlDbType.Varchar);
            await writer.WriteAsync(mapping.status.ToString(), NpgsqlDbType.Varchar);
            await writer.WriteAsync(mapping.db_report_time, NpgsqlDbType.Timestamp);
            await writer.WriteAsync(mapping.partition_time, NpgsqlDbType.Timestamp);
        }

        return await writer.CompleteAsync();
    }
    private async Task<int> MergeFromTempTable(IDbTransaction tran, Guid tableGuid)
    {
        var sql = @$"INSERT INTO t_tp_bet_record_v2
                        SELECT rowid, hall, round, category, gameid, game_name, casino_account,
                               betvalid, betamount, betresult, pca_contribute, pca_win, revenue, bettime, payout_time, reporttime,
                               trace, freegame, bettype, gameresult, status, db_report_time, create_time, partition_time
                        FROM temp_t_tp_bet_record_v2_{tableGuid:N} AS  temp
                        WHERE NOT EXISTS 
                        (
                            SELECT NULL
                            FROM public.t_tp_bet_record_v2
                            WHERE rowid = temp.rowid 
                                AND status = temp.status
                                AND partition_time = temp.partition_time
                        )";
        return  await tran.Connection.ExecuteAsync(sql, tran);
    }

    /// <summary>
    /// 依下注時間取得注單
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <returns></returns>
    public async Task<List<t_tp_bet_record>> GetTpRecordsByBetTime(DateTime start, DateTime end)
    {
        var sql = @"
                    SELECT * FROM t_tp_bet_record
                    WHERE bettime >= @startTime 
                        AND bettime <= @endTime";

        var par = new DynamicParameters();
        par.Add("@startTime", start);
        par.Add("@endTime", end);

        await using var conn = new NpgsqlConnection(await PGRead);
        var result = await conn.QueryAsync<t_tp_bet_record>(sql, par);
        return result.ToList();
    }


    public async Task<List<t_tp_bet_record>> GetTpRecordsBySummary(GetBetRecordReq RecordReq)
    {
        var sql = @"SELECT 
                    rowid, hall, round, category, gameid, game_name,
                    casino_account, betvalid, betamount, betresult, pca_contribute, pca_win, revenue, bettime, payout_time,
                    reporttime, trace, freegame, bettype, gameresult, status,summary_id
                    FROM public.t_tp_bet_record 
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
        var result = await conn.QueryAsync<t_tp_bet_record>(sql, param);
        return result.ToList();
    }


    public async Task<(int totalCount, decimal totalBetValid, decimal totalNetWin)> SumTpBetRecordByBetTime(DateTime start, DateTime end)
    {
        var sql = @"SELECT 
                    COUNT(rowid) AS totalCount
                    , coalesce(SUM(betvalid),0) AS totalBetValid
                    , coalesce(SUM(betresult),0) AS totalNetWin
                    FROM t_tp_bet_record_v2
                    WHERE partition_time >= @startTime 
                        AND partition_time <= @endTime ";

        var par = new DynamicParameters();
        par.Add("@startTime", start);
        par.Add("@endTime", end);

        await using var conn = new NpgsqlConnection(await PGRead);
        var result = (await conn.QueryAsync<dynamic>(sql, par)).FirstOrDefault();
        return ((int)result.totalcount, (decimal)result.totalbetvalid, (decimal)result.totalnetwin);
    }

    #endregion
    #region t_tp_live_bet_record
    /// <summary>
    /// 存真人注單
    /// </summary>
    /// <param name="conn"></param>
    /// <param name="tran"></param>
    /// <param name="betLogs"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    //public async Task<int> PostTpLiveRecord(NpgsqlConnection conn, IDbTransaction tran, IEnumerable<BetLog> betLogs)
    //{
    //    if (tran == null) throw new ArgumentNullException(nameof(tran));
    //    if (betLogs == null) throw new ArgumentNullException(nameof(betLogs));
    //    if (!betLogs.Any()) return 0;

    //    var tableGuid = Guid.NewGuid();
    //    //建立暫存表
    //    await CreateLiveBetRecordTempTable(tran, tableGuid);
    //    //將資料倒進暫存表
    //    await BulkInsertToLiveTempTable(tran, tableGuid, betLogs);
    //    //將資料由暫存表倒回主表(過濾重複)
    //    return await MergeFromLiveTempTable(tran, tableGuid);
    //}
    //private Task<int> CreateLiveBetRecordTempTable(IDbTransaction tran, Guid tableGuid)
    //{
    //    var sql = $@"CREATE TEMPORARY TABLE temp_t_tp_live_bet_record_v2_{tableGuid:N} 
    //                        ( LIKE t_tp_live_bet_record_v2  INCLUDING ALL );";

    //    return tran.Connection.ExecuteAsync(sql, transaction: tran);
    //}

    //private async Task<ulong> BulkInsertToLiveTempTable(IDbTransaction tran, Guid tableGuid,IEnumerable<BetLog> records)
    //{
    //    if (tran is not NpgsqlTransaction npTran)
    //        throw new ArgumentException("Must be NpgsqlTransaction", nameof(tran));

    //    if (npTran.Connection == null)
    //        throw new ArgumentNullException(nameof(tran.Connection));

    //    await using var writer =
    //         await npTran.Connection.BeginBinaryImportAsync(
    //             $"COPY temp_t_tp_live_bet_record_v2_{tableGuid:N} (rowid, hall, round, category, gameid, game_name, casino_account, betvalid," +
    //             $" betamount, betresult, pca_contribute, pca_win, revenue, bettime, payout_time, reporttime, trace, freegame, bettype, gameresult," +
    //             $" status, pre_betvalid, pre_betresult, pre_betamount, partition_time ) FROM STDIN (FORMAT BINARY)");

    //    foreach (var mapping in records)
    //    {
    //        await writer.StartRowAsync();
    //        await writer.WriteAsync(mapping.rowid.ToString(), NpgsqlDbType.Varchar);
    //        await writer.WriteAsync(mapping.hall.ToString(), NpgsqlDbType.Varchar);
    //        await writer.WriteAsync(mapping.round.ToString(), NpgsqlDbType.Varchar);
    //        await writer.WriteAsync(mapping.category.ToString(), NpgsqlDbType.Varchar);
    //        await writer.WriteAsync(mapping.gameid.ToString(), NpgsqlDbType.Varchar);
    //        await writer.WriteAsync(mapping.game_name.ToString(), NpgsqlDbType.Varchar);
    //        await writer.WriteAsync(mapping.casino_account.ToString(), NpgsqlDbType.Varchar);
    //        // 数值字段需要转换
    //        await writer.WriteAsync(decimal.Parse(mapping.betvalid), NpgsqlDbType.Numeric);
    //        await writer.WriteAsync(decimal.Parse(mapping.betamount), NpgsqlDbType.Numeric);
    //        await writer.WriteAsync(decimal.Parse(mapping.betresult), NpgsqlDbType.Numeric);
    //        await writer.WriteAsync(decimal.Parse(mapping.pca_contribute), NpgsqlDbType.Numeric);
    //        await writer.WriteAsync(decimal.Parse(mapping.pca_win), NpgsqlDbType.Numeric);
    //        await writer.WriteAsync(decimal.Parse(mapping.revenue), NpgsqlDbType.Numeric);
    //        await writer.WriteAsync(decimal.Parse(mapping.pre_betvalid), NpgsqlDbType.Numeric);
    //        await writer.WriteAsync(decimal.Parse(mapping.pre_betresult), NpgsqlDbType.Numeric);
    //        await writer.WriteAsync(decimal.Parse(mapping.pre_betamount), NpgsqlDbType.Numeric);
    //        await writer.WriteAsync(mapping.bettime, NpgsqlDbType.Timestamp);
    //        await writer.WriteAsync(mapping.payout_time, NpgsqlDbType.Timestamp);
    //        await writer.WriteAsync(mapping.reporttime, NpgsqlDbType.Timestamp);
    //        await writer.WriteAsync(mapping.trace, NpgsqlDbType.Boolean);
    //        await writer.WriteAsync(mapping.freegame ?? (object)DBNull.Value, NpgsqlDbType.Smallint);
    //        await writer.WriteAsync(mapping.bettype ?? (object)DBNull.Value, NpgsqlDbType.Varchar);
    //        await writer.WriteAsync(mapping.gameresult ?? (object)DBNull.Value, NpgsqlDbType.Varchar);
    //        await writer.WriteAsync(mapping.status.ToString(), NpgsqlDbType.Varchar);
    //        await writer.WriteAsync(mapping.partition_time, NpgsqlDbType.Timestamp);
    //    }

    //    return await writer.CompleteAsync();
    //}
    //private Task<int> MergeFromLiveTempTable(IDbTransaction tran, Guid tableGuid)
    //{
    //    var sql = @$"INSERT INTO t_tp_live_bet_record_v2
    //                    SELECT rowid, hall, round, category, gameid, game_name, casino_account, betvalid,
    //                            betamount, betresult, pca_contribute, pca_win, revenue, bettime, payout_time, reporttime, trace, freegame, bettype, gameresult,
    //                            status, pre_betvalid, pre_betresult, pre_betamount, partition_time
    //                    FROM temp_t_tp_live_bet_record_v2_{tableGuid:N} temp
    //                    WHERE NOT EXISTS 
    //                    (
    //                        SELECT NULL
    //                        FROM public.t_tp_live_bet_record_v2
    //                        WHERE rowid = temp.rowid 
    //                            AND reporttime = temp.reporttime
    //                            AND bettime = temp.bettime
    //                    )
    //                    ";
    //    return tran.Connection.ExecuteAsync(sql, tran);
    //}
    
    /// <summary>
    /// 依下注時間取得真人注單 舊表
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <returns></returns>
    //public async Task<List<t_tp_live_bet_record>> GetTpLiveRecordsByBetTime(DateTime start, DateTime end)
    //{
    //    var sql = @"
    //                SELECT 
    //                    rowid, hall, round, category, gameid, game_name, casino_account, betvalid, betamount, betresult, pca_contribute, pca_win, revenue,
    //                    bettime, payout_time, reporttime, trace, freegame, bettype, gameresult, status 
    //                FROM t_tp_live_bet_record
    //                WHERE bettime >= @startTime 
    //                    AND bettime <= @endTime";

    //    var par = new DynamicParameters();
    //    par.Add("@startTime", start);
    //    par.Add("@endTime", end);

    //    await using var conn = new NpgsqlConnection(await PGRead);
    //    var result = await conn.QueryAsync<t_tp_live_bet_record>(sql, par);
    //    return result.ToList();
    //}
    /// <summary>
    /// 依下注時間取得真人注單 新表
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <returns></returns>
    //public async Task<List<t_tp_live_bet_record>> GetTpLiveRecordsV2ByBetTime(DateTime start, DateTime end)
    //{
    //    var sql = @"SELECT rowid
    //                     , bettime
    //                     , status 
    //                FROM t_tp_live_bet_record_v2
    //                WHERE partition_time >= @startTime 
    //                    AND partition_time <= @endTime";
    //    var par = new DynamicParameters();
    //    par.Add("@startTime", start);
    //    par.Add("@endTime", end);
    //    await using var conn = new NpgsqlConnection(await PGRead);
    //    var result = await conn.QueryAsync<t_tp_live_bet_record>(sql, par);
    //    return result.ToList();
    //}
    //public async Task<List<t_tp_live_bet_record>> GetTpLiveRecordsByRowId(IDbTransaction tran, string rowId, DateTime bettime)
    //{
    //    var sql = @"SELECT * FROM t_tp_live_bet_record
    //                WHERE bettime >= @startTime 
    //                    AND bettime <= @endTime
    //                    and rowid = @rowId";
    //    var par = new DynamicParameters();
    //    par.Add("@startTime", bettime.AddMinutes(-1));
    //    par.Add("@endTime", bettime.AddMinutes(1));
    //    par.Add("@rowId", rowId);

    //    var result = await tran.Connection.QueryAsync<t_tp_live_bet_record>(sql, par, tran);
    //    return result.ToList();
    //}
    //public async Task<List<t_tp_live_bet_record>> GetTpLiveRecordsBySummary(GetBetRecordReq RecordReq)
    //{
    //    var sql = @"SELECT * 
    //                FROM public.t_tp_live_bet_record 
    //                WHERE bettime >= @start 
    //                    AND bettime <= @end
    //                    AND summary_id = @summaryId::uuid";
    //    var param = new
    //    {
    //        summaryId = RecordReq.summary_id,
    //        start = RecordReq.ReportTime.AddDays(-3),
    //        end = RecordReq.ReportTime.AddDays(1),
    //    };

    //    await using var conn = new NpgsqlConnection(await PGRead);
    //    var result = await conn.QueryAsync<t_tp_live_bet_record>(sql, param);
    //    return result.ToList();
    //}
    //public async Task<(int totalCount, decimal totalBetValid, decimal totalNetWin)> SumTpLiveBetRecordByBetTime(DateTime start, DateTime end)
    //{
    //    var sql = @"SELECT 
    //                COUNT(distinct rowid) AS totalCount
    //                , CASE WHEN SUM(betvalid) IS NULL THEN 0 ELSE SUM(betvalid) END  AS totalBetValid
    //                , CASE WHEN SUM(betresult) IS NULL THEN 0 ELSE SUM(betresult) END AS totalNetWin
    //                FROM t_tp_live_bet_record_v2
    //                WHERE bettime >= @startTime 
    //                    AND bettime <= @endTime ";

    //    var par = new DynamicParameters();
    //    par.Add("@startTime", start);
    //    par.Add("@endTime", end);

    //    await using var conn = new NpgsqlConnection(await PGRead);
    //    var result = (await conn.QueryAsync<dynamic>(sql, par)).FirstOrDefault();
    //    return ((int)result.totalcount, (decimal)result.totalbetvalid, (decimal)result.totalnetwin);
    //}
    #endregion

    #region t_tp_game_report
    public async Task<int> PostTpReport(IEnumerable<StatisticsByGameResponse.Statistics> statistics)
    {
        var sql = @"INSERT INTO public.t_tp_game_report
                    (report_time
                    , currency
                    , bet_amount
                    , bet_value
                    , bet_count
                    , bet_result
                    , profit_result_percent
                    , gamehall
                    , fullname
                    , gamecode
                    , ""name""
                    , name_cn
                    , category
                    , win_count
                    , win_point
                    , rtp)
                    VALUES
                    (@report_time
                    , @currency
                    , CAST(@bet_amount AS numeric(19,4))
                    , CAST(@bet_value AS numeric(19,4))
                    , CAST(@bet_count AS int8)
                    , CAST(@bet_result AS numeric(19,4))
                    , CAST(@profit_result_percent AS numeric(19,4))
                    , @gamehall
                    , @fullname
                    , @gamecode
                    , @gameName
                    , @name_cn
                    , @category
                    , CAST(@win_count AS int8)
                    , CAST(@win_point AS numeric(19,4))
                    , CAST(@rtp AS numeric(19,4)));";

        var param = statistics.Select(s => new {
            s.report_time,
            s.category,
            s.bet_amount,
            s.bet_value,
            s.bet_count,
            s.bet_result,
            s.profit_result_percent,
            s.gamehall,
            s.fullname,
            s.gamecode,
            gameName = s.name,
            s.name_cn,
            s.currency,
            s.win_count,
            s.win_point,
            s.rtp,
        });

        await using var conn = new NpgsqlConnection(PGMaster);
        return await conn.ExecuteAsync(sql, param);
    }

    public async Task<int> DeleteTpReport(IEnumerable<StatisticsByGameResponse.Statistics> statistics)
    {
        var sql = @"DELETE FROM public.t_tp_game_report WHERE report_time = @report_time and gamecode = @gamecode";

        await using var conn = new NpgsqlConnection(PGMaster);
        return await conn.ExecuteAsync(sql, statistics.ToList());
    }
    #endregion

    #region t_bet_record_summary
    public async Task<ulong> BatchInsertRecordSummaryAsync(NpgsqlConnection conn, List<BetRecordSummary> summaryData)
    {
        string stSql = @"COPY t_bet_record_summary
                            (
                                id,
                                club_id,
                                game_id,
                                game_type,
                                bet_type,
                                bet_amount,
                                turnover,
                                win,
                                netwin,
                                jackpotwin,
                                reportdatetime,
                                currency,
                                recordcount,
                                franchiser_id,
                                updatedatetime
                            )
                            FROM STDIN (FORMAT BINARY)";

        using var writer = await conn.BeginBinaryImportAsync(stSql);
        foreach (var summary in summaryData)
        {
            await writer.StartRowAsync();
            await writer.WriteAsync(summary.id, NpgsqlTypes.NpgsqlDbType.Uuid);
            await writer.WriteAsync(summary.Club_id, NpgsqlTypes.NpgsqlDbType.Varchar);
            await writer.WriteAsync(summary.Game_id, NpgsqlTypes.NpgsqlDbType.Varchar);
            await writer.WriteAsync(summary.Game_type, NpgsqlTypes.NpgsqlDbType.Integer);
            await writer.WriteAsync(summary.Bet_type, NpgsqlTypes.NpgsqlDbType.Integer);
            await writer.WriteAsync(summary.Bet_amount.GetValueOrDefault(), NpgsqlTypes.NpgsqlDbType.Numeric);
            await writer.WriteAsync(summary.Turnover.GetValueOrDefault(), NpgsqlTypes.NpgsqlDbType.Numeric);
            await writer.WriteAsync(summary.Win.GetValueOrDefault(), NpgsqlTypes.NpgsqlDbType.Numeric);
            await writer.WriteAsync(summary.Netwin.GetValueOrDefault(), NpgsqlTypes.NpgsqlDbType.Numeric);
            await writer.WriteAsync(summary.JackpotWin.GetValueOrDefault(), NpgsqlTypes.NpgsqlDbType.Numeric);
            await writer.WriteAsync(summary.ReportDatetime.GetValueOrDefault(), NpgsqlTypes.NpgsqlDbType.Timestamp);
            await writer.WriteAsync(summary.Currency, NpgsqlTypes.NpgsqlDbType.Varchar);
            await writer.WriteAsync(summary.RecordCount, NpgsqlTypes.NpgsqlDbType.Integer);
            await writer.WriteAsync(summary.Franchiser_id, NpgsqlTypes.NpgsqlDbType.Varchar);
            await writer.WriteAsync(summary.updatedatetime, NpgsqlTypes.NpgsqlDbType.Timestamp);
        }

        return await writer.CompleteAsync();
    }
    public async Task<int> BulkInsertSummaryBetRecordMapping(IDbTransaction tran, IEnumerable<t_summary_bet_record_mapping> mappings)
    {
        if (tran == null) throw new ArgumentNullException(nameof(tran));
        if (mappings == null) throw new ArgumentNullException(nameof(mappings));
        if (!mappings.Any()) return 0;

        var tableGuid = Guid.NewGuid();
        //建立暫存表
        await CreateSummaryBetRecordMappingTempTable(tran, tableGuid);

        //將資料倒進暫存表
        await BulkInsertToSummaryTempTable(tran, tableGuid, mappings);

        //將資料由暫存表倒回主表(過濾重複)
        return await MergeFromSummaryTempTable(tran, tableGuid);
    }

    private Task<int> CreateSummaryBetRecordMappingTempTable(IDbTransaction tran, Guid tableGuid)
    {
        var sql = $@"CREATE TEMPORARY TABLE temp_t_summery_bet_record_mapping_{tableGuid:N} 
                            ( LIKE t_summery_bet_record_mapping INCLUDING ALL );";

        return tran.Connection.ExecuteAsync(sql, transaction: tran);
    }

    private async Task<ulong> BulkInsertToSummaryTempTable(IDbTransaction tran, Guid tableGuid,IEnumerable<t_summary_bet_record_mapping> mappings)
    {
        if (tran is not NpgsqlTransaction npTran)
            throw new ArgumentException("Must be NpgsqlTransaction", nameof(tran));

        if (npTran.Connection == null)
            throw new ArgumentNullException(nameof(tran.Connection));

        await using var writer =
            await npTran.Connection.BeginBinaryImportAsync(
                $"COPY temp_t_summery_bet_record_mapping_{tableGuid:N} (summary_id, report_time, partition_time) FROM STDIN (FORMAT BINARY)");
        foreach (var mapping in mappings)
        {
            await writer.StartRowAsync();
            await writer.WriteAsync(mapping.summary_id, NpgsqlDbType.Uuid);
            await writer.WriteAsync(mapping.report_time, NpgsqlDbType.Timestamp);
            await writer.WriteAsync(mapping.partition_time, NpgsqlDbType.Timestamp);
        }

        return await writer.CompleteAsync();
    }

    private Task<int> MergeFromSummaryTempTable(IDbTransaction tran, Guid tableGuid)
    {
        var sql = @$"INSERT INTO t_summery_bet_record_mapping
                        SELECT summary_id, report_time, partition_time 
                        FROM temp_t_summery_bet_record_mapping_{tableGuid:N} temp
                        WHERE NOT EXISTS 
                        (
                            SELECT NULL
                            FROM public.t_summery_bet_record_mapping
                            WHERE summary_id = temp.summary_id 
                                AND report_time = temp.report_time
                                AND partition_time = temp.partition_time
                        )
                        ";

        return tran.Connection.ExecuteAsync(sql, tran);
    }
    #endregion
}