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
using static H1_ThirdPartyWalletAPI.Service.Common.HttpService;
using System.Net.Http;
using static Google.Rpc.Context.AttributeContext.Types;
using H1_ThirdPartyWalletAPI.Extensions;

namespace H1_ThirdPartyWalletAPI.Service.Game
{
    public interface ISaba2ApiService
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
        Task<SABA_GetBetSettingLimit_Res> GetBetSettingLimit(SABA_GetBetSettingLimit request);
    }
    public class SABA2ApiService : ISaba2ApiService
    {
        private readonly ILogger<SABA2ApiService> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IWebHostEnvironment _env;

        public SABA2ApiService(ILogger<SABA2ApiService> logger, IHttpClientFactory httpClientFactory, IWebHostEnvironment env)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _env = env;
        }

        public async Task<SABA_CreateMember_Res> CreateMember(SABA_CreateMember request)
        {
            SABA_CreateMember_Res res = new SABA_CreateMember_Res();
            try
            {
                _logger.LogDebug("SABA2_CreateMember");
                SABA_CreateMember objData = request;
                objData.vendor_id = Config.CompanyToken.SABA2_Token;
                objData.operatorid = Config.CompanyToken.SABA2_Operatorid;
                objData.maxtransfer = 100000000;
                objData.mintransfer = 0;
                if (_env.EnvironmentName != "PRD")
                {
                    objData.currency = 20; //SABA測試環境僅支援UUS:20
                }
                var ReqJson = System.Text.Json.JsonSerializer.Serialize(objData);
                var dicArgus = JsonConvert.DeserializeObject<Dictionary<string, string>>(ReqJson);
                var json = await PostAsnyc<string>(HttpService.GameProvider.SABA2, HttpService.API_Type.CreateMember, dicArgus);
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
            catch (Exception ex)
            {
                res.error_code = (int)ResponseCode.CreateMemberFail;
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                _logger.LogError("SABA2 CreateMember exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
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
                objData.vendor_id = Config.CompanyToken.SABA2_Token;
                objData.maxtransfer = 10000000;
                objData.mintransfer = 0;
                var ReqJson = System.Text.Json.JsonSerializer.Serialize(objData);
                var dicArgus = JsonConvert.DeserializeObject<Dictionary<string, string>>(ReqJson);
                var json = await PostAsnyc<string>(HttpService.GameProvider.SABA2, HttpService.API_Type.UpdateMember, dicArgus);
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
                _logger.LogError("SABA2 UpdateMember exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
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
                objData.vendor_id = Config.CompanyToken.SABA2_Token;
                var ReqJson = System.Text.Json.JsonSerializer.Serialize(objData);
                var dicArgus = JsonConvert.DeserializeObject<Dictionary<string, string>>(ReqJson);
                var json = await PostAsnyc<string>(HttpService.GameProvider.SABA2, HttpService.API_Type.CheckUserBalance, dicArgus);
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
                _logger.LogError("SABA2 GetBalance exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
                return res;
            }
        }
        public async Task<SABA_KickUser_Res> KickUser(SABA_KickUser request)
        {
            SABA_KickUser_Res res = new SABA_KickUser_Res();
            try
            {
                SABA_KickUser objData = request;
                objData.vendor_id = Config.CompanyToken.SABA2_Token;
                var ReqJson = System.Text.Json.JsonSerializer.Serialize(objData);
                var dicArgus = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(ReqJson);
                var json = await PostAsnyc<string>(HttpService.GameProvider.SABA2, HttpService.API_Type.KickUser, dicArgus);
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
                    _logger.LogInformation("SABA2 KickUser exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
                }
                else
                {
                    _logger.LogError("SABA2 KickUser exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
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
                objData.vendor_id = Config.CompanyToken.SABA2_Token;
                objData.vendor_trans_id = Config.CompanyToken.SABA2_Operatorid + "_" + request.vendor_trans_id;
                objData.wallet_id = 1; //1 : 體育
                if (_env.EnvironmentName != "PRD")
                {
                    objData.currency = 20; //SABA測試環境僅支援UUS:20
                }
                string ReqJson = System.Text.Json.JsonSerializer.Serialize(objData);
                var dicArgus = JsonConvert.DeserializeObject<Dictionary<string, string>>(ReqJson);
                var json = await PostAsnyc<string>(HttpService.GameProvider.SABA2, HttpService.API_Type.FundTransfer, dicArgus);
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
                _logger.LogError("SABA2 FundTransfer exception EX : {ex}  MSG : {Message}", ex.GetType().FullName, ex.Message);
                return res;
            }
            catch (Exception ex)
            {
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                res.error_code = (int)ResponseCode.FundTransferFail;
                res.message = MessageCode.Message[(int)ResponseCode.FundTransferFail];
                _logger.LogError("SABA2 FundTransfer exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
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
                objData.vendor_id = Config.CompanyToken.SABA2_Token;

                var ReqJson = System.Text.Json.JsonSerializer.Serialize(objData);
                var dicArgus = JsonConvert.DeserializeObject<Dictionary<string, string>>(ReqJson);
                var json = await PostAsnyc<string>(HttpService.GameProvider.SABA2, HttpService.API_Type.GetSabaUrl, dicArgus);
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
                _logger.LogError("GetSaba2Url exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
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
                objData.vendor_id = Config.CompanyToken.SABA2_Token;

                var ReqJson = JsonConvert.SerializeObject(objData);
                var dicArgus = JsonConvert.DeserializeObject<Dictionary<string, string>>(ReqJson);
                var json = await PostAsnyc<string>(HttpService.GameProvider.SABA2, HttpService.API_Type.GetBetDetail, dicArgus);
                SABA_GetBetDetail_Res jObj = JsonConvert.DeserializeObject<SABA_GetBetDetail_Res>(json);
                res.error_code = jObj.error_code;
                res.message = jObj.message;
                if (res.error_code == (int)SABA_GetBetDetail_Res.ErrorCode.Success)
                {
                    res.error_code = (int)ResponseCode.Success;
                    res.message = MessageCode.Message[(int)ResponseCode.Success];
                    res.Data.last_version_key = jObj.Data.last_version_key;
                    res.Data.BetDetails = jObj.Data.BetDetails;
                    res.Data.BetDetails.AddRange(jObj.Data.BetNumberDetails);
                    res.Data.BetDetails.AddRange(jObj.Data.BetVirtualSportDetails);
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
                _logger.LogError(ex, "SABA2_GetBetDetail exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
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
                objData.vendor_id = Config.CompanyToken.SABA2_Token;

                var ReqJson = JsonConvert.SerializeObject(objData);
                var dicArgus = JsonConvert.DeserializeObject<Dictionary<string, string>>(ReqJson);
                var json = await PostAsnyc<string>(HttpService.GameProvider.SABA2, HttpService.API_Type.GetBetDetailByTimeframe, dicArgus);
                SABA_GetBetDetailByTimeframe_Res jObj = JsonConvert.DeserializeObject<SABA_GetBetDetailByTimeframe_Res>(json);
                res.error_code = jObj.error_code;
                res.message = jObj.message;
                if (res.error_code == (int)SABA_GetBetDetailByTimeframe_Res.ErrorCode.Success)
                {
                    res.error_code = (int)ResponseCode.Success;
                    res.message = MessageCode.Message[(int)ResponseCode.Success];
                    res.Data.last_version_key = jObj.Data.last_version_key;
                    res.Data.BetDetails = jObj.Data.BetDetails;
                    res.Data.BetDetails.AddRange(jObj.Data.BetNumberDetails);
                    res.Data.BetDetails.AddRange(jObj.Data.BetVirtualSportDetails);
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
                _logger.LogError("SABA2_GetBetDetailByTimeframe exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
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
                objData.vendor_id = Config.CompanyToken.SABA2_Token;
                var ReqJson = System.Text.Json.JsonSerializer.Serialize(objData);
                var dicArgus = JsonConvert.DeserializeObject<Dictionary<string, string>>(ReqJson);
                var json = await PostAsnyc<string>(HttpService.GameProvider.SABA2, HttpService.API_Type.GetBetDetailByTransID, dicArgus);
                var jObj = Newtonsoft.Json.Linq.JObject.Parse(json);
                res.error_code = jObj.Value<int>("error_code");
                res.message = jObj.Value<string>("message");
                if (res.error_code == (int)SABA_GetBetDetailByTransID_Res.ErrorCode.Success)
                {
                    res.Data = (jObj["Data"]["BetDetails"] != null) ? jObj["Data"]["BetDetails"].FirstOrDefault() :
                        ((jObj["Data"]["BetNumberDetails"] != null) ? jObj["Data"]["BetNumberDetails"].FirstOrDefault() : jObj["Data"]["BetVirtualSportDetails"].FirstOrDefault());
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
                _logger.LogError("SABA2_GetBetDetailByTransID exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
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
                objData.vendor_id = Config.CompanyToken.SABA2_Token;
                objData.wallet_id = 1; //1 : 體育
                var ReqJson = JsonConvert.SerializeObject(objData);
                var dicArgus = JsonConvert.DeserializeObject<Dictionary<string, string>>(ReqJson);
                var json = await PostAsnyc<string>(HttpService.GameProvider.SABA2, HttpService.API_Type.CheckFundTransfer, dicArgus);

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
                _logger.LogError("SABA2 FundTransfer exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
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
                objData.vendor_id = Config.CompanyToken.SABA2_Token;
                var ReqJson = System.Text.Json.JsonSerializer.Serialize(objData);
                var dicArgus = JsonConvert.DeserializeObject<Dictionary<string, string>>(ReqJson);
                var json = await PostAsnyc<string>(HttpService.GameProvider.SABA2, HttpService.API_Type.SetMemberBetSetting, dicArgus);
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
                _logger.LogError("SABA2 SetMemberBetSetting exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
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
                objData.vendor_id = Config.CompanyToken.SABA2_Token;

                var ReqJson = JsonConvert.SerializeObject(objData);
                var dicArgus = JsonConvert.DeserializeObject<Dictionary<string, string>>(ReqJson);
                var json = await PostAsnyc<string>(HttpService.GameProvider.SABA2, HttpService.API_Type.SetMemberBetSettingBySubsidiary, dicArgus);
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
                _logger.LogError("SABA2 SetMemberBetSettingBySubsidiary exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
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
                objData.vendor_id = Config.CompanyToken.SABA2_Token;

                var ReqJson = JsonConvert.SerializeObject(objData);
                var dicArgus = JsonConvert.DeserializeObject<Dictionary<string, string>>(ReqJson);
                var json = await PostAsnyc<string>(HttpService.GameProvider.SABA2, HttpService.API_Type.GetMaintenanceTime, dicArgus);
                res = JsonConvert.DeserializeObject<SABA_GetMaintenanceTime_Res>(json);
                if (res.error_code is (int)SABA_GetMaintenanceTime_Res.ErrorCode.Success 
                    or (int)SABA_GetMaintenanceTime_Res.ErrorCode.Nofund)//No Found表示當下無維護
                {
                    res.error_code = (int)ResponseCode.Success;
                    res.message = MessageCode.Message[(int)ResponseCode.Success];
                }
                else if (res.error_code == (int)SABA_GetMaintenanceTime_Res.ErrorCode.SystemMaintain)
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
                _logger.LogError("SABA2 GetMaintenanceTime exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
                return res;
            }
        }
        public async Task<SABA_GetOnlineUserCount_Res> GetOnlineUserCount(SABA_GetOnlineUserCount request)
        {
            SABA_GetOnlineUserCount_Res res = new SABA_GetOnlineUserCount_Res();
            try
            {
                SABA_GetOnlineUserCount objData = request;
                objData.vendor_id = Config.CompanyToken.SABA2_Token;

                var ReqJson = JsonConvert.SerializeObject(objData);
                var dicArgus = JsonConvert.DeserializeObject<Dictionary<string, string>>(ReqJson);
                var json = await PostAsnyc<string>(HttpService.GameProvider.SABA2, HttpService.API_Type.GetOnlineUserCount, dicArgus);
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
                _logger.LogError("SABA2 GetMaintenanceTime exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
                return res;
            }
        }
        public async Task<SABA_GetFinancialReport_Res> GetFinancialReport(SABA_GetFinancialReport request)
        {
            SABA_GetFinancialReport_Res res = new SABA_GetFinancialReport_Res();
            try
            {
                SABA_GetFinancialReport objData = request;
                objData.vendor_id = Config.CompanyToken.SABA2_Token;

                var ReqJson = JsonConvert.SerializeObject(objData);
                var dicArgus = JsonConvert.DeserializeObject<Dictionary<string, string>>(ReqJson);
                var json = await PostAsnyc<string>(HttpService.GameProvider.SABA2, HttpService.API_Type.GetFinancialReport, dicArgus);
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
                _logger.LogError("SABA2 GetMaintenanceTime exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
                return res;
            }
        }

        public async Task<SABA_GetBetSettingLimit_Res> GetBetSettingLimit(SABA_GetBetSettingLimit request)
        {
            SABA_GetBetSettingLimit_Res res = new SABA_GetBetSettingLimit_Res();
            try
            {
                _logger.LogDebug("SABA_GetBetSettingLimit");
                SABA_GetBetSettingLimit objData = request;
                objData.vendor_id = Config.CompanyToken.SABA2_Token;

                var ReqJson = System.Text.Json.JsonSerializer.Serialize(objData);
                var dicArgus = JsonConvert.DeserializeObject<Dictionary<string, string>>(ReqJson);
                var json = await PostAsnyc<string>(HttpService.GameProvider.SABA2, HttpService.API_Type.GetBetSettingLimit, dicArgus);
                res = System.Text.Json.JsonSerializer.Deserialize<SABA_GetBetSettingLimit_Res>(json);
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
                res.error_code = (int)ResponseCode.Fail;
                res.message = MessageCode.Message[(int)ResponseCode.Fail];
                _logger.LogError(ex, "SABA2 GetBetSettingLimit exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
                return res;
            }
        }

        public async Task<T> PostAsnyc<T>(GameProvider gameProvider, API_Type apiType, Dictionary<string, string> postBody = null, Dictionary<string, string> queryString = null)
        {
            var apiResInfo = new ApiResponseData();
            try
            {
                string url = this.API_TypeToUrl(apiType);
                if (queryString != null)
                    url += this.DictionaryToQueryString(queryString);

                var formData = new FormUrlEncodedContent(postBody);

                using (var httpClient = _httpClientFactory.CreateClient("log"))
                {
                    httpClient.Timeout = TimeSpan.FromSeconds(14);
                    httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/x-www-form-urlencoded");

                    var sw = System.Diagnostics.Stopwatch.StartNew();
                    var httpResult = await httpClient.PostAsync(Platform.SABA2, url, formData);
                    sw.Stop();
                    apiResInfo.ElapsedMilliseconds = sw.ElapsedMilliseconds;
                    var result = await httpResult.Content.ReadAsStringAsync();
                    var dic = new Dictionary<string, object>();
                    dic.Add("request", postBody);
                    dic.Add("response", result);

                    using (var scope = this._logger.BeginScope(dic))
                    {
                        this._logger.LogInformation("Get RequestPath: {RequestPath} | ResponseHttpStatus:{Status} | exeTime:{exeTime} ms", url, httpResult.StatusCode, sw.Elapsed.TotalMilliseconds);
                    }
                    if (httpResult.IsSuccessStatusCode == false)
                        throw new Exception($"呼叫 APi 失敗：{apiType.ToString()}");
                    var tType = typeof(T);
                    if (tType.IsValueType || tType.Equals(typeof(string)))
                        return (T)Convert.ChangeType(result, tType);
                    else
                        return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(result);
                }
            }
            catch (TaskCanceledException ex)
            {
                apiResInfo.ElapsedMilliseconds = 99999;
                throw;
            }
        }


        private string API_TypeToUrl(API_Type apiType)
        {
            var url = System.IO.Path.Combine(Config.GameAPI.SABA2_URL, $"{apiType.ToString().Replace("_", @"/")}");
            return url;
        }

        private string DictionaryToQueryString(Dictionary<string, string> dicArgus)
        {
            var ar = (from item in dicArgus
                      select $"{item.Key}={item.Value}").ToArray();

            if (ar.Length > 0)
                return "?" + string.Join('&', ar);
            else
                return string.Empty;
        }
    }
}
