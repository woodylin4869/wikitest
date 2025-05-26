using H1_ThirdPartyWalletAPI.Model.Game.KS.Response;
using H1_ThirdPartyWalletAPI.Model.W1API;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using H1_ThirdPartyWalletAPI.Model.Config;
using System;
using H1_ThirdPartyWalletAPI.Utility;

namespace H1_ThirdPartyWalletAPI.Service.Common.DB.Betlogs
{
    public interface IKSDBService
    {
        Task<int> PostKSRecord(NpgsqlConnection conn, IDbTransaction tran, IEnumerable<Record> betLogs);

        Task<int> PostKSRunningRecord(NpgsqlConnection conn, IDbTransaction tran, IEnumerable<Record> betLogs);

        Task<int> DeleteKSRunningRecord(NpgsqlConnection conn, IDbTransaction tran, Record record_data);
        Task<List<GetKSRecordsRunningBySummaryResponse>> GetKSRunningRecord(GetBetRecordUnsettleReq RecordReq);

        Task<List<Record>> GetKSRecordsBytime(DateTime start, DateTime end);
        Task<List<Record>> GetKSRecords(NpgsqlConnection conn, IDbTransaction tran, string orderid, DateTime start);
        Task<(int totalcount, decimal totalbetvalid, decimal totalnetwin)> SumKSBetRecordByBetTime(DateTime start, DateTime end);
        Task<List<GetKSRecordsBySummaryResponse>> GetKSRecordsBySummary(GetBetRecordReq RecordReq);
    }
    public class KSDBService: BetlogsDBServiceBase, IKSDBService
    {
        public KSDBService(ILogger<KSDBService> logger, IBetLogsDbConnectionStringManager connectionStringManager) : base(logger, connectionStringManager)
        {
        }
        /// <summary>
        /// 寫入資料
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="tran"></param>
        /// <param name="betLogs"></param>
        /// <returns></returns>
        public async Task<int> PostKSRecord(NpgsqlConnection conn, IDbTransaction tran, IEnumerable<Record> betLogs)
        {
            var sql = @"INSERT INTO public.t_ks_bet_record 
(summary_id, orderid, username, ""type"", status, betamount, betmoney, ""money"", createat, resultat, rewardat, updateat, ip, ""language"", currency, istest, resettlement, oddstype, odds, cateid, category, ""content"", ""result"", matchid, ""match"", betid, bet, pre_betamount, pre_betmoney, pre_money, league) 
VALUES(@summary_id, @orderid, @username, @type, @status, @betamount, @betmoney, @money, @createat, @resultat, @rewardat, @updateat, @ip, @language, @currency, @istest, @resettlement, @oddstype, @odds, @cateid, @category, @content, @result, @matchid, @match, @betid, @bet, @pre_betamount, @pre_betmoney, @pre_money, @league);";
            return await conn.ExecuteAsync(sql, betLogs, tran);
        }

        /// <summary>
        /// 寫入資料
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="tran"></param>
        /// <param name="betLogs"></param>
        /// <returns></returns>
        public async Task<int> PostKSRunningRecord(NpgsqlConnection conn, IDbTransaction tran, IEnumerable<Record> betLogs)
        {
            var sql = @"INSERT INTO public.t_ks_bet_record_running 
(summary_id,club_id,franchiser_id, orderid, username, ""type"", status, betamount, betmoney, ""money"", createat, resultat, rewardat, updateat, ip, ""language"", currency, istest, resettlement, oddstype, odds, cateid, category, ""content"", ""result"", matchid, ""match"", betid, bet, pre_betamount, pre_betmoney, pre_money, league) 
VALUES(@summary_id,@club_id,@franchiser_id, @orderid, @username, @type, @status, @betamount, @betmoney, @money, @createat, @resultat, @rewardat, @updateat, @ip, @language, @currency, @istest, @resettlement, @oddstype, @odds, @cateid, @category, @content, @result, @matchid, @match, @betid, @bet, @pre_betamount, @pre_betmoney, @pre_money, @league);";
            return await conn.ExecuteAsync(sql, betLogs, tran);
        }

        /// <summary>
        /// 刪除未結算
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="tran"></param>
        /// <param name="record_data"></param>
        /// <returns></returns>
        public async Task<int> DeleteKSRunningRecord(NpgsqlConnection conn, IDbTransaction tran, Record record_data)
        {
            string strSqlDel = @"DELETE FROM t_ks_bet_record_running
                               WHERE orderid=@orderid";
            return await conn.ExecuteAsync(strSqlDel, record_data, tran);
        }


