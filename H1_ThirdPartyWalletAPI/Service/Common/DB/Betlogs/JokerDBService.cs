using Dapper;
using H1_ThirdPartyWalletAPI.Model.DataModel;
using H1_ThirdPartyWalletAPI.Utility;
using Microsoft.Extensions.Logging;
using Npgsql;
using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace H1_ThirdPartyWalletAPI.Service.Common.DB.Betlogs;

public interface IJokerDBService
{
    /// <summary>
    /// 新增遊戲注單
    /// </summary>
    /// <param name="conn"></param>
    /// <param name="tran"></param>
    /// <param name="source"></param>
    /// <returns></returns>
    Task<int> PostJokerRecord(NpgsqlConnection conn, IDbTransaction tran, IEnumerable<t_joker_bet_record> source);

    /// <summary>
    /// 根據時間區間取得注單明細
    /// </summary>
    /// <param name="startTime"></param>
    /// <param name="endTime"></param>
    /// <returns></returns>
    Task<List<t_joker_bet_record>> GetJokerRecordsV2ByBetTime(DateTime startTime, DateTime endTime);

    /// <summary>
    /// 根據時間區間統計注單輸贏
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <returns></returns>
    Task<(int totalCount, decimal totalBetValid, decimal totalNetWin, decimal Jackpotwin)> SumJokerBetRecordByBetTime(DateTime start, DateTime end);

    /// <summary>
    /// 後匯總
    /// </summary>
    /// <param name="reportTime"></param>
    /// <param name="startTime"></param>
    /// <param name="endTime"></param>
    /// <returns></returns>
    public Task<IEnumerable<(int count, decimal win, decimal bet, decimal jackpot, string userid, DateTime bettime)>> SummaryGameRecord(DateTime reportTime, DateTime startTime, DateTime endTime);

    /// <summary>
    /// 第二層明細取得新表注單明細
    /// </summary>
    /// <param name="RecordReq"></param>
    /// <returns></returns>
    public Task<List<t_joker_bet_record>> GetJokerRecordByReportTime(BetRecordSummary RecordReq, DateTime startTime, DateTime endTime, string username);

    /// <summary>
    /// 新增遊戲匯總紀錄
    /// </summary>
    /// <param name="source"></param>
    /// <returns></returns>
    //public Task<int> PostJokerReport(t_joker_game_report source);
    /// <summary>
    /// 刪除遊戲匯總紀錄
    /// </summary>
    /// <param name="source"></param>
    /// <returns></returns>
    //public Task<int> DeleteJokerReport(t_joker_game_report source);


}

public class JokerDBService : BetlogsDBServiceBase, IJokerDBService
{
    public JokerDBService(ILogger<JokerDBService> logger, IBetLogsDbConnectionStringManager connectionStringManager) : base(logger, connectionStringManager)
    {
    }
    #region t_Joker_bet_record

    /// <summary>
    /// 根據時間區間統計注單輸贏
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <returns></returns>
    public async Task<(int totalCount, decimal totalBetValid, decimal totalNetWin, decimal Jackpotwin)> SumJokerBetRecordByBetTime(DateTime start, DateTime end)
    {
        var sql = @"SELECT 
                    COUNT(ocode) AS totalCount
                    , coalesce(SUM(amount),0) AS totalBetValid
                    , coalesce(SUM(result),0) AS totalNetWin
                    , coalesce(SUM(jackpotwin),0) AS jackpotwin
                    FROM t_joker_bet_record_v2
                    WHERE partition_time  BETWEEN @startTime AND @endTime ";

        var par = new DynamicParameters();
        par.Add("@startTime", start);
        par.Add("@endTime", end);

        await using var conn = new NpgsqlConnection(await PGRead);
        var result = (await conn.QueryAsync<dynamic>(sql, par)).FirstOrDefault();
        return ((int)result.totalcount, (decimal)result.totalbetvalid, (decimal)result.totalnetwin, (decimal)result.jackpotwin);
    }

    #endregion

