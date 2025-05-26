namespace H1_ThirdPartyWalletAPI.Model.Config
{
    public static class Config
    {
        /// <summary>
        /// 環境變數
        /// </summary>
        public static string Environment { get; set; }

        /// <summary>
        /// 是否為開發環境
        /// </summary>
        public static bool IsDevelopment { get; set; }

        /// <summary>
        /// JWT
        /// </summary>
        public static JWT JWT { get; set; }

        /// <summary>
        /// GameAPI
        /// </summary>
        public static GameAPI GameAPI { get; set; }

        /// <summary>
        /// OneWalletAPI
        /// </summary>
        public static OneWalletAPI OneWalletAPI { get; set; }

        /// <summary>
        /// Redis
        /// </summary>
        public static Redis Redis { get; set; }

        public static CompanyToken CompanyToken { get; set; }

        /// <summary>
        /// W1 Schedule 組態設定
        /// </summary>
        public static W1ScheduleConfig W1ScheduleConfig { get; set; }

        public static RsgJackpotHistoryConfig RsgJackpotHistoryConfig { get; set; }
    }
}