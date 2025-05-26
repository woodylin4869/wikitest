using Dapper;
using H1_ThirdPartyWalletAPI.Model.Config;
using H1_ThirdPartyWalletAPI.Model.DataModel;
using H1_ThirdPartyWalletAPI.Model.Game.NEXTSPIN.Response;
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
using H1_ThirdPartyWalletAPI.Model.DB.NEXTSPIN;
using H1_ThirdPartyWalletAPI.Model.DB.NEXTSPIN.Response;
using static H1_ThirdPartyWalletAPI.Model.Game.NEXTSPIN.Response.GetBetHistoryResponse;

namespace H1_ThirdPartyWalletAPI.Service.Common.DB.Betlogs;

public interface INextSpinDBService
{
    Task<int> PostNextSpinRecord(IDbTransaction tran, IEnumerable<GetBetHistoryResponse.BetInfo> betInfos);
    Task<List<t_nextspin_bet_record>> GetNextSpinRecordsBySummary(string summaryId, DateTime start, DateTime end);
    Task<(int totalCount, decimal totalBetValid, decimal totalNetWin)> SumNextSpinBetRecordByTicketTime(DateTime start, DateTime end);
    Task<List<NextSpinPrimaryKey>> GetNextSpinRecordsByTicketTime(DateTime start, DateTime end);
    Task<int> PostNextSpinRecordV2(NpgsqlConnection conn, IDbTransaction tran, List<GetBetHistoryResponse.BetInfo> betInfos);

    /// <summary>
    /// 依下注時間取得注單
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <returns></returns>
    Task<List<NextSpinPrimaryKey>> GetNextSpinRecordV2sByTicketTime(DateTime start, DateTime end);

    Task<(int totalCount, decimal totalBetValid, decimal totalNetWin)> SumNextSpinBetRecordV2ByTicketTime(DateTime start, DateTime end);
    Task<List<(int count, decimal win, decimal bet, decimal netwin, string userid, DateTime ticketTimeDate)>> NextSpinBetRecordV2Summary(DateTime reportTime, DateTime startTime, DateTime endTime);
    Task<List<GetNextSpinRecordV2sBySummaryResponse>> GetNextSpinRecordV2sBySummary(string userId, DateTime reportTime, DateTime start, DateTime end);
}

public class NextSpinDBService : BetlogsDBServiceBase, INextSpinDBService
{
    public NextSpinDBService(ILogger<NextSpinDBService> logger, IBetLogsDbConnectionStringManager connectionStringManager) : base(logger, connectionStringManager)
    {
    }

    public Task<int> PostNextSpinRecord(IDbTransaction tran, IEnumerable<GetBetHistoryResponse.BetInfo> betInfos)
    {
        var sql = @"INSERT INTO public.t_nextspin_bet_record
                    ( ticketid
                    , acctid
                    , categoryid
                    , gamecode
                    , tickettime
                    , betip
                    , betamount
                    , winloss
                    , currency
                    , ""result""
                    , jackpotamount
                    , luckydrawid
                    , completed
                    , roundid
                    , ""sequence""
                    , channel
                    , balance
                    , jpwin
                    , summary_id)
                    VALUES
                    ( @ticketid
                    , @acctid
                    , @categoryid
                    , @gamecode
                    , @TicketTimeFormatted
                    , @betip
                    , @betamount
                    , @winloss
                    , @currency
                    , @result
                    , @jackpotamount
                    , @luckydrawid
                    , @completed
                    , @roundid
                    , @sequence
                    , @channel
                    , @balance
                    , @jpwin
                    , @summary_id);
                    ";

        return tran.Connection.ExecuteAsync(sql, betInfos, tran);
    }

    public async Task<List<t_nextspin_bet_record>> GetNextSpinRecordsBySummary(string summaryId, DateTime start, DateTime end)
    {
        var sql = @"SELECT ticketid,acctid,categoryid,gamecode,betamount,betip,winloss,currency,""result"",jackpotamount,luckydrawid,completed,roundid,""sequence"",channel,balance
                    ,jpwin,create_time,tickettime
                    FROM public.t_nextspin_bet_record 
                    WHERE ticketTime >= @start 
                        AND ticketTime <= @end
                        AND summary_id = @summaryId::uuid";
        var param = new
        {
            summaryId,
            start,
            end,
        };

        await using var conn = new NpgsqlConnection(await PGRead);
        var result = await conn.QueryAsync<t_nextspin_bet_record>(sql, param);
        return result.ToList();
    }

