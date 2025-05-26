using System;

namespace H1_ThirdPartyWalletAPI.Model.DB.CMD
{
    public class CMDRecordPrimaryKey
    {
        /// <summary>
        ///供查询的自增 ID
        /// </summary>
        public string ReferenceNo { get; set; }

        /// <summary>
        /// 输赢状态，WA = Win All，WH = Win Half，LA = Lose All，LH = Lose Half，D = Draw，P = Pending，P 为未结算其他为已经结算
        /// </summary>
        public string WinLoseStatus { get; set; }
        /// <summary>
        /// 下单时间,Ticks数据
        /// </summary>
        public DateTime TransDate { get; set; }
        /// <summary>
        /// 注单结算时间(如赛事结束后又重启赛事的情形,则注单结算时间以最后赛事结束时的注单结算时间为准。)
        /// </summary>
        public DateTime StateUpdateTs { get; set; }
    }
}
