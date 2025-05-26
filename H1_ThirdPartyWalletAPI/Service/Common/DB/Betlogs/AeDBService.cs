using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using H1_ThirdPartyWalletAPI.Model.Config;
using H1_ThirdPartyWalletAPI.Model.DataModel;
using H1_ThirdPartyWalletAPI.Model.DB.JDB.Response;
using H1_ThirdPartyWalletAPI.Model.Game.AE.Response;
using H1_ThirdPartyWalletAPI.Model.Game.JDB.Enum;
using H1_ThirdPartyWalletAPI.Model.W1API;
using H1_ThirdPartyWalletAPI.Utility;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;
using static Google.Api.ResourceDescriptor.Types;
using static H1_ThirdPartyWalletAPI.Model.Game.TP.Response.BetLogResponse;

namespace H1_ThirdPartyWalletAPI.Service.Common.DB.Betlogs;

public interface IAeDBService
{
    public Task<dynamic> GetAeRecord(string betid, DateTime reportTime);

    public Task<int> PostAeRecord(NpgsqlConnection conn, IDbTransaction tran, List<BetHistory> recordData);

    public Task<IEnumerable<(int count, decimal win, decimal bet, string userid, decimal jackpotwin, DateTime createtime)>> SummaryGameRecord(DateTime reportTime, DateTime startTime, DateTime endTime);

    public Task<IEnumerable<dynamic>> GetAeRecordBySummary(GetBetRecordReq recordReq);

    Task<List<dynamic>> SumAeBetRecordHourly(DateTime reportDate);

    Task<List<t_ae_bet_record>> SumAeBetRecord(DateTime startDateTime, DateTime endDateTime);

    Task<IEnumerable<dynamic>> GetAeRecordByReportTime(string club_id, DateTime report_time, DateTime start, DateTime end);

    Task<dynamic> GetAeRecordByReportTime(DateTime reportTime, long round_id);

    Task<List<t_ae_bet_record>> SumAeBetRecordV2(DateTime startDateTime, DateTime endDateTime);
}

public class AeDBService : BetlogsDBServiceBase, IAeDBService
{
    public AeDBService(ILogger<AeDBService> logger, IOptions<DBConnection> options, IBetLogsDbConnectionStringManager connectionStringManager) : base(logger, connectionStringManager)
    {
    }

    #region t_ae_bet_record

    public async Task<dynamic> GetAeRecord(string betid, DateTime reportTime)
    {
        var sql = @"SELECT * FROM t_ae_bet_record
                        WHERE round_id = @round_id
                        AND completed_at > @start_date
                        AND completed_at < @end_date";

        var parameters = new DynamicParameters();
        parameters.Add("@round_id", long.Parse(betid));
        parameters.Add("@start_date", reportTime.AddDays(-3));
        parameters.Add("@end_date", reportTime.AddDays(1));

        using (var conn = new NpgsqlConnection(await PGRead))
        {
            return await conn.QuerySingleOrDefaultAsync<dynamic>(sql, parameters);
        }
    }

    public async Task<IEnumerable<(int count, decimal win, decimal bet, string userid,decimal jackpotwin,DateTime createtime)>> SummaryGameRecord(DateTime reportTime, DateTime startTime, DateTime endTime)
    {
        // TODO 後匯總jdb更名
        var sql = @"SELECT CAST(count(1) AS INTEGER) AS count,
                        SUM(coalesce(payout_amt,0)) AS win,
                        SUM(coalesce(bet_amt,0)) AS bet,
                        account_name AS userid,
                        SUM(coalesce(prize_amt,0)) AS jackpotwin,
                        Date(completed_at) as createtime
                        FROM t_ae_bet_record_v2
                        WHERE completed_at BETWEEN @start_time AND @end_time
                        AND report_time = @report_time
                        GROUP BY account_name,Date(completed_at)
                        ";

        var par = new DynamicParameters();
        par.Add("@start_time", startTime);
        par.Add("@end_time", endTime);
        par.Add("@report_time", reportTime);
        using (NpgsqlConnection conn = new NpgsqlConnection(await PGRead))
        {
            var result = await conn.QueryAsync(sql, par, commandTimeout: 270);
            return result.Select(x => ((int)x.count, (decimal)x.win, (decimal)x.bet, (string)x.userid,(decimal)x.jackpotwin, (DateTime) x.createtime));
        }
    }

