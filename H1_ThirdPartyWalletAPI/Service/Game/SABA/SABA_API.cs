using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using H1_ThirdPartyWalletAPI.Model.Game;
using H1_ThirdPartyWalletAPI.Model.Config;
using Newtonsoft.Json;
using H1_ThirdPartyWalletAPI.Code;
using H1_ThirdPartyWalletAPI.Service.Common;
using Microsoft.AspNetCore.Hosting;

namespace H1_ThirdPartyWalletAPI.Service.Game
{
    public interface ISabaApiService
    {
        public Task<SABA_CreateMember_Res> CreateMember(SABA_CreateMember request);
        public Task<SABA_FundTransfer_Res> FundTransfer(SABA_FundTransfer request);
        public Task<SABA_CheckUserBalance_Res> CheckUserBalance(SABA_CheckUserBalance request);
        public Task<SABA_GetSabaUrl_Res> GetSabaUrl(SABA_GetSabaUrl request);
        public Task<SABA_GetBetDetail_Res> GetBetDetail(SABA_GetBetDetail request);
        public Task<SABA_UpdateMember_Res> UpdateMember(SABA_UpdateMember request);
        public Task<SABA_KickUser_Res> KickUser(SABA_KickUser request);
        public Task<SABA_CheckFundTransfer_Res> CheckFundTransfer(SABA_CheckFundTransfer request);
        public Task<SABA_SetMemberBetSetting_Res> SetMemberBetSetting(SABA_SetMemberBetSetting request);
        public Task<SABA_SetMemberBetSettingBySubsidiary_Res> SetMemberBetSettingBySubsidiary(SABA_SetMemberBetSettingBySubsidiary request);
        public Task<SABA_GetBetDetailByTimeframe_Res> GetBetDetailByTimeframe(SABA_GetBetDetailByTimeframe request);
        public Task<SABA_GetBetDetailByTransID_Res> GetBetDetailByTransID(SABA_GetBetDetailByTransID request);
        public Task<SABA_GetMaintenanceTime_Res> GetMaintenanceTime(SABA_GetMaintenanceTime request);
        public Task<SABA_GetOnlineUserCount_Res> GetOnlineUserCount(SABA_GetOnlineUserCount request);
        public Task<SABA_GetFinancialReport_Res> GetFinancialReport(SABA_GetFinancialReport request);

    }
    public class SABAApiService : ISabaApiService
    {
        private readonly ILogger<SABAApiService> _logger;
        private readonly IHttpService _serviceHttp;
        private readonly IWebHostEnvironment _env;
        public SABAApiService(ILogger<SABAApiService> logger, IHttpService serviceHttp, IWebHostEnvironment env)
        {
            _logger = logger;
            _serviceHttp = serviceHttp;
            _env = env;
        }

