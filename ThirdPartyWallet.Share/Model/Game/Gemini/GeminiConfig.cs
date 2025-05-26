using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThirdPartyWallet.Share.Model.Game.Gemini
{
    public  class GeminiConfig
    {
        public const string ConfigKey = "GeminiConfig";
        public string Gemini_URL { get; set; }
        public string Gemini_secrect { get; set; }
        public string Gemini_MerchantCode { get; set; }
    }
}
