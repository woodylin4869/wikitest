using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace H1_ThirdPartyWalletAPI.Model.Game.SEXY.Request
{
    public class CheckTransferOperationRequest : SexyRequestBase
    {
        [Required]
        public string txCode { get; set; }
    }
}
