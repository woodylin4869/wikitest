using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using Npgsql;
using H1_ThirdPartyWalletAPI.Model.DataModel;
using System.Data;
using System.Linq;
using H1_ThirdPartyWalletAPI.Model.W1API;
using Microsoft.Extensions.Logging;
using H1_ThirdPartyWalletAPI.Model.DB.GetWalletSessionStatus;

namespace H1_ThirdPartyWalletAPI.Service.Common
{
    public partial interface IDBService
    {
        Task<bool> PostWalletSessionV2(NpgsqlConnection conn, IDbTransaction tran, WalletSessionV2 walletSessionData);
        Task<WalletSessionV2> GetWalletSessionV2Lock(NpgsqlConnection conn, IDbTransaction tran, Guid session_id, WalletSessionV2.SessionStatus status);
        Task<bool> PutWalletSessionV2(NpgsqlConnection conn, IDbTransaction tran, WalletSessionV2 walletSessionData);
        Task<int> PutWalletSessionV2(Guid session_id);
        Task<IEnumerable<WalletSessionV2>> GetWalletSessionV2(List<short> status, string club_id = null);
        Task<IEnumerable<WalletSessionV2>> GetWalletSessionV2byUpdateTime(DateTime StartTime, DateTime EndTime);
        Task<WalletSessionV2> GetWalletSessionV2ById(Guid session_id);
        Task<WalletSessionV2> GetWalletSessionV2ByIdFromMaster(Guid session_id);
        Task<int> DeleteWalletSessionV2(NpgsqlConnection conn, IDbTransaction tran, Guid session_id);
        Task<IEnumerable<string>> GetIdleWalletSessionV2Ids(DateTime StartTime, DateTime EndTime, int size);
        Task<int> PostWalletSessionV2History(NpgsqlConnection conn, IDbTransaction tran, List<WalletSessionV2> walletSessionData);

