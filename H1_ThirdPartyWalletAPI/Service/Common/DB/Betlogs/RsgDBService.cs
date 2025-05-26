using Dapper;
using H1_ThirdPartyWalletAPI.Model.Config;
using H1_ThirdPartyWalletAPI.Model.DataModel;
using H1_ThirdPartyWalletAPI.Model.DB.RSG.Response;
using H1_ThirdPartyWalletAPI.Model.Game.RSG.Response;
using H1_ThirdPartyWalletAPI.Utility;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace H1_ThirdPartyWalletAPI.Service.Common.DB.Betlogs;

public interface IRsgDBService
{
    Task<GameDetail> GetRsgRecord(string betid, DateTime reporttime);

    Task<int> PostRsgRecord(NpgsqlConnection conn, IDbTransaction tran, List<GameDetail> record_data);

    Task<IEnumerable<GetRsgRecordByReportTimeResponse>> GetRsgRecordByReportTime(BetRecordSummary RecordReq);

    Task<IEnumerable<GetRsgSlotRecordDetailByTimeResponse>> GetRsgSlotRecordDetailByTime(DateTime startTime, DateTime endTime);

    Task<IEnumerable<GetRsgFishRecordDetailByTimeResponse>> GetRsgFishRecordDetailByTime(DateTime startTime, DateTime endTime);

    Task<IEnumerable<(int count, decimal win, decimal bet, decimal jackpot, string userid, int gameid)>> SummaryGameRecord(DateTime reportTime, DateTime startTime, DateTime endTime);

    Task<dynamic> SumRsgBetRecordMinutely(DateTime reportDate);
}

public class RsgDBService : BetlogsDBServiceBase, IRsgDBService
{
    public RsgDBService(ILogger<RsgDBService> logger, IBetLogsDbConnectionStringManager connectionStringManager) : base(logger, connectionStringManager)
    {
    }

    #region t_rsg_bet_record

    public async Task<GameDetail> GetRsgRecord(string betid, DateTime reporttime)
    {
        var sql = @"SELECT * FROM t_rsg_bet_record
                        WHERE sequenNumber = @sequenNumber
                        AND playtime BETWEEN @starttime AND @endtime";

        var parameters = new DynamicParameters();
        parameters.Add("@sequenNumber", long.Parse(betid));
        parameters.Add("@starttime", reporttime.AddDays(-3));
        parameters.Add("@endtime", reporttime.AddMinutes(5));

        using (var conn = new NpgsqlConnection(await PGRead))
        {
            return await conn.QuerySingleOrDefaultAsync<GameDetail>(sql, parameters);
        }
    }
    
    public async Task<IEnumerable<GetRsgRecordByReportTimeResponse>> GetRsgRecordByReportTime(BetRecordSummary RecordReq)
    {
        var sql = @"SELECT sequennumber, playtime, report_time, betamt, winamt, jackpotwin FROM t_rsg_bet_record
                        WHERE playtime BETWEEN @starttime AND @endtime
                        AND report_time = @reporttime
                        AND gameid = @gameid
                        AND userid = @userid";

        var parameters = new DynamicParameters();
        parameters.Add("@starttime", RecordReq.ReportDatetime.GetValueOrDefault().AddDays(-3));
        parameters.Add("@endtime", RecordReq.ReportDatetime.GetValueOrDefault().AddMinutes(5));
        parameters.Add("@reporttime", RecordReq.ReportDatetime);
        parameters.Add("@gameid", RecordReq.Game_type);
        parameters.Add("@userid", RecordReq.Club_id);

        using (var conn = new NpgsqlConnection(await PGRead))
        {
            return await conn.QueryAsync<GetRsgRecordByReportTimeResponse>(sql, parameters);
        }
    }