    public async Task<IEnumerable<dynamic>> GetAeRecordBySummary(GetBetRecordReq recordReq)
    {
        var sql = @"SELECT * FROM t_ae_bet_record
                        WHERE summary_id = @summary_id
                        AND completed_at > @start_date
                        AND completed_at < @end_date";

        var parameters = new DynamicParameters();
        parameters.Add("@summary_id", Guid.Parse(recordReq.summary_id));
        parameters.Add("@start_date", recordReq.ReportTime.AddDays(-3));
        parameters.Add("@end_date", recordReq.ReportTime.AddDays(1));
        using (var conn = new NpgsqlConnection(await PGRead))
        {
            return await conn.QueryAsync<dynamic>(sql, parameters);
        }
    }

    public async Task<List<dynamic>> SumAeBetRecordHourly(DateTime reportDate)
    {
        var sql = @"SELECT
                    COUNT(*) AS total_cont
                    , SUM(coalesce(bet_amt,0)) AS total_bet
                    , SUM(coalesce(payout_amt,0)) AS total_win
                    FROM t_ae_bet_record_v2
                    WHERE completed_at >= @startTime
                    AND completed_at <= @endTime ";

        var par = new DynamicParameters();
        par.Add("@startTime", reportDate);
        par.Add("@endTime", reportDate.AddHours(1).AddSeconds(-1));

        using (var conn = new NpgsqlConnection(await PGRead))
        {
            var result = await conn.QueryAsync<dynamic>(sql, par);
            return result.ToList();
        }
    }

    public async Task<List<t_ae_bet_record>> SumAeBetRecord(DateTime startDateTime, DateTime endDateTime)
    {
        var sql = @"
                    SELECT * FROM t_ae_bet_record
                    WHERE completed_at >= @startTime
                        AND completed_at < @endTime ";

        var par = new DynamicParameters();
        par.Add("@startTime", startDateTime);
        par.Add("@endTime", endDateTime);

        using (var conn = new NpgsqlConnection(await PGRead))
        {
            var result = await conn.QueryAsync<t_ae_bet_record>(sql, par);
            return result.ToList();
        }
    }

    #endregion t_ae_bet_record

    #region t_ae_bet_record_v2

    public async Task<int> PostAeRecord(NpgsqlConnection conn, IDbTransaction tran, List<BetHistory> recordData)
    {

        var tempTableName = $"t_ae_bet_record_v2_{Guid.NewGuid():N}";
        try
        {
            await CreateTempTable(conn, tran, tempTableName);
            await BulkInsertTonextspinTempTable(conn, tran, tempTableName, recordData);
            return await MergenextspinRecordFromTempTable(conn, tran, tempTableName);
        }
        finally
        {
            await RemovePostnextspinRecordTempTable(conn, tran, tempTableName);
        }
        //var sql = @"INSERT INTO public.t_ae_bet_record_v2
        //                (
        //                     account_name,
        //                     currency,
        //                     bet_amt,
        //                     payout_amt,
        //                     bet_at,
        //                     end_balance,
        //                     rebate_amt,
        //                     game_id,
        //                     round_id,
        //                     free,
        //                     completed_at,
        //                     jp_pc_con_amt,
        //                     jp_pc_win_amt,
        //                     jp_jc_con_amt,
        //                     jp_jc_win_amt,
        //                     jp_win_id,
        //                     jp_win_lv,
        //                     jp_direct_pay,
        //                     prize_type,
        //                     prize_amt,
        //                     side_id,
        //                     report_time
        //                )
        //             VALUES
        //                (
        //                     @account_name,
        //                     @currency,
        //                     CAST(@bet_amt AS DECIMAL ),
        //                     CAST(@payout_amt AS DECIMAL ),
        //                     @bet_at,
        //                     CAST(@end_balance AS DECIMAL ),
        //                     CAST(@rebate_amt AS DECIMAL ),
        //                     @game_id,
        //                     @round_id,
        //                     @free,
        //                     @completed_at,
        //                     CAST(@jp_pc_con_amt AS DECIMAL ),
        //                     CAST(@jp_pc_win_amt AS DECIMAL ),
        //                     CAST(@jp_jc_con_amt AS DECIMAL ),
        //                     CAST(@jp_jc_win_amt AS DECIMAL ),
        //                     @jp_win_id,
        //                     @jp_win_lv,
        //                     @jp_direct_pay,
        //                     @prize_type,
        //                     CAST(@prize_amt AS DECIMAL ),
        //                     @side_id,
        //                     @report_time
        //                );";

        //return await conn.ExecuteAsync(sql, recordData, tran);
    }

