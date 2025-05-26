using Dapper;
using Microsoft.Extensions.Logging;
using Npgsql;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Timer = System.Timers.Timer;

namespace H1_ThirdPartyWalletAPI.Utility
{
    public interface IWalletDbConnectionStringManager : IDbConnectionStringManager
    {
    }
    public interface IBetLogsDbConnectionStringManager : IDbConnectionStringManager
    {
    }

    public interface IDbConnectionStringManager
    {
        Task<string> GetReadConnectionString();
        string GetMasterConnectionString();

        Task<NpgsqlConnection> GetReadConnectionAsync();

        Task<NpgsqlConnection> GetMasterConnectionAsync();
    }

    public class WalletDbConnectionStringManager : DbConnectionStringManager,
        IWalletDbConnectionStringManager
    {
        public WalletDbConnectionStringManager(ILogger<WalletDbConnectionStringManager> logger, string master, string[] readPool) : base(logger, master, readPool)
        {
        }
    }

    public class BetLogsDbConnectionStringManager : DbConnectionStringManager,
        IBetLogsDbConnectionStringManager
    {
        public BetLogsDbConnectionStringManager(ILogger<BetLogsDbConnectionStringManager> logger, string master, string[] readPool) : base(logger, master, readPool)
        {
        }
    }

    public class DbConnectionStringManager : IDbConnectionStringManager, IDisposable
    {
        private readonly ILogger<DbConnectionStringManager> _logger;
        private readonly string[] _readPool;
        private readonly string _master;
        private int _availableReadIndex;
        private int _isTimerHandlerRunning;

        //Timer 檢查斷同步讀側是否修復
        private readonly Timer _timer = new(TimeSpan.FromSeconds(30).TotalMilliseconds);

        public DbConnectionStringManager(ILogger<DbConnectionStringManager> logger, string master, string[] readPool)
        {
            this._logger = logger;
            this._master = master;
            this._readPool = readPool;

            //_timer.Elapsed += CheckDeadReadAsyncStatus;
            //_timer.AutoReset = true;
            //_timer.Enabled = true;
        }

        public async Task<string> GetReadConnectionString()
        {
            //當前_availableReadIndex值，因未進行lock，避免後續運行時_availableReadIndex被其他執行續改變，需先儲存當前值。
            int index;

            //_availableReadIndex 是否已超過 pool 長度，並儲存當前值
            while ((index = Interlocked.CompareExchange(ref _availableReadIndex, -1, -1))
                   < _readPool.Length)
            {
                if (await CheckAsync(index))
                    return _readPool[index];

                //if _availableReadIndex == index then _availableReadIndex = index + 1
                Interlocked.CompareExchange(ref _availableReadIndex, index + 1, index);
            }

            //所有讀側皆斷同步
            return _master;
        }

        public string GetMasterConnectionString() => _master;

        public async Task<NpgsqlConnection> GetReadConnectionAsync()
        {
            return new NpgsqlConnection(await GetReadConnectionString());
        }

        public Task<NpgsqlConnection> GetMasterConnectionAsync()
        {
            return Task.FromResult(new NpgsqlConnection(GetMasterConnectionString()));
        }

        /// <summary>
        /// 檢查讀側同步狀態
        /// </summary>
        /// <returns></returns>
        private async Task<bool> CheckAsync(int index)
        {
            await using var conn = new NpgsqlConnection(_readPool[index]);
            try
            {
                const string sql = "select 1 from pg_stat_wal_receiver";
                var result = await conn.QueryFirstOrDefaultAsync<int>(sql, commandTimeout: 3);

                if(result == default)
                    _logger.LogCritical("{database} 讀側資料庫同步異常 ", conn.Database);

                return result != default;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{action} exception {database}", nameof(DbConnectionStringManager), conn.Database);
                return false;
            }
        }

        private async void CheckDeadReadAsyncStatus(object sender, ElapsedEventArgs e)
        {
            // 若 _availableReadIndex == 0 則無需檢查
            if (Interlocked.CompareExchange(ref _availableReadIndex, -1, -1) == 0)
                return;

            // 若 _isTimerHandlerRunning == 0 則 設為 1 表示Handler執行中， 否則表示已有Handler正在執行，直接結束
            if (Interlocked.CompareExchange(ref _isTimerHandlerRunning, 1, 0) != 0)
                return;

            //找出同步狀態良好的讀側並將 _availableReadIndex 切換過去
            var readIndex = 0;
            while (readIndex < _readPool.Length)
            {
                if (await CheckAsync(readIndex))
                {
                    Interlocked.Exchange(ref _availableReadIndex, readIndex);
                    break;
                }

                readIndex++;
            }

            //_isTimerHandlerRunning 設為 0 表示Handler執行完畢
            Interlocked.Exchange(ref _isTimerHandlerRunning, 0);
        }

        public void Dispose()
        {
            _timer?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
