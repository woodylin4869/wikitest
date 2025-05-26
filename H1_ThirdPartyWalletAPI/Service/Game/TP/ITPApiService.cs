using H1_ThirdPartyWalletAPI.Model.Game.TP.Request;
using H1_ThirdPartyWalletAPI.Model.Game.TP.Response;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace H1_ThirdPartyWalletAPI.Service.Game.TP
{
    public interface ITPApiService
    {
        /// <summary>
        /// 代理商遊戲開關
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<TpResponse<string>> AgentGameSwith(AgentGameSwitchRequest request);

        /// <summary>
        /// 查詢注單
        /// 依注單產生時間
        /// 
        /// 搜尋區間是依api服務爬到注單的時間,所以跟投注時間有落差是正常現象
        /// 
        /// 查詢日期範圍:
        /// 搜尋時間不可早於60天前
        /// 搜尋時間區間不可大於30分鐘
        /// 
        /// 查詢注單頻率:
        /// 建議20~30秒爬一次
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<TpResponse<BetLogResponse>> BetLog(BetLogRequest request);

        /// <summary>
        /// 查詢注單
        /// 依投注時間
        /// 
        /// 搜尋區間是依api服務爬到注單的時間,所以跟投注時間有落差是正常現象
        /// 
        /// 查詢日期範圍:
        /// 搜尋時間不可早於60天前
        /// 搜尋時間區間不可大於5分鐘
        /// 
        /// 查詢注單頻率:
        /// 建議20~30秒爬一次
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<TpResponse<BetLogResponse>> BetLogByBetTime(BetLogRequest request);

        /// <summary>
        /// 玩家更換密碼
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<TpResponse<ChangePasswordResponse>> ChangePassword(ChangePasswordRequest request);

        /// <summary>
        /// 查詢登出所有玩家進度
        /// 
        /// 請使用 登出所有玩家 (kickall) api回覆的查詢序號check_key查詢處理進度
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<TpResponse<CheckKickAllResponse>> CheckKickAll(CheckKickAllRequest request);

        /// <summary>
        /// 查詢玩家帳號是否存在
        /// 
        /// true =該帳號存在， false =該帳號不存在
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<TpResponse<bool>> CheckPlayer(CheckPlayerRequest request);

        /// <summary>
        /// 存款
        /// 
        /// 若出現timeout或其他網路問題 (http status code 不是200) 時, 請用相同transaction_id重試以避免重複存款
        /// 出現ErrorCode 1006, 1015, 1999，該轉帳可能還在處理中，請用相同transaction_id重試以避免重複存款
        /// 
        /// PG娛樂城、IM : 使用幣別為越南盾(VND)、印尼盾(IDR)時，娛樂城內遊戲顯示的金額單位為1000，amount參數(轉帳金額)有效位數只支援到十分位。EX : 1230為有效金額, 1234、1234.5、1234.56為無效金額
        /// MEGA娛樂城、BG真人、AB真人 : 轉帳到MEGA、BG、AB時，amount參數(轉帳金額)不可小於1.00
        /// SA真人: 越南盾(VND)、印尼盾(IDR)、緬甸元(MMK) 遊戲內顯示的金額單位為千分之一, amount參數(轉帳金額)有效位數只支援到十分位。EX: 1230為有效金額, 1234、1234.5、1234.56為無效金額
        /// 3SING、GTI: 越南盾(VND)、印尼盾(IDR) amount參數(轉帳金額)只接受小數點後一位
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<TpResponse<DepositResponse>> Deposit(DepositRequest request);

        /// <summary>
        /// 遊戲廠商列表
        /// </summary>
        /// <returns></returns>
        Task<TpResponse<List<GamehallsResponse>>> Gamehalls();

        /// <summary>
        /// 遊戲列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<TpResponse<List<GameListResponse>>> GameList(GameListRequest request);

        /// <summary>
        /// 遊戲圖片zip壓縮檔
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<Stream> GameImage(GameImageRequest request);

        /// <summary>
        /// Player 取得遊戲連結
        /// 
        /// 目前只有AP愛棋牌的開房遊戲有使用register_url、room_id參數
        /// register_url : 取得遊戲連結時傳入平台端的註冊連結，AP愛棋牌會在開房遊戲中將此註冊連結加上房間編號，組成邀請連結
        /// room_id : 取得遊戲連結時傳入遊戲房間id，則可透過取得的遊戲連結直接進到該遊戲房間
        /// 
        /// VG棋牌的遊戲需要使用agent_id參數
        /// agent_id [Optional]: linecode 控制盈利率分組功能 組名為 1-600 若沒有帶入則不會對該玩家的分組作出改動
        /// 
        /// RM棋牌遊戲若需要分組可帶入agent_id參數，否則預設帶入玩家所屬代理的代碼作為分組參數
        /// 
        /// ING電子百家樂若需要設定分組可帶入agent_id參數 組名為 1-5 若沒有帶入則會進入預設分組
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<TpResponse<GameLinkResponse>> GameLink(GameLinkRequest request);

        /// <summary>
        /// 登出所有玩家
        /// 
        /// 同一遊戲商的請求若進行中尚未完成，api將回覆錯誤701:Kickall request duplicate.
        /// 請求成功時，api會回覆查詢序號check_key，請使用 查詢登出所有玩家進度(check-kickall) api，並帶入此check_key查詢處理進度
        /// 查詢序號check_key有效期限為發出請求後的24小時
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<TpResponse<KickAllResponse>> KickAll(KickAllRequest request);

        /// <summary>
        /// 代理商取得彩票後台連結
        /// 
        /// 目前只有樂利彩票的遊戲有使用agent_id參數 [Required]
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<TpResponse<LotteryAdministrationLinkResponse>> LotteryAdministrationLink(LotteryAdministrationLinkRequest request);

        /// <summary>
        /// 注單詳細資訊 PlayCheck
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<TpResponse<PlayCheckResponse>> PlayCheck(PlayCheckResquest request);

        /// <summary>
        /// 建立 Player
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<TpResponse<PlayerResponse>> Player(PlayerRequest request);

        /// <summary>
        /// 查詢玩家投注量統計
        /// 
        /// 功能：查詢時間區間內，總注單數、總有效投注、總派彩等統計資料
        /// 本 API 並非即時查詢注單，而是每15分鐘結算一次
        /// 本 API 會因為遊戲商派彩快慢，與實際注單有落差
        /// 
        /// 查詢區間最小為1小時，不支援分秒查詢
        /// 若不輸入開始時間與結束時間參數，則預設代入近一小時時間區間
        /// timezone 可以代入-04、+00、+08
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<TpResponse<List<PlayerBettingStatisticsResponse>>> PlayerBettingStatistics(PlayerBettingStatisticsRequest request);

        /// <summary>
        /// 玩家登出
        /// 
        /// 目前只有樂利彩票的遊戲有使用agent_id參數 [Required]
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<TpResponse<string[]>> PlayerLogout(PlayerLogoutRequest request);

        /// <summary>
        /// 玩家遊戲錢包查詢
        /// 目前只有樂利彩票的遊戲有使用agent_id參數 [Required]
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<TpResponse<PlayerWalletResponse>> PlayerWallet(PlayerWalletRequest request);

        /// <summary>
        /// 單筆交易紀錄查詢
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<TpResponse<SingleTransactionResponse>> SingleTransaction(SingleTransactionRequest request);

        /// <summary>
        /// 查詢投注量統計
        /// 
        /// 功能：查詢時間區間內，總注單數、總有效投注、總派彩等統計資料
        /// 本 API 並非即時查詢注單，而是每15分鐘結算一次
        /// 本 API 會因為遊戲商派彩快慢，與實際注單有落差
        /// 
        /// 查詢區間最小為1小時，不支援分秒查詢
        /// timezone 可以代入-04、+00、+08
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<TpResponse<List<StatisticsResponse>>> Statistics(StatisticsRequest request);

        /// <summary>
        /// 查詢投注量統計_10分鐘統計報表
        /// 
        /// 功能：查詢時間區間內，依照玩家分類統計的注單數，總有效投注及總派彩等統計資料
        /// 本 API 並非即時查詢注單，而是每15分鐘結算一次
        /// 本 API 會因為遊戲商派彩快慢，與實際注單有落差
        /// 
        /// 查詢區間最小為10分鐘
        /// timezone 可以代入-04、+00、+08
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        //Task<TpResponse<Statistics10MinResponse>> Statistics10Min(Statistics10MinRequest request);

        /// <summary>
        /// 查詢投注量統計_依遊戲
        /// 
        /// 功能：查詢時間區間內，玩家的注單數，總有效投注及總派彩等統計資料
        /// 本 API 並非即時查詢注單，而是每15分鐘結算一次
        /// 本 API 會因為遊戲商派彩快慢，與實際注單有落差
        /// 
        /// 查詢區間最小為1小時，不支援分秒查詢
        /// timezone 可以代入-04、+00、+08
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<TpResponse<StatisticsByGameResponse>> StatisticsByGame(StatisticsByGameRequest request);

        /// <summary>
        /// 查詢投注量統計_依玩家
        /// 
        /// 功能：查詢時間區間內，依照玩家分類統計的注單數，總有效投注及總派彩等統計資料
        /// 本 API 並非即時查詢注單，而是每15分鐘結算一次
        /// 本 API 會因為遊戲商派彩快慢，與實際注單有落差
        /// 
        /// 查詢區間最小為1小時，不支援分秒查詢
        /// timezone 可以代入-04、+00、+08
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<TpResponse<StatisticsByUserResponse>> StatisticsByUser(StatisticsByUserRequest request);

        /// <summary>
        /// 查詢投注量統計_依玩家與遊戲
        /// 
        /// 功能：查詢時間區間內，依照玩家分類統計的注單數，總有效投注及總派彩等統計資料
        /// 本 API 並非即時查詢注單，而是每15分鐘結算一次
        /// 本 API 會因為遊戲商派彩快慢，與實際注單有落差
        /// 
        /// 查詢區間最小為1小時，不支援分秒查詢
        /// timezone 可以代入-04、+00、+08
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<TpResponse<StatisticsByUserGameResponse>> StatisticsByUserGame(StatisticsByUserGameRequest request);

        /// <summary>
        /// 多筆交易紀錄查詢
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<TpResponse<List<TransactionResponse>>> Transaction(TransactionRequest request);

        /// <summary>
        /// 取款
        /// 
        /// 若出現timeout或其他網路問題 (http status code 不是200) 時, 請用相同transaction_id重試以避免重複取款
        /// 出現ErrorCode 1006, 1015, 1999，該轉帳可能還在處理中，請用相同transaction_id重試以避免重複取款
        /// 
        /// PG娛樂城、IM : 使用幣別為越南盾(VND)、印尼盾(IDR)時，娛樂城內遊戲顯示的金額單位為1000，amount參數(轉帳金額)有效位數只支援到十分位。EX : 1230為有效金額, 1234、1234.5、1234.56為無效金額
        /// MEGA娛樂城、BG真人、AB真人 : 轉帳到MEGA、BG、AB時，amount參數(轉帳金額)不可小於1.00
        /// SA真人: 越南盾(VND)、印尼盾(IDR)、緬甸元(MMK) 遊戲內顯示的金額單位為千分之一, amount參數(轉帳金額)有效位數只支援到十分位。EX: 1230為有效金額, 1234、1234.5、1234.56為無效金額
        /// 3SING、GTI: 越南盾(VND)、印尼盾(IDR) amount參數(轉帳金額)只接受小數點後一位
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<TpResponse<WithdrawResponse>> Withdraw(WithdrawRequest request);
    }
}