    /// <summary>
    /// 取得第二層明細
    /// </summary>
    /// <param name="RecordReq"></param>
    /// <returns></returns>
    public async Task<IEnumerable<dynamic>> GetAeRecordByReportTime(string club_id, DateTime report_time, DateTime start, DateTime end)
    {
        var sql = @"SELECT
                        *
                    FROM t_ae_bet_record_v2
                    WHERE completed_at BETWEEN @starttime AND @endtime
                    AND report_time = @reporttime
                    AND account_name = @account_name";

        var parameters = new DynamicParameters();
        parameters.Add("@starttime", start);
        parameters.Add("@endtime", end);
        parameters.Add("@reporttime", report_time);
        parameters.Add("@account_name", club_id.ToLower());

        using (var conn = new NpgsqlConnection(await PGRead))
        {
            return await conn.QueryAsync<dynamic>(sql, parameters);
        }
    }

    /// <summary>
    /// 依照BetRecordSummary、遊戲序號找到明細資料
    /// </summary>
    /// <param name="RecordReq"></param>
    /// <param name="round_id">遊戲序號</param>
    /// <returns></returns>
    public async Task<dynamic> GetAeRecordByReportTime(DateTime reportTime, long round_id)
    {
        var par = new DynamicParameters();
        string strSql = @"SELECT
                                    *
                    FROM t_ae_bet_record_v2
                    WHERE completed_at > @start_date
                    AND completed_at < @end_date
                    AND report_time = @reporttime
                    AND round_id = @round_id
                    ";
        par.Add("@round_id", round_id);
        par.Add("@start_date", reportTime.AddDays(-3));
        par.Add("@end_date", reportTime.AddDays(1));
        par.Add("@reporttime", reportTime);
        using (NpgsqlConnection conn = new NpgsqlConnection(await PGRead))
        {
            return await conn.QuerySingleOrDefaultAsync<dynamic>(strSql, par);
        }
    }

    public async Task<List<t_ae_bet_record>> SumAeBetRecordV2(DateTime startDateTime, DateTime endDateTime)
    {
        var sql = @"
                    SELECT * FROM t_ae_bet_record_v2
                    WHERE completed_at >= @startTime
                        AND completed_at < @endTime ";

        var par = new DynamicParameters();
        par.Add("@startTime", startDateTime);
        par.Add("@endTime", endDateTime);

        using (var conn = new NpgsqlConnection(await PGRead))
        {
            var result = await conn.QueryAsync<t_ae_bet_record>(sql, par);
            return result.ToList();
        }
    }

