using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace H1_ThirdPartyWalletAPI.Service.Game.JDB.API.Utility
{
    public static class TimeStampHelper
    {
        public static int DateTimeToUnixTimeStamp(DateTime? dateTime)
        {
            if (dateTime == null)
            {
                return 0;
            }
            else
            {
                return (Int32)dateTime.Value.ToUniversalTime().Subtract(new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc)).TotalSeconds;
            }
        }

    }
}
