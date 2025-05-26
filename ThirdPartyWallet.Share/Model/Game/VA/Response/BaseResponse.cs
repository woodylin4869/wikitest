namespace ThirdPartyWallet.Share.Model.Game.VA.Response
{
    public class BaseResponse<T>
    {
        public T Data { get; set; }
        public Status Status { get; set; }

        public BaseResponse(T data, Status status)
        {
            Data = data;
            Status = status;
        }
    }

    public class Status
    {
        public int Code { get; set; }
        public string Message { get; set; }
        public DateTime DateTime { get; set; }
        public string TraceCode { get; set; }

        public Status(int code, string message, DateTime dateTime, string traceCode)
        {
            Code = code;
            Message = message;
            DateTime = dateTime;
            TraceCode = traceCode;
        }
    }

}
