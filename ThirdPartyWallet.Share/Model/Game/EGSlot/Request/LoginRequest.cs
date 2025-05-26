using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThirdPartyWallet.Share.Model.Game.EGSlot.Request
{
    public class LoginRequest
    {
        /// <summary>
        /// 轉帳錢包：玩家帳號，唯一值
        /// </summary>
        public string Username { get; set; }
        /// <summary>
        /// 遊戲代號
        /// </summary>
        public string GameID { get; set; }
        /// <summary>
        /// 是否登入大廳。預設不進入，若要進入大廳，請傳入 true，GameID 與 LoginHall 擇一填選
        /// </summary>
        public bool LoginHall { get; set; } = false;
        /// <summary>
        /// 運營商帳號
        /// </summary>
        public string AgentName { get; set; }
        /// <summary>
        /// 語系，預設為英文
        /// </summary>
        public string Lang { get; set; }
        /// <summary>
        /// 遊戲內的離開按鈕，要導回的網址
        /// 若無 HomeURL 的 Key，則嘗試關閉遊戲視窗
        /// 若有 HomeURL 的 Key，則導回該網址
        /// </summary>
        public string HomeURL { get; set; }
    }
}
