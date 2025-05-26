using Google.Protobuf.WellKnownTypes;
using H1_ThirdPartyWalletAPI.Model.Game.NEXTSPIN.Request;
using H1_ThirdPartyWalletAPI.Model.Game.NEXTSPIN.Response;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace H1_ThirdPartyWalletAPI.Service.Game.NEXTSPIN
{
    public partial class NEXTSPINApiService : INEXTSPINApiService
    {
        /// <summary>
        /// Log長度上限
        /// </summary>
        private const int LOG_MAX = 10000;
        private static readonly SemaphoreSlim recordLock = new(1);
        private readonly TimeSpan recordAPIDelay = TimeSpan.FromSeconds(5);

        /// <summary>
        /// 查询用户信息
        /// 使用场景: 查询用户信息(如余额)
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public Task<GetAcctInfoResponse> GetAcctInfoAsync(GetAcctInfoRequest request)
        {
            return PostAsync<GetAcctInfoRequest, GetAcctInfoResponse>("getAcctInfo", request);
        }

        /// <summary>
        /// 存款
        /// 使用场景: 由 merchant 将相应的金额存入 Game Provider 中用户的余额
        /// 
        /// 特别注意:
        /// 1.serialNo 是消息标识, 同一个 serialNo 只会有一个 request/response, 请确保每次生成的 serialNo 是唯一的值.
        /// 2.建议使用 GUID/UUID, 或是日期+时间+随机数的字符串.
        /// 3.Acct ID 格式: [a-zA-Z0-9_-]{5, 30}. 例子: TestPlayer_1
        /// 4.若未建立會員將同時建立會員
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public Task<DepositResponse> DepositAsync(DepositRequest request)
        {
            return PostAsync<DepositRequest, DepositResponse>("deposit", request);
        }

        /// <summary>
        /// 取款
        /// 使用场景: 由 merchant 将相应的金额存入 Game Provider 中用户的余额提取
        /// 
        /// 特别注意:
        /// 1.serialNo 是消息标识, 同一个 serialNo 只会有一个 request/response, 请确保每次生成的 serialNo 是唯一的值.
        /// 2.建议使用 GUID/UUID, 或是日期+时间+随机数的字符串.
        /// 3.Acct ID 格式: [a-zA-Z0-9_-]{5, 30}. 例子: TestPlayer_1
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public Task<WithdrawResponse> WithdrawAsync(WithdrawRequest request)
        {
            return PostAsync<WithdrawRequest, WithdrawResponse>("withdraw", request);
        }

        /// <summary>
        /// 查询转帐记录
        /// 使用场景 ： 查询转帐状态，供核对转帐数据.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public Task<CheckTransferResponse> CheckTransferAsync(CheckTransferRequest request)
        {
            return PostAsync<CheckTransferRequest, CheckTransferResponse>("checkTransfer", request);
        }

        /// <summary>
        /// 查询用户下注记录
        /// 使用场景: 查询下注记录
        /// 
        /// 特别注意:
        /// 1.建议商家将每次搜索时间调至最多两个小时
        /// 2.且让其每次查询间隔时间为3至5秒钟
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<GetBetHistoryResponse> GetBetHistoryAsync(GetBetHistoryRequest request)
        {
            var logFormat = (string response) => {
                if (response.Length <= LOG_MAX)
                    return response;

                var resObj = JsonConvert.DeserializeObject<GetBetHistoryResponse>(response);

                return JsonConvert.SerializeObject(new
                {
                    resObj.code,
                    resObj.msg,
                    resObj.serialNo,
                    dataCount = resObj.list?.Length ?? 0,
                    resObj.pageCount,
                    resObj.resultCount
                });
            };

            await recordLock.WaitAsync();
            _ = Task.Run(async () =>
            {
                await Task.Delay(recordAPIDelay);
                recordLock.Release();
            });

            return await PostAsync<GetBetHistoryRequest, GetBetHistoryResponse>("getBetHistory", request, logFormat);
        }

        /// <summary>
        /// 强制退出当前在线用户
        /// 使用场景: 当 merchant 网站进行维护时, 如果不希望当前在游戏中的玩家继续游戏, 则调用本接口, 将所属的在线用户强制退出
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public Task<KickAcctResponse> KickAcctAsync(KickAcctRequest request)
        {
            return PostAsync<KickAcctRequest, KickAcctResponse>("kickAcct", request);
        }

        /// <summary>
        /// 强制退出当前在线用户
        /// 使用场景: 当 merchant 网站进行维护时, 如果不希望当前在游戏中的玩家继续游戏, 则调用本接口, 将所属的在线用户强制退出
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public Task<GetReportResponse> GetReportAsync(GetReportRequest request)
        {
            return PostAsync<GetReportRequest, GetReportResponse>("getProfitLoss", request);
        }
        /// <summary>
        /// 查询域名列表 健康度
        /// 使用场景: 查询域名列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public Task<GetDomainListResponse> GetDomainListAsync(GetDomainListRequest request)
        {
            return PostAsync1<GetDomainListRequest, GetDomainListResponse>("getDomainList", request);
        }
    }
}
