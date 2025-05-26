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
using H1_ThirdPartyWalletAPI.Model.Game.META.Response;
using H1_ThirdPartyWalletAPI.Utility;


namespace H1_ThirdPartyWalletAPI.Service.Common.DB.Betlogs;

public interface IMETADBService
{
    Task<int> PostmetaRecord(NpgsqlConnection conn, IDbTransaction tran, IEnumerable<Record> betLogs);
    Task<List<Record>> GetmetaRecordsBytime(DateTime start, DateTime end);
    Task<List<Record>> GetmetaRecords(long serial);
    Task<(int totalCount, decimal totalBetValid, decimal totalWin)> SummetaBetRecordByBetTime(DateTime start, DateTime end);
    Task<List<Record>> GetmetaRecordsBySummary(GetBetRecordReq RecordReq);

    Task<dynamic> GetmetaRecord(string serial);

    public Task<long> GetmetaLastSerial();
}

public class METADBService : BetlogsDBServiceBase, IMETADBService
{
    public METADBService(ILogger<METADBService> logger, IBetLogsDbConnectionStringManager connectionStringManager) : base(logger, connectionStringManager)
    {
    }

    /// <summary>
    /// 寫入資料
    /// </summary>
    /// <param name="conn"></param>
    /// <param name="tran"></param>
    /// <param name="betLogs"></param>
    /// <returns></returns>
    public async Task<int> PostmetaRecord(NpgsqlConnection conn, IDbTransaction tran, IEnumerable<Record> betLogs)
    {
        var sql = @"INSERT INTO public.t_meta_bet_record
                    (summary_id, serial, ""no"", round, betcount, account, bettotal, winnings, ""collect"", status, datecurrent, dateclosing, datedraw, datecreate, gametype, game, ""table"", tableid, currency, rate)
                    VALUES
                    ( @summary_id,@serial,@no,@round,@betcount,@account,@bettotal,@winnings,@collect,@status,@datecurrent,@dateclosing,@datedraw,@datecreate,@gametype,@game,@table,@tableid,@currency,@rate)
                    ";

        return await conn.ExecuteAsync(sql, betLogs, tran);
    }

    /// <summary>
    /// 注單號取資料
    /// </summary>
    /// <param name="serial"></param>
    /// <returns></returns>
    public async Task<List<Record>> GetmetaRecords(long serial)
    {
        var sql = @"
                    SELECT * FROM t_meta_bet_record
                    WHERE serial = @serial";

        var par = new DynamicParameters();
        par.Add("@serial", serial);


        await using var conn = new NpgsqlConnection(await PGRead);
        var result = await conn.QueryAsync<Record>(sql, par);
        return result.ToList();
    }

    /// <summary>
    /// 每小時匯總
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <returns></returns>
    public async Task<(int totalCount, decimal totalBetValid, decimal totalWin)> SummetaBetRecordByBetTime(DateTime start, DateTime end)
    {
        var sql = @"SELECT 
                    COUNT(serial) AS totalCount
                    , CASE WHEN SUM(bettotal) IS NULL THEN 0 ELSE SUM(bettotal) END  AS totalBetValid
                    , CASE WHEN SUM(winnings) IS NULL THEN 0 ELSE SUM(winnings) END AS totalWin
                    FROM t_meta_bet_record
                    WHERE datecreate >= @startTime 
                        AND datecreate < @endTime";

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
    public async Task<List<Record>> GetmetaRecordsBytime(DateTime start, DateTime end)
    {
        var sql = @"SELECT * 
                    FROM public.t_meta_bet_record 
                    WHERE datecreate >= @startTime 
                        AND datecreate < @endTime";
        var par = new DynamicParameters();
        par.Add("@startTime", start);
        par.Add("@endTime", end);

        await using var conn = new NpgsqlConnection(await PGRead);
        var result = await conn.QueryAsync<Record>(sql, par);
        return result.ToList();
    }

    /// <summary>
    /// 取得GUID資料
    /// </summary>
    /// <param name="RecordReq"></param>
    /// <returns></returns>
    public async Task<List<Record>> GetmetaRecordsBySummary(GetBetRecordReq RecordReq)
    {
        var sql = @"SELECT * 
                    FROM public.t_meta_bet_record 
                    WHERE datecreate >= @start 
                        AND datecreate <= @end
                        AND summary_id = @summaryId::uuid";
        var param = new
        {
            summaryId = RecordReq.summary_id,
            start = RecordReq.ReportTime.AddDays(-3),
            end = RecordReq.ReportTime.AddDays(1),
        };

        await using var conn = new NpgsqlConnection(await PGRead);
        var result = await conn.QueryAsync<Record>(sql, param);
        return result.ToList();
    }
    /// <summary>
    /// 使用serial住單號取得資料
    /// </summary>
    /// <param name="serial"></param>
    /// <returns></returns>
    public async Task<dynamic> GetmetaRecord(string serial)
    {
        string strSql = @"SELECT *
                    FROM t_meta_bet_record
                    WHERE serial = @serial
                    Limit 1";
        var parameters = new DynamicParameters();
        parameters.Add("@serial", Int64.Parse(serial));

        using (var conn = new NpgsqlConnection(await PGRead))
        {
            return await conn.QuerySingleOrDefaultAsync<dynamic>(strSql, parameters);
        }
    }

    public async Task<long> GetmetaLastSerial()
    {
        try
        {
            long lastSerial = 0;
            using (NpgsqlConnection conn = new NpgsqlConnection(await PGRead))
            {
                string strSql = @"
                                SELECT MAX(serial)
                                FROM t_meta_bet_record;";
                var results = await conn.QueryMultipleAsync(strSql);
                while (!results.IsConsumed)
                {
                    List<dynamic> result = results.Read().ToList();
                    foreach (dynamic r in result)
                    {
                        if (r.max > lastSerial)
                        {
                            lastSerial = r.max;
                        }
                    }
                }
            }
            return lastSerial;
        }
        catch (Exception ex)
        {
            var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
            var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
            _logger.LogError("META GetmetaLastSerial exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
            return 0;
        }
    }

}