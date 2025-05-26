namespace H1_ThirdPartyWalletAPI.Model.Game.META.Response
{
    public class GetGameListResponse : GetMetaDataDecryptBase
    {
        public int totalRows { get; set; }
        public Row[] rows { get; set; }

        public class Row
        {
            public int gameTypeId { get; set; }
            public string gameTypeName { get; set; }
            public int gameId { get; set; }
            public string gameName { get; set; }
            public int tableId { get; set; }
            public string tableName { get; set; }
        }




    }
}
