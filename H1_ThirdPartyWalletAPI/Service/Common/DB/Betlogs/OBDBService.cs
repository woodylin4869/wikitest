using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using H1_ThirdPartyWalletAPI.Model.Config;
using H1_ThirdPartyWalletAPI.Model.Game.OB.Response;
using H1_ThirdPartyWalletAPI.Model.W1API;
using H1_ThirdPartyWalletAPI.Utility;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;

namespace H1_ThirdPartyWalletAPI.Service.Common.DB.Betlogs
{
    public interface IOBDBService
    {
        Task<int> PostOBRecord(NpgsqlConnection conn, IDbTransaction tran, IEnumerable<BetHistoryRecordResponse.Record> betLogs);
        Task<List<BetHistoryRecordResponse.FromDateRecord>> GetOBRecordsBytime(DateTime start, DateTime end);
        Task<List<BetHistoryRecordResponse.FromDateRecord>> GetOBRecords(long id);
        Task<(int totalCount, decimal totalBetValid, decimal totalWin)> SumOBBetRecordByBetTime(DateTime start, DateTime end);
        Task<List<BetHistoryRecordResponse.FromDateRecord>> GetOBRecordsBySummary(GetBetRecordReq RecordReq);

        Task<dynamic> GetOBRecord(string id);
    }
    public class OBDBService: BetlogsDBServiceBase, IOBDBService
    {
        public OBDBService(ILogger<OBDBService> logger, IBetLogsDbConnectionStringManager connectionStringManager) : base(logger, connectionStringManager)
        {
        }

