using Dapper;
using H1_ThirdPartyWalletAPI.Code;
using H1_ThirdPartyWalletAPI.Model.Config;
using H1_ThirdPartyWalletAPI.Model.DataModel;
using H1_ThirdPartyWalletAPI.Model.W1API;
using H1_ThirdPartyWalletAPI.Utility;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace H1_ThirdPartyWalletAPI.Service.Common.DB.Betlogs;

public interface IGameReportDBService
{
    #region t_game_report
    Task<IEnumerable<GameReport>> GetGameReport(Platform platform, string reportType, DateTime startTime, DateTime endTime, int page, int count);
    Task<List<GameReport>> GetGameReportSummary(Platform platform, string reportType, DateTime startTime, DateTime endTime);
    Task<IEnumerable<GameReport>> GetGameReport(long id);
    Task<int> PostGameReport(GameReport report_data);
    Task<int> DeleteGameReport(GameReport report_data);
    Task<int> DeleteGameReport(long id);
    Task<int> PutGameReport(long id, PutGameReportReq req);
    #endregion

}

public class GameReportDBService : BetlogsDBServiceBase, IGameReportDBService
{
    public GameReportDBService(ILogger<GameReportDBService> logger, IBetLogsDbConnectionStringManager connectionStringManager) : base(logger, connectionStringManager)
    {
    }
    #region t_game_report
    public async Task<IEnumerable<GameReport>> GetGameReport(long id)
    {
        var par = new DynamicParameters();
        string strSql = @"SELECT *
                    FROM t_game_report
                    WHERE id = @id";
        par.Add("@id", id);
        using (NpgsqlConnection conn = new NpgsqlConnection(await PGRead))
        {
            return await conn.QueryAsync<GameReport>(strSql, par);
        }
    }
    public async Task<IEnumerable<GameReport>> GetGameReport(Platform platform, string reportType, DateTime startTime, DateTime endTime, int page, int count)
    {
        var par = new DynamicParameters();
        string strSql = @"SELECT *
                    FROM t_game_report
                    WHERE report_datetime BETWEEN @startTime AND @endTime";

        if (platform != Platform.ALL)
        {
            strSql += " AND platform = @platform";
            par.Add("@platform", platform.ToString());
        }
        if (reportType != null)
        {
            strSql += " AND report_type = @report_type";
            par.Add("@report_type", int.Parse(reportType));
        }

        strSql += @" OFFSET @offset
                        LIMIT @limit";

        par.Add("@startTime", startTime);
        par.Add("@endTime", endTime);
        par.Add("@offset", page * count);
        par.Add("@limit", count);

        using (NpgsqlConnection conn = new NpgsqlConnection(await PGRead))
        {
            return await conn.QueryAsync<GameReport>(strSql, par);
        }
    }
    public async Task<List<GameReport>> GetGameReportSummary(Platform platform, string reportType, DateTime startTime, DateTime endTime)
    {
        var par = new DynamicParameters();
        string strSql = @"SELECT COUNT(id),
                            SUM(total_count) AS total_count,
                            SUM(total_bet) AS total_bet,
                            SUM(total_win) AS total_win,
                            SUM(total_netwin) AS total_netwin
                    FROM t_game_report
                    WHERE report_datetime BETWEEN @startTime AND @endTime";
        if (platform != Platform.ALL)
        {
            strSql += " AND platform = @platform";
            par.Add("@platform", platform.ToString());
        }
        if (reportType != null)
        {
            strSql += " AND report_type = @report_type";
            par.Add("@report_type", int.Parse(reportType));
        }
        par.Add("@startTime", startTime);
        par.Add("@endTime", endTime);
        using (NpgsqlConnection conn = new NpgsqlConnection(await PGRead))
        {
            var gameReports = await conn.QueryAsync<GameReport>(strSql, par);
            return gameReports.ToList();
        }
    }
    public async Task<int> PostGameReport(GameReport report_data)
    {
        await using (var conn = new NpgsqlConnection(PGMaster))
        {
            string stSqlInsert = @"INSERT INTO t_game_report
                (
	                platform,
	                report_datetime,
	                total_count,
	                total_bet,
	                total_win,
	                total_netwin,
	                report_type,
                    update_datetime
                )
                VALUES
                (
	                @platform,
	                @report_datetime,
	                @total_count,
	                @total_bet,
	                @total_win,
	                @total_netwin,
	                @report_type,
                    @update_datetime
                )";
            return await conn.ExecuteAsync(stSqlInsert, report_data);
        }
    }
    public async Task<int> DeleteGameReport(GameReport report_data)
    {
        await using (var conn = new NpgsqlConnection(PGMaster))
        {
            string strSqlDel = @"DELETE FROM t_game_report
                                    WHERE platform=@platform
                                    AND report_datetime=@report_datetime
                                    AND report_type = @report_type
                                    ";
            return await conn.ExecuteAsync(strSqlDel, report_data);
        }
    }
    public async Task<int> DeleteGameReport(long id)
    {
        await using (var conn = new NpgsqlConnection(PGMaster))
        {
            var par = new DynamicParameters();
            string strSqlDel = @"DELETE FROM t_game_report
                                    WHERE id=@id
                                    ";
            par.Add("@id", id);
            return await conn.ExecuteAsync(strSqlDel, par);
        }
    }
    public async Task<int> PutGameReport(long id, PutGameReportReq req)
    {
        var par = new DynamicParameters();
        var strSql = @"UPDATE t_game_report
                        SET report_datetime = @report_datetime, 
                        total_count = @total_count, 
                        total_bet = @total_bet, 
                        total_win = @total_win, 
                        total_netwin = @total_netwin
                        WHERE id = @id";
        par.Add("@report_datetime", req.ReportDateTime);
        par.Add("@total_count", req.TotalCount);
        par.Add("@total_bet", req.TotalBet);
        par.Add("@total_win", req.TotalWin);
        par.Add("@total_netwin", req.TotalNetwin);
        par.Add("@id", id);
        using (NpgsqlConnection conn = new NpgsqlConnection(PGMaster))
        {
            return await conn.ExecuteAsync(strSql, par);
        }
    }
    #endregion

}