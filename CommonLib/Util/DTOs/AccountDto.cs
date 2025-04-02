using Assignment1.Util.DTOs;
namespace CommonLib.Util.DTOs
{
    public class AccountDto
    {
        public required int AccountNumber { get; set; }
        public required char AccountType { get; set; }
        public List<TransactionDto> Transactions { get; set; }
    }
}