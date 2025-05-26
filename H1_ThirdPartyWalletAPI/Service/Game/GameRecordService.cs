using H1_ThirdPartyWalletAPI.Code;
using H1_ThirdPartyWalletAPI.Model.DataModel;
using H1_ThirdPartyWalletAPI.Model.Game;
using H1_ThirdPartyWalletAPI.Model.W1API;
using H1_ThirdPartyWalletAPI.Service.Game.AE;
using H1_ThirdPartyWalletAPI.Service.Game.DS;
using H1_ThirdPartyWalletAPI.Service.Game.FC;
using H1_ThirdPartyWalletAPI.Service.Game.JILI;
using H1_ThirdPartyWalletAPI.Service.Game.KS;
using H1_ThirdPartyWalletAPI.Service.Game.OB;
using H1_ThirdPartyWalletAPI.Service.Game.PG.Service;
using H1_ThirdPartyWalletAPI.Service.Game.PP;
using H1_ThirdPartyWalletAPI.Service.Game.RLG;
using H1_ThirdPartyWalletAPI.Service.Game.SEXY;
using H1_ThirdPartyWalletAPI.Service.Game.RCG2;
using System;
using System.Threading.Tasks;

namespace H1_ThirdPartyWalletAPI.Service.Game
{
    public class GameRecordService
    {
        public IJdbInterfaceService _JdbInterfaceService { get; }
        public IRcgInterfaceService _rcgInterfaceService { get; }
        //public IRcgSessionRecordService _RcgSessionRecordService { get; }
        public IMgInterfaceService _mgInterfaceService { get; }
        public IDsInterfaceService _dsInterfaceService { get; }
        public IPGInterfaceService _pgInterfaceService { get; }
        public IAeInterfaceService _aeInterfaceService { get; }
        public IRsgH1InterfaceService _rsgH1InterfaceService { get; }
        public IRlgInterfaceService _rlgInterfaceService { get; }
        public IStreamerInterfaceService _streamerInterfaceService { get; }
        public IJILIInterfaceService _jiliInterfaceService { get; }
        public IOBInterfaceService _OBInterfaceService { get; }
        public ISEXYInterfaceService _SEXYInterfaceService { get; }
        public IPPInterfaceService _PPInterfaceService { get; }

        public IFCInterfaceService _FCInterfaceService { get; }

        public ISaba2InterfaceService _Saba2InterfaceService { get; }
        public IRCG2InterfaceService _rcg2InterfaceService { get; }

        public IKSInterfaceService _KSInterfaceService { get; }

        public GameRecordService(
            IJdbInterfaceService jdbRecordService,
            IRcgInterfaceService rcgRecordService,
            //IRcgSessionRecordService rcgSessionRecordService,
            IMgInterfaceService mgRecordService,
            IDsInterfaceService dsRecordService,
            IPGInterfaceService pgRecordService,
            IAeInterfaceService aeRecordService,
            IRsgH1InterfaceService rsgH1RecordService,
            IRlgInterfaceService rlgRecordService,
            IStreamerInterfaceService streamerInterfaceService,
            IJILIInterfaceService jiliInterfaceService,
            ISEXYInterfaceService sexyInterfaceService,
            IPPInterfaceService ppInterfaceService,
            IFCInterfaceService fcInterfaceService,
            ISaba2InterfaceService saba2RecordService,
            IKSInterfaceService ksInterfaceService,
            IRCG2InterfaceService rcg2RecordService
        )
        {
            _JdbInterfaceService = jdbRecordService;
            _rcgInterfaceService = rcgRecordService;
            //_RcgSessionRecordService = rcgSessionRecordService;
            _mgInterfaceService = mgRecordService;
            _dsInterfaceService = dsRecordService;
            _pgInterfaceService = pgRecordService;
            _aeInterfaceService = aeRecordService;
            _rsgH1InterfaceService = rsgH1RecordService;
            _rlgInterfaceService = rlgRecordService;
            _streamerInterfaceService = streamerInterfaceService;
            _jiliInterfaceService = jiliInterfaceService;
            _SEXYInterfaceService = sexyInterfaceService;
            _PPInterfaceService = ppInterfaceService;
            _FCInterfaceService = fcInterfaceService;
            _Saba2InterfaceService = saba2RecordService;
            _KSInterfaceService = ksInterfaceService;
            _rcg2InterfaceService = rcg2RecordService;
        }
    }
}