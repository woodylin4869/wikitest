using H1_ThirdPartyWalletAPI.Code;
using H1_ThirdPartyWalletAPI.Model.W1API;
using H1_ThirdPartyWalletAPI.Service.Game;
using H1_ThirdPartyWalletAPI.Service.Game.CMD368;
using H1_ThirdPartyWalletAPI.Service.Game.KS;
using H1_ThirdPartyWalletAPI.Service.Game.PME;
using H1_ThirdPartyWalletAPI.Service.Game.PP;
using H1_ThirdPartyWalletAPI.Service.Game.RLG;
using H1_ThirdPartyWalletAPI.Service.Game.WS168;
using H1_ThirdPartyWalletAPI.Service.Game.BTI;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using H1_ThirdPartyWalletAPI.Service.Game.Gemini;
using H1_ThirdPartyWalletAPI.Service.Game.RGRICH;
using H1_ThirdPartyWalletAPI.Service.Game.CR;

namespace H1_ThirdPartyWalletAPI.Service.W1API
{
    public interface IBetRecordService
    {
        public Task<dynamic> GetBetRecord(GetBetRecordReq RecordReq);

        public Task<dynamic> GetBetRecordUnsettle(GetBetRecordUnsettleReq RecordReq);
    }

    public class BetRecordService : IBetRecordService
    {
        private readonly IGameInterfaceService _gameInterfaceService;
        private readonly ISaba2InterfaceService _SabaInterfaceService;
        private readonly IRlgInterfaceService _RlgInterfaceService;
        private readonly IPPInterfaceService _PpInterfaceService;
        private readonly IWS168InterfaceService _Ws168InterfaceService;
        private readonly IMgInterfaceService _MgInterfaceService;
        private readonly IPMEInterfaceService _pmeInterfaceService;
        private readonly IKSInterfaceService _ksInterfaceService;
        private readonly ICMDInterfaceService _cmdInterfaceService;
        private readonly IBTIInterfaceService _btiInterfaceService;
        private readonly IGeminiInterfaceService _geminiInterfaceService;
        private readonly IRGRICHInterfaceService _rgrichInterfaceService;
        private readonly ICRInterfaceService _crInterfaceService;

        public BetRecordService(ILogger<BetRecordService> logger
                , IGameInterfaceService gameInterfaceService
                , ISaba2InterfaceService sabaInterfaceService
                , IRlgInterfaceService rlgInterfaceService
                , IPPInterfaceService PpInterfaceService
                , IWS168InterfaceService Ws168InterfaceService
                , IMgInterfaceService MgInterfaceService
                , IPMEInterfaceService pmeInterfaceService
                , IKSInterfaceService KSInterfaceService
                , ICMDInterfaceService cmdInterfaceService
                , IBTIInterfaceService btiInterfaceService
                , IGeminiInterfaceService geminiInterfaceService
                , IRGRICHInterfaceService rgrichInterfaceService
                , ICRInterfaceService crInterfaceService
                )
        {
            _gameInterfaceService = gameInterfaceService;
            _SabaInterfaceService = sabaInterfaceService;
            _RlgInterfaceService = rlgInterfaceService;
            _PpInterfaceService = PpInterfaceService;
            _Ws168InterfaceService = Ws168InterfaceService;
            _MgInterfaceService = MgInterfaceService;
            _pmeInterfaceService = pmeInterfaceService;
            _ksInterfaceService = KSInterfaceService;
            _cmdInterfaceService = cmdInterfaceService;
            _btiInterfaceService = btiInterfaceService;
            _geminiInterfaceService = geminiInterfaceService;
            _rgrichInterfaceService = rgrichInterfaceService;
            _crInterfaceService = crInterfaceService;
        }

        public async Task<dynamic> GetBetRecord(GetBetRecordReq RecordReq)
        {
            return await _gameInterfaceService.GetBetRecords(RecordReq);
        }

        public async Task<dynamic> GetBetRecordUnsettle(GetBetRecordUnsettleReq RecordReq)
        {
            GetBetRecordUnsettleRes res = new GetBetRecordUnsettleRes();
            switch (RecordReq.Platform)
            {
                case nameof(Platform.SABA):
                case nameof(Platform.SABA2):
                    return await _SabaInterfaceService.GetBetRecordUnsettle(RecordReq);
                case nameof(Platform.RLG):
                    return await _RlgInterfaceService.GetBetRecordUnsettle(RecordReq);

                case nameof(Platform.PP):
                    return await _PpInterfaceService.GetBetRecordUnsettle(RecordReq);

                case nameof(Platform.WS168):
                    return await _Ws168InterfaceService.GetBetRecordUnsettle(RecordReq);

                case nameof(Platform.MG):
                    return await _MgInterfaceService.GetBetRecordUnsettle(RecordReq);

                case nameof(Platform.PME):
                    return await _pmeInterfaceService.GetBetRecordUnsettle(RecordReq);

                case nameof(Platform.KS):
                    return await _ksInterfaceService.GetBetRecordUnsettle(RecordReq);

                case nameof(Platform.CMD368):
                    return await _cmdInterfaceService.GetBetRecordUnsettle(RecordReq);

                case nameof(Platform.BTI):
                    return await _btiInterfaceService.GetBetRecordUnsettle(RecordReq);

                case nameof(Platform.GEMINI):
                    return await _geminiInterfaceService.GetBetRecordUnsettle(RecordReq);

                case nameof(Platform.RGRICH):
                    return await _rgrichInterfaceService.GetBetRecordUnsettle(RecordReq);

                case nameof(Platform.CR):
                    return await _crInterfaceService.GetBetRecordUnsettle(RecordReq);

                default:
                    throw new Exception("unknow game platform");
            }
        }
    }
}