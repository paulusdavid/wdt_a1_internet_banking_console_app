using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLib.Data.Models
{
    public class Transaction
    {
        public int TransactionID { get; set; }

        public required char TransactionType { get; set; }

        public required int AccountNumber { get; set; }

        public int? DestinationAccountNumber { get; set; }

        public required decimal Amount { get; set; }

        public string Comment { get; set; }

        public required DateTime TransactionTimeUtc { get; set; }
    }
}
