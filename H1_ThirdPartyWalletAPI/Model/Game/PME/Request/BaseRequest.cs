using System;

namespace H1_ThirdPartyWalletAPI.Model.Game.PME.Request
{
    public class BaseRequest
    {
        /// <summary>
        /// 商户ID(平台提供)
        /// </summary>
        public long merchant { get; set; }

        /// <summary>
        /// Unix时间戳
        /// 精确到秒
        /// </summary>
        public long time { get; set; }
    }
}
