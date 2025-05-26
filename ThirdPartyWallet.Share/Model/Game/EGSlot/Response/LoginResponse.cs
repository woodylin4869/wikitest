using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThirdPartyWallet.Share.Model.Game.EGSlot.Response
{
    public class LoginResponse:ErrorCodeResponse

    {
    public string URL { get; set; }
    }

}