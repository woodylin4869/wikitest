using Dapper;
using H1_ThirdPartyWalletAPI.Model.Config;
using H1_ThirdPartyWalletAPI.Model.Game.WS168.Response;
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

namespace H1_ThirdPartyWalletAPI.Service.Common.DB.Betlogs;

public interface IWS168DBService
{
    Task<int> PostWS168Record(NpgsqlConnection conn, IDbTransaction tran, IEnumerable<SearchingOrdersStatusResponse.Datum> betLogs);
    Task<List<SearchingOrdersStatusResponse.Datum>> GetWS168RecordsBytime(DateTime start, DateTime end);
    Task<List<SearchingOrdersStatusResponse.Datum>> GetWS168Records(string id, DateTime time);
    Task<(int totalCount, decimal totalBetValid, decimal totalWin)> SumWS168BetRecordByBetTime(DateTime start, DateTime end);

    Task<List<SearchingOrdersStatusResponse.Datum>> GetWS168RecordsBySummary(GetBetRecordReq RecordReq);

    Task<int> Postws168RunningRecord(NpgsqlConnection conn, IDbTransaction tran, IEnumerable<SearchingOrdersStatusResponse.Datum> betLogs);
    Task<int> Deletews168RunningRecord(NpgsqlConnection conn, IDbTransaction tran, string slug, DateTime betTime);
    Task<IEnumerable<SearchingOrdersStatusResponse.Datum>> Getws168RunningRecord(GetBetRecordUnsettleReq RecordReq);
    Task<List<SearchingOrdersStatusResponse.Datum>> GetWS168RecordsV2(string id, DateTime time);
    Task<IEnumerable<(int count, decimal win, decimal bet, decimal jackpot, string userid, decimal netwin, DateTime bettime, decimal validbet, string Game_type)>> SummaryGameRecord
        (DateTime reportTime, DateTime startTime, DateTime endTime);
    Task<List<SearchingOrdersStatusResponse.Datum>> GetRecordsBytime(DateTime createtime, DateTime report_time, string club_id);
    Task<List<SearchingOrdersStatusResponse.Datum>> GetRecordsByBetTime(DateTime start, DateTime end);

}

