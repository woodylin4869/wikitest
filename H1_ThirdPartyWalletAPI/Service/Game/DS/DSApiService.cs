using H1_ThirdPartyWalletAPI.Model.Game.DS;
using H1_ThirdPartyWalletAPI.Model.Game.DS.Request;
using H1_ThirdPartyWalletAPI.Model.Game.DS.Response;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using H1_ThirdPartyWalletAPI.Code;
using H1_ThirdPartyWalletAPI.Model;
using H1_ThirdPartyWalletAPI.Model.Config;
using H1_ThirdPartyWalletAPI.Model.DataModel;
using H1_ThirdPartyWalletAPI.Service.Common;
using Npgsql;

namespace H1_ThirdPartyWalletAPI.Service.Game.DS
{
    public class DSApiService : DSApiServiceBase, IDSApiService
    {
        private readonly ILogger<DSApiServiceBase> _logger;
        public DSApiService(ILogger<DSApiServiceBase> logger, IHttpClientFactory httpClientFactory) : base(logger, httpClientFactory)
        {
            _logger = logger;
        }

        #region player

        public async Task<CreateMemberRepsonse> CreateMember(CreateMemberRequest request)
        {
            var RequestPath = "member/create";
            var result = await PostAsync<CreateMemberRequest, CreateMemberRepsonse>(request, RequestPath);
            if (result.result.code != (int)error_code.succeeded && result.result.code != (int)error_code.duplicated)
            {
                throw new Exception(Enum.GetName(typeof(error_code), result.result.code));
            }

            return result;
        }

        public async Task<LoginGameResponse> LoginGame(LoginGameRequest request)
        {
            var RequestPath = "member/login_game";
            var result = await PostAsync<LoginGameRequest, LoginGameResponse>(request, RequestPath);
            if (result.result.code != (int)error_code.succeeded)
            {
                throw new Exception(Enum.GetName(typeof(error_code), result.result.code));
            }

            return result;
        }

        public async Task<LogoutResponse> Logout(LogoutRequest request)
        {
            var RequestPath = "member/logout";
            var result = await PostAsync<LogoutRequest, LogoutResponse>(request, RequestPath);
            if (result.result.code != (int)error_code.succeeded)
            {
                throw new Exception(Enum.GetName(typeof(error_code), result.result.code));
            }

            return result;
        }

        public async Task<GetLoginInfoResponse> GetLoginInfo(GetLoginInfoRequest request)
        {
            var RequestPath = "member/login_info";
            var result = await PostAsync<GetLoginInfoRequest, GetLoginInfoResponse>(request, RequestPath);
            if (result.result.code != (int)error_code.succeeded)
            {
                throw new Exception(Enum.GetName(typeof(error_code), result.result.code));
            }

            return result;
        }

        public async Task<GetOnlineMembersResponse> GetOnlineMembers()
        {
            var RequestPath = "member/online_list ";
            var result = await PostAsync<GetOnlineMembersResponse>(RequestPath);
            if (result.result.code != (int)error_code.succeeded)
            {
                throw new Exception(Enum.GetName(typeof(error_code), result.result.code));
            }

            return result;
        }

        public async Task<GetOnlineMemberBalanceResponse> GetOnlineMemberBalance(GetOnlineMemberBalanceRequest request)
        {
            var RequestPath = "member/online_balance_list";
            var result = await PostAsync<GetOnlineMemberBalanceRequest, GetOnlineMemberBalanceResponse>(request, RequestPath);
            if (result.result.code != (int)error_code.succeeded)
            {
                throw new Exception(Enum.GetName(typeof(error_code), result.result.code));
            }

            return result;
        }

        #endregion

        #region Transaction

        public async Task<TransferResponse> Transfer(TransferRequest request)
        {
            var RequestPath = "trans/transfer";
            var result = await PostAsync<TransferRequest, TransferResponse>(request, RequestPath);
            if (result.result.code != (int)error_code.succeeded)
            {
                throw new ExceptionMessage(result.result.code, result.result.msg);
            }
            if (result.trans_id == null || result.trans_id == "" || result.trans_id.Length < 2)
            {
                result.result.code = (int)error_code.trnasfer_response_format_fail;
                throw new Exception(Enum.GetName(typeof(error_code), result.result.code));
            }
            return result;
        }

