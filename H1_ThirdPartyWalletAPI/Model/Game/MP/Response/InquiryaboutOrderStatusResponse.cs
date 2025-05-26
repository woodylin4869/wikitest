namespace H1_ThirdPartyWalletAPI.Model.Game.MP.Response
{
    public class InquiryaboutOrderStatusResponse
    {


        public string m { get; set; }
        public int s { get; set; }
        public InquiryaboutOrderStatus d { get; set; }


        public class InquiryaboutOrderStatus
        {
            public int code { get; set; }
            /// <summary>
            /// 状态码(-1:不存在、0:成功、2:失败、3:处理中)
            /// </summary>
            public int status { get; set; }
            public string money { get; set; }
        }

    }
}
