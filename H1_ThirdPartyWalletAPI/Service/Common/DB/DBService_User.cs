using Dapper;
using H1_ThirdPartyWalletAPI.Model.DataModel;
using H1_ThirdPartyWalletAPI.Model.W1API;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace H1_ThirdPartyWalletAPI.Service.Common
{
    public partial interface IDBService
    {
        Task<int> PostWallet(NpgsqlConnection conn, IDbTransaction tran, Wallet memberWallet);
        Task<IEnumerable<Wallet>> GetWallet(string Club_id);
        Task<IEnumerable<Wallet>> GetWallet(IEnumerable<string> Club_ids);
        Task<IEnumerable<Wallet>> GetWalletLock(NpgsqlConnection conn, IDbTransaction tran, string Club_id);
        Task<int> PutWallet(NpgsqlConnection conn, IDbTransaction tran, Wallet memberWallet);
        Task<int> PutWalletLastPlatform(Wallet memberWallet);
        //Task<int> PutWalletStopBalance(Wallet memberWallet);
        Task<int> DeleteWallet(string club_id);
        Task<dynamic> GetWalletSummary(string franchiser_id);
        Task<IEnumerable<Wallet>> GetWalletList(GetUserWalletReq req);

        //Task<IEnumerable<GetOnlineUserData>> GetUserLastPlatformByGame(string platform);

        Task<PlatformUser> GetGamePlatformPGUser(string pgId);
        Task<GamePlatformUser> GetGamePlatformByGameUserId(string gamePlatform, string gameUserId);
        #region t_game_platform_user
        Task<List<GamePlatformUser>> GetGamePlatformUser(string Club_id);
        Task<int> PostGamePlatformUser(GamePlatformUser userData);
        #endregion

        Task<int> SetWalletLastPlatform(string clubId, string lastPlatform);
        Task<int> SetWalletLastPlatform(IDbTransaction tran, string clubId, string lastPlatform);
        Task<int> DeleteWalletLastPlatform(string clubId);
        Task<List<t_wallet_last_platform>> GetWalletLastPlatformByPlatform(string lastPlatform);
        Task<t_wallet_last_platform> GetWalletLastPlatformById(string clubId);
        Task<List<t_wallet_last_platform>> GetWalletLastPlatform();

        Task<List<string>> GetWalletLastPlatformByCreateTime(DateTime createtime, int limitSize);
    }
    public partial class DBService : IDBService
    {
        #region t_wallet
        public async Task<int> PostWallet(NpgsqlConnection conn, IDbTransaction tran, Wallet memberWallet)
        {
            string stSqlInsert = @"INSERT INTO t_wallet
                                    (
                                        club_id,
                                        club_ename,
                                        credit,
                                        lock_credit,
                                        currency,
                                        franchiser_id
                                    )
                                    VALUES
                                    (
                                        @club_id,
                                        @club_Ename,
                                        @credit,
                                        @lock_credit,
                                        @currency,
                                        @franchiser_id
                                    )";
            return await conn.ExecuteAsync(stSqlInsert, memberWallet, tran);
        }
        public async Task<IEnumerable<Wallet>> GetWallet(string Club_id)
        {
            var par = new DynamicParameters();
            string strSql = @"SELECT club_id, club_ename, credit, lock_credit, currency, franchiser_id, last_platform
                            FROM t_wallet
                            WHERE club_id = @club_id";
            par.Add("@club_id", Club_id);
            using (NpgsqlConnection conn = new NpgsqlConnection(await PGRead))
            {
                return await conn.QueryAsync<Wallet>(strSql, par);
            }
        }
        public async Task<IEnumerable<Wallet>> GetWallet(IEnumerable<string> Club_ids)
        {
            string strSql = @"SELECT club_id, club_ename, credit, lock_credit, currency, franchiser_id, last_platform
                            FROM t_wallet
                            WHERE club_id = ANY (@club_id) ";

            var par = new
            {
                club_id = Club_ids
            };

            using (NpgsqlConnection conn = new NpgsqlConnection(await PGRead))
            {
                return await conn.QueryAsync<Wallet>(strSql, par);
            }
        }
        public async Task<IEnumerable<Wallet>> GetWalletLock(NpgsqlConnection conn, IDbTransaction tran, string Club_id)
        {
            var par = new DynamicParameters();
            string strSql = @"SELECT club_id, club_ename, credit, lock_credit, currency, franchiser_id, last_platform
                            FROM t_wallet
                            WHERE club_id = @club_id
                            FOR UPDATE
                            ";
            par.Add("@club_id", Club_id);
            var results = await conn.QueryAsync<Wallet>(strSql, par, tran);
            return results;
        }
        public async Task<int> PutWallet(NpgsqlConnection conn, IDbTransaction tran, Wallet memberWallet)
        {
            var par = new DynamicParameters();
            string strSql = @"UPDATE t_wallet
                                    SET credit = @credit,
                                    lock_credit = @lock_credit,
                                    franchiser_id = @franchiser_id,
                                    last_platform = @last_platform
                                    WHERE club_id = @club_id";
            par.Add("@credit", memberWallet.Credit);
            par.Add("@lock_credit", memberWallet.Lock_credit);
            par.Add("@club_id", memberWallet.Club_id);
            par.Add("@last_platform", memberWallet.last_platform);
            par.Add("@franchiser_id", memberWallet.Franchiser_id);
            return await conn.ExecuteAsync(strSql, par, tran);
        }
        public async Task<int> PutWalletLastPlatform(Wallet memberWallet)
        {
            var par = new DynamicParameters();
            string strSql = @"UPDATE t_wallet
                            SET last_platform = @last_platform
                            WHERE club_id = @club_id";
            par.Add("@last_platform", memberWallet.last_platform);
            par.Add("@club_id", memberWallet.Club_id);
            using (NpgsqlConnection conn = new NpgsqlConnection(PGMaster))
            {
                return await conn.ExecuteAsync(strSql, par);
            }
        }

        public async Task<int> SetWalletLastPlatform(string clubId, string lastPlatform)
        {
            await using NpgsqlConnection conn = new NpgsqlConnection(PGMaster);
            await conn.OpenAsync();
            await using var tran = await conn.BeginTransactionAsync();
            var result = await SetWalletLastPlatform(tran, clubId, lastPlatform);
            await tran.CommitAsync();
            return result;
        }

        public async Task<List<t_wallet_last_platform>> GetWalletLastPlatformByPlatform(string lastPlatform)
        {
            if (string.IsNullOrWhiteSpace(lastPlatform))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(lastPlatform));

            const string sql = @"SELECT club_id, last_platform
                                FROM public.t_wallet_last_platform
                                WHERE last_platform = @lastPlatform ";

            var param = new
            {
                lastPlatform,
            };

            await using NpgsqlConnection conn = new(await PGRead);
            var result = await conn.QueryAsync<t_wallet_last_platform>(sql, param);
            return result.ToList();
        }

        public async Task<List<string>> GetWalletLastPlatformByCreateTime(DateTime createtime, int limitSize)
        {

            const string sql = @"SELECT club_id
                                FROM public.t_wallet_last_platform
                                WHERE createtime < @createtime
                                limit @limitSize";
            var param = new
            {
                createtime,
                limitSize
            };

            await using NpgsqlConnection conn = new(await PGRead);
            var result = await conn.QueryAsync<string>(sql, param);
            return result.ToList();
        }

        public async Task<t_wallet_last_platform> GetWalletLastPlatformById(string clubId)
        {
            if (string.IsNullOrWhiteSpace(clubId))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(clubId));

            const string sql = @"SELECT club_id, last_platform
                                FROM public.t_wallet_last_platform
                                WHERE club_id = @clubId ";

            var param = new
            {
                clubId,
            };

            await using NpgsqlConnection conn = new(PGMaster);
            var result = await conn.QuerySingleOrDefaultAsync<t_wallet_last_platform>(sql, param);
            return result;
        }

        public async Task<List<t_wallet_last_platform>> GetWalletLastPlatform()
        {
            const string sql = "SELECT club_id, last_platform FROM public.t_wallet_last_platform";

            await using NpgsqlConnection conn = new(await PGRead);
            var result = await conn.QueryAsync<t_wallet_last_platform>(sql);
            return result.ToList();
        }

        public async Task<int> SetWalletLastPlatform(IDbTransaction tran, string clubId, string lastPlatform)
        {
            const string sql = @"INSERT INTO public.t_wallet_last_platform
                                (club_id, last_platform)
                                VALUES(@clubId, @lastPlatform)";

            var par = new
            {
                clubId,
                lastPlatform
            };

            // 先刪除
            await DeleteWalletLastPlatform(tran, clubId);
            return await tran.Connection.ExecuteAsync(sql, par, tran);
        }

        public async Task<int> DeleteWalletLastPlatform(string clubId)
        {
            await using NpgsqlConnection conn = new NpgsqlConnection(PGMaster);
            await conn.OpenAsync();
            await using var tran = await conn.BeginTransactionAsync();
            var result = await DeleteWalletLastPlatform(tran, clubId);
            await tran.CommitAsync();
            return result;
        }

        private async Task<int> DeleteWalletLastPlatform(IDbTransaction tran, string clubId)
        {
            const string sql = @"DELETE FROM public.t_wallet_last_platform
                                WHERE club_id=@clubId";

            var par = new
            {
                clubId,
            };

            return await tran.Connection.ExecuteAsync(sql, par, tran);
        }

        public async Task<int> PutWalletStopBalance(Wallet memberWallet)
        {
            var par = new DynamicParameters();
            string strSql = @"UPDATE t_wallet
                                    SET stop_balance= @stop_balance
                                    WHERE club_id = @club_id";
            par.Add("@stop_balance", memberWallet.stop_balance);
            par.Add("@club_id", memberWallet.Club_id);
            using (NpgsqlConnection conn = new NpgsqlConnection(PGMaster))
            {
                return await conn.ExecuteAsync(strSql, par);
            }
        }
        public async Task<dynamic> GetWalletSummary(string franchiser_id)
        {
            var par = new DynamicParameters();
            string strSql = @"SELECT COUNT(club_id)
                    FROM t_wallet";
            if (franchiser_id != null)
            {
                strSql += " WHERE franchiser_id = @franchiser_id";
                par.Add("@franchiser_id", franchiser_id);
            }
            using (NpgsqlConnection conn = new NpgsqlConnection(await PGRead))
            {
                return await conn.QueryAsync<dynamic>(strSql, par);
            }
        }
        public async Task<IEnumerable<Wallet>> GetWalletList(GetUserWalletReq req)
        {
            var par = new DynamicParameters();
            string strSql = @"SELECT club_id, club_ename, credit, lock_credit, currency, franchiser_id, last_platform
                    FROM t_wallet
                    WHERE true";
            if (req.Franchiser != null)
            {
                strSql += " AND franchiser_id = @franchiser_id";
                par.Add("@franchiser_id", req.Franchiser);
            }
            if (req.Ename != null)
            {
                strSql += " AND club_ename = @club_ename";
                par.Add("@club_ename", req.Ename);
            }
            strSql += @" OFFSET @offset
                        LIMIT @limit";
            par.Add("@offset", req.Page * req.Count);
            par.Add("@limit", req.Count);

            using (NpgsqlConnection conn = new NpgsqlConnection(await PGRead))
            {
                return await conn.QueryAsync<Wallet>(strSql, par);
            }
        }
        public async Task<int> DeleteWallet(string club_id)
        {
            var par = new DynamicParameters();
            string strSqlDel = @"DELETE FROM t_wallet
                               WHERE club_id=@club_id";

            par.Add("@club_id", club_id);
            using (var conn = new NpgsqlConnection(PGMaster))
            {
                return await conn.ExecuteAsync(strSqlDel, par);
            }
        }
        public async Task<IEnumerable<GetOnlineUserData>> GetUserLastPlatformByGame(string platform)
        {
            var par = new DynamicParameters();
            string strSql = @"SELECT club_id, last_platform
                            FROM t_wallet";
            if (platform != null)
            {
                strSql += " WHERE last_platform = @last_platform";
                par.Add("@last_platform", platform);
            }
            else
            {
                strSql += " WHERE last_platform IS NOT NULL";
            }
            using (NpgsqlConnection conn = new NpgsqlConnection(await PGRead))
            {
                return await conn.QueryAsync<GetOnlineUserData>(strSql, par);
            }
        }
        #endregion                
        #region t_game_platform_user
        private async Task<List<GamePlatformUser>> GetMasterGamePlatformUser(string Club_id)
        {
            var par = new DynamicParameters();
            string strSql = @"SELECT club_id, agent_id, game_platform, game_user_id, game_token
                            FROM t_game_platform_user                            
                            WHERE club_id = @club_id
                            ";
            par.Add("@club_id", Club_id);
            using (NpgsqlConnection conn = new NpgsqlConnection(PGMaster))
            {
                var result = await conn.QueryAsync<GamePlatformUser>(strSql, par);
                return result.ToList();
            }
        }
        public async Task<List<GamePlatformUser>> GetGamePlatformUser(string Club_id)
        {
            var par = new DynamicParameters();
            string strSql = @"SELECT club_id, agent_id, game_platform, game_user_id, game_token
                            FROM t_game_platform_user                            
                            WHERE club_id = @club_id
                            ";
            par.Add("@club_id", Club_id);
            using (NpgsqlConnection conn = new NpgsqlConnection(await PGRead))
            {
                var result = await conn.QueryAsync<GamePlatformUser>(strSql, par);
                return result.ToList();
            }
        }
        public async Task<int> PostGamePlatformUser(GamePlatformUser userData)
        {
            string stSqlInsert = @"INSERT INTO t_game_platform_user
                                    (
                                        club_id,
                                        agent_id,
                                        game_platform,
                                        game_user_id,
                                        game_token
                                    )
                                    VALUES
                                    (
                                        @club_id,
                                        @agent_id,
                                        @game_platform,
                                        @game_user_id,
                                        @game_token
                                    )";
            //await _cacheDataService.KeyDelete($"{RedisCacheKeys.PlatformUser}/{userData.agent_id}/{userData.club_id}");
            int result;
            using (var conn = new NpgsqlConnection(PGMaster))
            {
                result = await conn.ExecuteAsync(stSqlInsert, userData);
            }
            //var gamePlatformUser = await GetMasterGamePlatformUser(userData.club_id);
            //await _cacheDataService.StringSetAsync($"{RedisCacheKeys.PlatformUser}/{L2RedisCacheKeys.game_user}/{userData.club_id}", gamePlatformUser, 600);
            return result;
        }
        public async Task<PlatformUser> GetGamePlatformPGUser(string pgId)
        {
            var par = new DynamicParameters();
            string strSql = @"SELECT club_id, agent_id, game_platform, game_user_id, game_token
                            FROM t_game_platform_user
                            WHERE game_platform = 'PG'
                            AND game_user_id = @pg_id
                            LIMIT 1 ";
            par.Add("@pg_id", pgId);
            using (NpgsqlConnection conn = new NpgsqlConnection(await PGRead))
            {
                return await conn.QuerySingleOrDefaultAsync<PlatformUser>(strSql, par);
            }
        }

        public async Task<GamePlatformUser> GetGamePlatformByGameUserId(string gamePlatform, string gameUserId)
        {
            var par = new DynamicParameters();
            string strSql = @"SELECT club_id, agent_id, game_platform, game_user_id, game_token
                            FROM t_game_platform_user
                            WHERE game_platform = @gamePlatform
                            AND game_user_id = @gameUserId
                            LIMIT 1 ";

            par.Add("@gamePlatform", gamePlatform);
            par.Add("@gameUserId", gameUserId);
            using (NpgsqlConnection conn = new NpgsqlConnection(await PGRead))
            {
                return await conn.QuerySingleOrDefaultAsync<GamePlatformUser>(strSql, par);
            }
        }
        #endregion
    }
}