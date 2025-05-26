namespace H1_ThirdPartyWalletAPI.Model.DB.JDB.Response
{
    public class GetJdbRecordResponse : JDBRecordPrimaryKey
    {
        public string playerid { get; set; }

        public int gtype { get; set; }
    }
}
