using H1_ThirdPartyWalletAPI.Model.Game.FC.Request;
using H1_ThirdPartyWalletAPI.Model.Game.FC.Response;
using System.Threading.Tasks;

namespace H1_ThirdPartyWalletAPI.Service.Game.FC
{
    public interface IFCApiService
    {
        Task<CreateMemberResponse> CreateMember(CreateMemberRequest source);

        //Task<GetGameListResponse> GetGameList(GetGameListRequest source);

        Task<SearchMemberResponse> SearchMember(SearchMemberRequest source);

        Task<SetPointsResponse> SetPoints(SetPointsRequest source);

        Task<LoginResponse> Login(LoginRequest source);


        Task<KickOutResponse> KickOut(KickOutRequest source);


        /// <summary>
        /// 踢出全部玩家
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        Task<KickoutAllResponse> KickoutAll(KickoutAllRequest source);

        /// <summary>
        /// 3-8、交易纪录单笔查询
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        Task<GetSingleBillResponse> GetSingleBill(GetSingleBillRequest source);

        /// <summary>
        /// 3-10、取得玩家报表
        /// 使用时机
        /// 查询玩家游戏报表
        /// 未带入 RecordID 会进入报表主页
        /// 有带入 GameType 则会导向对应的游戏类别之报表主页
        /// 有带入 RecordID 会直接进入该游戏的详细讯息页面，并且不参考 GameType
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        Task<GetPlayerReportResponse> GetPlayerReport(GetPlayerReportRequest source);

        /// <summary>
        /// 3-11、每日币别报表
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        Task<GetCurrencyReportResponse> GetCurrencyReport(GetCurrencyReportRequest source);


        Task<GetRecordListResponse> GetRecordList(GetRecordListRequest source);


        /// <summary>
        /// 3-15、取得充提交易纪录
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        Task<GetBillListResponse> GetBillList(GetBillListRequest source);

        /// <summary>
        /// 3-16、每日会员游戏报表
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        Task<GetMemberGameReportResponse> GetMemberGameReport(GetMemberGameReportRequest source);


        /// <summary>
        /// 3-17、取得游戏缩图清单
        /// 取得游戏缩图与清单时使用
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        Task<GetGameIconListResponse> GetGameIconList(GetGameIconListRequest source);
        

        /// <summary>
        /// 3-20、取得历史游戏纪录列表
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        Task<GetHistoryRecordListResponse> GetHistoryRecordList(GetHistoryRecordListRequest source);
    }
}
