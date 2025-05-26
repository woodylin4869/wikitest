using Dapper;
using H1_ThirdPartyWalletAPI.Model.Config;
using H1_ThirdPartyWalletAPI.Model.Game.MT.Response;
using H1_ThirdPartyWalletAPI.Model.W1API;
using H1_ThirdPartyWalletAPI.Utility;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;
using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using ThirdPartyWallet.Share.Model.Game.PS.Response;

namespace H1_ThirdPartyWalletAPI.Service.Common.DB.Betlogs;

public interface IMTDBService
{
    Task<int> PostMTRecord(NpgsqlConnection conn, IDbTransaction tran, IEnumerable<queryMerchantGameRecord2Response.Translist> betLogs);
    Task<List<queryMerchantGameRecord2Response.Translist>> GetMTRecordsBytime(DateTime start, DateTime end);
    Task<List<queryMerchantGameRecord2Response.Translist>> GetMTRecords(string id);
    Task<(int totalCount, decimal totalBetValid, decimal totalWin)> SumMTBetRecordByBetTime(DateTime start, DateTime end);

    Task<List<queryMerchantGameRecord2Response.Translist>> GetMTRecordsBySummary(GetBetRecordReq RecordReq);

    Task<IEnumerable<(int count, decimal win, decimal bet, decimal jackpot, string userid, int game_type, DateTime createtime, decimal netwin,decimal turnover)>> SummaryGameRecord
        (DateTime reportTime, DateTime startTime, DateTime endTime);
    Task<List<queryMerchantGameRecord2Response.Translist>> GetMTRecordsV2BySummary(GetBetRecordReq RecordReq);
}

public class MTDBService : BetlogsDBServiceBase, IMTDBService
{
    public MTDBService(ILogger<MTDBService> logger, IBetLogsDbConnectionStringManager connectionStringManager) : base(logger, connectionStringManager)
    {
    }