public class WS168DBService : BetlogsDBServiceBase, IWS168DBService
{
    public WS168DBService(ILogger<WS168DBService> logger, IBetLogsDbConnectionStringManager connectionStringManager) : base(logger, connectionStringManager)
    {
    }
    /// <summary>
    /// 寫入資料
    /// </summary>
    /// <param name="conn"></param>
    /// <param name="tran"></param>
    /// <param name="betLogs"></param>
    /// <returns></returns>
    public async Task<int> PostWS168Record(NpgsqlConnection conn, IDbTransaction tran, IEnumerable<SearchingOrdersStatusResponse.Datum> betLogs)
    {

        //var sql = @"INSERT INTO public.t_ws168_bet_record
        //            (slug
        //            ,arena_fight_no
        //            ,round_id
        //            ,fight_no
        //            ,side
        //            ,account
        //            ,status
        //            ,odd
        //            ,bet_amount
        //            ,net_income 
        //            ,bet_return
        //            ,valid_amount
        //            ,""result""
        //            ,is_settled
        //            ,bet_at
        //            ,settled_at
        //            ,arena_no
        //            ,pre_bet_amount
        //            ,pre_net_income
        //            ,pre_valid_amount 
        //            ,summary_id)
        //            VALUES
        //            ( 
        //               @slug
        //              ,@arena_fight_no
        //              ,@round_id
        //              ,@fight_no
        //              ,@side
        //              ,@account
        //              ,@status
        //              ,@odd
        //              ,to_number(@bet_amount,'99G999D9S')
        //              ,to_number(@net_income,'99G999D9S')
        //              ,to_number(@bet_return,'99G999D9S')
        //              ,to_number(@valid_amount,'99G999D9S')
        //              ,@result
        //              ,@is_settled
        //              ,@bet_at
        //              ,@settled_at
        //              ,@arena_no
        //              ,to_number(@pre_bet_amount,'99G999D9S')
        //              ,to_number(@pre_net_income,'99G999D9S')
        //              ,to_number(@pre_valid_amount,'99G999D9S')
        //              ,@summary_id)";
        //try
        //{
        //    return await conn.ExecuteAsync(sql, betLogs, tran);
        //}
        //catch (Exception ex)
        //{
        //    throw;
        //}

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
        var sql = $@"CREATE TEMPORARY TABLE temp_t_ws168_bet_record_v2_{tableGuid:N} 
                            ( LIKE t_ws168_bet_record_v2  INCLUDING ALL );";
        return tran.Connection.ExecuteAsync(sql, transaction: tran);
    }
    private async Task<ulong> BulkInsertToTempTable(IDbTransaction tran, Guid tableGuid, IEnumerable<SearchingOrdersStatusResponse.Datum> records)
    {
        if (tran is not NpgsqlTransaction npTran)
            throw new ArgumentException("Must be NpgsqlTransaction", nameof(tran));

        if (npTran.Connection == null)
            throw new ArgumentNullException(nameof(tran.Connection));

        await using var writer =
            await npTran.Connection.BeginBinaryImportAsync(
                $@"COPY temp_t_ws168_bet_record_v2_{tableGuid:N} (slug, arena_fight_no, round_id, fight_no, side, account, status,
                            odd, bet_amount, net_income, bet_return, valid_amount, ""result"",is_settled, bet_at, settled_at, arena_no,
                            pre_bet_amount, pre_net_income, pre_valid_amount, report_time, partition_time) FROM STDIN (FORMAT BINARY)");

        foreach (var mapping in records)
        {
            await writer.StartRowAsync();
            await writer.WriteAsync(mapping.slug, NpgsqlDbType.Varchar); // varchar(255)
            await writer.WriteAsync(mapping.arena_fight_no.ToString(), NpgsqlDbType.Varchar); // varchar(255)
            await writer.WriteAsync(mapping.round_id, NpgsqlDbType.Integer); // int4
            await writer.WriteAsync(mapping.fight_no, NpgsqlDbType.Integer); // int4
            await writer.WriteAsync(mapping.side, NpgsqlDbType.Varchar); // varchar(255)
            await writer.WriteAsync(mapping.account, NpgsqlDbType.Varchar); // varchar(20)
            await writer.WriteAsync(mapping.status, NpgsqlDbType.Varchar); // varchar(20)
            await writer.WriteAsync(mapping.odd, NpgsqlDbType.Numeric); // numeric(14, 2)
            await writer.WriteAsync(decimal.Parse(mapping.bet_amount), NpgsqlDbType.Numeric); // numeric(14, 2)
            await writer.WriteAsync(decimal.Parse(mapping.net_income), NpgsqlDbType.Numeric); // numeric(14, 2)
            await writer.WriteAsync(decimal.Parse(mapping.bet_return), NpgsqlDbType.Numeric); // numeric(14, 2)
            await writer.WriteAsync(decimal.Parse(mapping.valid_amount), NpgsqlDbType.Numeric); // numeric(14, 2)
            await writer.WriteAsync(mapping.result, NpgsqlDbType.Varchar); // varchar(255)
            await writer.WriteAsync(mapping.is_settled, NpgsqlDbType.Boolean); // bool
            await writer.WriteAsync(mapping.bet_at, NpgsqlDbType.Timestamp); // timestamp
            await writer.WriteAsync(mapping.settled_at.HasValue ? mapping.settled_at.Value: (object)DBNull.Value, NpgsqlDbType.Timestamp); // timestamp
            await writer.WriteAsync(mapping.arena_no.ToString(), NpgsqlDbType.Varchar); // varchar(255)
            await writer.WriteAsync(decimal.Parse(mapping.pre_bet_amount), NpgsqlDbType.Numeric); // numeric(14, 2)
            await writer.WriteAsync(decimal.Parse(mapping.pre_net_income), NpgsqlDbType.Numeric); // numeric(14, 2)
            await writer.WriteAsync(decimal.Parse(mapping.pre_valid_amount), NpgsqlDbType.Numeric); // numeric(14, 2)
            await writer.WriteAsync(mapping.report_time, NpgsqlDbType.Timestamp); // timestamp
            await writer.WriteAsync(mapping.partition_time, NpgsqlDbType.Timestamp); // timestamp
        }

        return await writer.CompleteAsync();
    }
    private async Task<int> MergeFromTempTable(IDbTransaction tran, Guid tableGuid)
    {
        var sql = @$"INSERT INTO t_ws168_bet_record_v2
                        SELECT slug, arena_fight_no, round_id, fight_no, side, account, status, odd, bet_amount, net_income, bet_return, valid_amount, ""result"",
                               is_settled, bet_at, settled_at, arena_no, pre_bet_amount, pre_net_income, pre_valid_amount, create_time, report_time, partition_time
                        FROM temp_t_ws168_bet_record_v2_{tableGuid:N} AS  temp
                        WHERE NOT EXISTS 
                        (
                            SELECT NULL
                            FROM public.t_ws168_bet_record_v2
                            WHERE partition_time = temp.partition_time 
                                AND slug = temp.slug
                                AND status = temp.status
                        )";
        return await tran.Connection.ExecuteAsync(sql, tran);
    }