    #region t_joker_bet_record_v2
    /// <summary>
    /// 新增遊戲注單
    /// </summary>
    /// <param name="conn"></param>
    /// <param name="tran"></param>
    /// <param name="source"></param>
    /// <returns></returns>
    public async Task<int> PostJokerRecord(NpgsqlConnection conn, IDbTransaction tran, IEnumerable<t_joker_bet_record> source)
    {
        #region ordcode
        //var sql = @"INSERT INTO public.t_joker_bet_record_v2
        //            (
        //             ocode,
        //             username,
        //             gamecode,
        //             description,
        //             type,
        //             amount,
        //             result,
        //             time,
        //             report_time,
        //                roundid,
        //                transactionocode,
        //                bettype,
        //                jackpotwin
        //            )
        //            VALUES
        //            (
        //             @ocode,
        //             @username,
        //             @gamecode,
        //             @description,
        //             @type,
        //             @amount,
        //             @result,
        //             @time,
        //             @report_time,
        //                @roundid,
        //                @transactionocode,
        //                @bettype,
        //                @jackpotwin
        //            );";

        //return await conn.ExecuteAsync(sql, source, tran);

        #endregion ordcode

        if (tran == null) throw new ArgumentNullException(nameof(tran));
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (!source.Any()) return 0;

        var tableGuid = Guid.NewGuid();
        //建立暫存表
        await CreateBetRecordTempTable(tran, tableGuid);
        //將資料倒進暫存表
        await BulkInsertToTempTable(tran, tableGuid, source);
        //將資料由暫存表倒回主表(過濾重複)
        return await MergeFromTempTable(tran, tableGuid);
    }

    private Task<int> CreateBetRecordTempTable(IDbTransaction tran, Guid tableGuid)
    {
        var sql = $@"CREATE TEMPORARY TABLE temp_t_joker_bet_record_v2_{tableGuid:N} 
                            ( LIKE t_joker_bet_record_v2  INCLUDING ALL );";

        return tran.Connection.ExecuteAsync(sql, transaction: tran);
    }

    private async Task<ulong> BulkInsertToTempTable(IDbTransaction tran, Guid tableGuid, IEnumerable<t_joker_bet_record> records)
    {
        if (tran is not NpgsqlTransaction npTran)
            throw new ArgumentException("Must be NpgsqlTransaction", nameof(tran));

        if (npTran.Connection == null)
            throw new ArgumentNullException(nameof(tran.Connection));

        await using var writer =
            await npTran.Connection.BeginBinaryImportAsync(
                $@"COPY temp_t_joker_bet_record_v2_{tableGuid:N} (ocode, username, gamecode, description, type,amount, result, time,
                roundid, transactionocode, report_time, jackpotwin, bettype, partition_time) FROM STDIN (FORMAT BINARY)");

        foreach (var mapping in records)
        {
            await writer.StartRowAsync();
            await writer.WriteAsync(mapping.Ocode.ToString(), NpgsqlDbType.Varchar);
            await writer.WriteAsync(mapping.Username.ToString(), NpgsqlDbType.Varchar);
            await writer.WriteAsync(mapping.Gamecode.ToString(), NpgsqlDbType.Varchar);
            await writer.WriteAsync(mapping.Description.ToString(), NpgsqlDbType.Varchar);
            await writer.WriteAsync(mapping.Type.ToString(), NpgsqlDbType.Varchar);
            await writer.WriteAsync(mapping.Amount, NpgsqlDbType.Numeric);
            await writer.WriteAsync(mapping.Result, NpgsqlDbType.Numeric);
            await writer.WriteAsync(mapping.Time, NpgsqlDbType.Timestamp);
            // 数值字段需要转换
            await writer.WriteAsync(mapping.Roundid.ToString(), NpgsqlDbType.Varchar);
            await writer.WriteAsync(mapping.Transactionocode.ToString(), NpgsqlDbType.Varchar);
            await writer.WriteAsync(mapping.report_time, NpgsqlDbType.Timestamp);
            await writer.WriteAsync(mapping.JackpotWin, NpgsqlDbType.Numeric);
            await writer.WriteAsync((int)mapping.BetType, NpgsqlDbType.Integer);
            await writer.WriteAsync(mapping.Partition_time, NpgsqlDbType.Timestamp);
        }

        return await writer.CompleteAsync();
    }
    private async Task<int> MergeFromTempTable(IDbTransaction tran, Guid tableGuid)
    {
        var sql = @$"INSERT INTO t_joker_bet_record_v2
                        SELECT ocode, username, gamecode, description, type,amount, result, time,
                               roundid, transactionocode, report_time, create_time, jackpotwin, bettype, partition_time
                        FROM temp_t_joker_bet_record_v2_{tableGuid:N} AS  temp
                        WHERE NOT EXISTS 
                        (
                            SELECT NULL
                            FROM public.t_joker_bet_record_v2
                            WHERE ocode = temp.ocode 
                                AND partition_time = temp.partition_time
                        )";
        return await tran.Connection.ExecuteAsync(sql, tran);
    }

