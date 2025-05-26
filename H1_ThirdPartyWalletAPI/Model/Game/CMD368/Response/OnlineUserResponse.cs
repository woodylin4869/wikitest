using System;
using System.ComponentModel.DataAnnotations;

namespace H1_ThirdPartyWalletAPI.Model.Game.CMD368.Response
{
    /// <summary>
    /// 在線用戶列表
    /// </summary>
    public class OnlineUserResponse
    {
        /// <summary>
        /// 操作批次號
        /// </summary>
        public string SerialKey { get; set; }
        /// <summary>
        /// 時間戳記
        /// </summary>
        public long Timestamp { get; set; }
        /// <summary>
        /// 錯誤代碼
        /// </summary>
        public int Code { get; set; }
        /// <summary>
        /// 錯誤訊息
        /// </summary>
        public string Message { get; set; }

        public Dato[] Data { get; set; }
        }

        public class Dato
        {
        /// <summary>
        /// 用戶名稱
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string UserName { get; set; }
        /// <summary>
        /// 最後登入時間
        /// </summary>
        public DateTime LastLoginTime { get; set; }
        /// <summary>
        /// 最後登入IP
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string LastLoginIP { get; set; }
        }

    }

