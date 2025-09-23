using System;
using System.Collections.Generic;  
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using APISietemasdereservas.Models.Request;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Cors;
using System.IdentityModel.Tokens.Jwt;

namespace APISietemasdereservas.Controllers
{
    //[Route("api/[controller]")]
   [ApiController]
    public class LoginController : ControllerBase
    {
         
        private IConfiguration _config;

        public LoginController(IConfiguration config)
        {
            _config = config;
        }
        [EnableCors("CorsApi")]
        [HttpPost, Route("api/tours/v1.0/Login")]
        public Dictionary<String, Object> Login(UserModel users)
        {
            Dictionary<string, object> respues = new Dictionary<string, object>();
            dynamic ResulToken = new List<string>();
            try
            {
                if (users.Username.ToString().Trim().Equals(""))
                    throw new Exception("El Usuario Del Administrador No Puede Estar Vacio");
                if (users.Password.ToString().Trim().Equals(""))
                    throw new Exception("La Contraseña Del Administrador No Puede Estar Vacio");
                UserModel Login = new UserModel { };
                Login.Username = users.Username;
                Login.Password = users.Password;
                var user = AuthenticateUser(Login);
                if (user != null)
                {
                    ResulToken = GenerateJSONWebToken(user);
                    respues.Add("Error", false);
                    respues.Add("Mensaje", "");
                }
                else
                {
                    throw new Exception("Error Los Datos No Coinciden Con La Del Administrador");
                }

            }
            catch (Exception ex)
            {
                respues.Add("Error", true);
                respues.Add("Mensaje", ex.Message);
            }
            respues.Add("Respuesta", ResulToken);

            return respues;

        }

        private UserModel AuthenticateUser(UserModel Login)
        {
            UserModel user = null;
            if (Login.Username == "Sidecilsas" && Login.Password == "Sidecilsas")
            {
                user = new UserModel { Username = "Sidecilsas", EmailAddress = "info@Sidecil.com.co", Password = "Sidecilsas" };

            }else if(Login.Username == "Sidecilsas" && Login.Password == "Sidecilsas")
            {
                user = new UserModel { Username = "Sidecilsas", EmailAddress = "info@Sidecil.com.co", Password = "Sidecilsas" };
            }

            return user;
        }


        private string GenerateJSONWebToken(UserModel userinfo)
        {
            dynamic claims = System.String.Empty;
            claims = new List<Claim>
                {
                    new Claim(JwtRegisteredClaimNames.Sub, userinfo.Username),
                    new Claim(JwtRegisteredClaimNames.Email, userinfo.EmailAddress),
                    new Claim(JwtRegisteredClaimNames.Jti,Guid.NewGuid().ToString()),
                    new Claim(ClaimTypes.Role, "Admin2024Sidecil")
                };

            var key = new SymmetricSecurityKey(
      Encoding.UTF8.GetBytes("estesessunssecretossuperslargosdes32sbytess2024s")
  );

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var key1 = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes("1234567890ABCDEF") // exactamente 16 chars = 128 bits
            );



            var encryptingCreds = new EncryptingCredentials(key1, SecurityAlgorithms.Aes128KW, SecurityAlgorithms.Aes128CbcHmacSha256);
            var handler = new JwtSecurityTokenHandler();
            var t = handler.CreateJwtSecurityToken();
            var token = handler.CreateJwtSecurityToken("http://localhost:61768/", "http://localhost:61768/"
                , new ClaimsIdentity(claims)
                , expires: DateTime.Now.AddMinutes(10)
                , signingCredentials: creds
                , encryptingCredentials: encryptingCreds
                , notBefore: DateTime.Now
                , issuedAt: DateTime.Now);
            return new JwtSecurityTokenHandler().WriteToken(token);


        }

    }
}

 