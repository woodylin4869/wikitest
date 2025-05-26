namespace H1_ThirdPartyWalletAPI.Model.DataModel
{
    /// <summary>
    /// 系統參數
    /// </summary>
    public class t_system_parameter
    {
        /// <summary>
        /// key
        /// </summary>
        public string key { get; set; }
        /// <summary>
        /// value
        /// </summary>
        public string value { get; set; }
        /// <summary>
        /// 名稱
        /// </summary>
        public string name { get; set; }
        /// <summary>
        /// 描述
        /// </summary>
        public string description { get; set; }
        /// <summary>
        /// 最小值
        /// </summary>
        public string min_value { get; set; }
        /// <summary>
        /// 最大值
        /// </summary>
        public string max_value { get; set; }
    }
}