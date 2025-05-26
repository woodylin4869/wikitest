using Dapper;
using H1_ThirdPartyWalletAPI.Model.Config;
using H1_ThirdPartyWalletAPI.Model.DataModel;
using H1_ThirdPartyWalletAPI.Model.W1API;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using NpgsqlTypes;
using H1_ThirdPartyWalletAPI.Utility;

namespace H1_ThirdPartyWalletAPI.Service.Common.DB.Betlogs
{
    public interface ISummaryDBService
    {
        Task<IEnumerable<BetRecordSummary>> GetRecordSummary(GetBetSummaryReq SummaryRecordReq);
        Task<BetRecordSummary> GetRecordSummaryById(GetBetRecordReq SummaryRecordReq);
        Task<dynamic> GetBetRecordSummary(GetBetSummary_SummaryReq req);
        Task<IEnumerable<BetRecordSummary>> GetRecordSummaryBySession(GetBetSummaryReq SummaryRecordReq);
        Task<IEnumerable<dynamic>> GetRecordSummaryLock(NpgsqlConnection conn, IDbTransaction tran, BetRecordSummary recordData);
        Task<IEnumerable<BetRecordSummary>> GetBetRecordSummaryById(Guid id);
        Task<IEnumerable<BetRecordSummary>> GetBetRecordSummaryByIdLock(NpgsqlConnection conn, IDbTransaction tran, Guid id);
        Task<int> PutBetRecordSummary(NpgsqlConnection conn, IDbTransaction tran, BetRecordSummary RecordSummary);
        Task<int> DeleteBetRecordSummaryById(Guid id);
        Task<int> PostRecordSummaryNoDel(NpgsqlConnection conn, IDbTransaction tran, List<BetRecordSummary> summaryData);
        Task<int> PostRecordSummary(NpgsqlConnection conn, IDbTransaction tran, List<BetRecordSummary> summaryData);
        Task<int> PostSummaryBetRecordMapping(IDbTransaction tran, IEnumerable<t_summary_bet_record_mapping> mappings);
        Task<DateTime[]> GetPartitionTime(Guid summaryId, DateTime ReportTime);
        Task<t_summary_bet_record_mapping[]> GetSummaryMappings(IDbTransaction tran, Guid summaryId, DateTime ReportTime);
        Task<IEnumerable<string>> GetActiveClubIdByRecordSummary(DateTime starTime, DateTime entTime);
        Task<HashSet<string>> GetInactiveClubIdByRecordSummary(DateTime checkTime, int limitSize);
        Task<ulong> BatchInsertRecordSummaryAsync(NpgsqlConnection conn, List<BetRecordSummary> summaryData);
        Task<int> BulkInsertSummaryBetRecordMapping(IDbTransaction tran, IEnumerable<t_summary_bet_record_mapping> mappings);
    }

    public class SummaryDBService : BetlogsDBServiceBase, ISummaryDBService
    {
        public SummaryDBService(ILogger<SummaryDBService> logger, IBetLogsDbConnectionStringManager connectionStringManager) : base(logger, connectionStringManager)
        {
        }