    public async Task<int> Postws168RunningRecord(NpgsqlConnection conn, IDbTransaction tran, IEnumerable<SearchingOrdersStatusResponse.Datum> betLogs)
    {
        //var sql = @"INSERT INTO public.t_ws168_bet_record_running
        //            (slug
        //            ,arena_fight_no
        //            ,round_id
        //            ,fight_no
        //            ,side
        //            ,account
        //            ,status
        //            ,odd
        //            ,bet_amount
        //            ,net_income 
        //            ,bet_return
        //            ,valid_amount
        //            ,""result""
        //            ,is_settled
        //            ,bet_at
        //            ,settled_at
        //            ,arena_no
        //            ,summary_id
        //            ,club_id
        //            ,franchiser_id)
        //            VALUES
        //            ( 
        //               @slug
        //              ,@arena_fight_no
        //              ,@round_id
        //              ,@fight_no
        //              ,@side
        //              ,@account
        //              ,@status
        //              ,@odd
        //              ,to_number(@bet_amount,'99G999D9S')
        //              ,to_number(@net_income,'99G999D9S')
        //              ,to_number(@bet_return,'99G999D9S')
        //              ,to_number(@valid_amount,'99G999D9S')
        //              ,@result
        //              ,@is_settled
        //              ,@bet_at
        //              ,@settled_at
        //              ,@arena_no
        //              ,@summary_id
        //              ,@club_id
        //              ,@franchiser_id)";
        //try
        //{
        //    return await conn.ExecuteAsync(sql, betLogs, tran);
        //}
        //catch (Exception ex)
        //{
        //    throw;
        //}if (tran == null) throw new ArgumentNullException(nameof(tran));
        if (betLogs == null) throw new ArgumentNullException(nameof(betLogs));
        if (!betLogs.Any()) return 0;

        var tableGuid = Guid.NewGuid();
        //建立暫存表
        await CreateBetrunningRecordTempTable(tran, tableGuid);
        //將資料倒進暫存表
        await BulkInsertTorunningTempTable(tran, tableGuid, betLogs);
        //將資料由暫存表倒回主表(過濾重複)
        return await MergeFromrunningTempTable(tran, tableGuid);
    }
    private Task<int> CreateBetrunningRecordTempTable(IDbTransaction tran, Guid tableGuid)
    {
        var sql = $@"CREATE TEMPORARY TABLE temp_t_ws168_bet_record_running_{tableGuid:N} 
                            ( LIKE t_ws168_bet_record_running  INCLUDING ALL );";
        return tran.Connection.ExecuteAsync(sql, transaction: tran);
    }
    private async Task<ulong> BulkInsertTorunningTempTable(IDbTransaction tran, Guid tableGuid, IEnumerable<SearchingOrdersStatusResponse.Datum> records)
    {
        if (tran is not NpgsqlTransaction npTran)
            throw new ArgumentException("Must be NpgsqlTransaction", nameof(tran));

        if (npTran.Connection == null)
            throw new ArgumentNullException(nameof(tran.Connection));

        await using var writer =
            await npTran.Connection.BeginBinaryImportAsync(
                $@"COPY temp_t_ws168_bet_record_running_{tableGuid:N} (slug, arena_fight_no, round_id, fight_no, side, account, status, odd,
                bet_amount, net_income, bet_return, valid_amount, ""result"", is_settled, bet_at, settled_at, arena_no, summary_id, club_id, franchiser_id
                ) FROM STDIN (FORMAT BINARY)");

        foreach (var mapping in records)
        {
            await writer.StartRowAsync();
            await writer.WriteAsync(mapping.slug.ToString(), NpgsqlDbType.Varchar); // varchar(255)
            await writer.WriteAsync(mapping.arena_fight_no.ToString(), NpgsqlDbType.Varchar); // varchar(255)
            await writer.WriteAsync(mapping.round_id, NpgsqlDbType.Integer); // int4
            await writer.WriteAsync(mapping.fight_no, NpgsqlDbType.Integer); // int4
            await writer.WriteAsync(mapping.side.ToString(), NpgsqlDbType.Varchar); // varchar(255)
            await writer.WriteAsync(mapping.account.ToString(), NpgsqlDbType.Varchar); // varchar(20)
            await writer.WriteAsync(mapping.status.ToString(), NpgsqlDbType.Varchar); // varchar(20)
            await writer.WriteAsync(mapping.odd, NpgsqlDbType.Numeric); // numeric(14, 2)
            await writer.WriteAsync(decimal.Parse(mapping.bet_amount), NpgsqlDbType.Numeric); // numeric(14, 2)
            await writer.WriteAsync(decimal.Parse(mapping.net_income), NpgsqlDbType.Numeric); // numeric(14, 2)
            await writer.WriteAsync(decimal.Parse(mapping.bet_return), NpgsqlDbType.Numeric); // numeric(14, 2)
            await writer.WriteAsync(decimal.Parse(mapping.valid_amount), NpgsqlDbType.Numeric); // numeric(14, 2)
            await writer.WriteAsync(mapping.result.ToString(), NpgsqlDbType.Varchar); // varchar(255)
            await writer.WriteAsync(mapping.is_settled, NpgsqlDbType.Boolean); // bool
            await writer.WriteAsync(mapping.bet_at, NpgsqlDbType.Timestamp); // timestamp
            await writer.WriteAsync(mapping.settled_at, NpgsqlDbType.Timestamp); // timestamp
            await writer.WriteAsync(mapping.arena_no.ToString(), NpgsqlDbType.Varchar); // varchar(255)
            await writer.WriteAsync(mapping.summary_id.ToString(), NpgsqlDbType.Uuid); // varchar(255)
            await writer.WriteAsync(mapping.club_id, NpgsqlDbType.Varchar); // varchar(20)
            await writer.WriteAsync(mapping.franchiser_id, NpgsqlDbType.Varchar); // varchar(20)
        }

        return await writer.CompleteAsync();
    }
    private async Task<int> MergeFromrunningTempTable(IDbTransaction tran, Guid tableGuid)
    {
        var sql = @$"INSERT INTO t_ws168_bet_record_running
                        SELECT slug, arena_fight_no, round_id, fight_no, side, account, status, odd,bet_amount, net_income,
                               bet_return, valid_amount, ""result"", is_settled, bet_at, settled_at, arena_no, summary_id, club_id, franchiser_id
                        FROM temp_t_ws168_bet_record_running_{tableGuid:N} AS  temp
                        WHERE NOT EXISTS 
                        (
                            SELECT NULL
                            FROM public.t_ws168_bet_record_running
                            WHERE bet_at = temp.bet_at 
                                AND slug = temp.slug
                        )";
        return await tran.Connection.ExecuteAsync(sql, tran);
    }