        Task<int> MoveWalletSessionToHistory(IEnumerable<short> status, int limit);
        Task<WalletSessionV2> GetalletSessionV2HistoryLock(NpgsqlConnection conn, IDbTransaction tran, Guid session_id, WalletSessionV2.SessionStatus status);
        Task<IEnumerable<WalletSessionV2>> GetalletSessionV2History(List<short> status, string club_id, DateTime searchTime);
        Task<bool> PutWalletSessionV2History(NpgsqlConnection conn, IDbTransaction tran, WalletSessionV2 walletSessionData);
        Task<WalletSessionV2> GetalletSessionV2HistoryById(Guid session_id);
        Task<WalletSessionV2> GetWalletSessionV2HistoryByIdAndStartTime(Guid session_id, DateTime startTime);
        Task<IEnumerable<dynamic>> GetRecordSession(NpgsqlConnection conn, IDbTransaction tran, BetRecordSession recordData);
        Task<int> PostRecordSession(NpgsqlConnection conn, IDbTransaction tran, List<BetRecordSession> summaryData);
        Task<IEnumerable<BetRecordSession>> GetRecordSessionBySessionId(Guid session_id);
        Task<IEnumerable<BetRecordSession>> GetRecordSessionByBetSessionId(Guid bet_session_id);
        Task<IEnumerable<BetRecordSession>> GetRecordSessionByStatus(BetRecordSession.Recordstatus recordstatus);
        Task<bool> PutRecordSession(BetRecordSession RecordSessionData);
        Task<IEnumerable<BetRecordSession>> GetRecordSession(GetBetRecordSessionReq SessionRecordReq);
        Task<dynamic> GetRecordSessionSummary(GetBetRecordSession_SummaryReq req);
        Task<IEnumerable<BetRecordSession>> GetRecordSessionById(Guid id);
        Task<IEnumerable<BetRecordSession>> GetRecordSessionByIdLock(NpgsqlConnection conn, IDbTransaction tran, Guid id);
        Task<int> PutRecordSession(NpgsqlConnection conn, IDbTransaction tran, BetRecordSession BetRecordSession);
        Task<IEnumerable<GetWalletSessionStatusResponse>> GetWalletSessionStatus(List<short> status, string club_id = null);
        Task<int> DeleteRecordSessionById(Guid id);
    }
    public partial class DBService : IDBService
    {
        #region t_wallet_session_v2
        public async Task<bool> PostWalletSessionV2(NpgsqlConnection conn, IDbTransaction tran, WalletSessionV2 walletSessionData)
        {


            var sql = @"
                INSERT INTO t_wallet_session_v2
                (
	                session_id,	                
	                start_time,
	                end_time,
	                start_balance,
	                end_balance,
	                amount_change,
	                netwin,
	                status,
	                club_id,
	                update_time,
	                total_in,
	                total_out,
                    franchiser_id
                )
                VALUES
                (
	                @session_id,
	                @start_time,
	                @end_time,
	                @start_balance,
	                @end_balance,
	                @amount_change,
	                @netwin,
	                @status,
	                @club_id,
	                @update_time,
	                @total_in,
	                @total_out,
                    @franchiser_id
                )";
            var result = await conn.ExecuteAsync(sql, walletSessionData, tran);
            return result > 0;
        }
        public async Task<WalletSessionV2> GetWalletSessionV2Lock(NpgsqlConnection conn, IDbTransaction tran, Guid session_id, WalletSessionV2.SessionStatus status)
        {
            var par = new DynamicParameters();
            string strSql = @"SELECT 
                                    session_id
	                                ,start_time
	                                ,end_time
	                                ,start_balance
	                                ,end_balance
	                                ,amount_change
	                                ,netwin
	                                ,STATUS
	                                ,club_id
	                                ,update_time
	                                ,total_in
	                                ,total_out
	                                ,push_times
	                                ,franchiser_id
                            FROM t_wallet_session_v2
                            WHERE session_id = @session_id
                            AND status = @status
                            LIMIT 1 
                            FOR UPDATE
                            ";
            par.Add("@session_id", session_id);
            par.Add("@status", status);
            return await conn.QuerySingleOrDefaultAsync<WalletSessionV2>(strSql, par, tran);
        }
        public async Task<IEnumerable<WalletSessionV2>> GetWalletSessionV2(List<short> status, string club_id)
        {
            var par = new DynamicParameters();
            string strSql = @"SELECT session_id
	                                ,start_time
	                                ,end_time
	                                ,start_balance
	                                ,end_balance
	                                ,amount_change
	                                ,netwin
	                                ,STATUS
	                                ,club_id
	                                ,update_time
	                                ,total_in
	                                ,total_out
	                                ,push_times
	                                ,franchiser_id
                            FROM t_wallet_session_v2
                            WHERE status IN (               
                        ";
            foreach (short id in status)
            {
                strSql += id.ToString() + ",";
            }
            strSql = strSql.TrimEnd(',');
            strSql += ")";

            if (club_id != null)
            {
                strSql += " AND club_id = @club_id";
                par.Add("@club_id", club_id);
            }

            await using (var conn = new NpgsqlConnection(await PGRead))
            {
                return await conn.QueryAsync<WalletSessionV2>(strSql, par);
            }
        }
        public async Task<IEnumerable<GetWalletSessionStatusResponse>> GetWalletSessionStatus(List<short> status, string club_id)
        {
            var par = new DynamicParameters();
            string strSql = @"SELECT 
                                     STATUS
                                    ,club_id
                                    ,franchiser_id
                            FROM t_wallet_session_v2
                            WHERE status IN (               
                        ";
            foreach (short id in status)
            {
                strSql += id.ToString() + ",";
            }
            strSql = strSql.TrimEnd(',');
            strSql += ")";

            if (club_id != null)
            {
                strSql += " AND club_id = @club_id";
                par.Add("@club_id", club_id);
            }

            await using (var conn = new NpgsqlConnection(await PGRead))
            {
                return await conn.QueryAsync<GetWalletSessionStatusResponse>(strSql, par);
            }
        }
        public async Task<bool> PutWalletSessionV2(NpgsqlConnection conn, IDbTransaction tran, WalletSessionV2 walletSessionData)
        {
            var par = new DynamicParameters();
            var strSql = @"UPDATE t_wallet_session_v2
                        SET end_time = @end_time, 
                        end_balance = @end_balance, 
                        amount_change = @amount_change,
                        total_in = @total_in,
                        total_out = @total_out,
                        status = @status,
                        netwin = @netwin,
                        update_time = @update_time,
                        push_times = @push_times
                        WHERE session_id = @session_id";
            par.Add("@end_time", walletSessionData.end_time);
            par.Add("@end_balance", walletSessionData.end_balance);
            par.Add("@amount_change", walletSessionData.amount_change);
            par.Add("@total_in", walletSessionData.total_in);
            par.Add("@total_out", walletSessionData.total_out);
            par.Add("@status", walletSessionData.status);
            par.Add("@netwin", walletSessionData.netwin);
            par.Add("@update_time", walletSessionData.update_time);
            par.Add("@session_id", walletSessionData.session_id);
            par.Add("@push_times", walletSessionData.push_times);
            var result = await conn.ExecuteAsync(strSql, par, tran);
            return result > 0;
        }
        public async Task<int> PutWalletSessionV2(Guid session_id)
        {
            var par = new DynamicParameters();
            var strSql = @"UPDATE t_wallet_session_v2
                        SET update_time = @update_time
                        WHERE session_id = @session_id";

            par.Add("@update_time", DateTime.Now);
            par.Add("@session_id", session_id);

            await using (var conn = new NpgsqlConnection(PGMaster))
            {
                return await conn.ExecuteAsync(strSql, par);
            }
        }
        public async Task<WalletSessionV2> GetWalletSessionV2ById(Guid session_id)
        {
            var par = new DynamicParameters();
            string strSql = @"SELECT 
                                    session_id
	                                ,start_time
	                                ,end_time
	                                ,start_balance
	                                ,end_balance
	                                ,amount_change
	                                ,netwin
	                                ,STATUS
	                                ,club_id
	                                ,update_time
	                                ,total_in
	                                ,total_out
	                                ,push_times
	                                ,franchiser_id
                            FROM t_wallet_session_v2
                            WHERE session_id = @session_id";
            par.Add("@session_id", session_id);
            await using (var conn = new NpgsqlConnection(await PGRead))
            {
                return await conn.QuerySingleOrDefaultAsync<WalletSessionV2>(strSql, par);
            }
        }

        public async Task<WalletSessionV2> GetWalletSessionV2ByIdFromMaster(Guid session_id)
        {
            var par = new DynamicParameters();
            string strSql = @"SELECT 
                                    session_id
	                                ,start_time
	                                ,end_time
	                                ,start_balance
	                                ,end_balance
	                                ,amount_change
	                                ,netwin
	                                ,STATUS
	                                ,club_id
	                                ,update_time
	                                ,total_in
	                                ,total_out
	                                ,push_times
	                                ,franchiser_id
                    FROM t_wallet_session_v2
                    WHERE session_id = @session_id";
            par.Add("@session_id", session_id);
            await using (var conn = new NpgsqlConnection(PGMaster))
            {
                return await conn.QuerySingleOrDefaultAsync<WalletSessionV2>(strSql, par);
            }
        }

        public async Task<int> DeleteWalletSessionV2(NpgsqlConnection conn, IDbTransaction tran, Guid session_id)
        {
            var par = new DynamicParameters();
            string strSql = @"DELETE FROM t_wallet_session_v2
                               WHERE session_id=@session_id";
            par.Add("@session_id", session_id);
            return await conn.ExecuteAsync(strSql, par, tran);
        }
        public async Task<IEnumerable<WalletSessionV2>> GetWalletSessionV2byUpdateTime(DateTime StartTime, DateTime EndTime)
        {
            var par = new DynamicParameters();
            string strSql = @"SELECT 
                                    session_id
	                                ,start_time
	                                ,end_time
	                                ,start_balance
	                                ,end_balance
	                                ,amount_change
	                                ,netwin
	                                ,STATUS
	                                ,club_id
	                                ,update_time
	                                ,total_in
	                                ,total_out
	                                ,push_times
	                                ,franchiser_id
                            FROM t_wallet_session_v2
                            WHERE update_time BETWEEN @StartTime AND @EndTime
                            limit 80000
                        ";
            par.Add("@StartTime", StartTime);
            par.Add("@EndTime", EndTime);
            await using (var conn = new NpgsqlConnection(await PGRead))
            {
                return await conn.QueryAsync<WalletSessionV2>(strSql, par);
            }
        }
        public async Task<IEnumerable<string>> GetIdleWalletSessionV2Ids(DateTime startTime, DateTime endTime, int size)
        {
            var sql = $@"select club_id
                        from t_wallet_session_v2 twsv
                        where status = 1
                        and update_time between @StartTime and @EndTime
                        limit {size}";

            var par = new
            {
                StartTime = startTime, EndTime = endTime
            };

            await using var conn = new NpgsqlConnection(await PGRead);
            return (await conn.QueryAsync<string>(sql, par)).ToList();
        }

        # endregion
        #region t_wallet_session_v2_history
        public async Task<int> PostWalletSessionV2History(NpgsqlConnection conn, IDbTransaction tran, List<WalletSessionV2> walletSessionData)
        {
            const string sql = @"INSERT INTO t_wallet_session_v2_history 
                                (
	                                session_id,	                
	                                start_time,
	                                end_time,
	                                start_balance,
	                                end_balance,
	                                amount_change,
	                                netwin,
	                                status,
	                                club_id,
	                                update_time,
	                                total_in,
	                                total_out,
                                    franchiser_id
                                ) 
                                SELECT 
	                                session_id,	                
	                                start_time,
	                                end_time,
	                                start_balance,
	                                end_balance,
	                                amount_change,
	                                netwin,
	                                status,
	                                club_id,
	                                update_time,
	                                total_in,
	                                total_out,
                                    franchiser_id
                                FROM t_wallet_session_v2 
                                WHERE session_id = ANY (@SessionIdList)";

            var param = new
            {
                SessionIdList = walletSessionData.Select(x => x.session_id).ToList()
            };

            return await conn.ExecuteAsync(sql, param, tran);
        }

        /// <summary>
        /// 批量寫入t_wallet_session_v2_history
        /// COPY
        /// </summary>
        public async Task<ulong> BulkInsertWalletSessionV2History(NpgsqlConnection conn,
            IEnumerable<WalletSessionV2> walletSessions)
        {
            const string sql = @"COPY t_wallet_session_v2_history 
                                (
	                                session_id,	                
	                                start_time,
	                                end_time,
	                                start_balance,
	                                end_balance,
	                                amount_change,
	                                netwin,
	                                status,
	                                club_id,
	                                update_time,
	                                total_in,
	                                total_out,
                                    franchiser_id
                                ) 
                                FROM STDIN (FORMAT BINARY)";
            
            await using var writer = await conn.BeginBinaryImportAsync(sql);
            foreach (var walletSession in walletSessions)
            {
                await writer.StartRowAsync();
                await writer.WriteAsync(walletSession.session_id, NpgsqlTypes.NpgsqlDbType.Uuid);
                await writer.WriteAsync(walletSession.start_time, NpgsqlTypes.NpgsqlDbType.Timestamp);
                await writer.WriteAsync(walletSession.end_time, NpgsqlTypes.NpgsqlDbType.Timestamp);
                await writer.WriteAsync(walletSession.start_balance, NpgsqlTypes.NpgsqlDbType.Numeric);
                await writer.WriteAsync(walletSession.end_balance, NpgsqlTypes.NpgsqlDbType.Numeric);
                await writer.WriteAsync(walletSession.amount_change, NpgsqlTypes.NpgsqlDbType.Numeric);
                await writer.WriteAsync(walletSession.netwin, NpgsqlTypes.NpgsqlDbType.Numeric);
                await writer.WriteAsync((short)walletSession.status, NpgsqlTypes.NpgsqlDbType.Smallint);
                await writer.WriteAsync(walletSession.club_id, NpgsqlTypes.NpgsqlDbType.Varchar);
                await writer.WriteAsync(walletSession.update_time, NpgsqlTypes.NpgsqlDbType.Timestamp);
                await writer.WriteAsync(walletSession.total_in, NpgsqlTypes.NpgsqlDbType.Numeric);
                await writer.WriteAsync(walletSession.total_out, NpgsqlTypes.NpgsqlDbType.Numeric);
                await writer.WriteAsync(walletSession.franchiser_id, NpgsqlTypes.NpgsqlDbType.Varchar);
            }
            return await writer.CompleteAsync();
        }

        public async Task<int> MoveWalletSessionToHistory(IEnumerable<short> status, int limit)
        {
            //檢查status是否為null
            if (status == null || !status.Any())
                throw new ArgumentNullException(nameof(status));

            var result = 0;

            //建立連線
            await using var conn = new NpgsqlConnection(PGMaster);
            await conn.OpenAsync();


            //建立暫存表名稱
            var tempTableGuid = DateTime.Now.ToString("yyyyMMddHHmmss") + "_" + Guid.NewGuid().ToString("N");

            //建立交易
            await using var tran = await conn.BeginTransactionAsync();
            try
            {
                await CreateTempWalletSessionTable(tran, tempTableGuid);
                await InsertWalletSessionToTempTable(tran, tempTableGuid, status.ToList(), limit);
                result = await InsertTempTableToHistoryTable(tran, tempTableGuid);
                await DeleteWalletSessionByTempTable(tran, tempTableGuid);
                await tran.CommitAsync();
            }
            catch (Exception ex)
            {
                await tran.RollbackAsync();
                _logger.LogError(ex, "MoveWalletSessionToHistory failed.");
            }
            finally
            {
                await DropWalletSessionTempTable(conn, tempTableGuid);
            }

            return result;
        }

        private Task<int> CreateTempWalletSessionTable(IDbTransaction tran, string tempTableGuid)
        {
            //check parameter
            if (tran == null)
                throw new ArgumentNullException(nameof(tran));

            //檢查tempTableName是否為null
            if (string.IsNullOrWhiteSpace(tempTableGuid))
                throw new ArgumentNullException(nameof(tempTableGuid));

            var sql = $"CREATE TEMPORARY TABLE temp_t_wallet_session_v2_{tempTableGuid} (LIKE t_wallet_session_v2 INCLUDING ALL)";
            return tran.Connection.ExecuteAsync(sql, tran);
        }

        private Task<int> InsertWalletSessionToTempTable(IDbTransaction tran, string tempTableGuid,
            List<short> status,
            int limit)
        {
            var sql = $@"insert into temp_t_wallet_session_v2_{tempTableGuid}
                        select 
                            session_id
                            , start_time
                            , end_time
                            , start_balance
                            , end_balance
                            , amount_change
                            , netwin
                            , status
                            , club_id
                            , update_time
                            , total_in
                            , total_out
                            , push_times
                            , franchiser_id
                        from t_wallet_session_v2 
                        where status = ANY (@status)
                        limit @limit";

            var param = new
            {
                status,
                limit
            };

            return tran.Connection.ExecuteAsync(sql, param, tran, commandTimeout: 300);
        }

        private Task<int> InsertTempTableToHistoryTable(IDbTransaction tran, string tempTableGuid)
        {
            var sql = $@"insert into t_wallet_session_v2_history
                        select 
                            session_id
                            , start_time
                            , end_time
                            , start_balance
                            , end_balance
                            , amount_change
                            , netwin
                            , status
                            , club_id
                            , update_time
                            , total_in
                            , total_out
                            , push_times
                            , franchiser_id
                        from temp_t_wallet_session_v2_{tempTableGuid}";

            return tran.Connection.ExecuteAsync(sql, null, tran, commandTimeout: 300);
        }

        private Task<int> DeleteWalletSessionByTempTable(IDbTransaction tran, string tempTableGuid)
        {
            var sql = $@"delete from t_wallet_session_v2 twsv
                        where exists (
	                        select null
	                        from temp_t_wallet_session_v2_{tempTableGuid} twsvt
	                        where twsvt.session_id = twsv.session_id
                        )";

            return tran.Connection.ExecuteAsync(sql, null, tran, commandTimeout: 300);
        }

        private Task<int> DropWalletSessionTempTable(IDbConnection conn, string tempTableGuid)
        {
            var sql = $"DROP TABLE IF EXISTS temp_t_wallet_session_v2_{tempTableGuid}";

            return conn.ExecuteAsync(sql, commandTimeout: 300);
        }

        public async Task<WalletSessionV2> GetalletSessionV2HistoryLock(NpgsqlConnection conn, IDbTransaction tran, Guid session_id, WalletSessionV2.SessionStatus status)
        {
            var par = new DynamicParameters();
            string strSql = @"SELECT 
                                    session_id
	                                ,start_time
	                                ,end_time
	                                ,start_balance
	                                ,end_balance
	                                ,amount_change
	                                ,netwin
	                                ,STATUS
	                                ,club_id
	                                ,update_time
	                                ,total_in
	                                ,total_out
	                                ,push_times
	                                ,franchiser_id
                            FROM t_wallet_session_v2_history
                            WHERE session_id = @session_id
                            AND status = @status
                            LIMIT 1 
                            FOR UPDATE
                            ";
            par.Add("@session_id", session_id);
            par.Add("@status", status);
            return await conn.QuerySingleOrDefaultAsync<WalletSessionV2>(strSql, par, tran);
        }
        public async Task<IEnumerable<WalletSessionV2>> GetalletSessionV2History(List<short> status, string club_id, DateTime searchTime)
        {
            var par = new DynamicParameters();
            string strSql = @"SELECT session_id
	                                ,start_time
	                                ,end_time
	                                ,start_balance
	                                ,end_balance
	                                ,amount_change
	                                ,netwin
	                                ,STATUS
	                                ,club_id
	                                ,update_time
	                                ,total_in
	                                ,total_out
	                                ,push_times
	                                ,franchiser_id
                            FROM t_wallet_session_v2_history
                            WHERE status IN (               
                        ";
            foreach (short id in status)
            {
                strSql += id.ToString() + ",";
            }
            strSql = strSql.TrimEnd(',');
            strSql += ")";

            if (club_id != null)
            {
                strSql += " AND club_id = @club_id";
                par.Add("@club_id", club_id);
            }
            strSql += " AND start_time > @searchTime";
            par.Add("@searchTime", searchTime);
            await using (var conn = new NpgsqlConnection(await PGRead))
            {
                return await conn.QueryAsync<WalletSessionV2>(strSql, par);
            }
        }
        public async Task<bool> PutWalletSessionV2History(NpgsqlConnection conn, IDbTransaction tran, WalletSessionV2 walletSessionData)
        {
            var par = new DynamicParameters();
            var strSql = @"UPDATE t_wallet_session_v2_history
                        SET end_time = @end_time, 
                        end_balance = @end_balance, 
                        amount_change = @amount_change,
                        total_in = @total_in,
                        total_out = @total_out,
                        status = @status,
                        netwin = @netwin,
                        update_time = @update_time,
                        push_times = @push_times
                        WHERE session_id = @session_id";
            par.Add("@end_time", walletSessionData.end_time);
            par.Add("@end_balance", walletSessionData.end_balance);
            par.Add("@amount_change", walletSessionData.amount_change);
            par.Add("@total_in", walletSessionData.total_in);
            par.Add("@total_out", walletSessionData.total_out);
            par.Add("@status", walletSessionData.status);
            par.Add("@netwin", walletSessionData.netwin);
            par.Add("@update_time", walletSessionData.update_time);
            par.Add("@session_id", walletSessionData.session_id);
            par.Add("@push_times", walletSessionData.push_times);
            var result = await conn.ExecuteAsync(strSql, par, tran);
            return result > 0;
        }
        public async Task<WalletSessionV2> GetalletSessionV2HistoryById(Guid session_id)
        {
            var par = new DynamicParameters();
            string strSql = @"SELECT session_id
	                                ,start_time
	                                ,end_time
	                                ,start_balance
	                                ,end_balance
	                                ,amount_change
	                                ,netwin
	                                ,STATUS
	                                ,club_id
	                                ,update_time
	                                ,total_in
	                                ,total_out
	                                ,push_times
	                                ,franchiser_id
                            FROM t_wallet_session_v2_history
                            WHERE session_id = @session_id";
            par.Add("@session_id", session_id);
            await using (var conn = new NpgsqlConnection(await PGRead))
            {
                return await conn.QuerySingleOrDefaultAsync<WalletSessionV2>(strSql, par);
            }
        }
        /// <summary>
        /// 取得單一歷史錢包Session紀錄
        /// </summary>
        /// <param name="session_id">唯一值</param>
        /// <param name="startTime">起始時間</param>
        /// <returns></returns>
        public async Task<WalletSessionV2> GetWalletSessionV2HistoryByIdAndStartTime(Guid session_id, DateTime startTime)
        {
            var par = new DynamicParameters();
            string strSql = @"SELECT session_id
	                                ,start_time
	                                ,end_time
	                                ,start_balance
	                                ,end_balance
	                                ,amount_change
	                                ,netwin
	                                ,STATUS
	                                ,club_id
	                                ,update_time
	                                ,total_in
	                                ,total_out
	                                ,push_times
	                                ,franchiser_id
                            FROM t_wallet_session_v2_history
                            WHERE
                                start_time between @start_time_start and @start_time_end
                                AND session_id = @session_id";
            par.Add("@session_id", session_id);
            par.Add("@start_time_start", startTime);
            par.Add("@start_time_end", startTime.AddSeconds(1));
            await using (var conn = new NpgsqlConnection(await PGRead))
            {
                return await conn.QuerySingleOrDefaultAsync<WalletSessionV2>(strSql, par);
            }
        }

        #endregion
        #region t_bet_record_session
        public async Task<IEnumerable<dynamic>> GetRecordSession(NpgsqlConnection conn, IDbTransaction tran, BetRecordSession recordData)
        {
            var par = new DynamicParameters();
            string strSql = @"SELECT *
                    FROM t_bet_record_session
                    WHERE startdatetime = @startdatetime
                    AND club_id = @club_id
                    AND game_id = @game_id";
            par.Add("@startdatetime", recordData.StartDatetime);
            par.Add("@club_id", recordData.Club_id);
            par.Add("@game_id", recordData.Game_id);
            var results = await conn.QueryAsync<BetRecordSession>(strSql, par, tran);
            return results;
        }
        public async Task<int> PostRecordSession(NpgsqlConnection conn, IDbTransaction tran, List<BetRecordSession> summaryData)
        {
            string strSqlDel = @"DELETE FROM t_bet_record_session
                               WHERE id=@id";
            await conn.ExecuteAsync(strSqlDel, summaryData, tran);

            string stSqlInsert = @"INSERT INTO t_bet_record_session
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
	                                    currency,
	                                    franchiser_id,
	                                    recordcount,
	                                    updatedatetime,
	                                    session_id,
	                                    reward,
	                                    fee,
	                                    jackpotcon,
	                                    jackpotwin,
	                                    startdatetime,
	                                    enddatetime,
	                                    status,
                                        bet_session_id
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
	                                    @currency,
	                                    @franchiser_id,
	                                    @recordcount,
	                                    @updatedatetime,
	                                    @session_id,
	                                    @reward,
	                                    @fee,
	                                    @jackpotcon,
	                                    @jackpotwin,
	                                    @startdatetime,
	                                    @enddatetime,
	                                    @status,
                                        @bet_session_id
                                    )";
            return await conn.ExecuteAsync(stSqlInsert, summaryData, tran);
        }
        public async Task<IEnumerable<BetRecordSession>> GetRecordSessionBySessionId(Guid session_id)
        {
            var par = new DynamicParameters();
            string strSql = @"SELECT *
                        FROM t_bet_record_session
                        WHERE session_id = @session_id           
                        ";
            par.Add("@session_id", session_id);
            await using (var conn = new NpgsqlConnection(await PGRead))
            {
                return await conn.QueryAsync<BetRecordSession>(strSql, par);
            }
        }
        public async Task<IEnumerable<BetRecordSession>> GetRecordSessionByBetSessionId(Guid bet_session_id)
        {
            var par = new DynamicParameters();
            string strSql = @"SELECT *
                        FROM t_bet_record_session
                        WHERE bet_session_id = @bet_session_id           
                        ";
            par.Add("@bet_session_id", bet_session_id);
            await using (var conn = new NpgsqlConnection(await PGRead))
            {
                return await conn.QueryAsync<BetRecordSession>(strSql, par);
            }
        }
        public async Task<IEnumerable<BetRecordSession>> GetRecordSessionByStatus(BetRecordSession.Recordstatus recordstatus)
        {
            var par = new DynamicParameters();
            string strSql = @"SELECT *
                        FROM t_bet_record_session
                        WHERE status = @status           
                        ";
            par.Add("@status", recordstatus);
            await using (var conn = new NpgsqlConnection(await PGRead))
            {
                return await conn.QueryAsync<BetRecordSession>(strSql, par);
            }
        }
        public async Task<bool> PutRecordSession(BetRecordSession RecordSessionData)
        {
            var par = new DynamicParameters();
            var strSql = @"UPDATE t_bet_record_session
                        SET session_id = @session_id, 
                        updatedatetime = @updatedatetime,
                        status = @status
                        WHERE id = @id";
            par.Add("@session_id", RecordSessionData.Session_id);
            par.Add("@updatedatetime", DateTime.Now);
            par.Add("@status", RecordSessionData.status);
            par.Add("@id", RecordSessionData.id);
            await using (var conn = new NpgsqlConnection(PGMaster))
            {
                var result = await conn.ExecuteAsync(strSql, par);
                return result > 0;
            }            
        }
        public async Task<IEnumerable<BetRecordSession>> GetRecordSession(GetBetRecordSessionReq SessionRecordReq)
        {
            var par = new DynamicParameters();
            string strSql = @"SELECT *
                    FROM t_bet_record_session";
            if (SessionRecordReq.SearchType == 1)
            {
                strSql += " WHERE startdatetime BETWEEN @startdatetime AND @EndTime";
            }
            else
            {
                strSql += @" WHERE updatedatetime BETWEEN @StartTime AND @EndTime
                       AND startdatetime > @startdatetime";
                par.Add("@startdatetime", SessionRecordReq.StartTime.AddDays(-3));
            }
            if (SessionRecordReq.Club_id != null)
            {
                par.Add("@Club_id", SessionRecordReq.Club_id);
                strSql += " AND Club_id = @Club_id";
            }
            if (SessionRecordReq.Franchiser_id != null)
            {
                par.Add("@Franchiser_id", SessionRecordReq.Franchiser_id);
                strSql += " AND Franchiser_id = @Franchiser_id";
            }
            if (SessionRecordReq.game_id != null)
            {
                par.Add("@game_id", SessionRecordReq.game_id);
                strSql += " AND game_id = @game_id";
            }
            if (SessionRecordReq.Page != null && SessionRecordReq.Count != null)
            {
                strSql += @" OFFSET @offset
                        LIMIT @limit";
                par.Add("@offset", SessionRecordReq.Page * SessionRecordReq.Count);
                par.Add("@limit", SessionRecordReq.Count);
            }
            par.Add("@StartTime", SessionRecordReq.StartTime);
            par.Add("@EndTime", SessionRecordReq.EndTime);
            using (NpgsqlConnection conn = new NpgsqlConnection(await PGRead))
            {
                return await conn.QueryAsync<BetRecordSession>(strSql, par);
            }
        }
        public async Task<dynamic> GetRecordSessionSummary(GetBetRecordSession_SummaryReq req)
        {
            var par = new DynamicParameters();
            string strSql = @"SELECT COUNT(id)
                    FROM t_bet_record_session";


            if (req.SearchType == 1)
            {
                strSql += " WHERE startdatetime BETWEEN @StartTime AND @EndTime";
            }
            else
            {
                strSql += @" WHERE updatedatetime BETWEEN @StartTime AND @EndTime
                       AND startdatetime > @startdatetime";
                par.Add("@startdatetime", req.StartTime.AddDays(-3));
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
        public async Task<IEnumerable<BetRecordSession>> GetRecordSessionById(Guid id)
        {
            var par = new DynamicParameters();
            string strSql = @"SELECT *
                    FROM t_bet_record_session
                    WHERE id = @id
                    ";
            par.Add("@id", id);
            using (NpgsqlConnection conn = new NpgsqlConnection(await PGRead))
            {
                return await conn.QueryAsync<BetRecordSession>(strSql, par);
            }
        }
        public async Task<IEnumerable<BetRecordSession>> GetRecordSessionByIdLock(NpgsqlConnection conn, IDbTransaction tran, Guid id)
        {
            var par = new DynamicParameters();
            string strSql = @"SELECT *
                    FROM t_bet_record_session
                    WHERE id = @id
                    FOR UPDATE
                    ";
            par.Add("@id", id);
            var results = await conn.QueryAsync<BetRecordSession>(strSql, par, tran);
            return results;
        }
        public async Task<int> PutRecordSession(NpgsqlConnection conn, IDbTransaction tran, BetRecordSession BetRecordSession)
        {
            var par = new DynamicParameters();
            string strSql = @"UPDATE t_bet_record_session
                                    SET bet_amount = @bet_amount,
                                    turnover = @turnover,
                                    win = @win,
                                    netwin = @netwin,
                                    recordcount = @recordcount,
                                    updatedatetime = @updatedatetime
                                    WHERE id = @id";

            par.Add("@bet_amount", BetRecordSession.Bet_amount);
            par.Add("@turnover", BetRecordSession.Turnover);
            par.Add("@win", BetRecordSession.Win);
            par.Add("@netwin", BetRecordSession.Netwin);
            par.Add("@recordcount", BetRecordSession.RecordCount);
            par.Add("@updatedatetime", BetRecordSession.UpdateDatetime);
            par.Add("@id", BetRecordSession.id);
            return await conn.ExecuteAsync(strSql, par, tran);
        }
        public async Task<int> DeleteRecordSessionById(Guid id)
        {
            var par = new DynamicParameters();
            string strSqlDel = @"DELETE FROM t_bet_record_session
                               WHERE id=@id";

            par.Add("@id", id);
            using (var conn = new NpgsqlConnection(PGMaster))
            {
                return await conn.ExecuteAsync(strSqlDel, par);
            }
        }
        #endregion
    }
}
