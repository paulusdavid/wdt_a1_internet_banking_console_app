using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLib.Data.Models
{
    public class Customer
    {
        public required int CustomerID { get; set; }

        public required string Name { get; set; }

        public required string Address { get; set; }

        public required string City { get; set; }

        public required string PostCode { get; set; }

        public List<Login> Logins { get; set; }
        public List<Account> Accounts { get; set; }
    }
}
