using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace H1_ThirdPartyWalletAPI.Model.Game.JDB.Enum
{
    public enum BedRequestEnum
    {
        TransferIdNotFound = 9015,
        TransferIdDuplicate = 9011,
        MemberNotExist = 7501,
        BalanceIsZero = 6002,
        InsufficientBalance = 6006,
    }
    public enum DbConnectionEnum
    {
        PlatFormMain,
        JDB
    }
    public enum GameType
    {
        Slot = 0,
        Fish = 7,
        Arcade = 9,
        Lottery = 12,
        Poker = 18
    }
}