    public async Task<IEnumerable<(int count, decimal win, decimal bet, decimal jackpot, string userid, int gameid)>> SummaryGameRecord(DateTime reportTime, DateTime startTime, DateTime endTime)
    {
        var sql = @"SELECT CAST(count(1) AS INTEGER) AS count,
                        SUM(winamt) AS win,
                        SUM(betamt) AS bet,
                        SUM(jackpotwin) AS jackpot,
                        userid,
                        gameid
                        FROM t_rsg_bet_record
                        WHERE playtime BETWEEN @start_time AND @end_time
                        AND report_time = @report_time
                        GROUP BY userid, gameid
                        ";

        var par = new DynamicParameters();
        par.Add("@start_time", startTime);
        par.Add("@end_time", endTime);
        par.Add("@report_time", reportTime);
        using (NpgsqlConnection conn = new NpgsqlConnection(PGMaster))
        {
            var result = await conn.QueryAsync(sql, par, commandTimeout: 270);
            return result.Select(x => ((int)x.count, (decimal)x.win, (decimal)x.bet, (decimal)x.jackpot, (string)x.userid, (int)x.gameid));
        }
    }

    public async Task<IEnumerable<GetRsgSlotRecordDetailByTimeResponse>> GetRsgSlotRecordDetailByTime(DateTime startTime, DateTime endTime)
    {
        var par = new DynamicParameters();
        string strSql = @"SELECT sequennumber, playtime, report_time
                    FROM t_rsg_bet_record
                    WHERE playtime BETWEEN @startTime and @endTime
                    AND gameid BETWEEN 1 AND 2999
                    ";
        par.Add("@startTime", startTime);
        par.Add("@endTime", endTime.AddMinutes(1).AddMilliseconds(-3));
        using (NpgsqlConnection conn = new NpgsqlConnection(await PGRead))
        {
            return await conn.QueryAsync<GetRsgSlotRecordDetailByTimeResponse>(strSql, par);
        }
    }

    public async Task<IEnumerable<GetRsgFishRecordDetailByTimeResponse>> GetRsgFishRecordDetailByTime(DateTime startTime, DateTime endTime)
    {
        var par = new DynamicParameters();
        string strSql = @"SELECT sequennumber, playtime, report_time
                    FROM t_rsg_bet_record
                    WHERE playtime BETWEEN @startTime and @endTime
                    AND gameid BETWEEN 3000 AND 3999
                    ";
        par.Add("@startTime", startTime);
        par.Add("@endTime", endTime.AddMinutes(1).AddMilliseconds(-3));
        using (NpgsqlConnection conn = new NpgsqlConnection(await PGRead))
        {
            return await conn.QueryAsync<GetRsgFishRecordDetailByTimeResponse>(strSql, par);
        }
    }

    /// <summary>
    /// 查詢每5分鐘內的注單
    /// </summary>
    /// <param name="reportDate"></param>
    /// <returns></returns>
    public async Task<dynamic> SumRsgBetRecordMinutely(DateTime reportDate)
    {
        var par = new DynamicParameters();
        string strSql = @"SELECT
                    count(*) AS total_cont
                    , SUM(betamt) AS total_bet
                    , SUM(winamt) AS total_win
                    , SUM(jackpotwin) AS total_jackpot
                    FROM t_rsg_bet_record
                    WHERE playtime  >= @startTime
                    AND playtime < @endTime
                    ";

        par.Add("@startTime", reportDate);
        par.Add("@endTime", reportDate.AddMinutes(5));
        using (NpgsqlConnection conn = new NpgsqlConnection(await PGRead))
        {
            return await conn.QueryAsync(strSql, par);
        }
    }

    

    /// <summary>
    /// 移除暫存資料表
    /// </summary>
    /// <param name="conn"></param>
    /// <param name="tran"></param>
    /// <param name="funcScheduleDate"></param>
    /// <param name="pullBetRecordPhaseType"></param>
    /// <param name="scheduleGameType"></param>
    /// <returns></returns>
    private async Task<int> RemovePostRsgRecordTempTable(NpgsqlConnection conn, IDbTransaction tran, string tempTableName)
    {
        var sql = @"DROP TABLE IF EXISTS #TempTableName ;";

        sql = sql.Replace("#TempTableName", tempTableName);

        var rows = await conn.ExecuteAsync(sql, tran, commandTimeout: 60);

        return rows;
    }

