using H1_ThirdPartyWalletAPI.Service.Game.CR;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using ThirdPartyWallet.Share.Model.Game.CR.Request;
using ThirdPartyWallet.Share.Model.Game.CR.Response;
using System.Collections.Generic;
using System.Globalization;

namespace H1_ThirdPartyWalletAPI.Controllers.Game.CR
{
    /// <summary>
    /// CR API
    /// </summary>
    [Route("/[controller]")]
    [ApiController]
    public class CRController : ControllerBase
    {

        private readonly ICRApiService _iCRApiService;

        public CRController(ICRApiService iCRApiService)
        {
            _iCRApiService = iCRApiService;
        }


        /// <summary>
        /// 3.1 登入系統AGLogin
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("AGLogin")]
        public async Task<AGLoginResponse> AGLoginAsync(AGLoginRequest source)
        {
            return await _iCRApiService.AGLoginAsync(source);
        }


        /// <summary>
        /// 3.2 建立新的會員CreateMember
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("CreateMember")]
        public async Task<CreateMemberResponse> CreateMemberAsync(CreateMemberRequest source)
        {
            source.currency = "THB";
            source.memname = "Joshua";

            return await _iCRApiService.CreateMemberAsync(source);
        }

        /// <summary>
        /// 存款Deposit 
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("Deposit")]
        public async Task<DepositResponse> DepositAsync(DepositRequest source)
        {
            source.memname = "Joshua";
            source.amount = 66;
            source.payno = Guid.NewGuid().ToString();

            return await _iCRApiService.DepositAsync(source);
        }


        /// <summary>
        /// 3.4 提款Withdraw 
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("Withdraw")]
        public async Task<WithdrawResponse> WithdrawAsync(WithdrawRequest source)
        {
            source.memname = "Joshua";
            source.amount = 44;
            source.payno = Guid.NewGuid().ToString();

            return await _iCRApiService.WithdrawAsync(source);
        }

        /// <summary>
        /// 3.5 會員登入 MemLogin
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("MemLogin")]
        public async Task<MemLoginResponse> MemLoginAsync(MemLoginRequest source)
        {
            source.memname = "Joshua";

            return await _iCRApiService.MemLoginAsync(source);
        }


        /// <summary>
        /// 3.6 登入遊戲LaunchGame
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("LaunchGame")]
        public async Task<LaunchGameResponse> LaunchGameAsync(LaunchGameRequest source)
        {
            source.memname = "Joshua";
            source.remoteip = "127.0.0.1";
            source.machine = "PC";
            source.langx = "zh-tw";
            source.remoteip = "127.0.0.1";
            source.isSSL = "Y";
            source.currency = "THB";

            return await _iCRApiService.LaunchGameAsync(source);
        }

        /// <summary>
        /// 3.8 確認會員目前餘額chkMemberBalance
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("chkMemberBalance")]
        public async Task<chkMemberBalanceResponse> chkMemberBalanceAsync(chkMemberBalanceRequest source)
        {
            source.memname = "joshua";

            return await _iCRApiService.chkMemberBalanceAsync(source);
        }



        /// <summary>
        /// 3.12 全部會員的注單資料ALLWager
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("ALLWager")]
        public async Task<ALLWagerResponse> ALLWagerAsync(ALLWagerRequest source)
        {
            source.dateStart = new DateTime(2024, 6, 5, 10, 0, 0, DateTimeKind.Local).AddHours(-12);
            source.dateEnd = new DateTime(2024, 6, 5, 12, 0, 0, DateTimeKind.Local).AddHours(-12);
            source.settle = 1;
            source.langx = "en-us";
            source.page = 1;

            ALLWagerResponse aLLWagerResponse = await _iCRApiService.ALLWagerAsync(source, 2);
           

            //foreach (var wager in aLLWagerResponse.wager_data)
            //{
            //    if (wager.parlaysub != null)
            //    {
            //        // Convert the dynamic object to a JObject
            //        JObject parlaySubObject = JObject.Parse(JsonConvert.SerializeObject(wager.parlaysub));
            //        int parlayCount = parlaySubObject.Count;

            //        decimal combinedIoratio = 1;

            //        foreach (var parlay in wager.parlaysub.Values)
            //        {
            //            if (decimal.TryParse(parlay.ioratio, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal ioratio))
            //            {
            //                combinedIoratio *= ioratio;
            //            }
            //        }
            //        Console.WriteLine($"Parlay count: {parlayCount}");
            //    }
            //}
            return aLLWagerResponse;
        }

        /// <summary>
        /// 3.18  將單一會員登出 KickOutMem
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("KickOutMem")]
        public async Task<KickOutMemResponse> KickOutMemAsync(KickOutMemRequest source)
        {
            source.memname = "Joshua";

            return await _iCRApiService.KickOutMemAsync(source);
        }
        /// <summary>
        /// 3.18  將單一會員登出 KickOutMem
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("healthcheck")]
        public async Task<string> healthcheckAsync()
        {
            return await _iCRApiService.healthcheckAsync();
        }

        /// <summary>
        /// 3.16 查詢存提款記錄 ChkTransInfo
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("ChkTransInfo")]
        public async Task<ChkTransInfoResponse> ChkTransInfoAsync(ChkTransInfoRequest source)
        {
            source.memname = "Joshua";
            source.transidtype = "1";
            //source.transid = Guid.NewGuid().ToString();
            source.transid = "568566a0-520b-4121-9874-f767ae2480f0";
            return await _iCRApiService.ChkTransInfoAsync(source);
        }


        /// <summary>
        ///  3.22 檢查報表統計資料 CheckAGReport
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("CheckAGReport")]
        public async Task<CheckAGReportResponse> CheckAGReportAsync(CheckAGReportRequest source)
        {
            source.datestart = new DateTime(2024, 5, 28, 0, 0, 0, DateTimeKind.Local).AddHours(-12);
            source.dateend = new DateTime(2024, 5, 29, 0, 0, 0, DateTimeKind.Local).AddHours(-12);
            source.settle = "1";
            return await _iCRApiService.CheckAGReportAsync(source);
        }
    }

}
