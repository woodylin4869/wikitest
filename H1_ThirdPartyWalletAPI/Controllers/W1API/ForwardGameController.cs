using H1_ThirdPartyWalletAPI.Model.W1API;
using H1_ThirdPartyWalletAPI.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace H1_ThirdPartyWalletAPI.Controllers.W1API
{
    [ApiController]
    [Route("w1api/[controller]")]
    public class ForwardGameController : ControllerBase
    {
        private readonly ILogger<ForwardGameController> _logger;
        private readonly IForwardGameService _forwardGameService;

        public ForwardGameController(ILogger<ForwardGameController> logger,
             IForwardGameService forwardGameService)
        {
            _logger = logger;
            _forwardGameService = forwardGameService;
        }
        /// <summary>
        /// 建立使用者錢包並登入遊戲返回登入連結
        /// </summary>
        //[Authorize(Roles = "admin")]
        [HttpPost]
        async public Task<ForwardGame> Post(ForwardGameReq request)
        {
            return await _forwardGameService.ForwardGame(request);
        }

    }
}
