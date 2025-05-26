using System.ComponentModel;

namespace H1_ThirdPartyWalletAPI.Model.Game.XG.Enum
{
    /// <summary>
    /// 轉帳類型，1 = 轉出，2 = 轉入
    /// </summary>
    public enum TransferTypeEnum
    {
        [Description("轉出")]
        Withdraw = 1,

        [Description("轉入")]
        Deposit = 2,
    }
}
