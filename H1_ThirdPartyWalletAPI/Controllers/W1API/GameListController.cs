using H1_ThirdPartyWalletAPI.Code;
using H1_ThirdPartyWalletAPI.Service.Common;
using Microsoft.AspNetCore.Mvc;
using H1_ThirdPartyWalletAPI.Model;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Npgsql;
using H1_ThirdPartyWalletAPI.Service.Game;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Caching.Memory;
using H1_ThirdPartyWalletAPI.Model.Config;
using Microsoft.AspNetCore.Hosting;
using H1_ThirdPartyWalletAPI.Model.Game.JDB;
using H1_ThirdPartyWalletAPI.Model.Game.JDB.Request;
using H1_ThirdPartyWalletAPI.Model.Game.JDB.Response;
using H1_ThirdPartyWalletAPI.Model.Game.JDB.Exceptions;
using H1_ThirdPartyWalletAPI.Model.Game.JDB.Enum;
using H1_ThirdPartyWalletAPI.Model.W1API;
using System.Linq;

namespace H1_ThirdPartyWalletAPI.Controllers.W1API
{
    [ApiController]
    [Route("w1api/[controller]")]
    public class GameListController : ControllerBase
    {
        private readonly ILogger<GameListController> _logger;
        private readonly IGameApiService _gameaApiService;
        private readonly ICommonService _commonService;
        private readonly IWebHostEnvironment _env;
        private readonly IGameInterfaceService _gameInterfaceService;
        public GameListController(ILogger<GameListController> logger
        , ICommonService commonService
        , IGameApiService gameaApiService
        , IWebHostEnvironment env
        , IGameInterfaceService gameInterfaceService)
        {
            _logger = logger;
            _gameaApiService = gameaApiService;
            _commonService = commonService;
            _env = env;
            _gameInterfaceService = gameInterfaceService;
        }
        /// <summary>
        /// 取得遊戲清單
        /// </summary>
        [HttpGet]
        async public Task<GetGameList> Get([FromQuery] GetGameListReq gamelistReq)
        {
            GetGameList res = new GetGameList();
            try
            {
                gamelistReq.Platform = (gamelistReq.Platform == null) ? nameof(Platform.ALL) : gamelistReq.Platform;
                Platform platformid = (Platform)Enum.Parse(typeof(Platform), gamelistReq.Platform.ToUpper());
                IEnumerable<Model.DataModel.GameList> result = await _commonService._serviceDB.GetGameList(-1, platformid, gamelistReq.Page, gamelistReq.Count);
                res.Data = result.ToList();
                return await Task.FromResult(res);
            }
            catch (Exception ex)
            {
                res.code = (int)ResponseCode.GetGameListFail;
                res.Message = MessageCode.Message[(int)ResponseCode.GetGameListFail] + " | " + ex.Message;
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                _logger.LogError("get gamelist exception EX : {ex}  MSG : {Message} Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
                return res;
            }
        }
        /// <summary>
        /// 重設遊戲清單
        /// </summary>
        [HttpPut]
        async public Task<ResCodeBase> Put()
        {
            ResCodeBase res = new ResCodeBase();
            try
            {
                List<Model.DataModel.GameList> gameList = new List<Model.DataModel.GameList>();
                //await ResetJDBGameLsit(gameList);
                //await ResetMGGameLsit(gameList);
                //await ResetDSGameLsit(gameList);
                //await ResetPGGameLsit(gameList);
                //await ResetAEGameLsit(gameList);
                await ResetRTGGameLsit(gameList);
                using (NpgsqlConnection conn = new NpgsqlConnection(Config.OneWalletAPI.DBConnection.Wallet.Master))
                {
                    await conn.OpenAsync();
                    using (var tran = conn.BeginTransaction())
                    {
                        try
                        {
                            //await _commonService._serviceDB.DeleteGameList(conn, tran, Platform.MG);
                            //await _commonService._serviceDB.DeleteGameList(conn, tran, Platform.JDB);
                            //await _commonService._serviceDB.DeleteGameList(conn, tran, Platform.DS);
                            //await _commonService._serviceDB.DeleteGameList(conn, tran, Platform.PG);
                            //await _commonService._serviceDB.DeleteGameList(conn, tran, Platform.AE);
                            await _commonService._serviceDB.PostGameList(conn, tran, gameList);
                            await tran.CommitAsync();
                        }
                        catch
                        {
                            await tran.RollbackAsync();
                        }
                    }
                    await conn.CloseAsync();
                }
                res.code = (int)ResponseCode.Success;
                res.Message = MessageCode.Message[(int)ResponseCode.Success];
                return res;
            }
            catch (JDBBadRequestException ex)
            {
                res.code = (int)ResponseCode.GetGameListFail;
                res.Message = MessageCode.Message[(int)ResponseCode.GetGameListFail];
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                _logger.LogError("get jdb gamelist exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
                return res;
            }
            catch (Exception ex)
            {
                res.code = (int)ResponseCode.GetGameListFail;
                res.Message = MessageCode.Message[(int)ResponseCode.GetGameListFail];
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                _logger.LogError("put gamelist exception EX : {ex}  MSG : {Message} Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
                return res;
            }
        }

        /// <summary>
        /// 取得遊戲清單總計
        /// </summary>
        [HttpGet("summary")]
        public async Task<GetGameListSummary> GetSummary([FromQuery] GetGameListReq gamelistReq)
        {
            GetGameListSummary res = new GetGameListSummary();
            try
            {
                gamelistReq.Platform = (gamelistReq.Platform == null) ? nameof(Platform.ALL) : gamelistReq.Platform;
                Platform platformid = (Platform)Enum.Parse(typeof(Platform), gamelistReq.Platform.ToUpper());
                IEnumerable<dynamic> result = await _commonService._serviceDB.GetGameListSummary(platformid);
                res.Count = (int)result.SingleOrDefault().count;
                return res;
            }
            catch (Exception ex)
            {
                res.code = (int)ResponseCode.Fail;
                res.Message = MessageCode.Message[(int)ResponseCode.Fail] + "|" + ex.Message;
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                _logger.LogError("get gamelist summary exception EX : {ex}  MSG : {Message} Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
                return res;
            }
        }

        /// <summary>
        /// 取得指定ID遊戲
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        public async Task<GetGameList> Get(long id)
        {
            GetGameList res = new GetGameList();
            try
            {
                IEnumerable<Model.DataModel.GameList> result = await _commonService._serviceDB.GetGameList(id, Platform.ALL, 0, 1);
                res.Data = result.ToList();
                return res;
            }
            catch (Exception ex)
            {
                res.code = (int)ResponseCode.Fail;
                res.Message = MessageCode.Message[(int)ResponseCode.Fail] + "|" + ex.Message;
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                _logger.LogError("get gamelist summary exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
                return res;
            }
        }

        /// <summary>
        /// 更新指定ID遊戲
        /// </summary>
        /// <param name="id"></param>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPut("{id}")]
        public async Task<ResCodeBase> Put([FromBody] PutGameListReq req, long id)
        {
            ResCodeBase res = new ResCodeBase();
            try
            {
                IEnumerable<Model.DataModel.GameList> result = await _commonService._serviceDB.GetGameList(id, Platform.ALL, 0, 1);
                var gamedata = result.Single();
                req.enable_game = (req.enable_game == null) ? gamedata.enable_game : req.enable_game;
                req.popular_game = (req.popular_game == null) ? gamedata.popular_game : req.popular_game;
                req.recommend_game = (req.recommend_game == null) ? gamedata.recommend_game : req.recommend_game;
                req.new_game = (req.new_game == null) ? gamedata.new_game : req.new_game;
                req.game_type = (req.game_type == null) ? gamedata.game_type : req.game_type;
                req.Icon = (req.Icon == null) ? gamedata.icon : req.Icon;
                req.game_name_ch = (req.game_name_ch == null) ? gamedata.game_name_ch : req.game_name_ch;
                req.game_name_en = (req.game_name_en == null) ? gamedata.game_name_en : req.game_name_en;
                req.game_name_mm = (req.game_name_mm == null) ? gamedata.game_name_mm : req.game_name_mm;
                req.game_name_th = (req.game_name_th == null) ? gamedata.game_name_th : req.game_name_th;
                req.game_name_vn = (req.game_name_vn == null) ? gamedata.game_name_vn : req.game_name_vn;
                if (await _commonService._serviceDB.PutGameList(id, req) != 1)
                    throw new Exception("update game fail");
                return res;
            }
            catch (Exception ex)
            {
                res.code = (int)ResponseCode.Fail;
                res.Message = MessageCode.Message[(int)ResponseCode.Fail] + "|" + ex.Message;
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                _logger.LogError("put gamelist id EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
                return res;
            }
        }
        /// <summary>
        /// 刪除指定ID遊戲
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        public async Task<ResCodeBase> Delete(long id)
        {
            ResCodeBase res = new ResCodeBase();
            try
            {
                if (await _commonService._serviceDB.DeleteGameList(id) != 1)
                    throw new Exception("delete game fail");
                return res;
            }
            catch (Exception ex)
            {
                res.code = (int)ResponseCode.Fail;
                res.Message = MessageCode.Message[(int)ResponseCode.Fail] + "|" + ex.Message;
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                _logger.LogError("delete gamelist  exception EX : {ex}  MSG : {Message} Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
                return res;
            }
        }
        /// <summary>
        /// 新增遊戲
        /// </summary>
        [HttpPost]
        async public Task<ResCodeBase> Post([FromBody] PostGameListReq gamelistReq)
        {
            ResCodeBase res = new ResCodeBase();
            try
            {
                if (await _commonService._serviceDB.PostGameList(gamelistReq) != 1)
                    throw new Exception("insert game fail");
                return res;
            }
            catch (Exception ex)
            {
                res.code = (int)ResponseCode.Fail;
                res.Message = MessageCode.Message[(int)ResponseCode.Fail] + " | " + ex.Message;
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                _logger.LogError("Post gamelist exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
                return res;
            }
        }
        private async Task<int> ResetJDBGameLsit(List<Model.DataModel.GameList> gamelist)
        {
            GetGameListRequest req = new GetGameListRequest();
            GetGameListResponse result = await _gameaApiService._JdbAPI.Action49_GetGameList(req);
            req.Lang = "ch";
            GetGameListResponse result_cn = await _gameaApiService._JdbAPI.Action49_GetGameList(req);
            foreach (Model.Game.JDB.Response.GameList r in result.Data)
            {
                foreach (JDBGameInfo s in r.list)
                {
                    Model.DataModel.GameList gamelsitdata = new Model.DataModel.GameList();
                    gamelsitdata.game_name_en = s.name;
                    gamelsitdata.game_no = s.mType.ToString();
                    gamelsitdata.game_type = r.gtype.ToString();
                    gamelsitdata.new_game = s.isNew;
                    gamelsitdata.icon = s.image;
                    gamelsitdata.platform = Platform.JDB.ToString();
                    switch (r.gtype)
                    {
                        case GameType.Slot:
                        case GameType.Fish:
                        case GameType.Arcade:
                            break;
                        case GameType.Poker:
                        case GameType.Lottery:
                            gamelsitdata.enable_game = false;
                            break;
                        default:
                            break;
                    }
                    gamelist.Add(gamelsitdata);
                }
            }
            foreach (Model.Game.JDB.Response.GameList r in result_cn.Data)
            {
                foreach (JDBGameInfo s in r.list)
                {
                    var gamelsitdata = gamelist.SingleOrDefault(x => x.game_no == s.mType.ToString());
                    gamelsitdata.game_name_ch = s.name;
                }
            }

            return 0;
        }
        private async Task<int> ResetMGGameLsit(List<Model.DataModel.GameList> gamelist)
        {
            Model.Game.MG.Response.GetGameListResponse GetMgGameListResponse = await _gameaApiService._MgAPI.GetGameList();
            foreach (Model.Game.MG.Response.ProductInfo r in GetMgGameListResponse.Data)
            {
                Model.DataModel.GameList gamelsitdata = new Model.DataModel.GameList();
                gamelsitdata.game_name_en = r.gameName;
                var cn = r.translatedGameName.Find(x => x.code == "zh-cn");
                gamelsitdata.game_name_ch = cn.value;
                gamelsitdata.game_no = r.gameCode;
                gamelsitdata.game_type = r.gameCategoryName;
                gamelsitdata.platform = Platform.MG.ToString();
                gamelist.Add(gamelsitdata);
            }
            return 0;
        }
        private async Task<int> ResetDSGameLsit(List<Model.DataModel.GameList> gamelist)
        {
            Model.Game.DS.Response.GetGameInfoStateListResponse GetDsGameListResponse = await _gameaApiService._DsAPI.GetGameInfoStateList();
            foreach (Model.Game.DS.Response.DSGameInfo r in GetDsGameListResponse.game_info_state_list)
            {
                Model.DataModel.GameList gamelsitdata = new Model.DataModel.GameList();
                gamelsitdata.game_name_en = r.names.en_us;
                gamelsitdata.game_name_ch = r.names.zh_cn;
                gamelsitdata.game_name_vn = r.names.vi_vn;
                gamelsitdata.game_name_th = r.names.th_th;

                gamelsitdata.game_no = r.id;
                gamelsitdata.game_type = r.type;
                gamelsitdata.platform = Platform.DS.ToString();
                gamelist.Add(gamelsitdata);
            }
            return 0;
        }
        private async Task<int> ResetPGGameLsit(List<Model.DataModel.GameList> gamelist)
        {
            Model.Game.PG.Request.GetGameListRequest req = new Model.Game.PG.Request.GetGameListRequest();
            req.operator_token = Config.CompanyToken.PG_Token;
            req.secret_key = Config.CompanyToken.PG_Key;
            req.currency = Model.Game.PG.PG.Currency["THB"];
            req.language = "en-us";
            req.status = 1;
            Model.Game.PG.Response.GetGameListResponse GetPgGameListResponse = await _gameaApiService._PgAPI.GetGameListAsync(req);
            req.language = "zh-cn";
            Model.Game.PG.Response.GetGameListResponse GetPgGameListResponse_cn = await _gameaApiService._PgAPI.GetGameListAsync(req);
            foreach (Model.Game.PG.Response.GetGameListResponse.Data r in GetPgGameListResponse.data)
            {
                Model.DataModel.GameList gamelsitdata = new Model.DataModel.GameList();
                gamelsitdata.game_name_en = r.gameName;
                gamelsitdata.game_type = r.category.ToString();
                gamelsitdata.game_name_ch = GetPgGameListResponse_cn.data.SingleOrDefault(x => x.gameId == r.gameId).gameName;
                gamelsitdata.game_no = r.gameId.ToString();
                gamelsitdata.platform = Platform.PG.ToString();
                gamelist.Add(gamelsitdata);
            }
            return 0;
        }
        private async Task<int> ResetAEGameLsit(List<Model.DataModel.GameList> gamelist)
        {
            Model.Game.AE.Request.GetGameListRequest req = new Model.Game.AE.Request.GetGameListRequest();
            req.site_id = Config.CompanyToken.AE_SiteId;

            Model.Game.AE.Response.GetGameListResponse GetAeGameListResponse = await _gameaApiService._AeAPI.GetGameListAsync(req);

            foreach (Model.Game.AE.Response.Game r in GetAeGameListResponse.games)
            {
                Model.DataModel.GameList gamelsitdata = new Model.DataModel.GameList();
                gamelsitdata.game_name_en = r.locale.enUS.name;
                gamelsitdata.game_name_ch = r.locale.zhCN.name;
                gamelsitdata.game_no = r.id.ToString();
                gamelsitdata.platform = Platform.AE.ToString();
                gamelist.Add(gamelsitdata);
            }
            return 0;
        }
        private async Task<int> ResetRTGGameLsit(List<Model.DataModel.GameList> gamelist)
        {
            var req = new Model.Game.RTG.Request.GetGameRequest();
            req.Language = "en-US";

            var getGameResponse = await _gameaApiService._RtgAPI.GetGame(req);

            foreach (var r in getGameResponse.Data.Content)
            {
                Model.DataModel.GameList gamelsitdata = new Model.DataModel.GameList();
                gamelsitdata.game_name_en = r.GameName;
                gamelsitdata.game_no = r.GameId.ToString();
                gamelsitdata.platform = Platform.RTG.ToString();
                gamelist.Add(gamelsitdata);
            }
            return 0;
        }


        [HttpGet("GetGameApiList")]
        public async Task<GetGameApiList<object>> GetGameApiList([FromQuery] string Platform)
        {
            var res = new GetGameApiList<object>();
            try
            {
                Platform platformid = (Platform)Enum.Parse(typeof(Platform), Platform.ToUpper());
                var GameList = await _gameInterfaceService.GetGameApiList(platformid);
                res.data = GameList;
                return res;
            }
            catch (Exception ex)
            {
                res.code = (int)ResponseCode.GetGameListFail;
                res.Message = MessageCode.Message[(int)ResponseCode.GetGameListFail] + " | " + ex.Message;
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                _logger.LogError("get gameapilist exception EX : {ex}  MSG : {Message} Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
                return res;
            }
        }

    }
}