    #endregion t_rsg_bet_record
    public async Task<int> PostRsgRecord(NpgsqlConnection conn, IDbTransaction tran, List<GameDetail> record_data)
    {
        // 建立暫存表
        var tempTableName = $"temp_t_rsg_bet_record_{Guid.NewGuid():N}";

        #region backup

        //var sql = @"INSERT INTO public.#TempTabelName
        //                (
        //                    currency,
        //                    webid,
        //                    userid,
        //                    sequennumber,
        //                    playtime,
        //                    gameid,
        //                    subgametype,
        //                    betamt,
        //                    winamt,
        //                    jackpotcontribution,
        //                    jackpotwin,
        //                    report_time
        //                )
        //             VALUES
        //                (
        //                    @currency,
        //                    @webid,
        //                    @userid,
        //                    @sequennumber,
        //                    @playtime,
        //                    @gameid,
        //                    @subgametype,
        //                    @betamt,
        //                    @winamt,
        //                    @jackpotcontribution,
        //                    @jackpotwin,
        //                    @report_time
        //                );";

        #endregion backup

        try
        {
            await CreateTempTable(conn, tran, tempTableName);
            await BulkInsertToRsgTempTable(conn, tran, tempTableName, record_data);
            return await MergeRsgRecordFromTempTable(conn, tran, tempTableName);
        }
        finally
        {
            await RemovePostRsgRecordTempTable(conn, tran, tempTableName);
        }
    }

