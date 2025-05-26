using Dapper;
using H1_ThirdPartyWalletAPI.Model.Config;
using H1_ThirdPartyWalletAPI.Model.DB.BTI.DBResponse;
using H1_ThirdPartyWalletAPI.Model.Game.BTI.Response;
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

public interface IBTIDBService
{
    Task<int> PostBTIRecord(IDbTransaction tran, IEnumerable<Bets> bets);
    Task<int> PostBTIRecordRunning(IDbTransaction tran, IEnumerable<Bets> betInfos);
    Task<int> DeleteBTIRecordRunning(IDbTransaction tran, string purchaseid, DateTime betTime);
    Task<List<GetBTIRunningRecordDBResponse>> GetBTIRunningRecord(GetBetRecordUnsettleReq RecordReq);
    Task<List<GetBTIRecordsBySummaryDBResponse>> GetBTIRecordsBySummary(GetBetRecordReq RecordReq);
    Task<List<BTIRecordPrimaryKey>> GetBTIHistoryBetPKByBetTime(DateTime start, DateTime end);
    Task<List<BTIRecordPrimaryKey>> GetBTIHistoryBetPKBySearchDateTime(DateTime start, DateTime end);
    Task<List<GetBTIPreRecordDBResponse>> GetBTIRecordsPreAmountByPurchaseId(IDbTransaction tran, string purchaseId, DateTime bettime);
    Task<(int totalCount, decimal totalBetValid, decimal totalNetWin)> SumBTIBetRecordByCreationdate(DateTime start, DateTime end);
}

public class BTIDBService : BetlogsDBServiceBase, IBTIDBService
{
    public BTIDBService(ILogger<BTIDBService> logger, IBetLogsDbConnectionStringManager connectionStringManager) : base(logger, connectionStringManager)
    {
    }

    public Task<int> PostBTIRecord(IDbTransaction tran, IEnumerable<Bets> bets)
    {
        var sql = @"INSERT INTO public.t_bti_bet_record(
                        summary_id
	                    ,betsettleddate
	                    ,betstatus
	                    ,bettypeid
	                    ,bettypename
	                    ,brand
	                    ,combobonusamount
	                    ,creationdate
	                    ,currency
	                    ,customerid
	                    ,domainid
	                    ,merchantcustomerid
	                    ,noncashoutamount
	                    ,numberofbets
	                    ,odds
	                    ,oddsdec
	                    ,oddsinuserstyle
	                    ,oddsstyleofuser
	                    ,pl
	                    ,platform
	                    ,playerlevelid
	                    ,playerlevelname
	                    ,purchaseid
	                    ,realmoneyamount
	                    ,return
	                    ,searchdatetime
	                    ,systemname
	                    ,totalstake
	                    ,updatedate
	                    ,username
	                    ,validstake
	                    ,pre_totalstake
	                    ,pre_validstake
	                    ,pre_pl
	                    ,pre_return
	                    ,pre_betstatus
                        ,branchid
                        ,branchname
                        ,leaguename
                        ,hometeam
                        ,awayteam
                        ,yourbet)
                    VALUES(
                        @summary_id
                        , @betsettleddate
                        , @betstatus
                        , @bettypeid
                        , @bettypename
                        , @brand
                        , @combobonusamount
                        , @creationdate
                        , @currency
                        , @customerid
                        , @domainid
                        , @merchantcustomerid
                        , @noncashoutamount
                        , @numberofbets
                        , @odds
                        , @oddsdec
                        , @oddsinuserstyle
                        , @oddsstyleofuser
                        , @pl
                        , @platform
                        , @playerlevelid
                        , @playerlevelname
                        , @purchaseid
                        , @realmoneyamount
                        , @return
                        , @searchdatetime
                        , @systemname
                        , @totalstake
                        , @updatedate
                        , @username
                        , @validstake
                        , @pre_totalstake
                        , @pre_validstake
                        , @pre_pl
                        , @pre_return
                        , @pre_betstatus
                        , @branchid
                        , @branchname
                        , @leaguename
                        , @hometeam
                        , @awayteam
                        , @yourbet);";

