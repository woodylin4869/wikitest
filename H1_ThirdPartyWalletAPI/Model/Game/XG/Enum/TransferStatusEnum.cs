using System.ComponentModel;

namespace H1_ThirdPartyWalletAPI.Model.Game.XG.Enum
{
    /// <summary>
    /// 交易狀態，1 = 成功，2 = 失敗，9 = 處理中
    /// </summary>
    public enum TransferStatusEnum
    {
        [Description("成功")]
        Success = 1,

        [Description("失敗")]
        Fail = 2,

        [Description("處理中")]
        Loading = 9,
    }
}
