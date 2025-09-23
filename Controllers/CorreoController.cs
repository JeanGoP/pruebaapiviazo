using APISietemasdereservas.Models.Request;
using APISietemasdereservas.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace APISietemasdereservas.Controllers
{
    [EnableCors("CorsApi")]
    [ApiController]
    [Authorize]
    public class CorreoController : ControllerBase
    {
        [HttpPost]
        [Route("api/mails/v1.0.1/ObtenMails")]
        [AllowAnonymous]
        public async Task<IActionResult> ListadoCorreo([FromBody] MailRequest request)
        {
            var service = new MailService();
            var correos = await service.LeerCorreosAsync(request);
            return Ok(new { correos });
        }


    }
}
