using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Net;
using System.Text;
using ThirdPartyWallet.Common;
using ThirdPartyWallet.GameAPI.Service.Game.WE;
using ThirdPartyWallet.Share.Model.Game.WE;
using ThirdPartyWallet.Share.Model.Game.WE.Request;
using ThirdPartyWallet.Share.Model.Game.WE.Response;
using WEsetup = ThirdPartyWallet.Share.Model.Game.WE.WE;



namespace H1_ThirdPartyWalletAPI.Service.Game.WE
{
    public class WEApiService : IWEApiService
    {
        public const string PlatformName = "WE";
        private readonly LogHelper<WEApiService> _logger;
        private readonly IOptions<WEConfig> _options;
        private readonly HttpClient _httpClient;

        public WEApiService(LogHelper<WEApiService> logger, IOptions<WEConfig> options,
            HttpClient httpClient)
        {
            _logger = logger;
            _options = options;
            _httpClient = httpClient;
        }
        /// <summary>
        /// 會員登入
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public async Task<LoginResponse> LoginAsync(LoginRequest source)
        {
            var url = _options.Value.WE_URL + $"/player/launch";
            source.operatorID = _options.Value.WE_operatorrID;
            source.requestTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            string sha256string = _options.Value.WE_appSecret + source.requestTime;
           var accessKey=  Halper.ComputeSha256Hash(sha256string);
           
            return await NexApiHandle<LoginRequest, LoginResponse>(url, source, accessKey);
        }
        /// <summary>
        /// 登出會員
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public async Task<LogoutResponse> LogoutAsync(LogoutRequest source)
        {
            var url = _options.Value.WE_URL + "/player/logout";
            source.operatorID = _options.Value.WE_operatorrID;
            source.requestTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            string MD5string = _options.Value.WE_appSecret + _options.Value.WE_operatorrID + source.playerID +
                               source.requestTime;

            return await ApiHandle<LogoutRequest, LogoutResponse>(url, source, MD5string);
        }
        /// <summary>
        /// 登出全部會員
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public async Task<LogoutAllResponse> LogoutAllAsync(LogoutAllRequest source)
        {
            var url = _options.Value.WE_URL + "/player/logout";
            source.operatorID = _options.Value.WE_operatorrID;
            source.requestTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            string MD5string = _options.Value.WE_appSecret + _options.Value.WE_operatorrID + source.playerID + source.requestTime;

            return await ApiHandle<LogoutAllRequest, LogoutAllResponse>(url, source, MD5string);
        }
        /// <summary>
        /// 創建會員
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public async Task<CreateUserResponse> CreateUserAsync(CreateUserRequest source)
        {
            var url = _options.Value.WE_URL + "/player/create";
            source.operatorID = _options.Value.WE_operatorrID;
            source.requestTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            string MD5string = _options.Value.WE_appSecret + source.nickname + _options.Value.WE_operatorrID + source.playerID +
                               source.requestTime;

            return await ApiHandle<CreateUserRequest, CreateUserResponse>(url, source, MD5string);
        }
        /// <summary>
        /// 存款
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public async Task<DepositResponse> DepositAsync(DepositRequest source)
        {
            var url = _options.Value.WE_URL + "/player/deposit";
            source.operatorID = _options.Value.WE_operatorrID;
            source.requestTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            string MD5string = source.amount + _options.Value.WE_appSecret + _options.Value.WE_operatorrID + source.playerID +
                               source.requestTime + source.uid;

            return await ApiHandle<DepositRequest, DepositResponse>(url, source, MD5string);
        }
        /// <summary>
        /// 取款
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public async Task<WithdrawResponse> WithdrawAsync(WithdrawRequest source)
        {
            var url = _options.Value.WE_URL + "/player/withdraw";
            source.operatorID = _options.Value.WE_operatorrID;
            source.requestTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            string MD5string = source.amount + _options.Value.WE_appSecret + _options.Value.WE_operatorrID + source.playerID +
                               source.requestTime + source.uid;

            return await ApiHandle<WithdrawRequest, WithdrawResponse>(url, source, MD5string);
        }
        /// <summary>
        /// 查詢餘額
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public async Task<BalanceResponse> BalanceAsync(BalanceRequest source)
        {
            var url = _options.Value.WE_URL + "/player/balance";
            source.operatorID = _options.Value.WE_operatorrID;
            source.requestTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            string MD5string = _options.Value.WE_appSecret + _options.Value.WE_operatorrID + source.playerID +
                               source.requestTime;

            return await ApiHandle<BalanceRequest, BalanceResponse>(url, source, MD5string);
        }
        /// <summary>
        /// 查詢預設限紅
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public async Task<BetlimitResponse> BetlimitAsync(BetlimitRequest source)
        {
            var url = _options.Value.WE_URL + "/player/getbetlimit";
            source.operatorID = _options.Value.WE_operatorrID;
            source.requestTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            string MD5string = _options.Value.WE_appSecret + _options.Value.WE_operatorrID + source.playerID +
                               source.requestTime;

            return await ApiHandle<BetlimitRequest, BetlimitResponse>(url, source, MD5string);
        }
        /// <summary>
        /// 遊戲館遊戲桌ID
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public async Task<List<GameListResponse.Datum>> GameListAsync(GameListRequest source)
        {
            var url = _options.Value.WE_URL + "/game/gamelist";
            source.operatorID = _options.Value.WE_operatorrID;
            source.requestTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            string MD5string = _options.Value.WE_appSecret + source.gamecategory + _options.Value.WE_operatorrID + source.requestTime;

            var Gamedata = await ApiHandle<GameListRequest, GameListResponse>(url, source, MD5string);
            return Gamedata.data.ToList();


            //var Gamelist = WEsetup.GetGamelist();
            //return Gamedata.data.Where(x => Gamelist.Contains(x.zh)).ToList();

        }

