

using System;
using System.Collections.Generic;

namespace H1_ThirdPartyWalletAPI.Model.Game.FC.Response
{
    public class GetRecordListResponse : FCBaseStatusRespones
    {
        public List<Record> Records { get; set; }
    }

    public class Record
    {
        /// <summary>
        /// 下注点数
        /// </summary>
        public decimal bet { get; set; }

        /// <summary>
        /// 赢分点数
        /// </summary>
        public decimal prize { get; set; }
        /// <summary>
        /// 输赢点数（含下注）
        /// </summary>
        public decimal winlose { get; set; }

        /// <summary>
        /// 下注前点数
        /// </summary>
        public decimal before { get; set; }

        /// <summary>
        /// 下注后点数
        /// </summary>
        public decimal after { get; set; }
        /// <summary>
        /// 彩金抽水 (支持到小数第六位
        /// </summary>
        public decimal jptax { get; set; }
        /// <summary>
        /// 彩金点数
        /// </summary>
        public decimal jppoints { get; set; }

        /// <summary>
        /// 游戏记录编号（唯一码），长度 24 码
        /// </summary>
        public string recordID { get; set; }
        /// <summary>
        /// 玩家账号
        /// </summary>
        public string account { get; set; }
        /// <summary>
        /// 游戏编号
        /// </summary>
        public int gameID { get; set; }
        /// <summary>
        /// 游戏类型（请参照游戏类别对应表）
        /// </summary>
        public int gametype { get; set; }
        /// <summary>
        /// 彩金模式
        /// </summary>
        public int jpmode { get; set; }
        /// <summary>
        /// 下注时间
        /// </summary>
        public DateTime bdate { get; set; }
        /// <summary>
        /// 是否购买免费游戏
        /// </summary>
        public bool isBuyFeature { get; set; }
        /// <summary>
        /// 匯總帳時間
        /// </summary>
        public DateTime report_time { get; set; }
        public DateTime partition_time { get; set; }
    }
}
