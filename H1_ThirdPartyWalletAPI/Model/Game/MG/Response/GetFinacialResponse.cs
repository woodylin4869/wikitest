using System;
using System.Collections.Generic;

namespace H1_ThirdPartyWalletAPI.Model.Game.MG.Response
{
    /// <summary>
    /// Get detailed financial report 获取详细资金报表
    /// </summary>
    public class GetFinacialResponse { 
        public List<FinacialReport> data { get; set; }
    }
    public class FinacialReport
    {
        public DateTime Time { get; set; }
        public string ProductCode { get; set; }
        public string ProductDescription { get; set; }
        public string AgentCode { get; set; }
        public string AgentDescription { get; set; }
        /// <summary>
        /// 下注金额
        /// </summary>
        public decimal Income { get; set; }
        public decimal IncomePercentage { get; set; }
        /// <summary>
        /// 派彩金额
        /// </summary>
        public decimal Payout { get; set; }
        public decimal PayoutPercentage { get; set; }
        public decimal GrossWin { get; set; }
        public decimal GrossWinPercentage { get; set; }
        public int NumOfBets { get; set; }
        public decimal Margin { get; set; }
        public decimal DepositAmount { get; set; }
        public decimal WithdrawalAmount { get; set; }
        public decimal NetCash { get; set; }
        public int NumOfDeposits { get; set; }
        public int NumOfWithdrawals { get; set; }
       
    }
}
