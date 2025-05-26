using Dapper;
using H1_ThirdPartyWalletAPI.Model.Config;
using H1_ThirdPartyWalletAPI.Model.DataModel;
using H1_ThirdPartyWalletAPI.Model.DB.DS.Response;
using H1_ThirdPartyWalletAPI.Model.DB.JDB.Response;
using H1_ThirdPartyWalletAPI.Model.W1API;
using H1_ThirdPartyWalletAPI.Utility;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using static H1_ThirdPartyWalletAPI.Model.Game.NEXTSPIN.Response.GetBetHistoryResponse;

namespace H1_ThirdPartyWalletAPI.Service.Common.DB.Betlogs;

public interface IDsDBService
{
    Task<int> PostDsRecord(NpgsqlConnection conn, IDbTransaction tran, List<Model.Game.DS.Response.DSBetRecord> record_data);

    Task<IEnumerable<dynamic>> GetDsRecordBySummary(GetBetRecordReq RecordReq);

    Task<IEnumerable<dynamic>> GetDsRecord(string id, DateTime ReportTime);

    Task<int> PostDsReport(t_ds_game_report report_data);

    Task<int> DeleteDsReport(t_ds_game_report report_data);

    Task<dynamic> SumDsBetRecordHourly(DateTime reportDate);

    Task<List<t_ds_bet_record>> SumDsBetRecord(DateTime startDateTime, DateTime endDateTime);

    Task<List<t_ds_bet_record>> SumDsBetRecordV2(DateTime startDateTime, DateTime endDateTime);

    Task<IEnumerable<(int count, decimal win, decimal bet, string userid, decimal bet_valid, decimal fee_amount, DateTime createtime)>> SummaryGameRecord(DateTime reportTime, DateTime startTime, DateTime endTime);

    /// <summary>
    /// 取得第二層明細
    /// </summary>
    /// <param name="RecordReq"></param>
    /// <returns>參考GetDsRecordBySummary的Resp物件</returns>
    Task<IEnumerable<dynamic>> GetDsRecordByReportTimeV2(DateTime partitiontime, DateTime report_time, string playerid);

    /// <summary>
    /// 依照BetRecordSummary、遊戲序號找到明細資料
    /// </summary>
    /// <param name="RecordReq"></param>
    /// <param name="id"></param>
    /// <returns>參考GetDsRecord() 的Resp物件</returns>
    Task<IEnumerable<GetDSRecordResponse>> GetDsRecordByReportTime(DateTime reportTime, string id);
    /// <summary>
    /// 查詢舊資料
    /// </summary>
    /// <param name="reportTime"></param>
    /// <param name="id"></param>
    /// <returns></returns>
    Task<IEnumerable<GetDSRecordResponse>> GetDsRecordByReportTimeOld(DateTime reportTime, string id);
}

public class DsDBService : BetlogsDBServiceBase, IDsDBService
{
    public DsDBService(ILogger<DsDBService> logger, IBetLogsDbConnectionStringManager connectionStringManager) : base(logger, connectionStringManager)
    {
    }

    #region t_ds_bet_record

    public async Task<int> PostDsRecord(NpgsqlConnection conn, IDbTransaction tran, List<Model.Game.DS.Response.DSBetRecord> record_data)
    {
        var tempTableName = $"t_ds_bet_record_v2_{Guid.NewGuid():N}";
        try
        {
            await CreateTempTable(conn, tran, tempTableName);
            await BulkInsertTonextspinTempTable(conn, tran, tempTableName, record_data);
            return await MergenextspinRecordFromTempTable(conn, tran, tempTableName);
        }
        finally
        {
            await RemovePostnextspinRecordTempTable(conn, tran, tempTableName);
        }
        #region OLD SQL
        // TODO:後匯總ds更名
        //string stSqlInsert = @"INSERT INTO t_ds_bet_record_v2
        //    (
        //        id,
        //        bet_at,
        //        finish_at,
        //        agent,
        //        member,
        //        game_id,
        //        game_serial,
        //        game_type,
        //        round_id,
        //        bet_amount,
        //        payout_amount,
        //        valid_amount,
        //        status,
        //        fee_amount,
        //        jp_amount,
        //        report_time
        //    )
        //    VALUES
        //    (
        //        @id,
        //        @bet_at,
        //        @finish_at,
        //        @agent,
        //        @member,
        //        @game_id,
        //        @game_serial,
        //        @game_type,
        //        @round_id,
        //        @bet_amount,
        //        @payout_amount,
        //        @valid_amount,
        //        @status,
        //        @fee_amount,
        //        @jp_amount,
        //        @report_time
        //    )";
        //return await conn.ExecuteAsync(stSqlInsert, record_data, tran);
        #endregion
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
        var sql = "CREATE TEMPORARY TABLE IF NOT EXISTS #TempTableName ( LIKE t_ds_bet_record_v2 INCLUDING DEFAULTS INCLUDING CONSTRAINTS );";
        sql = sql.Replace("#TempTableName", tempTableName);
        // 建立temp資料表
        await conn.ExecuteAsync(sql, tran, commandTimeout: 60);

        // 建立唯一索引避免資料重複
        sql = $"CREATE UNIQUE index IF NOT EXISTS {tempTableName}_un ON {tempTableName} (id);";
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
    List<Model.Game.DS.Response.DSBetRecord> record_data)
    {
        var sql = @"COPY #TempTableName (
                    id,
                    bet_at,
                    finish_at,
                    agent,
                    member,
                    game_id,
                    game_serial,
                    game_type,
                    round_id,
                    bet_amount,
                    payout_amount,
                    valid_amount,
                    status,
                    fee_amount,
                    jp_amount,
                    report_time,
                    partition_time
                        ) FROM STDIN (FORMAT BINARY)";


