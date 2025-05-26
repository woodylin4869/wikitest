using Dapper;
using H1_ThirdPartyWalletAPI.Model.Config;
using H1_ThirdPartyWalletAPI.Model.DataModel;
using H1_ThirdPartyWalletAPI.Model.Game.SEXY.Response;
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

namespace H1_ThirdPartyWalletAPI.Service.Common.DB.Betlogs;

public interface ISEXYDBService
{
    Task<int> PostSEXYRecord(NpgsqlConnection conn, IDbTransaction tran, IEnumerable<Record> betLogs);
    Task<List<Record>> GetsexyRecordsBytime(DateTime start, DateTime end);
    //Task<List<Record>> GetsexyRecords(long platformTxId);
    Task<(int totalCount, decimal totalBetValid, decimal totalWin)> SumsexyBetRecordByBetTime(DateTime start, DateTime end);
    Task<List<Record>> GetsexyRecordsBySummary(GetBetRecordReq RecordReq);


    Task<List<Record>> GetsexyRecord(IDbTransaction tran, string platformTxId, DateTime bettime);
    Task<dynamic> GetsexyRecord(string platformTxId, DateTime bettime);

    public Task<DateTime> GetsexyLastupdatetime();
}

public class SEXYDBService : BetlogsDBServiceBase, ISEXYDBService
{
    public SEXYDBService(ILogger<SEXYDBService> logger, IBetLogsDbConnectionStringManager connectionStringManager) : base(logger, connectionStringManager)
    {
    }
    /// <summary>
    /// 寫入資料
    /// </summary>
    /// <returns></returns>
    public async Task<int> PostSEXYRecord(NpgsqlConnection conn, IDbTransaction tran, IEnumerable<Record> betLogs)
    {
        var sql = @"INSERT INTO public.t_sexy_bet_record
                        (summary_id, platformtxid, gametype, winamount, txtime, settlestatus, gameinfo, realwinamount, updatetime, realbetamount, userid, bettype, platform, txstatus, betamount, gamename, bettime, gamecode, currency, jackpotwinamount, jackpotbetamount, turnover, roundid, pre_betAmount,pre_realWinAmount,pre_turnover,pre_realBetAmount)
                    VALUES(@summary_id,@platformtxid,@gametype,@winamount,@txtime,@settlestatus,@gameinfo,@realwinamount,@updatetime,@realbetamount,@userid,@bettype,@platform,@txstatus,@betamount,@gamename,@bettime,@gamecode,@currency,@jackpotwinamount,@jackpotbetamount,@turnover,@roundid,@pre_betAmount,@pre_realWinAmount,@pre_turnover,@pre_realBetAmount);";

        return await conn.ExecuteAsync(sql, betLogs, tran);
    }

    ///// <summary>
    ///// 注單號取資料
    ///// </summary>
    ///// <param name="platformTxId"></param>
    ///// <returns></returns>
    //public async Task<List<Record>> GetsexyRecords(long platformTxId)
    //{
    //    var sql = @"
    //            SELECT * FROM t_sexy_bet_record
    //            WHERE platformTxId = @platformTxId";

    //    var par = new DynamicParameters();
    //    par.Add("@platformTxId", platformTxId);


    //    await using var conn = new NpgsqlConnection(await PGRead);
    //    var result = await conn.QueryAsync<Record>(sql, par);
    //    return result.ToList();
    //}

    /// <summary>
    /// 每小時匯總
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <returns></returns>
    public async Task<(int totalCount, decimal totalBetValid, decimal totalWin)> SumsexyBetRecordByBetTime(DateTime start, DateTime end)
    {
        var sql = @"SELECT 
                    COUNT(platformTxId) AS totalCount
                    , CASE WHEN SUM(betamount) IS NULL THEN 0 ELSE SUM(betamount) END  AS totalBetValid
                    , CASE WHEN SUM(realWinAmount) IS NULL THEN 0 ELSE SUM(realWinAmount) END AS totalWin
                    FROM t_sexy_bet_record
                    WHERE bettime >= @startTime 
                        AND bettime < @endTime";

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
    public async Task<List<Record>> GetsexyRecordsBytime(DateTime start, DateTime end)
    {
        var sql = @"SELECT * 
                    FROM public.t_sexy_bet_record 
                    WHERE bettime >= @startTime 
                        AND bettime < @endTime";
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
    public async Task<List<Record>> GetsexyRecordsBySummary(GetBetRecordReq RecordReq)
    {
        var sql = @"SELECT * 
                    FROM public.t_sexy_bet_record 
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
        var result = await conn.QueryAsync<Record>(sql, param);
        return result.ToList();
    }


    /// <summary>
    /// 使用platformTxId住單號取得資料
    /// </summary>
    /// <returns></returns>
    public async Task<List<Record>> GetsexyRecord(IDbTransaction tran, string platformTxId, DateTime bettime)
    {
        var sql = @"
                    SELECT * FROM t_sexy_bet_record
                    WHERE bettime >= @startTime 
                        AND bettime <= @endTime
                        and platformTxId = @platformTxId";

        var par = new DynamicParameters();
        par.Add("@startTime", bettime.AddMinutes(-1));
        par.Add("@endTime", bettime.AddMinutes(1));
        par.Add("@platformTxId", platformTxId);

        var result = await tran.Connection.QueryAsync<Record>(sql, par, tran);
        return result.ToList();
    }

    /// <summary>
    /// 使用platformTxId住單號取得資料
    /// </summary>
    /// <param name="platformTxId"></param>
    /// <param name="bettime"></param>
    /// <returns></returns>
    public async Task<dynamic> GetsexyRecord(string platformTxId, DateTime bettime)
    {
        string strSql = @"SELECT *
                    FROM t_sexy_bet_record
                    WHERE bettime >= @start 
                        AND bettime <= @end and platformTxId = @platformTxId
                    Limit 1";
        var param = new
        {
            platformTxId = platformTxId,
            start = bettime.AddDays(-3),
            end = bettime.AddDays(1),
        };

        using (var conn = new NpgsqlConnection(await PGRead))
        {
            return await conn.QuerySingleOrDefaultAsync<dynamic>(strSql, param);
        }
    }

    public async Task<DateTime> GetsexyLastupdatetime()
    {
        try
        {
            DateTime lastupdatetime = DateTime.Now.AddDays(-1);
            using (NpgsqlConnection conn = new NpgsqlConnection(await PGRead))
            {
                string strSql = @"
                                SELECT MAX(updatetime)
                                FROM t_sexy_bet_record;";
                var results = await conn.QueryMultipleAsync(strSql);
                while (!results.IsConsumed)
                {
                    List<dynamic> result = results.Read().ToList();
                    foreach (dynamic r in result)
                    {
                        if (r.max > lastupdatetime)
                        {
                            lastupdatetime = r.max;
                        }
                    }
                }
            }
            return lastupdatetime;
        }
        catch (Exception ex)
        {
            var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
            var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
            _logger.LogError("SEXY GetsexyLastupdatetime exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
            return DateTime.Now.AddDays(-1);
        }
    }
}
