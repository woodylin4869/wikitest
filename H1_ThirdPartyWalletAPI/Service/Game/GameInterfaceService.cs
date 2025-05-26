using H1_ThirdPartyWalletAPI.Code;
using H1_ThirdPartyWalletAPI.Model;
using H1_ThirdPartyWalletAPI.Model.DataModel;
using H1_ThirdPartyWalletAPI.Model.Game;
using H1_ThirdPartyWalletAPI.Model.W1API;
using H1_ThirdPartyWalletAPI.Service.Game.AE;
using H1_ThirdPartyWalletAPI.Service.Game.BTI;
using H1_ThirdPartyWalletAPI.Service.Game.CMD368;
using H1_ThirdPartyWalletAPI.Service.Game.CR;
using H1_ThirdPartyWalletAPI.Service.Game.DS;
using H1_ThirdPartyWalletAPI.Service.Game.EGSlot;
using H1_ThirdPartyWalletAPI.Service.Game.FC;
using H1_ThirdPartyWalletAPI.Service.Game.Gemini;
using H1_ThirdPartyWalletAPI.Service.Game.GR;
using H1_ThirdPartyWalletAPI.Service.Game.JILI;
using H1_ThirdPartyWalletAPI.Service.Game.JOKER;
using H1_ThirdPartyWalletAPI.Service.Game.KS;
using H1_ThirdPartyWalletAPI.Service.Game.META;
using H1_ThirdPartyWalletAPI.Service.Game.MP;
using H1_ThirdPartyWalletAPI.Service.Game.MT;
using H1_ThirdPartyWalletAPI.Service.Game.NEXTSPIN;
using H1_ThirdPartyWalletAPI.Service.Game.OB;
using H1_ThirdPartyWalletAPI.Service.Game.PG.Service;
using H1_ThirdPartyWalletAPI.Service.Game.PME;
using H1_ThirdPartyWalletAPI.Service.Game.PP;
using H1_ThirdPartyWalletAPI.Service.Game.RCG2;
using H1_ThirdPartyWalletAPI.Service.Game.RCG3;
using H1_ThirdPartyWalletAPI.Service.Game.RGRICH;
using H1_ThirdPartyWalletAPI.Service.Game.RLG;
using H1_ThirdPartyWalletAPI.Service.Game.SEXY;
using H1_ThirdPartyWalletAPI.Service.Game.TP;
using H1_ThirdPartyWalletAPI.Service.Game.WM;
using H1_ThirdPartyWalletAPI.Service.Game.WE;
using H1_ThirdPartyWalletAPI.Service.Game.WS168;
using H1_ThirdPartyWalletAPI.Service.Game.XG;
using H1_ThirdPartyWalletAPI.Service.Game.IDN;
using H1_ThirdPartyWalletAPI.Service.Game.PS;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using H1_ThirdPartyWalletAPI.Service.Game.VA;
using H1_ThirdPartyWalletAPI.Service.Game.SPLUS;


namespace H1_ThirdPartyWalletAPI.Service.Game
{
    public class GameInterfaceService : IGameInterfaceService
    {
        private readonly Dictionary<Platform, IGameInterfaceService> _services;

