using Dapper;
using H1_ThirdPartyWalletAPI.Model.Config;
using H1_ThirdPartyWalletAPI.Model.DataModel;
using H1_ThirdPartyWalletAPI.Model.DB.JDB.Response;
using H1_ThirdPartyWalletAPI.Model.Game.JDB.Response;
using H1_ThirdPartyWalletAPI.Model.Game.NEXTSPIN.Response;
using H1_ThirdPartyWalletAPI.Model.W1API;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using H1_ThirdPartyWalletAPI.Model.DB.JILI;
using H1_ThirdPartyWalletAPI.Model.DB.JILI.Response;
using H1_ThirdPartyWalletAPI.Model.Game.JILI.Responses;
using H1_ThirdPartyWalletAPI.Utility;
using static H1_ThirdPartyWalletAPI.Model.Game.NEXTSPIN.Response.GetBetHistoryResponse;

namespace H1_ThirdPartyWalletAPI.Service.Common.DB.Betlogs;

public interface IJILIDBService
{
    Task<int> PostjiliRecord(NpgsqlConnection conn, IDbTransaction tran, IEnumerable<GetBetRecordByTimeResponse.Result> betLogs);
    Task<List<JILIRecordPrimaryKey>> GetjiliRecordsBytime(DateTime start, DateTime end);

    Task<(int totalCount, decimal totalBetValid, decimal totalWin)> SumjiliBetRecordByBetTime(DateTime start, DateTime end);
    Task<List<GetJILIRecordsResponse>> GetjiliRecordsBySummary(string summaryId, DateTime start, DateTime end);
    Task<List<GetJILIRecordsResponse>> GetjiliRecordsByreporttime(string club_id, DateTime report_time, DateTime start, DateTime end);

    Task<IEnumerable<(int count, decimal win, decimal bet, decimal jackpot, string userid, int game_type, DateTime createtime)>> SummaryGameRecord(DateTime reportTime, DateTime startTime, DateTime endTime);
}

public class JILIDBService : BetlogsDBServiceBase, IJILIDBService
{
    public JILIDBService(ILogger<JILIDBService> logger, IBetLogsDbConnectionStringManager connectionStringManager) : base(logger, connectionStringManager)
    {
    }

    /// <summary>
    /// 寫入資料
    /// </summary>
    /// <param name="conn"></param>
    /// <param name="tran"></param>
    /// <param name="betLogs"></param>
    /// <returns></returns>
    public async Task<int> PostjiliRecord(NpgsqlConnection conn, IDbTransaction tran, IEnumerable<GetBetRecordByTimeResponse.Result> betLogs)
    {

        var tempTableName = $"t_jili_bet_record_v2_{Guid.NewGuid():N}";
        try
        {
            await CreateTempTable(conn, tran, tempTableName);
            await BulkInsertTonextspinTempTable(conn, tran, tempTableName, betLogs);
            return await MergenextspinRecordFromTempTable(conn, tran, tempTableName);
        }
        finally
        {
            await RemovePostnextspinRecordTempTable(conn, tran, tempTableName);
        }
        //var sql = @"INSERT INTO public.t_jili_bet_record_v2
        //            (account
        //            ,wagersid
        //            ,gameid
        //            ,wagerstime
        //            ,betamount
        //            ,payofftime
        //            ,payoffamount
        //            ,status
        //            ,settlementtime
        //            ,gamecategoryid
        //            ,versionkey
        //            ,type
        //            ,agentid
        //            ,turnover
        //            ,report_time)
        //            VALUES
        //            ( @account
        //             ,@wagersid
        //             ,@gameid
        //             ,@wagerstime
        //             ,@betamount
        //             ,@payofftime
        //             ,@payoffamount
        //             ,@status
        //             ,@settlementtime
        //             ,@gamecategoryid
        //             ,@versionkey
        //             ,@type
        //             ,@agentid
        //             ,@turnover
        //             ,@report_time)
        //            ";

        //return await conn.ExecuteAsync(sql, betLogs, tran);
    }