    /// <summary>
    /// 根據時間區間取得注單明細
    /// </summary>
    /// <param name="startTime"></param>
    /// <param name="endTime"></param>
    /// <returns></returns>
    public async Task<List<t_joker_bet_record>> GetJokerRecordsV2ByBetTime(DateTime startTime, DateTime endTime)
    {
        var sql = @"SELECT ocode FROM t_joker_bet_record_v2
                    WHERE partition_time BETWEEN @startTime AND @endTime";

        var par = new DynamicParameters();
        par.Add("@startTime", startTime);
        par.Add("@endTime", endTime);

        await using var conn = new NpgsqlConnection(await PGRead);
        var result = await conn.QueryAsync<t_joker_bet_record>(sql, par);
        return result.ToList();
    }
    public async Task<List<t_joker_bet_record>> GetJokerRecordByReportTime(BetRecordSummary RecordReq, DateTime startTime, DateTime endTime, string username)
    {
        var sql = @"SELECT ocode, username, gamecode, description, ""type"", amount, ""result"", ""time"", roundid, transactionocode FROM t_joker_bet_record_v2
                        WHERE partition_time BETWEEN @starttime AND @endtime
                        AND report_time = @reporttime
                        AND username = @userid";
        var parameters = new DynamicParameters();
        parameters.Add("@starttime", RecordReq.ReportDatetime.GetValueOrDefault().AddDays(-3));
        parameters.Add("@endtime", RecordReq.ReportDatetime.GetValueOrDefault().AddMinutes(5));
        parameters.Add("@reporttime", RecordReq.ReportDatetime);
        parameters.Add("@userid", username);

        using (var conn = new NpgsqlConnection(await PGRead))
        {
            var result = await conn.QueryAsync<t_joker_bet_record>(sql, parameters);
            return result.ToList();
        }
    }
    #endregion

    #region t_Joker_game_report
    /// <summary>
    /// 後匯總
    /// </summary>
    /// <param name="reportTime"></param>
    /// <param name="startTime"></param>
    /// <param name="endTime"></param>
    /// <returns></returns>
    public async Task<IEnumerable<(int count, decimal win, decimal bet, decimal jackpot, string userid, DateTime bettime)>> SummaryGameRecord(DateTime reportTime, DateTime startTime, DateTime endTime)
    {
        var sql = @"SELECT CAST(count(1) AS INTEGER) AS count,
                    coalesce(SUM(result),0) AS win,
                    coalesce(SUM(amount),0) AS bet,
                    coalesce(SUM(jackpotwin),0) AS jackpot,
                        username as userid,
                        DATE(partition_time) as bettime
                        FROM t_joker_bet_record_v2
                        WHERE partition_time BETWEEN @start_time AND @end_time
                        AND report_time = @report_time
                        GROUP BY username,partition_time
                        ";

        var par = new DynamicParameters();
        par.Add("@start_time", startTime);
        par.Add("@end_time", endTime);
        par.Add("@report_time", reportTime);
        using (NpgsqlConnection conn = new NpgsqlConnection(PGMaster))
        {
            var result = await conn.QueryAsync(sql, par, commandTimeout: 270);
            return result.Select(x => ((int)x.count, (decimal)x.win, (decimal)x.bet, (decimal)x.jackpot, (string)x.userid, (DateTime)x.bettime));
        }
    }

    /// <summary>
    /// 新增遊戲匯總紀錄
    /// </summary>
    /// <param name="source"></param>
    /// <returns></returns>
    //public async Task<int> PostJokerReport(t_joker_game_report source)
    //{
    //    var sql = @"INSERT INTO public.t_joker_game_report
    //                (
    //                 time,
    //                 amount,
    //                 result,
    //                    count
    //                )
    //                VALUES
    //                (
    //                 @time,
    //                 @amount,
    //                 @result,
    //                 @count
    //                ) ";
    //    await using var conn = new NpgsqlConnection(PGMaster);
    //    return await conn.ExecuteAsync(sql, source);
    //}

    /// <summary>
    /// 刪除遊戲匯總紀錄
    /// </summary>
    /// <param name="source"></param>
    /// <returns></returns>
    //public async Task<int> DeleteJokerReport(t_joker_game_report source)
    //{
    //    var sql = @"DELETE FROM t_joker_game_report
    //                WHERE time=@time ";

    //    await using var conn = new NpgsqlConnection(PGMaster);
    //    return await conn.ExecuteAsync(sql, source);
    //}

    #endregion




}