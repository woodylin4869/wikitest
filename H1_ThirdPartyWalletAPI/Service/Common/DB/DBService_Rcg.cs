using Dapper;
using H1_ThirdPartyWalletAPI.Model;
using H1_ThirdPartyWalletAPI.Model.DataModel;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace H1_ThirdPartyWalletAPI.Service.Common
{
    public partial interface IDBService
    {
        public Task<RcgToken> GetRcgToken(string Club_id);

        public Task<int> PostRcgToken(RcgToken rcg_data);

        public Task<int> PutRcgToken(string Club_id, string token);

        public Task<IEnumerable<dynamic>> GetSystemWebCode();

        public Task<int> PostRcgTransaction(NpgsqlConnection conn, IDbTransaction tran, List<RcgWalletTransaction> rcg_data);

        public Task<int> PostRcgTransaction(NpgsqlConnection conn, IDbTransaction tran, RcgWalletTransaction rcg_data);

        public Task<int> PutRcgTransaction(NpgsqlConnection conn, IDbTransaction tran, RcgWalletTransaction rcg_data);

        public Task<IEnumerable<RcgWalletTransaction>> GetRcgTransaction(Guid tran_id);

        public Task<IEnumerable<RcgWalletTransaction>> GetRcgTransactionBySummaryId(Guid summary_id);

        public Task<IEnumerable<RcgWalletTransaction>> GetRcgTransaction(DateTime startTime, DateTime endTime);

        public Task<IEnumerable<RcgWalletTransaction>> GetRcgMemberTransaction(string club_id, DateTime startTime, DateTime endTime);
    }

    public partial class DBService : IDBService
    {
        #region t_rcg_token

        public async Task<RcgToken> GetRcgToken(string Club_id)
        {
            var cacheResult = await _cacheDataService.GetOrSetValueAsync($"{RedisCacheKeys.RcgToken}/RCG/{Club_id}",
            async () =>
            {
                using (NpgsqlConnection conn = new NpgsqlConnection(await PGRead))
                {
                    var par = new DynamicParameters();
                    string strSql = @"SELECT *
                    FROM t_rcg_token
                    WHERE club_id = @club_id
                    LIMIT 1 ";

                    par.Add("@club_id", Club_id);
                    //var results = await conn.QueryAsync<RcgToken>(strSql, par);
                    return await conn.QuerySingleOrDefaultAsync<RcgToken>(strSql, par);
                }
            },
            _cacheSeconds_api);
            return cacheResult;
        }

        public async Task<int> PostRcgToken(RcgToken rcg_data)
        {
            using (NpgsqlConnection conn = new NpgsqlConnection(PGMaster))
            {
                string stSqlInsert = @"INSERT INTO t_rcg_token
                                    (
                                        club_id,
                                        system_code,
                                        web_id,
                                        auth_token
                                    )
                                    VALUES
                                    (
                                        @club_id,
                                        @system_code,
                                        @web_id,
                                        @auth_token
                                    )";
                return await conn.ExecuteAsync(stSqlInsert, rcg_data);
            }
        }

        public async Task<int> PutRcgToken(string Club_id, string token)
        {
            using (NpgsqlConnection conn = new NpgsqlConnection(PGMaster))
            {
                var par = new DynamicParameters();
                string strSql = @"UPDATE t_rcg_token
                                    SET auth_token = @auth_token
                                    WHERE club_id = @club_id";
                par.Add("@auth_token", token);
                par.Add("@club_id", Club_id);
                return await conn.ExecuteAsync(strSql, par);
            }
        }

        public async Task<IEnumerable<dynamic>> GetSystemWebCode()
        {
            using (NpgsqlConnection conn = new NpgsqlConnection(await PGRead))
            {
                var par = new DynamicParameters();
                // TODO:討論refresh MATERIALIZED 機制在打開(https://www.postgresql.org/docs/current/sql-refreshmaterializedview.html)

                #region

                //string strSql = @"CREATE MATERIALIZED VIEW IF NOT EXISTS public.rsg_token_web_id
                //                TABLESPACE pg_default
                //                AS SELECT
                //                        t_rsg_token.system_code,
                //                        t_rsg_token.web_id
                //                   FROM t_rsg_token
                //                   GROUP BY t_rsg_token.system_code, t_rsg_token.web_id
                //                WITH DATA;
                //                -- 取得使用資料
                //                select system_code,web_id from rsg_token_web_id;";

                #endregion t_rcg_token

                string strSql = @"SELECT system_code , web_id
                                FROM t_rcg_token
                                GROUP BY system_code , web_id ";

                var results = await conn.QueryAsync<dynamic>(strSql);

                return results;
            }
        }

        #endregion

        #region t_rcg_wallet_transaction

        public async Task<int> PostRcgTransaction(NpgsqlConnection conn, IDbTransaction tran, List<RcgWalletTransaction> rcg_data)
        {
            string stSqlInsert = @"INSERT INTO t_rcg_wallet_transaction
                                (
                                    tran_id,
                                    req_id,
                                    desk_id,
                                    game_name,
                                    shoe_no,
                                    round_no,
                                    create_datetime,
                                    before_balance,
                                    after_balance,
                                    club_id,
                                    amount,
                                    franchiser_id,
                                    tran_type,
                                    tran_rid
                                )
                                VALUES
                                (
                                    @tran_id,
                                    @req_id,
                                    @desk_id,
                                    @game_name,
                                    @shoe_no,
                                    @round_no,
                                    @create_datetime,
                                    @before_balance,
                                    @after_balance,
                                    @club_id,
                                    @amount,
                                    @franchiser_id,
                                    @tran_type,
                                    @tran_rid
                                )";

            return await conn.ExecuteAsync(stSqlInsert, rcg_data, tran);
        }

        public async Task<int> PostRcgTransaction(NpgsqlConnection conn, IDbTransaction tran, RcgWalletTransaction rcg_data)
        {
            string stSqlInsert = @"INSERT INTO t_rcg_wallet_transaction
                                (
                                    tran_id,
                                    req_id,
                                    desk_id,
                                    game_name,
                                    shoe_no,
                                    round_no,
                                    create_datetime,
                                    before_balance,
                                    after_balance,
                                    club_id,
                                    amount,
                                    franchiser_id,
                                    tran_type,
                                    tran_rid
                                )
                                VALUES
                                (
                                    @tran_id,
                                    @req_id,
                                    @desk_id,
                                    @game_name,
                                    @shoe_no,
                                    @round_no,
                                    @create_datetime,
                                    @before_balance,
                                    @after_balance,
                                    @club_id,
                                    @amount,
                                    @franchiser_id,
                                    @tran_type,
                                    @tran_rid
                                )";

            return await conn.ExecuteAsync(stSqlInsert, rcg_data, tran);
        }

        public async Task<int> PutRcgTransaction(NpgsqlConnection conn, IDbTransaction tran, RcgWalletTransaction rcg_data)
        {
            var par = new DynamicParameters();
            string strSql = @"UPDATE t_rcg_wallet_transaction
                                    SET summary_id = @summary_id
                                    WHERE tran_id = @tran_id";
            par.Add("@summary_id", rcg_data.summary_id);
            par.Add("@tran_id", rcg_data.tran_id);

            return await conn.ExecuteAsync(strSql, par, tran);
        }

        public async Task<IEnumerable<RcgWalletTransaction>> GetRcgTransaction(Guid tran_id)
        {
            var par = new DynamicParameters();
            string strSql = @"SELECT *
                            FROM t_rcg_wallet_transaction
                            WHERE tran_id = @tran_id
                            LIMIT 1 ";
            par.Add("@tran_id", tran_id);
            using (NpgsqlConnection conn = new NpgsqlConnection(await PGRead))
            {
                return await conn.QueryAsync<RcgWalletTransaction>(strSql, par);
            }
        }

        public async Task<IEnumerable<RcgWalletTransaction>> GetRcgTransactionBySummaryId(Guid summary_id)
        {
            var par = new DynamicParameters();
            string strSql = @"SELECT *
                    FROM t_rcg_wallet_transaction
                    WHERE summary_id = @summary_id";

            par.Add("@summary_id", summary_id);
            using (NpgsqlConnection conn = new NpgsqlConnection(await PGRead))
            {
                return await conn.QueryAsync<RcgWalletTransaction>(strSql, par);
            }
        }

        public async Task<IEnumerable<RcgWalletTransaction>> GetRcgTransaction(DateTime startTime, DateTime endTime)
        {
            var par = new DynamicParameters();
            string strSql = @"SELECT *
                            FROM t_rcg_wallet_transaction
                            WHERE summary_id IS NULL
                            AND create_datetime >= @startTime
                            AND create_datetime < @endTime
                            ORDER by create_datetime ASC
                            ";

            par.Add("@startTime", startTime);
            par.Add("@endTime", endTime);
            using (NpgsqlConnection conn = new NpgsqlConnection(await PGRead))
            {
                return await conn.QueryAsync<RcgWalletTransaction>(strSql, par);
            }
        }

        public async Task<IEnumerable<RcgWalletTransaction>> GetRcgMemberTransaction(string club_id, DateTime startTime, DateTime endTime)
        {
            var par = new DynamicParameters();
            string strSql = @"SELECT *
                            FROM t_rcg_wallet_transaction
                            WHERE club_id = @club_id
                            AND create_datetime >= @startTime
                            AND create_datetime < @endTime
                            ORDER by create_datetime ASC
                            ";

            par.Add("@startTime", startTime);
            par.Add("@endTime", endTime);
            par.Add("@club_id", club_id);
            using (NpgsqlConnection conn = new NpgsqlConnection(await PGRead))
            {
                return await conn.QueryAsync<RcgWalletTransaction>(strSql, par);
            }
        }

        #endregion
    }
}