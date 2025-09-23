using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Web;

namespace APISietemasdereservas.Models.Request
{
    public class UserLogIn
    {

        [JsonPropertyName("user")]
        public string user { get; set; }
        [JsonPropertyName("contrase")]
        public string contrase { get; set; }


    }

    public class UserRegister
    {
        public string user { get; set; }
        public string contrase { get; set; }
        public string email { get; set; }
        public string nombre { get; set; }
        public string ciudad { get; set; }
    }

    public class ResetUserRegister
    {
        public string email { get; set; }
    }

    public class resert
    {
        public string email { get; set; }
        public string otpcode { get; set; }
        public string password { get; set; }
    }

}