        public GameInterfaceService(
            ISaba2InterfaceService sabaRecordService,
            //ISabaSessionRecordService sabaSessionRecordService,
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
            ITPInterfaceService tpInterfaceService,
            IRtgInterfaceService rtgInterfaceService,
            IJILIInterfaceService jiliInterfaceService,
            IJOKER_InterfaceService jokerInterfaceService,
            IOBInterfaceService obInterfaceService,
            IMETAInterfaceService metaInterfaceService,
            IGRInterfaceService grInterfaceService,
            IWMInterfaceService wmInterfaceService,
            ISEXYInterfaceService sexyInterfaceService,
            INEXTSPIN_InterfaceService nextspinInterfaceService,
            IXGInterfaceService xgInterfaceService,
            IPPInterfaceService ppInterfaceService,
            IFCInterfaceService fcInterfaceService,
            IWS168InterfaceService ws168InterfaceService,
            ISaba2InterfaceService saba2RecordService,
            IMTInterfaceService mtecordService,
            IPMEInterfaceService pmeInterfaceService,
            IMPInterfaceService mpecordService,
            ICMDInterfaceService cmdInterfaceService,
            IKSInterfaceService ksInterfaceService,
            IBTIInterfaceService btiInterfaceService,
            IRCG2InterfaceService rcg2InterfaceService,
            IGeminiInterfaceService geminiInterfaceService,
            IRGRICHInterfaceService rgrichInterfaceService,
            IEGSlotInterfaceService egslotInterfaceService,
            ICRInterfaceService crInterfaceService,
            IRCG3InterfaceService rcg3InterfaceService,
            IWEInterfaceService weInterfaceService,
            IPsInterfaceService PSInterfaceService,
            IIDNInterfaceService idnInterfaceService,
            IVAInterfaceService vaInterfaceService,
            ISPLUS_InterfaceService SPLUSInterfaceService
        )
        {
            _services = new Dictionary<Platform, IGameInterfaceService>()
            {
                { Platform.SABA, sabaRecordService},
                { Platform.JDB, jdbRecordService},
                { Platform.RCG, rcgRecordService },
                { Platform.MG, mgRecordService},
                { Platform.DS, dsRecordService },
                { Platform.PG, pgRecordService },
                { Platform.AE, aeRecordService },
                { Platform.RSG, rsgH1RecordService },
                { Platform.RLG, rlgRecordService },
                { Platform.STREAMER, streamerInterfaceService },
                { Platform.TP, tpInterfaceService },
                { Platform.RTG, rtgInterfaceService },
                { Platform.JILI, jiliInterfaceService },
                { Platform.JOKER, jokerInterfaceService },
                { Platform.OB, obInterfaceService },
                { Platform.GR, grInterfaceService },
                { Platform.META, metaInterfaceService },
                { Platform.SEXY, sexyInterfaceService },
                { Platform.WM, wmInterfaceService },
                { Platform.XG, xgInterfaceService },
                { Platform.NEXTSPIN, nextspinInterfaceService },
                { Platform.PP, ppInterfaceService },
                { Platform.FC, fcInterfaceService },
                { Platform.WS168, ws168InterfaceService },
                { Platform.SABA2, saba2RecordService},
                { Platform.MT, mtecordService},
                { Platform.PME, pmeInterfaceService},
                { Platform.MP, mpecordService},
                { Platform.CMD368, cmdInterfaceService},
                { Platform.KS, ksInterfaceService},
                { Platform.BTI, btiInterfaceService},
                 { Platform.RCG2, rcg2InterfaceService},
                 { Platform.GEMINI, geminiInterfaceService},
                 { Platform.RGRICH, rgrichInterfaceService},
                 { Platform.EGSLOT, egslotInterfaceService},
                 { Platform.CR, crInterfaceService},
                 { Platform.RCG3, rcg3InterfaceService},
                 { Platform.WE, weInterfaceService},
                 { Platform.PS, PSInterfaceService},
                 { Platform.IDN, idnInterfaceService},
                 { Platform.VA, vaInterfaceService},
                 { Platform.SPLUS, SPLUSInterfaceService}
            };
        }

        public async Task<MemberBalance> GetGameCredit(Platform platform, GamePlatformUser platform_user)
        {
            if (_services.TryGetValue(platform, out var service))
            {
                return await service.GetGameCredit(platform, platform_user);
            }

            throw new ExceptionMessage((int)ResponseCode.UnavailablePlatform, MessageCode.Message[(int)ResponseCode.UnavailablePlatform]);
        }

        public async Task<bool> KickUser(Platform platform, GamePlatformUser platform_user)
        {
            if (_services.TryGetValue(platform, out var service))
            {
                return await service.KickUser(platform, platform_user);
            }

            throw new ExceptionMessage((int)ResponseCode.UnavailablePlatform, MessageCode.Message[(int)ResponseCode.UnavailablePlatform]);
        }

        public async Task<string> Deposit(GamePlatformUser platform_user, Wallet walletData, WalletTransferRecord RecordData)
        {
            Platform platformid = (Platform)Enum.Parse(typeof(Platform), RecordData.target.ToUpper());
            if (_services.TryGetValue(platformid, out var service))
            {
                return await service.Deposit(platform_user, walletData, RecordData);
            }

            throw new ExceptionMessage((int)ResponseCode.UnavailablePlatform, MessageCode.Message[(int)ResponseCode.UnavailablePlatform]);
        }

        public async Task<string> Withdraw(GamePlatformUser platform_user, Wallet walletData, WalletTransferRecord RecordData)
        {
            Platform platformid = (Platform)Enum.Parse(typeof(Platform), RecordData.type.ToUpper());
            if (_services.TryGetValue(platformid, out var service))
            {
                return await service.Withdraw(platform_user, walletData, RecordData);
            }

            throw new ExceptionMessage((int)ResponseCode.UnavailablePlatform, MessageCode.Message[(int)ResponseCode.UnavailablePlatform]);
        }

        public async Task<GamePlatformUser> CreateGameUser(ForwardGameReq request, Wallet userData)
        {
            Platform platformid = (Platform)Enum.Parse(typeof(Platform), request.Platform.ToUpper());
            if (_services.TryGetValue(platformid, out var service))
            {
                return await service.CreateGameUser(request, userData);
            }

            throw new ExceptionMessage((int)ResponseCode.UnavailablePlatform, MessageCode.Message[(int)ResponseCode.UnavailablePlatform]);
        }

        public async Task<string> Login(ForwardGameReq request, Wallet userData, GamePlatformUser platformUser)
        {
            Platform platformid = (Platform)Enum.Parse(typeof(Platform), request.Platform.ToUpper());
            if (_services.TryGetValue(platformid, out var service))
            {
                return await service.Login(request, userData, platformUser);
            }

            throw new ExceptionMessage((int)ResponseCode.UnavailablePlatform, MessageCode.Message[(int)ResponseCode.UnavailablePlatform]);
        }