    /// <summary>
    /// 注單號取資料
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<List<SearchingOrdersStatusResponse.Datum>> GetWS168Records(string id, DateTime time)
    {
        var sql = @" SELECT  slug, arena_fight_no, round_id, fight_no, side, account, status, odd, bet_amount,
                             net_income, bet_return, valid_amount, ""result"", is_settled, bet_at, settled_at,
                             arena_no, pre_bet_amount, pre_net_income, pre_valid_amount, summary_id
                     FROM t_ws168_bet_record
                    WHERE slug = @id and
                          bet_at = @startTime 
                        AND bet_at < @endTime";
        var par = new DynamicParameters();
        par.Add("@id", id);
        par.Add("@startTime", time.AddDays(-3));
        par.Add("@endTime", time.AddDays(1));

        await using var conn = new NpgsqlConnection(await PGRead);
        var result = await conn.QueryAsync<SearchingOrdersStatusResponse.Datum>(sql, par);
        return result.ToList();
    }

    /// <summary>
    /// 每小時匯總
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <returns></returns>
    public async Task<(int totalCount, decimal totalBetValid, decimal totalWin)> SumWS168BetRecordByBetTime(DateTime start, DateTime end)
    {
        var sql = @"SELECT 
                    COUNT(slug) AS totalCount
                    , CASE WHEN SUM(valid_amount) IS NULL THEN 0 ELSE SUM(valid_amount) END  AS totalBetValid
                    , CASE WHEN SUM(bet_return) IS NULL THEN 0 ELSE SUM(bet_return) END AS totalWin
                    FROM t_ws168_bet_record
                    WHERE bet_at >= @startTime 
                        AND bet_at < @endTime";

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
    public async Task<List<SearchingOrdersStatusResponse.Datum>> GetWS168RecordsBytime(DateTime start, DateTime end)
    {
        try
        {
            var sql = @"SELECT *
                    FROM public.t_ws168_bet_record 
                    WHERE bet_at >= @startTime 
                        AND bet_at < @endTime";
            var par = new DynamicParameters();
            par.Add("@startTime", start);
            par.Add("@endTime", end);

            await using var conn = new NpgsqlConnection(await PGRead);
            var result = await conn.QueryAsync<SearchingOrdersStatusResponse.Datum>(sql, par);
            return result.ToList();
        }
        catch (Exception ex)
        {
            throw;
        }
    }

    /// <summary>
    /// 取得GUID資料
    /// </summary>
    /// <param name="RecordReq"></param>
    /// <returns></returns>
    public async Task<List<SearchingOrdersStatusResponse.Datum>> GetWS168RecordsBySummary(GetBetRecordReq RecordReq)
    {
        var sql = @"SELECT * 
                    FROM public.t_ws168_bet_record 
                    WHERE bet_at >= @start 
                        AND bet_at <= @end
                        AND summary_id = @summaryId::uuid";
        var param = new
        {
            summaryId = RecordReq.summary_id,
            start = RecordReq.ReportTime.AddDays(-3),
            end = RecordReq.ReportTime.AddDays(1),
        };

        await using var conn = new NpgsqlConnection(await PGRead);
        var result = await conn.QueryAsync<SearchingOrdersStatusResponse.Datum>(sql, param);
        return result.ToList();
    }
    /// <summary>
    /// 刪除未結算
    /// </summary>
    /// <param name="conn"></param>
    /// <param name="tran"></param>
    /// <param name="record_data"></param>
    /// <returns></returns>
    public async Task<int> Deletews168RunningRecord(NpgsqlConnection conn, IDbTransaction tran, string slug, DateTime betTime)
    {
        string strSqlDel = @"DELETE FROM t_ws168_bet_record_running
                               WHERE slug=@slug 
                               AND bet_at =@bet_at";
        var par = new DynamicParameters();
        par.Add("@slug", slug);
        par.Add("@bet_at", betTime);
        return await conn.ExecuteAsync(strSqlDel, par, tran);
    }

    public async Task<IEnumerable<SearchingOrdersStatusResponse.Datum>> Getws168RunningRecord(GetBetRecordUnsettleReq RecordReq)
    {
        var par = new DynamicParameters();
        string strSql = @"SELECT *
                    FROM t_ws168_bet_record_running
                    WHERE bet_at BETWEEN @StartTime AND @EndTime
                    ";

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
            return await conn.QueryAsync<SearchingOrdersStatusResponse.Datum>(strSql, par);
        }
    }

