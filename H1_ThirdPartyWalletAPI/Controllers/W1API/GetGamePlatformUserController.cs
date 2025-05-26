using H1_ThirdPartyWalletAPI.Code;
using H1_ThirdPartyWalletAPI.Model;
using H1_ThirdPartyWalletAPI.Model.W1API;
using H1_ThirdPartyWalletAPI.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace H1_ThirdPartyWalletAPI.Controllers.W1API
{
    [Route("api/[controller]")]
    [ApiController]
    public class GetGamePlatformUserController : ControllerBase
    {
        private readonly ILogger<GetGamePlatformUserController> _logger;
        private readonly IGamePlatformUserService _service;

        public GetGamePlatformUserController(ILogger<GetGamePlatformUserController> logger, IGamePlatformUserService service)
        {
            _logger = logger;
            _service = service;
        }

        /// <summary>
        /// 取得會員遊戲商帳號
        /// </summary>
        /// <param name="clubId"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<GetGamePlatformUserResponse> GetGamePlatformUser([FromQuery] string clubId)
        {
            var res = new GetGamePlatformUserResponse();
            try
            {
                if (string.IsNullOrWhiteSpace(clubId))
                {
                    throw new ArgumentException($"'{nameof(clubId)}' 不得為 Null 或空白字元。", nameof(clubId));
                }

                var result = await _service.GetGamePlatformUserAsync(clubId);
                if (result.Any())
                    res.Datas = result.Select(r => new GetGamePlatformUserResponse.GamePlatformUser()
                    {
                        club_id = r.club_id,
                        platform = r.game_platform,
                        platform_id = r.game_user_id,
                    }).ToArray();

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{action} {message}", nameof(GetGamePlatformUser), ex.Message);
                res.code = (int)ResponseCode.Fail;
                res.Message = ex.Message;
            }

            return res;
        }
    }
}
