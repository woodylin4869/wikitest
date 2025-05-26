using System.ComponentModel;

namespace H1_ThirdPartyWalletAPI.Model.Game.KS.Enum
{

    /// <summary>
    /// 请注意：发生异常提示表示程序运行失败，但是操作有可能是成功的，如果设计到金额操作（转账等），
    /// 请稍后使用对账的接口进行查询确认该笔订单的状态。
    /// </summary>
    public enum ErrorCodeEnum
    {
        [Description("正常")]
        success = 1,

        [Description("用户不存在")]
        NOUSER,

        [Description("用户名不符合规则,用户名格式为数字+字母+下划线的组合，2~16位")]
        BADNAME,

        [Description("密码不符合规则，密码长度位5~16位")]
        BADPASSWORD,

        [Description("用户名已经存在")]
        EXISTSUSER,

        [Description("金额错误, 金额支持两位小数。")]
        BADMONEY,

        [Description("订单号错误（不符合规则或者不存在）")]
        NOORDER,

        [Description("订单号已经存在，转账订单号为全局唯一")]
        EXISTSORDER,

        [Description("未指定转账动作，转账动作必须为 IN 或者 OUT")]
        TRANSFER_NO_ACTION,

        [Description("IP未授权")]
        IP,

        [Description("用户被锁定，禁止登录")]
        USERLOCK,

        [Description("余额不足")]
        NOBALANCE,

        [Description("平台额度不足（适用于买分商户)")]
        NOCREDIT,

        [Description("API密钥错误")]
        Authorization,

        [Description("发生错误")]
        Faild,

        [Description("未配置域名（请与客服联系）")]
        DOMAIN,

        [Description("内容错误（提交的参数不符合规则）")]
        CONTENT,

        [Description("签名错误（适用于单一钱包的通信错误提示）")]
        Sign,

        [Description("不支持该操作")]
        NOSUPPORT,

        [Description("超时请求")]
        TIMEOUT,

        [Description("状态错误(商户被冻结）")]
        STATUS,

        [Description("商户信息配置错误（请联系客服处理）")]
        CONFIGERROR,

        [Description("查询日期错误,日期超过了1天或者结束时间大于开始时间")]
        DATEEROOR,

        [Description("查询使用的订单号不存在")]
        ORDER_NOTFOUND,

        [Description("订单正在处理中")]
        PROCCESSING,

        [Description("系统维护中")]
        MAINTENANCE
    }
}