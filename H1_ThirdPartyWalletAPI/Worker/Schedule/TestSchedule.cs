using System;
using System.Threading.Tasks;
using Coravel.Invocable;
using H1_ThirdPartyWalletAPI.Model.DataModel;
using Microsoft.Extensions.Logging;
using H1_ThirdPartyWalletAPI.Service.Common;
using H1_ThirdPartyWalletAPI.Service.W1API;
using Npgsql;
using H1_ThirdPartyWalletAPI.Model.Config;
using System.Collections.Generic;
using Dapper;

namespace H1_ThirdPartyWalletAPI.Worker
{
    /// <summary>
    /// 每分鐘檢查洗分清單
    /// 檢查已經洗分15分鐘以上狀態為INIT Session
    /// </summary>
    public class TestSchedule : IInvocable
    {
        private readonly ILogger<TestSchedule> _logger;
        private bool _runningLock = false;
        private int _workTime = 0;

        public TestSchedule(ILogger<TestSchedule> logger)
        {
            _logger = logger;

        }
        public async Task Invoke()
        {
            _logger.LogInformation("Invoke TestSchedule on time : {time}", DateTime.Now);

            if (_runningLock)
            {
                await Task.CompletedTask;
                return;
            }

            _runningLock = true;
            try
            {
                var taskList = new List<Task>();
                taskList.Add(Create_t_bouncertest());
                taskList.Add(Read_t_wallet());

                Random myObject = new Random();
                int readtimes = myObject.Next(5, 10);
                for (int i = 0; i < readtimes; i++)
                    taskList.Add(Read_t_rcg_wallet_transaction());

                taskList.Add(Read_t_bouncertest());
                taskList.Add(Update_t_bouncertest());
                taskList.Add(Delete_t_bouncertest());

                await Task.WhenAll(taskList);
                _workTime++;
                _logger.LogInformation("Invoke total conount {workTime} end on time : {time}", _workTime, DateTime.Now);
            }
            catch (Exception ex)
            {
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                _logger.LogError("Run Test schedule exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
            }
            finally
            {
                _runningLock = false;
                await Task.CompletedTask;
            }

        }
        private async Task Create_t_bouncertest()
        {
            using (NpgsqlConnection conn = new NpgsqlConnection(Config.OneWalletAPI.DBConnection.Wallet.Master))
            {
                await conn.OpenAsync();
                using (var tran = conn.BeginTransaction())
                {
                    //Enumerable<dynamic> results = await _commonService._serviceDB.GetWalletLock(conn, tran, Club_id);


                    List<Wallet> walletList = new List<Wallet>();

                    for (int i = 0; i < 1000; i++)
                    {
                        var wallet_test = new Wallet();
                        wallet_test.Club_id = Guid.NewGuid().ToString();
                        wallet_test.Club_Ename = wallet_test.Club_id;
                        wallet_test.Currency = "THB";
                        walletList.Add(wallet_test);
                    }


                    string stSqlInsert = @"INSERT INTO t_bouncertest
                                    (
                                        club_id,
                                        club_ename,
                                        credit,
                                        lock_credit,
                                        currency,
                                        franchiser_id
                                    )
                                    VALUES
                                    (
                                        @club_id,
                                        @club_Ename,
                                        @credit,
                                        @lock_credit,
                                        @currency,
                                        @franchiser_id
                                    )";

                    await conn.ExecuteAsync(stSqlInsert, walletList, tran);
                    tran.Commit();
                }
            }
        }
        private async Task Read_t_wallet()
        {
            using (NpgsqlConnection conn = new NpgsqlConnection(Config.OneWalletAPI.DBConnection.Wallet.Master))
            {
                var par = new DynamicParameters();
                string strSql = @"SELECT *
                        FROM t_wallet";
                var results = await conn.QueryAsync<Wallet>(strSql, par);
            }

        }
        private async Task Read_t_rcg_wallet_transaction()
        {
            using (NpgsqlConnection conn = new NpgsqlConnection(Config.OneWalletAPI.DBConnection.Wallet.Master))
            {
                var par = new DynamicParameters();
                string strSql = @"SELECT *
                        FROM t_rcg_wallet_transaction
                        LIMIT 5
                        ";
                var results = await conn.QueryAsync<Wallet>(strSql, par);
            }

        }
        private async Task Read_t_bouncertest()
        {
            using (NpgsqlConnection conn = new NpgsqlConnection(Config.OneWalletAPI.DBConnection.Wallet.Master))
            {
                var par = new DynamicParameters();
                string strSql = @"SELECT *
                        FROM t_bouncertest
                        LIMIT 5000
                        ";
                var results = await conn.QueryAsync<Wallet>(strSql, par);
            }

        }
        private async Task Update_t_bouncertest()
        {
            using (NpgsqlConnection conn = new NpgsqlConnection(Config.OneWalletAPI.DBConnection.Wallet.Master))
            {
                await conn.OpenAsync();
                using (var tran = conn.BeginTransaction())
                {
                    var par = new DynamicParameters();

                    string strSql = @"UPDATE t_bouncertest
                                    SET credit = @credit,
                                    franchiser_id = @franchiser_id
                                    WHERE credit = @newcredit
                                    ";
                    par.Add("@credit", DateTime.Now.Second);
                    par.Add("@newcredit", DateTime.Now.Second + 1);
                    par.Add("@franchiser_id", "bouncertest");
                    int result = await conn.ExecuteAsync(strSql, par, tran);
                    await tran.CommitAsync();
                }
                await conn.CloseAsync();
            }

        }
        private async Task Delete_t_bouncertest()
        {
            var par = new DynamicParameters();
            string strSqlDel = @"DELETE FROM t_bouncertest
                               WHERE credit=@credit";

            par.Add("@credit", DateTime.Now.Second);
            using (var conn = new NpgsqlConnection(Config.OneWalletAPI.DBConnection.Wallet.Master))
            {
                await conn.ExecuteAsync(strSqlDel, par);
            }

        }

    }
}
