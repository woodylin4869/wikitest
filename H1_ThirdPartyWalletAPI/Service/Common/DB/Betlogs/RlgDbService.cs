using Dapper;
using H1_ThirdPartyWalletAPI.Model.Config;
using H1_ThirdPartyWalletAPI.Model.DB.RLG;
using H1_ThirdPartyWalletAPI.Model.DB.RLG.Response;
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
using static H1_ThirdPartyWalletAPI.Model.Game.RLG.Response.GetBetRecordResponse;

namespace H1_ThirdPartyWalletAPI.Service.Common.DB.Betlogs
{
    public interface IRlgDbService
    {
        Task<IEnumerable<GetRlgRunningRecordResponse>> GetRlgRunningRecord(GetBetRecordUnsettleReq RecordReq);
        Task<IList<RLGRecordPrimaryKey>> GetRlgRecordPrimaryKey(DateTime start, DateTime end);
        Task<IEnumerable<GetRlgRecordByOrderResponse>> GetRlgRecordByOrder(NpgsqlConnection conn, IDbTransaction tran, GetBetRecordResponseDataList record_data);
        Task<IEnumerable<GetRlgRecordByOrderResponse>> GetRlgRecordV2ByOrder(NpgsqlConnection conn, IDbTransaction tran, GetBetRecordResponseDataList record_data);
        Task<IEnumerable<GetRlgRecordBySummaryResponse>> GetRlgRecordBySummary(string summary_id, DateTime createTime);
        Task<int> DeleteRlgRunningRecord(NpgsqlConnection conn, IDbTransaction tran, GetBetRecordResponseDataList record_data);
        Task<int> PostRlgRecord(NpgsqlConnection conn, IDbTransaction tran, IEnumerable<GetBetRecordResponseDataList> betLogs);
        Task<int> PostRlgRunningRecord(NpgsqlConnection conn, IDbTransaction tran, IEnumerable<GetBetRecordResponseDataList> betLogs);
        Task<dynamic> SumRlgBetRecordHourly(DateTime reportDate);
        Task<IEnumerable<GetRlgRecordBySummaryResponse>> GetRlgV2RecordBySummary(DateTime createtime, DateTime report_time, string club_id);

        Task<IEnumerable<(int count, decimal win, decimal bet, decimal jackpot, string userid, int game_type, DateTime createtime)>>
          SummaryGameRecord(DateTime reportTime, DateTime startTime, DateTime endTime);
    }

    public class RlgDbService : BetlogsDBServiceBase, IRlgDbService
    {

        #region t_rlg_bet_record + t_rlg_bet_running_record

        public RlgDbService(ILogger<RlgDbService> logger, IBetLogsDbConnectionStringManager connectionStringManager) : base(logger, connectionStringManager)
        {
        }

        public async Task<IEnumerable<GetRlgRunningRecordResponse>> GetRlgRunningRecord(GetBetRecordUnsettleReq RecordReq)
        {
            var par = new DynamicParameters();
            string strSql = @"SELECT ordernumber
                                , createtime
                                , drawtime
                                , gamecode
                                , totalamount
                                , bettingbalance
                                , status
                                , numberofperiod
                                , odds
                                , gameplaycode
                                , contentcode
                                , club_id
                                , franchiser_id
                    FROM t_rlg_bet_record_running
                    WHERE createtime BETWEEN @StartTime AND @EndTime
                    ";

            if (RecordReq.Club_id != null)
            {
                par.Add("@Club_id", RecordReq.Club_id);
                strSql += " AND Club_id = @Club_id";
            }
            if (RecordReq.Franchiser_id != null)
            {
                par.Add("@Franchiser_id", RecordReq.Franchiser_id);
                strSql += " AND Franchiser_id = @Franchiser_id";
            }
            par.Add("@StartTime", RecordReq.StartTime != null ? RecordReq.StartTime : DateTime.Now.AddDays(-100));
            par.Add("@EndTime", RecordReq.EndTime != null ? RecordReq.EndTime : DateTime.Now);
            using (NpgsqlConnection conn = new NpgsqlConnection(await PGRead))
            {
                return await conn.QueryAsync<GetRlgRunningRecordResponse>(strSql, par);
            }
        }

