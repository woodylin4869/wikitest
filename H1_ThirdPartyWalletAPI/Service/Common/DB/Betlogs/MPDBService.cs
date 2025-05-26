using Dapper;
using H1_ThirdPartyWalletAPI.Model.Config;
using H1_ThirdPartyWalletAPI.Model.Game.MP.Response;
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

public interface IMPDBService
{
    Task<int> PostMPRecord(NpgsqlConnection conn, IDbTransaction tran, IEnumerable<MPData> betLogs);
    Task<List<MPData>> GetMPRecordsBytime(DateTime start, DateTime end);
    Task<List<MPData>> GetMPRecords(string id, DateTime time);
    Task<(int totalCount, decimal totalBetValid, decimal totalWin)> SumMPBetRecordByBetTime(DateTime start, DateTime end);

    Task<List<MPData>> GetMPRecordsBySummary(GetBetRecordReq RecordReq);
}

public class MPDBService : BetlogsDBServiceBase, IMPDBService
{
    public MPDBService(ILogger<MPDBService> logger, IBetLogsDbConnectionStringManager connectionStringManager) : base(logger, connectionStringManager)
    {
    }
    /// <summary>
    /// 寫入資料
    /// </summary>
    /// <param name="conn"></param>
    /// <param name="tran"></param>
    /// <param name="betLogs"></param>
    /// <returns></returns>
    public async Task<int> PostMPRecord(NpgsqlConnection conn, IDbTransaction tran, IEnumerable<MPData> betLogs)
    {

        var sql = @"INSERT INTO public.t_mp_bet_record
                    ( gameid
                     ,accounts
                     ,serverid
                     ,kindid
                     ,tableid
                     ,chairid
                     ,usercount
                     ,cellscore
                     ,allbet
                     ,profit
                     ,revenue
                     ,newscore
                     ,gamestarttime
                     ,gameendtime
                     ,cardvalue
                     ,channelid
                     ,linecode
                     ,summary_id)
                    VALUES
                    ( 
                     @gameid
                    ,@accounts
                    ,@serverid
                    ,@kindid
                    ,@tableid
                    ,@chairid
                    ,@usercount
                    ,to_number(@cellscore,'9G999g999.99')
                    ,to_number(@allbet,'9G999g999.99')
                    ,to_number(@profit,'9G999g999.99')
                    ,to_number(@revenue,'9G999g999.99')
                    ,to_number(@newscore,'9G999g999.99')
                    ,@gamestarttime
                    ,@gameendtime
                    ,@cardvalue
                    ,@channelid
                    ,@linecode
                    ,@summary_id)";

        return await conn.ExecuteAsync(sql, betLogs, tran);

    }

    /// <summary>
    /// 注單號取資料
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<List<MPData>> GetMPRecords(string id, DateTime time)
    {
        var sql = @"
                    SELECT gameid,accounts,kindid,cellscore,allbet,profit,gamestarttime,gameendtime
                   FROM t_mp_bet_record
                    WHERE gameid = @id
                        and gamestarttime >= @startTime 
                        AND gamestarttime < @endTime";

        var par = new DynamicParameters();
        par.Add("@id", id);
        par.Add("@startTime", time.AddDays(-3));
        par.Add("@endTime", time.AddDays(1));

        await using var conn = new NpgsqlConnection(await PGRead);
        var result = await conn.QueryAsync<MPData>(sql, par);
        return result.ToList();
    }

    /// <summary>
    /// 每小時匯總
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <returns></returns>
    public async Task<(int totalCount, decimal totalBetValid, decimal totalWin)> SumMPBetRecordByBetTime(DateTime start, DateTime end)
    {
        var sql = @"SELECT 
                    COUNT(gameid) AS totalCount
                    , CASE WHEN SUM(cellscore) IS NULL THEN 0 ELSE SUM(cellscore) END  AS totalBetValid
                    , CASE WHEN SUM(profit) IS NULL THEN 0 ELSE SUM(profit) END AS totalWin
                    FROM t_mp_bet_record
                    WHERE gamestarttime >= @startTime 
                        AND gamestarttime < @endTime";

        var par = new DynamicParameters();
        par.Add("@startTime", start);
        par.Add("@endTime", end);

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
    public async Task<List<MPData>> GetMPRecordsBytime(DateTime start, DateTime end)
    {
        var sql = @"SELECT gameid
                    FROM public.t_mp_bet_record 
                    WHERE gamestarttime >= @startTime 
                        AND gamestarttime < @endTime";
        var par = new DynamicParameters();
        par.Add("@startTime", start);
        par.Add("@endTime", end);

        await using var conn = new NpgsqlConnection(await PGRead);
        var result = await conn.QueryAsync<MPData>(sql, par);
        return result.ToList();
    }

    /// <summary>
    /// 取得GUID資料
    /// </summary>
    /// <param name="RecordReq"></param>
    /// <returns></returns>
    public async Task<List<MPData>> GetMPRecordsBySummary(GetBetRecordReq RecordReq)
    {
        var sql = @"SELECT gameid,accounts,kindid,cellscore,allbet,profit,gamestarttime,gameendtime,summary_id
                    FROM public.t_mp_bet_record 
                    WHERE gamestarttime >= @start 
                        AND gamestarttime <= @end
                        AND summary_id = @summaryId::uuid";
        var param = new
        {
            summaryId = RecordReq.summary_id,
            start = RecordReq.ReportTime.AddDays(-3),
            end = RecordReq.ReportTime.AddDays(1),
        };

        await using var conn = new NpgsqlConnection(await PGRead);
        var result = await conn.QueryAsync<MPData>(sql, param);
        return result.ToList();
    }
}