    /// <summary>
    /// 建立站存表
    /// </summary>
    /// <param name="conn"></param>
    /// <param name="tran"></param>
    /// <param name="tempTableName"></param>
    /// <returns></returns>
    private async Task<string> CreateTempTable(NpgsqlConnection conn, IDbTransaction tran, string tempTableName)
    {
        //var tempTableName = $"temp_t_rsg_bet_record_{Guid.NewGuid():N}";
        var sql = "CREATE TEMPORARY TABLE IF NOT EXISTS #TempTableName ( LIKE t_ae_bet_record_v2 INCLUDING ALL);";
        sql = sql.Replace("#TempTableName", tempTableName);
        // 建立temp資料表
        await conn.ExecuteAsync(sql, tran, commandTimeout: 60);

        // 建立唯一索引避免資料重複
        sql = $"CREATE UNIQUE index IF NOT EXISTS {tempTableName}_un ON {tempTableName} (round_id);";
        await conn.ExecuteAsync(sql, tran, commandTimeout: 60);

        return tempTableName;
    }

    /// <summary>
    /// Copy至暫存表
    /// </summary>
    /// <param name="conn"></param>
    /// <param name="tran"></param>
    /// <param name="tempTableName"></param>
    /// <param name="record_data"></param>
    /// <returns></returns>
    private async Task<ulong> BulkInsertTonextspinTempTable(NpgsqlConnection conn, IDbTransaction tran, string tempTableName,
    IEnumerable<BetHistory> record_data)
    {
        var sql = @"COPY #TempTableName (
                         account_name,
                         currency,
                         bet_amt,
                         payout_amt,
                         bet_at,
                         end_balance,
                         rebate_amt,
                         game_id,
                         round_id,
                         free,
                         completed_at,
                         jp_pc_con_amt,
                         jp_pc_win_amt,
                         jp_jc_con_amt,
                         jp_jc_win_amt,
                         jp_win_id,
                         jp_win_lv,
                         jp_direct_pay,
                         prize_type,
                         prize_amt,
                         side_id,
                         report_time) FROM STDIN (FORMAT BINARY)";


