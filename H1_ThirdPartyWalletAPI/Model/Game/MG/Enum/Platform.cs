namespace H1_ThirdPartyWalletAPI.Model.Game.MG.Enum
{
    public enum Platform
    {
        UnKnown,
        Desktop,
        Mobile
    }
    public enum BetStatus
    {
        Closed,
        Canceled
    }
    public enum TransactionType
    {
        Deposit,
        Withdraw
    }
    public enum TransactionStatus
    {
        Succeeded, Inprogress, Unconfrmed, Failed
    }
    public enum TransationDetailStatus
    {
        Succeeded, GameInProgress, Unavailable
    }
    public enum TimeAggregation
    {
        Monthly, 
        Daily, 
        Hourly
    }
    public enum ProductResponseStatus
    {
        Success, Failure, NotSupported, Partial
    }
}