        /// <summary>
        /// 交易紀錄
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public async Task<TransferResponse> TransferAsync(TransferRequest source)
        {
            var url = _options.Value.WE_URL + "/history/transfer";
            source.operatorID = _options.Value.WE_operatorrID;
            source.requestTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            string MD5string = _options.Value.WE_appSecret + source.endTime + source.limit +
                               _options.Value.WE_operatorrID + source.playerID + source.requestTime + source.startTime + source.uid;

            return await ApiHandle<TransferRequest, TransferResponse>(url, source, MD5string);
        }
        /// <summary>
        /// 取住單
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public async Task<BetRecordResponse> BetRecordAsync(BetRecordRequest source)
        {
            var url = _options.Value.WE_URL + "/history/bet";
            source.operatorID = _options.Value.WE_operatorrID;
            source.requestTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            string MD5string = _options.Value.WE_appSecret + source.betID + source.betstatus +
                               source.category + source.endTime + source.isSettlementTime + source.limit +
                               source.offset + _options.Value.WE_operatorrID + source.playerID + source.requestTime + source.startTime;

            return await ApiHandle<BetRecordRequest, BetRecordResponse>(url, source, MD5string);
        }
        /// <summary>
        /// 小時帳
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public async Task<ReportHourResponse> ReportHourAsync(ReportHourRequest source)
        {
            var url = _options.Value.WE_URL + "/report/summary";
            source.operatorID = _options.Value.WE_operatorrID;
            source.requestTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            string MD5string = _options.Value.WE_appSecret + source.endTime + source.isSettlementTime +
            _options.Value.WE_operatorrID + source.requestTime + source.startTime;

            return await ApiHandle<ReportHourRequest, ReportHourResponse>(url, source, MD5string);
        }
        /// <summary>
        /// 細單
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public async Task<BetDetailUrlResponse> BetDetailUrlAsync(BetDetailUrlRequest source)
        {
            var url = _options.Value.WE_URL + "/report/betrecord";
            source.operatorID = _options.Value.WE_operatorrID;
            source.requestTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            string MD5string = _options.Value.WE_appSecret + source.betID + _options.Value.WE_operatorrID +
                               +source.requestTime;

            return await ApiHandle<BetDetailUrlRequest, BetDetailUrlResponse>(url, source, MD5string);
        }
        /// <summary>
        /// 健康度
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public async Task<dynamic> HealthCheckAsync(HealthCheckRequest source)
        {
            var url = _options.Value.WE_URL + "/check";
            source.operatorID = _options.Value.WE_operatorrID;
            source.appSecret = _options.Value.WE_appSecret;
            var postData = new Dictionary<string, string>
            {
                {"operatorID", _options.Value.WE_operatorrID},
                {"appSecret", _options.Value.WE_appSecret}
            };
            return await Post(url, postData, null);
        }
        /// <summary>
        /// 設定限紅
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public async Task<SetbetlimitResponse> SetBetLimitAsync(SetbetlimitRequest source)
        {
            var url = _options.Value.WE_URL + "/player/setbetlimit";
            source.operatorID = _options.Value.WE_operatorrID;
            source.requestTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            string MD5string = _options.Value.WE_appSecret + source.betlimit + _options.Value.WE_operatorrID + source.playerID
                               + source.requestTime;

            return await ApiHandle<SetbetlimitRequest, SetbetlimitResponse>(url, source, MD5string);
        }
        /// <summary>
        /// POST的傳輸組成
        /// </summary>
        /// <typeparam name="TRequest"></typeparam>
        /// <typeparam name="TResponse"></typeparam>
        /// <param name="url"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        private async Task<TResponse> ApiHandle<TRequest, TResponse>(string url, TRequest source, string MD5String)
        {
            string accessKey = Halper.MD5encryption(MD5String).ToLower();
            var headers = new Dictionary<string, string>
            {
                {"signature",accessKey},  // 根據需求，可能需要調整授權方式
               
            };
            var data = Halper.GetDictionary(source);
            var responseData = await Post(url, data, headers);
            return JsonConvert.DeserializeObject<TResponse>(responseData);
        }


        private async Task<TResponse> NexApiHandle<TRequest, TResponse>(string url, TRequest source, string accessKey)
        {
        
            var data = Halper.GetDictionary(source);

            var headers = new Dictionary<string, string>
            {
                {"signature",accessKey},  // 根據需求，可能需要調整授權方式
               
            };
            var responseData = await Post(url, data, headers);
            return JsonConvert.DeserializeObject<TResponse>(responseData);
        }

        /// <summary>
        /// 呼叫廠商Post方法
        /// </summary>
        /// <param name="url">APIUrl</param>
        /// <param name="postData">廠商API Request Model</param>
        /// <param name="headers">headers</param>
        /// <returns></returns>
        private async Task<string> Post(string url, Dictionary<string, string> postData, Dictionary<string, string> headers = null)
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, url);
            if (headers != null)
            {
                foreach (var item in headers)
                {
                    request.Headers.TryAddWithoutValidation(item.Key, item.Value);
                }
            }

            // 序列化 postData，注意这里 postData 可以是任何类型
            request.Content = new FormUrlEncodedContent(postData);

            using var response = await _httpClient.SendAsync(request);
            var body = await response.Content.ReadAsStringAsync();



            return body;
        }

    }
}
