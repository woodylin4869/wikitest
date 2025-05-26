using Dapper;
using H1_ThirdPartyWalletAPI.Model.ClickHouseDB;
using H1_ThirdPartyWalletAPI.Model.Config;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace H1_ThirdPartyWalletAPI.Service.Common.DB.ClickHouse;

public interface IBetSummaryReportDBService
{
    Task<IEnumerable<PlayerSummary>> GetPlayerBetSummary();
    Task<IEnumerable<PlayerSummaryDay>> GetPlayerSummaryDay(string clubID, string reportTime);
    Task<dynamic> HealthCheck();
}

public class BetSummaryReportDBService : ClickHouseDBService, IBetSummaryReportDBService
{
    public BetSummaryReportDBService(ClickHouseDBConnection config) : base(config)
    {
    }
    #region t_game_report

    public async Task<dynamic> HealthCheck()
    {
        string CommandText = @"select now()";
        return await ExecuteQuerySQLAsync<dynamic>(CommandText, null);
    }
    public async Task<IEnumerable<PlayerSummary>> GetPlayerBetSummary()
    {
        string CommandText = @"SELECT report_time, platform, game_id, club_id, total_count, BetAmount, WinAmount ,Win , LoseAmount, NetWinAmount, JackPot, update_datetime
FROM report.t_player_summary_day;";
        return await ExecuteQuerySQLAsync<PlayerSummary>(CommandText, null);
    }

    public async Task<IEnumerable<PlayerSummaryDay>> GetPlayerSummaryDay(string clubID, string reportTime)
    {
        var par = new DynamicParameters();

        string CommandText = @"SELECT
    ReportDate,
    Platform,
    GameId,
    upper(ClubId) AS ClubId,
    SUM(TotalCount) AS TotalCount,
    SUM(BetAmount) AS BetAmount,
    SUM(WinAmount) AS WinAmount,
    SUM(Win) AS Win,
    SUM(LoseAmount) AS LoseAmount,
    SUM(NetWinAmount) AS NetWinAmount,
    SUM(JackPot) AS JackPot
FROM
    Report.PlayerSummary_Day final
WHERE ReportDate = @ReportDate AND upper(ClubId) = upper(@ClubId)
GROUP BY
    ReportDate,
    Platform,
    GameId,
    ClubId;";

        par.Add("@ReportDate", reportTime); 
        par.Add("@ClubId", clubID);
        return await ExecuteQuerySQLAsync<PlayerSummaryDay>(CommandText, par);
    }
    #endregion

}