    /// <summary>
    /// 建立暫存資料表
    /// </summary>
    /// <param name="conn"></param>
    /// <param name="tran"></param>
    /// <param name="funcScheduleDate"></param>
    /// <param name="pullBetRecordPhaseType"></param>
    /// <param name="scheduleGameType"></param>
    /// <returns></returns>
    private async Task<string> CreateTempTable(NpgsqlConnection conn, IDbTransaction tran, string tempTableName)
    {
        //var tempTableName = $"temp_t_rsg_bet_record_{Guid.NewGuid():N}";
        var sql = "CREATE TEMPORARY TABLE IF NOT EXISTS #TempTableName ( LIKE t_rsg_bet_record INCLUDING DEFAULTS INCLUDING CONSTRAINTS );";
        sql = sql.Replace("#TempTableName", tempTableName);
        // 建立temp資料表
        await conn.ExecuteAsync(sql, tran, commandTimeout: 60);

        // 建立唯一索引避免資料重複
        sql = $"CREATE UNIQUE index IF NOT EXISTS {tempTableName}_un ON {tempTableName} (sequennumber);";
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
    private async Task<ulong> BulkInsertToRsgTempTable(NpgsqlConnection conn, IDbTransaction tran, string tempTableName,
        List<GameDetail> record_data)
    {
        var sql = @"COPY #TempTableName
                    (
                        currency,
                        webid,
                        userid,
                        sequennumber,
                        playtime,
                        gameid,
                        subgametype,
                        betamt,
                        winamt,
                        jackpotcontribution,
                        jackpotwin,
                        report_time
                    )
                    FROM STDIN (FORMAT BINARY)";

        sql = sql.Replace("#TempTableName", tempTableName);

        await using var writer = await conn.BeginBinaryImportAsync(sql);
        foreach (var data in record_data)
        {
            await writer.StartRowAsync();
            await writer.WriteAsync(data.currency, NpgsqlTypes.NpgsqlDbType.Varchar);
            await writer.WriteAsync(data.webid, NpgsqlTypes.NpgsqlDbType.Varchar);
            await writer.WriteAsync(data.userid, NpgsqlTypes.NpgsqlDbType.Varchar);
            await writer.WriteAsync(data.sequennumber, NpgsqlTypes.NpgsqlDbType.Bigint);
            await writer.WriteAsync(data.playtime, NpgsqlTypes.NpgsqlDbType.Timestamp);
            await writer.WriteAsync(data.gameid, NpgsqlTypes.NpgsqlDbType.Integer);
            await writer.WriteAsync(data.subgametype, NpgsqlTypes.NpgsqlDbType.Integer);
            await writer.WriteAsync(data.betamt, NpgsqlTypes.NpgsqlDbType.Numeric);
            await writer.WriteAsync(data.winamt, NpgsqlTypes.NpgsqlDbType.Numeric);
            await writer.WriteAsync(data.jackpotcontribution, NpgsqlTypes.NpgsqlDbType.Numeric);
            await writer.WriteAsync(data.jackpotwin, NpgsqlTypes.NpgsqlDbType.Numeric);
            await writer.WriteAsync(data.report_time, NpgsqlTypes.NpgsqlDbType.Timestamp);
        }

        return await writer.CompleteAsync();
    }

    /// <summary>
    /// 從TemapTable和主資料表做差集後，搬移資料回主注單資料表
    /// </summary>
    /// <param name="conn"></param>
    /// <param name="tran"></param>
    /// <param name="funcScheduleDate"></param>
    /// <param name="pullBetRecordPhaseType"></param>
    /// <param name="scheduleGameType"></param>
    /// <param name="partitionIndex"></param>
    /// <returns></returns>
    private async Task<int> MergeRsgRecordFromTempTable(NpgsqlConnection conn, IDbTransaction tran, string tempTableName)
    {
        var sql = @"
                    insert into t_rsg_bet_record (
                            currency
	                        ,webid
	                        ,userid
	                        ,sequennumber
	                        ,playtime
	                        ,gameid
	                        ,subgametype
	                        ,betamt
	                        ,winamt
	                        ,jackpotcontribution
	                        ,jackpotwin
	                        ,report_time
	                        ,create_time
                        )
                        select currency
	                            ,webid
	                            ,userid
	                            ,sequennumber
	                            ,playtime
	                            ,gameid
	                            ,subgametype
	                            ,betamt
	                            ,winamt
	                            ,jackpotcontribution
	                            ,jackpotwin
	                            ,report_time
	                            ,create_time
                        from #TempTableName tempTable
                        where not exists (
                                select null from t_rsg_bet_record
		                        where playtime = tempTable.playtime
		                        and  sequennumber = tempTable.sequennumber
	                    );
                    ";

        sql = sql.Replace("#TempTableName", tempTableName);

        var rows = await conn.ExecuteAsync(sql, tran, commandTimeout: 60);

        return rows;
    }

    #region OldPostRsgRecord

    //public async Task<ulong> PostRsgRecord(NpgsqlConnection conn, IDbTransaction tran, List<GameDetail> record_data)
    //{
    //    var sql = @"COPY public.t_rsg_bet_record (
    //                        currency,
    //                        webid,
    //                        userid,
    //                        sequennumber,
    //                        playtime,
    //                        gameid,
    //                        subgametype,
    //                        betamt,
    //                        winamt,
    //                        jackpotcontribution,
    //                        jackpotwin,
    //                        report_time
    //                    )
    //                FROM STDIN (FORMAT BINARY)";

    //    using var writer = await conn.BeginBinaryImportAsync(sql);
    //    foreach (var record in record_data)
    //    {
    //        await writer.StartRowAsync();
    //        await writer.WriteAsync(record.currency, NpgsqlTypes.NpgsqlDbType.Varchar);
    //        await writer.WriteAsync(record.webid, NpgsqlTypes.NpgsqlDbType.Varchar);
    //        await writer.WriteAsync(record.userid, NpgsqlTypes.NpgsqlDbType.Varchar);
    //        await writer.WriteAsync(record.sequennumber, NpgsqlTypes.NpgsqlDbType.Bigint);
    //        await writer.WriteAsync(record.playtime, NpgsqlTypes.NpgsqlDbType.Timestamp);
    //        await writer.WriteAsync(record.gameid, NpgsqlTypes.NpgsqlDbType.Integer);
    //        await writer.WriteAsync(record.subgametype, NpgsqlTypes.NpgsqlDbType.Integer);
    //        await writer.WriteAsync(record.betamt, NpgsqlTypes.NpgsqlDbType.Numeric);
    //        await writer.WriteAsync(record.winamt, NpgsqlTypes.NpgsqlDbType.Numeric);
    //        await writer.WriteAsync(record.jackpotcontribution, NpgsqlTypes.NpgsqlDbType.Numeric);
    //        await writer.WriteAsync(record.jackpotwin, NpgsqlTypes.NpgsqlDbType.Numeric);
    //        await writer.WriteAsync(record.report_time, NpgsqlTypes.NpgsqlDbType.Timestamp);
    //    }

    //    var rows = await writer.CompleteAsync();

    //    return rows;
    //}

    #endregion
}