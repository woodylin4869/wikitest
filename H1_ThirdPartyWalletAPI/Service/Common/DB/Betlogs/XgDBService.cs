using Dapper;
using H1_ThirdPartyWalletAPI.Model.Config;
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
using static H1_ThirdPartyWalletAPI.Model.Game.XG.Response.GetBetRecordByTimeResponse;

namespace H1_ThirdPartyWalletAPI.Service.Common.DB.Betlogs;

public interface IXgDBService
{
    Task<int> PostXgRecord(NpgsqlConnection conn, IDbTransaction tran, IEnumerable<Result> betLogs);
    Task<List<Result>> GetXgRecordsBytime(DateTime start, DateTime end);
    Task<List<Result>> GetXgRecords(IDbTransaction tran, string wagersId, DateTime wagersTime);
    Task<(int totalCount, decimal totalBetValid, decimal totalWin)> SumXgBetRecordByBetTime(DateTime start, DateTime end);
    Task<List<Result>> GetXgRecordsBySummary(GetBetRecordReq RecordReq);
}

public class XgDBService : BetlogsDBServiceBase, IXgDBService
{
    public XgDBService(ILogger<XgDBService> logger, IBetLogsDbConnectionStringManager connectionStringManager) : base(logger, connectionStringManager)
    {
    }

    /// <summary>
    /// 寫入資料
    /// </summary>
    /// <param name="conn"></param>
    /// <param name="tran"></param>
    /// <param name="betLogs"></param>
    /// <returns></returns>
    public async Task<int> PostXgRecord(NpgsqlConnection conn, IDbTransaction tran, IEnumerable<Result> betLogs)
    {
        var sql = @"INSERT INTO public.t_xg_bet_record
                    (
                    summary_id
                    ,Account
                    ,WagersId
                    ,GameType
                    ,Currency
                    ,BetAmount
                    ,validBetAmount
                    ,WagersTime
                    ,PayoffTime
                    ,SettlementTime
                    ,PayoffAmount
                    ,Commission
                    ,Status
                    ,GameMethod
                    ,TableId
                    ,Round
                    ,Run
                    ,GameResult
                    ,BetType
                    ,Transactions
                    ,pre_BetAmount
                    ,pre_validBetAmount
                    ,pre_PayoffAmount
                    ,pre_Status
                    )
                    VALUES
                    ( 
                      @summary_id
                     ,@Account
                     ,@WagersId
                     ,@GameType
                     ,@Currency
                     ,@BetAmount
                     ,@validBetAmount
                     ,@WagersTime
                     ,@PayoffTime
                     ,@SettlementTime
                     ,@PayoffAmount
                     ,@Commission
                     ,@Status
                     ,@GameMethod
                     ,@TableId
                     ,@Round
                     ,@Run
                     ,@GameResult
                     ,@BetType
                     ,@Transactions
                     ,@pre_BetAmount
                     ,@pre_validBetAmount
                     ,@pre_PayoffAmount
                     ,@pre_Status
                    )
                    ";

        return await conn.ExecuteAsync(sql, betLogs, tran);
    }

    /// <summary>
    /// 注單號取資料
    /// </summary>
    /// <param name="tran"></param>
    /// <param name="wagersId"></param>
    /// <param name="wagersTime"></param>
    /// <returns></returns>
    public async Task<List<Result>> GetXgRecords(IDbTransaction tran, string wagersId, DateTime wagersTime)
    {
        var sql = @"
                    SELECT * FROM t_xg_bet_record
                    WHERE WagersId = @WagersId and WagersTime >= @start and WagersTime < @end";

        var par = new DynamicParameters();
        par.Add("@WagersId", wagersId);
        par.Add("@start", wagersTime.AddHours(-1));
        par.Add("@end", wagersTime.AddHours(1));

        return (await tran.Connection.QueryAsync<Result>(sql, par, tran)).ToList();
    }

    /// <summary>
    /// 每小時匯總
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <returns></returns>
    public async Task<(int totalCount, decimal totalBetValid, decimal totalWin)> SumXgBetRecordByBetTime(DateTime start, DateTime end)
    {
        var sql = @"SELECT 
                    COUNT(1) AS totalCount
                    , CASE WHEN SUM(BetAmount) IS NULL THEN 0 ELSE SUM(BetAmount) END  AS totalBetValid
                    , CASE WHEN SUM(PayoffAmount) IS NULL THEN 0 ELSE SUM(PayoffAmount) END AS totalWin
                    FROM t_xg_bet_record
                    WHERE WagersTime >= @startTime 
                        AND WagersTime < @endTime";

        var par = new DynamicParameters();
        par.Add("@startTime", start);
        par.Add("@endTime", start.AddHours(1));

        await using var conn = new NpgsqlConnection(await PGRead);
        var result = (await conn.QueryAsync<dynamic>(sql, par)).FirstOrDefault();
        return ((int)result.totalcount, (decimal)result.totalbetvalid, (decimal)result.totalwin);
    }

    /// <summary>
    /// 取得時間內的注單
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <returns></returns>
    public async Task<List<Result>> GetXgRecordsBytime(DateTime start, DateTime end)
    {
        var sql = @"SELECT * 
                    FROM public.t_xg_bet_record 
                    WHERE WagersTime >= @startTime 
                        AND WagersTime < @endTime";

        var par = new DynamicParameters();
        par.Add("@startTime", start);
        par.Add("@endTime", end);

        await using var conn = new NpgsqlConnection(await PGRead);
        var result = await conn.QueryAsync<Result>(sql, par);
        return result.ToList();
    }

    /// <summary>
    /// 取得GUID資料
    /// </summary>
    /// <param name="RecordReq"></param>
    /// <returns></returns>
    public async Task<List<Result>> GetXgRecordsBySummary(GetBetRecordReq RecordReq)
    {
        var sql = @"SELECT * 
                    FROM public.t_xg_bet_record 
                    WHERE WagersTime >= @start 
                        AND WagersTime <= @end
                        AND summary_id = @summaryId::uuid";

        var param = new
        {
            summaryId = RecordReq.summary_id,
            start = RecordReq.ReportTime.AddDays(-3),
            end = RecordReq.ReportTime.AddDays(1),
        };

        await using var conn = new NpgsqlConnection(await PGRead);
        var result = await conn.QueryAsync<Result>(sql, param);
        return result.ToList();
    }
}