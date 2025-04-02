namespace Assignment1.Util.DTOs
{
    public class TransactionDto
    {
        public required decimal Amount { get; set; }
        public string Comment { get; set; }
        public required string TransactionTimeUtc { get; set; }
    }
}
