using Dapper;
using H1_ThirdPartyWalletAPI.Model;
using H1_ThirdPartyWalletAPI.Model.Config;
using H1_ThirdPartyWalletAPI.Model.DataModel;
using H1_ThirdPartyWalletAPI.Model.Game;
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

public interface IRcgDBService
{
    public Task<int> PostRcgRecord(NpgsqlConnection conn, IDbTransaction tran, List<BetRecord> record_data);
    public Task<IEnumerable<dynamic>> GetRcgRecordLatest(string systemCode, string webId);
    public Task<GameReport> SumRcgBetRecordHourly(DateTime reportDateTime);
    public Task<(int totalCount, decimal totalBetValid, decimal totalWin)> SumRcgBetRecordByReportdt(DateTime start, DateTime end);
    public Task<List<BetRecord>> GetRcgRecordByWebForRepair(string systemcode, string webid, DateTime start, DateTime end);
    public Task<GetRcgRunNoById> GetRcgRunNoById(long id, DateTime reportdt);
    public Task<BetRecord> GetRcgRecordById(long id, DateTime reportdt);
}

public class RcgDBService : BetlogsDBServiceBase, IRcgDBService
{
    public RcgDBService(ILogger<RcgDBService> logger, IBetLogsDbConnectionStringManager connectionStringManager) : base(logger, connectionStringManager)
    {
    }


    #region t_rcg_bet_record
    public async Task<int> PostRcgRecord(NpgsqlConnection conn, IDbTransaction tran, List<BetRecord> record_data)
    {
        string stSqlInsert = @"INSERT INTO t_rcg_bet_record
            (
                summary_id,
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
                odds,
                originRecordId,
                pre_bet,
                pre_available,
                pre_winlose,
                pre_id
            )
            VALUES
            (
                @summary_id,
                @systemcode,
                @webid,
                @memberaccount,
                @id,
                @gameid,
                @desk,
                @betarea,
                @bet,
                @available,
                @winlose,
                @waterrate,
                @activeno,
                @runno,
                @balance,
                @datetime,
                @reportdt,
                @ip,
                @odds,
                @originRecordId,
                @pre_bet,
                @pre_available,
                @pre_winlose,
                @pre_id
            )";
        return await conn.ExecuteAsync(stSqlInsert, record_data, tran);
    }
    public async Task<IEnumerable<dynamic>> GetRcgRecordLatest(string systemCode, string webId)
    {
        var par = new DynamicParameters();
        par.Add("@systemcode", systemCode);
        par.Add("@webid", webId);
        par.Add("@reportdt", DateTime.Now.AddDays(-3));
        string strSql = @"SELECT MAX(id)
                            FROM t_rcg_bet_record
                            WHERE reportdt > @reportdt
                            AND systemcode = @systemcode
                            AND webid = @webid";
        using (NpgsqlConnection conn = new NpgsqlConnection(await PGRead))
        {
            return await conn.QueryAsync<dynamic>(strSql, par);
        }
    }

    public async Task<GameReport> SumRcgBetRecordHourly(DateTime reportDateTime)
    {
        var par = new DynamicParameters();
        string strSql = @"SELECT 
                    COUNT(*) AS total_cont
                    , SUM(available) AS total_bet
                    , SUM(winlose) AS total_netwin
                    FROM t_rcg_bet_record
                    WHERE reportdt >= @startTime
                    AND reportdt < @endTime
                    ";

        par.Add("@startTime", reportDateTime);
        par.Add("@endTime", reportDateTime.AddHours(1));
        using (NpgsqlConnection conn = new NpgsqlConnection(await PGRead))
        {
            return await conn.QuerySingleOrDefaultAsync<GameReport>(strSql, par);
        }
    }

    public async Task<(int totalCount, decimal totalBetValid, decimal totalWin)> SumRcgBetRecordByReportdt(DateTime start, DateTime end)
    {
        var sql = @"SELECT 
                        COUNT(1) AS totalCount
                        , CASE WHEN SUM(available) IS NULL THEN 0 ELSE SUM(available) END  AS totalBetValid
                        , CASE WHEN SUM(winlose) IS NULL THEN 0 ELSE SUM(winlose) END AS totalWin
                        FROM t_rcg_bet_record
                        WHERE reportdt >= @startTime 
                            AND reportdt < @endTime";

        var par = new DynamicParameters();
        par.Add("@startTime", start);
        par.Add("@endTime", start.AddHours(1));

        await using var conn = new NpgsqlConnection(await PGRead);
        var result = (await conn.QueryAsync<dynamic>(sql, par)).FirstOrDefault();
        return ((int)result.totalcount, (decimal)result.totalbetvalid, (decimal)result.totalwin);
    }

    /// <summary>
    /// RCG 取得時間內的注單編號 ForRepair 比對id用途
    /// </summary>
    /// <param name="systemcode"></param>
    /// <param name="webid"></param>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <returns></returns>
    public async Task<List<BetRecord>> GetRcgRecordByWebForRepair(string systemcode, string webid, DateTime start, DateTime end)
    {
        var sql = @"SELECT id 
                    FROM t_rcg_bet_record 
                    WHERE systemcode = @systemcode
                        AND webid = @webid
                        AND reportdt >= @startTime 
                        AND reportdt < @endTime";
        var par = new DynamicParameters();
        par.Add("@systemcode", systemcode);
        par.Add("@webid", webid);
        par.Add("@startTime", start);
        par.Add("@endTime", end);

        await using var conn = new NpgsqlConnection(await PGRead);
        var result = await conn.QueryAsync<BetRecord>(sql, par);
        return result.ToList();
    }

    /// <summary>
    /// 查詢注單之查輪局桌別 By Id
    /// </summary>
    /// <param name="id"></param>
    /// <param name="reportdt"></param>
    /// <returns></returns>
    public async Task<GetRcgRunNoById> GetRcgRunNoById(long id, DateTime reportdt)
    {
        string strSql = @"SELECT desk, activeno, runno, reportdt
                    FROM t_rcg_bet_record
                    WHERE reportdt >= @startTime
                        AND reportdt <= @endTime
                        AND id = @id";
        var param = new
        {
            id = id,
            startTime = reportdt.AddDays(-2),
            endTime = reportdt.AddDays(2),
        };

        using (var conn = new NpgsqlConnection(await PGRead))
        {
            return await conn.QuerySingleOrDefaultAsync<GetRcgRunNoById>(strSql, param);
        }
    }

    /// <summary>
    /// 查詢注單資料 By Id
    /// </summary>
    /// <param name="id"></param>
    /// <param name="reportdt"></param>
    /// <returns></returns>
    public async Task<BetRecord> GetRcgRecordById(long id, DateTime reportdt)
    {
        string strSql = @"SELECT *
                    FROM t_rcg_bet_record
                    WHERE reportdt >= @startTime
                        AND reportdt <= @endTime
                        AND id = @id";
        var param = new
        {
            id = id,
            startTime = reportdt.AddDays(-2),
            endTime = reportdt.AddDays(2),
        };

        using (var conn = new NpgsqlConnection(await PGRead))
        {
            return await conn.QuerySingleOrDefaultAsync<BetRecord>(strSql, param);
        }
    }

    #endregion
}