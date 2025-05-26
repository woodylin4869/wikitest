namespace H1_ThirdPartyWalletAPI.Model.Game.PG.Request
{
    /// <summary>
    /// 取得遊戲連結
    /// </summary>
    public class GetGameUrlRequest
    {
		/// <summary>
		/// 游戏启动模式
        /// 1 真钱模式
        /// 3 锦标赛模式
        /// </summary>
        public int btt { get; set; }
        /// <summary>
        /// 运营商独有的身份识别
        /// </summary>
        public string ot { get; set; }
        /// <summary>
        /// 运营商系统生成的令牌
        ///	注：
        ///	• 最多 200 字符
        ///	• 请使用 UrlEncode 对值进行编码，以避免意外错误 
        /// </summary>
        public string ops { get; set; }
        /// <summary>
        /// 游戏显示语言4
        /// 默认：英文
        /// </summary>
        public string l { get; set; }
        /// <summary>
        /// 运营商运行健康提醒的时间，以秒为单位（现实查核5）
        /// </summary>
        //public int te { get; set; }
        /// <summary>
        /// 健康提醒的间隔时间，以秒为单位（现实查核6）
        /// </summary>
        //public int ri { get; set; }
        /// <summary>
        /// 运营商的自定义参数，PG API 将在验证运营商玩家会话时包含参数值。
        /// 注：请使用 UrlEncode 对值进行编码，以避免意外错误
        /// </summary>
        //public string op { get; set; }
        /// <summary>
        /// 游戏退出 URL
        ///	默认：重定向到 PG 退出页面
        ///	注：
        ///	• 在试玩模式下，该数值将用于重定向到真实游戏的提示
        ///	• 分配数值到 PGGameCloseUrl以关闭游戏窗口
        ///	• 请使用 UrlEncode 对值进行编码，以避免意外错误
        /// </summary>
        public string f { get; set; }
        /// <summary>
        /// 该 URL 将在真实游戏期间用于重定向
        ///	默认：重定向到 PG 大厅
        ///	注：
        ///	• 仅限于试玩游戏
        ///	• 请使用 UrlEncode 对值进行编码，以避免意外错误
        /// </summary>
        //public string real_url { get; set; }
        /// <summary>
        /// 缓存玩家令牌
        ///	0: 将为 PG 游戏存储玩家令牌（全球）
        ///	1:将分别为每个游戏存储玩家令牌
        ///	2:登录以让每次登录都始终调用verifySession。不会储存玩家令牌。 
        /// </summary>
        //public string cached_t { get; set; }
        /// <summary>
        /// 启动游戏时进行设备兼容性检查7
        /// 0: 普通模式（默认）
        /// 1: 跳过兼容性检查
        /// </summary>
        //public int iwk { get; set; }
        /// <summary>
        /// 启动游戏时检查屏幕方向8
        /// 0: 跳过屏幕方向检查
        /// 1: 普通模式（默认）
        /// </summary>
        //public int oc { get; set; }
    }
}