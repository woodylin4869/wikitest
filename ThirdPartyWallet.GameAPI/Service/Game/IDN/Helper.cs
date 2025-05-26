namespace H1_ThirdPartyWalletAPI.Service.Game.IDN
{
    public static class Helper
    {

        /// <summary>
        /// 轉換成 Key=Value
        /// </summary>
        public static string ConvertToKeyValue<T>(T source) where T : class
        {
            var type = source.GetType();
            var properties = type.GetProperties();
            var list = properties.OrderBy(x => x.Name).Select(x => x.Name + "=" + x.GetValue(source)).ToList();
            return string.Join("&", list);
        }
    }
}