    /// <summary>
    /// 依下注時間取得注單
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <returns></returns>
    public async Task<List<NextSpinPrimaryKey>> GetNextSpinRecordsByTicketTime(DateTime start, DateTime end)
    {
        var sql = @"
                    SELECT ticketId,  ticketTime FROM t_nextspin_bet_record
                    WHERE ticketTime >= @startTime 
                        AND ticketTime <= @endTime";

        var par = new DynamicParameters();
        par.Add("@startTime", start);
        par.Add("@endTime", end);

        await using var conn = new NpgsqlConnection(await PGRead);
        var result = await conn.QueryAsync<NextSpinPrimaryKey>(sql, par);
        return result.ToList();
    }

    public async Task<(int totalCount, decimal totalBetValid, decimal totalNetWin)> SumNextSpinBetRecordByTicketTime(DateTime start, DateTime end)
    {
        var sql = @"SELECT 
                    COUNT(ticketid) AS totalCount
                    , CASE WHEN SUM(betamount) IS NULL THEN 0 ELSE SUM(betamount) END  AS totalBetValid
                    , CASE WHEN SUM(winloss) IS NULL THEN 0 ELSE SUM(winloss) END AS totalNetWin
                    FROM t_nextspin_bet_record
                    WHERE tickettime >= @startTime 
                        AND tickettime <= @endTime";

        var par = new DynamicParameters();
        par.Add("@startTime", start);
        par.Add("@endTime", end);

        await using var conn = new NpgsqlConnection(await PGRead);
        var result = (await conn.QueryAsync<dynamic>(sql, par)).FirstOrDefault();
        return ((int)result.totalcount, (decimal)result.totalbetvalid, (decimal)result.totalnetwin);
    }

    #region v2

    public async Task<int> PostNextSpinRecordV2(NpgsqlConnection conn, IDbTransaction tran, List<GetBetHistoryResponse.BetInfo> betInfos)
    {
        var tempTableName = $"t_nextspin_bet_record_v2_{Guid.NewGuid():N}";
        try
        {
            await CreateTempTable(conn, tran, tempTableName);
            await BulkInsertTonextspinTempTable(conn, tran, tempTableName, betInfos);
            return await MergenextspinRecordFromTempTable(conn, tran, tempTableName);
        }
        finally
        {
            await RemovePostnextspinRecordTempTable(conn, tran, tempTableName);
        }
        //var sql = @"INSERT INTO public.t_nextspin_bet_record_v2
        //            ( ticketid
        //            , acctid
        //            , categoryid
        //            , gamecode
        //            , tickettime
        //            , betip
        //            , betamount
        //            , winloss
        //            , currency
        //            , ""result""
        //            , jackpotamount
        //            , luckydrawid
        //            , completed
        //            , roundid
        //            , ""sequence""
        //            , channel
        //            , balance
        //            , jpwin
        //            , report_time)
        //            VALUES
        //            ( @ticketid
        //            , @acctid
        //            , @categoryid
        //            , @gamecode
        //            , @TicketTimeFormatted
        //            , @betip
        //            , @betamount
        //            , @winloss
        //            , @currency
        //            , @result
        //            , @jackpotamount
        //            , @luckydrawid
        //            , @completed
        //            , @roundid
        //            , @sequence
        //            , @channel
        //            , @balance
        //            , @jpwin
        //            , @ReportTime);
        //            ";

        //return tran.Connection.ExecuteAsync(sql, betInfos, tran);
    }

