using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace H1_ThirdPartyWalletAPI.Model.Game.WM
{
    public class WM_Mapping
    {

        #region Mapping Function
        private static string OutPutClassTostring<T>(T openListBaccModel)
        {
            var toDic = openListBaccModel.GetType()
          .GetProperties()
               .ToDictionary(prop => prop.Name, prop => (string)prop.GetValue(openListBaccModel, null));
            string outputString = string.Join(", ", toDic.Select(x => $"{x.Key}: {x.Value}"));
            return outputString;
        }

        public static List<object> RCG_MappingFunc(int gid, string gameresult, OpenListModelBase modelBase)
        {
            List<object> olist = new List<object>();
            Object MappingResult = new();
            switch (gid)
            {
                case 101:
                    MappingResult = RCGBaccMapping(gid, gameresult);
                    break;
                case 102:
                    MappingResult = RCGLongHuMapping(gid, gameresult);
                    break;
                case 103:
                    MappingResult = RCGlunpanMapping(gid, gameresult);
                    break;
                case 104:
                case 110:
                    MappingResult = RCGshaiziMapping(gid, gameresult);
                    break;
                case 105:
                    MappingResult = RCGbullbullMapping(gid, gameresult);
                    break;
                case 107:
                    MappingResult = RCGfantanMapping(gid, gameresult);
                    break;
                case 108:
                    MappingResult = RCGsedieMapping(gid, gameresult);
                    break;
                //case 110:
                //    MappingResult = RCGyuxiaxieMapping(gid, gameresult);
                //    break;
                case 128:
                    MappingResult = RCGandarbaharMapping(gid, gameresult);
                    break;
            }

            //這段在去重VideoUrls
            var resObjects = MappingResult as OpenListModelBase;

            if (resObjects != null)
            {
                resObjects.NoActive = modelBase.NoActive;
                resObjects.NoRun = modelBase.NoRun;
                resObjects.ServerId = modelBase.ServerId;
                resObjects.DateTime = modelBase.DateTime;
            }
            olist.Add(MappingResult);
            return olist;
        }

        private static object RCGandarbaharMapping(int gid, string gameresult)
        {
            //1,1,5
            //gameresult = "The council canceled";
            OpenListAnDarBaHarModel request = new OpenListAnDarBaHarModel();
            if (gameresult == "The council canceled")
            {
                request.Cancel = "1";
                return request;
            }

            gameresult = gameresult.Replace("A", "1").Replace("10", "A");
            var _wmPaiSplit = gameresult.Split(" ");


            #region PaiList

            string Joker = "";
            List<string> twoList = new List<string>();
            List<string> threeList = new List<string>();

            string[] result = new string[3] { "", "", "" };
            if (_wmPaiSplit.Length == 3)
            {
                var _Joker = gameresult.Split(" ")[0].Split(":")[1];
                var Andar = gameresult.Split(" ")[1].Split(":")[1];
                var Bahar = gameresult.Split(" ")[2].Split(":")[1];

                Joker = MapToCardType(_Joker[0]) + _Joker[1];
                for (int i = 0; i < Andar.Length; i += 2)
                {
                    twoList.Add(MapToCardType(Andar[i]) + Andar[i + 1]);
                }

                for (int i = 0; i < Bahar.Length; i += 2)
                {
                    threeList.Add(MapToCardType(Bahar[i]) + Bahar[i + 1]);
                }
            }
            else
            {
                throw new Exception(string.Format("{2} Length Error gid:{0},gameresult{1}", gid, gameresult, MethodBase.GetCurrentMethod().DeclaringType));
            }
            #endregion



            request.One = Joker;
            request.Two = string.Join(":", twoList);
            request.Three = string.Join(":", threeList);
            request.Cancel = "0";


            var jokerPoint = GetPoint(request.One);
            var andarPoint = GetLastPoint(request.Two);
            var baharPoint = GetLastPoint(request.Three);

            var winFlag = JudgeWinFlag(jokerPoint, andarPoint, baharPoint);

            request.WinFlag = winFlag.ToString();


            return request;
        }

        private static object RCGyuxiaxieMapping(int gid, string gameresult)
        {
            //1,1,5
            //gameresult = "The council canceled";
            OpenListShaiZiModel request = new OpenListShaiZiModel();
            if (gameresult == "The council canceled")
            {
                request.Cancel = "1";
                return request;
            }



            #region PaiList
            var result = new string[3] { "", "", "" };

            var _wmPaiSplit = gameresult.Split(",").Select(x => x.ToString()).ToList();

            if (_wmPaiSplit.Count == 3)
            {
                result[0] = _wmPaiSplit[0];
                result[1] = _wmPaiSplit[1];
                result[2] = _wmPaiSplit[2];
            }
            else
            {
                throw new Exception(string.Format("{2} Error gid:{0},gameresult{1}", gid, gameresult, MethodBase.GetCurrentMethod().DeclaringType));
            }
            #endregion


            request.One = result[0];
            request.Two = result[1];
            request.Three = result[2];
            request.Cancel = "0";


            return request;
        }

        private static object RCGsedieMapping(int gid, string gameresult)
        {
            //gameresult = "The council canceled";
            OpenListSeDieModel request = new OpenListSeDieModel();
            if (gameresult == "The council canceled")
            {
                request.Cancel = "1";
                return request;
            }

            #region PaiList

            var result = new string[1] { "" };

            if (!string.IsNullOrEmpty(gameresult))
            {
                //00：0紅(4白)
                //01：1紅(3白)
                //02：2紅(2白)
                //03：3紅(1白)
                //04：4紅(0白)

                switch (gameresult)
                {
                    case "4White":
                        result[0] = "00";
                        break;
                    case "3White1Red":
                        result[0] = "01";
                        break;
                    case "2Red2White":
                        result[0] = "02";
                        break;
                    case "3Red1White":
                        result[0] = "03";
                        break;
                    case "4Red":
                        result[0] = "04";
                        break;
                }


            }
            else
            {
                throw new Exception(string.Format("{2} Error gid:{0},gameresult{1}", gid, gameresult, MethodBase.GetCurrentMethod().DeclaringType));
            }
            #endregion


            request.Number = result[0];
            request.Cancel = "0";


            return request;
        }

        private static object RCGfantanMapping(int gid, string gameresult)
        {
            //gameresult = "The council canceled";
            OpenListFanTanModel request = new OpenListFanTanModel();
            if (gameresult == "The council canceled")
            {
                request.Cancel = "1";
                return request;
            }



            #region PaiList
            var result = new string[1] { "" };

            if (!string.IsNullOrEmpty(gameresult))
            {
                result[0] = gameresult.PadLeft(2, '0');
            }
            else
            {
                throw new Exception(string.Format("{2} Error gid:{0},gameresult{1}", gid, gameresult, MethodBase.GetCurrentMethod().DeclaringType));
            }
            #endregion


            request.Number = result[0];
            request.Cancel = "0";


            return request;
        }

        private static object RCGbullbullMapping(int gid, string gameresult)
        {

            OpenListBullBullModel request = new OpenListBullBullModel();
            if (gameresult == "The council canceled")
            {
                request.Cancel = "1";
                return request;
            }


            // gameresult = "Banker:♣A,♠6,♦3,♣K,♠4 Player1:♥5,♣6,♠A,♦K,♦6 Player2:♥6,♣4,♦9,♥2,♣3 Player3:♠J,♥9,♥K,♣5,♥10";
            #region PaiList
            var result = new string[20] { "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "" };
            var _wmPaiSplit = gameresult.Split(" ");

            if (_wmPaiSplit.Length == 4)
            {
                gameresult = gameresult.Replace("A", "1").Replace("10", "A");

                var _Player1 = gameresult.Split(" ")[1].Split(":")[1];
                var _Player2 = gameresult.Split(" ")[2].Split(":")[1];
                var _Player3 = gameresult.Split(" ")[3].Split(":")[1];
                var _Banker = gameresult.Split(" ")[0].Split(":")[1];
                int r = 0;


                for (int i = 0; i < _Player1.Length; i += 3)
                {
                    result[r] = MapToCardType(_Player1[i]) + _Player1[i + 1];
                    r++;
                }
                for (int i = 0; i < _Player2.Length; i += 3)
                {
                    result[r] = MapToCardType(_Player2[i]) + _Player2[i + 1];
                    r++;
                }
                for (int i = 0; i < _Player3.Length; i += 3)
                {
                    result[r] = MapToCardType(_Player3[i]) + _Player3[i + 1];
                    r++;
                }

                for (int i = 0; i < _Banker.Length; i += 3)
                {
                    result[r] = MapToCardType(_Banker[i]) + _Banker[i + 1];
                    r++;
                }

            }
            else
            {
                throw new Exception(string.Format("{2} Error gid:{0},gameresult{1}", gid, gameresult, MethodBase.GetCurrentMethod().DeclaringType));
            }
            #endregion


            // [0]: 牌型 / [1]: 牌1 / [2]: 牌2 / [3]: 牌3 / [4]: 牌4 / [5]: 牌5
            string[] Player1, Player2, Player3, Banker;
            int flag1, flag2, flag3;

            Player1 = new string[6];
            Player2 = new string[6];
            Player3 = new string[6];
            Banker = new string[6];


            request.One = "";
            request.Two = result[0];
            request.Three = result[1];
            request.Four = result[2];
            request.Five = result[3];
            request.Six = result[4];
            request.Seven = result[5];
            request.Eight = result[6];
            request.Nine = result[7];
            request.Ten = result[8];
            request.Eleven = result[9];
            request.Twelve = result[10];
            request.Thirteen = result[11];
            request.Fourteen = result[12];
            request.Fifteen = result[13];
            request.Sixteen = result[14];
            request.Seventeen = result[15];
            request.Eighteen = result[16];
            request.Nineteen = result[17];
            request.Twenty = result[18];
            request.TwentyOne = result[19];

            #region 各閒莊家牌
            Player1[1] = request.Two;
            Player1[2] = request.Three;
            Player1[3] = request.Four;
            Player1[4] = request.Five;
            Player1[5] = request.Six;

            Player2[1] = request.Seven;
            Player2[2] = request.Eight;
            Player2[3] = request.Nine;
            Player2[4] = request.Ten;
            Player2[5] = request.Eleven;

            Player3[1] = request.Twelve;
            Player3[2] = request.Thirteen;
            Player3[3] = request.Fourteen;
            Player3[4] = request.Fifteen;
            Player3[5] = request.Sixteen;

            Banker[1] = request.Seventeen;
            Banker[2] = request.Eighteen;
            Banker[3] = request.Nineteen;
            Banker[4] = request.Twenty;
            Banker[5] = request.TwentyOne;
            #endregion

            #region 判斷牌型
            JudgeCardType(Player1[1], Player1[2], Player1[3], Player1[4], Player1[5], out Player1[0]);
            JudgeCardType(Player2[1], Player2[2], Player2[3], Player2[4], Player2[5], out Player2[0]);
            JudgeCardType(Player3[1], Player3[2], Player3[3], Player3[4], Player3[5], out Player3[0]);
            JudgeCardType(Banker[1], Banker[2], Banker[3], Banker[4], Banker[5], out Banker[0]);
            #endregion

            //判斷勝負
            flag1 = JudgeWinFlag(Player1, Banker);
            flag2 = JudgeWinFlag(Player2, Banker);
            flag3 = JudgeWinFlag(Player3, Banker);


            request.BankerResult = Banker[0];
            request.Player1Result = Player1[0];
            request.Player2Result = Player2[0];
            request.Player3Result = Player3[0];
            request.IsPlayer1Win = flag1.ToString();
            request.IsPlayer2Win = flag2.ToString();
            request.IsPlayer3Win = flag3.ToString();



            request.Cancel = "0";


            return request;
        }

        private static object RCGshaiziMapping(int gid, string gameresult)
        {
            //1,1,5
            //gameresult = "The council canceled";
            OpenListShaiZiModel request = new OpenListShaiZiModel();
            if (gameresult == "The council canceled")
            {
                request.Cancel = "1";
                return request;
            }



            #region PaiList
            var result = new string[3] { "", "", "" };

            var _wmPaiSplit = gameresult.Split(",").Select(x => x.ToString()).ToList();

            if (_wmPaiSplit.Count == 3)
            {
                result[0] = _wmPaiSplit[0];
                result[1] = _wmPaiSplit[1];
                result[2] = _wmPaiSplit[2];
            }
            else
            {
                throw new Exception(string.Format("{2} Error gid:{0},gameresult{1}", gid, gameresult, MethodBase.GetCurrentMethod().DeclaringType));
            }
            #endregion


            request.One = result[0];
            request.Two = result[1];
            request.Three = result[2];
            request.Cancel = "0";


            return request;
        }

        private static object RCGBaccMapping(int gid, string gameresult)
        {
            var result = new string[6] { "", "", "", "", "", "" };
            //gameresult = "The council canceled";
            OpenListBaccModel request = new OpenListBaccModel();
            if (gameresult == "The council canceled")
            {
                request.Cancel = "1";
                return request;
            }



            //gameresult = "Banker:♥6♠8♥2 Player:♣9♣5♦7";
            gameresult = gameresult.Replace("A", "1").Replace("10", "A");
            var _wmPaiSplit = gameresult.Split(" ");

            if (_wmPaiSplit.Length != 2)
            {
                throw new Exception(string.Format("{2} Length Error gid:{0},gameresult{1}", gid, gameresult, MethodBase.GetCurrentMethod().DeclaringType));
            }


            #region PaiList



            if (_wmPaiSplit.Length == 2)
            {
                var player = gameresult.Split(" ")[1].Split(":")[1];
                var banker = gameresult.Split(" ")[0].Split(":")[1];

                for (int i = 0; i < player.Length; i += 2)
                {
                    result[i] = MapToCardType(player[i]) + player[i + 1];
                }

                for (int i = 0; i < banker.Length; i += 2)
                {
                    result[i + 1] = MapToCardType(banker[i]) + banker[i + 1];
                }
            }
            #endregion
            request.One = result[0];
            request.Two = result[1];
            request.Three = result[2];
            request.Four = result[3];
            request.Five = result[4];
            request.Six = result[5];



            int winFlag = -1;
            float pointXian = -1;
            float pointZhuang = -1;
            int XDD = -1;
            int ZDD = -1;

            XDD = JudgePair(request.One, request.Three) ? 1 : 0;
            ZDD = JudgePair(request.Two, request.Four) ? 1 : 0;

            PointSum(request.One, request.Three, request.Five, out pointXian);
            PointSum(request.Two, request.Four, request.Six, out pointZhuang);

            winFlag = JudgeWinFlag(pointXian, pointZhuang);


            request.Zhuang = pointZhuang.ToString();
            request.Xian = pointXian.ToString();
            request.WinFlag = winFlag.ToString();
            request.Cancel = "0";

            return request;
        }

        private static object RCGLongHuMapping(int gid, string gameresult)
        {
            var result = new string[6] { "", "", "", "", "", "" };
            //gameresult = "The council canceled";
            OpenListLongHuModel request = new OpenListLongHuModel();
            if (gameresult == "The council canceled")
            {
                request.Cancel = "1";
                return request;
            }



            //gameresult = "Banker:♥6♠8♥2 Player:♣9♣5♦7";
            gameresult = gameresult.Replace("A", "1").Replace("10", "A");
            var _wmPaiSplit = gameresult.Split(" ");

            if (_wmPaiSplit.Length != 2)
            {
                throw new Exception(string.Format("{2} Length Error gid:{0},gameresult{1}", gid, gameresult, MethodBase.GetCurrentMethod().DeclaringType));
            }


            #region PaiList



            if (_wmPaiSplit.Length == 2)
            {
                var player = gameresult.Split(" ")[1].Split(":")[1];
                var banker = gameresult.Split(" ")[0].Split(":")[1];

                for (int i = 0; i < player.Length; i += 2)
                {
                    result[i] = MapToCardType(player[i]) + player[i + 1];
                }

                for (int i = 0; i < banker.Length; i += 2)
                {
                    result[i + 1] = MapToCardType(banker[i]) + banker[i + 1];
                }
            }
            #endregion



            request.One = result[0];
            request.Two = result[1];


            int winFlag = -1;
            float HuPoint = -1;
            float LongPoint = -1;

            ParsePoint(request.One, out HuPoint);
            ParsePoint(request.Two, out LongPoint);

            winFlag = JudgeWinFlag(HuPoint, LongPoint);


            request.Long = LongPoint.ToString();
            request.Hu = HuPoint.ToString();
            request.WinFlag = winFlag.ToString();
            request.Cancel = "0";


            return request;
        }

        private static object RCGlunpanMapping(int gid, string gameresult)
        {

            //gameresult = "The council canceled";
            OpenListLunPanModel request = new OpenListLunPanModel();
            if (gameresult == "The council canceled")
            {
                request.Cancel = "1";
                return request;
            }

            if (!double.TryParse(gameresult, out double _wmPai))
            {
                throw new Exception(string.Format("{2} Error gid:{0},gameresult{1}", gid, gameresult, MethodBase.GetCurrentMethod().DeclaringType));
            }


            #region PaiList
            var result = new string[1] { "" };

            if (!string.IsNullOrEmpty(gameresult))
            {
                result[0] = gameresult.PadLeft(2, '0');

                if (gameresult == "0")
                {
                    result[0] = "37";
                }
                else
                {
                    result[0] = gameresult.PadLeft(2, '0');
                }
            }
            #endregion

            request.Number = result[0];
            request.Cancel = "0";


            return request;
        }


        /// <summary>
        /// 牌型轉換點數
        /// </summary>
        /// <param name="pai"> 牌1代碼 </param>
        /// <param name="point"> 點數 </param>
        /// <returns></returns>
        public static bool ParsePoint(string pai, out float point)
        {
            point = 0;

            try
            {
                if (string.IsNullOrEmpty(pai) == true)
                {
                    return false;
                }
                else
                {
                    string strPoint = pai.Substring(1, 1);

                    switch (strPoint)
                    {
                        case "A":
                            point = 10;
                            break;

                        case "J":
                            point = 11;
                            break;

                        case "Q":
                            point = 12;
                            break;

                        case "K":
                            point = 13;
                            break;

                        default:
                            point = int.Parse(strPoint);
                            break;
                    }
                }
            }
            catch
            {
                return false;
            }

            return true;
        }



        public static string MapToCardType(char input)
        {
            var result = "";
            switch (input)
            {
                case '♥':
                    result = "1";
                    break;

                case '♠':
                    result = "2";
                    break;

                case '♦':
                    result = "3";
                    break;

                case '♣':
                    result = "4";
                    break;
            }

            return result;
        }


        /// <summary>
        /// 牌型轉換總和點數
        /// </summary>
        /// <param name="pai1"> 牌1代碼 </param>
        /// <param name="pai2"> 牌2代碼 </param>
        /// <param name="pai3"> 牌3代碼 </param>
        /// <param name="pointSum"> 總點數 </param>
        /// <returns></returns>
        public static bool PointSum(string pai1, string pai2, string pai3, out float pointSum)
        {
            pointSum = 0;

            try
            {
                //牌1與牌2不可為空
                if (string.IsNullOrEmpty(pai1) || string.IsNullOrEmpty(pai2))
                {
                    return false;
                }

                if (string.IsNullOrEmpty(pai3) == true)
                {
                    pai3 = "00";
                }

                pai1 = pai1.Substring(1, 1);
                pai2 = pai2.Substring(1, 1);
                pai3 = pai3.Substring(1, 1);

                if (pai1 == "A" || pai1 == "J" || pai1 == "Q" || pai1 == "K")
                {
                    pai1 = "0";
                }

                if (pai2 == "A" || pai2 == "J" || pai2 == "Q" || pai2 == "K")
                {
                    pai2 = "0";
                }

                if (pai3 == "A" || pai3 == "J" || pai3 == "Q" || pai3 == "K")
                {
                    pai3 = "0";
                }

                int ip1 = 0;
                int ip2 = 0;
                int ip3 = 0;

                if (int.TryParse(pai1, out ip1) == false)
                {
                    return false;
                }

                if (int.TryParse(pai2, out ip2) == false)
                {
                    return false;
                }

                if (int.TryParse(pai3, out ip3) == false)
                {
                    ip3 = 0;
                }

                pointSum = (ip1 + ip2 + ip3) % 10;
            }
            catch
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// 判斷是否為對子
        /// </summary>
        /// <param name="pai1"> 牌1 </param>
        /// <param name="pai2"> 牌2 </param>
        /// <returns></returns>
        private static bool JudgePair(string pai1, string pai2)
        {
            if (pai1.Substring(1, 1) == pai2.Substring(1, 1))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// 判斷勝負旗標
        /// </summary>
        /// <param name="pointXian"> 閒家點數 </param>
        /// <param name="pointZhuang"> 莊家點數 </param>
        /// <returns></returns>
        private static int JudgeWinFlag(float pointXian, float pointZhuang)
        {
            int flag = -1;

            if (pointXian != -1 && pointZhuang != -1)
            {
                //比較牌型
                if (pointXian > pointZhuang)
                {
                    flag = 1;
                }

                if (pointXian < pointZhuang)
                {
                    flag = 2;
                }

                if (pointXian == pointZhuang)
                {
                    flag = 3;
                }
            }

            return flag;
        }



        /// <summary>
        /// 判斷牛牛牌型
        /// </summary>
        /// <param name="pai1"> 牌1 </param>
        /// <param name="pai2"> 牌2 </param>
        /// <param name="pai3"> 牌3 </param>
        /// <param name="pai4"> 牌4 </param>
        /// <param name="pai5"> 牌5 </param>
        /// <param name="cardType"> 牌型代碼 </param>
        /// <returns></returns>
        public static bool JudgeCardType(string pai1, string pai2, string pai3, string pai4, string pai5, out string cardType)
        {
            cardType = "B0";

            //牌型完整才進行判斷
            if (string.IsNullOrEmpty(pai1) == false && string.IsNullOrEmpty(pai2) == false &&
                string.IsNullOrEmpty(pai3) == false && string.IsNullOrEmpty(pai4) == false && string.IsNullOrEmpty(pai5) == false)
            {
                try
                {
                    int point1, point2, point3, point4, point5;
                    int type1, type2, type3, type4, type5;

                    CodeToData(pai1, out point1, out type1);
                    CodeToData(pai2, out point2, out type2);
                    CodeToData(pai3, out point3, out type3);
                    CodeToData(pai4, out point4, out type4);
                    CodeToData(pai5, out point5, out type5);

                    // 判斷是否為五公
                    if (point1 >= 11 && point2 >= 11 && point3 >= 11 && point4 >= 11 && point5 >= 11)
                    {
                        cardType = "FF";
                    }
                    else
                    {
                        // 是否有牛
                        bool Bull = false;

                        List<int> rankPointList = new List<int>()
                        {
                            // 超過10點時則視為10點
                            point1 > 10 ? 10 : point1,
                            point2 > 10 ? 10 : point2,
                            point3 > 10 ? 10 : point3,
                            point4 > 10 ? 10 : point4,
                            point5 > 10 ? 10 : point5
                        };

                        int totalRankPoint = rankPointList.Sum();
                        int totalPointRemainder = totalRankPoint % 10;

                        for (int indexFornt = 0; indexFornt < rankPointList.Count - 1; indexFornt++)
                        {
                            for (int indexBack = indexFornt + 1; indexBack < rankPointList.Count; indexBack++)
                            {
                                int pairPoint = rankPointList[indexFornt] + rankPointList[indexBack];

                                int pairPointRemainder = pairPoint % 10;

                                if (pairPointRemainder == totalPointRemainder)
                                {
                                    Bull = true;

                                    break;
                                }
                            }

                            if (Bull)
                            {
                                break;
                            }
                        }

                        if (Bull == true)
                        {
                            switch (totalPointRemainder)
                            {
                                case 0:
                                    cardType = "BB";
                                    break;

                                case 1:
                                    cardType = "B1";
                                    break;

                                case 2:
                                    cardType = "B2";
                                    break;

                                case 3:
                                    cardType = "B3";
                                    break;

                                case 4:
                                    cardType = "B4";
                                    break;

                                case 5:
                                    cardType = "B5";
                                    break;

                                case 6:
                                    cardType = "B6";
                                    break;

                                case 7:
                                    cardType = "B7";
                                    break;

                                case 8:
                                    cardType = "B8";
                                    break;

                                case 9:
                                    cardType = "B9";
                                    break;

                                default:
                                    cardType = "B0";
                                    break;
                            }
                        }
                        else
                        {
                            // 無牛
                            cardType = "BN";
                        }
                    }
                }
                catch
                {
                    return false;
                }

            }

            return true;
        }

        /// <summary>
        /// 判斷勝負旗標
        /// </summary>
        /// <param name="pointXian"> 閒家點數 </param>
        /// <param name="pointZhuang"> 莊家點數 </param>
        /// <returns></returns>
        private static int JudgeWinFlag(string[] player, string[] banker)
        {
            int flag = -1;

            if (player[0] != "B0" && banker[0] != "B0")
            {
                // 五公點數大小 (小 ~ 大)
                string[] array_FF = new string[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13" };

                // 牛牛牌型大小 (小 ~ 大)
                string[] array_BB = new string[] { "B0", "B1", "B2", "B3", "B4", "B5", "B6", "B7", "B8", "B9", "BB" };

                // 牌1 ~ 牌5 [ 花色 , 點數 ]
                int[,] card_P = new int[5, 2];
                int[,] card_B = new int[5, 2];

                // 點數陣列
                int[] point_P = new int[5];
                int[] point_B = new int[5];

                // 花色陣列
                int[] type_P = new int[5];
                int[] type_B = new int[5];

                // 紀錄牌型點數 & 花色
                CodeToData(player[1], out card_P[0, 1], out card_P[0, 0]);
                CodeToData(player[2], out card_P[1, 1], out card_P[1, 0]);
                CodeToData(player[3], out card_P[2, 1], out card_P[2, 0]);
                CodeToData(player[4], out card_P[3, 1], out card_P[3, 0]);
                CodeToData(player[5], out card_P[4, 1], out card_P[4, 0]);

                CodeToData(banker[1], out card_B[0, 1], out card_B[0, 0]);
                CodeToData(banker[2], out card_B[1, 1], out card_B[1, 0]);
                CodeToData(banker[3], out card_B[2, 1], out card_B[2, 0]);
                CodeToData(banker[4], out card_B[3, 1], out card_B[3, 0]);
                CodeToData(banker[5], out card_B[4, 1], out card_B[4, 0]);

                // 存成點數陣列
                for (int i = 0; i < 5; i++)
                {
                    point_P[i] = card_P[i, 1];
                    point_B[i] = card_B[i, 1];

                    type_P[i] = card_P[i, 0];
                    type_B[i] = card_B[i, 0];
                }

                // 五公判斷
                if (player[0] == "FF" || banker[0] == "FF")
                {
                    if (player[0] == banker[0])
                    {
                        // 都五公則比較最大牌
                        flag = CompareCard(point_P, point_B, type_P, type_B);
                    }
                    else if (player[0] == "FF")
                    {
                        flag = 1;
                    }
                    else
                    {
                        flag = 0;
                    }
                }
                else
                {
                    // 牌型的索引
                    int index_P = Array.IndexOf(array_BB, player[0]);
                    int index_B = Array.IndexOf(array_BB, banker[0]);

                    if (index_P == index_B)
                    {
                        // 同牌型則比較最大牌
                        flag = CompareCard(point_P, point_B, type_P, type_B);
                    }
                    else if (index_P > index_B)
                    {
                        flag = 1;
                    }
                    else
                    {
                        flag = 0;
                    }
                }
            }

            return flag;
        }

        /// <summary>
        /// 代碼轉換
        /// </summary>
        /// <param name="strCode"></param>
        protected static void CodeToData(string strCode, out int num, out int type)
        {
            num = 0;
            type = 0;

            if (string.IsNullOrEmpty(strCode) == false)
            {
                if (strCode.Length == 2)
                {
                    switch (strCode.Substring(1, 1))
                    {
                        case "A":
                            num = 10;
                            break;

                        case "J":
                            num = 11;
                            break;

                        case "Q":
                            num = 12;
                            break;

                        case "K":
                            num = 13;
                            break;

                        default:
                            //牌點代碼需為1 ~ 9、AJQK
                            if (int.TryParse(strCode.Substring(1, 1), out num) == false)
                            {
                                num = -1;
                            }
                            break;
                    }

                    int.TryParse(strCode.Substring(0, 1), out int typeTest);

                    //牌色代碼需為1 ~ 4
                    if (typeTest >= 1 && typeTest <= 4)
                    {
                        type = typeTest - 1;
                    }
                    else
                    {
                        type = -1;
                    }
                }
                else
                {
                    type = -1;
                }
            }
        }

        /// <summary>
        /// 比較閒莊的大小花色順位並回傳勝負
        /// </summary>
        /// <param name="player"> 閒家各牌點數 </param>
        /// <param name="banker"> 莊家各牌點數 </param>
        /// <param name="cardName_Player"> 閒家牌型 </param>
        /// <param name="cardName_Banker"> 莊家牌型 </param>
        /// <returns></returns>
        public static int CompareCard(int[] point_P, int[] point_B, int[] type_P, int[] type_B)
        {
            int flag = -1;

            // 花色大小 (小 ~ 大) [3: 方塊 / 4: 梅花 / 1: 紅心 / 2:黑桃]
            int[] array_Type = new int[] { 3, 4, 1, 2 };

            // 最大點數
            int pointMax_P;
            int pointMax_B;

            // 最大點數的索引位置
            int indexMax_P;
            int indexMax_B;

            // 花色的索引位置
            int indexTypeMax_P;
            int indexTypeMax_B;

            //取得最大點數牌及索引
            pointMax_P = Max(point_P, type_P, out indexMax_P);
            pointMax_B = Max(point_B, type_B, out indexMax_B);

            if (pointMax_P == pointMax_B)
            {
                //從最大點數的牌索引取出花色並比對出花色大小索引
                indexTypeMax_P = Array.IndexOf(array_Type, type_P[indexMax_P] + 1);
                indexTypeMax_B = Array.IndexOf(array_Type, type_B[indexMax_B] + 1);

                if (indexTypeMax_P > indexTypeMax_B)
                {
                    flag = 1;
                }
                else
                {
                    flag = 0;
                }
            }
            else if (pointMax_P > pointMax_B)
            {
                flag = 1;
            }
            else
            {
                flag = 0;
            }

            return flag;
        }


        /// <summary>
        /// 陣列取最大值
        /// </summary>
        /// <param name="pointArray"> 整數陣列 </param>
        /// <returns></returns>
        public static int Max(int[] pointArray, int[] typeArray, out int indexMax)
        {
            // 花色大小 (小 ~ 大) [3: 方塊 / 4: 梅花 / 1: 紅心 / 2:黑桃]
            int[] array_Type = new int[] { 3, 4, 1, 2 };

            int value = 0;
            int index = -1;
            indexMax = 0;

            foreach (int x in pointArray)
            {
                index++;

                if (x > value)
                {
                    value = x;

                    indexMax = index;
                }
                else if (x == value)
                {
                    //最大牌有1張以上的情況
                    int typeMax_New = Array.IndexOf(array_Type, typeArray[index] + 1);
                    int typeMax_Old = Array.IndexOf(array_Type, typeArray[indexMax] + 1);

                    if (typeMax_New > typeMax_Old)
                    {
                        indexMax = index;
                    }
                }
            }

            return value;
        }




        /// <summary>
        /// 取得牌點數
        /// </summary>
        /// <param name="card"></param>
        /// <returns></returns>
        private static string GetPoint(string card)
        {
            //取得點數
            //花色 + 點數
            if (card.Length != 2)
            {
                return "";
            }
            else
            {
                var cardPoint = card.Substring(1, 1);

                return cardPoint;
            }
        }

        /// <summary>
        /// 取得陣列字串的最後一張牌點數
        /// </summary>
        /// <param name="cards"></param>
        /// <returns></returns>
        private static string GetLastPoint(string cards)
        {
            // input "12:43:56"
            // output "6"

            var splitCard = cards.Split(':');
            var lastCardIndex = splitCard.Length - 1;

            if (lastCardIndex < 0)
            {
                return "";
            }

            var lastCard = splitCard[lastCardIndex];
            var lastCardPoint = GetPoint(lastCard);

            return lastCardPoint;
        }

        /// <summary>
        /// 判斷勝負
        /// </summary>
        /// <param name="jokerPoint"></param>
        /// <param name="andarPoint"></param>
        /// <param name="baharPoint"></param>
        /// <returns></returns>
        private static int JudgeWinFlag(string jokerPoint, string andarPoint, string baharPoint)
        {
            //都有牌
            if (!string.IsNullOrEmpty(jokerPoint) && !string.IsNullOrEmpty(andarPoint) && !string.IsNullOrEmpty(baharPoint))
            {
                if (jokerPoint == andarPoint)
                {
                    //安達
                    return 1;
                }

                if (jokerPoint == baharPoint)
                {
                    //巴哈
                    return 2;
                }
            }

            //只有小丑、安達
            if (!string.IsNullOrEmpty(jokerPoint) && !string.IsNullOrEmpty(andarPoint) && string.IsNullOrEmpty(baharPoint))
            {
                if (jokerPoint == andarPoint)
                {
                    //安達
                    return 1;
                }
            }

            //未勝負
            return 0;
        }
        #endregion

        #region RCG Class Model
        /// <summary>
        /// 開牌記錄基底
        /// </summary>
        public class OpenListModelBase
        {
            public string ServerId { get; set; }
            public DateTime? DateTime { get; set; }
            public string NoRun { get; set; }
            public string NoActive { get; set; }

            public OpenListVideoModleBase Video { get; set; }

        }

        public class OpenListVideoModleBase
        {
            public string VideoOne { get; set; }
            public string VideoTwo { get; set; }
            public string VideoThree { get; set; }
        }

        /// <summary>
        /// 百家 / 區塊百家
        /// </summary>
        public class OpenListBaccModel : OpenListModelBase
        {

            public string One { get; set; }
            public string Two { get; set; }
            public string Three { get; set; }
            public string Four { get; set; }
            public string Five { get; set; }
            public string Six { get; set; }
            public string Zhuang { get; set; }
            public string Xian { get; set; }
            public string Cancel { get; set; }
            public string WinFlag { get; set; }
        }

        /// <summary>
        /// 保險百家
        /// </summary>
        public class OpenListInsuBaccModel : OpenListBaccModel
        {
            public string InsuFlag { get; set; }
            public string InsuId { get; set; }
            public string ZDD { get; set; }
            public string XDD { get; set; }
        }
        /// <summary>
        /// 安達巴哈
        /// </summary>
        public class OpenListAnDarBaHarModel : OpenListModelBase
        {
            public string One { get; set; }
            public string Two { get; set; }
            public string Three { get; set; }
            public string Cancel { get; set; }
            public string WinFlag { get; set; }
        }

        /// <summary>
        /// 區塊射龍門
        /// </summary>
        public class OpenListBcSddModel : OpenListModelBase
        {
            public string One { get; set; }
            public string Two { get; set; }
            public string Three { get; set; }
            public string Cancel { get; set; }
            public string WinFlag { get; set; }
        }

        /// <summary>
        /// 牛牛
        /// </summary>
        public class OpenListBullBullModel : OpenListModelBase
        {
            public string One { get; set; }
            public string Two { get; set; }
            public string Three { get; set; }
            public string Four { get; set; }
            public string Five { get; set; }
            public string Six { get; set; }
            public string Seven { get; set; }
            public string Eight { get; set; }
            public string Nine { get; set; }
            public string Ten { get; set; }
            public string Eleven { get; set; }
            public string Twelve { get; set; }
            public string Thirteen { get; set; }
            public string Fourteen { get; set; }
            public string Fifteen { get; set; }
            public string Sixteen { get; set; }
            public string Seventeen { get; set; }
            public string Eighteen { get; set; }
            public string Nineteen { get; set; }
            public string Twenty { get; set; }
            public string TwentyOne { get; set; }
            public string BankerResult { get; set; }
            public string Player1Result { get; set; }
            public string Player2Result { get; set; }
            public string Player3Result { get; set; }
            public string IsPlayer1Win { get; set; }
            public string IsPlayer2Win { get; set; }
            public string IsPlayer3Win { get; set; }
            public string Cancel { get; set; }
        }

        /// <summary>
        /// 番攤
        /// </summary>
        public class OpenListFanTanModel : OpenListModelBase
        {
            public string Number { get; set; }
            public string Cancel { get; set; }
        }

        /// <summary>
        /// 泰國骰
        /// </summary>
        public class OpenListHiLoModel : OpenListModelBase
        {
            public string One { get; set; }

            public string Two { get; set; }

            public string Three { get; set; }

            public string Cancel { get; set; }


        }
        /// <summary>
        /// 龍虎 / 區塊龍虎
        /// </summary>
        public class OpenListLongHuModel : OpenListModelBase
        {
            public string One { get; set; }
            public string Two { get; set; }
            public string Hu { get; set; }
            public string Long { get; set; }
            public string Cancel { get; set; }
            public string WinFlag { get; set; }
        }
        /// <summary>
        /// 輪盤
        /// </summary>
        public class OpenListLunPanModel : OpenListModelBase
        {
            public string Number { get; set; }
            public string Cancel { get; set; }
        }
        /// <summary>
        /// 博丁
        /// </summary>
        public class OpenListPokDengModel : OpenListModelBase
        {
            public string One { get; set; }
            public string Two { get; set; }
            public string Three { get; set; }
            public string Four { get; set; }
            public string Five { get; set; }
            public string Six { get; set; }
            public string Seven { get; set; }
            public string Eight { get; set; }
            public string Nine { get; set; }
            public string Ten { get; set; }
            public string Eleven { get; set; }
            public string Twelve { get; set; }
            public string P_1 { get; set; }
            public string P_2 { get; set; }
            public string P_3 { get; set; }
            public string P_4 { get; set; }
            public string P_5 { get; set; }
            public string B_1 { get; set; }
            public string WinFlag_P1 { get; set; }
            public string WinFlag_P2 { get; set; }
            public string WinFlag_P3 { get; set; }
            public string WinFlag_P4 { get; set; }
            public string WinFlag_P5 { get; set; }
            public string WinFlag_B { get; set; }
            public string Cancel { get; set; }

        }
        /// <summary>
        /// 賽車
        /// </summary>
        public class OpenListRgRacingModel : OpenListModelBase
        {
            public string One { get; set; }
            public string Two { get; set; }
            public string Three { get; set; }
            public string Four { get; set; }
            public string Five { get; set; }
            public string Six { get; set; }
            public string Seven { get; set; }
            public string Eight { get; set; }
            public string Nine { get; set; }
            public string Ten { get; set; }
            public string Cancel { get; set; }

        }
        /// <summary>
        /// 三寶
        /// </summary>
        public class OpenListSamBoModel : OpenListModelBase
        {
            public string Card01 { get; set; }
            public string Card02 { get; set; }
            public string Card03 { get; set; }
            public string Card04 { get; set; }
            public string Card05 { get; set; }
            public string Card06 { get; set; }
            public string Card07 { get; set; }
            public string Card08 { get; set; }
            public string Card09 { get; set; }
            public string Card10 { get; set; }
            public string Card11 { get; set; }
            public string Card12 { get; set; }
            public string Player1Result { get; set; }
            public string Player2Result { get; set; }
            public string Player3Result { get; set; }
            public string BankerResult { get; set; }
            public string WinFlag_P1 { get; set; }
            public string WinFlag_P2 { get; set; }
            public string WinFlag_P3 { get; set; }
            public string Cancel { get; set; }


        }
        /// <summary>
        /// 色碟
        /// </summary>
        public class OpenListSeDieModel : OpenListModelBase
        {
            public string Number { get; set; }
            public string Cancel { get; set; }

        }
        /// <summary>
        /// 骰子
        /// </summary>
        public class OpenListShaiZiModel : OpenListModelBase
        {
            public string One { get; set; }
            public string Two { get; set; }
            public string Three { get; set; }
            public string Cancel { get; set; }
        }

        #endregion
    }
}
