using Dapper;
using H1_ThirdPartyWalletAPI.Model.Config;
using H1_ThirdPartyWalletAPI.Model.DataModel;
using H1_ThirdPartyWalletAPI.Model.DB.JDB.Response;
using H1_ThirdPartyWalletAPI.Model.DB.RSG.Response;
using H1_ThirdPartyWalletAPI.Model.Game.JDB.Response;
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
using System.Threading;
using System.Threading.Tasks;

namespace H1_ThirdPartyWalletAPI.Service.Common.DB.Betlogs;

public interface IJdbDBService
{
    public Task<int> PostJdbRecord(NpgsqlConnection conn, IDbTransaction tran, List<CommonBetRecord> record_data);

    public Task<int> PostJdbReport(List<DaliyReportContent> report_data);

    public Task<int> DeleteJdbReport(List<DaliyReportContent> report_data);

    public Task<IEnumerable<GetJdbRecordByTimeResponse>> GetJdbRecordV2ByTime(DateTime startTime, DateTime endTime);

    public Task<IEnumerable<(int count, decimal win, decimal bet, decimal netwin, string userid, DateTime createtime)>> SummaryGameRecord(DateTime reportTime, DateTime startTime, DateTime endTime);

    public Task<dynamic> SumJdbBetReportDaily(DateTime reportDate);

    public Task<dynamic> SumJdbBetRecordDaily(DateTime reportDate);

    public Task<IEnumerable<GetJdbRecordBySummaryResponse>> GetJdbRecordByReportTime(DateTime start, DateTime end, DateTime reporttime, string playerid);
    public Task<IEnumerable<GetJdbRecordBySummaryResponse>> GetJdbRecordByReportTimeOld(BetRecordSummary RecordReq);

    public Task<GetJdbRecordResponse> GetJdbRecordByReportTime(DateTime reportTime, string historyid);
}

public class JdbDBService : BetlogsDBServiceBase, IJdbDBService
{
    public JdbDBService(ILogger<JdbDBService> logger, IBetLogsDbConnectionStringManager connectionStringManager) : base(logger, connectionStringManager)
    {
    }

    #region t_jdb_bet_record_v2

    /// <summary>
    /// 補單查詢遊戲單號(新表)
    /// </summary>
    /// <param name="startTime"></param>
    /// <param name="endTime"></param>
    /// <returns></returns>
    public async Task<IEnumerable<GetJdbRecordByTimeResponse>> GetJdbRecordV2ByTime(DateTime startTime, DateTime endTime)
    {
        var par = new DynamicParameters();
        string strSql = @"SELECT
                                    historyid
                                    , lastmodifytime
                    FROM t_jdb_bet_record_v2
                    WHERE lastmodifytime BETWEEN @startTime and @endTime
                    ";
        par.Add("@startTime", startTime);
        par.Add("@endTime", endTime);
        using (NpgsqlConnection conn = new NpgsqlConnection(await PGRead))
        {
            return await conn.QueryAsync<GetJdbRecordByTimeResponse>(strSql, par);
        }
    }

    public async Task<int> PostJdbRecord(NpgsqlConnection conn, IDbTransaction tran, List<CommonBetRecord> record_data)
    {
        #region old table

        //string stSqlInsert = @"INSERT INTO t_jdb_bet_record
        //    (
        //     sessionid,
        //     seqno,
        //     playerid,
        //     gtype,
        //     mtype,
        //     gamedate,
        //     bet,
        //     win,
        //     total,
        //     currency,
        //     denom,
        //     lastmodifytime,
        //     playerip,
        //     clienttype,
        //     gamblebet,
        //     jackpot,
        //     jackpotcontribute,
        //     hasfreegame,
        //     hasgamble,
        //     systemtakewin,
        //     roomtype,
        //     beforebalance,
        //     afterbalance,
        //     hasbonusgame,
        //     tax,
        //     validbet,
        //        summary_id,
        //        historyid
        //    )
        //    VALUES
        //    (
        //     @sessionid,
        //     @seqno,
        //     @playerid,
        //     @gtype,
        //     @mtype,
        //     @gamedate,
        //     @bet,
        //     @win,
        //     @total,
        //     @currency,
        //     @denom,
        //     @lastmodifytime,
        //     @playerip,
        //     @clienttype,
        //     @gamblebet,
        //     @jackpot,
        //     @jackpotcontribute,
        //     @hasfreegame,
        //     @hasgamble,
        //     @systemtakewin,
        //     @roomtype,
        //     @beforebalance,
        //     @afterbalance,
        //     @hasbonusgame,
        //     @tax,
        //     @validbet,
        //        @summary_id,
        //        @historyId
        //    )";

        #endregion old table

        // TODO 後匯總jdb更名

        string stSqlInsert = @"INSERT INTO t_jdb_bet_record_v2
            (
	            sessionid,
                seqno,
	            playerid,
	            gtype,
	            mtype,
	            gamedate,
	            bet,
	            win,
	            total,
	            currency,
	            denom,
	            lastmodifytime,
	            playerip,
	            clienttype,
	            gamblebet,
	            jackpot,
	            jackpotcontribute,
	            hasfreegame,
	            hasgamble,
	            systemtakewin,
	            roomtype,
	            beforebalance,
	            afterbalance,
	            hasbonusgame,
	            tax,
	            validbet,
                historyid,
                report_time
            )
            VALUES
            (
	            @sessionid,
                @seqno,
	            @playerid,
	            @gtype,
	            @mtype,
	            @gamedate,
	            @bet,
	            @win,
	            @total,
	            @currency,
	            @denom,
	            @lastmodifytime,
	            @playerip,
	            @clienttype,
	            @gamblebet,
	            @jackpot,
	            @jackpotcontribute,
	            @hasfreegame,
	            @hasgamble,
	            @systemtakewin,
	            @roomtype,
	            @beforebalance,
	            @afterbalance,
	            @hasbonusgame,
	            @tax,
	            @validbet,
                @historyId,
                @report_time
            )";

        return await conn.ExecuteAsync(stSqlInsert, record_data, tran);
    }

