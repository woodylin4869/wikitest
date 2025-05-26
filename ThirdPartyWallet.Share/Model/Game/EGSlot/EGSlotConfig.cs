using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThirdPartyWallet.Share.Model.Game.EGSlot
{
    public  class EGSlotConfig
    {
        public const string ConfigKey = "EGSlotConfig";
        public string EGSlot_URL { get; set; }
        public string EGSlot_HashKey { get; set; }
        public string EGSlot_MerchantCode { get; set; }
        public string EGSlot_Platform { get; set; }
    }
}
