using ThirdPartyWallet.Share.Model.Game.CR.Enum;

namespace ThirdPartyWallet.Share.Model.Game.CR.Response
{
    public class ALLWagerResponse : ApiResponseBase
    {
        /// <summary>
        /// 代理名稱
        /// </summary>
        public string agname { get; set; }

        /// <summary>
        /// 兌現更新注單資料 詳見4.12
        /// </summary>
        public object[] wager_cashout { get; set; }


        /// <summary>
        /// 當前頁次
        /// </summary>
        public int page { get; set; }

        /// <summary>
        /// 代理商編號
        /// </summary>
        public string agid { get; set; }

        /// <summary>
        /// 總頁數
        /// </summary>
        public int wager_totalpage { get; set; }

        public List<Wager_Data> wager_data { get; set; }

    }

    public class Wager_Data
    {
        public string parlaynum { get; set; }

        public Dictionary<string, Parlaysub> parlaysub { get; set; }

        /// <summary>
        /// 交易單的日期時間 例：2017-06-21 11:36:54 
        /// </summary>
        public DateTime adddate { get; set; }

        /// <summary>
        /// 交易單序號
        /// </summary>
        public string cashoutid { get; set; }

        /// <summary>
        /// 兌現金額 (會員貨幣)
        /// </summary>
        public decimal cashout { get; set; }

        /// <summary>
        /// 兌現金額 (站別貨幣)
        /// </summary>
        public decimal cashout_d { get; set; }

        /// <summary>
        /// 幣別 詳見4.7
        /// </summary>
        public string currency { get; set; }

        /// <summary>
        /// 實際扣除額 (會員貨幣)
        /// </summary>
        public decimal degold { get; set; }

        /// <summary>
        /// 實際扣除額 (站別貨幣)
        /// </summary>
        public decimal degold_d { get; set; }

        /// <summary>
        /// 下注金額 (會員貨幣)
        /// </summary>
        public decimal gold { get; set; }


        /// <summary>
        /// 下注金額 (站別貨幣)
        /// </summary>
        public decimal gold_d { get; set; }

        /// <summary>
        /// 球類(FT:足球,BK:籃球/美足,TN:網球, BS:棒球,OP:其他, VF:虛擬足球,SK:台球,MT:跨球類過關, FS:冠軍,VB:排球) 
        /// </summary>
        public string gtype { get; set; }

        /// <summary>
        /// 1:單盤 2:雙盤
        /// </summary>
        public string handicap { get; set; }

        /// <summary>
        /// 交易單序號
        /// </summary>
        public string id { get; set; }

        /// <summary>
        /// 交易單IP 
        /// </summary>
        public string IP { get; set; }

        /// <summary>
        /// 交易單當下使用的賠率
        /// </summary>

        public decimal? ioratio { get; set; }


        /// <summary>
        /// 聯盟名稱 
        /// </summary>
        public string league { get; set; }

        /// <summary>
        /// 會員編號 
        /// </summary>
        public string mid { get; set; }

        /// <summary>
        /// 球頭
        /// </summary>
        public string oddsFormat { get; set; }


        /// <summary>
        /// 盤口類型(H=香港,M=馬來,I=印尼, E = 歐洲) 
        /// </summary>
        public string odds { get; set; }

        /// <summary>
        /// 開賽日期
        /// </summary>
        public string orderdate { get; set; }


        /// <summary>
        /// 玩法細項 
        /// </summary>
        public string order { get; set; }

        /// <summary>
        /// 開賽時間 
        /// </summary>

        public string ordertime { get; set; }


        /// <summary>
        /// 節次名稱
        /// </summary>
        public string pname { get; set; }

        /// <summary>
        /// 測試
        /// </summary>

        public string report_test { get; set; }

        /// <summary>
        /// 玩法 
        /// </summary>
        public string rtype { get; set; }


