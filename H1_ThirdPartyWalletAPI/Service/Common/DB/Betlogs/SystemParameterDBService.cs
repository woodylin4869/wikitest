using Dapper;
using H1_ThirdPartyWalletAPI.Model.DataModel;
using Npgsql;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using H1_ThirdPartyWalletAPI.Model.Config;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using H1_ThirdPartyWalletAPI.Utility;

namespace H1_ThirdPartyWalletAPI.Service.Common.DB.Betlogs;

public interface ISystemParameterDbService
{
    Task<List<t_system_parameter>> GetAllSystemParameter();
    Task<t_system_parameter> GetSystemParameter(string key);
    Task<bool> PostSystemParameter(t_system_parameter source);
    Task<bool> PutMinSystemParameter(t_system_parameter source);
    Task<bool> PutSystemParameter(t_system_parameter source);
    Task<bool> PutSystemParameterValue(t_system_parameter source);
    Task<bool> DeleteSystemParameter(string key);
}

public class SystemParameterDBService : BetlogsDBServiceBase, ISystemParameterDbService
{
    #region t_system_parameter

    public SystemParameterDBService(ILogger<SystemParameterDBService> logger, IBetLogsDbConnectionStringManager connectionStringManager) : base(logger, connectionStringManager)
    {
    }

    public async Task<List<t_system_parameter>> GetAllSystemParameter()
    {
        const string sql = @"select * from t_system_parameter";
        await using (var conn = new NpgsqlConnection(PGMaster))
        {
            var result = await conn.QueryAsync<t_system_parameter>(sql);
            return result.ToList();
        }
    }
    public async Task<t_system_parameter> GetSystemParameter(string key)
    {
        const string sql = @"select * from t_system_parameter where key = @key";

        var parameters = new DynamicParameters();
        parameters.Add("key", key);

        await using (var conn = new NpgsqlConnection(PGMaster))
        {
            return await conn.QuerySingleOrDefaultAsync<t_system_parameter>(sql, parameters);
        }
    }
    public async Task<bool> PostSystemParameter(t_system_parameter source)
    {
        const string sql = @"INSERT INTO t_system_parameter
                (
	                key,
	                value,
	                name,
	                description,
	                min_value,
	                max_value
                )
                VALUES
                (
	                @key,
	                @value,
	                @name,
	                @description,
	                @min_value,
	                @max_value
                )";

        await using (var conn = new NpgsqlConnection(PGMaster))
        {
            return await conn.ExecuteAsync(sql, source) > 0;
        }
    }
    public async Task<bool> PutSystemParameter(t_system_parameter source)
    {
        var sql = @"update t_system_parameter
                        set value = @value, 
                            name = @name, 
                            description  = @description, 
                            min_value = @min_value,
                            max_value = @max_value
                        where key = @key";

        await using (var conn = new NpgsqlConnection(PGMaster))
        {
            return await conn.ExecuteAsync(sql, source) > 0;
        }
    }
    public async Task<bool> PutMinSystemParameter(t_system_parameter source)
    {
        var sql = @"update t_system_parameter
                        set min_value = @min_value
                        where key = @key";

        await using (var conn = new NpgsqlConnection(PGMaster))
        {
            return await conn.ExecuteAsync(sql, source) > 0;
        }
    }
    public async Task<bool> PutSystemParameterValue(t_system_parameter source)
    {
        var sql = @"update t_system_parameter
                        set value = @value
                        where key = @key";

        await using (var conn = new NpgsqlConnection(PGMaster))
        {
            return await conn.ExecuteAsync(sql, source) > 0;
        }
    }
    public async Task<bool> DeleteSystemParameter(string key)
    {
        const string sql = @"DELETE FROM t_system_parameter WHERE key = @key ";
        await using (var conn = new NpgsqlConnection(PGMaster))
        {
            return await conn.ExecuteAsync(sql, key) > 0;
        }
    }
    #endregion
}