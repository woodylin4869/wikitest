using System;

namespace H1_ThirdPartyWalletAPI.Model.Game.JDB.Exceptions
{
    #region Exceptions
    public class JDBBadRequestException : Exception
    {
        public JDBBadRequestException(string _status, string _err_text) : base(_err_text)
        {
            status = _status;
            err_text = _err_text;
        }

        public string status { get; set; }

        public string err_text { get; set; }
    }
    public static class JDBErrorParser
    {
        public static string Parse(string status)
        {
            switch (status)
            {
                case "0":
                    return "0000";
                default:
                    throw new Exception(string.Format("not support Exception: JDBErrorPaser Status {0}", status));
            }
        }
    }
    #endregion

}