        public async Task<IEnumerable<BetRecordSummary>> GetRecordSummary(GetBetSummaryReq SummaryRecordReq)
        {
            var par = new DynamicParameters();
            string strSql = @"SELECT id
                                    , club_id
                                    , game_id
                                    , game_type
                                    , bet_type
                                    , bet_amount
                                    , turnover
                                    , win
                                    , netwin
                                    , reportdatetime
                                    , currency
                                    , franchiser_id
                                    , recordcount
                                    , updatedatetime
                                    , jackpotwin
                            FROM t_bet_record_summary";
            if (SummaryRecordReq.SearchType == 1)
            {
                strSql += " WHERE reportdatetime BETWEEN @StartTime AND @EndTime";
            }
            else
            {
                strSql += @" WHERE updatedatetime BETWEEN @StartTime AND @EndTime
                       AND reportdatetime >= @reportdatetimeStart AND reportdatetime < @reportdatetimeEnd";
                par.Add("@reportdatetimeStart", SummaryRecordReq.StartTime.AddDays(-3));
                par.Add("@reportdatetimeEnd", SummaryRecordReq.EndTime.AddHours(1));
            }
            if (SummaryRecordReq.Club_id != null)
            {
                par.Add("@Club_id", SummaryRecordReq.Club_id);
                strSql += " AND Club_id = @Club_id";
            }
            if (SummaryRecordReq.Franchiser_id != null)
            {
                par.Add("@Franchiser_id", SummaryRecordReq.Franchiser_id);
                strSql += " AND Franchiser_id = @Franchiser_id";
            }
            if (SummaryRecordReq.game_id != null)
            {
                par.Add("@game_id", SummaryRecordReq.game_id);
                strSql += " AND game_id = @game_id";
            }
            if (SummaryRecordReq.Page != null && SummaryRecordReq.Count != null)
            {
                strSql += @" OFFSET @offset
                        LIMIT @limit";
                par.Add("@offset", SummaryRecordReq.Page * SummaryRecordReq.Count);
                par.Add("@limit", SummaryRecordReq.Count);
            }
            par.Add("@StartTime", SummaryRecordReq.StartTime);
            par.Add("@EndTime", SummaryRecordReq.EndTime);
            using (NpgsqlConnection conn = new NpgsqlConnection(await PGRead))
            {
                return await conn.QueryAsync<BetRecordSummary>(strSql, par, commandTimeout: 300);
            }
        }
        public async Task<BetRecordSummary> GetRecordSummaryById(GetBetRecordReq SummaryRecordReq)
        {
            var par = new DynamicParameters();
            string strSql = @"SELECT id
                                    , club_id
                                    , game_id
                                    , game_type
                                    , bet_type
                                    , bet_amount
                                    , turnover
                                    , win
                                    , netwin
                                    , reportdatetime
                                    , currency
                                    , franchiser_id
                                    , recordcount
                                    , updatedatetime
                                    , jackpotwin
                            FROM t_bet_record_summary
                            WHERE reportdatetime BETWEEN @StartTime AND @EndTime
                            AND id = @id";

            par.Add("@id", Guid.Parse(SummaryRecordReq.summary_id));
            par.Add("@StartTime", SummaryRecordReq.ReportTime.AddDays(-10));
            par.Add("@EndTime", SummaryRecordReq.ReportTime.AddDays(10));

            if (!string.IsNullOrEmpty(SummaryRecordReq.ClubId))
            {
                strSql+= " AND club_id=@club_id";
                par.Add("@club_id", SummaryRecordReq.ClubId);
            }

            using (NpgsqlConnection conn = new NpgsqlConnection(await PGRead))
            {
                return await conn.QuerySingleOrDefaultAsync<BetRecordSummary>(strSql, par);
            }
        }
        public async Task<IEnumerable<BetRecordSummary>> GetRecordSummaryBySession(GetBetSummaryReq SummaryRecordReq)
        {
            var par = new DynamicParameters();
            string strSql = @"SELECT id
                                    , club_id
                                    , game_id
                                    , game_type
                                    , bet_type
                                    , bet_amount
                                    , turnover
                                    , win
                                    , netwin
                                    , reportdatetime
                                    , currency
                                    , franchiser_id
                                    , recordcount
                                    , updatedatetime
                                    , jackpotwin
                            FROM t_bet_record_summary
                            WHERE reportdatetime BETWEEN @StartTime AND @EndTime";
            if (SummaryRecordReq.Club_id != null)
            {
                par.Add("@Club_id", SummaryRecordReq.Club_id);
                strSql += " AND Club_id = @Club_id";
            }
            par.Add("@StartTime", SummaryRecordReq.StartTime);
            par.Add("@EndTime", SummaryRecordReq.EndTime);
            using (NpgsqlConnection conn = new NpgsqlConnection(await PGRead))
            {
                return await conn.QueryAsync<BetRecordSummary>(strSql, par);
            }
        }
        public async Task<IEnumerable<dynamic>> GetRecordSummaryLock(NpgsqlConnection conn, IDbTransaction tran, BetRecordSummary recordData)
        {
            var par = new DynamicParameters();
            string strSql = @"SELECT id
                                    , club_id
                                    , game_id
                                    , game_type
                                    , bet_type
                                    , bet_amount
                                    , turnover
                                    , win
                                    , netwin
                                    , reportdatetime
                                    , currency
                                    , franchiser_id
                                    , recordcount
                                    , updatedatetime
                                    , jackpotwin
                            FROM t_bet_record_summary
                            WHERE reportdatetime = @reportdatetime
                            AND club_id = @club_id
                            AND game_id = @game_id
                            AND game_type=@game_type
                            FOR UPDATE";
            par.Add("@reportdatetime", recordData.ReportDatetime);
            par.Add("@club_id", recordData.Club_id);
            par.Add("@game_id", recordData.Game_id);
            par.Add("@game_type", recordData.Game_type);
            var results = await conn.QueryAsync<BetRecordSummary>(strSql, par, tran);
            return results;
        }
        public async Task<int> PostRecordSummary(NpgsqlConnection conn, IDbTransaction tran, List<BetRecordSummary> summaryData)
        {
            string strSqlDel = @"DELETE FROM t_bet_record_summary
                                WHERE reportdatetime = @reportdatetime
                                AND id=@id";
            await conn.ExecuteAsync(strSqlDel, summaryData, tran);

            string stSqlInsert = @"INSERT INTO t_bet_record_summary
                                    (
                                        id,
                                        club_id,
                                        game_id,
                                        game_type,
                                        bet_type,
                                        bet_amount,
                                        turnover,
                                        win,
                                        netwin,
                                        jackpotwin,
                                        reportdatetime,
                                        currency,
                                        recordcount,
                                        franchiser_id,
                                        updatedatetime
                                    )
                                    VALUES
                                    (
                                        @id,
                                        @club_id,
                                        @game_id,
                                        @game_type,
                                        @bet_type,
                                        @bet_amount,
                                        @turnover,
                                        @win,
                                        @netwin,
                                        @jackpotwin,
                                        @reportdatetime,
                                        @currency,
                                        @recordcount,
                                        @franchiser_id,
                                        @updatedatetime
                                    )";
            return await conn.ExecuteAsync(stSqlInsert, summaryData, tran);
        }
        public async Task<int> PostRecordSummaryNoDel(NpgsqlConnection conn, IDbTransaction tran, List<BetRecordSummary> summaryData)
        {
            string stSqlInsert = @"INSERT INTO t_bet_record_summary
                                    (
                                        id,
                                        club_id,
                                        game_id,
                                        game_type,
                                        bet_type,
                                        bet_amount,
                                        turnover,
                                        win,
                                        netwin,
                                        jackpotwin,
                                        reportdatetime,
                                        currency,
                                        recordcount,
                                        franchiser_id,
                                        updatedatetime
                                    )
                                    VALUES
                                    (
                                        @id,
                                        @club_id,
                                        @game_id,
                                        @game_type,
                                        @bet_type,
                                        @bet_amount,
                                        @turnover,
                                        @win,
                                        @netwin,
                                        @jackpotwin,
                                        @reportdatetime,
                                        @currency,
                                        @recordcount,
                                        @franchiser_id,
                                        @updatedatetime
                                    )";
            return await conn.ExecuteAsync(stSqlInsert, summaryData, tran);
        }

