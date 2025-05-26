using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThirdPartyWallet.Share.Model.Game.RGRICH.Request
{
    public class ReportHourRequest : DataRequestBase
    {
        /// <summary>
        /// 查詢小時(RFC3339格式) ex:2024-01-02T15:00:00+08:00
        /// </summary>
        public DateTime Hour { get; set; }
    }
}