        public async Task<IList<RLGRecordPrimaryKey>> GetRlgRecordPrimaryKey(DateTime start, DateTime end)
        {
            string strSql = @"SELECT ordernumber
                                , createtime
                                , drawtime
                    FROM t_rlg_bet_record
                    WHERE createtime >= @start_date
                    AND createtime < @end_date";
            var parameters = new DynamicParameters();
            parameters.Add("@start_date", start);
            parameters.Add("@end_date", end);

            using var conn = new NpgsqlConnection(await PGRead);
            return (await conn.QueryAsync<RLGRecordPrimaryKey>(strSql, parameters)).ToList();
        }

        public async Task<IEnumerable<GetRlgRecordByOrderResponse>> GetRlgRecordByOrder(NpgsqlConnection conn, IDbTransaction tran, GetBetRecordResponseDataList record_data)
        {
            var par = new DynamicParameters();
            string strSql = @"SELECT ordernumber
                                , createtime
                                , drawtime
                                , pre_totalamount
                                , pre_bettingbalance
                    FROM t_rlg_bet_record
                    WHERE ordernumber = @OrderNumber
                        AND createtime = @Createtime";

            par.Add("@OrderNumber", record_data.ordernumber);
            par.Add("@Createtime", record_data.createtime);

            return await conn.QueryAsync<GetRlgRecordByOrderResponse>(strSql, par);
        }
        public async Task<IEnumerable<GetRlgRecordByOrderResponse>> GetRlgRecordV2ByOrder(NpgsqlConnection conn, IDbTransaction tran, GetBetRecordResponseDataList record_data)
        {
            var par = new DynamicParameters();
            string strSql = @"SELECT ordernumber
                                , createtime
                                , drawtime
                                , pre_totalamount
                                , pre_bettingbalance
                    FROM t_rlg_bet_record_v2
                    WHERE ordernumber = @OrderNumber
                        AND partition_time = @partition_time";

            par.Add("@OrderNumber", record_data.ordernumber);
            par.Add("@partition_time", record_data.partition_time);

            return await conn.QueryAsync<GetRlgRecordByOrderResponse>(strSql, par);
        }
        public async Task<IEnumerable<GetRlgRecordBySummaryResponse>> GetRlgRecordBySummary(string summary_id, DateTime createTime)
        {

            string strSql = @"SELECT ordernumber
                                , createtime
                                , drawtime
                                , gamecode
                                , totalamount
                                , bettingbalance
                                , status
                                , numberofperiod
                                , odds
                                , gameplaycode
                                , contentcode
                            FROM t_rlg_bet_record
                           WHERE summary_id = @summary_id 
                           AND createtime >= @start_date
                            AND createtime < @end_date
                    ";

            var parameters = new DynamicParameters();
            parameters.Add("@summary_id", Guid.Parse(summary_id));

            parameters.Add("@start_date", createTime);
            parameters.Add("@end_date", createTime.AddDays(1));

            using (var conn = new NpgsqlConnection(await PGRead))
            {
                return await conn.QueryAsync<GetRlgRecordBySummaryResponse>(strSql, parameters);
            }
        }

        public async Task<int> DeleteRlgRunningRecord(NpgsqlConnection conn, IDbTransaction tran, GetBetRecordResponseDataList record_data)
        {
            string strSqlDel = @"DELETE FROM t_rlg_bet_record_running
                               WHERE OrderNumber=@OrderNumber";
            return await conn.ExecuteAsync(strSqlDel, record_data, tran);
        }

      