        public async Task<CheckTransferRecordResponse> CheckTransferRecord(WalletTransferRecord transfer_record)
        {
            Platform platformid = (Platform)Enum.Parse(typeof(Platform), transfer_record.type.ToUpper());
            if (_services.TryGetValue(platformid, out var service))
            {
                return await service.CheckTransferRecord(transfer_record);
            }

            throw new ExceptionMessage((int)ResponseCode.UnavailablePlatform, MessageCode.Message[(int)ResponseCode.UnavailablePlatform]);
        }

        public async Task<dynamic> GetBetRecords(GetBetRecordReq RecordReq)
        {
            Platform platformid = (Platform)Enum.Parse(typeof(Platform), RecordReq.Platform.ToUpper());
            if (_services.TryGetValue(platformid, out var service))
            {
                return await service.GetBetRecords(RecordReq);
            }
            throw new ExceptionMessage((int)ResponseCode.UnavailablePlatform, MessageCode.Message[(int)ResponseCode.UnavailablePlatform]);
        }

        public async Task<string> GameDetailURL(GetBetDetailReq RecordDetailReq)
        {
            Platform platformid = (Platform)Enum.Parse(typeof(Platform), RecordDetailReq.Platform.ToUpper());
            if (_services.TryGetValue(platformid, out var service))
            {
                return await service.GameDetailURL(RecordDetailReq);
            }

            throw new ExceptionMessage((int)ResponseCode.UnavailablePlatform, MessageCode.Message[(int)ResponseCode.UnavailablePlatform]);
        }

        public async Task<RCGRowData> GameRowData(GetRowDataReq RecordDetailReq)
        {
            Platform platformid = (Platform)Enum.Parse(typeof(Platform), RecordDetailReq.Platform.ToUpper());
            if (_services.TryGetValue(platformid, out var service))
            {
                return await service.GameRowData(RecordDetailReq);
            }

            throw new ExceptionMessage((int)ResponseCode.UnavailablePlatform, MessageCode.Message[(int)ResponseCode.UnavailablePlatform]);
        }

        public async Task<string> RepairGameRecord(RepairBetSummaryReq RepairReq)
        {
            Platform platformid = (Platform)Enum.Parse(typeof(Platform), RepairReq.game_id.ToUpper());
            if (_services.TryGetValue(platformid, out var service))
            {
                return await service.RepairGameRecord(RepairReq);
            }

            throw new ExceptionMessage((int)ResponseCode.UnavailablePlatform, MessageCode.Message[(int)ResponseCode.UnavailablePlatform]);
        }

        public Task HealthCheck(Platform platform)
        {
            if (_services.TryGetValue(platform, out var service))
            {
                return service.HealthCheck(platform);
            }

            throw new ExceptionMessage((int)ResponseCode.UnavailablePlatform, MessageCode.Message[(int)ResponseCode.UnavailablePlatform]);
        }

        public async Task<bool> KickAllUser(Platform platform)
        {
            if (_services.TryGetValue(platform, out var service))
            {
                return await service.KickAllUser(platform);
            }

            throw new ExceptionMessage((int)ResponseCode.UnavailablePlatform, MessageCode.Message[(int)ResponseCode.UnavailablePlatform]);
        }

        public async Task<bool> RecordSummary(Platform platform, DateTime reportDatetime, DateTime startTime, DateTime endTime)
        {
            if (_services.TryGetValue(platform, out var service))
            {
                return await service.RecordSummary(platform, reportDatetime, startTime, endTime);
            }

            throw new ExceptionMessage((int)ResponseCode.UnavailablePlatform, MessageCode.Message[(int)ResponseCode.UnavailablePlatform]);
        }

        public PlatformType GetPlatformType(Platform platform)
        {
            if (_services.TryGetValue(platform, out var service))
            {
                return service.GetPlatformType(platform);
            }

            throw new ExceptionMessage((int)ResponseCode.UnavailablePlatform, MessageCode.Message[(int)ResponseCode.UnavailablePlatform]);
        }

        public async Task<ResCodeBase> SetLimit(SetLimitReq setLimitReq, GamePlatformUser gameUser, Wallet memberWalletData)
        {
            Platform platformid = (Platform)Enum.Parse(typeof(Platform), setLimitReq.Platform.ToUpper());
            if (_services.TryGetValue(platformid, out var service))
            {
                return await service.SetLimit(setLimitReq, gameUser, memberWalletData);
            }

            throw new ExceptionMessage((int)ResponseCode.UnavailablePlatform, MessageCode.Message[(int)ResponseCode.UnavailablePlatform]);
        }

        public async Task<List<object>> GetGameApiList(Platform platform)
        {
            if (_services.TryGetValue(platform, out var service))
            {
                return await service.GetGameApiList(platform);
            }

            throw new ExceptionMessage((int)ResponseCode.UnavailablePlatform, MessageCode.Message[(int)ResponseCode.UnavailablePlatform]);
        }
    }
}