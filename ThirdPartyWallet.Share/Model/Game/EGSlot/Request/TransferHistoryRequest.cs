using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThirdPartyWallet.Share.Model.Game.EGSlot.Request;
public class TransferHistoryRequest
{
    public long StartTime { get; set; }
    public long EndTime { get; set; }
    public string AgentName { get; set; }
    public string Username { get; set; }
    public string ReferenceCode { get; set; }
    public int Status { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}
