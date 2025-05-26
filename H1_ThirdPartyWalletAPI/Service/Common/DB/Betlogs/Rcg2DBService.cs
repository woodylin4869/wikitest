using Dapper;
using H1_ThirdPartyWalletAPI.Model.DB.RCG2.DBResponse;
using H1_ThirdPartyWalletAPI.Model.Game.RCG2.Response;
using H1_ThirdPartyWalletAPI.Model.W1API;
using H1_ThirdPartyWalletAPI.Utility;
using Microsoft.Extensions.Logging;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace H1_ThirdPartyWalletAPI.Service.Common.DB.Betlogs;

public interface IRcg2DBService
{
    public Task<int> PostRcg2Record(NpgsqlConnection conn, IDbTransaction tran, List<RCG2BetRecord> record_data);
    public Task<(int totalCount, decimal totalBetValid, decimal totalWin)> SumRCG2BetRecordByReportDT(DateTime start, DateTime end);

    public Task<List<RCG2PreRecordDBResponse>> GetRcg2PreRecordById(IDbTransaction tran, long id, DateTime reportdt);
    public Task<List<RCG2RealIDDBResponse>> GetRcg2RealIDByReportDT(DateTime startTime, DateTime endTime);
    public Task<List<RCG2RecordPrimaryKey>> GetRcg2PrimaryKeyByReportDT(DateTime startTime, DateTime endTime);
    public Task<List<RCG2PreRecordDBResponse>> GetRcg2PreRecordById_old(IDbTransaction tran, long id, DateTime reportdt);
    Task<List<RCG2RecordsBySummaryDBResponse>> GetRcg2RecordsBySummary_Old(GetBetRecordReq RecordReq);
    Task<List<RCG2RecordsBySummaryDBResponse>> GetRcg2RecordsBySummary(DateTime partition_time, DateTime report_time, int Game_id, string memberaccount);
    Task<IEnumerable<(int count, decimal netwin, decimal bet, string userid, int game_type, DateTime partitionTime)>> SummaryGameRecord(DateTime reportTime, DateTime startTime, DateTime endTime);
}

public class Rcg2DBService : BetlogsDBServiceBase, IRcg2DBService
{
    public Rcg2DBService(ILogger<Rcg2DBService> logger, IBetLogsDbConnectionStringManager connectionStringManager) : base(logger, connectionStringManager)
    {
    }
    // 預設小時
    private const int _defaultHour = 1;

    // 預設天數
    private const int _defaultDay = 2;

    /// <summary>
    /// 寫入資料
    /// </summary>
    /// <param name="conn"></param>
    /// <param name="tran"></param>
    /// <param name="betLogs"></param>
    /// <returns></returns>
    public async Task<int> PostRcg2Record(NpgsqlConnection conn, IDbTransaction tran, List<RCG2BetRecord> betLogs)
    {
        // 建立暫存表
        var tempTableName = $"temp_t_rcg2_bet_record_v2_{Guid.NewGuid():N}";
        try
        {
            await CreateTempTable(conn, tran, tempTableName);
            await BulkInsertToRCGTempTable(conn, tran, tempTableName, betLogs);
            return await MergeRCGRecordFromTempTable(conn, tran, tempTableName);
        }
        catch (Exception ex)
        {

            return 0;
        }
        finally
        {
            await RemovePostRCGRecordTempTable(conn, tran, tempTableName);
        }

    }

