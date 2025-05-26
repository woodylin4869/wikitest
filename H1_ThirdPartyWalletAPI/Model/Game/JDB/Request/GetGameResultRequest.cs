using System;
using System.Collections.Generic;
using System.Text;

namespace H1_ThirdPartyWalletAPI.Model.Game.JDB.Request
{
    public class GetGameResultRequest : RequestBaseModel
    {
        public override int Action => 54;
        public string uid { get; set; }
        public string lang { get; set; }

        public int gType { get; set; }

        // JDB預計2023-04-25刪除，W1超前部屬移除
        //public long seqNo { get; set; }

        /// <summary>
        /// 集成遊戲商提供的遊戲序號
        /// </summary>
        public string historyId { get; set; }

        public int showUid { get; set; }
    }
}