using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using H1_ThirdPartyWalletAPI.Service.Game.RGRICH;
using Microsoft.AspNetCore.Mvc;
using ThirdPartyWallet.Share.Model.Game.RGRICH.Response;
using ThirdPartyWallet.Share.Model.Game.RGRICH.Request;

namespace H1_ThirdPartyWalletAPI.Controllers.Game.RGRICH
{
    [Route("api/[controller]")]
    [ApiController]
    public class RGRICHController : ControllerBase
    {
        private readonly IRGRICHApiService _iRGRICHApiService;

        public RGRICHController(IRGRICHApiService iRGRICHApiService)
        {
            _iRGRICHApiService = iRGRICHApiService;
        }

        /// <summary>
        /// 創建玩家帳號接口
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("CreateUser")]
        public async Task<ResponseBase<CreateUserResponse>> CreateUserAsync(CreateUserRequest source)
        {
            return await _iRGRICHApiService.CreateUserAsync(source);
        }

        /// <summary>
        /// 檢查玩家帳號是否存在接口
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("UserIsExist")]
        public async Task<ResponseBase<UserIsExistResponse>> UserIsExistAsync(UserIsExistDataRequest source)
        {
            return await _iRGRICHApiService.UserIsExistAsync(source);
        }

        /// <summary>
        /// 查詢玩家餘額接口
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("Balance")]
        public async Task<ResponseBase<BalanceResponse>> BalanceAsync(BalanceRequest source)
        {
            return await _iRGRICHApiService.BalanceAsync(source);
        }

        /// <summary>
        /// 玩家充值接口
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("Recharge")]
        public async Task<ResponseBase<RechargeResponse>> RechargeAsync(RechargeRequest source)
        {
            return await _iRGRICHApiService.RechargeAsync(source);
        }

        /// <summary>
        /// 玩家提現接口
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("Withdraw")]
        public async Task<ResponseBase<WithdrawResponse>> WithdrawAsync(WithdrawRequest source)
        {
            return await _iRGRICHApiService.WithdrawAsync(source);
        }

        /// <summary>
        /// 根據流水號查詢玩家充值或提現記錄接口
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("RechargeOrWithdrawRecord")]
        public async Task<ResponseBase<RechargeOrWithdrawRecordResponse>> RechargeOrWithdrawRecordAsync(RechargeOrWithdrawRecordRequest source)
        {
            return await _iRGRICHApiService.RechargeOrWithdrawRecordAsync(source);
        }

        /// <summary>
        /// 獲取單一遊戲地址接口
        /// </summary>
        /// <param name="source"></param>
        /// <param name="lang">語系zh_TW、zh_CN、en、vi、th</param>
        /// <returns></returns>
        [HttpPost]
        [Route("GameUrl")]
        public async Task<ResponseBase<GameUrlResponse>> GameUrlAsync(GameUrlRequest source, string lang)
        {
            return await _iRGRICHApiService.GameUrlAsync(source, lang);
        }

        /// <summary>
        /// 查詢玩家下注記錄接口
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("BetRecord")]
        public async Task<ResponseBaseWithMeta<List<BetRecordResponse>>> BetRecordAsync(BetRecordRequest source)
        {
            //var begintime = new DateTime(2024, 1, 12, 15, 00, 00);
            //var endtime = new DateTime(2024, 1, 12, 16, 00, 00);
            //source.StartTime = begintime;
            //source.EndTime = endtime;

            var result = await _iRGRICHApiService.BetRecordAsync(source);

            #region 測試用可以取5筆展示

#if DEBUG
            result.Data = result.Data.Take(5).ToList();
            if (result.Data.Any() == false)
            {
                // 拉單和補單可以藉由這判斷執行 while break 條件式
                Console.WriteLine("無注單");
            }
#endif
            //
            //

            #endregion 測試用可以取5筆展示

            return result;
        }

        /// <summary>
        /// 獲取細單連結接口
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("BetDetialUrl")]
        public async Task<ResponseBase<BetDetailUrlResponse>> BetDetailUrlAsync(BetDetailUrlRequest source)
        {
            return await _iRGRICHApiService.BetDetailUrlAsync(source, "");
        }

        /// <summary>
        /// 獲取遊戲清單接口
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("GameList")]
        public async Task<ResponseBase<Dictionary<string, string>>> GetListAsync(GameListRequest source)
        {
            return await _iRGRICHApiService.GameListAsync(source);
        }

        /// <summary>
        /// 查詢每小時注單統計紀錄接口
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("ReportHour")]
        public async Task<ResponseBase<ReportHourResponse>> ReportHourAsync(ReportHourRequest source)
        {
            return await _iRGRICHApiService.ReportHourAsync(source);
        }

        /// <summary>
        /// 剔除在線會員接口
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("kickUser")]
        public async Task<ResponseBase<object>> KickUserAsync(KickUserRequest source)
        {
            return await _iRGRICHApiService.KickUserAsync(source);
        }

        [HttpPost]
        [Route("HealthCheck")]
        public async Task<HealthCheckResponse> HealthCheckAsync(HealthCheckRequest source)
        {
            return await _iRGRICHApiService.HealthCheckAsync(source);
        }
    }
}