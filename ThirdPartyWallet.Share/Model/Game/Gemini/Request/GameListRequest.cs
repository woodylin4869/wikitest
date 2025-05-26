using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThirdPartyWallet.Share.Model.Game.Gemini.Request
{
    public class GameListRequest
    {
        /// <summary>
        /// 流水號
        /// </summary>
        public string seq { get;set; }

        /// <summary>
        /// 產品ID (商戶ID)
        /// 請洽商務取得您的 ProductId (PID),格式為 GMM開頭接四位數字,例如:GMM0000
        /// </summary>
        public string product_id { get; set; }
    }
}
