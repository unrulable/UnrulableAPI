public class TransactionInfo
{
    public string TransactionId { get; set; }
    public decimal Amount { get; set; }
    public bool IsOutgoing { get; set; }
    public string ReceivingAddress { get; set; }
}