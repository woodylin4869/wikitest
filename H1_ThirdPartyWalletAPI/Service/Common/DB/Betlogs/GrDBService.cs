using Dapper;
using H1_ThirdPartyWalletAPI.Model.Config;
using H1_ThirdPartyWalletAPI.Model.DataModel;
using H1_ThirdPartyWalletAPI.Model.DB.JDB.Response;
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
using System.Threading;
using System.Threading.Tasks;
using static H1_ThirdPartyWalletAPI.Model.Game.GR.Response.CommBetDetailsResponse;
using static H1_ThirdPartyWalletAPI.Model.Game.PME.Response.QueryScrollResponse;
using static H1_ThirdPartyWalletAPI.Model.Game.TP.Response.BetLogResponse;

namespace H1_ThirdPartyWalletAPI.Service.Common.DB.Betlogs;

public interface IGrDBService
{
    Task<int> PostGrRecord(NpgsqlConnection conn, IDbTransaction tran, IEnumerable<CommBetDetails> betLogs);
    Task<List<CommBetDetails>> GetGrRecordsBytimeForRepair(DateTime start, DateTime end);
    Task<(int totalCount, decimal totalBetValid, decimal totalWin)> SumGrBetRecordByBetTime(DateTime start, DateTime end);
    Task<IEnumerable<(int count, decimal win, decimal bet, string userid, decimal bet_valid, decimal netwin, DateTime bettime)>> SummaryGameRecord(DateTime reportTime, DateTime startTime, DateTime endTime);
    public Task<List<CommBetDetails>> GetGrRecordByReportTime(BetRecordSummary RecordReq, DateTime startTime, DateTime endTime, string Club_id);
    Task<List<CommBetDetails>> GettGrRecordsBySummary(GetBetRecordReq RecordReq);
}

public class GrDBService : BetlogsDBServiceBase, IGrDBService
{
    public GrDBService(ILogger<GrDBService> logger, IBetLogsDbConnectionStringManager connectionStringManager) : base(logger, connectionStringManager)
    {
    }

    public async Task<int> PostGrRecord(NpgsqlConnection conn, IDbTransaction tran, IEnumerable<CommBetDetails> betLogs)
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
        var sql = $@"CREATE TEMPORARY TABLE temp_t_gr_bet_record_v2_{tableGuid:N} 
                            ( LIKE t_gr_bet_record_v2  INCLUDING ALL );";

