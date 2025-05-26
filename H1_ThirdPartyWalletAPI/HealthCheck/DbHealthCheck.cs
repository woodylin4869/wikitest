using System;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using H1_ThirdPartyWalletAPI.Controllers.Game;
using H1_ThirdPartyWalletAPI.Model.Config;
using H1_ThirdPartyWalletAPI.Model.DataModel;
using H1_ThirdPartyWalletAPI.Service.Common;
using H1_ThirdPartyWalletAPI.Utility;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace H1_ThirdPartyWalletAPI.HealthCheck
{
    public class DbHealthCheck : IHealthCheck
    {
        private readonly IWalletDbConnectionStringManager _connectionStringManager;
        private readonly ILogger<DbHealthCheck> _logger;

        public DbHealthCheck(ILogger<DbHealthCheck> logger, IWalletDbConnectionStringManager connectionStringManager)
        {
            _connectionStringManager = connectionStringManager;
            _logger = logger;
        }


        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                using (var conn = new NpgsqlConnection(await _connectionStringManager.GetReadConnectionString()))
                {
                    var sql = @"select now()";
                    var result = await conn.QueryFirstOrDefaultAsync<RcgToken>(sql);
                    if (result != null)
                    {
                        return await Task.FromResult(HealthCheckResult.Healthy("db healthCheck success"));
                    }

                    return await Task.FromResult(new HealthCheckResult(context.Registration.FailureStatus, "db healthCheck Fail"));
                }
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "{message}", ex.Message);
                return await Task.FromResult(new HealthCheckResult(context.Registration.FailureStatus, "db healthCheck Fail"));
            }
        }

    }
}