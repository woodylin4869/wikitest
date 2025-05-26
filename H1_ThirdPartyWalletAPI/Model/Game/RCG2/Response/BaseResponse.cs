namespace H1_ThirdPartyWalletAPI.Model.Game.RCG2.Response
{
    /// <summary>
    /// 共同回傳欄位
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class BaseResponse<T>
    {
        public int msgId { get; set; }
        public string message { get; set; }
        public int timestamp { get; set; }
        public T data { get; set; }
    }
}