        public async Task<ulong> BatchInsertRecordSummaryAsync(NpgsqlConnection conn, List<BetRecordSummary> summaryData)
        {
            string stSql = @"COPY t_bet_record_summary
                            (
                                id,
                                club_id,
                                game_id,
                                game_type,
                                bet_type,
                                bet_amount,
                                turnover,
                                win,
                                netwin,
                                jackpotwin,
                                reportdatetime,
                                currency,
                                recordcount,
                                franchiser_id,
                                updatedatetime
                            )
                            FROM STDIN (FORMAT BINARY)";

            using var writer = await conn.BeginBinaryImportAsync(stSql);
            foreach (var summary in summaryData)
            {
                await writer.StartRowAsync();
                await writer.WriteAsync(summary.id, NpgsqlTypes.NpgsqlDbType.Uuid);
                await writer.WriteAsync(summary.Club_id, NpgsqlTypes.NpgsqlDbType.Varchar);
                await writer.WriteAsync(summary.Game_id, NpgsqlTypes.NpgsqlDbType.Varchar);
                await writer.WriteAsync(summary.Game_type, NpgsqlTypes.NpgsqlDbType.Integer);
                await writer.WriteAsync(summary.Bet_type, NpgsqlTypes.NpgsqlDbType.Integer);
                await writer.WriteAsync(summary.Bet_amount.GetValueOrDefault(), NpgsqlTypes.NpgsqlDbType.Numeric);
                await writer.WriteAsync(summary.Turnover.GetValueOrDefault(), NpgsqlTypes.NpgsqlDbType.Numeric);
                await writer.WriteAsync(summary.Win.GetValueOrDefault(), NpgsqlTypes.NpgsqlDbType.Numeric);
                await writer.WriteAsync(summary.Netwin.GetValueOrDefault(), NpgsqlTypes.NpgsqlDbType.Numeric);
                await writer.WriteAsync(summary.JackpotWin.GetValueOrDefault(), NpgsqlTypes.NpgsqlDbType.Numeric);
                await writer.WriteAsync(summary.ReportDatetime.GetValueOrDefault(), NpgsqlTypes.NpgsqlDbType.Timestamp);
                await writer.WriteAsync(summary.Currency, NpgsqlTypes.NpgsqlDbType.Varchar);
                await writer.WriteAsync(summary.RecordCount, NpgsqlTypes.NpgsqlDbType.Integer);
                await writer.WriteAsync(summary.Franchiser_id, NpgsqlTypes.NpgsqlDbType.Varchar);
                await writer.WriteAsync(summary.updatedatetime, NpgsqlTypes.NpgsqlDbType.Timestamp);
            }

            return await writer.CompleteAsync();
        }
        public async Task<dynamic> GetBetRecordSummary(GetBetSummary_SummaryReq req)
        {
            var par = new DynamicParameters();
            string strSql = @"SELECT COUNT(1)
                    FROM t_bet_record_summary";


            if (req.SearchType == 1)
            {
                strSql += " WHERE reportdatetime BETWEEN @StartTime AND @EndTime";
            }
            else
            {
                strSql += @" WHERE updatedatetime BETWEEN @StartTime AND @EndTime
                       reportdatetime >= @reportdatetimeStart AND reportdatetime < @reportdatetimeEnd";
                par.Add("@reportdatetimeStart", req.StartTime.AddDays(-3));
                par.Add("@reportdatetimeEnd", req.EndTime.AddHours(1));
            }
            if (req.Club_id != null)
            {
                strSql += " AND club_id = @club_id";
                par.Add("@club_id", req.Club_id);
            }
            if (req.Franchiser_id != null)
            {
                strSql += " AND franchiser_id = @franchiser_id";
                par.Add("@franchiser_id", req.Franchiser_id);
            }
            if (req.game_id != null)
            {
                par.Add("@game_id", req.game_id.ToUpper());
                strSql += " AND game_id = @game_id";
            }
            par.Add("@StartTime", req.StartTime);
            par.Add("@EndTime", req.EndTime);
            using (NpgsqlConnection conn = new NpgsqlConnection(await PGRead))
            {
                return await conn.QueryAsync<dynamic>(strSql, par);
            }
        }
        public async Task<IEnumerable<BetRecordSummary>> GetBetRecordSummaryById(Guid id)
        {
            var par = new DynamicParameters();
            string strSql = @"SELECT id
                                    , club_id
                                    , game_id
                                    , game_type
                                    , bet_type
                                    , bet_amount
                                    , turnover
                                    , win
                                    , netwin
                                    , reportdatetime
                                    , currency
                                    , franchiser_id
                                    , recordcount
                                    , updatedatetime
                                    , jackpotwin
                            FROM t_bet_record_summary
                            WHERE id = @id
                    ";
            par.Add("@id", id);
            using (NpgsqlConnection conn = new NpgsqlConnection(await PGRead))
            {
                return await conn.QueryAsync<BetRecordSummary>(strSql, par);
            }
        }
        public async Task<IEnumerable<BetRecordSummary>> GetBetRecordSummaryByIdLock(NpgsqlConnection conn, IDbTransaction tran, Guid id)
        {
            var par = new DynamicParameters();
            string strSql = @"SELECT id
                                    , club_id
                                    , game_id
                                    , game_type
                                    , bet_type
                                    , bet_amount
                                    , turnover
                                    , win
                                    , netwin
                                    , reportdatetime
                                    , currency
                                    , franchiser_id
                                    , recordcount
                                    , updatedatetime
                                    , jackpotwin
                            FROM t_bet_record_summary
                            WHERE id = @id
                            FOR UPDATE";
            par.Add("@id", id);
            var results = await conn.QueryAsync<BetRecordSummary>(strSql, par, tran);
            return results;
        }
        public async Task<int> PutBetRecordSummary(NpgsqlConnection conn, IDbTransaction tran, BetRecordSummary RecordSummary)
        {
            var par = new DynamicParameters();
            string strSql = @"UPDATE t_bet_record_summary
                                    SET bet_amount = @bet_amount,
                                    turnover = @turnover,
                                    win = @win,
                                    netwin = @netwin,
                                    recordcount = @recordcount,
                                    updatedatetime = @updatedatetime
                                    WHERE id = @id";

            par.Add("@bet_amount", RecordSummary.Bet_amount);
            par.Add("@turnover", RecordSummary.Turnover);
            par.Add("@win", RecordSummary.Win);
            par.Add("@netwin", RecordSummary.Netwin);
            par.Add("@recordcount", RecordSummary.RecordCount);
            par.Add("@updatedatetime", RecordSummary.updatedatetime);
            par.Add("@id", RecordSummary.id);
            return await conn.ExecuteAsync(strSql, par, tran);
        }
        public async Task<int> DeleteBetRecordSummaryById(Guid id)
        {
            var par = new DynamicParameters();
            string strSqlDel = @"DELETE FROM t_bet_record_summary
                               WHERE id=@id";

            par.Add("@id", id);
            using (var conn = new NpgsqlConnection(PGMaster))
            {
                return await conn.ExecuteAsync(strSqlDel, par);
            }
        }