        return tran.Connection.ExecuteAsync(sql, transaction: tran);
    }

    private async Task<ulong> BulkInsertToTempTable(IDbTransaction tran, Guid tableGuid, IEnumerable<CommBetDetails> records)
    {
        if (tran is not NpgsqlTransaction npTran)
            throw new ArgumentException("Must be NpgsqlTransaction", nameof(tran));

        if (npTran.Connection == null)
            throw new ArgumentNullException(nameof(tran.Connection));

        await using var writer =
            await npTran.Connection.BeginBinaryImportAsync(
                @$"COPY temp_t_gr_bet_record_v2_{tableGuid:N} ( 
                    id_str, id, sid, account, game_type, game_module_type, game_round, game_round_str,
                    game_round_hex, bet, game_result, valid_bet, win, create_time, order_id, device,
                    client_ip, c_type, profit, room_id, table_id, bullet_count, report_time, partition_time
                ) FROM STDIN (FORMAT BINARY)");

        foreach (var mapping in records)
        {
            await writer.StartRowAsync();
            await writer.WriteAsync(mapping.id_str.ToString(), NpgsqlDbType.Varchar);
            await writer.WriteAsync(mapping.id, NpgsqlDbType.Bigint);
            await writer.WriteAsync(mapping.sid.ToString(), NpgsqlDbType.Varchar);
            await writer.WriteAsync(mapping.account.ToString(), NpgsqlDbType.Varchar);
            await writer.WriteAsync(mapping.game_type, NpgsqlDbType.Integer);
            await writer.WriteAsync(mapping.game_module_type, NpgsqlDbType.Integer);
            await writer.WriteAsync(mapping.game_round, NpgsqlDbType.Bigint);
            // 数值字段需要转换
            await writer.WriteAsync(mapping.game_round_str, NpgsqlDbType.Varchar);
            await writer.WriteAsync(mapping.game_round_hex, NpgsqlDbType.Varchar);
            await writer.WriteAsync(mapping.bet, NpgsqlDbType.Numeric);
            await writer.WriteAsync(mapping.game_result, NpgsqlDbType.Varchar);
            await writer.WriteAsync(mapping.valid_bet, NpgsqlDbType.Numeric);
            await writer.WriteAsync(mapping.win, NpgsqlDbType.Numeric);
            await writer.WriteAsync(mapping.create_time, NpgsqlDbType.Timestamp);
            await writer.WriteAsync(mapping.order_id, NpgsqlDbType.Varchar);
            await writer.WriteAsync(mapping.device, NpgsqlDbType.Varchar);
            await writer.WriteAsync(mapping.client_ip, NpgsqlDbType.Varchar);
            await writer.WriteAsync(mapping.c_type , NpgsqlDbType.Varchar);
            await writer.WriteAsync(mapping.profit, NpgsqlDbType.Numeric); 
            await writer.WriteAsync(mapping.room_id.HasValue ? (long)mapping.room_id.Value : (object)DBNull.Value, NpgsqlDbType.Bigint);
            await writer.WriteAsync(mapping.table_id.HasValue ? (long)mapping.table_id.Value : (object)DBNull.Value, NpgsqlDbType.Bigint);
            await writer.WriteAsync(mapping.bullet_count.HasValue ? (long)mapping.bullet_count.Value : (object)DBNull.Value , NpgsqlDbType.Bigint);
            await writer.WriteAsync(mapping.report_time, NpgsqlDbType.Timestamp);
            await writer.WriteAsync(mapping.partition_time, NpgsqlDbType.Timestamp);
        }

        return await writer.CompleteAsync();
    }
    private async Task<int> MergeFromTempTable(IDbTransaction tran, Guid tableGuid)
    {
        var sql = @$"INSERT INTO t_gr_bet_record_v2 
                        SELECT
                            id_str, id, sid, account, game_type, game_module_type, game_round, game_round_str,
                            game_round_hex, bet, game_result, valid_bet, win, create_time, order_id, device, 
                            client_ip, c_type, profit, room_id, table_id, bullet_count, report_time, db_create_time, partition_time
                        FROM temp_t_gr_bet_record_v2_{tableGuid:N} AS  temp
                        WHERE NOT EXISTS 
                        (
                            SELECT NULL
                            FROM public.t_gr_bet_record_v2
                            WHERE sid = temp.sid 
                              AND partition_time = temp.partition_time
                        )";

        var rows = await tran.Connection.ExecuteAsync(sql, tran);
        return rows;
    }

    /// <summary>
    /// 每小時匯總
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <returns></returns>
    public async Task<(int totalCount, decimal totalBetValid, decimal totalWin)> SumGrBetRecordByBetTime(DateTime start, DateTime end)
    {
        var sql = @"SELECT
                    COUNT(1) AS totalCount
                    ,coalesce(SUM(valid_bet),0) AS totalBetValid
                    ,coalesce(SUM(profit),0) AS totalWin
                    FROM t_gr_bet_record_v2
                    WHERE partition_time BETWEEN @startTime AND  @endTime";

        var par = new DynamicParameters();
        par.Add("@startTime", start);
        par.Add("@endTime", start.AddDays(1));

        await using var conn = new NpgsqlConnection(await PGRead);
        var result = (await conn.QueryAsync<dynamic>(sql, par)).FirstOrDefault();
        return ((int)result.totalcount, (decimal)result.totalbetvalid, (decimal)result.totalwin);
    }

    /// <summary>
    ///
    /// 取得時間內的注單 ForRepair
    ///
    /// 請求 GR 起始時間 與 結束時間 是包含等於
    /// 起始時間 <= 回傳資料時間 <= 結束時間
    /// start_time = startTime.AddHours(0).AddSeconds(1)
    /// end_time = endTime.AddHours(0).AddSeconds(0)
    /// 配合API輸入起訖時間
    ///
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <returns></returns>
    public async Task<List<CommBetDetails>> GetGrRecordsBytimeForRepair(DateTime start, DateTime end)
    {
        var sql = @"SELECT sid
                    FROM public.t_gr_bet_record
                    WHERE create_time > @startTime
                        AND create_time <= @endTime";
        var par = new DynamicParameters();
        par.Add("@startTime", start);
        par.Add("@endTime", end);

        await using var conn = new NpgsqlConnection(await PGRead);
        var result = await conn.QueryAsync<CommBetDetails>(sql, par);
        return result.ToList();
    }
    /// <summary>
    /// 取得遊戲彙總資料
    /// 後彙總使用
    /// </summary>
    /// <param name="reportTime"></param>
    /// <param name="startTime"></param>
    /// <param name="endTime"></param>
    /// <returns></returns>
    public async Task<IEnumerable<(int count, decimal win, decimal bet, string userid, decimal bet_valid, decimal netwin,DateTime bettime)>> SummaryGameRecord(DateTime reportTime, DateTime startTime, DateTime endTime)
    {
        var sql = @"SELECT CAST(count(1) AS INTEGER) AS count,
                        coalesce(SUM(win),0) AS win,
                        coalesce(SUM(bet),0) AS bet,
                        coalesce(SUM(valid_bet),0) AS bet_valid,
                        coalesce(SUM(profit),0) AS netwin,
                        DATE(partition_time) AS bettime,
                        account AS userid
                        FROM t_gr_bet_record_v2
                        WHERE partition_time BETWEEN @start_time AND @end_time
                        AND report_time = @report_time
                        GROUP BY account,partition_time
                        ";

        var par = new DynamicParameters();
        par.Add("@start_time", startTime);
        par.Add("@end_time", endTime);
        par.Add("@report_time", reportTime);
        using (NpgsqlConnection conn = new NpgsqlConnection(await PGRead))
        {
            var result = await conn.QueryAsync(sql, par, commandTimeout: 270);
            return result.Select(x => ((int)x.count, (decimal)x.win, (decimal)x.bet, (string)x.userid, (decimal)x.bet_valid, (decimal)x.netwin,(DateTime)x.bettime));
        }
    }
    public async Task<List<CommBetDetails>> GettGrRecordsBySummary(GetBetRecordReq RecordReq)
    {
        var sql = @"SELECT 
                    summary_id, id_str, id, sid, account, game_type, game_module_type, game_round,
                    game_round_str, game_round_hex, bet, game_result, valid_bet, win, create_time,
                    order_id, device,client_ip, c_type, profit, room_id, table_id, bullet_count
                    FROM public.t_gr_bet_record 
                    WHERE create_time >= @start 
                        AND create_time <= @end
                        AND summary_id = @summaryId::uuid";
        var param = new
        {
            summaryId = RecordReq.summary_id,
            start = RecordReq.ReportTime.AddDays(-3),
            end = RecordReq.ReportTime.AddDays(1),
        };

        await using var conn = new NpgsqlConnection(await PGRead);
        var result = await conn.QueryAsync<CommBetDetails>(sql, param);
        return result.ToList();
    }
    /// <summary>
    /// 取得第二層明細
    /// </summary>
    /// <param name="RecordReq"></param>
    /// <returns></returns>
    public async Task<List<CommBetDetails>> GetGrRecordByReportTime(BetRecordSummary RecordReq, DateTime startTime, DateTime endTime,string Club_id)
    {
        var sql = @"SELECT
                        id_str, id, sid, account, game_type, game_module_type, game_round, game_round_str,
                        game_round_hex, bet, game_result, valid_bet, win, create_time, order_id, device, client_ip,
                        c_type, profit, room_id, table_id, bullet_count, report_time, db_create_time, partition_time
                    FROM t_gr_bet_record_v2
                    WHERE partition_time BETWEEN @starttime AND @endtime
                    AND report_time = @reporttime
                    AND account = @account";

        var parameters = new DynamicParameters();
        parameters.Add("@starttime",startTime);
        parameters.Add("@endtime", endTime);
        parameters.Add("@reporttime", RecordReq.ReportDatetime);
        parameters.Add("@account", Club_id);

        using (var conn = new NpgsqlConnection(await PGRead))
        {
            return (await conn.QueryAsync<CommBetDetails>(sql, parameters))?.ToList() ?? new List<CommBetDetails>();
        }
    }
}