    public async Task<IEnumerable<(int count, decimal win, decimal bet, decimal netwin, string userid, DateTime createtime)>> SummaryGameRecord(DateTime reportTime, DateTime startTime, DateTime endTime)
    {
        // TODO 後匯總jdb更名
        var sql = @"SELECT CAST(count(1) AS INTEGER) AS count,
                        SUM(coalesce(win,0)) AS win,
                        SUM(coalesce(bet,0)) AS bet,
                        SUM(coalesce(total,0)) AS netwin,
                         Date(lastmodifytime) AS createtime,
                        playerid AS userid
                        FROM t_jdb_bet_record_v2
                        WHERE lastmodifytime BETWEEN @start_time AND @end_time
                        AND report_time = @report_time
                        GROUP BY playerid, Date(lastmodifytime)
                        ";

        var par = new DynamicParameters();
        par.Add("@start_time", startTime);
        par.Add("@end_time", endTime);
        par.Add("@report_time", reportTime);
        using (NpgsqlConnection conn = new NpgsqlConnection(await PGRead))
        {
            var result = await conn.QueryAsync(sql, par, commandTimeout: 270);
            return result.Select(x => ((int)x.count, (decimal)x.win, (decimal)x.bet, (decimal)x.netwin, (string)x.userid, (DateTime)x.createtime));
        }
    }

    /// <summary>
    /// 取得第二層明細
    /// </summary>
    /// <param name="RecordReq"></param>
    /// <returns></returns>
    public async Task<IEnumerable<GetJdbRecordBySummaryResponse>> GetJdbRecordByReportTimeOld(BetRecordSummary RecordReq)
    {
        var sql = @"SELECT
                        historyid as seqno
                        , historyid
                        , lastmodifytime
                        , mtype
                        , gamedate
                        , total
                    FROM t_jdb_bet_record_v2
                    WHERE lastmodifytime BETWEEN @starttime AND @endtime
                    AND report_time = @reporttime
                    AND playerid = @playerid";

        var parameters = new DynamicParameters();
        parameters.Add("@starttime", RecordReq.ReportDatetime.GetValueOrDefault().AddDays(-3));
        parameters.Add("@endtime", RecordReq.ReportDatetime.GetValueOrDefault().AddMinutes(5));
        parameters.Add("@reporttime", RecordReq.ReportDatetime);
        parameters.Add("@playerid", RecordReq.Club_id.ToLower()); // jdb吐會注單帳號會英文轉為全小寫

        using (var conn = new NpgsqlConnection(await PGRead))
        {
            return await conn.QueryAsync<GetJdbRecordBySummaryResponse>(sql, parameters);
        }
    }


