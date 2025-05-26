using System;

namespace H1_ThirdPartyWalletAPI.Model.Game.JDB.Response
{
    public class CommonBetRecord : JDBBetRecordBase
    {
        public decimal gambleBet { get; set; }

        // slot
        public decimal jackpot { get; set; }

        public decimal jackpotContribute { get; set; }
        public int hasFreegame { get; set; }
        public int hasGamble { get; set; }
        public int systemTakeWin { get; set; }

        //魚機
        public int roomType { get; set; }
        // 街機
        public int hasBonusGame { get; set; }

        //棋牌
        public decimal tax { get; set; }

        public decimal validBet { get; set; }
    }
}