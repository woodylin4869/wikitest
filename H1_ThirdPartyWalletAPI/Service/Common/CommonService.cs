using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using H1_ThirdPartyWalletAPI.Code;

namespace H1_ThirdPartyWalletAPI.Service.Common
{
    public interface ICommonService
    {
        public IDBService _serviceDB { get; }
        public ICacheDataService _cacheDataService { get; }
        public IAuthService _authService { get; }
        public IHttpService _httpService { get; }
        public JWTService _JWTService { get; }
        public IApiHealthCheckService _apiHealthCheck { get; }
        public IGamePlatformUserService _gamePlatformUserService { get; }
    }
    public class CommonService: ICommonService
    {
        public  IDBService _serviceDB { get;}
        public ICacheDataService _cacheDataService { get; }
        public IAuthService _authService { get; }
        public IHttpService _httpService { get; }
        public JWTService _JWTService { get; }
        public IApiHealthCheckService _apiHealthCheck { get; }
        public IGamePlatformUserService _gamePlatformUserService { get; }

        public CommonService(
            IDBService serviceDB
          , ICacheDataService cacheDataService
          , IAuthService authService
          , IHttpService httpService
          , JWTService JWTService
          , IApiHealthCheckService apiHealthCheck
          , IGamePlatformUserService gamePlatformUserService
        )
        {
            _serviceDB = serviceDB;
            _cacheDataService = cacheDataService;
            _authService = authService;
            _httpService = httpService;
            _JWTService = JWTService;
            _apiHealthCheck = apiHealthCheck;
            _gamePlatformUserService = gamePlatformUserService;
        }
    }
}
