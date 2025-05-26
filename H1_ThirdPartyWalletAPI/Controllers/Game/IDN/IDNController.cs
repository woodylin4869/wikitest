using H1_ThirdPartyWalletAPI.Service.Game.IDN;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using ThirdPartyWallet.Share.Model.Game.IDN.Request;
using ThirdPartyWallet.Share.Model.Game.IDN.Response;

namespace H1_ThirdPartyWalletAPI.Controllers.Game.IDN
{
    [Route("api/[controller]")]
    [ApiController]
    public class IDNController : ControllerBase
    {
        private readonly IIDNApiService _iIDNApiService;

        private readonly IIDNInterfaceService  iDN_InterfaceService;

        public IDNController(IIDNApiService iIDNApiService, IIDNInterfaceService iDN_InterfaceService)
        {
            _iIDNApiService = iIDNApiService;
            this.iDN_InterfaceService = iDN_InterfaceService;
        }

        /// <summary>
        /// 創建玩家帳號接口
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("Auth")]
        public async Task<AuthResponse> AuthAsync(AuthRequest source)
        {
            source.scope = "";
            AuthResponse responseBase = await _iIDNApiService.AuthAsync(source);

            return responseBase;
        }

        /// <summary>
        /// 創建玩家帳號接口
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("Registration")]
        public async Task<ResponseBase<RegistrationResponse>> RegistrationAsync(RegistrationRequest source)
        {

            source.username = "Joshua";
            source.password = "aa8888";
            source.password_confirmation = "aa8888";
            source.fullname = "Joshua";
            source.currency = 243;
            source.whitelabel_id = 232;
            source.is_mobile = 0;
            source.reg_token = "";
            source.signup_ip = "127.0.0.1";

            return await _iIDNApiService.RegistrationAsync(source);
        }

        /// <summary>
        /// 創建玩家帳號接口
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("UserAuth")]
        public async Task<ResponseBase<UserAuthResponse>> UserAuthAsync(UserAuthRequest source)
        {

            source.username = "Joshua";
            source.password = "aa8888";

            return await _iIDNApiService.UserAuthAsync(source);
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
            source.UserName = "Joshua";
            return await _iIDNApiService.BalanceAsync(source);
        }


        /// <summary>
        /// 刷新玩家餘額接口
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("Calibrate")]
        public async Task<ResponseBase<object>> CalibrateAsync(CalibrateRequest source)
        {
            source.UserName = "Joshua";
            return await _iIDNApiService.CalibrateAsync(source);
        }


        /// <summary>
        /// 玩家提現接口
        /// </summary>
        /// <param name="UserName"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("Withdraw")]
        public async Task<ResponseBase<WithdrawResponse>> WithdrawAsync(string UserName, WithdrawRequest source)
        {
            UserName = "Joshua";

            return await _iIDNApiService.WithdrawAsync(UserName, source);
        }

        /// <summary>
        /// 玩家充值
        /// </summary>
        /// <param name="UserName"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("Deposit")]
        public async Task<ResponseBase<DepositResponse>> DepositAsync(string UserName, DepositRequest source)
        {
            UserName = "Joshua";

            return await _iIDNApiService.DepositAsync(UserName, source);
        }

        /// <summary>
        /// 檢查存款清單
        /// </summary>
        /// <param name="Page"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("CheckDepositList")]
        public async Task<ResponseBase<CheckDepositListResponse>> CheckDepositListAsync(int Page, CheckDepositListRequest source)
        {
            return await _iIDNApiService.CheckDepositListAsync(Page, source);
        }

        /// <summary>
        /// 檢查取款清單
        /// </summary>
        /// <param name="Page"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("CheckWithdrawList")]
        public async Task<ResponseBase<CheckWithdrawListResponse>> CheckWithdrawListAsync(int Page, CheckWithdrawListRequest source)
        {
            return await _iIDNApiService.CheckWithdrawListAsync(Page, source);
        }


        /// <summary>
        /// Launch
        /// </summary>
        /// <param name="UserName"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("Launch")]
        public async Task<ResponseBase<LaunchResponse>> LaunchAsync(string UserName, LaunchRequest source)
        {
            return await _iIDNApiService.LaunchAsync(UserName, "", source);
        }

        /// <summary>
        /// Logout
        /// </summary>
        /// <param name="UserName"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("Logout")]
        public async Task<LogoutResponse> LogoutAsync(string UserName, LogoutRequest source)
        {
            return await _iIDNApiService.LogoutAsync(UserName, source);
        }

        ///// <summary>
        ///// 檢查玩家帳號是否存在接口
        ///// </summary>
        ///// <param name="source"></param>
        ///// <returns></returns>
        //[HttpPost]
        //[Route("UserIsExist")]
        //public async Task<UserIsExistResponse> UserIsExistAsync(UserIsExistDataRequest source)
        //{
        //    return await _iIDNApiService.UserIsExistAsync(source);
        //}






        ///// <summary>
        ///// 根據流水號查詢玩家充值或提現記錄接口
        ///// </summary>
        ///// <param name="source"></param>
        ///// <returns></returns>
        //[HttpPost]
        //[Route("RechargeOrWithdrawRecord")]
        //public async Task<RechargeOrWithdrawRecordResponse> RechargeOrWithdrawRecordAsync(RechargeOrWithdrawRecordRequest source)
        //{
        //    return await _iIDNApiService.RechargeOrWithdrawRecordAsync(source);
        //}

        ///// <summary>
        ///// 獲取單一遊戲地址接口
        ///// </summary>
        ///// <param name="source"></param>
        ///// <param name="lang">語系zh_TW、zh_CN、en、vi、th</param>
        ///// <returns></returns>
        //[HttpPost]
        //[Route("GameUrl")]
        //public async Task<GameUrlResponse> GameUrlAsync(GameUrlRequest source)
        //{
        //    return await _iIDNApiService.GameUrlAsync(source);
        //}

        /// <summary>
        /// 查詢玩家下注記錄接口
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("bethistory")]
        public async Task<ResponseBase<bethistoryResponse>> bethistoryAsync(bethistoryRequest source)
        {
            var begintime = new DateTime(2024, 8, 02, 16, 48, 00).AddHours(-1);
            //var endtime = new DateTime(2024, 1, 12, 16, 00, 00);
            source.date = begintime.ToString("yyyy-MM-dd");
            source.from = begintime.ToString("HH:mm:ss");
            source.to = begintime.AddHours(1).ToString("HH:mm:ss");

            var result = await _iIDNApiService.bethistoryAsync(source);
            return result;
        }


        /// <summary>
        /// 查詢玩家下注記錄接口
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("GetGameResult")]
        public async Task<GetGameResultResponse> GetGameResultAsync(GetGameResultRequest source)
        {
            //2024-08-29 14:07:39.000
            var begintime = new DateTime(2024, 8, 29, 14, 07, 39).AddHours(-1);
            //var endtime = new DateTime(2024, 1, 12, 16, 00, 00);
            source.date = begintime.ToString("yyMMdd");
            source.matchId = "2696928";
            source.gameId = "baccarat";
            _iIDNApiService.SetAuthToken("eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1NiJ9.eyJhdWQiOiIzMCIsImp0aSI6IjUwYmNjOWQzMGYxYzcwNWQ5MjM3Yjc1MWQwNTRlZTUyYjRiMjRjYWY3ZmVmMGZiMGEwOTJlOTA2NjhmM2FlOTE2MDRlMjQyZDkxZTI4OTMxIiwiaWF0IjoxNzI1MDA3NjgwLjY3NTY4NTksIm5iZiI6MTcyNTAwNzY4MC42NzU2ODksImV4cCI6MTc1NjU0MzY4MC42MzE3NzM5LCJzdWIiOiIiLCJzY29wZXMiOltdfQ.xMT5CtsWqRSWk9DuLumiUTjPQrbBrSPv6Mh7ZG-UZLke78hBZlYhXjaMryGhxXxwp8Up0cTrHWkZ05aJzXaix2AQRjfrq7i2Smx2ttDzCYiFmXXGOxkddr4G9_i1Qx3vrCWB7sO28nZLYVYcso6ZIxA284qhxZMMzurjd9wX18Uq8a-fYuEExyaMVpQ7wJZsxOmByUxK4kK5ILJpEKPxuZ28Ks_DKJYdK-jI5GMoHDrMP7rfnyLdMUJtJjWC6faWu2jC6ez6Mi3qaY3zMkkB1mfZVvDM6k9HkxHhmiZbqnO3Gl9-VEsBNvG3RbaXh1VunM54pPbO7nhN6JUWVsAkCvgwq251YsgDDJfwoPx5ThNOm7XQyQm-y7jHcI_wc7NMSW43cNi7HzgVHPQQ6JllIna8NdZpPvc6drF5kWYTyTcmrsgNqfLHoGX0bTSVBTykGQUfi93TXsWKCgWVP3WdjgfCgb6-nX6H8RUWAlEEYmWoJg4Th6kWIjL0l-KDjHGAgzba5Bt1BRYfx0NHOkb8G_8ycXuxyOF6J_DQd2ifhuG2KyPKrc7Sun4qDyziCWsYrxPgv_qMUyMb-9ATbvjEklCR1yY3fKT25ZOrVk1GvNqcPUIIqJFHqsxdXz2kvS682ytkHDrj5tNF_9gVgOATPMfX-nIV5SFoWvH-Bc_cwws");
            var result = await _iIDNApiService.GetGameResultAsync(source);
            return result;
        }




        /// <summary>
        /// refreashToken
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("RefreashTokenAPI")]
        public async Task<string> RefreashTokenAPI()
        {
            string result = "";
            await iDN_InterfaceService.refreashToken();
            return result;
        }
    }
}