        public async Task<List<GetKSRecordsRunningBySummaryResponse>> GetKSRunningRecord(GetBetRecordUnsettleReq RecordReq)
        {
            var par = new DynamicParameters();
            string strSql = @"SELECT  orderid
                        , status
                        , createat
                        , updateat
                        , cateid
                        , league
                        , ""match""
                        , TRIM(split_part(REPLACE(REPLACE(""match"", ' vs ', ' VS ' ), '-', ' ' ),' VS ', 1)) as hometeam
                        , TRIM(split_part(REPLACE(REPLACE(""match"", ' vs ', ' VS ' ), '-', ' ' ),' VS ', 2)) as awayteam
                        , rewardat
                        , betamount
                        , content 
                        , odds
                        , money
                        , type
                        , club_id
                        , franchiser_id
                    FROM t_ks_bet_record_running
                    WHERE createat BETWEEN @StartTime AND @EndTime";

            if (RecordReq.Club_id != null)
            {
                par.Add("@Club_id", RecordReq.Club_id);
                strSql += " AND Club_id = @Club_id";
            }
            if (RecordReq.Franchiser_id != null)
            {
                par.Add("@Franchiser_id", RecordReq.Franchiser_id);
                strSql += " AND Franchiser_id = @Franchiser_id";
            }
            par.Add("@StartTime", RecordReq.StartTime != null ? RecordReq.StartTime : System.DateTime.Now.AddDays(-100));
            par.Add("@EndTime", RecordReq.EndTime != null ? RecordReq.EndTime : System.DateTime.Now);
            using (NpgsqlConnection conn = new NpgsqlConnection(await PGRead))
            {
                var result = await conn.QueryAsync<GetKSRecordsRunningBySummaryResponse>(strSql, par);
                return result.ToList();
            }
        }


        /// <summary>
        /// 每日匯總
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public async Task<(int totalcount, decimal totalbetvalid, decimal totalnetwin)> SumKSBetRecordByBetTime(System.DateTime start, System.DateTime end)
        {
            var sql = @"SELECT 
                    COUNT(distinct orderid) AS totalcount
                    , CASE WHEN SUM(betamount) IS NULL THEN 0 ELSE SUM(betamount) END  AS totalbetvalid
                    , CASE WHEN SUM(money) IS NULL THEN 0 ELSE SUM(money) END AS totalnetwin
                    FROM t_ks_bet_record
                    WHERE rewardat >= @startTime 
                        AND rewardat < @endTime";

            var par = new DynamicParameters();
            par.Add("@startTime", start);
            par.Add("@endTime", end);

            await using var conn = new NpgsqlConnection(await PGRead);
            var result = (await conn.QueryAsync<dynamic>(sql, par)).FirstOrDefault();
            return ((int)result.totalcount, (decimal)result.totalbetvalid, (decimal)result.totalnetwin);
        }
        /// <summary>
        /// 取得時間內的住單 (比對重複單)
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public async Task<List<Record>> GetKSRecordsBytime(System.DateTime start, System.DateTime end)
        {
            var sql = @"SELECT orderid ,createat,status,rewardat,updateat
                    FROM public.t_ks_bet_record 
                    WHERE createat >= @startTime 
                        AND createat < @endTime";
            var par = new DynamicParameters();
            par.Add("@startTime", start);
            par.Add("@endTime", end);

            await using var conn = new NpgsqlConnection(await PGRead);
            var result = await conn.QueryAsync<Record>(sql, par);
            return result.ToList();
        }

        /// <summary>
        /// 取得GUID資料
        /// </summary>
        /// <param name="RecordReq"></param>
        /// <returns></returns>
        public async Task<List<GetKSRecordsBySummaryResponse>> GetKSRecordsBySummary(GetBetRecordReq RecordReq)
        {
            var sql = @"SELECT orderid
                        , status
                        , createat
                        , updateat
                        , cateid
                        , league
                        , ""match""
                        , TRIM(split_part(REPLACE(REPLACE(""match"", ' vs ', ' VS ' ), '-', ' ' ),' VS ', 1)) as hometeam
                        , TRIM(split_part(REPLACE(REPLACE(""match"", ' vs ', ' VS ' ), '-', ' ' ),' VS ', 2)) as awayteam
                        , rewardat
                        , betamount
                        , content 
                        , odds
                        , money
                        , type
                    FROM public.t_ks_bet_record 
                    WHERE createat >= @start 
                        AND createat <= @end
                        AND summary_id = @summaryId::uuid";
            var param = new
            {
                summaryId = RecordReq.summary_id,
                start = RecordReq.ReportTime,
                end = RecordReq.ReportTime.AddDays(1),
            };

            await using var conn = new NpgsqlConnection(await PGRead);
            var result = await conn.QueryAsync<GetKSRecordsBySummaryResponse>(sql, param);
            return result.ToList();
        }


        /// <summary>
        /// 使用orderid住單號取得資料
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="tran"></param>
        /// <param name="orderid"></param>
        /// <param name="createat"></param>
        /// <returns></returns>
        public async Task<List<Record>> GetKSRecords(NpgsqlConnection conn, IDbTransaction tran, string orderid, System.DateTime createat)
        {
            string strSql = @"SELECT orderid , status , createat , updateat , rewardat,pre_betamount,pre_betmoney,pre_money FROM t_ks_bet_record
                    WHERE orderid = @orderid and createat >= @start 
                        AND createat <= @end";
            var parameters = new DynamicParameters();
            parameters.Add("@orderid", orderid);
            parameters.Add("@start", createat);
            parameters.Add("@end", createat.AddDays(1));

            var result = await conn.QueryAsync<Record>(strSql, parameters, tran);
            return result.ToList();
        }
    }
}
