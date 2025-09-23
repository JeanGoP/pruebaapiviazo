using APISietemasdereservas.Models.Request;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Search;
using MailKit.Security;
using MimeKit;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace APISietemasdereservas.Services
{
    public class MailService
    {
        public async Task<List<object>> LeerCorreosAsync(MailRequest request)
        {
            var correos = new List<object>();

            using var client = new ImapClient();
            client.ServerCertificateValidationCallback = (s, c, h, e) => true;
                        
            await client.ConnectAsync(request.HostImap, request.Puerto, SecureSocketOptions.SslOnConnect);
            await client.AuthenticateAsync(request.Correo, request.Password);

            var inbox = client.Inbox;
            await inbox.OpenAsync(FolderAccess.ReadOnly);

            SearchQuery query = SearchQuery.All;

            if (request.RemitentesFiltro?.Any() == true)
            {
                var remitenteQueries = request.RemitentesFiltro
                    .Where(r => !string.IsNullOrWhiteSpace(r))
                    .Select(r => SearchQuery.FromContains(r))
                    .ToList();

                SearchQuery remitenteQuery = null;

                foreach (var q in remitenteQueries)
                {
                    remitenteQuery = remitenteQuery == null ? q : remitenteQuery.Or(q);
                }

                if (remitenteQuery != null)
                {
                    query = SearchQuery.And(query, remitenteQuery);
                }
            }

            if (request.FechaInicio.HasValue)
            {
                query = query.And(SearchQuery.DeliveredAfter(request.FechaInicio.Value.AddDays(-1)));
            }

            if (request.FechaFin.HasValue)
            {
                query = query.And(SearchQuery.DeliveredBefore(request.FechaFin.Value.AddDays(1)));
            }

            var uids = await inbox.SearchAsync(query);

            foreach (var uid in uids)
            {
                var message = await inbox.GetMessageAsync(uid);

                correos.Add(new
                {
                    message.Subject,
                    De = message.From.ToString(),
                    Fecha = message.Date.ToString("yyyy-MM-dd HH:mm:ss"),
                    Cuerpo = message.TextBody ?? message.HtmlBody ?? "(Sin cuerpo)",
                    actividad = Regex.Match(message.TextBody, @"\*Actividad:\* (.+)").Groups[1].Value,
                    reserva = Regex.Match(message.TextBody, @"\*Número de reserva:\* (\d+)").Groups[1].Value,
                    ciudad = Regex.Match(message.TextBody, @"\*Ciudad:\* (.+)").Groups[1].Value,
                    idioma = Regex.Match(message.TextBody, @"\*Idioma:\* (.+)").Groups[1].Value,
                    Para = message.To.ToString(),
                    Cc = message.Cc.ToString(),
                    ConAdjuntos = message.Attachments?.Any() == true, 
                });
            }

            await client.DisconnectAsync(true);

            return correos;
        }



    }
}