        public async Task<int> PostSummaryBetRecordMapping(IDbTransaction tran, IEnumerable<t_summary_bet_record_mapping> mappings)
        {
            var strSql = @"INSERT INTO public.t_summery_bet_record_mapping
                        (summary_id, report_time, partition_time)
                        VALUES(@summary_id, @report_time, @partition_time)
                        ON CONFLICT (summary_id, report_time, partition_time)
                        DO NOTHING;
                        ";
            return await tran.Connection.ExecuteAsync(strSql, mappings, tran);
        }

        public async Task<int> BulkInsertSummaryBetRecordMapping(IDbTransaction tran, IEnumerable<t_summary_bet_record_mapping> mappings)
        {
            if (tran == null) throw new ArgumentNullException(nameof(tran));
            if (mappings == null) throw new ArgumentNullException(nameof(mappings));
            if (!mappings.Any()) return 0;

            var tableGuid = Guid.NewGuid();
            //建立暫存表
            await CreateSummaryBetRecordMappingTempTable(tran, tableGuid);

            //將資料倒進暫存表
            await BulkInsertToTempTable(tran, tableGuid, mappings);

            //將資料由暫存表倒回主表(過濾重複)
            return await MergeFromTempTable(tran, tableGuid);
        }

        private Task<int> CreateSummaryBetRecordMappingTempTable(IDbTransaction tran, Guid tableGuid)
        {
            var sql = $@"CREATE TEMPORARY TABLE temp_t_summery_bet_record_mapping_{tableGuid:N} 
                            ( LIKE t_summery_bet_record_mapping INCLUDING DEFAULTS INCLUDING CONSTRAINTS );";

            return tran.Connection.ExecuteAsync(sql, transaction: tran);
        }

