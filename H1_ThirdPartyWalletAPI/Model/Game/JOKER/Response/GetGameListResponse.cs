using System.Collections.Generic;

namespace H1_ThirdPartyWalletAPI.Model.Game.JOKER.Response;

public class GetGameListResponse
{
    public List<Listgame> ListGames { get; set; }

    public class Listgame
    {
        /// <summary>
        /// 游戏类型：Slot、Fishing、E-Casino、Single Player、Multiplayer v.v.
        /// </summary>
        public string GameType { get; set; }
        public string GameTypeName { get; set; }
        public string Code { get; set; }
        public string GameOCode { get; set; }
        /// <summary>
        /// 游戏别名 – 用于玩游戏
        /// </summary>
        public string GameCode { get; set; }
        /// <summary>
        /// 游戏名称
        /// </summary>
        public string GameName { get; set; }
        /// <summary>
        /// 包含关键的热门游戏或新游戏
        /// </summary>
        public string Specials { get; set; }
        public string Technology { get; set; }
        public string SupportedPlatForms { get; set; }
        /// <summary>
        /// 游戏次序
        /// </summary>
        public int Order { get; set; }
        public int DefaultWidth { get; set; }
        public int DefaultHeight { get; set; }
        /// <summary>
        /// 横向模式下的游戏图像
        /// </summary>
        public string Image1 { get; set; }
        /// <summary>
        /// 纵向模式下的游戏图像
        /// </summary>
        public string Image2 { get; set; }
        public bool FreeSpin { get; set; }
        public List<Localization> Localizations { get; set; }
    }

    public class Localization
    {
        public string Language { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }
}