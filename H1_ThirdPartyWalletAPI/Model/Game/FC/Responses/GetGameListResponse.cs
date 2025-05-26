using Swashbuckle.AspNetCore.SwaggerGen;
using System.Collections.Generic;

namespace H1_ThirdPartyWalletAPI.Model.Game.FC.Response
{
    public class GetGameListResponse : FCBaseStatusRespones
    {
        /// <summary>
        /// {游戏类别:{游戏编号:启用状态}} (请参照游戏类别对应表
        /// </summary>
        public object GetGameList { get; set; }
    }
}
