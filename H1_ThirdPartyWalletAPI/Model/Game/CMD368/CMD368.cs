using System.Collections.Generic;
using System.ComponentModel;

namespace H1_ThirdPartyWalletAPI.Model.Game.CMD368
{
    public class CMD368
    {
        /// <summary>
        /// 語系 Mapping
        /// key: W1, Value: 遊戲商
        /// </summary>
        public static Dictionary<string, string> lang = new Dictionary<string, string>()
        {
            {"en-US", "en-US"},  // 英文
            {"zh-TW", "zh-TW"},  // 繁體中文
            {"zh-CN", "zh-CN"},  // 簡體中文
            {"th-TH", "th-TH"},  // 泰文
            {"ko-KR", "ko-KR"},  // 韓文
            {"pt-PT", "pt-PT"},  // 葡萄牙文
            {"vi-VN", "vi-VN"},  // 越南文
            {"es-ES", "es-ES"}, // 西班牙文
            {"id-ID", "id-ID"},  // 印尼文
        };
        public static Dictionary<string, string> Currency = new Dictionary<string, string>()
        {
            {"THB", "THB"},  // 泰銖,編號:5
        };
        public enum error_code
        {
            [Description("成功")]
            successed = 0,
            [Description("失敗")]
            failed = -1,
            [Description("非法請求")]
            illegalrequest = -95,
            [Description("單據號已存在")]
            trackingnumberlreadyexists = -96,
            [Description("用戶不存在")]
            Userdoesnotexist = -97,
            [Description("用戶已存在")]
            Useralraedyexist = -98,
            [Description("無效的參數")]
            invalidparameter = -100,
            [Description("維護中")]
            inmaintenance = -101,
            [Description("請求限制")]
            requestlimit = -102,
            [Description("權限限制")]
            permissionrestriction = -103,
            [Description("字符超过限制")]
            characterexceedslimit = -104,
            [Description("伺服器錯誤")]
            servererror = -999,
            [Description("伺服器超時")]
            severtimeout = -1000,
            [Description("幣別必須為大小寫或是不支持該幣別")]
            currencyerror = -8012,
            [Description("幣別未申請")]
            Currencynotapplied = -8017,
            [Description("餘額不足")]
            Insufficientbalance = -8037,
        }
    }
}

