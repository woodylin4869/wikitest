using Dapper;
using H1_ThirdPartyWalletAPI.Model.Config;
using H1_ThirdPartyWalletAPI.Model.DataModel;
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

public interface IPgDBService
{
    public Task<IEnumerable<dynamic>> GetPgRecord(string betid, DateTime ReportTime);
    Task<IEnumerable<dynamic>> GetPgRecordByPlayer(string playerName, DateTime startTime);
    public Task<int> PostPgRecord(NpgsqlConnection conn, IDbTransaction tran, List<Model.Game.PG.Response.GetHistoryResponse.Data> record_data);
    public Task<IEnumerable<dynamic>> GetPgRecordBySummary(GetBetRecordReq RecordReq);
    public Task<int> PostPgReport(t_pg_game_report report_data);
    public Task<int> DeletePgReport(t_pg_game_report report_data);
    Task<List<t_pg_bet_record>> SumPgBetRecord(DateTime startDateTime, DateTime endDateTime);
    Task<dynamic> SumPgBetRecordHourly(DateTime reportDate);
}

public class PgDBService : BetlogsDBServiceBase, IPgDBService
{
    public PgDBService(ILogger<PgDBService> logger, IBetLogsDbConnectionStringManager connectionStringManager) : base(logger, connectionStringManager)
    {
    }
    #region t_pg_bet_record
    public async Task<IEnumerable<dynamic>> GetPgRecord(string betid, DateTime ReportTime)
    {
        var par = new DynamicParameters();
        string strSql = @"SELECT *
                    FROM t_pg_bet_record
                    WHERE betid = @betid
                    AND betendtime > @start_date
                    AND betendtime < @end_date
                    ";
        par.Add("@betid", Int64.Parse(betid));
        par.Add("@start_date", ReportTime.AddDays(-3));
        par.Add("@end_date", ReportTime.AddDays(1));
        using (NpgsqlConnection conn = new NpgsqlConnection(await PGRead))
        {
            return await conn.QueryAsync<dynamic>(strSql, par);
        }
    }
    public async Task<IEnumerable<dynamic>> GetPgRecordByPlayer(string playerName, DateTime startTime)
    {
        var par = new DynamicParameters();
        string strSql = @"SELECT *
                    FROM t_pg_bet_record
                    WHERE playername = @playername
                    AND betendtime >= @startTime
                    ";
        par.Add("@playername", playerName);
        par.Add("@startTime", startTime);
        using (NpgsqlConnection conn = new NpgsqlConnection(await PGRead))
        {
            return await conn.QueryAsync<dynamic>(strSql, par);
        }
    }
    public async Task<int> PostPgRecord(NpgsqlConnection conn, IDbTransaction tran, List<Model.Game.PG.Response.GetHistoryResponse.Data> record_data)
    {
        string stSqlInsert = @"INSERT INTO t_pg_bet_record
            (
                summary_id,
                parentbetid,
                betid,
                playername,
                gameid,
                bettype,
                transactiontype,
                platform,
                currency,
                betamount,
                winamount,
                jackpotrtpcontributionamount,
                jackpotcontributionamount,
                jackpotwinamount,
                balancebefore,
                balanceafter,
                handsstatus,
                rowversion,
                bettime,
                betendtime,
                isfeaturebuy
            )
            VALUES
            (
                @summary_id,
                @parentbetid,
                @betid,
                @playername,
                @gameid,
                @bettype,
                @transactiontype,
                @platform,
                @currency,
                @betamount,
                @winamount,
                @jackpotrtpcontributionamount,
                @jackpotcontributionamount,
                @jackpotwinamount,
                @balancebefore,
                @balanceafter,
                @handsstatus,
                to_timestamp(@rowversion/1000.0),
                to_timestamp(@bettime/1000.0),
                to_timestamp(@betendtime/1000.0),
                @isfeaturebuy
            )";
        return await conn.ExecuteAsync(stSqlInsert, record_data, tran);
    }
    public async Task<IEnumerable<dynamic>> GetPgRecordBySummary(GetBetRecordReq RecordReq)
    {
        var par = new DynamicParameters();
        string strSql = @"SELECT *
                    FROM t_pg_bet_record
                    WHERE summary_id = @summary_id
                    AND betendtime > @start_date
                    AND betendtime < @end_date
                    ";
        par.Add("@summary_id", Guid.Parse(RecordReq.summary_id));
        par.Add("@start_date", RecordReq.ReportTime.AddDays(-3));
        par.Add("@end_date", RecordReq.ReportTime.AddDays(1));

        using (NpgsqlConnection conn = new NpgsqlConnection(await PGRead))
        {
            return await conn.QueryAsync<dynamic>(strSql, par);
        }
    }
    public async Task<List<t_pg_bet_record>> SumPgBetRecord(DateTime startDateTime, DateTime endDateTime)
    {
        var sql = @"
                    SELECT * FROM t_pg_bet_record
                    WHERE betendtime >= @startTime 
                        AND betendtime < @endTime ";

        var par = new DynamicParameters();
        par.Add("@startTime", startDateTime);
        par.Add("@endTime", endDateTime);

        using (var conn = new NpgsqlConnection(await PGRead))
        {
            var result = await conn.QueryAsync<t_pg_bet_record>(sql, par);
            return result.ToList();
        }
    }
    public async Task<dynamic> SumPgBetRecordHourly(DateTime reportDate)
    {
        var par = new DynamicParameters();
        string strSql = @"SELECT 
                    COUNT(*) AS total_cont
                    , SUM(betamount) AS total_bet
                    , SUM(winamount) AS total_win
                    FROM t_pg_bet_record
                    WHERE betendtime >= @startTime
                    AND betendtime < @endTime
                    ";

        par.Add("@startTime", reportDate);
        par.Add("@endTime", reportDate.AddHours(1));
        using (NpgsqlConnection conn = new NpgsqlConnection(await PGRead))
        {
            return await conn.QueryAsync(strSql, par);
        }
    }
    #endregion

    #region t_pg_game_report
    public async Task<int> PostPgReport(t_pg_game_report report_data)
    {
        await using (var conn = new NpgsqlConnection(PGMaster))
        {
            string stSqlInsert = @"INSERT INTO t_pg_game_report
                (
	                datetime,
	                totalhands,
	                currency,
	                totalbetamount,
	                totalwinamount,
	                totalplayerwinlossamount,
	                totalcompanywinlossamount,
	                transactiontype,
                    totalcollapsespincount,
                    totalcollapsefreespincount
                )
                VALUES
                (
	                @datetime,
	                @totalhands,
	                @currency,
	                @totalbetamount,
	                @totalwinamount,
	                @totalplayerwinlossamount,
	                @totalcompanywinlossamount,
	                @transactiontype,
                    @totalcollapsespincount,
                    @totalcollapsefreespincount
                )";
            return await conn.ExecuteAsync(stSqlInsert, report_data);
        }
    }
    public async Task<int> DeletePgReport(t_pg_game_report report_data)
    {
        await using (var conn = new NpgsqlConnection(PGMaster))
        {
            string strSqlDel = @"DELETE FROM t_pg_game_report
                               WHERE datetime=@datetime ";
            return await conn.ExecuteAsync(strSqlDel, report_data);
        }
    }
    #endregion
}