        sql = sql.Replace("#TempTableName", tempTableName);
        try
        {
            await using var writer = await conn.BeginBinaryImportAsync(sql);
            foreach (var betInfo in record_data)
            {
                await writer.StartRowAsync();
                // 寫入每一列的資料，請根據你的數據庫結構和類型進行調整
                await writer.WriteAsync(betInfo.id, NpgsqlTypes.NpgsqlDbType.Varchar);
                await writer.WriteAsync(new DateTime(betInfo.bet_at.ToLocalTime().Ticks, DateTimeKind.Unspecified), NpgsqlTypes.NpgsqlDbType.Timestamp);
                await writer.WriteAsync(new DateTime(betInfo.finish_at.ToLocalTime().Ticks, DateTimeKind.Unspecified), NpgsqlTypes.NpgsqlDbType.Timestamp);
                await writer.WriteAsync(betInfo.agent, NpgsqlTypes.NpgsqlDbType.Varchar);
                await writer.WriteAsync(betInfo.member, NpgsqlTypes.NpgsqlDbType.Varchar);
                await writer.WriteAsync(betInfo.game_id, NpgsqlTypes.NpgsqlDbType.Varchar);
                await writer.WriteAsync(betInfo.game_serial, NpgsqlTypes.NpgsqlDbType.Varchar);
                await writer.WriteAsync(betInfo.game_type, NpgsqlTypes.NpgsqlDbType.Integer);
                await writer.WriteAsync(betInfo.round_id, NpgsqlTypes.NpgsqlDbType.Varchar);
                await writer.WriteAsync(betInfo.bet_amount, NpgsqlTypes.NpgsqlDbType.Numeric);
                await writer.WriteAsync(betInfo.payout_amount, NpgsqlTypes.NpgsqlDbType.Numeric);
                await writer.WriteAsync(betInfo.valid_amount, NpgsqlTypes.NpgsqlDbType.Numeric);
                await writer.WriteAsync(betInfo.status, NpgsqlTypes.NpgsqlDbType.Integer);
                await writer.WriteAsync(betInfo.fee_amount, NpgsqlTypes.NpgsqlDbType.Numeric);
                await writer.WriteAsync(betInfo.jp_amount, NpgsqlTypes.NpgsqlDbType.Numeric);
                await writer.WriteAsync(betInfo.report_time, NpgsqlTypes.NpgsqlDbType.Timestamp);
                await writer.WriteAsync(new DateTime(betInfo.finish_at.ToLocalTime().Ticks, DateTimeKind.Unspecified), NpgsqlTypes.NpgsqlDbType.Timestamp);

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
                    insert into t_ds_bet_record_v2 (
                    id,
                    bet_at,
                    finish_at,
                    agent,
                    member,
                    game_id,
                    game_serial,
                    game_type,
                    round_id,
                    bet_amount,
                    payout_amount,
                    valid_amount,
                    status,
                    fee_amount,
                    jp_amount,
                    report_time,
                    partition_time
                        )
                    select
                    id,
                    bet_at,
                    finish_at,
                    agent,
                    member,
                    game_id,
                    game_serial,
                    game_type,
                    round_id,
                    bet_amount,
                    payout_amount,
                    valid_amount,
                    status,
                    fee_amount,
                    jp_amount,
                    report_time,
                    partition_time
                        from #TempTableName tempTable
                        where not exists (
                                select null from t_ds_bet_record_v2
		                        where partition_time = tempTable.partition_time
		                        and  id = tempTable.id
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

    public async Task<IEnumerable<dynamic>> GetDsRecordBySummary(GetBetRecordReq RecordReq)
    {
        var par = new DynamicParameters();
        string strSql = @"SELECT
                    id,
                    game_id,
                    bet_at,
                    finish_at,
                    member,
                    bet_amount,
                    payout_amount,
                    valid_amount,
                    fee_amount,
                    jp_amount
                    FROM t_ds_bet_record
                    WHERE summary_id = @summary_id
                    AND finish_at >= @start_date
                    AND finish_at < @end_date
                    ";
        par.Add("@summary_id", Guid.Parse(RecordReq.summary_id));
        par.Add("@start_date", RecordReq.ReportTime.AddDays(-3));
        par.Add("@end_date", RecordReq.ReportTime.AddDays(1));

        using (NpgsqlConnection conn = new NpgsqlConnection(await PGRead))
        {
            return await conn.QueryAsync<dynamic>(strSql, par);
        }
    }

    public async Task<IEnumerable<dynamic>> GetDsRecord(string id, DateTime ReportTime)
    {
        var par = new DynamicParameters();
        string strSql = @"SELECT
                    id,
                    game_id,
                    game_serial,
                    bet_at,
                    finish_at,
                    member

                    FROM t_ds_bet_record_v2
                    WHERE id = @id
                    AND partition_time > @start_date
                    AND partition_time < @end_date
                    ";
        par.Add("@id", id);
        par.Add("@start_date", ReportTime.AddDays(-3));
        par.Add("@end_date", ReportTime.AddDays(1));
        using (NpgsqlConnection conn = new NpgsqlConnection(await PGRead))
        {
            return await conn.QueryAsync<dynamic>(strSql, par);
        }
    }

    public async Task<dynamic> SumDsBetRecordHourly(DateTime reportDate)
    {
        var par = new DynamicParameters();
        string strSql = @"SELECT
                    COUNT(*) AS total_cont
                    , SUM(coalesce(bet_amount,0)) AS total_bet
                    , SUM(coalesce(payout_amount,0)) AS total_win
                    FROM t_ds_bet_record_v2
                    WHERE partition_time >= @startTime
                    AND partition_time < @endTime
                    ";

        par.Add("@startTime", reportDate);
        par.Add("@endTime", reportDate.AddHours(1));
        using (NpgsqlConnection conn = new NpgsqlConnection(await PGRead))
        {
            return await conn.QueryAsync(strSql, par);
        }
    }

    public async Task<List<t_ds_bet_record>> SumDsBetRecord(DateTime startDateTime, DateTime endDateTime)
    {
        var sql = @"
                    SELECT 
                          id,
                          agent,
                          bet_amount,
                          payout_amount,
                          fee_amount,
                          status
                          FROM t_ds_bet_record
                    WHERE finish_at >= @startTime
                    AND finish_at < @endTime
                    AND status = 1 ";

        var par = new DynamicParameters();
        par.Add("@startTime", startDateTime);
        par.Add("@endTime", endDateTime);

        using (var conn = new NpgsqlConnection(await PGRead))
        {
            var result = await conn.QueryAsync<t_ds_bet_record>(sql, par);
            return result.ToList();
        }
    }

    public async Task<List<t_ds_bet_record>> SumDsBetRecordV2(DateTime startDateTime, DateTime endDateTime)
    {
        var sql = @"
                    SELECT
                          id,
                          agent,
                          bet_amount,
                          payout_amount,
                          fee_amount,
                          status 
                          FROM t_ds_bet_record_v2
                    WHERE partition_time >= @startTime
                    AND partition_time < @endTime
                    AND status = 1 ";

        var par = new DynamicParameters();
        par.Add("@startTime", startDateTime);
        par.Add("@endTime", endDateTime);

        using (var conn = new NpgsqlConnection(await PGRead))
        {
            var result = await conn.QueryAsync<t_ds_bet_record>(sql, par);
            return result.ToList();
        }
    }

    /// <summary>
    /// 取得遊戲彙總資料
    /// 後彙總使用
    /// </summary>
    /// <param name="reportTime"></param>
    /// <param name="startTime"></param>
    /// <param name="endTime"></param>
    /// <returns></returns>
    public async Task<IEnumerable<(int count, decimal win, decimal bet, string userid, decimal bet_valid, decimal fee_amount, DateTime createtime)>> SummaryGameRecord(DateTime reportTime, DateTime startTime, DateTime endTime)
    {
        var sql = @"SELECT CAST(count(1) AS INTEGER) AS count,
                        SUM(coalesce(payout_amount,0)) AS win,
                        SUM(coalesce(bet_amount,0)) AS bet,
                        SUM(coalesce(valid_amount,0)) AS bet_valid,
                        SUM(coalesce(fee_amount,0)) AS fee_amount,
                        member AS userid,
                        Date(partition_time) AS createtime
                        FROM t_ds_bet_record_v2
                        WHERE partition_time BETWEEN @start_time AND @end_time
                        AND report_time = @report_time
                        GROUP BY member, Date(partition_time)
                        ";

        var par = new DynamicParameters();
        par.Add("@start_time", startTime);
        par.Add("@end_time", endTime);
        par.Add("@report_time", reportTime);
        using (NpgsqlConnection conn = new NpgsqlConnection(await PGRead))
        {
            var result = await conn.QueryAsync(sql, par, commandTimeout: 270);
            return result.Select(x => ((int)x.count, (decimal)x.win, (decimal)x.bet, (string)x.userid, (decimal)x.bet_valid, (decimal)x.fee_amount, (DateTime)x.createtime));
        }
    }

    /// <summary>
    /// 取得第二層明細
    /// </summary>
    /// <param name="RecordReq"></param>
    /// <returns></returns>
    public async Task<IEnumerable<dynamic>> GetDsRecordByReportTimeV2(DateTime partitiontime , DateTime report_time, string playerid)
    {
        var sql = @"SELECT
                    id,
                    game_id,
                    bet_at,
                    finish_at,
                    member,
                    bet_amount,
                    payout_amount,
                    valid_amount,
                    fee_amount,
                    jp_amount
                    FROM t_ds_bet_record_v2
                    WHERE partition_time BETWEEN @starttime AND @endtime
                    AND report_time = @reporttime
                    AND member = @member";

        var parameters = new DynamicParameters();
        parameters.Add("@starttime", partitiontime);
        parameters.Add("@endtime", partitiontime.AddDays(1).AddMilliseconds(-1));
        parameters.Add("@reporttime", report_time);
        parameters.Add("@member", playerid);

        using (var conn = new NpgsqlConnection(await PGRead))
        {
            return await conn.QueryAsync<dynamic>(sql, parameters);
        }
    }

    /// <summary>
    /// 依照BetRecordSummary、遊戲序號找到明細資料
    /// </summary>
    /// <param name="RecordReq"></param>
    /// <returns></returns>
    public async Task<IEnumerable<GetDSRecordResponse>>GetDsRecordByReportTime(DateTime reportTime, string id)
    {
        var sql = @"SELECT
                    id,
                    game_id,
                    game_serial,
                    bet_at,
                    finish_at,
                    member
                    FROM t_ds_bet_record_v2
                    WHERE partition_time BETWEEN @starttime AND @endtime
                    AND id = @id";

        var parameters = new DynamicParameters();
        parameters.Add("@starttime", reportTime.AddDays(-3));
        parameters.Add("@endtime", reportTime.AddDays(1));
        parameters.Add("@id", id);

        using (var conn = new NpgsqlConnection(await PGRead))
        {
            return await conn.QueryAsync<GetDSRecordResponse>(sql, parameters);
        }
    }
    /// <summary>
    /// 第三層舊資料跳轉
    /// </summary>
    /// <param name="reportTime"></param>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<IEnumerable<GetDSRecordResponse>> GetDsRecordByReportTimeOld(DateTime reportTime, string id)
    {
        var sql = @"SELECT
                    id,
                    game_id,
                    game_serial,
                    bet_at,
                    finish_at,
                    member
                    FROM t_ds_bet_record
                    WHERE finish_at BETWEEN @starttime AND @endtime
                    AND id = @id";

        var parameters = new DynamicParameters();
        parameters.Add("@starttime", reportTime.AddDays(-3));
        parameters.Add("@endtime", reportTime.AddDays(1));
        parameters.Add("@id", id);

        using (var conn = new NpgsqlConnection(await PGRead))
        {
            return await conn.QueryAsync<GetDSRecordResponse>(sql, parameters);
        }
    }

    #endregion t_ds_bet_record

    #region t_ds_game_report

    public async Task<int> PostDsReport(t_ds_game_report report_data)
    {
        await using (var conn = new NpgsqlConnection(PGMaster))
        {
            string stSqlInsert = @"INSERT INTO t_ds_game_report
                (
	                agent,
	                bet_count,
	                bet_amount,
	                payout_amount,
	                valid_amount,
	                fee_amount,
	                jp_amount,
	                create_datetime
                )
                VALUES
                (
	                @agent,
	                CAST(@bet_count AS INTEGER ),
	                @bet_amount,
	                @payout_amount,
	                @valid_amount,
	                @fee_amount,
	                @jp_amount,
	                @create_datetime
                )";
            return await conn.ExecuteAsync(stSqlInsert, report_data);
        }
    }

    public async Task<int> DeleteDsReport(t_ds_game_report report_data)
    {
        await using (var conn = new NpgsqlConnection(PGMaster))
        {
            string strSqlDel = @"DELETE FROM t_ds_game_report
                               WHERE create_datetime=@create_datetime ";
            return await conn.ExecuteAsync(strSqlDel, report_data);
        }
    }

    #endregion t_ds_game_report
}