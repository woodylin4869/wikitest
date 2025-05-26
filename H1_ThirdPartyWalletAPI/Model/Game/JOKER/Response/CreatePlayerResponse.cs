namespace H1_ThirdPartyWalletAPI.Model.Game.JOKER.Response;

public class CreatePlayerResponse
{
    /// <summary>
    /// 确定 – 用户已经存在于提供者的系统中(HttpStatusCode = 200)
    /// 已创建 – 已在提供者的系统上成功创建用户(HttpStatusCode = 201)
    /// </summary>
    public string Status { get; set; }
    public CreatePlayerResponseData Data { get; set; }

    public class CreatePlayerResponseData
    {
        public string Username { get; set; }
        public string Status { get; set; }
    }
}