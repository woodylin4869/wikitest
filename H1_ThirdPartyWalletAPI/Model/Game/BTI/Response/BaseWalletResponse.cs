using System.Xml.Serialization;

namespace H1_ThirdPartyWalletAPI.Model.Game.BTI.Response;

/// <summary>
/// 回傳 XML共同欄位
/// </summary>
[XmlRoot(ElementName = "MerchantResponse", Namespace = "http://networkpot.com/")]
public class BaseWalletResponse
{
    /// <summary>
    /// NoError （无问题）
    /// AuthenticationFailed （代理商登入无法确认）
    /// MerchantIsFrozen （代理商已经停用）
    /// MerchantNotActive （代理商已无活跃）
    /// Exception （其他错误）
    /// TransactionCodeNotFound （转账码不存在）
    /// CustomerNotFound <自己測到的..帳號不存在>
    /// 或基本逻辑是当没有异常发生时（错误代码<> 0）事务失败
    /// </summary>
    [XmlElement(ElementName = "ErrorCode")]
    public string ErrorCode { get; set; }

    /// <summary>
    /// BTi 玩家的账号的 ID 
    /// </summary>
    [XmlElement(ElementName = "CustomerID")]
    public long CustomerID { get; set; }

    /// <summary>
    /// 玩家的令牌 AuthToken
    /// </summary>
    [XmlElement(ElementName = "AuthToken")]
    public string AuthToken { get; set; }

    /// <summary>
    /// 玩家转账后的余额(如果没有问题，余额会变)
    /// </summary>
    [XmlElement(ElementName = "Balance")]
    public decimal Balance { get; set; }

    // 有輸出但文件未使用
    //[XmlElement(ElementName = "OpenBetsBalance")]
    //public decimal OpenBetsBalance { get; set; }

    /// <summary>
    /// BTi 转账系统的 ID
    /// </summary>
    [XmlElement(ElementName = "TransactionID")]
    public long TransactionID { get; set; }
}