        return tran.Connection.ExecuteAsync(sql, bets, tran);
    }

    public Task<int> PostBTIRecordRunning(IDbTransaction tran, IEnumerable<Bets> betInfos)
    {
        var sql = @"INSERT INTO public.t_bti_bet_record_running(
                        summary_id
	                    ,betsettleddate
	                    ,betstatus
	                    ,bettypeid
	                    ,bettypename
	                    ,brand
	                    ,combobonusamount
	                    ,creationdate
	                    ,currency
	                    ,customerid
	                    ,domainid
	                    ,merchantcustomerid
	                    ,noncashoutamount
	                    ,numberofbets
	                    ,odds
	                    ,oddsdec
	                    ,oddsinuserstyle
	                    ,oddsstyleofuser
	                    ,pl
	                    ,platform
	                    ,playerlevelid
	                    ,playerlevelname
	                    ,purchaseid
	                    ,realmoneyamount
	                    ,return
	                    ,searchdatetime
	                    ,systemname
	                    ,totalstake
	                    ,updatedate
	                    ,username
	                    ,validstake
                        ,branchid
                        ,branchname
                        ,leaguename
                        ,hometeam
                        ,awayteam
                        ,yourbet
	                    ,club_id
	                    ,franchiser_id)
                    VALUES(
                        @summary_id
                        , @betsettleddate
                        , @betstatus
                        , @bettypeid
                        , @bettypename
                        , @brand
                        , @combobonusamount
                        , @creationdate
                        , @currency
                        , @customerid
                        , @domainid
                        , @merchantcustomerid
                        , @noncashoutamount
                        , @numberofbets
                        , @odds
                        , @oddsdec
                        , @oddsinuserstyle
                        , @oddsstyleofuser
                        , @pl
                        , @platform
                        , @playerlevelid
                        , @playerlevelname
                        , @purchaseid
                        , @realmoneyamount
                        , @return
                        , @searchdatetime
                        , @systemname
                        , @totalstake
                        , @updatedate
                        , @username
                        , @validstake
                        , @branchid
                        , @branchname
                        , @leaguename
                        , @hometeam
                        , @awayteam
                        , @yourbet
                        , @club_id
                        , @franchiser_id);";

        return tran.Connection.ExecuteAsync(sql, betInfos, tran);
    }

    public async Task<int> DeleteBTIRecordRunning(IDbTransaction tran, string purchaseid, DateTime creationdate)
    {
        var sql = "Delete from public.t_bti_bet_record_running where purchaseid = @purchaseid and creationdate = @creationdate;";

        return await tran.Connection.ExecuteAsync(sql, new { purchaseid, creationdate }, tran);
    }

    public async Task<List<GetBTIRunningRecordDBResponse>> GetBTIRunningRecord(GetBetRecordUnsettleReq RecordReq)
    {
        var par = new DynamicParameters();
        var sql = @"SELECT 
                        purchaseid, betstatus, creationdate, updatedate
                        , branchid
                        , branchname
                        , leaguename
                        , hometeam
                        , awayteam
                        , yourbet
                        , oddsinuserstyle
                        , oddsstyleofuser
                        , totalstake
                        , validstake
                        , pl
                        , bettypename
                        , club_id
                        , franchiser_id
                    FROM public.t_bti_bet_record_running 
                    WHERE creationdate >= @start 
                        AND creationdate < @end";

        if (RecordReq.Club_id != null)
        {
            par.Add("@Club_id", RecordReq.Club_id);
            sql += " AND Club_id = @Club_id";
        }
        if (RecordReq.Franchiser_id != null)
        {
            par.Add("@Franchiser_id", RecordReq.Franchiser_id);
            sql += " AND Franchiser_id = @Franchiser_id";
        }

        par.Add("@start", RecordReq.StartTime != null ? RecordReq.StartTime : DateTime.Now.AddDays(-100));
        par.Add("@end", RecordReq.EndTime != null ? RecordReq.EndTime : DateTime.Now);

        using NpgsqlConnection conn = new NpgsqlConnection(await PGRead);
        var result = await conn.QueryAsync<GetBTIRunningRecordDBResponse>(sql, par);
        return result.ToList();
    }

    public async Task<List<GetBTIRecordsBySummaryDBResponse>> GetBTIRecordsBySummary(GetBetRecordReq RecordReq)
    {
        var sql = @"SELECT
                        purchaseid, betstatus, creationdate, updatedate
                        , branchid
                        , branchname
                        , leaguename
                        , hometeam
                        , awayteam
                        , yourbet
                        , oddsinuserstyle
                        , oddsstyleofuser
                        , totalstake
                        , validstake
                        , pl
                        , bettypename
                    FROM public.t_bti_bet_record 
                    WHERE creationdate >= @start 
                        AND creationdate < @end
                        AND summary_id = @summaryId::uuid;";
        var param = new
        {
            summaryId = RecordReq.summary_id,
            start = RecordReq.ReportTime,
            end = RecordReq.ReportTime.AddDays(1),
        };

        await using var conn = new NpgsqlConnection(await PGRead);
        var result = await conn.QueryAsync<GetBTIRecordsBySummaryDBResponse>(sql, param);
        return result.ToList();
    }

    /// <summary>
    /// 已結算 依下注時間取得注單PK(purchaseid, betstatus, creationdate, updatedate)
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <returns></returns>
    public async Task<List<BTIRecordPrimaryKey>> GetBTIHistoryBetPKByBetTime(DateTime start, DateTime end)
    {
        var sql = @"
                    SELECT purchaseid, betstatus, creationdate, updatedate
                    FROM public.t_bti_bet_record
                    WHERE creationdate >= @startTime 
                        AND creationdate <= @endTime;";

        var par = new DynamicParameters();
        par.Add("@startTime", start);
        par.Add("@endTime", end);

        await using var conn = new NpgsqlConnection(await PGRead);
        var result = await conn.QueryAsync<BTIRecordPrimaryKey>(sql, par);
        return result.ToList();
    }

    /// <summary>
    /// 已結算 依SearchDateTime取得注單PK(purchaseid, betstatus, creationdate, updatedate)
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <returns></returns>
    public async Task<List<BTIRecordPrimaryKey>> GetBTIHistoryBetPKBySearchDateTime(DateTime start, DateTime end)
    {
        var sql = @"
                    SELECT purchaseid, betstatus, creationdate, updatedate, searchdatetime
                    FROM public.t_bti_bet_record
                    WHERE creationdate >= @creationdateStart 
                        AND creationdate <= @creationdateEnd
                        AND searchdatetime >= @startTime
                        AND searchdatetime <= @endTime;";

        var par = new DynamicParameters();
        // 不知BTI下注後 實際最久何時做結算 廠商不能保證多久內會結算 跟未來會不會發生兩日後結算的
        // 結算是當官方公布賽果，BTi收到結算資訊之後系統會進行結算 並不是有一個規定的時間
        // w1的話就先抓個往前30天吧...
        par.Add("@creationdateStart", start.AddDays(-30));
        par.Add("@creationdateEnd", end);
        par.Add("@startTime", start);
        par.Add("@endTime", end);

        await using var conn = new NpgsqlConnection(await PGRead);
        var result = await conn.QueryAsync<BTIRecordPrimaryKey>(sql, par);
        return result.ToList();
    }

    public async Task<List<GetBTIPreRecordDBResponse>> GetBTIRecordsPreAmountByPurchaseId(IDbTransaction tran, string purchaseId, DateTime creationdate)
    {
        var sql = @"
                    SELECT 
                         purchaseid, betstatus, creationdate, updatedate
                         , pre_totalstake
                         , pre_validstake
                         , pre_pl
                         , pre_return
                         , pre_betstatus
                    FROM public.t_bti_bet_record
                    WHERE creationdate = @creationdate
                        AND purchaseid = @purchaseId;";

        var par = new DynamicParameters();
        par.Add("@purchaseId", purchaseId);
        par.Add("@creationdate", creationdate);

        var result = await tran.Connection.QueryAsync<GetBTIPreRecordDBResponse>(sql, par, tran);
        return result.ToList();
    }

    public async Task<(int totalCount, decimal totalBetValid, decimal totalNetWin)> SumBTIBetRecordByCreationdate(DateTime start, DateTime end)
    {
        var sql = @"SELECT 
                    COUNT(Distinct purchaseid) AS totalCount
                    , CASE WHEN SUM(validstake) IS NULL THEN 0 ELSE SUM(validstake) END  AS totalBetValid
                    , CASE WHEN SUM(pl) IS NULL THEN 0 ELSE SUM(pl) END AS totalNetWin
                    FROM public.t_bti_bet_record
                    WHERE creationdate >= @startTime 
                        AND creationdate <= @endTime;";

        var par = new DynamicParameters();
        par.Add("@startTime", start);
        par.Add("@endTime", end);

        await using var conn = new NpgsqlConnection(await PGRead);
        var result = (await conn.QueryAsync<dynamic>(sql, par)).FirstOrDefault();
        return ((int)result.totalcount, (decimal)result.totalbetvalid, (decimal)result.totalnetwin);
    }
}