    /// <summary>
    /// 寫入資料
    /// </summary>
    /// <param name="conn"></param>
    /// <param name="tran"></param>
    /// <param name="betLogs"></param>
    /// <returns></returns>
    public async Task<int> PostMTRecord(NpgsqlConnection conn, IDbTransaction tran, IEnumerable<queryMerchantGameRecord2Response.Translist> betLogs)
    {
        //var sql = @"INSERT INTO public.t_mt_bet_record
        //            (rowid
        //             ,playername
        //             ,gamedate
        //             ,gamecode
        //             ,gametype
        //             ,""period""
        //             ,betamount
        //             ,winamount
        //             ,commissionable
        //             ,roomfee
        //             ,income
        //             ,timezone
        //             ,progressive_wins
        //             ,progressive_share
        //             ,merchantid
        //             ,currency
        //             ,recordid
        //             ,summary_id)
        //            VALUES
        //            ( 
        //                @rowid
        //               ,@playername
        //               ,@gamedate
        //               ,@gamecode
        //               ,@gametype
        //               ,@period
        //               ,@betamount
        //               ,@winamount
        //               ,@commissionable
        //               ,@roomfee
        //               ,@income
        //               ,@timezone
        //               ,@progressive_wins
        //               ,@progressive_share
        //               ,@merchantid
        //               ,@currency
        //               ,@recordid
        //               ,@summary_id)";
        //try
        //{
        //    return await conn.ExecuteAsync(sql, betLogs, tran);
        //}
        //catch (Exception ex)
        //{
        //    throw;
        //}

        if (tran == null) throw new ArgumentNullException(nameof(tran));
        if (betLogs == null) throw new ArgumentNullException(nameof(betLogs));
        if (!betLogs.Any()) return 0;

        var tableGuid = Guid.NewGuid();
        //建立暫存表
        await CreateBetRecordTempTable(tran, tableGuid);
        //將資料倒進暫存表
        await BulkInsertToTempTable(tran, tableGuid, betLogs);
        //將資料由暫存表倒回主表(過濾重複)
        return await MergeFromTempTable(tran, tableGuid);
    }
    private Task<int> CreateBetRecordTempTable(IDbTransaction tran, Guid tableGuid)
    {
        var sql = $@"CREATE TEMPORARY TABLE temp_t_mt_bet_record_v2_{tableGuid:N} 
                            ( LIKE t_mt_bet_record_v2  INCLUDING ALL );";
        return tran.Connection.ExecuteAsync(sql, transaction: tran);
    }
    private async Task<ulong> BulkInsertToTempTable(IDbTransaction tran, Guid tableGuid, IEnumerable<queryMerchantGameRecord2Response.Translist> records)
    {
        if (tran is not NpgsqlTransaction npTran)
            throw new ArgumentException("Must be NpgsqlTransaction", nameof(tran));

        if (npTran.Connection == null)
            throw new ArgumentNullException(nameof(tran.Connection));

        await using var writer =
            await npTran.Connection.BeginBinaryImportAsync(
                @$"COPY temp_t_mt_bet_record_v2_{tableGuid:N} (rowid, playername, gamedate, gamecode, gametype, ""period"", betamount, winamount, 
                commissionable, roomfee, income, timezone, progressive_wins, progressive_share, merchantid, currency, recordid, report_time, partition_time
                ) FROM STDIN (FORMAT BINARY)");

        foreach (var mapping in records)
        {
            await writer.StartRowAsync();

            await writer.WriteAsync(mapping.rowID, NpgsqlDbType.Varchar);   // varchar(255)
            await writer.WriteAsync(mapping.playerName, NpgsqlDbType.Varchar);  // varchar(25)
            await writer.WriteAsync(mapping.gameDate, NpgsqlDbType.Timestamp);  // timestamp
            await writer.WriteAsync(mapping.gameCode, NpgsqlDbType.Varchar);  // varchar(25)
            await writer.WriteAsync(mapping.gameType, NpgsqlDbType.Varchar);  // varchar(25)
            await writer.WriteAsync(mapping.period, NpgsqlDbType.Varchar);  // varchar(55)
            await writer.WriteAsync(mapping.betAmount, NpgsqlDbType.Numeric);  // numeric(14, 4)
            await writer.WriteAsync(mapping.winAmount, NpgsqlDbType.Numeric);  // numeric(14, 4)
            await writer.WriteAsync(mapping.commissionable, NpgsqlDbType.Numeric);  // numeric(14, 4)
            await writer.WriteAsync(mapping.roomFee, NpgsqlDbType.Numeric);  // numeric(14, 4)
            await writer.WriteAsync(mapping.income, NpgsqlDbType.Numeric);  // numeric(14, 4)
            await writer.WriteAsync(mapping.timeZone, NpgsqlDbType.Varchar);  // varchar(20)
            await writer.WriteAsync(mapping.progressive_wins, NpgsqlDbType.Numeric);  // numeric(14, 4)
            await writer.WriteAsync(mapping.progressive_share, NpgsqlDbType.Numeric);  // numeric(14, 4)
            await writer.WriteAsync(mapping.merchantId, NpgsqlDbType.Varchar);  // varchar(20)
            await writer.WriteAsync(mapping.currency, NpgsqlDbType.Varchar);  // varchar(20)
            await writer.WriteAsync(mapping.recordID, NpgsqlDbType.Varchar);  // varchar(25)
            await writer.WriteAsync(mapping.report_time, NpgsqlDbType.Timestamp);  // timestamp
            await writer.WriteAsync(mapping.partition_time, NpgsqlDbType.Timestamp);  // timestamp
        }

        return await writer.CompleteAsync();
    }
    private async Task<int> MergeFromTempTable(IDbTransaction tran, Guid tableGuid)
    {
        var sql = @$"INSERT INTO t_mt_bet_record_v2
                        SELECT rowid, playername, gamedate, gamecode, gametype, ""period"", betamount, winamount, 
                               commissionable, roomfee, income, timezone, progressive_wins, progressive_share, merchantid,
                               currency, recordid, create_time, report_time, partition_time
                        FROM temp_t_mt_bet_record_v2_{tableGuid:N} AS  temp
                        WHERE NOT EXISTS 
                        (
                            SELECT NULL
                            FROM public.t_mt_bet_record_v2
                            WHERE partition_time = temp.partition_time 
                                AND rowid = temp.rowid
                        )";
        return await tran.Connection.ExecuteAsync(sql, tran);
    }

    /// <summary>
    /// 注單號取資料
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<List<queryMerchantGameRecord2Response.Translist>> GetMTRecords(string id)
    {
        var sql = @"
                    SELECT * FROM t_mt_bet_record
                    WHERE slug = @id";

        var par = new DynamicParameters();
        par.Add("@id", id);


        await using var conn = new NpgsqlConnection(await PGRead);
        var result = await conn.QueryAsync<queryMerchantGameRecord2Response.Translist>(sql, par);
        return result.ToList();
    }

