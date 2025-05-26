namespace H1_ThirdPartyWalletAPI.Model.Game.MP.Response
{
    public class GetPlatformStatusResponse
    {
        public string m { get; set; }
        public int s { get; set; }
        public PlatformStatus d { get; set; }

        public class PlatformStatus
        {
            /// <summary>
            /// 回应代号
            /// (0 正常、46 伺服器异常)
            /// (查看附录说明)
            /// </summary>
            public int code {get; set; }

            /// <summary>
            /// 平台状态
            /// (-1 查询失败、0 维护中、1 正常)
            /// </summary>
            public int status { get; set; }
        }
    }
}
