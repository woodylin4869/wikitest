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
using H1_ThirdPartyWalletAPI.Model.W1API;
using H1_ThirdPartyWalletAPI.Model.H1API;
using H1_ThirdPartyWalletAPI.Model;
using H1_ThirdPartyWalletAPI.Model.DataModel;

namespace H1_ThirdPartyWalletAPI.Service.Game
{
    public interface IH1ApiService
    {
        Task<RefundAmountRes> Refund(RefundAmountReq request);
        Task<SettleBetRecordRes> Settle(SettleBetRecordReq request);
        Task<ResCodeBase> HealthCheck();
        Task<ResCodeBase> Kick(KickUserReq KickUserData);
    }
    public class H1ApiService : IH1ApiService
    {
        private readonly ILogger<H1ApiService> _logger;
        private readonly IHttpService _serviceHttp;
        public H1ApiService(ILogger<H1ApiService> logger, IHttpService serviceHttp)
        {
            _logger = logger;
            _serviceHttp = serviceHttp;
        }
        public async Task<RefundAmountRes> Refund(RefundAmountReq request)
        {
            RefundAmountRes res = new RefundAmountRes((int)ResponseCode.SessionRefundFail, MessageCode.Message[(int)ResponseCode.SessionRefundFail]);
            try
            {
                var ReqJson = System.Text.Json.JsonSerializer.Serialize(request);
                var json = await _serviceHttp.PostAsnyc<string>(HttpService.GameProvider.H1, HttpService.API_Type.RefundAmount, ReqJson);
                res = System.Text.Json.JsonSerializer.Deserialize<RefundAmountRes>(json);
                return res;
            }
            catch (TaskCanceledException ex)
            {
                res.code = (int)ResponseCode.TimeOut;
                res.message = MessageCode.Message[(int)ResponseCode.TimeOut];
                _logger.LogError("H1 Refund Time out EX : {ex}  MSG : {Message}", ex.GetType().FullName, ex.Message);
                return res;
            }
            catch (Exception ex)
            {
                res.code = (int)ResponseCode.SessionRefundFail;
                res.message = MessageCode.Message[(int)ResponseCode.SessionRefundFail];
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                _logger.LogError("H1 Refund exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
                return res;
            }
        }
        public async Task<SettleBetRecordRes> Settle(SettleBetRecordReq request)
        {
            SettleBetRecordRes res = new SettleBetRecordRes();
            try
            {
                var ReqJson = System.Text.Json.JsonSerializer.Serialize(request);
                var json = await _serviceHttp.PostAsnyc<string>(HttpService.GameProvider.H1, HttpService.API_Type.SettleBetRecord, ReqJson);
                res = System.Text.Json.JsonSerializer.Deserialize<SettleBetRecordRes>(json);
                return res;
            }
            catch (TaskCanceledException ex)
            {
                res.code = (int)ResponseCode.TimeOut;
                res.Message = MessageCode.Message[(int)ResponseCode.TimeOut];
                _logger.LogError("H1 Settle Time out EX : {ex}  MSG : {Message}", ex.GetType().FullName, ex.Message);
                return res;
            }
            catch (Exception ex)
            {
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                _logger.LogError("H1 SettleBetRecord exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
                res.code = (int)ResponseCode.Fail;
                res.Message = MessageCode.Message[(int)ResponseCode.Fail];
                return res;
            }
        }
        public async Task<ResCodeBase> HealthCheck()
        {
            var res = new ResCodeBase();
            try
            {
                var json = await _serviceHttp.PostAsnyc<string>(HttpService.GameProvider.H1_HEALTH, HttpService.API_Type.HealthCheck);
                res = System.Text.Json.JsonSerializer.Deserialize<ResCodeBase>(json);
                return res;
            }
            catch (TaskCanceledException ex)
            {
                res.code = (int)ResponseCode.TimeOut;
                res.Message = MessageCode.Message[(int)ResponseCode.TimeOut];
                _logger.LogError("H1 Refund Time out EX : {ex}  MSG : {Message}", ex.GetType().FullName, ex.Message);
                return res;
            }
            catch (Exception ex)
            {
                res.code = (int)ResponseCode.SessionRefundFail;
                res.Message = MessageCode.Message[(int)ResponseCode.SessionRefundFail];
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                _logger.LogError("H1 HealthCheck EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
                return res;
            }
        }

        public async Task<ResCodeBase> Kick(KickUserReq KickUserData)
        {
            var res = new ResCodeBase();
            try
            {
                var ReqJson = System.Text.Json.JsonSerializer.Serialize(KickUserData);
                var json = await _serviceHttp.PostAsnyc<string>(HttpService.GameProvider.H1, HttpService.API_Type.WalletTransferOut, ReqJson);
                return res;
            }
            catch (TaskCanceledException ex)
            {
                res.code = (int)ResponseCode.TimeOut;
                res.Message = MessageCode.Message[(int)ResponseCode.TimeOut];
                _logger.LogError("H1 Kick Time out EX : {ex}  MSG : {Message}", ex.GetType().FullName, ex.Message);
                return res;
            }
            catch (Exception ex)
            {
                res.code = (int)ResponseCode.SessionRefundFail;
                res.Message = MessageCode.Message[(int)ResponseCode.SessionRefundFail];
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                _logger.LogError("H1 Kick exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
                return res;
            }
        }
    }
}