        public async Task<dynamic> SumRlgBetRecordHourly(DateTime reportDate)
        {
            var par = new DynamicParameters();
            string strSql = @"SELECT 
                    COUNT(1) AS total_cont
                    , SUM(pre_totalamount) AS total_bet
                    , SUM(pre_bettingbalance) AS total_win
                    FROM t_rlg_bet_record
                    WHERE  createtime >= (@startTime - interval '7 days') and createtime < @endTime
                    AND drawtime >= @startTime AND  drawtime < @endTime
                    AND status != 0
                    ";

            par.Add("@startTime", reportDate);
            par.Add("@endTime", reportDate.AddHours(1));
            using (NpgsqlConnection conn = new NpgsqlConnection(await PGRead))
            {
                return await conn.QueryAsync(strSql, par);
            }
        }
        #endregion



        #region V2
        public async Task<int> PostRlgRecord(NpgsqlConnection conn, IDbTransaction tran, IEnumerable<GetBetRecordResponseDataList> betLogs)
        {
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
            var sql = $@"CREATE TEMPORARY TABLE temp_t_rlg_bet_record_v2_{tableGuid:N} 
                            ( LIKE t_rlg_bet_record_v2  INCLUDING ALL );";
            return tran.Connection.ExecuteAsync(sql, transaction: tran);
        }
        private async Task<ulong> BulkInsertToTempTable(IDbTransaction tran, Guid tableGuid, IEnumerable<GetBetRecordResponseDataList> records)
        {
            if (tran is not NpgsqlTransaction npTran)
                throw new ArgumentException("Must be NpgsqlTransaction", nameof(tran));

            if (npTran.Connection == null)
                throw new ArgumentNullException(nameof(tran.Connection));

            await using var writer =
                await npTran.Connection.BeginBinaryImportAsync(
                    $@"COPY temp_t_rlg_bet_record_v2_{tableGuid:N} (userid, ordernumber, numberofperiod, gamecode, gamename, gamegroupcode, gamegroupname, betnumber, odds, ""content"",
                               totalamount, gameplayname,createtime,status, bettingbalance, totalkickback, agpdamount, ""result"", drawtime, winningstatus,
                               isadjust, device, pre_totalamount, pre_bettingbalance, gameplaycode, contentcode, report_time, partition_time) FROM STDIN (FORMAT BINARY)");

            foreach (var mapping in records)
            {
                await writer.StartRowAsync();
                await writer.WriteAsync(mapping.userid, NpgsqlDbType.Varchar); // varchar(20)
                await writer.WriteAsync(mapping.ordernumber, NpgsqlDbType.Varchar); // varchar(50)
                await writer.WriteAsync(mapping.numberofperiod, NpgsqlDbType.Varchar); // varchar(20)
                await writer.WriteAsync(mapping.gamecode, NpgsqlDbType.Varchar); // varchar(50)
                await writer.WriteAsync(mapping.gamename, NpgsqlDbType.Varchar); // varchar(50)
                await writer.WriteAsync(mapping.gamegroupcode, NpgsqlDbType.Varchar); // varchar(50)
                await writer.WriteAsync(mapping.gamegroupname, NpgsqlDbType.Varchar); // varchar(50)
                await writer.WriteAsync(mapping.betnumber, NpgsqlDbType.Varchar); // varchar(10)
                await writer.WriteAsync(mapping.odds.ToString(), NpgsqlDbType.Varchar); // varchar(10)
                await writer.WriteAsync(mapping.content, NpgsqlDbType.Varchar); // varchar(10)
                await writer.WriteAsync(mapping.totalamount, NpgsqlDbType.Numeric); // numeric(30, 4)
                await writer.WriteAsync(mapping.gameplayname, NpgsqlDbType.Varchar); // varchar(15)
                await writer.WriteAsync(mapping.createtime, NpgsqlDbType.Timestamp); // timestamp
                await writer.WriteAsync(mapping.status, NpgsqlDbType.Smallint); // int2
                await writer.WriteAsync(mapping.bettingbalance, NpgsqlDbType.Numeric); // numeric(30, 4)
                await writer.WriteAsync(mapping.totalkickback, NpgsqlDbType.Varchar); // varchar(20)
                await writer.WriteAsync(mapping.agpdamount, NpgsqlDbType.Varchar); // varchar(20)
                await writer.WriteAsync(mapping.result, NpgsqlDbType.Varchar); // varchar(200)
                await writer.WriteAsync(mapping.drawtime, NpgsqlDbType.Timestamp); // timestamp
                await writer.WriteAsync(mapping.winningstatus, NpgsqlDbType.Varchar); // varchar(5)
                await writer.WriteAsync(mapping.isadjust, NpgsqlDbType.Varchar); // varchar(5)
                await writer.WriteAsync(mapping.device, NpgsqlDbType.Varchar); // varchar(5)
                await writer.WriteAsync(mapping.pre_totalamount, NpgsqlDbType.Numeric); // numeric(30, 4)
                await writer.WriteAsync(mapping.pre_bettingbalance, NpgsqlDbType.Numeric); // numeric(30, 4)
                await writer.WriteAsync(mapping.gameplaycode, NpgsqlDbType.Varchar); // varchar(15)
                await writer.WriteAsync(mapping.contentcode, NpgsqlDbType.Varchar); // varchar(10)
                await writer.WriteAsync(mapping.report_time, NpgsqlDbType.Timestamp); // timestamp
                await writer.WriteAsync(mapping.partition_time, NpgsqlDbType.Timestamp); // timestamp
            }

            return await writer.CompleteAsync();
        }
        private async Task<int> MergeFromTempTable(IDbTransaction tran, Guid tableGuid)
        {
            var sql = @$"INSERT INTO t_rlg_bet_record_v2
                        SELECT userid, ordernumber, numberofperiod, gamecode, gamename, gamegroupcode, gamegroupname, betnumber, odds, ""content"",
                               totalamount, gameplayname,createtime, status, bettingbalance, totalkickback, agpdamount, ""result"", drawtime, winningstatus,
                               isadjust, device, pre_totalamount, pre_bettingbalance, gameplaycode, contentcode, create_time, report_time, partition_time
                        FROM temp_t_rlg_bet_record_v2_{tableGuid:N} AS  temp
                        WHERE NOT EXISTS 
                        (
                            SELECT NULL
                            FROM public.t_rlg_bet_record_v2
                            WHERE partition_time = temp.partition_time 
                                AND ordernumber = temp.ordernumber 
                                AND drawtime = temp.drawtime 
                        )";
            return await tran.Connection.ExecuteAsync(sql, tran);
        }

