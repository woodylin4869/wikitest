using Dapper;
using H1_ThirdPartyWalletAPI.Model.Config;
using H1_ThirdPartyWalletAPI.Model.Game.RTG.Response;
using H1_ThirdPartyWalletAPI.Model.W1API;
using H1_ThirdPartyWalletAPI.Utility;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace H1_ThirdPartyWalletAPI.Service.Common.DB.Betlogs;

public interface IRtgDBService
{
    //public Task<dynamic> GetRtgRecord(string betid);
    public Task<int> PostRtgRecord(NpgsqlConnection conn, IDbTransaction tran, List<Record> record_data);
    public Task<IEnumerable<dynamic>> GetRtgRecordBySummary(GetBetRecordReq RecordReq);
    Task<dynamic> SumRtgBetRecordDaily(DateTime reportDate);
    Task<IEnumerable<Record>> GetRtgRecordByTime(DateTime startTime, DateTime endTime);
}

public class RtgDBService : BetlogsDBServiceBase, IRtgDBService
{
    public RtgDBService(ILogger<RtgDBService> logger, IBetLogsDbConnectionStringManager connectionStringManager) : base(logger, connectionStringManager)
    {
    }
    #region t_rtg_bet_record
    //public async Task<dynamic> GetRtgRecord(string recordid)
    //{
    //    var sql = @"SELECT * FROM t_rtg_bet_record
    //                    WHERE recordid = @recordid";

    //    var parameters = new DynamicParameters();
    //    parameters.Add("@recordid", long.Parse(recordid));

    //    using (var conn = new NpgsqlConnection(await PGRead))
    //    {
    //        return await conn.QuerySingleOrDefaultAsync<dynamic>(sql, parameters);
    //    }
    //}
    public async Task<int> PostRtgRecord(NpgsqlConnection conn, IDbTransaction tran, List<Record> record_data)
    {
        var sql = @"INSERT INTO public.t_rtg_bet_record
                        (
                            summary_id,
                            game_id,
                            recordid, 
                            jianghao, 
                            userid, 
                            bet, 
                            winlose, 
                            common, 
                            surplus, 
                            divided, 
                            recordtype, 
                            createtime,
                            settlementtime
                        )
	                    VALUES 
                        ( 
                            @summary_id,
                            @game_id,
                            @recordid, 
                            @jianghao, 
                            @userid, 
                            @bet, 
                            @winlose, 
                            @common, 
                            @surplus, 
                            @divided, 
                            @recordtype, 
                            @createtime,
                            @settlementtime
                        );";

        return await conn.ExecuteAsync(sql, record_data, tran);
    }
    public async Task<IEnumerable<dynamic>> GetRtgRecordBySummary(GetBetRecordReq RecordReq)
    {
        var sql = @"SELECT * FROM t_rtg_bet_record
                        WHERE summary_id = @summary_id 
                        AND settlementtime > @start_date
                        AND settlementtime <= @end_date";

        var parameters = new DynamicParameters();
        parameters.Add("@summary_id", Guid.Parse(RecordReq.summary_id));
        parameters.Add("@start_date", RecordReq.ReportTime.AddDays(-3));
        parameters.Add("@end_date", RecordReq.ReportTime.AddDays(1));

        using (var conn = new NpgsqlConnection(await PGRead))
        {
            return await conn.QueryAsync<dynamic>(sql, parameters);
        }
    }
    public async Task<IEnumerable<Record>> GetRtgRecordByTime(DateTime startTime, DateTime endTime)
    {
        var par = new DynamicParameters();
        string strSql = @"SELECT *
                    FROM t_rtg_bet_record
                    WHERE settlementtime BETWEEN @startTime and @endTime
                    ";
        par.Add("@startTime", startTime);
        par.Add("@endTime", endTime);
        using (NpgsqlConnection conn = new NpgsqlConnection(await PGRead))
        {
            return await conn.QueryAsync<Record>(strSql, par);
        }
    }
    public async Task<dynamic> SumRtgBetRecordDaily(DateTime reportDate)
    {
        var par = new DynamicParameters();
        string strSql = @"SELECT 
                    COUNT(*) AS total_cont
                    , SUM(bet) AS total_bet
                    , SUM(bet + winlose) AS total_win
                    FROM t_rtg_bet_record
                    WHERE settlementtime >= @startTime
                    AND settlementtime < @endTime
                    ";

        par.Add("@startTime", reportDate);
        par.Add("@endTime", reportDate.AddDays(1));
        using (NpgsqlConnection conn = new NpgsqlConnection(await PGRead))
        {
            return await conn.QueryAsync(strSql, par);
        }
    }
    #endregion
}