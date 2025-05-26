using System.ComponentModel;

namespace H1_ThirdPartyWalletAPI.Model.Game.RSG.Enum
{
    public enum ErrorCodeEnum
    {
		/// <summary>
        /// 正常。
        /// </summary>
        [Description("正常")]
        OK = 0,
        /// <summary>
        /// 執行失敗。
        /// </summary>
        [Description("執行失敗")]
        ExecuteFailed = 1001,
        /// <summary>
        /// 系統維護中。
        /// </summary>
        [Description("系統維護中")]
        SystemIsInMaintenance = 1002,
        /// <summary>
        /// 無效的參數。
        /// </summary>
        [Description("無效的參數")]
        IllegalArguments = 2001,
        /// <summary>
        /// 解密失敗。
        /// </summary>
        [Description("解密失敗")]
        InvalidDecrypt = 2002,
        /// <summary>
        /// 餘額不足。
        /// </summary>
        [Description("餘額不足")]
        BalanceIsNotEnough = 3005,
        /// <summary>
        /// 找不到交易結果。
        /// </summary>
        [Description("找不到交易結果")]
        TransactionIsNotFound = 3006,
        /// <summary>
        /// 此玩家帳戶不存在。
        /// </summary>
        [Description("此玩家帳戶不存在")]
        ThePlayerIsCurrencyDoesNotExist = 3008,
        /// <summary>
        /// 此玩家帳戶已存在。
        /// </summary>
        [Description("此玩家帳戶已存在")]
        ThePlayerIsCurrencyAlreadyExists = 3010,
        /// <summary>
        /// 系統商權限不足。
        /// </summary>
        [Description("系統商權限不足")]
        DenyPermissionForSystem = 3011,
        /// <summary>
        /// 遊戲權限不足。
        /// </summary>
        [Description("遊戲權限不足")]
        DenyPermissionForGame = 3012,
        /// <summary>
        /// 重複的 TransactionID。
        /// </summary>
        [Description("重複的 TransactionID")]
        DuplicateTransactionID = 3014,
        /// <summary>
        /// 時間不在允許的範圍內。
        /// </summary>
        [Description("時間不在允許的範圍內")]
        TimeIsNotInTheAllowedRange = 3015,
        /// <summary>
        /// 拒絕提點，玩家正在遊戲中。
        /// </summary>
        [Description("拒絕提點，玩家正在遊戲中")]
        DenyWithdrawPlayerIsInGaming = 3016,
	}
}