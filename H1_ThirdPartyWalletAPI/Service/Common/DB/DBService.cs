using Dapper;
using H1_ThirdPartyWalletAPI.Code;
using H1_ThirdPartyWalletAPI.Model.Config;
using H1_ThirdPartyWalletAPI.Model.DataModel;
using H1_ThirdPartyWalletAPI.Model.W1API;
using Microsoft.Extensions.Logging;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using H1_ThirdPartyWalletAPI.Utility;

namespace H1_ThirdPartyWalletAPI.Service.Common
{
    public partial interface IDBService
    {
        #region t_game_list

        Task<int> PostGameList(NpgsqlConnection conn, IDbTransaction tran, List<GameList> gameList_data);

        Task<int> PostGameList(PostGameListReq game_data);

        Task<int> PutGameList(long id, PutGameListReq req);

        Task<int> DeleteGameList(NpgsqlConnection conn, IDbTransaction tran, Platform platform);

        Task<int> DeleteGameList(long id);

        Task<dynamic> GetGameListSummary(Platform platform);

        Task<IEnumerable<GameList>> GetGameList(long id, Platform platform, int page, int count);



        #endregion t_game_list
    }

    public partial class DBService : IDBService
    {
        private readonly ILogger<DBService> _logger;
        private readonly string PGMaster;
        private readonly IDbConnectionStringManager _manager;
        private Task<string> PGRead => _manager.GetReadConnectionString();
        public ICacheDataService _cacheDataService;
        private const int _cacheSeconds_api = 60 * 30;

        public DBService(ILogger<DBService> logger
            , ICacheDataService cacheDataService
            , IWalletDbConnectionStringManager connectionStringManager)
        {
            _logger = logger;
            _cacheDataService = cacheDataService;
            _manager = connectionStringManager;
            PGMaster = _manager.GetMasterConnectionString();

            _cacheDataService = cacheDataService;
        }

        #region t_game_list

        public async Task<int> PostGameList(NpgsqlConnection conn, IDbTransaction tran, List<GameList> gameList_data)
        {
            string stSqlInsert = @"INSERT INTO t_game_list
            (
	            platform,
	            game_name_mm,
	            game_name_en,
	            game_name_ch,
	            game_name_th,
	            game_name_vn,
	            game_type,
	            game_no,
	            popular_game,
	            new_game,
	            enable_game,
	            icon,
	            recommend_game
            )
            VALUES
            (
	            @platform,
	            @game_name_mm,
	            @game_name_en,
	            @game_name_ch,
	            @game_name_th,
	            @game_name_vn,
	            @game_type,
	            @game_no,
	            @popular_game,
	            @new_game,
	            @enable_game,
	            @icon,
	            @recommend_game
            )";
            return await conn.ExecuteAsync(stSqlInsert, gameList_data, tran);
        }

        public async Task<int> PostGameList(PostGameListReq game_data)
        {
            string stSqlInsert = @"INSERT INTO t_game_list
            (
	            platform,
	            game_name_mm,
	            game_name_en,
	            game_name_ch,
	            game_name_th,
	            game_name_vn,
	            game_type,
	            game_no,
	            popular_game,
	            new_game,
	            enable_game,
	            icon,
	            recommend_game
            )
            VALUES
            (
	            @platform,
	            @game_name_mm,
	            @game_name_en,
	            @game_name_ch,
	            @game_name_th,
	            @game_name_vn,
	            @game_type,
	            @game_no,
	            @popular_game,
	            @new_game,
	            @enable_game,
	            @icon,
	            @recommend_game
            )";
            await using (var conn = new NpgsqlConnection(PGMaster))
            {
                return await conn.ExecuteAsync(stSqlInsert, game_data);
            }
        }

        public async Task<int> DeleteGameList(NpgsqlConnection conn, IDbTransaction tran, Platform platform)
        {
            var par = new DynamicParameters();
            string strSqlDel = @"DELETE FROM t_game_list
                               WHERE platform=@platform";

            par.Add("@platform", platform.ToString());
            return await conn.ExecuteAsync(strSqlDel, par, tran);
        }

        public async Task<int> DeleteGameList(long id)
        {
            var par = new DynamicParameters();
            string strSqlDel = @"DELETE FROM t_game_list
                               WHERE id=@id";

            par.Add("@id", id);
            using (var conn = new NpgsqlConnection(PGMaster))
            {
                return await conn.ExecuteAsync(strSqlDel, par);
            }
        }

        public async Task<IEnumerable<GameList>> GetGameList(long id, Platform platform, int page, int count)
        {
            var par = new DynamicParameters();
            string strSql = @"SELECT *
                    FROM t_game_list";
            if (platform != Platform.ALL)
            {
                strSql += " WHERE platform = @platform";
                par.Add("@platform", platform.ToString());
            }
            if (id != -1)
            {
                strSql += " WHERE id = @id";
                par.Add("@id", id);
            }

            strSql += @" OFFSET @offset
                        LIMIT @limit";
            par.Add("@offset", page * count);
            par.Add("@limit", count);

            using (NpgsqlConnection conn = new NpgsqlConnection(await PGRead))
            {
                return await conn.QueryAsync<Model.DataModel.GameList>(strSql, par);
            }
        }

        public async Task<int> PutGameList(long id, PutGameListReq req)
        {
            var par = new DynamicParameters();
            var strSql = @"UPDATE t_game_list
                                SET game_name_mm = @game_name_mm,
                                game_name_en = @game_name_en,
                                game_name_ch = @game_name_ch,
                                game_name_th = @game_name_th,
                                game_name_vn = @game_name_vn,
                                game_type = @game_type,
                                popular_game = @popular_game,
                                new_game = @new_game,
                                enable_game = @enable_game,
                                recommend_game = @recommend_game,
                                icon = @icon
                                WHERE id = @id";
            par.Add("@game_name_mm", req.game_name_mm);
            par.Add("@game_name_en", req.game_name_en);
            par.Add("@game_name_ch", req.game_name_ch);
            par.Add("@game_name_th", req.game_name_th);
            par.Add("@game_name_vn", req.game_name_vn);
            par.Add("@game_type", req.game_type);
            par.Add("@popular_game", req.popular_game);
            par.Add("@new_game", req.new_game);
            par.Add("@enable_game", req.enable_game);
            par.Add("@recommend_game", req.recommend_game);
            par.Add("@icon", req.Icon);
            par.Add("@id", id);
            using (NpgsqlConnection conn = new NpgsqlConnection(PGMaster))
            {
                return await conn.ExecuteAsync(strSql, par);
            }
        }

        public async Task<dynamic> GetGameListSummary(Platform platform)
        {
            var par = new DynamicParameters();
            string strSql = @"SELECT COUNT(id)
                    FROM t_game_list";
            if (platform != Platform.ALL)
            {
                strSql += " WHERE platform = @platform";
                par.Add("@platform", platform.ToString());
            }
            using (NpgsqlConnection conn = new NpgsqlConnection(await PGRead))
            {
                return await conn.QueryAsync<dynamic>(strSql, par);
            }
        }

        #endregion t_game_list

      

    }
}