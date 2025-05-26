using H1_ThirdPartyWalletAPI.Code;
using H1_ThirdPartyWalletAPI.Model;
using H1_ThirdPartyWalletAPI.Model.Config;
using H1_ThirdPartyWalletAPI.Model.Game;
using H1_ThirdPartyWalletAPI.Model.Game.JDB.Exceptions;
using H1_ThirdPartyWalletAPI.Model.Game.JDB.Request;
using H1_ThirdPartyWalletAPI.Service.Common;
using H1_ThirdPartyWalletAPI.Service.Game;
using H1_ThirdPartyWalletAPI.Service.Game.AE;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace H1_ThirdPartyWalletAPI.Service.W1API
{
    public interface IKickAllUserService
    {
        Task<ResCodeBase> KickAllUser(Platform platform);
    }
    public class KickAllUserService : IKickAllUserService
    {
        private readonly ILogger<KickAllUserService> _logger;
        private readonly IGameInterfaceService _gameInterfaceService;
        private readonly ICommonService _commonService;

        public KickAllUserService(ILogger<KickAllUserService> logger, IGameInterfaceService gameInterfaceService, ICommonService commonService)
        {
            _logger = logger;
            _gameInterfaceService = gameInterfaceService;
            _commonService = commonService;
        }
        async public Task<ResCodeBase> KickAllUser(Platform platform)
        {
            
            var res = new ResCodeBase();
            try
            {
                await _gameInterfaceService.KickAllUser(platform);
                res.code = (int)ResponseCode.Success;
                res.Message = MessageCode.Message[(int)ResponseCode.Success];
                return res;
            }
            catch(NotImplementedException)//為實做視為成功
            {
                res.code = (int)ResponseCode.Success;
                res.Message = MessageCode.Message[(int)ResponseCode.Success];
                return res;
            }
        }     
    }
}
