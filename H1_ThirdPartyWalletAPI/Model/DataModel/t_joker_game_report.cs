using System;

namespace H1_ThirdPartyWalletAPI.Model.DataModel;

public class t_joker_game_report
{
    public DateTime Time { get; set; }
    public decimal Amount { get; set; }
    public decimal Result { get; set; }
    public int Count { get; set; }
}