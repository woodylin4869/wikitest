using H1_ThirdPartyWalletAPI.Code;
using H1_ThirdPartyWalletAPI.Model.W1API;
using H1_ThirdPartyWalletAPI.Model;
using H1_ThirdPartyWalletAPI.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System;
using H1_ThirdPartyWalletAPI.Service.Common.DB.Betlogs;
using H1_ThirdPartyWalletAPI.Service.Game;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using System.ComponentModel.DataAnnotations;
using H1_ThirdPartyWalletAPI.Attributes;

namespace H1_ThirdPartyWalletAPI.Controllers.W1API
{
    /// <summary>
    /// 測試用
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [ApiAllow(EnvType.DEV | EnvType.UAT | EnvType.Local)]
    public class TestController : ControllerBase
    {
        private readonly ILogger<TestController> _logger;

        public TestController(ILogger<TestController> logger)
        {
            _logger = logger;
        }
    }
}