    /// <summary>
    /// 每小時匯總
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <returns></returns>
    public async Task<(int totalCount, decimal totalBetValid, decimal totalWin)> SumjiliBetRecordByBetTime(DateTime start, DateTime end)
    {
        var sql = @"SELECT 
                    COUNT(wagersid) AS totalCount
                    , CASE WHEN SUM(turnover) IS NULL THEN 0 ELSE SUM(turnover) END  AS totalBetValid
                    , CASE WHEN SUM(payoffamount) IS NULL THEN 0 ELSE SUM(payoffamount) END AS totalWin
                    FROM t_jili_bet_record_v2
                    WHERE wagerstime >= @startTime 
                        AND wagerstime < @endTime";

        var par = new DynamicParameters();
        par.Add("@startTime", start);
        par.Add("@endTime", start.AddHours(1));

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
    public async Task<List<JILIRecordPrimaryKey>> GetjiliRecordsBytime(DateTime start, DateTime end)
    {
        var sql = @"SELECT wagersId, wagersTime,account
                    FROM public.t_jili_bet_record_v2 
                    WHERE wagerstime >= @startTime 
                        AND wagerstime < @endTime";
        var par = new DynamicParameters();
        par.Add("@startTime", start);
        par.Add("@endTime", end);

        await using var conn = new NpgsqlConnection(await PGRead);
        var result = await conn.QueryAsync<JILIRecordPrimaryKey>(sql, par);
        return result.ToList();
    }
    /// <summary>
    /// 取得GUID資料
    /// </summary>
    /// <param name="RecordReq"></param>
    /// <returns></returns>
    public async Task<List<GetJILIRecordsResponse>> GetjiliRecordsBySummary(string summaryId, DateTime start, DateTime end)
    {
        var sql = @"SELECT wagersId, wagersTime,gameId,betAmount,payoffAmount,account,turnover,payofftime,settlementtime
                    FROM public.t_jili_bet_record 
                    WHERE wagerstime >= @start 
                        AND wagerstime < @end
                        AND summary_id = @summaryId::uuid";
        var param = new
        {
            summaryId,
            start,
            end,
        };

        await using var conn = new NpgsqlConnection(await PGRead);
        var result = await conn.QueryAsync<GetJILIRecordsResponse>(sql, param);
        return result.ToList();
    }
    public async Task<List<GetJILIRecordsResponse>> GetjiliRecordsByreporttime(string club_id, DateTime report_time, DateTime start, DateTime end)
    {
        var sql = @"SELECT wagersId, wagersTime,gameId,betAmount,payoffAmount,account,turnover,payofftime,settlementtime
                    FROM public.t_jili_bet_record_v2 
                    WHERE wagerstime >= @start 
                        AND wagerstime < @end
                        AND report_time = @report_time
                        AND account=@club_id";
        var param = new
        {
            report_time,
            start,
            end,
            club_id
        };

        await using var conn = new NpgsqlConnection(await PGRead);
        var result = await conn.QueryAsync<GetJILIRecordsResponse>(sql, param);
        return result.ToList();
    }
    /// <summary>
    /// 五分鐘會總
    /// </summary>
    /// <param name="reportTime"></param>
    /// <param name="startTime"></param>
    /// <param name="endTime"></param>
    /// <returns></returns>
    public async Task<IEnumerable<(int count, decimal win, decimal bet, decimal jackpot, string userid, int game_type, DateTime createtime)>> SummaryGameRecord(DateTime reportTime, DateTime startTime, DateTime endTime)
    {
        var sql = @"SELECT CAST(count(1) AS INTEGER) AS count,
                        SUM(payoffAmount) AS win,
                        SUM(betAmount) AS bet,
                        0 as jackpot,
                        account as userid, 
                        3 as game_type,
                        Date(wagersTime) as createtime
                        FROM t_jili_bet_record_v2
                        WHERE wagersTime BETWEEN @start_time AND @end_time
                        AND report_time = @report_time
                        GROUP BY account,Date(wagersTime)
                        ";

        var par = new DynamicParameters();
        par.Add("@start_time", startTime.AddDays(-2));
        par.Add("@end_time", endTime);
        par.Add("@report_time", reportTime);
        using (NpgsqlConnection conn = new NpgsqlConnection(await PGRead))
        {
            var result = await conn.QueryAsync(sql, par, commandTimeout: 270);
            var a = result.Select(x => ((int)x.count, (decimal)x.win, (decimal)x.bet, (decimal)x.jackpot, (string)x.userid, (int)x.game_type, (DateTime)x.createtime)).ToList();
            return a;
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
        var sql = "CREATE TEMPORARY TABLE IF NOT EXISTS #TempTableName ( LIKE t_jili_bet_record_v2 INCLUDING ALL);";
        sql = sql.Replace("#TempTableName", tempTableName);
        // 建立temp資料表
        await conn.ExecuteAsync(sql, tran, commandTimeout: 60);

        // 建立唯一索引避免資料重複
        sql = $"CREATE UNIQUE index IF NOT EXISTS {tempTableName}_un ON {tempTableName} (wagersid);";
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
    IEnumerable<GetBetRecordByTimeResponse.Result> record_data)
    {
        var sql = @"COPY #TempTableName (
                         account
                         ,wagersid
                         ,gameid
                         ,wagerstime
                         ,betamount
                         ,payofftime
                         ,payoffamount
                         ,status
                         ,settlementtime
                         ,gamecategoryid
                         ,versionkey
                         ,type
                         ,agentid
                         ,turnover
                         ,report_time) FROM STDIN (FORMAT BINARY)";


        sql = sql.Replace("#TempTableName", tempTableName);
        try
        {
            await using var writer = await conn.BeginBinaryImportAsync(sql);
            foreach (var betInfo in record_data)
            {
                await writer.StartRowAsync();
                // 寫入每一列的資料，請根據你的數據庫結構和類型進行調整
                await writer.WriteAsync(betInfo.Account, NpgsqlTypes.NpgsqlDbType.Varchar);
                await writer.WriteAsync(betInfo.WagersId, NpgsqlTypes.NpgsqlDbType.Bigint);
                await writer.WriteAsync(betInfo.GameId, NpgsqlTypes.NpgsqlDbType.Smallint);
                await writer.WriteAsync(betInfo.WagersTime, NpgsqlTypes.NpgsqlDbType.Timestamp);
                await writer.WriteAsync(betInfo.BetAmount, NpgsqlTypes.NpgsqlDbType.Numeric);
                await writer.WriteAsync(betInfo.PayoffTime, NpgsqlTypes.NpgsqlDbType.Timestamp);
                await writer.WriteAsync(betInfo.PayoffAmount, NpgsqlTypes.NpgsqlDbType.Numeric);
                await writer.WriteAsync(betInfo.Status, NpgsqlTypes.NpgsqlDbType.Smallint);
                await writer.WriteAsync(betInfo.SettlementTime, NpgsqlTypes.NpgsqlDbType.Timestamp);
                await writer.WriteAsync(betInfo.GameCategoryId, NpgsqlTypes.NpgsqlDbType.Smallint);
                await writer.WriteAsync(betInfo.VersionKey, NpgsqlTypes.NpgsqlDbType.Smallint);
                await writer.WriteAsync(betInfo.Type, NpgsqlTypes.NpgsqlDbType.Smallint);
                await writer.WriteAsync(betInfo.AgentId, NpgsqlTypes.NpgsqlDbType.Varchar);
                await writer.WriteAsync(betInfo.Turnover, NpgsqlTypes.NpgsqlDbType.Numeric);
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
                    insert into t_jili_bet_record_v2 (
                    account
                   ,wagersid
                   ,gameid
                   ,wagerstime
                   ,betamount
                   ,payofftime
                   ,payoffamount
                   ,status
                   ,settlementtime
                   ,gamecategoryid
                   ,versionkey
                   ,type
                   ,agentid
                   ,turnover
                   ,report_time
                        )
                        select        
                     account
                   ,wagersid
                   ,gameid
                   ,wagerstime
                   ,betamount
                   ,payofftime
                   ,payoffamount
                   ,status
                   ,settlementtime
                   ,gamecategoryid
                   ,versionkey
                   ,type
                   ,agentid
                   ,turnover
                   ,report_time
                        from #TempTableName tempTable
                        where not exists (
                                select null from t_jili_bet_record_v2
		                        where wagerstime = tempTable.wagerstime 
		                        and  wagersid = tempTable.wagersid
                                and account = tempTable.account
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
}