        public async Task<int> PostRlgRunningRecord(NpgsqlConnection conn, IDbTransaction tran, IEnumerable<GetBetRecordResponseDataList> betLogs)
        {
            if (tran == null) throw new ArgumentNullException(nameof(tran));
            if (betLogs == null) throw new ArgumentNullException(nameof(betLogs));
            if (!betLogs.Any()) return 0;

            var tableGuid = Guid.NewGuid();
            //建立暫存表
            await CreateBetRecordRunningTempTable(tran, tableGuid);
            //將資料倒進暫存表
            await BulkInsertToRunningTempTable(tran, tableGuid, betLogs);
            //將資料由暫存表倒回主表(過濾重複)
            return await MergeFromRunningTempTable(tran, tableGuid);
        }
        private Task<int> CreateBetRecordRunningTempTable(IDbTransaction tran, Guid tableGuid)
        {
            var sql = $@"CREATE TEMPORARY TABLE temp_t_rlg_bet_record_running_{tableGuid:N} 
                            ( LIKE t_rlg_bet_record_running  INCLUDING ALL );";
            return tran.Connection.ExecuteAsync(sql, transaction: tran);
        }
        private async Task<ulong> BulkInsertToRunningTempTable(IDbTransaction tran, Guid tableGuid, IEnumerable<GetBetRecordResponseDataList> records)
        {
            if (tran is not NpgsqlTransaction npTran)
                throw new ArgumentException("Must be NpgsqlTransaction", nameof(tran));

            if (npTran.Connection == null)
                throw new ArgumentNullException(nameof(tran.Connection));

            await using var writer =
                await npTran.Connection.BeginBinaryImportAsync(
                    $@"COPY temp_t_rlg_bet_record_running_{tableGuid:N} (userid, ordernumber, numberofperiod, gamecode, gamename, gamegroupcode, gamegroupname, 
                                    betnumber, odds, ""content"", totalamount, gameplayname, createtime, status, bettingbalance, totalkickback, agpdamount, ""result"",
                                    drawtime, winningstatus, isadjust, device, club_id, franchiser_id, summary_id, gameplaycode, contentcode
                            ) FROM STDIN (FORMAT BINARY)");

            foreach (var mapping in records)
            {
                await writer.StartRowAsync();

                // 依照 DDL 来选择正确的数据类型
                await writer.WriteAsync(mapping.userid, NpgsqlDbType.Varchar); // varchar(20)
                await writer.WriteAsync(mapping.ordernumber, NpgsqlDbType.Varchar); // varchar(50)
                await writer.WriteAsync(mapping.numberofperiod, NpgsqlDbType.Varchar); // varchar(20)
                await writer.WriteAsync(mapping.gamecode, NpgsqlDbType.Varchar); // varchar(50)
                await writer.WriteAsync(mapping.gamename, NpgsqlDbType.Varchar); // varchar(50)
                await writer.WriteAsync(mapping.gamegroupcode, NpgsqlDbType.Varchar); // varchar(50)
                await writer.WriteAsync(mapping.gamegroupname, NpgsqlDbType.Varchar); // varchar(50)
                await writer.WriteAsync(mapping.betnumber, NpgsqlDbType.Varchar); // varchar(10)
                await writer.WriteAsync(mapping.odds.ToString(), NpgsqlDbType.Varchar); // varchar(10)
                await writer.WriteAsync(mapping.content, NpgsqlDbType.Varchar); // varchar(10)
                await writer.WriteAsync(mapping.totalamount.ToString(), NpgsqlDbType.Varchar); // varchar(10) 需要注意: 数据库定义为 varchar(10)，但原代码使用了 numeric
                await writer.WriteAsync(mapping.gameplayname, NpgsqlDbType.Varchar); // varchar(15)
                await writer.WriteAsync(mapping.createtime, NpgsqlDbType.Timestamp); // timestamp
                await writer.WriteAsync(mapping.status, NpgsqlDbType.Integer); // int4
                await writer.WriteAsync(mapping.bettingbalance.ToString(), NpgsqlDbType.Varchar); // varchar(10) 需要注意: 数据库定义为 varchar(10)，但原代码使用了 numeric
                await writer.WriteAsync(mapping.totalkickback, NpgsqlDbType.Varchar); // varchar(20)
                await writer.WriteAsync(mapping.agpdamount, NpgsqlDbType.Varchar); // varchar(20)
                await writer.WriteAsync(mapping.result, NpgsqlDbType.Varchar); // varchar(200)
                await writer.WriteAsync(mapping.drawtime, NpgsqlDbType.Timestamp); // timestamp
                await writer.WriteAsync(mapping.winningstatus, NpgsqlDbType.Varchar); // varchar(5)
                await writer.WriteAsync(mapping.isadjust, NpgsqlDbType.Varchar); // varchar(5)
                await writer.WriteAsync(mapping.device, NpgsqlDbType.Varchar); // varchar(5)
                await writer.WriteAsync(mapping.club_id, NpgsqlDbType.Varchar); // varchar(20)
                await writer.WriteAsync(mapping.franchiser_id, NpgsqlDbType.Varchar); // varchar(20)
                await writer.WriteAsync(mapping.summary_id, NpgsqlDbType.Uuid); // uuid
                await writer.WriteAsync(mapping.gameplaycode, NpgsqlDbType.Varchar); // varchar(15)
                await writer.WriteAsync(mapping.contentcode, NpgsqlDbType.Varchar); // varchar(10)
            }

            return await writer.CompleteAsync();
        }
        private async Task<int> MergeFromRunningTempTable(IDbTransaction tran, Guid tableGuid)
        {
            var sql = @$"INSERT INTO t_rlg_bet_record_running
                        SELECT userid, ordernumber, numberofperiod, gamecode, gamename, gamegroupcode, gamegroupname, betnumber, odds, ""content"", totalamount,
                               gameplayname, createtime, status, bettingbalance, totalkickback, agpdamount, ""result"", drawtime, winningstatus, isadjust, device,
                               club_id, franchiser_id, summary_id, gameplaycode, contentcode
                        FROM temp_t_rlg_bet_record_running_{tableGuid:N} AS  temp
                        WHERE NOT EXISTS 
                        (
                            SELECT NULL
                            FROM public.t_rlg_bet_record_running
                            WHERE ordernumber = temp.ordernumber 
                                AND createtime = temp.createtime 
                        )";
            return await tran.Connection.ExecuteAsync(sql, tran);
        }
        /// <summary>
        /// 五分鐘會總
        /// </summary>
        /// <param name="reportTime"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        public async Task<IEnumerable<(int count, decimal win, decimal bet, decimal jackpot, string userid, int game_type, DateTime createtime)>> SummaryGameRecord(DateTime reportTime, DateTime startTime, DateTime endTime)
        {
            var sql = @"SELECT CAST(count(1) AS INTEGER) AS count,
                        SUM(BettingBalance) AS win,
                        SUM(TotalAmount) AS bet,
                        0 as jackpot,
                        userid as userid, 
                        0 as game_type,
                        Date(createtime) as createtime
                        FROM t_rlg_bet_record_v2
                        WHERE partition_time BETWEEN @start_time AND @end_time
                        AND report_time = @report_time
                        GROUP BY userid,Date(createtime)
                        ";

            var par = new DynamicParameters();
            par.Add("@start_time", startTime.AddDays(-2));
            par.Add("@end_time", endTime);
            par.Add("@report_time", reportTime);
            using (NpgsqlConnection conn = new NpgsqlConnection(await PGRead))
            {
                var result = await conn.QueryAsync(sql, par, commandTimeout: 270);
                var a = result.Select(x => ((int)x.count, (decimal)x.win, (decimal)x.bet, (decimal)x.jackpot, (string)x.userid, (int)x.game_type, (DateTime)x.createtime)).ToList();
                return a;
            }
        }

        public async Task<IEnumerable<GetRlgRecordBySummaryResponse>> GetRlgV2RecordBySummary(DateTime createtime, DateTime report_time, string club_id)
        {

            string strSql = @"SELECT ordernumber
                                , createtime
                                , drawtime
                                , gamecode
                                , totalamount
                                , bettingbalance
                                , status
                                , numberofperiod
                                , odds
                                , gameplaycode
                                , contentcode
                            FROM t_rlg_bet_record_v2
                           WHERE  partition_time BETWEEN @starttime AND @endtime
                                    AND report_time = @reporttime
                                    AND userid=@club_id";

            var parameters = new DynamicParameters();
            parameters.Add("@starttime", createtime);
            parameters.Add("@endtime", createtime.AddDays(1).AddMilliseconds(-1));
            parameters.Add("@reporttime", report_time);
            parameters.Add("@club_id", club_id);

            using (var conn = new NpgsqlConnection(await PGRead))
            {
                return await conn.QueryAsync<GetRlgRecordBySummaryResponse>(strSql, parameters);
            }
        }
        #endregion V2
    }
}
