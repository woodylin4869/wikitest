using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace H1_ThirdPartyWalletAPI.Model.Game.SEXY.Request
{
    public class CreateMemberRequest : SexyRequestBase
    {
        [Required]
        public string userId { get; set; }
        [Required]
        public string currency { get; set; }
        [Required]
        public string betLimit { get; set; }
        public string language { get; set; }
        public string userName { get; set; }
    }


}