        private async Task<ulong> BulkInsertToTempTable(IDbTransaction tran, Guid tableGuid,
            IEnumerable<t_summary_bet_record_mapping> mappings)
        {
            if (tran is not NpgsqlTransaction npTran)
                throw new ArgumentException("Must be NpgsqlTransaction", nameof(tran));

            if (npTran.Connection == null)
                throw new ArgumentNullException(nameof(tran.Connection));
            
            await using var writer =
                await npTran.Connection.BeginBinaryImportAsync(
                    $"COPY temp_t_summery_bet_record_mapping_{tableGuid:N} (summary_id, report_time, partition_time) FROM STDIN (FORMAT BINARY)");
            foreach (var mapping in mappings)
            {
                await writer.StartRowAsync();
                await writer.WriteAsync(mapping.summary_id, NpgsqlDbType.Uuid);
                await writer.WriteAsync(mapping.report_time, NpgsqlDbType.Timestamp);
                await writer.WriteAsync(mapping.partition_time, NpgsqlDbType.Timestamp);
            }

            return await writer.CompleteAsync();
        }

        private Task<int> MergeFromTempTable(IDbTransaction tran, Guid tableGuid)
        {
            var sql = @$"INSERT INTO t_summery_bet_record_mapping
                        SELECT summary_id, report_time, partition_time 
                        FROM temp_t_summery_bet_record_mapping_{tableGuid:N} temp
                        WHERE NOT EXISTS 
                        (
                            SELECT NULL
                            FROM public.t_summery_bet_record_mapping
                            WHERE summary_id = temp.summary_id 
                                AND report_time = temp.report_time
                                AND partition_time = temp.partition_time
                        )
                        ";

            return tran.Connection.ExecuteAsync(sql, tran);
        }

