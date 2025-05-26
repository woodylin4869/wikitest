using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThirdPartyWallet.Share.Model.Game.IDN.Request
{
    /// <summary>
    /// {{SERVER}}/pubs/v1/games/aaaaaa/idnlive/result/{{gameId}}/{{matchId}}/{{date}}
    /// </summary>
    public class GetGameResultRequest
    {
        /// <summary>
        /// game_id
        /// </summary>
        public string gameId { get; set; }


        /// <summary>
        /// matchId
        /// </summary>
        public string matchId { get; set; }

        /// <summary>
        /// "2024-08-02"
        /// </summary>
        public string date { get; set; }
        
    }
}