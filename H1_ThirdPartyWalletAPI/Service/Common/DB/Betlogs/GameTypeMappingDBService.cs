using Dapper;
using H1_ThirdPartyWalletAPI.Code;
using H1_ThirdPartyWalletAPI.Model.DataModel;
using H1_ThirdPartyWalletAPI.Model.Game.JDB.Enum;
using H1_ThirdPartyWalletAPI.Utility;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace H1_ThirdPartyWalletAPI.Service.Common.DB.Betlogs
{

    public interface IGameTypeMappingDBService
    {
        Task<List<t_gametype_mapping>> GetGameTypeMapping(Platform game_id);

        Task<bool> CreateGameTypeMapping(t_gametype_mapping data);

        Task<bool> DeleteGameTypeMapping(string gameid, string gametype);
    }
    public class GameTypeMappingDBService : BetlogsDBServiceBase, IGameTypeMappingDBService
    {
        public GameTypeMappingDBService(ILogger<GameTypeMappingDBService> logger, IBetLogsDbConnectionStringManager connectionStringManager) : base(logger, connectionStringManager)
        {
        }
        public async Task<List<t_gametype_mapping>> GetGameTypeMapping(Platform Platform)
        {

            var parameters = new DynamicParameters();
            parameters.Add("@game_id", Platform.ToString());
            string strSql = @"SELECT game_id, gametype, groupgametype,groupgametype_id, memo
                    FROM t_gametype_mapping
                    where game_id=@game_id";

            using (NpgsqlConnection conn = new NpgsqlConnection(await PGRead))
            {
                var result = await conn.QueryAsync<t_gametype_mapping>(strSql, parameters);
                return result.ToList();
            }

        }
        public async Task<bool> CreateGameTypeMapping(t_gametype_mapping data)
        {

            var parameters = new
            {
                GameId = data.game_id.ToUpper(),
                GameType = data.gametype,
                GroupGameType = data.groupgametype,
                GroupGametype_id = data.groupgametype_id,
                Memo = data.memo
            };
            string strSql = @"INSERT INTO t_gametype_mapping (game_id, gametype, groupgametype,groupgametype_id, memo)
            VALUES (@GameId, @GameType, @GroupGameType,@GroupGametype_id, @Memo)";

            using (NpgsqlConnection conn = new NpgsqlConnection(PGMaster))
            {
                var result = await conn.ExecuteAsync(strSql, parameters);
                return result == 1;
            }

        }

        public async Task<bool> DeleteGameTypeMapping(string gameid,  string gametype)
        {

            var parameters = new
            {
                GameId = gameid.ToUpper(),
                GameType = gametype
            };
            string strSql = @"Delete FROM  t_gametype_mapping where game_id=@GameId and gametype=@GameType";

            using (NpgsqlConnection conn = new NpgsqlConnection(PGMaster))
            {
                var result = await conn.ExecuteAsync(strSql, parameters);
                return result == 1;
            }

        }

    }
}
