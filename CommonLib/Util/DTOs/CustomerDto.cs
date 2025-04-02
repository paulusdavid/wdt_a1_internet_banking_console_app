using CommonLib.Util.DTOs;
namespace Assignment1.Util.DTOs
{
    public class CustomerDto
    {
        public required int CustomerID { get; set; }
        public required string Name { get; set; }
        public required string Address { get; set; }
        public required string City { get; set; }
        public required string PostCode { get; set; }
        public List<AccountDto> Accounts { get; set; }
        public LoginDto Login { get; set; }
    }
}