        /// <summary>
        /// 寫入資料
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="tran"></param>
        /// <param name="betLogs"></param>
        /// <returns></returns>
        public async Task<int> PostOBRecord(NpgsqlConnection conn, IDbTransaction tran, IEnumerable<BetHistoryRecordResponse.Record> betLogs)
        {

            var sql = @"INSERT INTO public.t_ob_bet_record
                    (id
                    ,agentcode
                    ,playername
                    ,betamount
                    ,validbetamount
                    ,netamount
                    ,payamount
                    ,createdat
                    ,netat
                    ,updatedat
                    ,recalcuat
                    ,gametypeid
                    ,gametypename
                    ,platformid
                    ,platformname
                    ,betstatus
                    ,betflag
                    ,betpointid
                    ,odds
                    ,betpointname
                    ,currency
                    ,tablecode
                    ,tablename
                    ,roundno
                    ,bootno
                    ,recordtype
                    ,gamemode
                    ,dealername
                    ,realdeductamount
                    ,bettingrecordtype
                    ,summary_id
                    ,pre_betamount
                    ,pre_validbetamount
                    ,pre_netamount
                    ,pre_payamount 
                    ,addstr1
                    ,addstr2)
                    VALUES
                    ( @id
                     ,@agentcode
                     ,@playername
                     ,@betamount
                     ,@validbetamount
                     ,@netamount
                     ,@payamount
                     ,to_timestamp(@createdat / 1000.0)
                     ,to_timestamp(@netat / 1000.0)
                     ,to_timestamp(@updatedat / 1000.0)
                     ,to_timestamp(@recalcuat / 1000.0)
                     ,@gametypeid
                     ,@gametypename
                     ,@platformid
                     ,@platformname
                     ,@betstatus
                     ,@betflag
                     ,@betpointid
                     ,@odds
                     ,@betpointname
                     ,@currency
                     ,@tablecode
                     ,@tablename
                     ,@roundno
                     ,@bootno
                     ,@recordtype
                     ,@gamemode
                     ,@dealername
                     ,@realdeductamount
                     ,@bettingrecordtype
                     ,@summary_id
                     ,@pre_betamount
                     ,@pre_validbetamount
                     ,@pre_netamount
                     ,@pre_payamount
                     ,@addstr1
                     ,@addstr2)
                    ";
            try
            {
                return await conn.ExecuteAsync(sql, betLogs, tran);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        /// <summary>
        /// 注單號取資料
        /// </summary>
        /// <param name="WagersId"></param>
        /// <returns></returns>
        public async Task<List<BetHistoryRecordResponse.FromDateRecord>> GetOBRecords(long id)
        {
            var sql = @"
                    SELECT * FROM t_ob_bet_record
                    WHERE id = @id";

            var par = new DynamicParameters();
            par.Add("@id", id);


            await using var conn = new NpgsqlConnection(await PGRead);
            var result = await conn.QueryAsync<BetHistoryRecordResponse.FromDateRecord>(sql, par);
            return result.ToList();
        }

        /// <summary>
        /// 每小時匯總
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public async Task<(int totalCount, decimal totalBetValid, decimal totalWin)> SumOBBetRecordByBetTime(DateTime start, DateTime end)
        {
            var sql = @"SELECT 
                    COUNT(id) AS totalCount
                    , CASE WHEN SUM(betamount) IS NULL THEN 0 ELSE SUM(betamount) END  AS totalBetValid
                    , CASE WHEN SUM(netamount) IS NULL THEN 0 ELSE SUM(netamount) END AS totalWin
                    FROM t_ob_bet_record
                    WHERE netat >= @startTime 
                        AND netat < @endTime";

            var par = new DynamicParameters();
            par.Add("@startTime", new DateTime(start.Year, start.Month, start.Day, 0, 0, 0));
            par.Add("@endTime", new DateTime(start.Year, start.Month, start.Day, 23, 59, 59, 59));

            await using var conn = new NpgsqlConnection(await PGRead);
            var result = (await conn.QueryAsync<dynamic>(sql, par)).FirstOrDefault();
            return ((int)result.totalcount, (decimal)result.totalbetvalid, (decimal)result.totalwin);
        }
        /// <summary>
        /// 取得時間內的住單
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public async Task<List<BetHistoryRecordResponse.FromDateRecord>> GetOBRecordsBytime(DateTime start, DateTime end)
        {
            try
            {
                var sql = @"SELECT *
                    FROM public.t_ob_bet_record 
                    WHERE netat >= @startTime 
                        AND netat < @endTime";
                var par = new DynamicParameters();
                par.Add("@startTime", start);
                par.Add("@endTime", end);

                await using var conn = new NpgsqlConnection(await PGRead);
                var result = await conn.QueryAsync<BetHistoryRecordResponse.FromDateRecord>(sql, par);
                return result.ToList();
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        /// <summary>
        /// 取得GUID資料
        /// </summary>
        /// <param name="RecordReq"></param>
        /// <returns></returns>
        public async Task<List<BetHistoryRecordResponse.FromDateRecord>> GetOBRecordsBySummary(GetBetRecordReq RecordReq)
        {
            var sql = @"SELECT * 
                    FROM public.t_ob_bet_record 
                    WHERE netat >= @start 
                        AND netat <= @end
                        AND summary_id = @summaryId::uuid";
            var param = new
            {
                summaryId = RecordReq.summary_id,
                start = RecordReq.ReportTime.AddDays(-3),
                end = RecordReq.ReportTime.AddDays(1),
            };

            await using var conn = new NpgsqlConnection(await PGRead);
            var result = await conn.QueryAsync<BetHistoryRecordResponse.FromDateRecord>(sql, param);
            return result.ToList();
        }
        /// <summary>
        /// 使用wagersId住單號取得資料
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<dynamic> GetOBRecord(string id)
        {
            string strSql = @"SELECT *
                    FROM t_ob_bet_record
                    WHERE id = @id
                    Limit 1";
            var parameters = new DynamicParameters();
            parameters.Add("@wagersId", Int64.Parse(id));

            using (var conn = new NpgsqlConnection(await PGRead))
            {
                return await conn.QuerySingleOrDefaultAsync<dynamic>(strSql, parameters);
            }
        }
    }
}
