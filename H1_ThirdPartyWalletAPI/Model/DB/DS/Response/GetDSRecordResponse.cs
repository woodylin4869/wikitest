namespace H1_ThirdPartyWalletAPI.Model.DB.DS.Response
{
    public class GetDSRecordResponse : DSRecordPrimaryKey
    {
        public string game_id { get; set; }
        public string game_serial { get; set;}
    }
}
