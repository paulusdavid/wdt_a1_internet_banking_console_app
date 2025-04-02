using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLib.Data.Models
{
    public class Login
    {
        public required string LoginID { get; set; }

        public required int CustomerID { get; set; }

        public required string PasswordHash { get; set; }
    }
}
