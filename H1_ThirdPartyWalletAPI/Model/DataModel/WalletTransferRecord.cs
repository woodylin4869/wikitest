using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace H1_ThirdPartyWalletAPI.Model.DataModel
{
    public class WalletTransferRecord
    {
        /// <summary>
        /// guid
        /// </summary>
        public Guid id { get; set; }
        public string source { get; set; }
        public string target { get; set; }
        public DateTime create_datetime { get; set; }
        public DateTime success_datetime { get; set; }
        public decimal before_balance { get; set; }
        public decimal after_balance { get; set; }
        public string status { get; set; }
        public string Club_id { get; set; }
        public decimal amount { get; set; }
        public string Franchiser_id { get; set; }
        public string type { get; set; }

        public enum TransferStatus
        {
            init,
            success,
            fail,
            pending,
        }
        public enum TransferType
        {
            IN,
            OUT,
            RCG,
        }
    }

}
