using System.Collections.Generic;
using System.Collections.Immutable;

namespace H1_ThirdPartyWalletAPI.Model.Game.BTI
{
    public static class BTI
    {

        /// <summary>
        /// 轉帳 Wallet API 回傳错码
        /// </summary>
        public static readonly Dictionary<string, string> WalletErrorCode = new Dictionary<string, string>()
        {
            {"NoError", "NoError"},                                             // NoError （无问题）
            {"AuthenticationFailed", "AuthenticationFailed"},                   // AuthenticationFailed （代理商登入无法确认）
            {"MerchantIsFrozen", "MerchantIsFrozen"},                           // MerchantIsFrozen （代理商已经停用）
            {"NoMerchantNotActiveError", "MerchantNotActive"},                  // MerchantNotActive （代理商已无活跃）
            {"Exception", "Exception"},                                         // Exception （其他错误）
            {"TransactionCodeNotFound", "TransactionCodeNotFound"},             // TransactionCodeNotFound （转账码不存在）
            {"CustomerNotFound", "CustomerNotFound"},                           // CustomerNotFound <自己測到的..帳號不存在>
            {"DuplicateMerchantCustomerCode", "DuplicateMerchantCustomerCode"}, // DuplicateMerchantCustomerCode <自己測到的..帳號已存在>
            {"DuplicateLoginName", "DuplicateLoginName"},                       // DuplicateLoginName <自己測到的..帳號已存在>
            {"RefTransactionCodeIsEmpty", "RefTransactionCodeIsEmpty"},         // RefTransactionCodeIsEmpty  <自己測到的..輸入轉帳ID為空>
        };

        /// <summary>
        /// 注單 DATA API 回傳错码 
        /// 廠商文件版本: (BTi 数据接口 APIver2.0 3.3.9.pdf)
        /// </summary>
        public enum DataErrorCode
        {
            Success = 0,                            // No error
            ExpiredToken = -1000,                   // Wrong/Expired Token -1000 使用的 token 失败或超时已过期
            AuthorizationError = -1,                // Authorization error -1 代理账号或密码错误
            //CustomerNotExist = -2,                // Customer doesn’t exist -2 玩家不存在
            GeneralError = 3,                       // General Error -3 一般错误 – 系统失败或超时，请回报在对接群组
            InvalidParameters = -4,                 // Invalid or Missing Parameters -4 参数不正确
            //WrongAgentUsernameOrPassword = -5,    // Wrong Agent Username or Password -5 代理账号或密码错误
            //ExceedQueryPeriod = -8,               // Exceed query period -8 超过访问的时间
            //ExceededAPICalls = -9,                // Exceeded API calls -9 超过呼叫的次数太频密
            //APINotAllowed = -10                   // API method is not allowed -10 API 方法不允许
        }

        /// <summary>
        /// 下注类别 ID 和下注名列表
        /// </summary>
        public enum BetType
        {
            /*  BetTypeID 下注类别 ID  BetTypeName 下注类别名
            1 Single bets 单场投注
            2 Combo bets 组合投注
            3 System bet 系统投注
            5 QA Bet QA 投注
            6 Exact Score 准确比分
            7 QA Bet QA 投注
            13 System bet 系统投注*/
        }

        /// <summary>
        /// Group1ID 请放 0（新玩家）或 1 （普通玩家)
        /// 这个可以用0 现在的这功能是没有影响到，所以开1 和0 都会一样 因为您开新的会员，可以用0
        /// </summary>
        public enum Group1ID
        {
            NewUser = 0,
            NormalUser = 1
        }

        /// <summary>
        /// 网站支持的语言:  EN / ZH / TH / IN / VI / JA / KO / PT / TR / ES / MY
        /// EN - English 英文
        /// ZH - Simplified Chinese 簡體中文
        /// IN - Indonesian 印尼文
        /// VI - Vietnamese 越南文
        /// TH - Thai 泰文
        /// KO - Korean 韓文
        /// JA - Japanese 日文
        /// ES - Spanish 西班牙文
        /// TR - Turkish 土耳其文
        /// PT - Portugese 葡萄牙文
        /// MY - Burmese 緬甸文
        /// </summary>
        public static readonly ImmutableDictionary<string, string> Lang = new Dictionary<string, string>()
        {
            {"en-US", "en"},  // 英文
            {"zh-TW", "zh"},  // 繁體中文
            {"zh-CN", "zh"},  // 簡體中文
            {"th-TH", "th"},  // 泰文
            {"ko-KR", "ko"},  // 韓文
            {"vi-VN", "vi"},  // 越南文
            {"ja-JP", "ja"},  // 日文
            {"id-ID", "in"},  // 印尼文
            {"es-ES", "es"},  // 西班牙文
        }.ToImmutableDictionary();

        /// <summary> 
        /// 货币必须英文. ISO 4217 标准. 如 RMB, CNY
        /// 
        /// 此幣別需該代理有啟用才能使用
        /// BTi 支持的货币： (可多选: THB / MYR / USD / VND (1:1000) / IDR (1:1000) / CNY / JPY / KRW / INR / MMK / BDT / mBC / USDT)
        /// 但貴司目前僅選擇:THB/CNY/PHP/VND/ KRW/IDR, 將依貴司所選進行設置
        /// </summary>
        public static readonly Dictionary<string, string> Currency = new Dictionary<string, string>()
        {
            {"THB", "THB"},
            {"CNY", "CNY"}, // h1是用CNY? RMB? 廠商是用CNY
            {"PHP", "PHP"},
            {"VND", "VND"},
            {"KRW", "KRW"},
            {"IDR", "IDR"}
        };

        // todo... 回傳有空白字
        /// <summary>
        /// BetStatus 下注状态: (注意. “Declined” 拒绝是在系统有误才会有的。)
        /// Opened 未结算
        /// Won 赢
        /// Lost 输
        /// Half Won 半赢
        /// Half Lost 半输
        /// Canceled 取消
        /// Cashout 提前兑现
        /// Draw 平局
        /// Declined 拒绝
        /// </summary>
        public enum BetStatus
        {
            Opened,     // 未结算
            Won,        // 赢
            Lost,       // 输
            HalfWon,    // 半赢
            HalfLost,   // 半输
            Canceled,   // 取消
            Cashout,    // 提前兑现
            Draw,       // 平局
            Declined    // 拒绝
        }
    }
}
