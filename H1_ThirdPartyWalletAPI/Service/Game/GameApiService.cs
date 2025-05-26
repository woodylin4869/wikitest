using H1_ThirdPartyWalletAPI.Service.Game.AE;
using H1_ThirdPartyWalletAPI.Service.Game.JDB.API.Interface;
using H1_ThirdPartyWalletAPI.Service.Game.MG;
using H1_ThirdPartyWalletAPI.Service.Game.DS;
using H1_ThirdPartyWalletAPI.Service.Game.JOKER;
using H1_ThirdPartyWalletAPI.Service.Game.PG.Service;
using H1_ThirdPartyWalletAPI.Service.Game.RSG;
using H1_ThirdPartyWalletAPI.Service.Game.RLG;
using H1_ThirdPartyWalletAPI.Service.Game.TP;
using H1_ThirdPartyWalletAPI.Service.Game.RTG;
using H1_ThirdPartyWalletAPI.Service.Game.JILI;
using H1_ThirdPartyWalletAPI.Service.Game.OB;
using H1_ThirdPartyWalletAPI.Service.Game.META;
using H1_ThirdPartyWalletAPI.Service.Game.GR;
using H1_ThirdPartyWalletAPI.Service.Game.WM;
using H1_ThirdPartyWalletAPI.Service.Game.SEXY;
using H1_ThirdPartyWalletAPI.Service.Game.XG;
using H1_ThirdPartyWalletAPI.Service.Game.PP;
using H1_ThirdPartyWalletAPI.Service.Game.FC;
using H1_ThirdPartyWalletAPI.Service.Game.WS168;
using H1_ThirdPartyWalletAPI.Service.Game.MT;
using H1_ThirdPartyWalletAPI.Service.Game.MP;
using H1_ThirdPartyWalletAPI.Service.Game.KS;
using H1_ThirdPartyWalletAPI.Service.Game.CMD368;
using H1_ThirdPartyWalletAPI.Service.Game.RCG2;


namespace H1_ThirdPartyWalletAPI.Service.Game
{
    public interface IGameApiService
    {
        public IRcgStealthilyApiService _RcgAPI { get; }
        public IJDBApiService _JdbAPI { get; }
        public IMGApiService _MgAPI { get; }
        public IDSApiService _DsAPI { get; }
        public IPGApiService _PgAPI { get; }
        public IRSGApiService _RsgAPI { get; }
        public IRLGApiService _RlgAPI { get; }
        public IAEApiService _AeAPI { get; }
        public IH1ApiService _h1API { get; }
        public IStreamerApiService _StreamerApi { get; }
        public ITPApiService _TpApi { get; }
        public IRTGApiService _RtgAPI { get; }
        public IJILIApiService _JiliApi { get; }
        public IJokerApiService _JokerApi { get; }
        public IOBApiService _OBApi { get; }
        public IMETAApiService _MetaApi { get; }
        public IGRApiService _GrAPI { get; }
        public IWMApiService _WMAPI { get; }

        public ISEXYApiService _SexyApi { get; }
        public IXGApiService _XgAPI { get; }
        public IPPApiService _PPAPI { get; }

        public IFCApiService _FcAPI { get; }
        public IWS168ApiService _Ws168API { get; }
        public ISaba2ApiService _Saba2API { get; }
        public IMTApiService _MTAPI { get; }
        public IMPApiService _MPAPI { get; }
        public IKSApiService _KSAPI { get; }
        public ICMDApiService _CMDAPI { get; }
        public IRCG2ApiService _RCG2API { get; }

    }
    public class GameApiService : IGameApiService
    {
        public IRcgStealthilyApiService _RcgAPI { get; }
        public IJDBApiService _JdbAPI { get; }
        public IMGApiService _MgAPI { get; }
        public IDSApiService _DsAPI { get; }
        public IPGApiService _PgAPI { get; }
        public IRSGApiService _RsgAPI { get; }
        public IRLGApiService _RlgAPI { get; }
        public IAEApiService _AeAPI { get; }
        public IH1ApiService _h1API { get; }
        public IStreamerApiService _StreamerApi { get; }
        public ITPApiService _TpApi { get; }
        public IRTGApiService _RtgAPI { get; }
        public IJokerApiService _JokerApi { get; }
        public IJILIApiService _JiliApi { get; }
        public IOBApiService _OBApi { get; }
        public IMETAApiService _MetaApi { get; }
        public IGRApiService _GrAPI { get; }
        public IWMApiService _WMAPI { get; }
        public ISEXYApiService _SexyApi { get; }

        public IXGApiService _XgAPI { get; }

        public IPPApiService _PPAPI { get; }

        public IFCApiService _FcAPI { get; }
        public IWS168ApiService _Ws168API { get; }
        public ISaba2ApiService _Saba2API { get; }
        public IMTApiService _MTAPI { get; }
        public IMPApiService _MPAPI { get; }
        public IKSApiService _KSAPI { get; }
        public ICMDApiService _CMDAPI { get; }
        public IRCG2ApiService _RCG2API { get; }
        public GameApiService(
            IRcgStealthilyApiService rcg_api
          , IJDBApiService jdb_api
          , IMGApiService mg_api
          , IDSApiService ds_api
          , IPGApiService pg_api
          , IRSGApiService rsg_api
          , IRLGApiService rlg_api
          , IAEApiService ae_api
          , IH1ApiService h1_api
          , IStreamerApiService streamerApi
          , ITPApiService tpApi
          , IRTGApiService rtg_api
          , IJILIApiService jiliApi
          , IJokerApiService jokerApi
          , IOBApiService OBApi
          , IGRApiService grApi
          , IMETAApiService metaApi
          , IWMApiService wmApi
          , ISEXYApiService sexyApi
          , IXGApiService xgApi
          , IPPApiService ppApi
          , IFCApiService fcApi
          , IWS168ApiService Ws168Api
          , ISaba2ApiService saba2_api
          , IMTApiService Mt_api
          , IMPApiService mpAPI
          , IKSApiService ks_api
          , ICMDApiService cmd_api
          , IRCG2ApiService rCG2API
        )
        {
            _RcgAPI = rcg_api;
            _JdbAPI = jdb_api;
            _MgAPI = mg_api;
            _DsAPI = ds_api;
            _PgAPI = pg_api;
            _RsgAPI = rsg_api;
            _RlgAPI = rlg_api;
            _AeAPI = ae_api;
            _h1API = h1_api;
            _StreamerApi = streamerApi;
            _TpApi = tpApi;
            _RtgAPI = rtg_api;
            _JiliApi = jiliApi;
            _JokerApi = jokerApi;
            _OBApi = OBApi;
            _GrAPI = grApi;
            _MetaApi = metaApi;
            _WMAPI = wmApi;
            _SexyApi = sexyApi;
            _XgAPI = xgApi;
            _PPAPI = ppApi;
            _FcAPI = fcApi;
            _Ws168API = Ws168Api;
            _Saba2API = saba2_api;
            _MTAPI = Mt_api;
            _MPAPI = mpAPI;
            _KSAPI = ks_api;
            _CMDAPI = cmd_api;
            _RCG2API = rCG2API;
        }
    }
}