        public async Task<DateTime[]> GetPartitionTime(Guid summaryId, DateTime ReportTime)
        {
            var sql = @"SELECT partition_time FROM t_summery_bet_record_mapping 
                    WHERE report_time >= @StartReportTime AND report_time < @EndReportTime 
                        AND summary_id = @summaryId";

            var param = new
            {
                summaryId,
                StartReportTime = ReportTime.AddMinutes(-10),
                EndReportTime = ReportTime.AddMinutes(10),
            };

            await using var conn = new NpgsqlConnection(await PGRead);
            var results = await conn.QueryAsync<DateTime>(sql, param);
            return results.ToArray();
        }

        public async Task<t_summary_bet_record_mapping[]> GetSummaryMappings(IDbTransaction tran, Guid summaryId, DateTime ReportTime)
        {
            var sql = @"SELECT summary_id, report_time, partition_time FROM t_summery_bet_record_mapping 
                    WHERE report_time = @ReportTime
                        AND summary_id = @summaryId";

            var param = new
            {
                summaryId,
                ReportTime,
            };

            var results = await tran.Connection.QueryAsync<t_summary_bet_record_mapping>(sql, param, tran);
            return results.ToArray();
        }

        public async Task<IEnumerable<string>> GetActiveClubIdByRecordSummary(DateTime starTime, DateTime entTime)
        {
            var sql = @"select club_id
					    from t_bet_record_summary tbrs
					    where reportdatetime >= @starTime and reportdatetime < @entTime
					    group by club_id ";

            var par = new
            {
                starTime,
                entTime
            };

            await using var conn = new NpgsqlConnection(await PGRead);
            return (await conn.QueryAsync<string>(sql, par)).ToList();
        }

        public async Task<HashSet<string>> GetInactiveClubIdByRecordSummary(DateTime checkTime, int limitSize)
        {
            var sql = @"select club_id
                        from t_bet_record_summary tbrs
                        where reportdatetime >= @oneHourOffset and reportdatetime < @thirtyMinOffset
                        and not exists (
                            select null
                            from t_bet_record_summary stbrs
                            where reportdatetime >= @thirtyMinOffset and reportdatetime < @checkTime
                            and stbrs.club_id = tbrs.club_id
                        )
                        group by club_id
                        limit @limitSize";

            var par = new
            {
                checkTime,
                thirtyMinOffset = checkTime - TimeSpan.FromMinutes(30),
                oneHourOffset = checkTime - TimeSpan.FromHours(1),
                limitSize
            };

            await using var conn = new NpgsqlConnection(await PGRead);
            return (await conn.QueryAsync<string>(sql, par)).ToHashSet();
        }
    }
}
