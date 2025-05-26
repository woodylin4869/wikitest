using H1_ThirdPartyWalletAPI.Model.Game.KS.Request;
using H1_ThirdPartyWalletAPI.Model.Game.KS.Response;
using System.Threading.Tasks;

namespace H1_ThirdPartyWalletAPI.Service.Game.KS
{
    public interface IKSApiService
    {

        /// <summary>
        /// 設定語系
        /// </summary>
        /// <param name="language"></param>
        void SetContentLanguage(string language);

        /// <summary>
        /// 2.2 register / 会员注册
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        Task<KSBaseRespones<UserRegisterResponse>> UserRegister(UserRegisterRequest source);

        /// <summary>
        /// 2.3 login / 会员登录
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        Task<KSBaseRespones<UserLoginResponse>> UserLogin(UserLoginRequest source);


        /// <summary>
        /// 2.4 logout / 会员下线
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        Task<KSBaseRespones<UserLogoutResponse>> UserLogout(UserLogoutRequest source);


        /// <summary>
        /// 2.5.1 balance / 查询余额
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        Task<KSBaseRespones<UserBalanceResponse>> UserBalance(UserBalanceRequest source);

        /// <summary>
        /// 2.5.2 translate / 转账
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        Task<KSBaseRespones<UserTransferResponse>> UserTransfer(UserTransferRequest source);

        /// <summary>
        /// 2.5.3 translateinfo / 转账查询
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        Task<KSBaseRespones<UserTransferInfoResponse>> UserTransferInfo(UserTransferInfoRequest source);


        /// <summary>
        /// 2.7 / group / 分组调整
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        Task<KSBaseRespones<UserGroupResponse>> UserGroup(UserGroupRequest source);

        /// <summary>
        /// 3.3 Get / 订单拉取
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        Task<KSBaseRespones<LogGetResponse>> LogGet(LogGetRequest source);


        /// <summary>
        /// 3.7 SiteReport / 商户报表  /日帳
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        Task<KSBaseRespones<SiteReportResponse>> SiteReport(SiteReportRequest source);

        /// <summary>
        /// 3.8 BillReport / 商户账单  /日帳
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        Task<KSBaseRespones<LogBillReprotResponse>> LogBillReprot(LogBillReprotRequest source);

        

    }
}