    #region V2
    /// <summary>
    /// 注單號取資料
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<List<SearchingOrdersStatusResponse.Datum>> GetWS168RecordsV2(string id, DateTime time)
    {
        var sql = @" SELECT  slug, arena_fight_no, round_id, fight_no, side, account, status, odd, bet_amount,
                             net_income, bet_return, valid_amount, ""result"", is_settled, bet_at, settled_at, arena_no,
                             pre_bet_amount, pre_net_income, pre_valid_amount, create_time, report_time, partition_time
                    FROM t_ws168_bet_record_v2
                    WHERE slug = @id and
                          partition_time = @partition_time";

        var par = new DynamicParameters();
        par.Add("@id", id);
        par.Add("@partition_time", time);

        await using var conn = new NpgsqlConnection(await PGRead);
        var result = await conn.QueryAsync<SearchingOrdersStatusResponse.Datum>(sql, par);
        return result.ToList();
    }

    public async Task<IEnumerable<(int count, decimal win, decimal bet, decimal jackpot, string userid, decimal netwin, DateTime bettime ,decimal validbet,string Game_type)>> SummaryGameRecord(DateTime reportTime, DateTime startTime, DateTime endTime)
    {
        // TODO 後匯總WS168更名
        var sql = @"SELECT CAST(count(1) AS INTEGER) AS count,
                               COALESCE (SUM(bet_return),0) AS win,
                               COALESCE (SUM(bet_amount),0) AS bet,
                               COALESCE (SUM(net_income),0) AS netwin,
                               coalesce(SUM(valid_amount),0) AS validbet,
                               account AS userid,
                               arena_no AS game_type,
                               DATE(partition_time) as bettime
                        FROM t_ws168_bet_record_v2
                        WHERE partition_time BETWEEN @start_time AND @end_time
                        AND report_time = @report_time
                        GROUP BY Date(partition_time),account,arena_no
                        ";

        var par = new DynamicParameters();
        par.Add("@start_time", startTime);
        par.Add("@end_time", endTime);
        par.Add("@report_time", reportTime);
        using (NpgsqlConnection conn = new NpgsqlConnection(await PGRead))
        {
            var result = await conn.QueryAsync(sql, par, commandTimeout: 270);

            return result.Select(x => ((int)x.count, (decimal)x.win, (decimal)x.bet, (decimal)0, (string)x.userid, (decimal)x.netwin, (DateTime)x.bettime, (decimal)x.validbet,(string)x.game_type)).ToList();
        }
    }