        sql = sql.Replace("#TempTableName", tempTableName);
        try
        {
            await using var writer = await conn.BeginBinaryImportAsync(sql);
            foreach (var betInfo in record_data)
            {
                await writer.StartRowAsync();
                // 寫入每一列的資料，請根據你的數據庫結構和類型進行調整
                await writer.WriteAsync(betInfo.account_name, NpgsqlTypes.NpgsqlDbType.Varchar);
                await writer.WriteAsync(betInfo.currency, NpgsqlTypes.NpgsqlDbType.Varchar);
                await writer.WriteAsync(decimal.Parse(betInfo.bet_amt), NpgsqlTypes.NpgsqlDbType.Numeric);
                await writer.WriteAsync(decimal.Parse(betInfo.payout_amt), NpgsqlTypes.NpgsqlDbType.Numeric);
                await writer.WriteAsync(betInfo.bet_at, NpgsqlTypes.NpgsqlDbType.Timestamp);
                await writer.WriteAsync(decimal.Parse(betInfo.end_balance), NpgsqlTypes.NpgsqlDbType.Numeric);
                await writer.WriteAsync(decimal.Parse(betInfo.rebate_amt), NpgsqlTypes.NpgsqlDbType.Numeric);
                await writer.WriteAsync(betInfo.game_id, NpgsqlTypes.NpgsqlDbType.Integer);
                await writer.WriteAsync(betInfo.round_id, NpgsqlTypes.NpgsqlDbType.Bigint);
                await writer.WriteAsync(betInfo.free, NpgsqlTypes.NpgsqlDbType.Boolean);
                await writer.WriteAsync(betInfo.completed_at, NpgsqlTypes.NpgsqlDbType.Timestamp);
                await writer.WriteAsync(decimal.Parse(betInfo.jp_pc_con_amt), NpgsqlTypes.NpgsqlDbType.Numeric);
                await writer.WriteAsync(decimal.Parse(betInfo.jp_pc_win_amt), NpgsqlTypes.NpgsqlDbType.Numeric);
                await writer.WriteAsync(decimal.Parse(betInfo.jp_jc_con_amt), NpgsqlTypes.NpgsqlDbType.Numeric);
                await writer.WriteAsync(decimal.Parse(betInfo.jp_jc_win_amt), NpgsqlTypes.NpgsqlDbType.Numeric);
                await writer.WriteAsync(betInfo.jp_win_id, NpgsqlTypes.NpgsqlDbType.Varchar);
                await writer.WriteAsync(betInfo.jp_win_lv, NpgsqlTypes.NpgsqlDbType.Integer);
                await writer.WriteAsync(betInfo.jp_direct_pay, NpgsqlTypes.NpgsqlDbType.Boolean);
                await writer.WriteAsync(betInfo.prize_type, NpgsqlTypes.NpgsqlDbType.Varchar);
                await writer.WriteAsync(decimal.Parse(betInfo.prize_amt), NpgsqlTypes.NpgsqlDbType.Numeric);
                await writer.WriteAsync(betInfo.side_id, NpgsqlTypes.NpgsqlDbType.Integer);
                await writer.WriteAsync(betInfo.report_time, NpgsqlTypes.NpgsqlDbType.Timestamp);
            }

            // 完成寫入操作
            return await writer.CompleteAsync();
        }
        catch (Exception ex)
        {
            throw;
        }
    }

    /// <summary>
    /// 從TemapTable和主資料表做差集後，搬移資料回主注單資料表
    /// </summary>
    /// <param name="conn"></param>
    /// <param name="tran"></param>
    /// <param name="tempTableName"></param>
    /// <returns></returns>
    private async Task<int> MergenextspinRecordFromTempTable(NpgsqlConnection conn, IDbTransaction tran, string tempTableName)
    {
        var sql = @"
                    insert into t_ae_bet_record_v2 (
                    account_name,
                         currency,
                         bet_amt,
                         payout_amt,
                         bet_at,
                         end_balance,
                         rebate_amt,
                         game_id,
                         round_id,
                         free,
                         completed_at,
                         jp_pc_con_amt,
                         jp_pc_win_amt,
                         jp_jc_con_amt,
                         jp_jc_win_amt,
                         jp_win_id,
                         jp_win_lv,
                         jp_direct_pay,
                         prize_type,
                         prize_amt,
                         side_id,
                         report_time
                        )
                        select  
                      account_name,
                         currency,
                         bet_amt,
                         payout_amt,
                         bet_at,
                         end_balance,
                         rebate_amt,
                         game_id,
                         round_id,
                         free,
                         completed_at,
                         jp_pc_con_amt,
                         jp_pc_win_amt,
                         jp_jc_con_amt,
                         jp_jc_win_amt,
                         jp_win_id,
                         jp_win_lv,
                         jp_direct_pay,
                         prize_type,
                         prize_amt,
                         side_id,
                         report_time
                        from #TempTableName tempTable
                        where not exists (
                                select null from t_ae_bet_record_v2
		                        where completed_at = tempTable.completed_at 
		                        and  round_id = tempTable.round_id
	                    );
                    ";

        sql = sql.Replace("#TempTableName", tempTableName);
        try
        {
            var rows = await conn.ExecuteAsync(sql, tran, commandTimeout: 60);

            return rows;
        }
        catch (Exception ex)
        {
            throw;
        }
    }
    /// <summary>
    /// 刪除
    /// </summary>
    /// <param name="conn"></param>
    /// <param name="tran"></param>
    /// <param name="tempTableName"></param>
    /// <returns></returns>
    private async Task<int> RemovePostnextspinRecordTempTable(NpgsqlConnection conn, IDbTransaction tran, string tempTableName)
    {
        var sql = @"DROP TABLE IF EXISTS #TempTableName ;";

        sql = sql.Replace("#TempTableName", tempTableName);

        var rows = await conn.ExecuteAsync(sql, tran, commandTimeout: 60);

        return rows;
    }

    #endregion t_ae_bet_record_v2
}