        public async Task<SABA_CreateMember_Res> CreateMember(SABA_CreateMember request)
        {
            SABA_CreateMember_Res res = new SABA_CreateMember_Res();
            try
            {
                _logger.LogDebug("SABA_CreateMember");
                SABA_CreateMember objData = request;
                objData.vendor_id = Config.CompanyToken.SABA_Token;
                objData.operatorid = "H1royal";
                objData.maxtransfer = 100000000;
                objData.mintransfer = 0;
                if (_env.EnvironmentName != "PRD")
                {
                    objData.currency = 20; //SABA測試環境僅支援UUS:20
                }
                var ReqJson = System.Text.Json.JsonSerializer.Serialize(objData);
                var dicArgus = JsonConvert.DeserializeObject<Dictionary<string, string>>(ReqJson);
                var json = await _serviceHttp.PostAsnyc_FormUrlEncoded<string>(HttpService.GameProvider.SABA, HttpService.API_Type.CreateMember, dicArgus);
                SABA_CreateMember_Res jObj = System.Text.Json.JsonSerializer.Deserialize<SABA_CreateMember_Res>(json);
                res.error_code = jObj.error_code;
                res.message = jObj.message;
                if (res.error_code == (int)SABA_CreateMember_Res.ErrorCode.Success)
                {
                    res.error_code = (int)ResponseCode.Success;
                    res.message = MessageCode.Message[(int)ResponseCode.Success];
                }
                else if (res.message == "Duplicate Vendor_Member_ID ") //已經建立過的會員視為成功
                {
                    res.error_code = (int)ResponseCode.Success;
                    res.message = MessageCode.Message[(int)ResponseCode.Success];
                }
                else
                {
                    throw new Exception(res.message);
                }
                return res;
            }
            catch(Exception ex)
            {
                res.error_code = (int)ResponseCode.CreateMemberFail;
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                _logger.LogError("SABA CreateMember exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
                return res;
            }
        }
        public async Task<SABA_UpdateMember_Res> UpdateMember(SABA_UpdateMember request)
        {
            SABA_UpdateMember_Res res = new SABA_UpdateMember_Res();
            try
            {
                _logger.LogDebug("SABA_UpdateMember");
                SABA_UpdateMember objData = request;
                objData.vendor_id = Config.CompanyToken.SABA_Token;
                objData.maxtransfer = 10000000;
                objData.mintransfer = 0;
                var ReqJson = System.Text.Json.JsonSerializer.Serialize(objData);
                var dicArgus = JsonConvert.DeserializeObject<Dictionary<string, string>>(ReqJson);
                var json = await _serviceHttp.PostAsnyc_FormUrlEncoded<string>(HttpService.GameProvider.SABA, HttpService.API_Type.UpdateMember, dicArgus);
                SABA_UpdateMember_Res jObj = System.Text.Json.JsonSerializer.Deserialize<SABA_UpdateMember_Res>(json);
                res.error_code = jObj.error_code;
                res.message = jObj.message;
                if (res.error_code == (int)SABA_UpdateMember_Res.ErrorCode.Success)
                {
                    res.error_code = (int)ResponseCode.Success;
                    res.message = MessageCode.Message[(int)ResponseCode.Success];
                }
                else
                {
                    throw new Exception(res.message);
                }

                return res;
            }
            catch (Exception ex)
            {
                res.error_code = (int)ResponseCode.UpdateMemberFail;
                res.message = MessageCode.Message[(int)ResponseCode.UpdateMemberFail];
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                _logger.LogError("SABA UpdateMember exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
                return res;
            }
        }
        public async Task<SABA_CheckUserBalance_Res> CheckUserBalance(SABA_CheckUserBalance request)
        {
            SABA_CheckUserBalance_Res res = new SABA_CheckUserBalance_Res();
            try
            {
                _logger.LogDebug("CheckUserBalance");
                SABA_CheckUserBalance objData = request;
                objData.wallet_id = 1;
                objData.vendor_id = Config.CompanyToken.SABA_Token;
                var ReqJson = System.Text.Json.JsonSerializer.Serialize(objData);
                var dicArgus = JsonConvert.DeserializeObject<Dictionary<string, string>>(ReqJson);
                var json = await _serviceHttp.PostAsnyc_FormUrlEncoded<string>(HttpService.GameProvider.SABA, HttpService.API_Type.CheckUserBalance, dicArgus);
                SABA_CheckUserBalance_Res jObj = JsonConvert.DeserializeObject<SABA_CheckUserBalance_Res>(json);
                res.error_code = jObj.error_code;
                res.message = jObj.message;
                res.Data = jObj.Data;
                if (res.error_code == (int)SABA_CheckUserBalance_Res.ErrorCode.Success)
                {
                    res.error_code = (int)ResponseCode.Success;
                    res.message = MessageCode.Message[(int)ResponseCode.Success];
                }
                else
                {
                    throw new Exception(res.message);
                }
                return res;
            }
            catch (Exception ex)
            {
                res.error_code = (int)ResponseCode.GetBalanceFail;
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                _logger.LogError("SABA GetBalance exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
                return res;
            }
        }
        public async Task<SABA_KickUser_Res> KickUser(SABA_KickUser request)
        {
            SABA_KickUser_Res res = new SABA_KickUser_Res();
            try
            {
                SABA_KickUser objData = request;
                objData.vendor_id = Config.CompanyToken.SABA_Token;
                var ReqJson = System.Text.Json.JsonSerializer.Serialize(objData);
                var dicArgus = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(ReqJson);
                var json = await _serviceHttp.PostAsnyc_FormUrlEncoded<string>(HttpService.GameProvider.SABA, HttpService.API_Type.KickUser, dicArgus);
                SABA_KickUser_Res jObj = System.Text.Json.JsonSerializer.Deserialize<SABA_KickUser_Res>(json);
                res.error_code = jObj.error_code;
                res.message = jObj.message;
                if (res.error_code == (int)SABA_KickUser_Res.ErrorCode.Success)
                {
                    res.error_code = (int)ResponseCode.Success;
                    res.message = MessageCode.Message[(int)ResponseCode.Success];
                }
                else
                {
                    throw new Exception(res.message);
                }
                return res;
            }
            catch (Exception ex)
            {
                res.error_code = (int)ResponseCode.KickUserFail;
                res.message = MessageCode.Message[(int)ResponseCode.KickUserFail];
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                if (ex.Message.ToString() == "User is not online")
                {
                    _logger.LogInformation("SABA KickUser exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
                }
                else
                {
                    _logger.LogError("SABA KickUser exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
                }
                return res;
            }
        }
        public async Task<SABA_FundTransfer_Res> FundTransfer(SABA_FundTransfer request)
        {
            SABA_FundTransfer_Res res = new SABA_FundTransfer_Res();
            try
            {
                _logger.LogDebug("SABA_CreateMember");
                SABA_FundTransfer objData = request;
                objData.vendor_id = Config.CompanyToken.SABA_Token;
                objData.vendor_trans_id = request.vendor_trans_id;
                objData.wallet_id = 1; //1 : 體育
                if (_env.EnvironmentName != "PRD")
                {
                    objData.currency = 20; //SABA測試環境僅支援UUS:20
                }
                string ReqJson = System.Text.Json.JsonSerializer.Serialize(objData);
                var dicArgus = JsonConvert.DeserializeObject<Dictionary<string, string>>(ReqJson);
                var json = await _serviceHttp.PostAsnyc_FormUrlEncoded<string>(HttpService.GameProvider.SABA, HttpService.API_Type.FundTransfer, dicArgus);
                SABA_FundTransfer_Res jObj = System.Text.Json.JsonSerializer.Deserialize<SABA_FundTransfer_Res>(json);
                res.error_code = jObj.error_code;
                res.message = jObj.message;
                res.Data = jObj.Data;

                if (res.error_code == (int)SABA_FundTransfer_Res.ErrorCode.Success)
                {
                    res.error_code = (int)ResponseCode.Success;
                    res.message = MessageCode.Message[(int)ResponseCode.Success];
                }
                else if (res.error_code == (int)SABA_GetMaintenanceTime_Res.ErrorCode.SystemMaintain)
                {
                    res.error_code = (int)ResponseCode.GameApiMaintain;
                    res.message = MessageCode.Message[(int)ResponseCode.GameApiMaintain];
                }
                else
                {
                    throw new Exception(res.message);
                }
                return res;
            }
            catch (TaskCanceledException ex)
            {
                res.error_code = (int)ResponseCode.TimeOut;
                res.message = MessageCode.Message[(int)ResponseCode.TimeOut];
                _logger.LogError("SABA FundTransfer exception EX : {ex}  MSG : {Message}", ex.GetType().FullName, ex.Message);
                return res;
            }
            catch (Exception ex)
            {
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                res.error_code = (int)ResponseCode.FundTransferFail;
                res.message = MessageCode.Message[(int)ResponseCode.FundTransferFail];
                _logger.LogError("SABA FundTransfer exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
                return res;
            }
        }
        public async Task<SABA_GetSabaUrl_Res> GetSabaUrl(SABA_GetSabaUrl request)
        {
            SABA_GetSabaUrl_Res res = new SABA_GetSabaUrl_Res();
            try
            {
                _logger.LogDebug("SABA_GetSabaUrl");
                SABA_GetSabaUrl objData = request;
                objData.vendor_id = Config.CompanyToken.SABA_Token;

                var ReqJson = System.Text.Json.JsonSerializer.Serialize(objData);
                var dicArgus = JsonConvert.DeserializeObject<Dictionary<string, string>>(ReqJson);
                var json = await _serviceHttp.PostAsnyc_FormUrlEncoded<string>(HttpService.GameProvider.SABA, HttpService.API_Type.GetSabaUrl, dicArgus);
                //var jObj = Newtonsoft.Json.Linq.JObject.Parse(json);
                res = System.Text.Json.JsonSerializer.Deserialize<SABA_GetSabaUrl_Res>(json);
                if (res.error_code == (int)SABA_GetSabaUrl_Res.ErrorCode.Success)
                {
                    res.error_code = (int)ResponseCode.Success;
                    res.message = MessageCode.Message[(int)ResponseCode.Success];
                }
                else
                {
                    throw new Exception(res.message);
                }
                return res;
            }
            catch (Exception ex)
            {
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                res.error_code = (int)ResponseCode.GetGameURLFail;
                res.message = MessageCode.Message[(int)ResponseCode.GetGameURLFail];
                _logger.LogError("GetSabaUrl exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
                return res;
            }
        }
        public async Task<SABA_GetBetDetail_Res> GetBetDetail(SABA_GetBetDetail request)
        {
            SABA_GetBetDetail_Res res = new SABA_GetBetDetail_Res();
            res.Data = new SABA_Game_Record();
            try
            {
                _logger.LogDebug("SABA_GetBetDetail");              
                SABA_GetBetDetail objData = request;
                objData.vendor_id = Config.CompanyToken.SABA_Token;

                var ReqJson = JsonConvert.SerializeObject(objData);
                var dicArgus = JsonConvert.DeserializeObject<Dictionary<string, string>>(ReqJson);
                var json = await _serviceHttp.PostAsnyc_FormUrlEncoded<string>(HttpService.GameProvider.SABA, HttpService.API_Type.GetBetDetail, dicArgus);
                SABA_GetBetDetail_Res jObj = JsonConvert.DeserializeObject<SABA_GetBetDetail_Res>(json);
                res.error_code = jObj.error_code;
                res.message = jObj.message;
                if (res.error_code == (int)SABA_GetBetDetail_Res.ErrorCode.Success)
                {
                    res.error_code = (int)ResponseCode.Success;
                    res.message = MessageCode.Message[(int)ResponseCode.Success];
                    res.Data.last_version_key = jObj.Data.last_version_key;
                    res.Data.BetDetails = jObj.Data.BetDetails;
                }
                else
                {
                    throw new Exception(res.message);
                }
                return res;
            }
            catch (Exception ex)
            {
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                res.error_code = (int)ResponseCode.GetGameRecordFail;
                res.message = MessageCode.Message[(int)ResponseCode.GetGameRecordFail];
                _logger.LogError("SABA_GetBetDetail exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
                return res;
            }
        }
        public async Task<SABA_GetBetDetailByTimeframe_Res> GetBetDetailByTimeframe(SABA_GetBetDetailByTimeframe request)
        {
            SABA_GetBetDetailByTimeframe_Res res = new SABA_GetBetDetailByTimeframe_Res();
            res.Data = new SABA_Game_Record();
            try
            {
                _logger.LogDebug("GetBetDetailByTimeframe");
                SABA_GetBetDetailByTimeframe objData = request;
                objData.vendor_id = Config.CompanyToken.SABA_Token;

                var ReqJson = JsonConvert.SerializeObject(objData);
                var dicArgus = JsonConvert.DeserializeObject<Dictionary<string, string>>(ReqJson);
                var json = await _serviceHttp.PostAsnyc_FormUrlEncoded<string>(HttpService.GameProvider.SABA, HttpService.API_Type.GetBetDetailByTimeframe, dicArgus);
                SABA_GetBetDetailByTimeframe_Res jObj = JsonConvert.DeserializeObject<SABA_GetBetDetailByTimeframe_Res>(json);
                res.error_code = jObj.error_code;
                res.message = jObj.message;
                if (res.error_code == (int)SABA_GetBetDetailByTimeframe_Res.ErrorCode.Success)
                {
                    res.error_code = (int)ResponseCode.Success;
                    res.message = MessageCode.Message[(int)ResponseCode.Success];
                    res.Data.last_version_key = jObj.Data.last_version_key;
                    res.Data.BetDetails = jObj.Data.BetDetails;
                }
                else
                {
                    throw new Exception(res.message);
                }
                return res;
            }
            catch (Exception ex)
            {
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                res.error_code = (int)ResponseCode.GetGameRecordFail;
                res.message = MessageCode.Message[(int)ResponseCode.GetGameRecordFail];
                _logger.LogError("SABA_GetBetDetailByTimeframe exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
                return res;
            }
        }
        public async Task<SABA_GetBetDetailByTransID_Res> GetBetDetailByTransID(SABA_GetBetDetailByTransID request)
        {
            SABA_GetBetDetailByTransID_Res res = new SABA_GetBetDetailByTransID_Res();

            try
            {
                _logger.LogDebug("SABA_GetBetDetailByTransID");
                SABA_GetBetDetailByTransID objData = request;
                objData.vendor_id = Config.CompanyToken.SABA_Token;
                var ReqJson = System.Text.Json.JsonSerializer.Serialize(objData);
                var dicArgus = JsonConvert.DeserializeObject<Dictionary<string, string>>(ReqJson);
                var json = await _serviceHttp.PostAsnyc_FormUrlEncoded<string>(HttpService.GameProvider.SABA, HttpService.API_Type.GetBetDetailByTransID, dicArgus);
                var jObj = Newtonsoft.Json.Linq.JObject.Parse(json);
                res.error_code = jObj.Value<int>("error_code");
                res.message = jObj.Value<string>("message");
                if (res.error_code == (int)SABA_GetBetDetailByTransID_Res.ErrorCode.Success)
                {
                    res.Data = jObj["Data"]["BetDetails"].FirstOrDefault();
                    res.error_code = (int)ResponseCode.Success;
                    res.message = MessageCode.Message[(int)ResponseCode.Success];
                }
                else
                {
                    throw new Exception(res.message);
                }
                return res;
            }
            catch (Exception ex)
            {
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                res.error_code = (int)ResponseCode.GetGameRecordFail;
                res.message = MessageCode.Message[(int)ResponseCode.GetGameRecordFail] + " | " + ex.Message.ToString();
                _logger.LogError("SABA_GetBetDetailByTransID exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
                return res;
            }
        }
        public async Task<SABA_CheckFundTransfer_Res> CheckFundTransfer(SABA_CheckFundTransfer request)
        {
            SABA_CheckFundTransfer_Res res = new SABA_CheckFundTransfer_Res();
            try
            {
                _logger.LogDebug("SABA_CreateMember");
                SABA_CheckFundTransfer objData = request;
                objData.vendor_id = Config.CompanyToken.SABA_Token;
                objData.wallet_id = 1; //1 : 體育
                var ReqJson = JsonConvert.SerializeObject(objData);
                var dicArgus = JsonConvert.DeserializeObject<Dictionary<string, string>>(ReqJson);
                var json = await _serviceHttp.PostAsnyc_FormUrlEncoded<string>(HttpService.GameProvider.SABA, HttpService.API_Type.CheckFundTransfer, dicArgus);

                SABA_CheckFundTransfer_Res jObj = JsonConvert.DeserializeObject<SABA_CheckFundTransfer_Res>(json);
                res.error_code = jObj.error_code;
                res.message = jObj.message;
                res.Data = jObj.Data;

                if (res.error_code == (int)SABA_CheckFundTransfer_Res.ErrorCode.Success)
                {
                    res.error_code = (int)ResponseCode.Success;
                    res.message = MessageCode.Message[(int)ResponseCode.Success];
                }
                else if ((res.error_code == (int)SABA_CheckFundTransfer_Res.ErrorCode.TidNotExist))
                {
                }
                else
                {
                    throw new Exception(res.message);
                }
                return res;
            }
            catch (Exception ex)
            {
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                res.error_code = (int)ResponseCode.CheckFundTransferFail;
                res.message = MessageCode.Message[(int)ResponseCode.CheckFundTransferFail];
                _logger.LogError("SABA FundTransfer exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
                return res;
            }
        }
        public async Task<SABA_SetMemberBetSetting_Res> SetMemberBetSetting(SABA_SetMemberBetSetting request)
        {
            SABA_SetMemberBetSetting_Res res = new SABA_SetMemberBetSetting_Res();
            try
            {
                _logger.LogDebug("Set Member Bet Setting");
                SABA_SetMemberBetSetting objData = request;
                objData.vendor_id = Config.CompanyToken.SABA_Token;
                var ReqJson = System.Text.Json.JsonSerializer.Serialize(objData);
                var dicArgus = JsonConvert.DeserializeObject<Dictionary<string, string>>(ReqJson);
                var json = await _serviceHttp.PostAsnyc_FormUrlEncoded<string>(HttpService.GameProvider.SABA, HttpService.API_Type.SetMemberBetSetting, dicArgus);
                SABA_SetMemberBetSetting_Res jObj = JsonConvert.DeserializeObject<SABA_SetMemberBetSetting_Res>(json);
                res.error_code = jObj.error_code;
                res.message = jObj.message;
                if (res.error_code == (int)SABA_SetMemberBetSetting_Res.ErrorCode.Success)
                {
                    res.error_code = (int)ResponseCode.Success;
                    res.message = MessageCode.Message[(int)ResponseCode.Success];
                }
                else
                {
                    throw new Exception(res.message);
                }
                return res;
            }
            catch (Exception ex)
            {
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                res.error_code = (int)ResponseCode.SetLimitFail;
                res.message = MessageCode.Message[(int)ResponseCode.SetLimitFail];
                _logger.LogError("SABA SetMemberBetSetting exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
                return res;
            }
        }
        public async Task<SABA_SetMemberBetSettingBySubsidiary_Res> SetMemberBetSettingBySubsidiary(SABA_SetMemberBetSettingBySubsidiary request)
        {
            SABA_SetMemberBetSettingBySubsidiary_Res res = new SABA_SetMemberBetSettingBySubsidiary_Res();
            try
            {
                _logger.LogDebug("Set Member Bet Setting");
                SABA_SetMemberBetSettingBySubsidiary objData = request;
                objData.vendor_id = Config.CompanyToken.SABA_Token;

                var ReqJson = JsonConvert.SerializeObject(objData);
                var dicArgus = JsonConvert.DeserializeObject<Dictionary<string, string>>(ReqJson);
                var json = await _serviceHttp.PostAsnyc_FormUrlEncoded<string>(HttpService.GameProvider.SABA, HttpService.API_Type.SetMemberBetSettingBySubsidiary, dicArgus);
                //var jObj = Newtonsoft.Json.Linq.JObject.Parse(json);
                SABA_SetMemberBetSettingBySubsidiary_Res jObj = JsonConvert.DeserializeObject<SABA_SetMemberBetSettingBySubsidiary_Res>(json);
                res.error_code = jObj.error_code;
                res.message = jObj.message;
                if (res.error_code == (int)SABA_SetMemberBetSettingBySubsidiary_Res.ErrorCode.Success)
                {
                    res.error_code = (int)ResponseCode.Success;
                    res.message = MessageCode.Message[(int)ResponseCode.Success];
                }
                else
                {
                    throw new Exception(res.message);
                }
                return res;
            }
            catch (Exception ex)
            {
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                res.error_code = (int)ResponseCode.SetGroupLimitFail;
                res.message = MessageCode.Message[(int)ResponseCode.SetGroupLimitFail];
                _logger.LogError("SABA SetMemberBetSettingBySubsidiary exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
                return res;
            }
        }
        public async Task<SABA_GetMaintenanceTime_Res> GetMaintenanceTime(SABA_GetMaintenanceTime request)
        {
            SABA_GetMaintenanceTime_Res res = new SABA_GetMaintenanceTime_Res();
            try
            {
                _logger.LogDebug("Get SABA Maintenance Time");
                SABA_GetMaintenanceTime objData = request;
                objData.vendor_id = Config.CompanyToken.SABA_Token;

                var ReqJson = JsonConvert.SerializeObject(objData);
                var dicArgus = JsonConvert.DeserializeObject<Dictionary<string, string>>(ReqJson);
                var json = await _serviceHttp.PostAsnyc_FormUrlEncoded<string>(HttpService.GameProvider.SABA, HttpService.API_Type.GetMaintenanceTime, dicArgus);
                res = JsonConvert.DeserializeObject<SABA_GetMaintenanceTime_Res>(json);
                if (res.error_code == (int)SABA_GetMaintenanceTime_Res.ErrorCode.Success)
                {
                    res.error_code = (int)ResponseCode.Success;
                    res.message = MessageCode.Message[(int)ResponseCode.Success];
                }
                else if(res.error_code == (int)SABA_GetMaintenanceTime_Res.ErrorCode.SystemMaintain)
                {
                    res.Data = new SABA_MaintenanceTimeData();
                    res.error_code = (int)ResponseCode.Success;
                    res.message = MessageCode.Message[(int)ResponseCode.Success];
                    res.Data.IsUM = true;
                }
                else
                {
                    throw new Exception(res.message);
                }
                return res;
            }
            catch (Exception ex)
            {
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                res.error_code = (int)ResponseCode.GetGameMaintenanceFail;
                res.message = MessageCode.Message[(int)ResponseCode.GetGameMaintenanceFail];
                _logger.LogError("SABA GetMaintenanceTime exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
                return res;
            }
        }
        public async Task<SABA_GetOnlineUserCount_Res> GetOnlineUserCount(SABA_GetOnlineUserCount request)
        {
            SABA_GetOnlineUserCount_Res res = new SABA_GetOnlineUserCount_Res();
            try
            {
                SABA_GetOnlineUserCount objData = request;
                objData.vendor_id = Config.CompanyToken.SABA_Token;

                var ReqJson = JsonConvert.SerializeObject(objData);
                var dicArgus = JsonConvert.DeserializeObject<Dictionary<string, string>>(ReqJson);
                var json = await _serviceHttp.PostAsnyc_FormUrlEncoded<string>(HttpService.GameProvider.SABA, HttpService.API_Type.GetOnlineUserCount, dicArgus);
                res = JsonConvert.DeserializeObject<SABA_GetOnlineUserCount_Res>(json);
                if (res.error_code == (int)SABA_GetOnlineUserCount_Res.ErrorCode.Success)
                {
                    res.error_code = (int)ResponseCode.Success;
                    res.message = MessageCode.Message[(int)ResponseCode.Success];
                }
                else
                {
                    throw new Exception(res.message);
                }
                return res;
            }
            catch (Exception ex)
            {
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                _logger.LogError("SABA GetMaintenanceTime exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
                return res;
            }
        }
        public async Task<SABA_GetFinancialReport_Res> GetFinancialReport(SABA_GetFinancialReport request)
        {
            SABA_GetFinancialReport_Res res = new SABA_GetFinancialReport_Res();
            try
            {
                SABA_GetFinancialReport objData = request;
                objData.vendor_id = Config.CompanyToken.SABA_Token;
                if (_env.EnvironmentName != "PRD")
                {
                    objData.currency = 20; //SABA測試環境僅支援UUS:20
                }

                var ReqJson = JsonConvert.SerializeObject(objData);
                var dicArgus = JsonConvert.DeserializeObject<Dictionary<string, string>>(ReqJson);
                var json = await _serviceHttp.PostAsnyc_FormUrlEncoded<string>(HttpService.GameProvider.SABA, HttpService.API_Type.GetFinancialReport, dicArgus);
                res = JsonConvert.DeserializeObject<SABA_GetFinancialReport_Res>(json);
                if (res.error_code == (int)SABA_GetOnlineUserCount_Res.ErrorCode.Success)
                {
                    res.error_code = (int)ResponseCode.Success;
                    res.message = MessageCode.Message[(int)ResponseCode.Success];
                }
                else
                {
                    throw new Exception(res.message);
                }
                return res;
            }
            catch (Exception ex)
            {
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                _logger.LogError("SABA GetMaintenanceTime exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
                return res;
            }
        }
    }
}