        /// <summary>
        /// 玩法原始資料
        /// </summary>
        public string rtypecode { get; set; }

        /// <summary>
        /// 未有結果滾球當下比分
        /// </summary>

        public string score { get; set; }

        /// <summary>
        /// 0:無結果 1:有結果
        /// </summary>
        public string settle { get; set; }

        /// <summary>
        /// 強弱 H：主隊為強隊 C：客隊為強隊  ‘’：不提供 （使用於讓球） 
        /// </summary>
        public string strong { get; set; }

        /// <summary>
        /// 客隊名稱
        /// </summary>

        public string tname_away { get; set; }

        /// <summary>
        /// 主隊名稱 
        /// </summary>

        public string tname_home { get; set; }

        /// <summary>
        /// 會員名稱
        /// </summary>

        public string username { get; set; }

        /// <summary>
        ///有效金額 
        /// </summary>
        public decimal vgold { get; set; }

        /// <summary>
        /// 有效金額 (會員貨幣)
        /// </summary>
        public decimal members_vgold { get; set; }

        /// <summary>
        /// 詳見 4.8結果詳細 資料
        /// </summary>
        public string resultdetail { get; set; }


        /// <summary>
        /// “0” 未有結果 “L”輸 “Ｗ”贏 “Ｐ”合 “D” 取消 “Ａ”還原
        /// </summary>
        public string result { get; set; }

        /// <summary>
        /// 算結果的日期時間 例：2017-06-21 11:36:54 ，重新結算資料會更新，未結算單程式改寫下注時間
        /// </summary>
        public DateTime? resultdate { get; set; }

        /// <summary>
        /// 有結果全場比分 
        /// </summary>
        public string result_score { get; set; }

        /// <summary>
        /// 玩法
        /// </summary>
        public string wtype { get; set; }

        /// <summary>
        /// 玩法原始資料
        /// </summary>

        public string wtypecode { get; set; }

        /// <summary>
        /// settle:0 可贏金額 settle:1 輸贏金額 皆含本金 (站別貨幣)
        /// </summary>
        public decimal wingold_d { get; set; }

        /// <summary>
        /// settle:0 可贏金額 settle:1 輸贏金額 皆含本金 (會員貨幣)
        /// </summary>
        public decimal wingold { get; set; }

        #region db Model

        /// <summary>
        /// 當前建立時間
        /// </summary>
        public DateTime? Create_time { get; set; }

        /// <summary>
        /// 彙總帳時間
        /// </summary>
        public DateTime? Report_time { get; set; }

        /// <summary>
        /// 下注總額(前一狀態)
        /// </summary>
        public decimal Pre_gold { get; set; }

        /// <summary>
        /// 有效投注(前一狀態)
        /// </summary>
        public decimal Pre_degold { get; set; }

        /// <summary>
        /// 玩家贏分
        /// </summary>
        public decimal Pre_wingold { get; set; }

        /// <summary>
        /// Club_id (running表)
        /// </summary>
        public string Club_id { get; set; }

        /// <summary>
        /// Franchiser_id (running表)
        /// </summary>
        public string Franchiser_id { get; set; }

        #endregion db Model


        /// <summary>
        /// 串關筆數
        /// </summary>
        public int parlaysubCount { get; set; }
    }



    public class Parlaysub
    {
        public string oddsFormat { get; set; }
        public string strong { get; set; }
        public string gtype { get; set; }
        public string tname_away { get; set; }
        public string pname { get; set; }
        public string league { get; set; }
        public string ioratio { get; set; }
        public string rtypecode { get; set; }
        public string orderdate { get; set; }
        public string ordertime { get; set; }
        public string report_test { get; set; }
        public string rtype { get; set; }
        public string score { get; set; }
        public string wtype { get; set; }
        public string tname_home { get; set; }
        public string resultdetail { get; set; }
        public string wtypecode { get; set; }
        public string result_score { get; set; }
        public string order { get; set; }
    }


}