    /// <summary>
    /// report_time 及時間區間
    /// </summary>
    /// <param name="createtime"></param>
    /// <param name="report_time"></param>
    /// <returns></returns>
    public async Task<List<SearchingOrdersStatusResponse.Datum>> GetRecordsBytime(DateTime createtime, DateTime report_time, string club_id)
    {
        try
        {
            var sql = @"SELECT slug, arena_fight_no, round_id, fight_no, side, account, status, odd, bet_amount,
                               net_income, bet_return, valid_amount, ""result"", is_settled, bet_at, settled_at, arena_no,
                               pre_bet_amount, pre_net_income, pre_valid_amount, create_time, report_time, partition_time
                        FROM public.t_ws168_bet_record_v2 
                        WHERE partition_time BETWEEN @starttime AND @endtime
                        AND report_time = @reporttime
                        AND account=@club_id";
            var parameters = new DynamicParameters();
            parameters.Add("@starttime", createtime);
            parameters.Add("@endtime", createtime.AddDays(1).AddMilliseconds(-1));
            parameters.Add("@reporttime", report_time);
            parameters.Add("@club_id", Config.OneWalletAPI.Prefix_Key + club_id);

            await using var conn = new NpgsqlConnection(await PGRead);
            var result = await conn.QueryAsync<SearchingOrdersStatusResponse.Datum>(sql, parameters);
            return result.ToList();
        }
        catch (Exception ex)
        {
            throw;
        }
    }

    /// <summary>
    /// 依下注時間取得注單
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <returns></returns>
    public async Task<List<SearchingOrdersStatusResponse.Datum>> GetRecordsByBetTime(DateTime start, DateTime end)
    {
        var sql = @"SELECT  slug, bet_at
                        FROM t_ws168_bet_record
                        WHERE bet_at BETWEEN @starttime AND @endtime ";
        var par = new DynamicParameters();
        par.Add("@startTime", start);
        par.Add("@endTime", end);

        await using var conn = new NpgsqlConnection(await PGRead);
        var result = await conn.QueryAsync<SearchingOrdersStatusResponse.Datum>(sql, par);
        return result.ToList();
    }
    #endregion
}