    /// <summary>
    /// 建立暫存資料表
    /// </summary>
    /// <param name="conn"></param>
    /// <param name="tran"></param>
    /// <param name="tempTableName"></param>
    /// <returns></returns>
    private async Task<string> CreateTempTable(NpgsqlConnection conn, IDbTransaction tran, string tempTableName)
    {
        //var tempTableName = $"temp_t_rcg2_bet_record_v2_{Guid.NewGuid():N}";
        var sql = "CREATE TEMPORARY TABLE IF NOT EXISTS #TempTableName ( LIKE t_rcg2_bet_record_v2 INCLUDING ALL);";
        sql = sql.Replace("#TempTableName", tempTableName);
        // 建立temp資料表
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
    private async Task<ulong> BulkInsertToRCGTempTable(NpgsqlConnection conn, IDbTransaction tran, string tempTableName,
        List<RCG2BetRecord> record_data)
    {
        // 定義COPY命令，使用二進制格式
        string sql = @"COPY #TempTableName (
        systemcode,
        webid,
        memberaccount,
        id,
        gameid,
        desk,
        betarea,
        bet,
        available,
        winlose,
        waterrate,
        activeno,
        runno,
        balance,
        datetime,
        reportdt,
        ip,
        status,
        odds,
        originRecordId,
        rootRecordId,
        pre_bet,
        pre_available,
        pre_winlose,
        pre_status,
        real_id,
        create_time,
        report_time,
        partition_time
    ) FROM STDIN (FORMAT BINARY);";
        sql = sql.Replace("#TempTableName", tempTableName);

        await using var writer = await conn.BeginBinaryImportAsync(sql);
        // 開始一個COPY操作
        foreach (var record in record_data)
        {
            await writer.StartRowAsync();

            // 判斷每個欄位是否為NULL，並寫入對應值
            await WriteColumnAsync(writer, record.systemCode);
            await WriteColumnAsync(writer, record.webId);
            await WriteColumnAsync(writer, record.memberAccount);
            await WriteColumnAsync(writer, record.id);
            await WriteColumnAsync(writer, record.gameId);
            await WriteColumnAsync(writer, record.desk);
            await WriteColumnAsync(writer, record.betArea);
            await WriteColumnAsync(writer, record.bet);
            await WriteColumnAsync(writer, record.available);
            await WriteColumnAsync(writer, record.winLose);
            await WriteColumnAsync(writer, record.waterRate);
            await WriteColumnAsync(writer, record.activeNo);
            await WriteColumnAsync(writer, record.runNo);
            await WriteColumnAsync(writer, record.balance);
            await WriteColumnAsync(writer, record.dateTime);
            await WriteColumnAsync(writer, record.reportDT);
            await WriteColumnAsync(writer, record.ip);
            await WriteColumnAsync(writer, record.status);
            await WriteColumnAsync(writer, record.odds);
            await WriteColumnAsync(writer, record.originRecordId);
            await WriteColumnAsync(writer, record.rootRecordId);
            await WriteColumnAsync(writer, record.pre_bet);
            await WriteColumnAsync(writer, record.pre_available);
            await WriteColumnAsync(writer, record.pre_winlose);
            await WriteColumnAsync(writer, record.pre_status);
            await WriteColumnAsync(writer, record.real_id);
            await WriteColumnAsync(writer, record.Create_time);
            await WriteColumnAsync(writer, record.Report_time);
            await WriteColumnAsync(writer, record.Partition_time);
        }

        return await writer.CompleteAsync(); // 完成批量導入
    }
    private async Task WriteColumnAsync(NpgsqlBinaryImporter writer, object value)
    {
        if (value == null)
        {
            await writer.WriteNullAsync();
        }
        else
        {
            await writer.WriteAsync(value);
        }
    }
    /// <summary>
    /// 從TemapTable和主資料表做差集後，搬移資料回主注單資料表
    /// </summary>
    /// <param name="conn"></param>
    /// <param name="tran"></param>
    /// <param name="tempTableName"></param>
    /// <returns></returns>
    private async Task<int> MergeRCGRecordFromTempTable(NpgsqlConnection conn, IDbTransaction tran, string tempTableName)
    {
        var sql = @"insert into t_rcg2_bet_record_v2 (
                            systemcode,
                            webid,
                            memberaccount,
                            id,
                            gameid,
                            desk,
                            betarea,
                            bet,
                            available,
                            winlose,
                            waterrate,
                            activeno,
                            runno,
                            balance,
                            datetime,
                            reportdt,
                            ip,
                            status,
                            odds,
                            originRecordId,
                            rootRecordId,
                            pre_bet,
                            pre_available,
                            pre_winlose,
                            pre_status,
                            real_id,
                            create_time,
                            report_time,
                            partition_time
                        )
                        select systemcode,
                               webid,
                               memberaccount,
                               id,
                               gameid,
                               desk,
                               betarea,
                               bet,
                               available,
                               winlose,
                               waterrate,
                               activeno,
                               runno,
                               balance,
                               datetime,
                               reportdt,
                               ip,
                               status,
                               odds,
                               originRecordId,
                               rootRecordId,
                               pre_bet,
                               pre_available,
                               pre_winlose,
                               pre_status,
                               real_id,
                               create_time,
                               report_time,
                               partition_time
                        from #TempTableName tempTable
                        where not exists (
                                select null from t_rcg2_bet_record_v2
		                        where partition_time = tempTable.partition_time
		                        and  originrecordid = tempTable.originrecordid
                                and  rootrecordid = tempTable.rootrecordid
                                and  id = tempTable.id
	                    );
                    ";

        sql = sql.Replace("#TempTableName", tempTableName);

        var rows = await conn.ExecuteAsync(sql, tran, commandTimeout: 60);

