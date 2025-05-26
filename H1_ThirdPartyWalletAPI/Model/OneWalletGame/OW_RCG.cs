using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using H1_ThirdPartyWalletAPI.Code;

namespace H1_ThirdPartyWalletAPI.Model.OneWalletGame
{
    public class OW_RCG
    {
        public enum TokenType
        {
            AuthToken,
            SessionToken
        }
    }
    public class ResponseBaseMessage<T>
    {
        public int MsgId { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }
        public long Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        public ResponseBaseMessage()
        {

        }
        public ResponseBaseMessage(T responseResult)
        {
            this.MsgId = (int)ResponseCode.Success;
            this.Message = MessageCode.Message[(int)ResponseCode.Success];
            this.Data = responseResult;
        }
        public ResponseBaseMessage(T responseResult, long timestamp)
        {
            this.MsgId = (int)ResponseCode.Success;
            this.Message = MessageCode.Message[(int)ResponseCode.Success];
            this.Data = responseResult;
            this.Timestamp = timestamp;
        }
        /// <summary>
        /// cutstom MsgId and Message
        /// </summary>
        /// <param name="responseResult"></param>
        /// <param name="errorType"></param>
        /// <param name="message"></param>
        public ResponseBaseMessage(T responseResult, int errorType, string message)
        {
            this.MsgId = (int)errorType;
            this.Message = message;
            this.Data = responseResult;
        }
        /// <summary>
        /// custom Message
        /// </summary>
        /// <param name="responseResult"></param>
        /// <param name="message"></param>
        public ResponseBaseMessage(T responseResult, string message)
        {
            this.MsgId = (int)ResponseCode.Success;
            this.Message = message;
            this.Data = responseResult;
        }

    }
    public class CheckUserRequest
    {
        public string SystemCode { get; set; }
        public string WebId { get; set; }
        public string Account { get; set; }
        public string Token { get; set; }
        public string RequestId { get; set; }
    }
    public class CheckUserResponse
    {
        public string RequstId { get; set; }
        public string Account { get; set; }
        public string AuthToken { get; set; }
        public string SessionToken { get; set; }
    }
    public class RequestExtendTokenRequest
    {
        public string RequestId { get; set; }
    }
    public class RequestExtendTokenResponse
    {
        public string SessionToken { get; set; }
        public string RequestId { get; set; }
    }
    public class GetBalanceResponse
    {
        public int Status { get; set; }
        public string RequestId { get; set; }
        public string Account { get; set; }
        public decimal Balance { get; set; }

    }
    public class GetBalanceRequest
    {
        public string Account { get; set; }

        public string SystemCode { get; set; }

        public string WebId { get; set; }
        public string RequestId { get; set; }

    }
    public class TransactionResponse
    {
        public TransactionResponse()
        {
        }
        public TransactionResponse(string id, decimal balance)
        {
            Id = id;
            Balance = balance;
        }
        public string Id { get; set; }

        public decimal Balance { get; set; }
    }
    public class DebitRequest
    {
        public string SystemCode { get; set; }

        public string WebId { get; set; }

        public string Account { get; set; }

        public GameInfo Game { get; set; }

        public TransactionInfo Transaction { get; set; }

        public string RequestId { get; set; }

    }
    public class DebitResponse
    {

        public DebitResponse() { }
        public DebitResponse(string requestId, string account, string transactionId, decimal balance)
        {
            RequstId = requestId;
            Account = account;
            Transaction = new TransactionResponse(transactionId, balance);
        }

        public string RequstId { get; set; }
        public string Account { get; set; }
        public TransactionResponse Transaction { get; set; }

    }
    public class CreditResponse
    {
        public CreditResponse() { }
        public CreditResponse(string requestId, string memberId, string transactionId, decimal balance)
        {
            RequestId = requestId;
            Account = memberId;
            Transaction = new TransactionResponse(transactionId, balance);

        }

        public string RequestId { get; set; }

        public string Account { get; set; }

        public TransactionResponse Transaction { get; set; }

    }
    public class CreditRequest
    {
        public string SystemCode { get; set; }

        public string WebId { get; set; }

        public string Account { get; set; }

        public GameInfo Game { get; set; }

        public TransactionInfo Transaction { get; set; }

        public string RequestId { get; set; }
    }
    public class CancelRequest
    {
        public string SystemCode { get; set; }

        public string WebId { get; set; }

        public string Account { get; set; }

        public GameInfo Game { get; set; }

        public CancelTransactionInfo Transaction { get; set; }

        public string RequestId { get; set; }
    }
    public class CancelResponse
    {

        public CancelResponse() { }
        public CancelResponse(string requestId, string account, string transactionId, decimal balance)
        {
            RequstId = requestId;
            Account = account;
            Transaction = new TransactionResponse(transactionId, balance);
        }

        public string RequstId { get; set; }
        public string Account { get; set; }
        public TransactionResponse Transaction { get; set; }
    }
    public class TransactionInfo
    {
        public string Id { get; set; }

        public decimal Amount { get; set; }

        public string ReferenceId { get; set; }
    }
    public class CancelTransactionInfo
    {
        public string Id { get; set; }
        public string TargetId { get; set; }
    }
    public class GameInfo
    {
        public string DeskId { get; set; }
        public string GameName { get; set; }
        public string Shoe { get; set; }
        public string Run { get; set; }
    }
}
