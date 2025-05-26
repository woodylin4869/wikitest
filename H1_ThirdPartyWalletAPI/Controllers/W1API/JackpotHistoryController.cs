using H1_ThirdPartyWalletAPI.Code;
using H1_ThirdPartyWalletAPI.Model.W1API;
using H1_ThirdPartyWalletAPI.Service.W1API;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using H1_ThirdPartyWalletAPI.Model;
using H1_ThirdPartyWalletAPI.Model.Config;
using H1_ThirdPartyWalletAPI.Model.Game.RSG.Response;
using H1_ThirdPartyWalletAPI.Service.Common;
using H1_ThirdPartyWalletAPI.Service.Game.RSG;
using H1_ThirdPartyWalletAPI.Model.Game.RSG.Request;
using System.IO.Compression;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace H1_ThirdPartyWalletAPI.Controllers.W1API
{
    [Route("w1api/[controller]")]
    [ApiController]
    public class JackpotHistoryController : ControllerBase
    {

        private readonly ILogger<JackpotHistoryController> _logger;
        private readonly IJackpotHistoryService _jackpotHistoryService;
        private readonly IRSGApiService _rsgApiService;
        private readonly ICommonService _commonService;

        public JackpotHistoryController(ILogger<JackpotHistoryController> logger, IJackpotHistoryService jackpotHistoryService, IRSGApiService rsgApiService, ICommonService commonService)
        {
            _logger = logger;
            _jackpotHistoryService = jackpotHistoryService;
            _rsgApiService = rsgApiService;
            _commonService = commonService;
        }

        /// <summary>
        /// 查詢彩金資料
        /// 1.可以查詢最多 60 天內
        /// 2.Jackpot Type: 0: GRAND, 1: MAJOR, 2: MINOR, 3: MINI
        /// </summary>
        /// <param name="StartTime">起始日期</param>
        /// <param name="EndTime">結束日期</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<JackpotHistoryRes> Get([FromQuery] DateTime StartTime, [FromQuery] DateTime EndTime)
        {
            var res = new JackpotHistoryRes();
            try
            {
                if (StartTime.AddDays(60) < EndTime)
                    throw new Exception("查詢期間不得超過60日");

                var data = _jackpotHistoryService.GetJackpotHistory();

                return new()
                {
                    code = (int)ResponseCode.Success,
                    Message = MessageCode.Message[(int)ResponseCode.Success],
                    Data = data
                        .Where(x => Convert.ToDateTime(x.HitTime) >= StartTime && Convert.ToDateTime(x.HitTime) <= EndTime)
                        .OrderBy(x => x.HitTime)
                        .ToList()
                };
            }
            catch(Exception ex)
            {
                res.code = (int)ResponseCode.GetJackpotHistoryFail;
                res.Message = MessageCode.Message[(int)ResponseCode.GetJackpotHistoryFail] + " | " + ex.Message;
                res.Data = new();
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                _logger.LogError("get jackpot history exception EX : {ex}  MSG : {Message} Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
                return res;
            }
        }

        /// <summary>
        /// 塞RSG彩金中獎清單
        /// </summary>
        /// <param name="history"></param>
        /// <returns></returns>
        [HttpPost("SetJackpotHistory")]
        public Task<ResCodeBase> SetJackpotHistory([FromBody] List<JackpotHistory> history)
        {
            var result = new ResCodeBase();
            _jackpotHistoryService.SetJackpotHistory(history);
            return Task.FromResult(result);
        }

        /// <summary>
        /// 取得 Jackpot 目前 Pool 值
        /// Jackpot 類型
        /// Type: 0, Name: GRAND
        /// Type: 1, Name: MAJOR
        /// Type: 2, Name: MINOR
        /// Type: 3, Name: MINI
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("JackpotPoolValue")]
        public async Task<JackpotPoolValueRes> JackpotPoolValue([FromQuery] string currency, [FromQuery] string webId)
        {
            var res = new JackpotPoolValueRes();
            try
            {
                if (!Model.Game.RSG.RSG.Currency.ContainsKey(currency))
                {
                    throw new ExceptionMessage((int)ResponseCode.UnavailableCurrency, MessageCode.Message[(int)ResponseCode.UnavailableCurrency]);
                }

                if (string.IsNullOrWhiteSpace(webId)) throw new ArgumentNullException(nameof(webId));

                //判斷是否已有暫存
                var data = await _commonService._cacheDataService.GetOrSetValueAsync(
                    $"{RedisCacheKeys.RsgGetJackpotPoolValue}:{currency.ToUpper()}:{webId}",
                    async() =>
                    {
                        var res = default(GetJackpotPoolValueResponse);
                        var expiry = TimeSpan.FromSeconds(15);
                        var wait = TimeSpan.FromSeconds(10);
                        var retry = TimeSpan.FromMilliseconds(50);
                        //鎖定Key
                        await _commonService._cacheDataService.LockAsyncRegular(
                            $"{RedisCacheKeys.RsgGetJackpotPoolValue}:{currency.ToUpper()}:{webId}",
                            async () =>
                            {
                                //第二次檢查暫存避免穿透
                                res = await _commonService._cacheDataService.StringGetAsync<GetJackpotPoolValueResponse>($"{RedisCacheKeys.RsgGetJackpotPoolValue}:{currency.ToUpper()}:{webId}");
                                if (res != default(GetJackpotPoolValueResponse))
                                    return;

                                res = await _rsgApiService.GetJackpotPoolValueAsync(new GetJackpotPoolValueRequest()
                                {
                                    SystemCode = Config.CompanyToken.RSG_SystemCode,
                                    Currency = currency,
                                    WebId = webId,
                                });
                            },
                            expiry, wait, retry);
                        return res;
                    }
                    , 5);

                if (data is default(GetJackpotPoolValueResponse) || data.ErrorCode != 0)
                {
                    return new()
                    {
                        code = (int)ResponseCode.Fail,
                        Message = MessageCode.Message[(int)ResponseCode.Fail],
                    };
                }

                return new()
                {
                    code = (int)ResponseCode.Success,
                    Message = MessageCode.Message[(int)ResponseCode.Success],
                    Data = data.Data.JackpotPool
                };
            }
            catch (Exception ex)
            {
                res.code = (int)ResponseCode.GetJackpotHistoryFail;
                res.Message = MessageCode.Message[(int)ResponseCode.GetJackpotHistoryFail] + " | " + ex.Message;
                res.Data = new();
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                _logger.LogError("get JackpotPoolValue exception EX : {ex}  MSG : {Message} Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
                return res;
            }
        }

        private static string Unzip(byte[] bytes)
        {
            using var msi = new MemoryStream(bytes);
            using var mso = new MemoryStream();
            using (var gs = new GZipStream(msi, CompressionMode.Decompress))
            {
                gs.CopyTo(mso);
            }

            return Encoding.UTF8.GetString(mso.ToArray());
        }
    }
}
