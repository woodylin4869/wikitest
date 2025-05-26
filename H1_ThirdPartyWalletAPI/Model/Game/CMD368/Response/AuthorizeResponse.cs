using System.Xml.Serialization;

namespace H1_ThirdPartyWalletAPI.Model.Game.CMD368.Response;


// 注意: 產生的程式碼可能至少需要 .NET Framework 4.5 或 .NET Core/Standard 2.0。
/// <remarks/>
/*[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
[System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]*/

[XmlRoot(ElementName = "authenticate")]
public class AuthorizeResponse
{
    [XmlElement(ElementName = "member_id")]
    public string member_id { get; set; }

    [XmlElement(ElementName = "status_code")]
    public int status_code { get; set; }

    [XmlElement(ElementName = "message")]
    public string message { get; set; }
}
