using H1_ThirdPartyWalletAPI.Service.Game.DS.JsonConverter;
using H1_ThirdPartyWalletAPI.Service.Game.DS.Utility;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace H1_ThirdPartyWalletAPI.Controllers.Game.DS
{
    [Route("api/[controller]")]
    [ApiController]
    public class DSController : ControllerBase
    {
        public string AESKey => "gyAGfYHxRMt4uTk2aCZeDhzicIBSF75N";
        public string MD5_key => "C6pQIxaiKZhVNRoErWOqdPnywmS85U1J";
        public string channel => "59237107";

        //public string AESKey => "1ac5d0205ab0e586ae738c65dc82ceb48318b0cd52a3b27a14f891e3706f2a1c";
        //public string sign => "ZFdeUewhnjSxC2ImsrmLFnOdHlbzQ7a4CQ9oWAF/bNY=";
        //public string channel => "59237107";


        [Route("MD5Encrypt")]
        public Task<md5result> MD5Encrypt(string content)
        {
            var helper = new MD5Helper();
            var result = helper.Encrypt(string.Format("{0}{1}", content, MD5_key));
            return Task.FromResult(new md5result { channel = channel,  data = content, sign = result });
        }

        [Route("AESEncrypt")]
        public Task<string> AesEncrypt(string json)
        {
            var helper = new AESHelper();
            var result = helper.Encrypt(json, AESKey);
            return Task.FromResult(result);
        }
        [Route("AESDecrypt")]
        public Task<string> AesDecrypt(string chipherText)
        {
            var helper = new AESHelper();
            var result = helper.Decrypt(chipherText, AESKey);
            return Task.FromResult(result);
        }
        [Route("DateTime")]
        public Task<string> DateTimeSerialize(DateTime date)
        {
            var result = JsonSerializer.Serialize(date, new JsonSerializerOptions { Converters = { new SerializeDateTimeConverter() } });
            return Task.FromResult(result);
        }
    }
    public class md5result {
        public string channel { get; set; }
        public string data {get;set;}
        public string sign { get; set; }
    }
}
