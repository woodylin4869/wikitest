using System.ComponentModel;

namespace H1_ThirdPartyWalletAPI.Model.Game.GR.Enum
{
    public enum OrderStateEnum
    {
        [Description("訂單不存在")]
        NotExists = 0,

        [Description("訂單處理中")]
        Processing = 1,

        [Description("訂單處理成功")]
        Success = 2,

        [Description("訂單處理失敗")]
        Fail = 3,
    }
}
