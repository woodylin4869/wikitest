using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThirdPartyWallet.Share.Model.Game.RGRICH.Response
{
    public class RechargeOrWithdrawRecordResponse
    {
        /// <summary>
        /// 廠商流水號
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 玩家id
        /// </summary>
        public int Uid { get; set; }

        /// <summary>
        /// 玩家用戶名
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// 類型(0:充值 , 1:提現)
        /// </summary>
        public int Type { get; set; }

        /// <summary>
        /// 變動前餘額
        /// </summary>
        public decimal Money_before { get; set; }

        /// <summary>
        /// 變動餘額(充值為正數，提現為負數)
        /// </summary>
        public decimal Money_add { get; set; }

        /// <summary>
        /// 變動後餘額
        /// </summary>
        public decimal Money_after { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 創建時間（RFC3339格式） e.g. 2020-05-22T14:40:00+08:00
        /// </summary>
        public DateTime Created_at { get; set; }
    }
}