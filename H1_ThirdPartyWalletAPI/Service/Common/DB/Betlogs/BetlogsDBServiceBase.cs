using System.Threading.Tasks;
using H1_ThirdPartyWalletAPI.Utility;
using Microsoft.Extensions.Logging;

namespace H1_ThirdPartyWalletAPI.Service.Common.DB.Betlogs
{
    public abstract class BetlogsDBServiceBase
    {
        protected readonly ILogger _logger;

        private readonly string _masterConnectionStr;
        protected string PGMaster => _masterConnectionStr;

        private readonly IDbConnectionStringManager _manager;
        protected Task<string> PGRead => _manager.GetReadConnectionString();

        public BetlogsDBServiceBase(ILogger logger
            , IBetLogsDbConnectionStringManager connectionStringManager) 
        {
            _logger = logger;
            _manager = connectionStringManager;
            _masterConnectionStr = _manager.GetMasterConnectionString();
        }
    }
}
