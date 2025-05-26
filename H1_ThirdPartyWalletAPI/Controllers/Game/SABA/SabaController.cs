using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Primitives;
using System.Net;
using H1_ThirdPartyWalletAPI.Model.OneWalletGame;
using H1_ThirdPartyWalletAPI.Service;
using H1_ThirdPartyWalletAPI.Model.Game;
using H1_ThirdPartyWalletAPI.Service.Game;

namespace H1_ThirdPartyWalletAPI.Controllers.Game.SABA
{
    [Route("[controller]")]
    public class SabaController : ControllerBase
    {
        private readonly ISaba2ApiService _apiService;

        public SabaController(ISaba2ApiService apiService)
        {
            _apiService = apiService;
        }
        /*
        [HttpPost]
        [Route("getbalance")]
        public async Task<ResponseGetBalance> GetBalance(RequestBaseMessage<RequestGetBalance> request)
        {
            ResponseGetBalance res = new ResponseGetBalance();
            try
            {
                return res;
            }
            catch (Exception)
            {
                return res;
            }
        }

        [HttpPost]
        [Route("placebet")]
        public async Task<ResponsePlaceBet> PlaceBet(RequestBaseMessage<RequestPlaceBet> request)
        {
            ResponsePlaceBet res = new ResponsePlaceBet();
            try
            {
                return res;
            }
            catch (Exception)
            {
                return res;
            }
        }

        [HttpPost]
        [Route("confirmbet")]
        public async Task<ResponseConfirmBet> ConfirmBet(RequestBaseMessage<RequestConfirmBet> request)
        {
            ResponseConfirmBet res = new ResponseConfirmBet();
            try
            {
                return res;
            }
            catch (Exception)
            {
                return res;
            }
        }

        [HttpPost]
        [Route("cancelbet")]
        public async Task<ResponseCancelBet> CancelBet(RequestBaseMessage<RequestCancelBet> request)
        {
            ResponseCancelBet res = new ResponseCancelBet();
            try
            {
                return res;
            }
            catch (Exception)
            {
                return res;
            }
        }

        [HttpPost]
        [Route("settle")]
        public async Task<ResponseSettle> Settle(RequestBaseMessage<RequestSettle> request)
        {
            ResponseSettle res = new ResponseSettle();
            try
            {
                return res;
            }
            catch (Exception)
            {
                return res;
            }
        }

        [HttpPost]
        [Route("resettle")]
        public async Task<ResponseReSettle> ReSettle(RequestBaseMessage<RequestReSettle> request)
        {
            ResponseReSettle res = new ResponseReSettle();
            try
            {
                return res;
            }
            catch (Exception)
            {
                return res;
            }
        }

        [HttpPost]
        [Route("unsettle")]
        public async Task<ResponseUnSettle> UnSettle(RequestBaseMessage<RequestUnSettle> request)
        {
            ResponseUnSettle res = new ResponseUnSettle();
            try
            {
                return res;
            }
            catch (Exception)
            {
                return res;
            }
        }

        [HttpPost]
        [Route("placebetparlay")]
        public async Task<ResponsePlaceBetParlay> PlaceBetParlay(RequestBaseMessage<RequestPlaceBetParlay> request)
        {
            ResponsePlaceBetParlay res = new ResponsePlaceBetParlay();
            try
            {
                return res;
            }
            catch (Exception)
            {
                return res;
            }
        }

        [HttpPost]
        [Route("confirmbetparlay")]
        public async Task<ResponseConfirmBetParlay> ConfirmBetParlay(RequestBaseMessage<RequestConfirmBetParlay> request)
        {
            ResponseConfirmBetParlay res = new ResponseConfirmBetParlay();
            try
            {
                return res;
            }
            catch (Exception)
            {
                return res;
            }
        }

        [HttpPost]
        [Route("placebet3rd")]
        public async Task<ResponsePlaceBet3rd> PlaceBet3rd(RequestBaseMessage<RequestPlaceBet3rd> request)
        {
            ResponsePlaceBet3rd res = new ResponsePlaceBet3rd();
            try
            {
                return res;
            }
            catch (Exception)
            {
                return res;
            }
        }

        [HttpPost]
        [Route("confirmbet3rd")]
        public async Task<ResponseConfirmBet3rd> ConfirmBet3rd(RequestBaseMessage<RequestConfirmBet3rd> request)
        {
            ResponseConfirmBet3rd res = new ResponseConfirmBet3rd();
            try
            {
                return res;
            }
            catch (Exception)
            {
                return res;
            }
        }

        [HttpPost]
        [Route("cashout")]
        public async Task<ResponseCashOut> CashOut(RequestBaseMessage<RequestCashOut> request)
        {
            ResponseCashOut res = new ResponseCashOut();
            try
            {
                return res;
            }
            catch (Exception)
            {
                return res;
            }
        }

        [HttpPost]
        [Route("cashoutresettle")]
        public async Task<ResponseCashOutReSettle> CashOutReSettle(RequestBaseMessage<RequestCashOutReSettle> request)
        {
            ResponseCashOutReSettle res = new ResponseCashOutReSettle();
            try
            {
                return res;
            }
            catch (Exception)
            {
                return res;
            }
        }

        [HttpPost]
        [Route("updatebet")]
        public async Task<ResponseUpdateBet> UpdateBet(RequestBaseMessage<RequestUpdateBet> request)
        {
            ResponseUpdateBet res = new ResponseUpdateBet();
            try
            {
                return res;
            }
            catch (Exception)
            {
                return res;
            }
        }

        [HttpPost]
        [Route("healthcheck")]
        public async Task<ResponseHealthCheck> HealthCheck(RequestBaseMessage<RequestHealthCheck> request)
        {
            ResponseHealthCheck res = new ResponseHealthCheck();
            try
            {
                return res;
            }
            catch (Exception)
            {
                return res;
            }
        }
        */

        [HttpPost]
        [Route("GetBetSettingLimit")]
        public Task<SABA_GetBetSettingLimit_Res> GetBetSettingLimit(SABA_GetBetSettingLimit request)
        {
            return _apiService.GetBetSettingLimit(request);
        }
    }

}
