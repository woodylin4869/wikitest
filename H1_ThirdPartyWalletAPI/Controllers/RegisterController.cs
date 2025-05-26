using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authorization;
using H1_ThirdPartyWalletAPI.Model;
using H1_ThirdPartyWalletAPI.Model.Config;
using H1_ThirdPartyWalletAPI.Code;
using Dapper;
using Npgsql;
using System.Threading.Tasks;

namespace H1_ThirdPartyWalletAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RegisterController : ControllerBase
    {
        private readonly ILogger<RegisterController> _logger;
        public RegisterController(ILogger<RegisterController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// 註冊API使用者, 僅Admin權限Token可使用
        /// </summary>
        ///
        //[Authorize("Register")]
        [Authorize(Roles = "admin")]
        [HttpPost]
        public async Task<Register> Post([FromBody] RegisterReq request)
        {
            Register res = new Register();
            try
            {
                List<Claim> UserClaim = User.Claims.ToList();
                string Role = UserClaim.Find(x => x.Type == ClaimTypes.Role).Value;
                
                if (Role != "admin")
                {
                    res.code = (int)ResponseCode.InsufficientPermissions;
                    res.Message = MessageCode.Message[(int)ResponseCode.InsufficientPermissions];
                    return res;
                }
                if (await CreateUser(request) != 1)
                {
                    res.code = (int)ResponseCode.RegisterFail;
                    res.Message = MessageCode.Message[(int)ResponseCode.RegisterFail];
                    return res;
                }
                res.code = (int)ResponseCode.Success;
                res.Message = MessageCode.Message[(int)ResponseCode.Success];
                return res;
            }
            catch(Exception ex)
            {
                res.code = (int)ResponseCode.RegisterFail;
                res.Message = MessageCode.Message[(int)ResponseCode.RegisterFail] + " | " + ex.Message.ToString();
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                _logger.LogError("Register exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
                return res;
            }

        }
        private async Task<int> CreateUser(RegisterReq request)
        {
            string conncetionString = Config.OneWalletAPI.DBConnection.Wallet.Master;
            string passwordHash = BCrypt.Net.BCrypt.HashPassword(request.User_Password);            
            try
            {
                using (NpgsqlConnection conn = new NpgsqlConnection(conncetionString))
                {
                    string strSql = @"INSERT INTO t_admin (user_account, user_password, role)
                                VALUES
                                (@user_account, @user_password, @role)";
                    var par = new DynamicParameters();
                    par.Add("@user_account", request.User_Account);
                    par.Add("@user_password", passwordHash);
                    par.Add("@role", request.Role);
                    var results = await conn.ExecuteAsync(strSql, par);
                    return results;
                }
            }
            catch(Exception ex)
            {
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                _logger.LogError("CreateUser exception EX : {ex}  MSG : {Message} Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
                return 0;
            }
        }
    }
}