    /// <summary>
    /// 依下注時間取得注單
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <returns></returns>
    public async Task<List<NextSpinPrimaryKey>> GetNextSpinRecordV2sByTicketTime(DateTime start, DateTime end)
    {
        var sql = @"
                    SELECT ticketId,  ticketTime
                    FROM t_nextspin_bet_record_v2
                    WHERE ticketTime >= @startTime 
                        AND ticketTime < @endTime";

        var par = new DynamicParameters();
        par.Add("@startTime", start);
        par.Add("@endTime", end);

        await using var conn = new NpgsqlConnection(await PGRead);
        var result = await conn.QueryAsync<NextSpinPrimaryKey>(sql, par);
        return result.ToList();
    }

    public async Task<List<GetNextSpinRecordV2sBySummaryResponse>> GetNextSpinRecordV2sBySummary(string userId, DateTime reportTime, DateTime start, DateTime end)
    {
        var sql = @"SELECT ticketId,  ticketTime, betamount, winloss, gamecode 
                    FROM public.t_nextspin_bet_record_v2
                    WHERE ticketTime >= @start 
                        AND ticketTime < @end
                        AND acctid = @userId
                        AND report_time = @reportTime";
        var param = new
        {
            userId,
            reportTime,
            start,
            end
        };

        await using var conn = new NpgsqlConnection(await PGRead);
        var result = await conn.QueryAsync<GetNextSpinRecordV2sBySummaryResponse>(sql, param);
        return result.ToList();
    }

    public async Task<(int totalCount, decimal totalBetValid, decimal totalNetWin)> SumNextSpinBetRecordV2ByTicketTime(DateTime start, DateTime end)
    {
        var sql = @"SELECT 
                    COUNT(ticketid) AS totalCount
                    , CASE WHEN SUM(betamount) IS NULL THEN 0 ELSE SUM(betamount) END  AS totalBetValid
                    , CASE WHEN SUM(winloss) IS NULL THEN 0 ELSE SUM(winloss) END AS totalNetWin
                    FROM t_nextspin_bet_record_v2
                    WHERE tickettime >= @startTime 
                        AND tickettime <= @endTime";

        var par = new DynamicParameters();
        par.Add("@startTime", start);
        par.Add("@endTime", end);

        await using var conn = new NpgsqlConnection(await PGRead);
        var result = (await conn.QueryAsync<dynamic>(sql, par)).FirstOrDefault();
        return ((int)result.totalcount, (decimal)result.totalbetvalid, (decimal)result.totalnetwin);
    }

    public async Task<List<(int count, decimal win, decimal bet, decimal netwin, string userid, DateTime ticketTimeDate)>> NextSpinBetRecordV2Summary(DateTime reportTime, DateTime startTime, DateTime endTime)
    {
        const string sql = @"SELECT count(1) AS count,
                                SUM(coalesce(betamount,0) + coalesce(winloss,0))  AS win,
                                SUM(coalesce(betamount,0)) AS bet,
                                SUM(coalesce(winloss,0)) AS netwin,
                                acctid AS userid,
                                date(ticketTime) AS ticketTimeDate
                            FROM t_nextspin_bet_record_v2
                            WHERE ticketTime BETWEEN @startTime AND @endTime
                            AND report_time = @reportTime
                            GROUP BY acctid, date(ticketTime)";

        var param = new
        {
            startTime,
            endTime,
            reportTime,
        };
        await using var conn = new NpgsqlConnection(PGMaster);
        var result = await conn.QueryAsync(sql, param, commandTimeout: 270);
        return result
            .Select(r => ((int)r.count, (decimal)r.win, (decimal)r.bet, (decimal)r.netwin, (string)r.userid, (DateTime)r.tickettimedate))
            .ToList();
    }
    #endregion
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
        var sql = "CREATE TEMPORARY TABLE IF NOT EXISTS #TempTableName ( LIKE t_nextspin_bet_record_v2 INCLUDING DEFAULTS INCLUDING CONSTRAINTS );";
        sql = sql.Replace("#TempTableName", tempTableName);
        // 建立temp資料表
        await conn.ExecuteAsync(sql, tran, commandTimeout: 60);

