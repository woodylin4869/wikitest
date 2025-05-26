using System.Collections.Generic;
using System.Threading.Tasks;
using H1_ThirdPartyWalletAPI.Model.Game.XG.Request;
using H1_ThirdPartyWalletAPI.Model.Game.XG.Response;

namespace H1_ThirdPartyWalletAPI.Service.Game.XG
{
    /// <summary>
    /// XG API
    /// https://github.com/jacky5823a/docs/blob/master/AccountingPlatformAPI/XG/accounting-platform-cht.md
    /// </summary>
    public interface IXGApiService
    {
        // 會員 創建會員帳號 /api/keno-api/xg-casino/CreateMember [Post]
        Task<CreateMemberResponse> CreateMember(CreateMemberRequest request);
        // 會員 取得登入連結 /api/keno-api/xg-casino/Login [Get]
        Task<LoginResponse> Login(LoginRequest request);
        // 會員 踢線 /api/keno-api/xg-casino/KickMember [Post]
        Task<KickMemberResponse> KickMember(KickMemberRequest request);
        // 會員 取得會員資料 /api/keno-api/xg-casino/Account [Get]
        Task<AccountResponse> Account(AccountRequest request);
        // 會員 限注取得 /api/keno-api/xg-casino/Template  [Get]
        Task<GetTemplateResponse> GetTemplate(GetTemplateRequest request);
        // 會員 限注設定 /api/keno-api/xg-casino/Template [Post]
        Task<SetTemplateResponse> SetTemplate(SetTemplateRequest request);

        // 轉帳 會員轉帳 /api/keno-api/xg-casino/Transfer [Post]
        Task<TransferResponse> Transfer(TransferRequest request);
        // 轉帳 取得單筆轉帳資料 /api/keno-api/xg-casino/CheckTransfer [Post]
        Task<CheckTransferResponse> CheckTransfer(CheckTransferRequest request);

        // 注單 取得會員下注內容 /api/keno-api/xg-casino/GetBetRecordByTime [Post]
        Task<GetBetRecordByTimeResponse> GetBetRecordByTime(GetBetRecordByTimeRequest request);
        // 注單 注單編號查詢會員下注內容  /api/keno-api/xg-casino/GetGameDetailUrl [Post]
        Task<GetGameDetailUrlResponse> GetGameDetailUrl(GetGameDetailUrlRequest request);
        // 注單 取得會員下注內容統計 /api/keno-api/xg-casino/GetGameDetailUrl [Post]
        Task<GetApiReportUrlResponse> GetApiReportUrl(GetApiReportUrlRequest request);
        // API健康檢查 /api/keno-api/xg-casino/Health [Get]
        Task<HealthResponse> Health(HealthRequest request);
    }
}