    /// <summary>
    /// 每小時匯總
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <returns></returns>
    public async Task<(int totalCount, decimal totalBetValid, decimal totalWin)> SumMTBetRecordByBetTime(DateTime start, DateTime end)
    {
        var sql = @"SELECT 
                    COUNT(rowid) AS totalCount
                    , CASE WHEN SUM(betAmount) IS NULL THEN 0 ELSE SUM(betAmount) END  AS totalBetValid
                    , CASE WHEN SUM(winamount) IS NULL THEN 0 ELSE SUM(winamount) END AS totalWin
                    FROM t_mt_bet_record
                    WHERE gamedate >= @startTime 
                        AND gamedate < @endTime";

        var par = new DynamicParameters();
        par.Add("@startTime", start);
        par.Add("@endTime", end.AddMilliseconds(-1));

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
    public async Task<List<queryMerchantGameRecord2Response.Translist>> GetMTRecordsBytime(DateTime start, DateTime end)
    {
        try
        {
            var sql = @"SELECT rowid
                    FROM public.t_mt_bet_record 
                    WHERE gamedate >= @startTime 
                        AND gamedate < @endTime";
            var par = new DynamicParameters();
            par.Add("@startTime", start);
            par.Add("@endTime", end.AddMilliseconds(-1));

            await using var conn = new NpgsqlConnection(await PGRead);
            var result = await conn.QueryAsync<queryMerchantGameRecord2Response.Translist>(sql, par);
            return result.ToList();
        }
        catch (Exception ex)
        {
            throw;
        }
    }

    /// <summary>
    /// 取得GUID資料
    /// </summary>
    /// <param name="RecordReq"></param>
    /// <returns></returns>
    public async Task<List<queryMerchantGameRecord2Response.Translist>> GetMTRecordsBySummary(GetBetRecordReq RecordReq)
    {
        var sql = @"SELECT rowID,playerName,gameDate,gameCode,gameType,betAmount,winAmount,commissionable,roomFee,income,currency,summary_id 
                    FROM public.t_mt_bet_record 
                    WHERE gamedate >= @start 
                        AND gamedate <= @end
                        AND summary_id = @summaryId::uuid";
        var param = new
        {
            summaryId = RecordReq.summary_id,
            start = RecordReq.ReportTime.AddDays(-3),
            end = RecordReq.ReportTime.AddDays(1),
        };

        await using var conn = new NpgsqlConnection(await PGRead);
        var result = await conn.QueryAsync<queryMerchantGameRecord2Response.Translist>(sql, param);
        return result.ToList();
    }
    /// <summary>
    /// 取得GUID資料
    /// </summary>
    /// <param name="RecordReq"></param>
    /// <returns></returns>
    public async Task<List<queryMerchantGameRecord2Response.Translist>> GetMTRecordsV2BySummary(GetBetRecordReq RecordReq)
    {
        var sql = @"SELECT rowID,playerName,gameDate,gameCode,gameType,betAmount,winAmount,commissionable,roomFee,income,currency ,partition_time
                    FROM public.t_mt_bet_record_v2
                    WHERE partition_time >= @start 
                        AND partition_time <= @end
                        AND report_time <= @report_time
                        AND playerName = @playerName";
        var param = new
        {
            playerName =Config.OneWalletAPI.Prefix_Key + RecordReq.ClubId,
            start = RecordReq.ReportTime,
            end = RecordReq.ReportTime.AddDays(1).AddMilliseconds(-1),
            report_time = RecordReq.ReportTime,
        };

        await using var conn = new NpgsqlConnection(await PGRead);
        var result = await conn.QueryAsync<queryMerchantGameRecord2Response.Translist>(sql, param);
        return result.ToList();
    }
    /// <summary>
    /// 五分鐘會總
    /// </summary>
    /// <param name="reportTime"></param>
    /// <param name="startTime"></param>
    /// <param name="endTime"></param>
    /// <returns></returns>
    public async Task<IEnumerable<(int count, decimal win, decimal bet, decimal jackpot, string userid, int game_type, DateTime createtime, decimal netwin, decimal turnover)>> SummaryGameRecord(DateTime reportTime, DateTime startTime, DateTime endTime)
    {
        var sql = @"SELECT CAST(count(1) AS INTEGER) AS count,
                        SUM(winAmount) AS win,
                        SUM(income) AS netwin,
                        SUM(commissionable) AS bet,
                        SUM(income) AS turnover,
                        SUM(progressive_wins) as jackpot,
                        playerName as userid, 
                        5 as game_type,
                        Date(gameDate) as createtime
                        FROM t_mt_bet_record_v2
                        WHERE partition_time BETWEEN @start_time AND @end_time
                        AND report_time = @report_time
                        GROUP BY playerName,Date(gameDate)
                        ";

        var par = new DynamicParameters();
        par.Add("@start_time", startTime.AddDays(-2));
        par.Add("@end_time", endTime);
        par.Add("@report_time", reportTime);
        using (NpgsqlConnection conn = new NpgsqlConnection(await PGRead))
        {
            var result = await conn.QueryAsync(sql, par, commandTimeout: 270);
            var a = result.Select(x => ((int)x.count, (decimal)x.win, (decimal)x.bet, (decimal)x.jackpot, (string)x.userid, (int)x.game_type, (DateTime)x.createtime, (decimal)x.netwin, (decimal)x.turnover)).ToList();
            return a;
        }
    }

}