using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using H1_ThirdPartyWalletAPI.Model.Game;
using Newtonsoft.Json;
using H1_ThirdPartyWalletAPI.Code;
using H1_ThirdPartyWalletAPI.Service.Common;

namespace H1_ThirdPartyWalletAPI.Service.Game
{
    public interface IStreamerApiService : IRcgApiService
    {
        Task<bool> STREAMER_CreateOrSetUser(STREAMER_CreateOrSetUser request);
        Task<string> STREAMER_ForwardGameURL(STREAMER_Login request);

    }
    public class STREAMERApiService: IStreamerApiService
    {
        private readonly ILogger<STREAMERApiService> _logger;
        private readonly IHttpService _serviceHttp;
        public STREAMERApiService(ILogger<STREAMERApiService> logger, IHttpService serviceHttp)
        {
            _logger = logger;
            _serviceHttp = serviceHttp;
        }
        public async Task<RCG_ResBase<RCG_CreateOrSetUser_Res>> CreateOrSetUser(RCG_CreateOrSetUser request)
        {
            RCG_ResBase<RCG_CreateOrSetUser_Res> res = new RCG_ResBase<RCG_CreateOrSetUser_Res>();
            try
            {
                string ReqJson = System.Text.Json.JsonSerializer.Serialize(request);
                var json = await _serviceHttp.PostAsnyc_streamer<string>
                    (HttpService.GameProvider.RCG_PLAYER, HttpService.API_Type.CreateOrSetUser, ReqJson);
                res = JsonConvert.DeserializeObject<RCG_ResBase<RCG_CreateOrSetUser_Res>>(json);

                if (res.msgId == (int)RCG.msgId.Success)
                {
                    res.msgId = (int)ResponseCode.Success;
                    res.message = MessageCode.Message[(int)ResponseCode.Success];
                }
                else
                {
                    throw new Exception(res.message);
                }
                return res;
            }
            catch (TaskCanceledException ex)
            {
                res.msgId = (int)RCG.msgId.TimeOut;
                res.message = RCG.msgId.TimeOut.ToString();
                _logger.LogError("RCG CreateMember exception EX : {ex}  MSG : {Message}", ex.GetType().FullName, ex.Message);
                return res;
            }
            catch (Exception ex)
            {
                res.msgId = (int)ResponseCode.CreateMemberFail;
                res.message = MessageCode.Message[(int)ResponseCode.CreateMemberFail];
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                _logger.LogError("RCG CreateMember exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
                return res;
            }
        }
        public async Task<RCG_ResBase<RCG_Login_Res>> Login(RCG_Login request)
        {
            RCG_ResBase<RCG_Login_Res> res = new RCG_ResBase<RCG_Login_Res>();
            try
            {
                RCG_Login objData = request;
                string ReqJson = System.Text.Json.JsonSerializer.Serialize(objData);
                var json = await _serviceHttp.PostAsnyc_streamer<string>
                    (HttpService.GameProvider.RCG_PLAYER, HttpService.API_Type.Login, ReqJson);
                RCG_ResBase<RCG_Login_Res> jObj = JsonConvert.DeserializeObject<RCG_ResBase<RCG_Login_Res>>(json);
                res.msgId = jObj.msgId;
                res.message = jObj.message;
                res.data = jObj.data;

                if (res.msgId == (int)RCG.msgId.Success)
                {
                    res.msgId = (int)ResponseCode.Success;
                    res.message = MessageCode.Message[(int)ResponseCode.Success];
                }
                else
                {
                    throw new Exception(res.message);
                }
                return res;
            }
            catch (TaskCanceledException ex)
            {
                res.msgId = (int)RCG.msgId.TimeOut;
                res.message = RCG.msgId.TimeOut.ToString();
                _logger.LogError("RCG Login exception EX : {ex}  MSG : {Message}", ex.GetType().FullName, ex.Message);
                return res;
            }
            catch (Exception ex)
            {
                res.msgId = (int)ResponseCode.GetGameURLFail;
                res.message = MessageCode.Message[(int)ResponseCode.GetGameURLFail];
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                _logger.LogError("RCG Login exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
                return res;
            }
        }
        public async Task<RCG_ResBase<RCG_GetBalance_Res>> GetBalance(RCG_GetBalance request)
        {
            RCG_ResBase<RCG_GetBalance_Res> res = new RCG_ResBase<RCG_GetBalance_Res>();
            try
            {
                RCG_GetBalance objData = request;
                string ReqJson = System.Text.Json.JsonSerializer.Serialize(objData);
                var json = await _serviceHttp.PostAsnyc_streamer<string>
                    (HttpService.GameProvider.RCG_PLAYER, HttpService.API_Type.GetBalance, ReqJson);
                RCG_ResBase<RCG_GetBalance_Res> jObj = JsonConvert.DeserializeObject<RCG_ResBase<RCG_GetBalance_Res>>(json);
                //RCG_GetBalance_Res jObj = JsonConvert.DeserializeObject<RCG_GetBalance_Res>(json);
                res.msgId = jObj.msgId;
                res.message = jObj.message;
                res.data = jObj.data;

                if (res.msgId == (int)RCG.msgId.Success)
                {
                    res.msgId = (int)ResponseCode.Success;
                    res.message = MessageCode.Message[(int)ResponseCode.Success];
                }
                else
                {
                    throw new Exception(res.message);
                }
                return res;
            }
            catch (TaskCanceledException ex)
            {
                res.msgId = (int)RCG.msgId.TimeOut;
                res.message = RCG.msgId.TimeOut.ToString();
                _logger.LogError("RCG get balance exception EX : {ex}  MSG : {Message}", ex.GetType().FullName, ex.Message);
                return res;
            }
            catch (Exception ex)
            {
                res.msgId = (int)ResponseCode.GetBalanceFail;
                res.message = MessageCode.Message[(int)ResponseCode.GetBalanceFail];
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                _logger.LogError("RCG get balance exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
                return res;
            }
        }
        public async Task<RCG_ResBase<RCG_KickOut_Res>> KickOut(RCG_KickOut request)
        {
            RCG_ResBase<RCG_KickOut_Res> res = new RCG_ResBase<RCG_KickOut_Res>();
            try
            {
                string ReqJson = System.Text.Json.JsonSerializer.Serialize(request);
                var json = await _serviceHttp.PostAsnyc_streamer<string>
                    (HttpService.GameProvider.RCG_PLAYER, HttpService.API_Type.KickOut, ReqJson);
                RCG_ResBase<RCG_KickOut_Res> jObj = JsonConvert.DeserializeObject<RCG_ResBase<RCG_KickOut_Res>>(json);
                res.msgId = jObj.msgId;
                res.message = jObj.message;
                res.data = jObj.data;

                if (res.msgId == (int)RCG.msgId.Success)
                {
                    res.msgId = (int)ResponseCode.Success;
                    res.message = MessageCode.Message[(int)ResponseCode.Success];
                }
                else
                {
                    throw new Exception(res.message);
                }
                return res;
            }
            catch (TaskCanceledException ex)
            {
                res.msgId = (int)RCG.msgId.TimeOut;
                res.message = RCG.msgId.TimeOut.ToString();
                _logger.LogError("RCG kickout exception EX : {ex}  MSG : {Message}", ex.GetType().FullName, ex.Message);
                return res;
            }
            catch (Exception ex)
            {
                res.msgId = (int)ResponseCode.KickUserFail;
                res.message = MessageCode.Message[(int)ResponseCode.KickUserFail];
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                _logger.LogError("RCG kickout exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
                return res;
            }
        }
        public async Task<RCG_KickOutByCompany_Res> KickOutByCompany()
        {
            RCG_KickOutByCompany_Res res = new RCG_KickOutByCompany_Res();
            try
            {
                var json = await _serviceHttp.PostAsnyc_streamer<string>
                    (HttpService.GameProvider.RCG_PLAYER, HttpService.API_Type.KickOutByCompany);
                res = JsonConvert.DeserializeObject<RCG_KickOutByCompany_Res>(json);


                if (res.msgId == (int)RCG.msgId.Success)
                {
                    res.msgId = (int)ResponseCode.Success;
                    res.message = MessageCode.Message[(int)ResponseCode.Success];
                }
                else
                {
                    throw new Exception(res.message);
                }
                return res;
            }
            catch (TaskCanceledException ex)
            {
                res.msgId = (int)RCG.msgId.TimeOut;
                res.message = RCG.msgId.TimeOut.ToString();
                _logger.LogError("RCG kickout exception EX : {ex}  MSG : {Message}", ex.GetType().FullName, ex.Message);
                return res;
            }
            catch (Exception ex)
            {
                res.msgId = (int)ResponseCode.KickUserFail;
                res.message = MessageCode.Message[(int)ResponseCode.KickUserFail];
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                _logger.LogError("RCG kickout exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
                return res;
            }
        }
        public async Task<RCG_ResBase<RCG_GetMaintenanceInfo_Res>> GetMaintenanceInfo()
        {
            RCG_ResBase<RCG_GetMaintenanceInfo_Res> res = new RCG_ResBase<RCG_GetMaintenanceInfo_Res>();
            try
            {
                var json = await _serviceHttp.PostAsnyc_streamer<string>
                    (HttpService.GameProvider.RCG_H1, HttpService.API_Type.GetMaintenanceInfo);
                RCG_ResBase<RCG_GetMaintenanceInfo_Res> jObj = JsonConvert.DeserializeObject<RCG_ResBase<RCG_GetMaintenanceInfo_Res>>(json);
                res.msgId = jObj.msgId;
                res.message = jObj.message;
                res.data = jObj.data;

                if (res.msgId == (int)RCG.msgId.Success)
                {
                    res.msgId = (int)ResponseCode.Success;
                    res.message = MessageCode.Message[(int)ResponseCode.Success];
                }
                else
                {
                    throw new Exception(res.message);
                }
                return res;
            }
            catch (TaskCanceledException ex)
            {
                res.msgId = (int)RCG.msgId.TimeOut;
                res.message = RCG.msgId.TimeOut.ToString();
                _logger.LogError("RCG GetMaintenanceInfo exception EX : {ex}  MSG : {Message}", ex.GetType().FullName, ex.Message);
                return res;
            }
            catch (Exception ex)
            {
                res.msgId = (int)ResponseCode.GetGameMaintenanceFail;
                res.message = MessageCode.Message[(int)ResponseCode.GetGameMaintenanceFail];
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                _logger.LogError("RCG GetMaintenanceInfo exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
                return res;
            }
        }
        public async Task<RCG_ResBase<RCG_Deposit_Res>> Deposit(RCG_Deposit request)
        {
            RCG_ResBase<RCG_Deposit_Res> res = new RCG_ResBase<RCG_Deposit_Res>();
            try
            {
                string ReqJson = System.Text.Json.JsonSerializer.Serialize(request);
                var json = await _serviceHttp.PostAsnyc_streamer<string>
                    (HttpService.GameProvider.RCG_PLAYER, HttpService.API_Type.Deposit, ReqJson);
                RCG_ResBase<RCG_Deposit_Res> jObj = JsonConvert.DeserializeObject<RCG_ResBase<RCG_Deposit_Res>>(json);
                res.msgId = jObj.msgId;
                res.message = jObj.message;
                res.data = jObj.data;

                if (res.msgId == (int)RCG.msgId.Success)
                {
                    res.msgId = (int)ResponseCode.Success;
                    res.message = MessageCode.Message[(int)ResponseCode.Success];
                }
                else
                {
                    throw new Exception(res.message);
                }
                return res;
            }
            catch (TaskCanceledException ex)
            {
                res.msgId = (int)RCG.msgId.TimeOut;
                res.message = RCG.msgId.TimeOut.ToString();
                _logger.LogError("RCG Deposit exception EX : {ex}  MSG : {Message}", ex.GetType().FullName, ex.Message);
                return res;
            }
            catch (Exception ex)
            {
                res.msgId = (int)ResponseCode.FundTransferFail;
                res.message = MessageCode.Message[(int)ResponseCode.FundTransferFail];
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                _logger.LogError("RCG Deposit exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
                return res;
            }
        }
        public async Task<RCG_ResBase<RCG_Withdraw_Res>> Withdraw(RCG_Withdraw request)
        {
            RCG_ResBase<RCG_Withdraw_Res> res = new RCG_ResBase<RCG_Withdraw_Res>();
            try
            {
                string ReqJson = System.Text.Json.JsonSerializer.Serialize(request);
                var json = await _serviceHttp.PostAsnyc_streamer<string>
                    (HttpService.GameProvider.RCG_PLAYER, HttpService.API_Type.Withdraw, ReqJson);
                RCG_ResBase<RCG_Withdraw_Res> jObj = JsonConvert.DeserializeObject<RCG_ResBase<RCG_Withdraw_Res>>(json);
                res.msgId = jObj.msgId;
                res.message = jObj.message;
                res.data = jObj.data;

                if (res.msgId == (int)RCG.msgId.Success)
                {
                    res.msgId = (int)ResponseCode.Success;
                    res.message = MessageCode.Message[(int)ResponseCode.Success];
                }
                else
                {
                    throw new Exception(res.message);
                }
                return res;
            }
            catch (TaskCanceledException ex)
            {
                res.msgId = (int)RCG.msgId.TimeOut;
                res.message = RCG.msgId.TimeOut.ToString();
                _logger.LogError("RCG Withdraw exception EX : {ex}  MSG : {Message}", ex.GetType().FullName, ex.Message);
                return res;
            }
            catch (Exception ex)
            {
                res.msgId = (int)ResponseCode.FundTransferFail;
                res.message = MessageCode.Message[(int)ResponseCode.FundTransferFail];
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                _logger.LogError("RCG Withdraw exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
                return res;
            }
        }
        public async Task<RCG_ResBase<RCG_GetBetRecordList_Res>> GetBetRecordList(RCG_GetBetRecordList request)
        {
            RCG_ResBase<RCG_GetBetRecordList_Res> res = new RCG_ResBase<RCG_GetBetRecordList_Res>();
            try
            {
                string ReqJson = System.Text.Json.JsonSerializer.Serialize(request);
                var json = await _serviceHttp.PostAsnyc_streamer<string>
                    (HttpService.GameProvider.RCG_RECORD, HttpService.API_Type.GetBetRecordList, ReqJson);
                RCG_ResBase<RCG_GetBetRecordList_Res> jObj = JsonConvert.DeserializeObject<RCG_ResBase<RCG_GetBetRecordList_Res>>(json);
                res.msgId = jObj.msgId;
                res.message = jObj.message;
                res.data = jObj.data;
                if (res.msgId == (int)RCG.msgId.Success)
                {
                    res.msgId = (int)ResponseCode.Success;
                    res.message = MessageCode.Message[(int)ResponseCode.Success];
                }
                else
                {
                    throw new Exception(res.message);
                }
                return res;
            }
            catch (TaskCanceledException ex)
            {
                res.msgId = (int)RCG.msgId.TimeOut;
                res.message = RCG.msgId.TimeOut.ToString();
                _logger.LogError("RCG GetBetRecordList exception EX : {ex}  MSG : {Message}", ex.GetType().FullName, ex.Message);
                return res;
            }
            catch (Exception ex)
            {
                res.msgId = (int)ResponseCode.FundTransferFail;
                res.message = MessageCode.Message[(int)ResponseCode.FundTransferFail];
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                _logger.LogError("RCG GetBetRecordList exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
                return res;
            }
        }
        public async Task<RCG_ResBase<RCG_GetPlayerOnlineList_Res>> GetPlayerOnlineList(RCG_GetPlayerOnlineList request)
        {
            RCG_ResBase<RCG_GetPlayerOnlineList_Res> res = new RCG_ResBase<RCG_GetPlayerOnlineList_Res>();
            try
            {
                string ReqJson = System.Text.Json.JsonSerializer.Serialize(request);
                var json = await _serviceHttp.PostAsnyc_streamer<string>
                    (HttpService.GameProvider.RCG_PLAYER, HttpService.API_Type.GetPlayerOnlineList, ReqJson);
                RCG_ResBase<RCG_GetPlayerOnlineList_Res> jObj = JsonConvert.DeserializeObject<RCG_ResBase<RCG_GetPlayerOnlineList_Res>>(json);
                res.msgId = jObj.msgId;
                res.message = jObj.message;
                res.data = jObj.data;
                if (res.msgId == (int)RCG.msgId.Success)
                {
                    res.msgId = (int)ResponseCode.Success;
                    res.message = MessageCode.Message[(int)ResponseCode.Success];
                }
                else
                {
                    throw new Exception(res.message);
                }
                return res;
            }
            catch (TaskCanceledException ex)
            {
                res.msgId = (int)RCG.msgId.TimeOut;
                res.message = RCG.msgId.TimeOut.ToString();
                _logger.LogError("RCG GetPlayerOnlineList exception EX : {ex}  MSG : {Message}", ex.GetType().FullName, ex.Message);
                return res;
            }
            catch (Exception ex)
            {
                res.msgId = (int)ResponseCode.GetOnlineUserFail;
                res.message = MessageCode.Message[(int)ResponseCode.GetOnlineUserFail];
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                _logger.LogError("RCG GetPlayerOnlineList exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
                return res;
            }
        }
        public async Task<RCG_ResBase<RCG_GetTransactionLog_Res>> GetTransactionLog(RCG_GetTransactionLog request)
        {
            RCG_ResBase<RCG_GetTransactionLog_Res> res = new RCG_ResBase<RCG_GetTransactionLog_Res>();
            try
            {
                string ReqJson = System.Text.Json.JsonSerializer.Serialize(request);
                var json = await _serviceHttp.PostAsnyc_streamer<string>
                    (HttpService.GameProvider.RCG_RECORD, HttpService.API_Type.GetTransactionLog, ReqJson);
                RCG_ResBase<RCG_GetTransactionLog_Res> jObj = JsonConvert.DeserializeObject<RCG_ResBase<RCG_GetTransactionLog_Res>>(json);
                res.msgId = jObj.msgId;
                res.message = jObj.message;
                res.data = jObj.data;
                if (res.msgId == (int)RCG.msgId.Success)
                {
                    res.msgId = (int)ResponseCode.Success;
                    res.message = MessageCode.Message[(int)ResponseCode.Success];
                }
                else
                {
                    throw new Exception(res.message);
                }
                return res;
            }
            catch (TaskCanceledException ex)
            {
                res.msgId = (int)RCG.msgId.TimeOut;
                res.message = RCG.msgId.TimeOut.ToString();
                _logger.LogError("RCG GetTransactionLog exception EX : {ex}  MSG : {Message}", ex.GetType().FullName, ex.Message);
                return res;
            }
            catch (Exception ex)
            {
                res.msgId = (int)ResponseCode.CheckFundTransferFail;
                res.message = ex.Message;
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                _logger.LogError("RCG GetTransactionLog exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
                return res;
            }
        }

        public Task<string> HelloWorld()
        {
            throw new NotImplementedException();
        }

        public async Task<bool> STREAMER_CreateOrSetUser(STREAMER_CreateOrSetUser request)
        {
            try
            {
                string ReqJson = System.Text.Json.JsonSerializer.Serialize(request);
                var json = await _serviceHttp.PostAsnyc<string>
                    (HttpService.GameProvider.STREAMER, HttpService.API_Type.H1CreateOrSetUser, ReqJson);
                return true;
            }
            catch (TaskCanceledException ex)
            {
                return false;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        public async Task<string> STREAMER_ForwardGameURL(STREAMER_Login request)
        {
            try
            {
                string ReqJson = System.Text.Json.JsonSerializer.Serialize(request);
                var json = await _serviceHttp.PostAsnyc<string>
                    (HttpService.GameProvider.STREAMER, HttpService.API_Type.ForwardGameURL, ReqJson);
                return json;
            }
            catch (TaskCanceledException ex)
            {
                return "fail";
            }
            catch (Exception ex)
            {
                return "fail";
            }
        }
    }
}
