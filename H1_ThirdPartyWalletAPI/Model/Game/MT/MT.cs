using System.Collections.Generic;

namespace H1_ThirdPartyWalletAPI.Model.Game.MT
{
    public class MT
    {
        public static Dictionary<string, string> lang = new Dictionary<string, string>()
        {
            {"en-US","EN-US"},//英文
            {"zh-CN","ZH-CN" },//簡體中文
            {"zh-TW","ZH-TW"},//繁體
            {"ko-KR","KO"},//韓語
            {"th-TH","TH"},//泰文
            {"vi-VN","VI"},//越文
        };

        public static Dictionary<string, string> Currency = new Dictionary<string, string>()
        {
            {"THB","THB1"},  // 泰銖
        };


        public static Dictionary<string, string> ErrorCode = new Dictionary<string, string>()
        {
            {"0","异常"},
            {"1","成功"},
            {"2","商户不存在"},
            {"3","商户无效"},
            {"4","商户用户不存在"},
            {"5","商户用户已注册"},
            {"6","商户用户系统禁用"},
            {"7","密码错误"},
            {"8","新密码与旧密码相同"},
            {"9","商户用户金币余额不足"},
            {"10","商户用户在线状态"},
            {"11","商户用户离线状态"},
            {"12","商户用户游戏在线"},
            {"13","商户用户游戏离线"},
            {"14","uId错误"},
            {"15","IP被限制"},
            {"16","游戏代码错误"},
            {"17","游戏未开通"},
            {"18","游戏房间未开通"},
            {"20","XML游戏信息无法解析"},
            {"21","解密出错"},
            {"22","商户用户登录禁用"},
            {"31","商户余额不足"},
            {"32","可选参数错误"},
            {"33","交易编码已存在"},
            {"36","存在未处理订单"},
            {"37","禁止游戏中转出"},
            {"38","请求超时"},
            {"40","维护模式"},
        };
    }
}
