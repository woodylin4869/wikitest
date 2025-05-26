using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using H1_ThirdPartyWalletAPI.Model.OneWalletGame;


namespace H1_ThirdPartyWalletAPI.Service.Common
{
    public interface IAuthService
    {
        Task<CheckUserResponse> CheckUser(CheckUserRequest request);
        public Task<bool> IsAuthenticatedAsync(string memberAccount, string token);
        Task<RequestExtendTokenResponse> RequestExtendToken(HttpContext context, RequestExtendTokenRequest request);
    }
    public class AuthService : IAuthService
    {
        private readonly ILogger<AuthService> _logger;
        private readonly IMemoryCache memoryCache;
        private readonly JWTService ServiceJWT;
        private readonly IDBService _serviceDB;
        public AuthService(ILogger<AuthService> logger, IMemoryCache memoryCache, JWTService jWTService, IDBService serviceDB)
        {
            this.memoryCache = memoryCache;
            ServiceJWT = jWTService;
            _serviceDB = serviceDB;
            _logger = logger;
        }
        public async Task<bool> IsAuthenticatedAsync(string memberAccount, string token)
        {
            if (memoryCache.TryGetValue<string>(memberAccount, out string tokenValue))
            {
                if (token == tokenValue)
                {
                    return await Task.FromResult(true);
                }
            }
            return await Task.FromResult(false);
        }
        public async Task<string> CreateTokenAsync(string systemcode, string webid, string memberAccount, OW_RCG.TokenType authType, DateTime expiredDateTime)
        {

            if (authType == OW_RCG.TokenType.SessionToken)
            {
                return await Task.FromResult(ServiceJWT.GenerateRcgJwtToken(systemcode, webid, memberAccount, expiredDateTime, tokenType: OW_RCG.TokenType.SessionToken));
            }
            else
            {
                return await Task.FromResult(ServiceJWT.GenerateRcgJwtToken(systemcode, webid, memberAccount, expiredDateTime));
            }
        }
        public async void UpdateAuthToken(string memberAccount, string token, DateTime expiredDateTime)
        {
            int result = await _serviceDB.PutRcgToken(memberAccount, token);
        }
        public async Task<bool> ValidateAuthToken(string memberAccount, string token)
        {
            try
            {
                var results = await _serviceDB.GetRcgToken(memberAccount);
                if (results == null)
                {
                    throw new Exception("no data");
                }
                var auth_token = results.auth_token;
                if (auth_token != token)
                {
                    throw new Exception("invalid token");
                }
                return true;
            }
            catch (Exception ex)
            {
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                _logger.LogDebug("ValidateAuthToken EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
                return false;
            }
        }
        public async Task<CheckUserResponse> CheckUser(CheckUserRequest request)
        {
            var AuthTokenExpiredDateTime = DateTime.Now.AddDays(180);
            var SessionTokenExpiredDateTime = DateTime.Now.AddMinutes(30); //to do
            if (!await ValidateAuthToken(request.Account, request.Token))
            {
                throw new Exception("Authentication Token Faid");
            }
            var result = new CheckUserResponse
            {
                RequstId = request.RequestId,
                Account = request.Account,
                AuthToken = await CreateTokenAsync(request.SystemCode, request.WebId, request.Account, OW_RCG.TokenType.AuthToken, AuthTokenExpiredDateTime),
                SessionToken = await CreateTokenAsync(request.SystemCode, request.WebId, request.Account, OW_RCG.TokenType.SessionToken, SessionTokenExpiredDateTime)
            };
            UpdateAuthToken(request.Account, result.AuthToken, AuthTokenExpiredDateTime);
            return result;
        }
        public async Task<RequestExtendTokenResponse> RequestExtendToken(HttpContext context, RequestExtendTokenRequest request)
        {
            var jwtToken = ServiceJWT.DecodeToken(context);
            var payload = jwtToken.Payload.ToDictionary(x => x.Key, x => x.Value);
            var Account = payload["memberaccount"].ToString();
            var systemcode = payload["systemcode"].ToString();
            var webid = payload["webid"].ToString();
            var SessionTokenExpiredDateTime = DateTime.Now.AddMinutes(5);
            if (!await ValidateAuthToken(Account, jwtToken.RawData))
            {
                throw new Exception("Authentication Token Faid");
            }
            var result = new RequestExtendTokenResponse
            {
                RequestId = request.RequestId,
                SessionToken = await CreateTokenAsync(systemcode, webid, Account, OW_RCG.TokenType.SessionToken, SessionTokenExpiredDateTime)
            };
            return result;
        }
    }
}
