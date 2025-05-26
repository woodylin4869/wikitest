using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using H1_ThirdPartyWalletAPI.Code;
using H1_ThirdPartyWalletAPI.Model;
using H1_ThirdPartyWalletAPI.Model.Config;
using H1_ThirdPartyWalletAPI.Model.DataModel;
using Dapper;
using Npgsql;
using H1_ThirdPartyWalletAPI.Service.Common;

namespace H1_ThirdPartyWalletAPI.Controllers
{

    [ApiController]
    [Route("[controller]")]
    public class LoginController : ControllerBase
    {
        private readonly ILogger<LoginController> _logger;
        private readonly JWTService _serviceJWT;
        public LoginController(ILogger<LoginController> logger, JWTService serviceJWT)
        {
            _logger = logger;
            _serviceJWT = serviceJWT;
        }

        /// <summary>
        /// login  and get jwt token
        /// </summary>
        /// <response code="200">OK</response> /// 
        [HttpPost("jwtLogin")]
        public Login Post(LoginReq request)
        {
            try
            {
                string role = CheckLogin(request);
                Login res = new Login();

                switch (role)
                {
                    case "admin":
                    case "user":
                    case "nan":
                        break;
                    default:
                        res.code = (int)ResponseCode.LoginFail;
                        res.Message = MessageCode.Message[res.code];
                        res.token = "";
                        throw new Exception("login fail");
                }
                var token = _serviceJWT.GenerateJwtToken(role, DateTime.Now.AddDays(30));

                res.code = (int)ResponseCode.Success;
                res.Message = MessageCode.Message[res.code];
                res.token = token;

                return res;
            }
            catch(Exception ex)
            {
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                _logger.LogError("CreateUser exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
                Login res = new Login();
                res.code = (int)ResponseCode.LoginFail;
                res.Message = MessageCode.Message[res.code];
                res.token = "";
                return res;
            }
        }
        private string CheckLogin(LoginReq request)
        {
            string conncetionString = Config.OneWalletAPI.DBConnection.Wallet.Read;
            try
            {
                using (NpgsqlConnection conn = new NpgsqlConnection(conncetionString))
                {
                    string strSql = @"SELECT *
                                FROM t_admin 
                                WHERE user_account = @user_Account
                                LIMIT 1";

                    var par = new DynamicParameters();
                    par.Add("@user_account", request.UserAccount);
                    var results = conn.Query<Admin>(strSql, par).ToList();
                 
                    if (results.Count() == 1)
                    {
                        Admin UserData = results[0];
                        bool verified = BCrypt.Net.BCrypt.Verify(request.UserPassword, UserData.User_Password);
                        if(verified)
                        {
                            return UserData.Role;
                        }
                        else
                        {
                            return "Password_fail";
                        }
                    }
                    else
                    {
                        return "Account_fail";
                    }
                }
            }
            catch (Exception ex)
            {
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                _logger.LogError("CreateUser exception EX : {ex}  MSG : {Message} Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
                return "login_fail";
            }
        }
    }
}
