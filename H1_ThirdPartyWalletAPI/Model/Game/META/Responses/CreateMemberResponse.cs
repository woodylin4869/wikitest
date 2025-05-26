namespace H1_ThirdPartyWalletAPI.Model.Game.META.Response
{
    public class CreateMemberResponse : GetMetaDataDecryptBase
    {

        public int totalRows { get; set; }
        public Rows rows { get; set; }

        public class Rows
        {
            public string MemberAccount { get; set; }
            public bool Enable { get; set; }
        }
    }
}
