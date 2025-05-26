
using H1_ThirdPartyWalletAPI.Model.Game.JDB.Request;
using H1_ThirdPartyWalletAPI.Model.Game.JDB.Response;
using H1_ThirdPartyWalletAPI.Service.Game.JDB.API.Interface;
using H1_ThirdPartyWalletAPI.Service.Game.JDB.JsonConverter;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Text.Json;
using System.Threading.Tasks;
using H1_ThirdPartyWalletAPI.Service.Game.JDB.API.Utility;

namespace H1_ThirdPartyWalletAPI.Controllers.Game.JDB
{
    /// <summary>
    /// JDB Controller
    /// </summary>
    [Route("jdb/api")]
    [ApiController]
    public class JDBController : ControllerBase
    {
        private readonly IJDBApiService service;

        public JsonSerializerOptions jsonSerializerOptions { get; set; }

        public JDBController(IJDBApiService service)
        {
            jsonSerializerOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                Converters = {
                new DeserializeDateTimeConverter()
               }
            };
            this.service = service;
        }
        [HttpPost]
        [Route("Encrypt")]
        public async Task<string> GetCryptContent([FromForm] Request request)
        {
            var key = "e9ae88dbeb8f43f2";
            var iv = "6caeb5fc6c20591f";
            var aes = new AESHelper();
            var result = aes.StartEncode(JsonSerializer.Serialize(request, jsonSerializerOptions), key, iv);
            return await Task.FromResult(result);
        }
        [HttpPost]
        [Route("Decrypt")]
        public async Task<string> GetdecryptContent([FromForm] string ciphertext)
        {
            var key = "e9ae88dbeb8f43f2";
            var iv = "6caeb5fc6c20591f";
            var aes = new AESHelper();
            var result = aes.StartDecode(ciphertext, key , iv);
            return await Task.FromResult(result);
        }
        [HttpPost]
        [Route("CreateUser")]
        public async Task<ResponseBaseModel> CreateUser(CreatePlayerRequest request)
        {
            //var request = new CreatePlayerRequest
            //{
            //    Action = 12,
            //    Ts = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            //    Name = "TEST00010",
            //    Uid = "1",
            //    Credit_allocated = 0
            //};
            
           return await service.Action12_CreatePlayer(request);

        }
        [HttpPost]
        [Route("GetGameList")]
        public async Task<GetGameListResponse> GetGameList(GetGameListRequest request)
        {
            //var request = new CreatePlayerRequest
            //{
            //    Action = 12,
            //    Ts = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            //    Name = "TEST00010",
            //    Uid = "1",
            //    Credit_allocated = 0
            //};

            return await service.Action49_GetGameList(request);

        }

    }
    public class Request
    {
        public string Content { get; set; }
    }
}
