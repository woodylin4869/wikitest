using H1_ThirdPartyWalletAPI.Model.DataModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace H1_ThirdPartyWalletAPI.Model.W1API
{
    public class GetSystemParameterRes : ResCodeBase
    {
        /// <summary>
        /// t_system_parameter資料
        /// </summary>
        public List<t_system_parameter> Data { get; set; }
    }
    public enum SystemParameterStatus
    {
        SUSPEND = 0,   //暫停
        RUNNIMG = 1,   //執行
    }

    public class PutSystemParameterReq
    {
        /// <summary>
        /// 主要執行參數
        /// </summary>
        public string value { get; set; }
        /// <summary>
        /// 最小值(啟用開關) 0:暫停 1:執行
        /// </summary>
        [Range(0, 1)]
        [DefaultValue(1)]
        public long min_value { get; set; }
        /// <summary>
        /// 最大值
        /// </summary>
        //[DefaultValue(1)]
        //public long max_value { get; set; }
    }
}
