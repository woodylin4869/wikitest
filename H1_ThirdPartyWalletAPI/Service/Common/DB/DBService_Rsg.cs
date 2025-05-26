using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using Npgsql;
using H1_ThirdPartyWalletAPI.Model.DataModel;
using System.Data;
using H1_ThirdPartyWalletAPI.Model.W1API;
using System.Linq;
using H1_ThirdPartyWalletAPI.Model;
using H1_ThirdPartyWalletAPI.Model.Game.RSG.Response;
using Microsoft.Extensions.Logging;
using H1_ThirdPartyWalletAPI.Model.DB.RSG.Response;

namespace H1_ThirdPartyWalletAPI.Service.Common
{
    public partial interface IDBService
    {
        Task<IEnumerable<dynamic>> GetRSgSystemWebCode();

        Task<RcgToken> GetRsgToken(string Club_id);
        Task<int> PostRsgToken(RsgToken rcg_data);



        Task<int> PostH1RsgRecord(NpgsqlConnection conn, IDbTransaction tran, List<SessionDetail> record_data);
        Task<IEnumerable<SessionDetail>> GetH1RsgRecordBySummary(GetBetRecordReq RecordReq);
        Task<IEnumerable<SessionDetail>> GetRsgRecordByTime(DateTime startTime, DateTime endTime);

    }
    public partial class DBService : IDBService
    {
        #region t_rsg_token
        public async Task<RcgToken> GetRsgToken(string Club_id)
        {
            var cacheResult = await _cacheDataService.GetOrSetValueAsync($"{RedisCacheKeys.RsgToken}/RSG/{Club_id}",
            async () =>
            {
                using (NpgsqlConnection conn = new NpgsqlConnection(await PGRead))
                {
                    var par = new DynamicParameters();
                    string strSql = @"SELECT *
                    FROM t_rsg_token
                    WHERE club_id = @club_id
                    LIMIT 1 ";
                    par.Add("@club_id", Club_id);
                    return await conn.QuerySingleOrDefaultAsync<RcgToken>(strSql, par);
                }
            },
            _cacheSeconds_api);
            return cacheResult;
        }
        public async Task<int> PostRsgToken(RsgToken rsg_data)
        {
            var postResult = 0;

            using (NpgsqlConnection conn = new NpgsqlConnection(PGMaster))
            {
                string stSqlInsert = @"INSERT INTO t_rsg_token
                                    (
                                        club_id,
                                        system_code,
                                        web_id
                                    )
                                    VALUES
                                    (
                                        @club_id,
                                        @system_code,
                                        @web_id
                                    )";
                postResult = await conn.ExecuteAsync(stSqlInsert, rsg_data);
            }

            if (postResult != 1) return postResult;

            await _cacheDataService.StringSetAsync($"{RedisCacheKeys.RsgToken}/RSG/{rsg_data.club_id}", rsg_data, _cacheSeconds_api);

            return postResult;
        }
        public async Task<IEnumerable<dynamic>> GetRSgSystemWebCode()
        {
            using (NpgsqlConnection conn = new NpgsqlConnection(await PGRead))
            {
                var par = new DynamicParameters();
                string strSql = @"SELECT system_code , web_id 
                                FROM t_rsg_token
                                GROUP BY system_code , web_id ";
                var results = await conn.QueryAsync<dynamic>(strSql);

                return results;
            }
        }
        #endregion
        #region t_rsg_session_detail

        public async Task<dynamic> GetH1RsgRecord(string betid)
        {
            var sql = @"SELECT * FROM t_rsg_session_detail
                        WHERE id = @id";

            var parameters = new DynamicParameters();
            parameters.Add("@id", long.Parse(betid));

            using (var conn = new NpgsqlConnection(await PGRead))
            {
                return await conn.QuerySingleOrDefaultAsync<dynamic>(sql, parameters);
            }
        }
        public async Task<int> PostH1RsgRecord(NpgsqlConnection conn, IDbTransaction tran, List<SessionDetail> record_data)
        {
            var sql = @"INSERT INTO public.t_rsg_session_detail
                        (
                            id,
                            currency, 
                            webid, 
                            userid, 
                            gameid, 
                            betsum, 
                            jackpotwinsum, 
                            netwinsum,
                            recordcount,
                            playstarttime,
                            playendtime,
                            sessionid,
                            hasjc,
                            isinvalid,
                            summary_id
                        )
	                    VALUES 
                        ( 
                            @id,
                            @currency, 
                            @webid, 
                            @userid, 
                            @gameid, 
                            @betsum, 
                            @jackpotwinsum, 
                            @netwinsum,
                            @recordcount,
                            @playstarttime,
                            @playendtime,
                            @sessionid,
                            @hasjc,
                            @isinvalid,
                            @summary_id
                        );";

            return await conn.ExecuteAsync(sql, record_data, tran);
        }
        public async Task<IEnumerable<SessionDetail>> GetH1RsgRecordBySummary(GetBetRecordReq RecordReq)
        {
            var sql = @"SELECT * FROM t_rsg_session_detail
                        WHERE summary_id = @summary_id AND playendtime > @playendtime";

            var parameters = new DynamicParameters();
            parameters.Add("@summary_id", Guid.Parse(RecordReq.summary_id));
            parameters.Add("@playendtime", RecordReq.ReportTime.AddDays(-1));

            using (var conn = new NpgsqlConnection(await PGRead))
            {
                return await conn.QueryAsync<SessionDetail>(sql, parameters);
            }
        }

        
        public async Task<IEnumerable<SessionDetail>> GetRsgRecordByTime(DateTime startTime, DateTime endTime)
        {
            var par = new DynamicParameters();
            string strSql = @"SELECT *
                    FROM t_rsg_session_detail
                    WHERE playendtime BETWEEN @startTime and @endTime
                    ";
            par.Add("@startTime", startTime);
            par.Add("@endTime", endTime.AddMinutes(1).AddMilliseconds(-3));
            using (NpgsqlConnection conn = new NpgsqlConnection(await PGRead))
            {
                return await conn.QueryAsync<SessionDetail>(strSql, par);
            }
        }
        #endregion
    }
}