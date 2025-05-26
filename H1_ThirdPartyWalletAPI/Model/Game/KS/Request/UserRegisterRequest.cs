namespace H1_ThirdPartyWalletAPI.Model.Game.KS.Request
{
    public class UserRegisterRequest
    {
        /// <summary>
        /// 数字字母下划线，2~30位 示例：ceshi008
        /// </summary>
        public string UserName { get; set; }


        /// <summary>
        /// 5~16位 示例：a123456
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// 指定会员的币种，如果留空则为商户的开户默认币种。 请注意，会员币种为注册时设定，之后不可再修改。币种代码参见 附录4:货币代码
        /// </summary>
        public string Currency { get; set; }

    }
}