    /// <summary>
    /// 依照BetRecordSummary、遊戲序號找到明細資料
    /// </summary>
    /// <param name="RecordReq"></param>
    /// <param name="historyid"></param>
    /// <returns></returns>
    public async Task<GetJdbRecordResponse> GetJdbRecordByReportTime(DateTime reportTime, string historyid)
    {
        var par = new DynamicParameters();
        string strSql = @"SELECT
                                    historyid
                                    , lastmodifytime
                                    , playerid
                                    , gtype
                    FROM t_jdb_bet_record_v2
                    WHERE lastmodifytime > @start_date
                    AND lastmodifytime < @end_date
                    AND report_time = @reporttime
                    AND historyid = @historyid
                    ";
        par.Add("@historyid", historyid);
        par.Add("@start_date", reportTime.AddDays(-3));
        par.Add("@end_date", reportTime.AddDays(1));
        par.Add("@reporttime", reportTime);
        using (NpgsqlConnection conn = new NpgsqlConnection(await PGRead))
        {
            return await conn.QuerySingleOrDefaultAsync<GetJdbRecordResponse>(strSql, par);
        }
    }

    /// <summary>
    /// 取得第二層明細
    /// </summary>
    /// <param name="RecordReq"></param>
    /// <returns></returns>
    public async Task<IEnumerable<GetJdbRecordBySummaryResponse>> GetJdbRecordByReportTime(DateTime start, DateTime end,DateTime reporttime,string playerid)
    {
        var sql = @"SELECT
                        historyid as seqno
                        , historyid
                        , lastmodifytime
                        , mtype
                        , gamedate
                        , bet
                        , total
                    FROM t_jdb_bet_record_v2
                    WHERE lastmodifytime BETWEEN @starttime AND @endtime
                    AND report_time = @reporttime
                    AND playerid = @playerid";

        var parameters = new DynamicParameters();
        parameters.Add("@starttime", start);
        parameters.Add("@endtime", end);
        parameters.Add("@reporttime", reporttime);
        parameters.Add("@playerid", playerid.ToLower()); // jdb吐會注單帳號會英文轉為全小寫

        using (var conn = new NpgsqlConnection(await PGRead))
        {
            return await conn.QueryAsync<GetJdbRecordBySummaryResponse>(sql, parameters);
        }
    }

    #endregion t_jdb_bet_record_v2

    #region t_jdb_game_report

    public async Task<int> PostJdbReport(List<DaliyReportContent> report_data)
    {
        await using (var conn = new NpgsqlConnection(PGMaster))
        {
            string stSqlInsert = @"INSERT INTO t_jdb_game_report
                (
	                uid,
	                bet,
	                win,
	                netwin,
	                jackpot,
	                jackpotcontribute,
	                count,
	                validbet,
	                tax,
                    financialdate,
                    gtype
                )
                VALUES
                (
	                @uid,
	                @bet,
	                @win,
	                @netwin,
	                @jackpot,
	                @jackpotcontribute,
	                @count,
	                @validbet,
	                @tax,
                    @financialdate,
                    @gtype
                )";
            return await conn.ExecuteAsync(stSqlInsert, report_data);
        }
    }

    public async Task<int> DeleteJdbReport(List<DaliyReportContent> report_data)
    {
        await using (var conn = new NpgsqlConnection(PGMaster))
        {
            string strSqlDel = @"DELETE FROM t_jdb_game_report
                               WHERE uid=@uid
                               AND financialdate = @financialdate
                               AND gtype = @gtype";
            return await conn.ExecuteAsync(strSqlDel, report_data);
        }
    }

    public async Task<dynamic> SumJdbBetReportDaily(DateTime reportDate)
    {
        var par = new DynamicParameters();
        string strSql = @"SELECT
                    SUM(bet) AS total_bet
                    , SUM(win) AS total_win
                    , SUM(netwin) AS total_netwin
                    , SUM(count) AS total_cont
                    FROM t_jdb_game_report
                    WHERE financialdate = @financialdate
                    ";

        par.Add("@financialdate", reportDate);
        using (NpgsqlConnection conn = new NpgsqlConnection(await PGRead))
        {
            return await conn.QueryAsync(strSql, par);
        }
    }

    public async Task<dynamic> SumJdbBetRecordDaily(DateTime reportDate)
    {
        //JDB切帳時間是中午12點
        reportDate = reportDate.AddHours(12);
        var par = new DynamicParameters();
        string strSql = @"SELECT
                    COUNT(*) AS total_cont
                    , SUM(coalesce(bet,0)) AS total_bet
                    , SUM(coalesce(win,0)) AS total_win
                    , SUM(coalesce(total,0)) AS total_netwin
                    FROM t_jdb_bet_record_v2
                    WHERE lastmodifytime >= @startTime
                    AND lastmodifytime < @endTime
                    ";

        par.Add("@startTime", reportDate);
        par.Add("@endTime", reportDate.AddDays(1));
        using (NpgsqlConnection conn = new NpgsqlConnection(await PGRead))
        {
            return await conn.QueryAsync(strSql, par);
        }
    }

    #endregion t_jdb_game_report
}