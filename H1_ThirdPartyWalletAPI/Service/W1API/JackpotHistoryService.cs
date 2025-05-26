using Coravel.Cache;
using Dapper;
using H1_ThirdPartyWalletAPI.Model;
using H1_ThirdPartyWalletAPI.Model.Game.RSG.Request;
using H1_ThirdPartyWalletAPI.Model.Game.RSG.Response;
using H1_ThirdPartyWalletAPI.Model.W1API;
using H1_ThirdPartyWalletAPI.Service.Common;
using H1_ThirdPartyWalletAPI.Service.Game;
using H1_ThirdPartyWalletAPI.Service.Game.JDB.API.Utility;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace H1_ThirdPartyWalletAPI.Service.W1API
{
    public interface IJackpotHistoryService
    {
        List<JackpotHistory> GetJackpotHistory();

        void SetJackpotHistory(List<JackpotHistory> history);

        Task<List<JackpotHistory>> GetJackpotHistoryFromRsgApiAsnyc(DateTime startTime, DateTime endTime);
    }

    public class JackpotHistoryService : IJackpotHistoryService, IDisposable
    {
        private List<JackpotHistory> _history = new();
        private bool disposedValue;
        private readonly ReaderWriterLockSlim _lock = new();

        private readonly ILogger<JackpotHistoryService> _logger;
        private readonly IGameApiService _gameaApiService;
        private readonly ICommonService _commonService;
        private readonly IMemoryCache _memoryCache;

        private readonly string memory_cache_key = "JackpotHistory";
        private readonly int _memorycacheMin = 5;

        public JackpotHistoryService(ILogger<JackpotHistoryService> logger, IGameApiService gameaApiService, ICommonService commonService, IMemoryCache memoryCache)
        {
            _logger = logger;
            _gameaApiService = gameaApiService;
            _commonService = commonService;
            _memoryCache = memoryCache;
        }

        /// <summary>
        /// 取得本地儲存的RSG彩金中獎清單
        /// </summary>
        /// <returns></returns>
        public List<JackpotHistory> GetJackpotHistory()
        {
            _lock.EnterReadLock();
            try
            {
                return _history;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        /// <summary>
        /// 更新本地儲存的RSG彩金中獎清單
        /// 有wallet-rsg排程至RSG取得彩金中獎清單並推送至wallet-api本地
        /// </summary>
        /// <param name="history"></param>
        public void SetJackpotHistory(List<JackpotHistory> history)
        {
            _lock.EnterWriteLock();
            try
            {
                _history = new(history);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        /// <summary>
        /// 從RSG取得彩金中獎清單
        /// </summary>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        public async Task<List<JackpotHistory>> GetJackpotHistoryFromRsgApiAsnyc(DateTime startTime, DateTime endTime)
        {
            #region 從 RSG API 取得彩金資料
            //取得所有SystemCode
            var systemCodes = await GetRsgSystemWebCodeAsync();


            var apiTask = new List<Task<GetJackpotHitRecResponse>>();
            var fishApiTask = new List<Task<GetFishJackpotHitRecResponse>>();

            foreach (var (SystemCode, WebId) in systemCodes)
            {
                var req = new GetJackpotHitRecRequest
                {
                    SystemCode = SystemCode,
                    WebId = WebId,
                    DateStart = startTime.ToString("yyyy-MM-dd"),
                    DateEnd = endTime.ToString("yyyy-MM-dd")
                };
                apiTask.Add(this._gameaApiService._RsgAPI.GetJackpotHitRecAsync(req));

                var fishReq = new GetFishJackpotHitRecRequest
                {
                    SystemCode = SystemCode,
                    WebId = WebId,
                    DateStart = startTime.ToString("yyyy-MM-dd"),
                    DateEnd = endTime.ToString("yyyy-MM-dd")
                };
                fishApiTask.Add(this._gameaApiService._RsgAPI.GetFishJackpotHitRec(fishReq));
            }

            var apiRes = await Task.WhenAll(apiTask);
            var fishApiRes = await Task.WhenAll(fishApiTask);

            if (apiRes.Any(r => r.ErrorCode != 0) || fishApiRes.Any(r => r.ErrorCode != 0))
            {
                foreach (var error in apiRes.Where(r => r.ErrorCode != 0).GroupBy(r => new { r.ErrorCode, r.ErrorMessage }).Select(e => (Code: e.Key.ErrorCode, Msg: e.Key.ErrorMessage)))
                {
                    _logger.LogError("GetJackpotHitRec Exception: Code: {code}, Message: {msg}", error.Code, error.Msg);
                }

                apiRes = apiRes.Where(r => r.ErrorCode == 0).ToArray();


                foreach (var error in fishApiRes.Where(r => r.ErrorCode != 0).GroupBy(r => new { r.ErrorCode, r.ErrorMessage }).Select(e => (Code: e.Key.ErrorCode, Msg: e.Key.ErrorMessage)))
                {
                    _logger.LogError("GetFishJackpotHitRec Exception: Code: {code}, Message: {msg}", error.Code, error.Msg);
                }

                fishApiRes = fishApiRes.Where(r => r.ErrorCode == 0).ToArray();
            }

            if (apiRes is not { Length: > 0 } && fishApiRes is not { Length: > 0 })
                return new();

            var jackpotHistoris = new List<JackpotHistory>();

            apiRes.Select(r => r.Data.JackpotHitRec)
                .ForEach(jacketpotHis =>
                {
                    jackpotHistoris.AddRange(jacketpotHis.Select(r => new JackpotHistory
                    {
                        JackpotId = r.JackpotHitID,
                        Seq = r.SequenNumber,
                        Currency = r.Currency,
                        WebId = r.WebId,
                        ClubId = r.UserId,
                        GameId = r.GameId,
                        JackpotType = r.JackpotType,
                        JackpotWin = r.JackpotWin,
                        HitTime = r.HitTime
                    }));
                });

            fishApiRes.Select(r => r.Data.JackpotHitRec)
                .ForEach(jacketpotHis =>
                {
                    jackpotHistoris.AddRange(jacketpotHis.Select(r => new JackpotHistory
                    {
                        JackpotId = r.FishJackpotHitID,
                        Seq = r.SequenNumber,
                        Currency = r.Currency,
                        WebId = r.WebId,
                        ClubId = r.UserId,
                        GameId = r.GameId,
                        JackpotType = r.FishJackpotType,
                        JackpotWin = r.JackpotWin,
                        HitTime = r.HitTime
                    }));
                });
            #endregion

            if (!jackpotHistoris.Any())
                return new();

            return jackpotHistoris;
        }

        private async Task<List<(string SystemCode, string WebId)>> GetRsgSystemWebCodeAsync()
        {
            var systemWebCodeList = await _memoryCache.GetOrCreateAsync($"{memory_cache_key}/RsgSystemWebCode", async entry =>
            {
                IEnumerable<dynamic> systemWebCode = await _commonService._serviceDB.GetRSgSystemWebCode();
                entry.SetAbsoluteExpiration(DateTime.Now.AddMinutes(_memorycacheMin));
                return systemWebCode;
            });

            return systemWebCodeList.Select(w => ((string)w.system_code, (string)w.web_id)).AsList();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: 處置受控狀態 (受控物件)
                    _lock.Dispose();
                }

                // TODO: 釋出非受控資源 (非受控物件) 並覆寫完成項
                // TODO: 將大型欄位設為 Null
                _history = null;
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // 請勿變更此程式碼。請將清除程式碼放入 'Dispose(bool disposing)' 方法
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
