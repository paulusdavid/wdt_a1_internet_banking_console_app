using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace CommonLib.Data.Models
{
    public class Account
    {
        public required int AccountNumber { get; set; }

        public required char AccountType { get; set; }

        public required int CustomerID { get; set; }

        public required decimal Balance { get; set; }
    }
}
