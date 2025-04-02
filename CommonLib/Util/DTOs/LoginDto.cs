using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assignment1.Util.DTOs
{
    public class LoginDto
    {
        public required string LoginID { get; set; }
        public required string PasswordHash { get; set; }
    }
}