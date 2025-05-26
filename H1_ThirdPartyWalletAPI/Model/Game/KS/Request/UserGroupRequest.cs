using System;
using System.ComponentModel.DataAnnotations;

namespace H1_ThirdPartyWalletAPI.Model.Game.KS.Request
{
    public class UserGroupRequest
    {
        /// <summary>
        /// 会员名
        /// 数字字母下划线，2~30位 示例：ceshi008
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// 币种代码参见 附录6:会员分组
        /// </summary>
        public string GroupID { get; set; }

    }
}
