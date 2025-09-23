using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace APISietemasdereservas.Models.Request
{
    public class UserModel
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string EmailAddress { get; set; }
        public string rol { get; set; }
    }
}
