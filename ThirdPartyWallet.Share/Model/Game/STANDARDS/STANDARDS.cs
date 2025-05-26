namespace ThirdPartyWallet.Share.Model.Game.STANDARDS
{
    public class STANDARDS
    {

        // todo: 此列表現況無用 GR 沒提供可選語系列表
        // GR 回應: 可透過代理後台 預設代理底下玩家語系
        public static Dictionary<string, string> lang = new Dictionary<string, string>()
        {
            {"zh-CN", "zh-CN"},
            {"zh-TW", "zh-TW"},
            {"en-US", "en-US"},
            {"th-TH", "th-TH"},
            {"lo-LAO", "lo-LAO"},
            {"vi-VN", "vi-VN"},
            {"ko-KR", "ko-KR"},
            {"ja-JP", "ja-JP"},
            {"ms-MY", "ms-MY"},
            {"en-MY", "en-MY"},
            {"id-ID", "id-ID"},
            {"es-ES", "es-ES"},
        };

        // todo: 此列表現況無用 GR 沒提供可選幣別列表
        public static Dictionary<string, string> Currency = new Dictionary<string, string>()
        {
             {"THB","THB"},  // 泰銖
        };
        public static Dictionary<string, string> ErrorCode = new Dictionary<string, string>()
        {
            {"1", "成功"},
            {"1001", "Token錯誤"},
            {"1002", "加密錯誤"},
            {"1003", "維護中"},
            {"1004", "未加入白名單"},
            {"1006", "API請求逾時"},
            {"2001", "帳號重複"},
            {"2002", "帳號不存在"},
            {"3001", "遊戲代碼不存在"},
            {"4001", "交易單號重複"},
            {"4002", "金額格式或不合格式"},
            {"4003", "取出大於目前錢包餘額的金額"},
            {"9001", "參數有誤"},
            {"9999", "沒有符合情形的錯誤碼"}
        };
    }
}
