using H1_ThirdPartyWalletAPI.Model.Game.FC.Request;
using H1_ThirdPartyWalletAPI.Model.Game.FC.Response;
using H1_ThirdPartyWalletAPI.Service;
using H1_ThirdPartyWalletAPI.Service.Common;
using H1_ThirdPartyWalletAPI.Service.Game.FC;
using H1_ThirdPartyWalletAPI.Service.Game.FC.Utility;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace H1_ThirdPartyWalletAPI.Controllers.Game.FC
{
    /// <summary>
    /// FC API
    /// </summary>
    [Route("/[controller]")]
    [ApiController]
    public class FCController : ControllerBase
    {

        private readonly IDBService _dbIdbService;
        private readonly ITransferWalletService _transferWalletService;
        private readonly IFCApiService _iFCapiservice;
        public FCController(IDBService dbIdbService, ITransferWalletService transferWalletService, IFCApiService iFCapiservice)
        {
            _dbIdbService = dbIdbService;
            _transferWalletService = transferWalletService;
            _iFCapiservice = iFCapiservice;
        }

        /// <summary>
        /// 建立帳號
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("CreateMember")]
        public async Task<CreateMemberResponse> CreateMemberAsync([FromBody] CreateMemberRequest source)
        {
            source.MemberAccount = "testJoshua";
            return await _iFCapiservice.CreateMember(source);
        }

        /// <summary>
        /// 取得遊戲列表
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("GetGameIconList")]
        public async Task<GetGameIconListResponse> GetGameIconList([FromBody] GetGameIconListRequest source)
        {
            return await _iFCapiservice.GetGameIconList(source);
        }

        /// <summary>
        /// 查询玩家基本信息
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("SearchMember")]
        public async Task<SearchMemberResponse> SearchMemberAsync([FromBody] SearchMemberRequest source)
        {
            source.MemberAccount = "testJoshua";
            return await _iFCapiservice.SearchMember(source);
        }

        /// <summary>
        /// 玩家钱包充提
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("SetPoints")]
        public async Task<SetPointsResponse> SetPointsAsync([FromBody] SetPointsRequest source)
        {
            source.MemberAccount = "testJoshua";
            source.TrsID = MD5Helper.GetFCTrsID();
            source.AllOut = 0;
            source.Points = 1000;
            return await _iFCapiservice.SetPoints(source);
        }

        /// <summary>
        /// 登录游戏
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("Login")]
        public async Task<LoginResponse> LoginAsync([FromBody] LoginRequest source)
        {
            source.MemberAccount = "testJoshua";
            source.GameID = 21003;
            source.LanguageID = 2;
            source.HomeUrl = "http://ts.bacctest.com/Home/Index";
            source.JackpotStatus = false;
            source.LoginGameHall = false;
            return await _iFCapiservice.Login(source);
        }

        /// <summary>
        /// 登录游戏
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("KickOut")]
        public async Task<KickOutResponse> KickOutAsync([FromBody] KickOutRequest source)
        {
            source.MemberAccount = "testJoshua";
            return await _iFCapiservice.KickOut(source);
        }

        /// <summary>
        /// 踢出全部玩家
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("KickoutAll")]
        public async Task<KickoutAllResponse> KickoutAllAsync([FromBody] KickoutAllRequest source)
        {
            return await _iFCapiservice.KickoutAll(source);
        }

        /// <summary>
        /// 3-8、交易纪录单笔查询
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("GetSingleBill")]
        public async Task<GetSingleBillResponse> GetSingleBillAsync([FromBody] GetSingleBillRequest source)
        {
            source.TrsID = MD5Helper.CutGuidTo30Characters(new Guid(source.TrsID));
            return await _iFCapiservice.GetSingleBill(source);
        }

        /// <summary>
        /// 3-10、取得玩家报表
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("GetPlayerReport")]
        public async Task<GetPlayerReportResponse> GetPlayerReportAsync([FromBody] GetPlayerReportRequest source)
        {
            source.MemberAccount = "testJoshua";
            source.LanguageID = 2;
            return await _iFCapiservice.GetPlayerReport(source);
        }

        /// <summary>
        /// 3-14、取得游戏纪录列表
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("GetRecordList")]
        public async Task<GetRecordListResponse> GetRecordListAsync([FromBody] GetRecordListRequest source)
        {
            source.StartDate = new DateTime(2023, 03, 29, 09, 44, 01, DateTimeKind.Local).AddHours(-12);
            source.EndDate = new DateTime(2023, 03, 29, 09, 45, 00, DateTimeKind.Local).AddHours(-12);
            return await _iFCapiservice.GetRecordList(source);
        }
        /// <summary>
        /// 3-15、取得充提交易纪录
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("GetBillList")]
        public async Task<GetBillListResponse> GetBillListAsync([FromBody] GetBillListRequest source)
        {
            source.StartDateTime = new DateTime(2023, 03, 22, 16, 06, 00, DateTimeKind.Local).AddHours(-12);
            source.EndDateTime = source.StartDateTime.AddMinutes(15);
            return await _iFCapiservice.GetBillList(source);
        }

        /// <summary>
        /// 3-16、每日会员游戏报表
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("GetMemberGameReport")]
        public async Task<GetMemberGameReportResponse> GetMemberGameReportAsync([FromBody] GetMemberGameReportRequest source)
        {
            source.Date = new DateTime(2023, 03, 22, 16, 06, 00, DateTimeKind.Local).AddHours(-12).ToString("yyyy-MM-dd");
            return await _iFCapiservice.GetMemberGameReport(source);
        }

        /// <summary>
        /// 3-20、取得历史游戏纪录列表
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("GetHistoryRecordList")]
        public async Task<GetHistoryRecordListResponse> GetHistoryRecordListAsync([FromBody] GetHistoryRecordListRequest source)
        {
            source.StartDate = new DateTime(2023, 03, 22, 16, 06, 00, DateTimeKind.Local).AddHours(-12);
            source.EndDate = source.StartDate.AddMinutes(1);
            return await _iFCapiservice.GetHistoryRecordList(source);
        }
    }
}
