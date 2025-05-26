namespace H1_ThirdPartyWalletAPI.Model.Game.PME.Response
{
    public class BaseResponse
    {
        /// <summary>
        /// 成功:true,
        /// 失败:false
        /// </summary>
        public string status { get; set; }

        /// <summary>
        /// 成功:succeed
        /// 失败:错误信息
        /// </summary>
        public string data { get; set; }


        public virtual bool IsSuccess { get
            {
                return status == "true";
            } 
        }
    }
}
