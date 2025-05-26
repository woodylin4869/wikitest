using System;
using System.Collections.Generic;

namespace H1_ThirdPartyWalletAPI.Model.Game.MG.Response
{
    /// <summary>
    /// Get game details list 获取游戏详情列表
    /// </summary>
    public class GetGameListResponse
    {
        public List<ProductInfo> Data { get; set; }
    }

    public class ProductInfo
    {
        /// <summary>
        /// 游戏名称
        /// </summary>
        public string gameName { get; set; }
        /// <summary>
        /// 游戏代码
        /// </summary>
        public string gameCode { get; set; }
        public List<Translatedgamename> translatedGameName { get; set; }
        /// <summary>
        /// 渠道代码
        /// </summary>
        public string channelCode { get; set; }
        /// <summary>
        /// 渠道名称
        /// </summary>
        public string channelName { get; set; }
        /// <summary>
        /// 类别代码
        /// </summary>
        public string gameCategoryCode { get; set; }
        /// <summary>
        /// 类别名称
        /// </summary>
        public string gameCategoryName { get; set; }
        /// <summary>
        /// 子类别代码
        /// </summary>
        public string gameSubcategoryCode { get; set; }
        /// <summary>
        /// 子类别名称
        /// </summary>
        public string gameSubcategoryName { get; set; }
        /// <summary>
        /// 游戏发布日期 UTC
        /// </summary>
        public DateTime? releaseDateUTC { get; set; }
        /// <summary>
        /// 游戏平台（已弃用）
        /// </summary>
        public string platform { get; set; }
        public List<Platform> platforms { get; set; }
        /// <summary>
        /// 指示游戏是否有演示模式
        /// </summary>
        public bool isDemoEnabled { get; set; }
    }

    public class Translatedgamename
    {
        public string code { get; set; }
        public string value { get; set; }
    }

    public class Platform
    {
        public string platform { get; set; }
    }

}
