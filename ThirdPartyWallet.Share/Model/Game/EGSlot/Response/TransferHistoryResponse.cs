using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThirdPartyWallet.Share.Model.Game.EGSlot.Response;
public class TransferHistoryResponse : ErrorCodeResponse
{
    public Datum[] Data { get; set; }
    public bool Next { get; set; }
    public class Datum
    {
        /// <summary>
        /// 轉帳參考碼
        /// </summary>
        public string ReferenceCode { get; set; }
        /// <summary>
        /// 玩家帳號
        /// </summary>
        public string Username { get; set; }
        /// <summary>
        /// 幣別
        /// </summary>
        public string Currency { get; set; }
        /// <summary>
        /// 轉帳時間戳，單位毫秒
        /// </summary>
        public long Time { get; set; }
        /// <summary>
        /// 轉帳金額。負數轉出，正數為轉入
        /// </summary>
        public string Amount { get; set; }
        /// <summary>
        /// 轉帳後，玩家餘額
        /// </summary>
        public string AfterBalance { get; set; }
        /// <summary>
        /// 轉點狀態 1: 處理中、2: 成功、3: 失敗
        /// </summary>
        public int Status { get; set; }
    }
}