        return rows;
    }


    /// <summary>
    /// 移除暫存資料表
    /// </summary>
    /// <param name="conn"></param>
    /// <param name="tran"></param>
    /// <param name="tempTableName"></param>
    /// <returns></returns>
    private async Task<int> RemovePostRCGRecordTempTable(NpgsqlConnection conn, IDbTransaction tran, string tempTableName)
    {
        var sql = @"DROP TABLE IF EXISTS #TempTableName ;";

        sql = sql.Replace("#TempTableName", tempTableName);

        var rows = await conn.ExecuteAsync(sql, tran, commandTimeout: 60);

        return rows;
    }

    /// <summary>
    /// 取明細的小時匯總結果
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <returns></returns>
    public async Task<(int totalCount, decimal totalBetValid, decimal totalWin)> SumRCG2BetRecordByReportDT(DateTime start, DateTime end)
    {
        var sql = @"SELECT 
                        COUNT(1) AS totalCount
                        , CASE WHEN SUM(available) IS NULL THEN 0 ELSE SUM(available) END  AS totalBetValid
                        , CASE WHEN SUM(winlose) IS NULL THEN 0 ELSE SUM(winlose) END AS totalWin
                        FROM t_rcg2_bet_record_v2
                        WHERE partition_time >= @startTime 
                            AND partition_time < @endTime;";

        var par = new DynamicParameters();
        par.Add("@startTime", start);
        par.Add("@endTime", start.AddHours(_defaultHour));

        await using var conn = new NpgsqlConnection(await PGRead);
        var result = (await conn.QueryAsync<dynamic>(sql, par)).FirstOrDefault();
        return ((int)result.totalcount, (decimal)result.totalbetvalid, (decimal)result.totalwin);
    }

    /// <summary>
    /// 往下查詢該層匯總的明細
    /// </summary>
    public async Task<List<RCG2RecordsBySummaryDBResponse>> GetRcg2RecordsBySummary(DateTime partition_time, DateTime report_time, int Game_id, string memberaccount)
    {
        var sql = @"SELECT 
                         id, originrecordid, rootrecordid, reportdt
                         , gameid
                         , bet
                         , available
                         , winlose
                         , datetime
                         , real_id
                         , desk
                        FROM public.t_rcg2_bet_record_v2 
                        WHERE partition_time >= @start 
                            AND partition_time < @end
                            AND report_time = @report_time
                            AND gameid = @gameid
                            AND memberaccount = @memberaccount;";
        var param = new
        {
            start = partition_time,
            end = partition_time.AddDays(1).AddMilliseconds(-1),
            report_time = report_time,
            gameid = Game_id,
            memberaccount = memberaccount
        };

        await using var conn = new NpgsqlConnection(await PGRead);
        var result = await conn.QueryAsync<RCG2RecordsBySummaryDBResponse>(sql, param);
        return result.ToList();
    }

    /// <summary>
    /// 往下查詢該層匯總的明細
    /// </summary>
    /// <param name="RecordReq"></param>
    /// <returns></returns>
    public async Task<List<RCG2RecordsBySummaryDBResponse>> GetRcg2RecordsBySummary_Old(GetBetRecordReq RecordReq)
    {
        var sql = @"SELECT 
                         id, originrecordid, rootrecordid, reportdt
                         , gameid
                         , bet
                         , available
                         , winlose
                         , datetime
                         , real_id
                         , desk
                        FROM public.t_rcg2_bet_record 
                        WHERE reportdt >= @start 
                            AND reportdt < @end
                            AND summary_id = @summaryId::uuid;";
        var param = new
        {
            summaryId = RecordReq.summary_id,
            start = RecordReq.ReportTime,
            end = RecordReq.ReportTime.AddDays(1),
        };

        await using var conn = new NpgsqlConnection(await PGRead);
        var result = await conn.QueryAsync<RCG2RecordsBySummaryDBResponse>(sql, param);
        return result.ToList();
    }

    /// <summary>
    /// 取原始下注、輸贏紀錄 by id
    /// </summary>
    /// <param name="tran"></param>
    /// <param name="id"></param>
    /// <param name="reportdt"></param>
    /// <returns></returns>
    public async Task<List<RCG2PreRecordDBResponse>> GetRcg2PreRecordById(IDbTransaction tran, long id, DateTime reportdt)
    {
        string strSql = @"SELECT 
                             id, originrecordid, rootrecordid, reportdt
                             , pre_bet
                             , pre_available
                             , pre_winlose                           
                             , pre_status
                             , real_id
                            FROM t_rcg2_bet_record_v2
                            WHERE partition_time >= @startTime
                                AND partition_time <= @endTime
                                AND id = @id;";
        var param = new
        {
            id = id,
            startTime = reportdt.AddDays(-_defaultDay), // todo 多久內改單??
            endTime = reportdt.AddDays(_defaultDay),
        };

        return (await tran.Connection.QueryAsync<RCG2PreRecordDBResponse>(strSql, param, tran)).ToList();
    }

    /// <summary>
    /// 取原始下注、輸贏紀錄 by id
    /// </summary>
    /// <param name="tran"></param>
    /// <param name="id"></param>
    /// <param name="reportdt"></param>
    /// <returns></returns>
    public async Task<List<RCG2PreRecordDBResponse>> GetRcg2PreRecordById_old(IDbTransaction tran, long id, DateTime reportdt)
    {
        string strSql = @"SELECT 
                             id, originrecordid, rootrecordid, reportdt
                             , pre_bet
                             , pre_available
                             , pre_winlose                           
                             , pre_status
                             , real_id
                            FROM t_rcg2_bet_record
                            WHERE reportdt >= @startTime
                                AND reportdt <= @endTime
                                AND id = @id;";
        var param = new
        {
            id = id,
            startTime = reportdt.AddDays(-_defaultDay), // todo 多久內改單??
            endTime = reportdt.AddDays(_defaultDay),
        };

        return (await tran.Connection.QueryAsync<RCG2PreRecordDBResponse>(strSql, param, tran)).ToList();
    }

    /// <summary>
    /// 根據時間區間取得注單 real_id
    /// </summary>
    /// <returns></returns>
    public async Task<List<RCG2RealIDDBResponse>> GetRcg2RealIDByReportDT(DateTime startTime, DateTime endTime)
    {
        string strSql = @"SELECT 
                             id, originrecordid, rootrecordid, reportdt
                             , real_id
                            FROM t_rcg2_bet_record_v2
                            WHERE partition_time >= @startTime
                                AND partition_time <= @endTime;";
        var par = new DynamicParameters();
        par.Add("@startTime", startTime);
        par.Add("@endTime", endTime);

        using (NpgsqlConnection conn = new(await PGRead))
        {
            var result = await conn.QueryAsync<RCG2RealIDDBResponse>(strSql, par);
            return result.ToList();
        }
    }

    /// <summary>
    /// 根據時間區間取得注單 PK
    /// </summary>
    /// <returns></returns>
    public async Task<List<RCG2RecordPrimaryKey>> GetRcg2PrimaryKeyByReportDT(DateTime startTime, DateTime endTime)
    {
        string strSql = @"SELECT 
                             id, originrecordid, rootrecordid, reportdt
                            FROM t_rcg2_bet_record_v2
                            WHERE partition_time >= @startTime
                                AND partition_time <= @endTime;";
        var par = new DynamicParameters();
        par.Add("@startTime", startTime);
        par.Add("@endTime", endTime);

        using (NpgsqlConnection conn = new(await PGRead))
        {
            var result = await conn.QueryAsync<RCG2RecordPrimaryKey>(strSql, par);
            return result.ToList();
        }
    }

    /// <summary>
    /// 五分鐘會總
    /// </summary>
    /// <param name="reportTime"></param>
    /// <param name="startTime"></param>
    /// <param name="endTime"></param>
    /// <returns></returns>
    public async Task<IEnumerable<(int count, decimal netwin, decimal bet, string userid, int game_type, DateTime partitionTime)>> SummaryGameRecord(DateTime reportTime, DateTime startTime, DateTime endTime)
    {
        var sql = @"SELECT CAST(count(1) AS INTEGER) AS count,
                        coalesce(SUM(winlose),0) AS netwin,
                        coalesce(SUM(bet),0) AS bet,
                        memberaccount as userid,
                        gameid as game_type,
                        Date(partition_time) as partition_time
                        FROM t_rcg2_bet_record_v2
                        WHERE partition_time BETWEEN @start_time AND @end_time
                        AND report_time = @report_time
                        GROUP BY memberaccount,Date(partition_time),gameid
                        ";

        var par = new DynamicParameters();
        par.Add("@start_time", startTime);
        par.Add("@end_time", endTime);
        par.Add("@report_time", reportTime);
        using (NpgsqlConnection conn = new NpgsqlConnection(await PGRead))
        {
            var result = await conn.QueryAsync(sql, par, commandTimeout: 270);
            var a = result.Select(x => ((int)x.count, (decimal)x.netwin, (decimal)x.bet, (string)x.userid, (int)x.game_type, (DateTime)x.partition_time)).ToList();
            return a;
        }
    }
}