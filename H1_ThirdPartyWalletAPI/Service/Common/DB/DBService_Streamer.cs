using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using Npgsql;
using H1_ThirdPartyWalletAPI.Model.DataModel;
using H1_ThirdPartyWalletAPI.Model.Game;
using System.Data;
using H1_ThirdPartyWalletAPI.Model;

namespace H1_ThirdPartyWalletAPI.Service.Common
{
    public partial interface IDBService
    {
        public Task<StreamerToken> GetStreamerToken(string Club_id);
        public Task<int> PostStreamerToken(StreamerToken streamer_data);
        public Task<int> PutStreamerToken(string Club_id, string token);
        public Task<IEnumerable<dynamic>> GetStreamerSystemWebCode();
        public Task<int> PostStreamerRecord(NpgsqlConnection conn, IDbTransaction tran, List<BetRecord> record_data);
        public Task<IEnumerable<dynamic>> GetStreamerRecordLatest(string systemCode, string webId);
        public Task<GameReport> SumStreamerBetRecordHourly(DateTime reportDateTime);
    }
    public partial class DBService : IDBService
    {
        #region t_streamer_token
        public async Task<StreamerToken> GetStreamerToken(string Club_id)
        {
            var cacheResult = await _cacheDataService.GetOrSetValueAsync($"{RedisCacheKeys.StreamerToken}/STREAMER/{Club_id}",
            async () =>
            {
                using (NpgsqlConnection conn = new NpgsqlConnection(await PGRead))
                {
                    var par = new DynamicParameters();
                    string strSql = @"SELECT *
                    FROM t_streamer_token
                    WHERE club_id = @club_id
                    LIMIT 1 ";
                    par.Add("@club_id", Club_id);
                    return await conn.QuerySingleOrDefaultAsync<StreamerToken>(strSql, par);
                }
            },
            _cacheSeconds_api);
            return cacheResult;
        }
        public async Task<int> PostStreamerToken(StreamerToken streamer_data)
        {
            using (NpgsqlConnection conn = new NpgsqlConnection(PGMaster))
            {
                string stSqlInsert = @"INSERT INTO t_streamer_token
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
                return await conn.ExecuteAsync(stSqlInsert, streamer_data);
            }
        }
        public async Task<int> PutStreamerToken(string Club_id , string token)
        {
            using (NpgsqlConnection conn = new NpgsqlConnection(PGMaster))
            {
                var par = new DynamicParameters();
                string strSql = @"UPDATE t_streamer_token
                                    SET auth_token = @auth_token
                                    WHERE club_id = @club_id";
                par.Add("@auth_token", token);
                par.Add("@club_id", Club_id);
                return await conn.ExecuteAsync(strSql, par);
            }
        }
        public async Task<IEnumerable<dynamic>> GetStreamerSystemWebCode()
        {
            using (NpgsqlConnection conn = new NpgsqlConnection(await PGRead))
            {
                var par = new DynamicParameters();
                string strSql = @"SELECT system_code , web_id 
                                FROM t_streamer_token
                                GROUP BY system_code , web_id ";
                var results = await conn.QueryAsync<dynamic>(strSql);

                return results;
            }
        }
        #endregion
        #region t_streamer_bet_record
        public async Task<int> PostStreamerRecord(NpgsqlConnection conn, IDbTransaction tran, List<BetRecord> record_data)
        {
            string stSqlInsert = @"INSERT INTO t_streamer_bet_record
            (
                summary_id,
                systemcode,
                webid,
                memberaccount,
                id,
                gameid,
                desk,
                betarea,
                bet,
                available,
                winlose,
                waterrate,
                activeno,
                runno,
                balance,
                datetime,
                reportdt,
                ip,
                odds
            )
            VALUES
            (
                @summary_id,
                @systemcode,
                @webid,
                @memberaccount,
                @id,
                @gameid,
                @desk,
                @betarea,
                @bet,
                @available,
                @winlose,
                @waterrate,
                @activeno,
                @runno,
                @balance,
                @datetime,
                @reportdt,
                @ip,
                @odds
            )";
            return await conn.ExecuteAsync(stSqlInsert, record_data, tran);
        }
        public async Task<IEnumerable<dynamic>> GetStreamerRecordLatest(string systemCode, string webId)
        {
            var par = new DynamicParameters();
            par.Add("@systemcode", systemCode);
            par.Add("@webid", webId);
            string strSql = @"SELECT MAX(id)
                            FROM t_streamer_bet_record
                            WHERE systemcode = @systemcode
                            AND webid = @webid";
            using (NpgsqlConnection conn = new NpgsqlConnection(await PGRead))
            {
                return await conn.QueryAsync<dynamic>(strSql, par);
            }
        }

        public async Task<GameReport> SumStreamerBetRecordHourly(DateTime reportDateTime)
        {
            var par = new DynamicParameters();
            string strSql = @"SELECT 
                    COUNT(*) AS total_cont
                    , SUM(available) AS total_bet
                    , SUM(winlose) AS total_netwin
                    FROM t_streamer_bet_record
                    WHERE reportdt >= @startTime
                    AND reportdt < @endTime
                    ";

            par.Add("@startTime", reportDateTime);
            par.Add("@endTime", reportDateTime.AddHours(1));
            using (NpgsqlConnection conn = new NpgsqlConnection(await PGRead))
            {
                return await conn.QuerySingleOrDefaultAsync<GameReport>(strSql, par);
            }
        }
        #endregion

    }
}