        // 建立唯一索引避免資料重複
        sql = $"CREATE UNIQUE index IF NOT EXISTS {tempTableName}_un ON {tempTableName} (ticketid);";
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
    List<GetBetHistoryResponse.BetInfo> record_data)
    {
        var sql = @"COPY #TempTableName (
                         ticketid
                    , acctid
                    , categoryid
                    , gamecode
                    , tickettime
                    , betip
                    , betamount
                    , winloss
                    , currency
                    , ""result""
                    , jackpotamount
                    , luckydrawid
                    , completed
                    , roundid
                    , ""sequence""
                    , channel
                    , balance
                    , jpwin
                    , report_time
                        ) FROM STDIN (FORMAT BINARY)";


        sql = sql.Replace("#TempTableName", tempTableName);
        try
        {
            await using var writer = await conn.BeginBinaryImportAsync(sql);
            foreach (var betInfo in record_data)
            {
                await writer.StartRowAsync();
                // 寫入每一列的資料，請根據你的數據庫結構和類型進行調整
                await writer.WriteAsync(betInfo.ticketId, NpgsqlTypes.NpgsqlDbType.Bigint);
                await writer.WriteAsync(betInfo.acctId, NpgsqlTypes.NpgsqlDbType.Varchar);
                await writer.WriteAsync(betInfo.categoryId, NpgsqlTypes.NpgsqlDbType.Varchar);
                await writer.WriteAsync(betInfo.gameCode, NpgsqlTypes.NpgsqlDbType.Varchar);
                await writer.WriteAsync(betInfo.TicketTimeFormatted, NpgsqlTypes.NpgsqlDbType.Timestamp);
                await writer.WriteAsync(betInfo.betIp, NpgsqlTypes.NpgsqlDbType.Varchar);
                await writer.WriteAsync(betInfo.betAmount, NpgsqlTypes.NpgsqlDbType.Numeric);
                await writer.WriteAsync(betInfo.winLoss, NpgsqlTypes.NpgsqlDbType.Numeric);
                await writer.WriteAsync(betInfo.currency, NpgsqlTypes.NpgsqlDbType.Varchar);
                await writer.WriteAsync(betInfo.result, NpgsqlTypes.NpgsqlDbType.Varchar);
                await writer.WriteAsync(betInfo.jackpotAmount, NpgsqlTypes.NpgsqlDbType.Numeric);
                await writer.WriteAsync(betInfo.luckyDrawId, NpgsqlTypes.NpgsqlDbType.Bigint);
                await writer.WriteAsync(betInfo.completed, NpgsqlTypes.NpgsqlDbType.Boolean);
                await writer.WriteAsync(betInfo.roundId, NpgsqlTypes.NpgsqlDbType.Bigint);
                await writer.WriteAsync(betInfo.sequence, NpgsqlTypes.NpgsqlDbType.Smallint);
                await writer.WriteAsync(betInfo.channel, NpgsqlTypes.NpgsqlDbType.Varchar);
                await writer.WriteAsync(betInfo.balance, NpgsqlTypes.NpgsqlDbType.Numeric);
                await writer.WriteAsync(betInfo.jpWin, NpgsqlTypes.NpgsqlDbType.Numeric);
                await writer.WriteAsync(betInfo.ReportTime, NpgsqlTypes.NpgsqlDbType.Timestamp);
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
                    insert into t_nextspin_bet_record_v2 (
                     ticketid
                    , acctid
                    , categoryid
                    , gamecode
                    , tickettime
                    , betip
                    , betamount
                    , winloss
                    , currency
                    , ""result""
                    , jackpotamount
                    , luckydrawid
                    , completed
                    , roundid
                    , ""sequence""
                    , channel
                    , balance
                    , jpwin
                    , report_time
                        )
                        select        ticketid
                    , acctid
                    , categoryid
                    , gamecode
                    , tickettime
                    , betip
                    , betamount
                    , winloss
                    , currency
                    , ""result""
                    , jackpotamount
                    , luckydrawid
                    , completed
                    , roundid
                    , ""sequence""
                    , channel
                    , balance
                    , jpwin
                    , report_time
                        from #TempTableName tempTable
                        where not exists (
                                select null from t_nextspin_bet_record_v2
		                        where tickettime = tempTable.tickettime
		                        and  ticketid = tempTable.ticketid
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