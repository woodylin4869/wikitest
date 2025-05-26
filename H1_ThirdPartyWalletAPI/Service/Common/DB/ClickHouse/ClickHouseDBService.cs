using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using H1_ThirdPartyWalletAPI.Model.Config;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Octonica.ClickHouseClient;
using Dapper;

namespace H1_ThirdPartyWalletAPI.Service.Common.DB.ClickHouse
{
    public class ClickHouseDBService : IDisposable
    {
        private readonly string _connectionString;
        private ClickHouseConnection _connection;


        public ClickHouseDBService(ClickHouseDBConnection config)
        {
            this._connectionString = config.Master;
            this._connection = new ClickHouseConnection();
            this._connection.ConnectionString = this._connectionString;
        }

        // 確保每次取得新連線
        private ClickHouseConnection GetConnection()
        {
            if (_connection == null)
            {
                _connection = new ClickHouseConnection(_connectionString);
                _connection.Open();
            }
            return _connection;
        }

        // 通用的查詢方法，釋放資源
        public async Task<List<T>> QueryAsync<T>(string query, Func<IDataRecord, T> mapFunction)
        {
            var results = new List<T>();
            await using var connection = GetConnection();
            await using var command = connection.CreateCommand();
            command.CommandText = query;

            await using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                results.Add(mapFunction(reader));
            }
            return results;
        }




        // 插入資料的範例，釋放資源
        public async Task<int> InsertAsync(string tableName, Dictionary<string, object> data)
        {
            if (data == null || data.Count == 0)
                throw new ArgumentException("Data dictionary cannot be empty.");

            var columns = string.Join(", ", data.Keys);
            var parameters = string.Join(", ", data.Keys.Select(k => "@" + k));

            var query = $"INSERT INTO {tableName} ({columns}) VALUES ({parameters})";
            await using var connection = GetConnection();
            await using var command = connection.CreateCommand();
            command.CommandText = query;

            foreach (var (key, value) in data)
            {
                command.Parameters.AddWithValue("@" + key, value);
            }

            return await command.ExecuteNonQueryAsync();
        }

        // 執行非查詢命令，釋放資源
        public async Task<int> ExecuteNonQueryAsync(string commandText)
        {
            await using var connection = GetConnection();
            await using var command = connection.CreateCommand();
            command.CommandText = commandText;
            return await command.ExecuteNonQueryAsync();
        }

        public async virtual Task<IEnumerable<T>> ExecuteQuerySQLAsync<T>(string sqlstr, object param = null)
        {
            try
            {
                var result = await _connection.QueryAsync<T>(sqlstr, param);
                return result;
            }
            catch
            {

                throw;
            }

        }

        // Dispose 方法釋放連線
        public void Dispose()
        {
            if (_connection != null)
            {
                _connection.Dispose();
                _connection = null;
            }
        }
    }


    public static class ClickHouseDBServiceExtensions
    {
        public static void AddClickHouseDBServiceExtensions(this IServiceCollection services, IConfiguration configuration)
        {
            if (!services.Any(x => x.ServiceType == typeof(Config)))
            {
                var config = Config.OneWalletAPI.DBConnection.ClickHouse;
                services.AddSingleton(config);
            }
            services.AddScoped<ClickHouseDBService>();
        }

    }
}