        public async Task<VerifyResponse> verify(VerifyRequest request)
        {
            var RequestPath = "trans/verify";
            var result = await PostAsync<VerifyRequest, VerifyResponse>(request, RequestPath);
            if (result.result.code != (int)error_code.succeeded && result.result.code!= (int)error_code.information_not_found)
            {
                throw new Exception(Enum.GetName(typeof(error_code), result.result.code));
            }            
            return result;
        }

        public async Task<CheckBalanceResponse> checkBalance(CheckBalanceRequest request)
        {
            var RequestPath = "trans/check_balance";
            var result = await PostAsync<CheckBalanceRequest, CheckBalanceResponse>(request, RequestPath);
            if (result.result.code != (int)error_code.succeeded)
            {
                throw new Exception(Enum.GetName(typeof(error_code), result.result.code));
            }

            return result;
        }

        #endregion

        #region BetRecord

        /// <summary>
        /// 查詢注單限制 查詢範圍一小時為限，資料只保存七天，若超出範圍查詢則會查詢失敗，返回 Code 1012。
        /// 查詢注單限制 十秒內只允許五次查詢，超過五次返回 Code 16。
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<GetBetRecordResponse> GetBetRecord(GetBetRecordRequest request)
        {
            var RequestPath = "record/get_bet_records";
            var result = await PostWithOutBaseRequestAsync<GetBetRecordRequest, GetBetRecordResponse>(request, RequestPath);
            if (result.result.code != (int)error_code.succeeded)
            {
                throw new Exception(Enum.GetName(typeof(error_code), result.result.code));
            }

            return result;
        }

        public async Task<GetBetDetailPageResponse> GetBetDetailPage(GetBetDetailPageRequest request)
        {
            var RequestPath = "record/get_bet_detail_page";
            var result = await PostWithOutBaseRequestAsync<GetBetDetailPageRequest, GetBetDetailPageResponse>(request, RequestPath);
            if (result.result.code != (int)error_code.succeeded)
            {
                throw new Exception(Enum.GetName(typeof(error_code), result.result.code));
            }

            return result;
        }

        public async Task<GetBetHistoryResponse> GetBetHistoryByPlayer(GetBetHistoryRequest request)
        {
            var RequestPath = "record/bet_history";
            var result = await PostAsync<GetBetHistoryRequest, GetBetHistoryResponse>(request, RequestPath);
            if (result.result.code != (int)error_code.succeeded)
            {
                throw new Exception(Enum.GetName(typeof(error_code), result.result.code));
            }

            return result;
        }

        public async Task<GetMemberBetRecordByHourResponse> GetMemberBetRecordByHour(
            GetMemberBetRecordByHourRequest request)
        {
            var RequestPath = "record/get_member_bet_records_by_hour";
            var result =
                await PostWithOutBaseRequestAsync<GetMemberBetRecordByHourRequest, GetMemberBetRecordByHourResponse>(
                    request, RequestPath);
            if (result.result.code != (int)error_code.succeeded)
            {
                throw new Exception(Enum.GetName(typeof(error_code), result.result.code));
            }

            return result;
        }

        /// <summary>
        /// 代理下注紀錄匯總
        /// 1小時內最多執行4次，每次查詢區間最長為24小時
        /// 最久能查到的時間為90天前到現在
        /// 資料有兩小時延遲(最快能看到的統計資料= 現在時間-2h)
        /// 最小查詢的時間單位為小時，ex: 00:00:00 ~ 23:59:59或 01:00:00 ~ 01:59:59
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<GetAgentSummaryBetRecordsResponse> GetAgentSummaryBetRecords(
            GetAgentSummaryBetRecordsRequest request)
        {
            var RequestPath = "record/get_agent_summary_bet_records";
            var result =
                await PostWithOutBaseRequestAsync<GetAgentSummaryBetRecordsRequest, GetAgentSummaryBetRecordsResponse>(
                    request, RequestPath);
            if (result.result.code != (int)error_code.succeeded)
            {
                throw new Exception(Enum.GetName(typeof(error_code), result.result.code));
            }

            return result;
        }

        #endregion

        #region Games

        public async Task<GetGameInfoStateListResponse> GetGameInfoStateList()
        {
            var RequestPath = "config/get_game_info_state_list";
            var result = await PostAsync<GetGameInfoStateListResponse>(RequestPath);
            if (result.result.code != (int)error_code.succeeded)
            {
                throw new Exception(Enum.GetName(typeof(error_code), result.result.code));
            }

            return result;
        }

        #endregion
    }
}
