using APISietemasdereservas.Models.Request;
using Google.Apis.Auth;
using J_W.Estructura;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration; 
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Xsl;

namespace APISietemasdereservas.Controllers
{
   
    public static class MethodsCompile
    {

        private static  IConfiguration _config;
        private static readonly string connectionString;
        private static readonly string rutasFileSaves;
        private static readonly string rutasFileImages;

        //public static MethodsCompile()
        //{
        //    _config = config;
        //    connectionString = config.GetSection("ConnectionStrings").GetSection("dbConnection").Value;
        //    rutasFileSaves = config.GetSection("Configuracion").GetSection("rutaImages").Value;
        //    rutasFileImages = config.GetSection("Configuracion").GetSection("rutaImages").Value;
        //}

        static Dbase dbase = new Dbase();
        //OTPManager otpManager = new OTPManager();
        public class ConfiguracionCorreo
        { 
            public string Email { get; set; } 
            public string LinkPagina { get; set; } 
            public string CodigodeInstagram { get; set; }
        }
        static IConfigurationRoot GetConfiguration()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

            return builder.Build();

        }
        public static Result ObtenerDatos()
        {
            try
            {
                var configuracion = GetConfiguration();
                var conxion = configuracion.GetSection("ConnectionStrings").GetSection("dbConnection").Value;
                dbase.Conexion = conxion;


                Result result = dbase.Procedure("[GS].[ST_ObtenerConfig]").RunRow();

                if (!result.Error)
                {  
                    return (result);
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public static object SaveLogCorreo(string message, string error, string correo)
        {
            try
            {
                var configuracion = GetConfiguration();
                var conxion = configuracion.GetSection("ConnectionStrings").GetSection("dbConnection").Value;
                dbase.Conexion = conxion;

                Result result = dbase.Procedure("[GS].[ST_LogCorreoSave]", "@mensaje:VARCHAR:100", message, "@error:VARCHAR:100", error, "@correo:VARCHAR:100", correo).RunData();

                return result;
            }
            catch (Exception ex)
            { 
                return null;
            }
        }

        public static bool MensajePorDifusion(string Email, string Mensaje, string NombreCliente, string Id_reserva, string NombreGuru, string Idioma, string LinkWeb)
        {
            try
            {
                Result ConfigInit = ObtenerDatos(); 
                
                
                Mensaje = Mensaje.Replace("<br>", "<br/>").Replace("<BR>", "<br/>");

                string saludo = "";
                string mensajeInicio = "";
                string mensajeFinal = "";
                string subject = "";

                // Determinar el idioma y personalizar el contenido
                if (Idioma == "es")  // Si el idioma es español
                {
                    saludo = $"Hola <strong>{SecurityElement.Escape(NombreCliente)}</strong>,";
                    mensajeInicio = $"Tu gurú <strong>{SecurityElement.Escape(NombreGuru)}</strong> te ha enviado un mensaje respecto a tu reserva <strong>#{SecurityElement.Escape(Id_reserva)}</strong>:";
                    mensajeFinal = "¡Gracias por confiar en nosotros!<br/>Equipo de Atención al Cliente";
                    subject = $"Mensaje de tu Gurú sobre la reserva #{Id_reserva}";
                }
                else if (Idioma == "en")  // Si el idioma es inglés
                {
                    saludo = $"Hello <strong>{SecurityElement.Escape(NombreCliente)}</strong>,";
                    mensajeInicio = $"Your guru <strong>{SecurityElement.Escape(NombreGuru)}</strong> has sent you a message regarding your reservation <strong>#{SecurityElement.Escape(Id_reserva)}</strong>:";
                    mensajeFinal = "Thank you for trusting us!<br/>Customer Service Team";
                    subject = $"Message from your Guru regarding reservation #{Id_reserva}";
                }
                else
                {
                    // Si el idioma no es reconocido, usar el español por defecto
                    saludo = $"Hola <strong>{SecurityElement.Escape(NombreCliente)}</strong>,";
                    mensajeInicio = $"Tu gurú <strong>{SecurityElement.Escape(NombreGuru)}</strong> te ha enviado un mensaje respecto a tu reserva <strong>#{SecurityElement.Escape(Id_reserva)}</strong>:";
                    mensajeFinal = "¡Gracias por confiar en nosotros!<br/>Equipo de Atención al Cliente";
                    subject = $"Mensaje de tu Gurú sobre la reserva #{Id_reserva}";
                }
                 
                string htmlBody = $@"
                <html>
                    <body style='font-family: Arial, sans-serif; color: #333;'>
                        <h2>{(Idioma == "es" ? "Mensaje de tu Gurú" : "Message from your Guru")}</h2>

                        <p>
                            {saludo}
                        </p>

                        <p>
                            {mensajeInicio}
                        </p>

                        {Mensaje}

                        <p>
                            {(Idioma == "es"
                                                ? "Si deseas responder o necesitas más información, puedes contactarnos escribiendo en nuestro sitio web en el menú de Mensajes o respondiendo a este correo."
                                                : "If you wish to reply or need more information, you can contact us by writing on our website in the Messages menu or replying to this email.")}
                            <a href='{LinkWeb}' style='color: #4CAF50; text-decoration: none; font-weight: bold;'>
                                {(Idioma == "es" ? "VER MIS MENSAJES" : "SEE MY MESSAGES")}
                            </a>.
                        </p>

                        <p style='margin-top: 20px;'>
                            {mensajeFinal}
                        </p>
                    </body>
                </html>";

                string email = ConfigInit.Row["UsuarioCorreo"].ToString();
                string nombreplatrom = ConfigInit.Row["NombrePlatForm"].ToString();
                string CodigodeInstagram = ConfigInit.Row["CodigodeInstagram"].ToString();
                string proveedorCorreo = ConfigInit.Row["proveedorCorreo"].ToString();
                var puerto = Convert.ToInt32(ConfigInit.Row["puerto"]);
                bool EnableSsl = Convert.ToBoolean(ConfigInit.Row["EnableSsl"]);
                // Enviar el correo
                var fromAddress = new MailAddress(email, nombreplatrom);
                var toAddress = new MailAddress(Email);
                string fromPassword = CodigodeInstagram;
                string subjectFinal = subject;

                var smtp = new SmtpClient
                {
                    Host = proveedorCorreo,
                    Port = puerto,
                    EnableSsl = EnableSsl,
                    Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
                };

                using var message = new MailMessage(fromAddress, toAddress)
                {
                    Subject = subjectFinal,
                    Body = htmlBody,
                    IsBodyHtml = true
                };

                smtp.Send(message);
                return true;
            }
            catch (Exception ex)
            {
                SaveLogCorreo("Fallo en envío DIFUSION", ex.Message, Email);
                Console.WriteLine($"Error al enviar mensaje: {ex.Message}");
                return false;
            }
        }


        public static bool NotificarConfirmacionReserva(
              string email,
              string nombreCliente,
              string nombreTour,
              string nombreTourEN,
              string fecha,
              string hora,
              string idioma, 
              string nombrePuntoEncuentro,
              string descripcionPuntoEncuentro,
              string linkMapsPuntoEncuentro
          )
        {
            try
            {
                Result ConfigInit = ObtenerDatos();

                string subject = idioma == "es"
                    ? "¡Tu reserva ha sido confirmada!"
                    : "Your reservation has been confirmed!";

                string saludo = idioma == "es"
                    ? $"Estimado(a) <strong>{SecurityElement.Escape(nombreCliente)}</strong>,"
                    : $"Dear <strong>{SecurityElement.Escape(nombreCliente)}</strong>,";

                string mensajeIntro = idioma == "es"
                    ? "Tu reserva ha sido confirmada con los siguientes detalles:"
                    : "Your reservation has been confirmed with the following details:";
                string nombreTTour = idioma == "es" ? nombreTour : "TOUR";
                string textoTour = idioma == "es" ? "TOUR" : "TOUR";
                string textoFecha = idioma == "es" ? "FECHA" : "DATE";
                string textoHora = idioma == "es" ? "HORA" : "TIME";

                string textoPuntoTitulo = idioma == "es" ? "Punto de Encuentro" : "Meeting Point";
                string textoNombre = idioma == "es" ? "Nombre" : "Name";
                string textoDescripcion = idioma == "es" ? "Descripción" : "Description";
                string textoVerMapa = idioma == "es" ? "Ver en el mapa" : "View on map";
                string textoBoton = idioma == "es" ? "Ir al Punto de Encuentro" : "Go to Meeting Point";

                string textoTitulo1 = idioma == "es" ? "¡Reserva Confirmada!" : "Reservation Confirmed!";
                string textoTitulo2 = idioma == "es" ? "Gracias por elegirnos. ¡Disfruta tu experiencia!" : "Thank you for choosing us. Enjoy your experience!";
                string textoFooter = idioma == "es"
                    ? "Este es un correo informativo, por favor no responda a este mensaje."
                    : "This is an informational email, please do not reply to this message.";

                string htmlBody = $@"
                <html lang='{idioma}'>
                    <head>
                        <meta charset='UTF-8'/>
                        <meta name='viewport' content='width=device-width, initial-scale=1.0'/>
                        <title>{subject}</title>
                        <style>
                            body {{
                                font-family: 'Montserrat', Arial, sans-serif;
                                background-color: #e5e7eb;
                                color: #082338 !important;
                                margin: 0;
                                padding: 30px;
                                text-decoration: none;
                            }}
                            .container {{
                                background-color: #ffffff;
                                border-radius: 12px;
                                max-width: 600px;
                                margin: auto;
                                box-shadow: 0 4px 20px rgba(0, 0, 0, 0.1);
                                overflow: hidden;
                                border: 1px solid #ddd;
                            }}
                            .header {{
                                background-image: url('https://img.freepik.com/foto-gratis/big-ben-puente-westminster-al-atardecer-londres-reino-unido_268835-1395.jpg?semt=ais_hybrid&amp;w=740');
                                background-size: cover;
                                background-position: center;
                                padding: 40px 20px;
                                color: white;
                                text-align: center;
                                -webkit-text-stroke: 1px #082338;
                            }}
                            .header h1 {{
                                font-size: 32px;
                                color: white;
                                margin: 0;
                                text-shadow: 2px 2px 4px rgba(0,0,0,0.5);
                            }}
                            .header h2 {{
                                font-size: 18px;
                                margin: 10px 0 0 0;
                            }}
                            .card-body {{
                                padding: 30px;
                            }}
                            .details p {{
                                font-size: 16px;
                                line-height: 1.6;
                                margin: 10px 0;
                                color: #082338;
                            }}
                            .details strong {{
                                color: #082338;
                            }}
                            .button {{
                                display: inline-block;
                                background-color: #082338;
                                color: white !important;
                                padding: 12px 20px;
                                border-radius: 6px;
                                text-decoration: none;
                                font-weight: bold;
                                margin-top: 20px;
                            }}
                            .footer {{
                                font-size: 12px;
                                color: #666;
                                text-align: center;
                                padding: 15px;
                                border-top: 1px solid #ddd;
                            }}
                        </style>
                    </head>
                    <body>
                        <div class='container'>
                            <div class='header'>
                                <h1>VIAZO</h1>
                                <h1>{textoTitulo1}</h1>
                                <h2>{textoTitulo2}</h2>
                            </div>
                            <div class='card-body'>
                                <div class='details'>
                                    <p>{saludo}</p>
                                    <p>{mensajeIntro}</p>
                                    <p><strong>{textoTour}:</strong> {SecurityElement.Escape(nombreTour)}</p>
                                    <p><strong>{textoFecha}:</strong> {SecurityElement.Escape(fecha)}</p>
                                    <p><strong>{textoHora}:</strong> {SecurityElement.Escape(hora)}</p>

                                    <h3 style='margin-top:30px;'>🗺 {textoPuntoTitulo}</h3>
                                    <p><strong>{textoNombre}:</strong> {SecurityElement.Escape(nombrePuntoEncuentro)}</p>
                                    <p><strong>{textoDescripcion}:</strong> {SecurityElement.Escape(descripcionPuntoEncuentro)}</p>
                                    <p><a href='{linkMapsPuntoEncuentro}' style='color:#082338; text-decoration:underline;'>{textoVerMapa}</a></p>

                                    <a href='{linkMapsPuntoEncuentro}' class='button'>{textoBoton}</a>
                                </div>
                            </div>
                            <div class='footer'>
                                <p>{textoFooter}</p>
                            </div>
                        </div>
                    </body>
                </html>";

                var fromAddress = new MailAddress(ConfigInit.Row["UsuarioCorreo"].ToString(), ConfigInit.Row["NombrePlatForm"].ToString());
                var toAddress = new MailAddress(email);
                string fromPassword = ConfigInit.Row["CodigodeInstagram"].ToString();

                var smtp = new SmtpClient
                {
                    Host = ConfigInit.Row["proveedorCorreo"].ToString(),
                    Port = Convert.ToInt32(ConfigInit.Row["puerto"]),
                    EnableSsl = Convert.ToBoolean(ConfigInit.Row["EnableSsl"]),
                    Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
                };

                using var message = new MailMessage(fromAddress, toAddress)
                {
                    Subject = subject,
                    Body = htmlBody,
                    IsBodyHtml = true
                };

                smtp.Send(message);
                return true;
            }
            catch (Exception ex)
            {
                MethodsCompile.SaveLogCorreo("Fallo en envío CONFIRMACIÓN", ex.Message, email);
                Console.WriteLine($"Error al enviar mensaje: {ex.Message}");
                return false;
            }
        }
        public static bool NotificarConfirmacionGuia(
      string idReserva,
            string emailGuia,
      string nombreGuia,
      string nombreCliente,
      string nombreTour,
      string nombreTourEN,
      string fecha,
      string hora,
      string idioma,
      string nombrePuntoEncuentro,
      string descripcionPuntoEncuentro,
      string linkMapsPuntoEncuentro
  )
        {
            try
            {
                Result ConfigInit = ObtenerDatos();

                string subject = idioma == "es"
                    ? "¡Nueva reserva asignada a tu tour!"
                    : "New reservation assigned to your tour!";

                string saludo = idioma == "es"
                    ? $"Estimado(a) <strong>{SecurityElement.Escape(nombreGuia)}</strong>,"
                    : $"Dear <strong>{SecurityElement.Escape(nombreGuia)}</strong>,";

                string mensajeIntro = idioma == "es"
                    ? "Se te ha asignado una nueva reserva con los siguientes detalles:"
                    : "A new reservation has been assigned to you with the following details:";

                string textoCliente = idioma == "es" ? "Cliente" : "Client";
                string textoTour = idioma == "es" ? "TOUR" : "TOUR";
                string textoFecha = idioma == "es" ? "FECHA" : "DATE";
                string textoHora = idioma == "es" ? "HORA" : "TIME";

                string textoPuntoTitulo = idioma == "es" ? "Punto de Encuentro" : "Meeting Point";
                string textoNombre = idioma == "es" ? "Nombre" : "Name";
                string textoDescripcion = idioma == "es" ? "Descripción" : "Description";
                string textoVerMapa = idioma == "es" ? "Ver en el mapa" : "View on map";
                string textoBoton = idioma == "es" ? "Ir al Punto de Encuentro" : "Go to Meeting Point";

                string textoTitulo1 = idioma == "es" ? "¡Nueva Reserva!" : "New Reservation!";
                string textoTitulo2 = idioma == "es" ? "Recuerda estar preparado y puntual." : "Remember to be prepared and on time.";
                string textoFooter = idioma == "es"
                    ? "Este es un correo informativo, por favor no responda a este mensaje."
                    : "This is an informational email, please do not reply to this message.";

                string htmlBody = $@"
        <html lang='{idioma}'>
            <head>
                <meta charset='UTF-8'/>
                <meta name='viewport' content='width=device-width, initial-scale=1.0'/>
                <title>{subject}</title>
                <style>
                    body {{
                        font-family: 'Montserrat', Arial, sans-serif;
                        background-color: #e5e7eb;
                        color: #082338 !important;
                        margin: 0;
                        padding: 30px;
                        text-decoration: none;
                    }}
                    .container {{
                        background-color: #ffffff;
                        border-radius: 12px;
                        max-width: 600px;
                        margin: auto;
                        box-shadow: 0 4px 20px rgba(0, 0, 0, 0.1);
                        overflow: hidden;
                        border: 1px solid #ddd;
                    }}
                    .header {{
                        background-image: url('https://img.freepik.com/foto-gratis/big-ben-puente-westminster-al-atardecer-londres-reino-unido_268835-1395.jpg?semt=ais_hybrid&amp;w=740');
                        background-size: cover;
                        background-position: center;
                        padding: 40px 20px;
                        color: white;
                        text-align: center;
                        -webkit-text-stroke: 1px #082338;
                    }}
                    .header h1 {{
                        font-size: 32px;
                        color: white;
                        margin: 0;
                        text-shadow: 2px 2px 4px rgba(0,0,0,0.5);
                    }}
                    .header h2 {{
                        font-size: 18px;
                        margin: 10px 0 0 0;
                    }}
                    .card-body {{
                        padding: 30px;
                    }}
                    .details p {{
                        font-size: 16px;
                        line-height: 1.6;
                        margin: 10px 0;
                        color: #082338;
                    }}
                    .details strong {{
                        color: #082338;
                    }}
                    .button {{
                        display: inline-block;
                        background-color: #082338;
                        color: white !important;
                        padding: 12px 20px;
                        border-radius: 6px;
                        text-decoration: none;
                        font-weight: bold;
                        margin-top: 20px;
                    }}
                    .footer {{
                        font-size: 12px;
                        color: #666;
                        text-align: center;
                        padding: 15px;
                        border-top: 1px solid #ddd;
                    }}
                </style>
            </head>
            <body>
                <div class='container'>
                    <div class='header'>
                        <h1>VIAZO</h1>
                        <h1>{textoTitulo1}</h1>
                        <h2>{textoTitulo2}</h2>
                    </div>
                    <div class='card-body'>
                        <div class='details'>
                            <p>{saludo}</p>
                            <p>{mensajeIntro}</p>
                            <p><strong>CODIGO/CODE:</strong> {SecurityElement.Escape(idReserva)}</p>                            
                            <p><strong>{textoCliente}:</strong> {SecurityElement.Escape(nombreCliente)}</p>
                            <p><strong>{textoTour}:</strong> {SecurityElement.Escape(nombreTour)}</p>
                            <p><strong>{textoFecha}:</strong> {SecurityElement.Escape(fecha)}</p>
                            <p><strong>{textoHora}:</strong> {SecurityElement.Escape(hora)}</p>

                            <h3 style='margin-top:30px;'>🗺 {textoPuntoTitulo}</h3>
                            <p><strong>{textoNombre}:</strong> {SecurityElement.Escape(nombrePuntoEncuentro)}</p>
                            <p><strong>{textoDescripcion}:</strong> {SecurityElement.Escape(descripcionPuntoEncuentro)}</p>
                            <p><a href='{linkMapsPuntoEncuentro}' style='color:#082338; text-decoration:underline;'>{textoVerMapa}</a></p>

                            <a href='{linkMapsPuntoEncuentro}' class='button'>{textoBoton}</a>
                        </div>
                    </div>
                    <div class='footer'>
                        <p>{textoFooter}</p>
                    </div>
                </div>
            </body>
        </html>";

                var fromAddress = new MailAddress(ConfigInit.Row["UsuarioCorreo"].ToString(), ConfigInit.Row["NombrePlatForm"].ToString());
                var toAddress = new MailAddress(emailGuia);
                string fromPassword = ConfigInit.Row["CodigodeInstagram"].ToString();

                var smtp = new SmtpClient
                {
                    Host = ConfigInit.Row["proveedorCorreo"].ToString(),
                    Port = Convert.ToInt32(ConfigInit.Row["puerto"]),
                    EnableSsl = Convert.ToBoolean(ConfigInit.Row["EnableSsl"]),
                    Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
                };

                using var message = new MailMessage(fromAddress, toAddress)
                {
                    Subject = subject,
                    Body = htmlBody,
                    IsBodyHtml = true
                };

                smtp.Send(message);
                return true;
            }
            catch (Exception ex)
            {
                MethodsCompile.SaveLogCorreo("Fallo en envío CONFIRMACIÓN GUIA", ex.Message, emailGuia);
                Console.WriteLine($"Error al enviar mensaje: {ex.Message}");
                return false;
            }
        }



        public static bool NotificarCancelacionReserva_USUARIO(string email, string nombreCliente, string nombreTour, string NombreTourEN, string fecha, string hora, string idioma, string linkWeb)
        {
            try
            {
                Result ConfigInit = ObtenerDatos();

                string subject = idioma == "es"
                    ? $"Confirmación de cancelación de tu reserva"
                    : $"Reservation Cancellation Confirmation";

                string saludo = idioma == "es"
                    ? $"Estimado(a) <strong>{SecurityElement.Escape(nombreCliente)}</strong>,"
                    : $"Dear <strong>{SecurityElement.Escape(nombreCliente)}</strong>,";

                string mensajeIntro = idioma == "es"
                    ? "Te informamos la cancelación de tu reserva con los siguientes detalles:"
                    : "We confirm the cancellation of your reservation with the following details:";
                string NOMBRE = idioma == "es" ? nombreTour : NombreTourEN;
                string textoTour = idioma == "es" ? "TOUR" : "TOUR";
                string textoFecha = idioma == "es" ? "FECHA" : "DATE";
                string textoHora = idioma == "es" ? "HORA" : "TIME";
                string textoBoton = idioma == "es" ? "Volver a reservar" : "Book again";
                string textoDespedida = idioma == "es"
                    ? "Este es un correo informativo, por favor no responda a este mensaje."
                    : "This is an informational email, please do not reply to this message.";

                string htmlBody = $@"
        <html lang='{idioma}'>
            <head>
                <meta charset='UTF-8'/>
                <meta name='viewport' content='width=device-width, initial-scale=1.0'/>
                <title>{subject}</title>
                <style>
                    body {{
                        font-family: 'Montserrat', Arial, sans-serif;
                        background-color: #e5e7eb;
                        color: #082338 !important;
                        margin: 0;
                        padding: 30px;
                        text-decoration: none;
                    }}
                    .container {{
                        background-color: #ffffff;
                        border-radius: 12px;
                        max-width: 600px;
                        margin: auto;
                        box-shadow: 0 4px 20px rgba(0, 0, 0, 0.1);
                        overflow: hidden;
                        border: 1px solid #ddd;
                    }}
                    .header {{
                        background-image: url('https://img.freepik.com/foto-gratis/big-ben-puente-westminster-al-atardecer-londres-reino-unido_268835-1395.jpg?semt=ais_hybrid&amp;w=740');
                        background-size: cover;
                        background-position: center;
                        padding: 40px 20px;
                        color: white;
                        text-align: center;
                        -webkit-text-stroke: 1px #082338;
                    }}
                    .header h1 {{
                        font-size: 32px;
                        color: white;
                        margin: 0;
                        text-shadow: 2px 2px 4px rgba(0,0,0,0.5);
                    }}
                    .header h2 {{
                        font-size: 18px;
                        margin: 10px 0 0 0;
                    }}
                    .card-body {{
                        padding: 30px;
                    }}
                    .details p {{
                        font-size: 16px;
                        line-height: 1.6;
                        margin: 10px 0;
                        color: #082338;
                    }}
                    .details strong {{
                        color: #082338;
                    }}
                    .button {{
                        display: inline-block;
                        background-color: #082338;
                        color: white !important;
                        padding: 12px 20px;
                        border-radius: 6px;
                        text-decoration: none;
                        font-weight: bold;
                        margin-top: 20px;
                    }}
                    .footer {{
                        font-size: 12px;
                        color: #666;
                        text-align: center;
                        padding: 15px;
                        border-top: 1px solid #ddd;
                    }}
                </style>
            </head>
            <body>
                <div class='container'>
                    <div class='header'>
                        <h1>VIAZO</h1>
                        <h1>{(idioma == "es" ? "¡Reserva Cancelada!" : "Reservation Cancelled!")}</h1>
                        <h2>{(idioma == "es" ? "¡Te esperamos pronto!" : "We hope to see you soon!")}</h2>
                    </div>
                    <div class='card-body'>
                        <div class='details'>
                            <p>{saludo}</p>
                            <p>{mensajeIntro}</p>
                            <p><strong>{textoTour}:</strong> {SecurityElement.Escape(NOMBRE)}</p>
                            <p><strong>{textoFecha}:</strong> {SecurityElement.Escape(fecha)}</p>
                            <p><strong>{textoHora}:</strong> {SecurityElement.Escape(hora)}</p>
                            <a href='{linkWeb}' class='button'>{textoBoton}</a>
                        </div>
                    </div>
                    <div class='footer'>
                        <p>{textoDespedida}</p>
                    </div>
                </div>
            </body>
        </html>";

                var fromAddress = new MailAddress(ConfigInit.Row["UsuarioCorreo"].ToString(), ConfigInit.Row["NombrePlatForm"].ToString());
                var toAddress = new MailAddress(email);
                string fromPassword = ConfigInit.Row["CodigodeInstagram"].ToString();

                var smtp = new SmtpClient
                {
                    Host = ConfigInit.Row["proveedorCorreo"].ToString(),
                    Port = Convert.ToInt32(ConfigInit.Row["puerto"]),
                    EnableSsl = Convert.ToBoolean(ConfigInit.Row["EnableSsl"]),
                    Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
                };

                using var message = new MailMessage(fromAddress, toAddress)
                {
                    Subject = subject,
                    Body = htmlBody,
                    IsBodyHtml = true
                };

                smtp.Send(message);
                return true;
            }
            catch (Exception ex)
            {
                MethodsCompile.SaveLogCorreo("Fallo en envío CANCELACIÓN CLIENTE", ex.Message, email);
                Console.WriteLine($"Error al enviar mensaje: {ex.Message}");
                return false;
            }
        }
        public static bool NotificarCancelacionReserva_GUIA(
    string id_reserva,
            string emailGuia,
    string nombreGuia,
    string nombreCliente,
    string nombreTour,
    string nombreTourEN,
    string fecha,
    string hora,
    string idioma
)
        {
            try
            {
                Result ConfigInit = ObtenerDatos();

                string subject = idioma == "es"
                    ? $"Cancelación de reserva asignada a tu tour"
                    : $"Reservation Cancellation of your tour";

                string saludo = idioma == "es"
                    ? $"Estimado(a) <strong>{SecurityElement.Escape(nombreGuia)}</strong>,"
                    : $"Dear <strong>{SecurityElement.Escape(nombreGuia)}</strong>,";

                string mensajeIntro = idioma == "es"
                    ? $"Te informamos que el cliente <strong>{SecurityElement.Escape(nombreCliente)}</strong> ha cancelado su reserva:"
                    : $"We inform you that the client <strong>{SecurityElement.Escape(nombreCliente)}</strong> has cancelled their reservation:";

                string NOMBRE = idioma == "es" ? nombreTour : nombreTourEN;
                string textoTour = idioma == "es" ? "TOUR" : "TOUR";
                string textoFecha = idioma == "es" ? "FECHA" : "DATE";
                string textoHora = idioma == "es" ? "HORA" : "TIME";
                string textoDespedida = idioma == "es"
                    ? "Este es un correo informativo, por favor no responda a este mensaje."
                    : "This is an informational email, please do not reply to this message.";

                string htmlBody = $@"
<html lang='{idioma}'>
    <head>
        <meta charset='UTF-8'/>
        <meta name='viewport' content='width=device-width, initial-scale=1.0'/>
        <title>{subject}</title>
        <style>
            body {{
                font-family: 'Montserrat', Arial, sans-serif;
                background-color: #e5e7eb;
                color: #082338 !important;
                margin: 0;
                padding: 30px;
                text-decoration: none;
            }}
            .container {{
                background-color: #ffffff;
                border-radius: 12px;
                max-width: 600px;
                margin: auto;
                box-shadow: 0 4px 20px rgba(0, 0, 0, 0.1);
                overflow: hidden;
                border: 1px solid #ddd;
            }}
            .header {{
                background-image: url('https://img.freepik.com/foto-gratis/big-ben-puente-westminster-al-atardecer-londres-reino-unido_268835-1395.jpg?semt=ais_hybrid&amp;w=740');
                background-size: cover;
                background-position: center;
                padding: 40px 20px;
                color: white;
                text-align: center;
                -webkit-text-stroke: 1px #082338;
            }}
            .header h1 {{
                font-size: 32px;
                color: white;
                margin: 0;
                text-shadow: 2px 2px 4px rgba(0,0,0,0.5);
            }}
            .header h2 {{
                font-size: 18px;
                margin: 10px 0 0 0;
            }}
            .card-body {{
                padding: 30px;
            }}
            .details p {{
                font-size: 16px;
                line-height: 1.6;
                margin: 10px 0;
                color: #082338;
            }}
            .details strong {{
                color: #082338;
            }}
            .footer {{
                font-size: 12px;
                color: #666;
                text-align: center;
                padding: 15px;
                border-top: 1px solid #ddd;
            }}
        </style>
    </head>
    <body>
        <div class='container'>
            <div class='header'>
                <h1>VIAZO</h1>
                <h1>{(idioma == "es" ? "¡Reserva Cancelada!" : "Reservation Cancelled!")}</h1>
                <h2>{(idioma == "es" ? "Se ha liberado un cupo en tu tour." : "A spot in your tour has been cancelled.")}</h2>
            </div>
            <div class='card-body'>
                <div class='details'>
                    <p>{saludo}</p>
                    <p>{mensajeIntro}</p>
                    <p><strong>CODE/CODIGO:</strong> {SecurityElement.Escape(id_reserva)}</p>                    
                    <p><strong>{textoTour}:</strong> {SecurityElement.Escape(NOMBRE)}</p>
                    <p><strong>{textoFecha}:</strong> {SecurityElement.Escape(fecha)}</p>
                    <p><strong>{textoHora}:</strong> {SecurityElement.Escape(hora)}</p>
                </div>
            </div>
            <div class='footer'>
                <p>{textoDespedida}</p>
            </div>
        </div>
    </body>
</html>";

                var fromAddress = new MailAddress(ConfigInit.Row["UsuarioCorreo"].ToString(), ConfigInit.Row["NombrePlatForm"].ToString());
                var toAddress = new MailAddress(emailGuia);
                string fromPassword = ConfigInit.Row["CodigodeInstagram"].ToString();

                var smtp = new SmtpClient
                {
                    Host = ConfigInit.Row["proveedorCorreo"].ToString(),
                    Port = Convert.ToInt32(ConfigInit.Row["puerto"]),
                    EnableSsl = Convert.ToBoolean(ConfigInit.Row["EnableSsl"]),
                    Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
                };

                using var message = new MailMessage(fromAddress, toAddress)
                {
                    Subject = subject,
                    Body = htmlBody,
                    IsBodyHtml = true
                };

                smtp.Send(message);
                return true;
            }
            catch (Exception ex)
            {
                MethodsCompile.SaveLogCorreo("Fallo en envío CANCELACIÓN GUIA", ex.Message, emailGuia);
                Console.WriteLine($"Error al enviar mensaje: {ex.Message}");
                return false;
            }
        }



        public static bool NotificarCancelacionReserva(string email, string nombreCliente, string idReserva, string nombreGuru, string idioma, string linkWeb)
        {
            try
            {

                Result ConfigInit = ObtenerDatos();
                string subject = idioma == "es"
                    ? $"Cancelación de tu reserva #{idReserva}"
                    : $"Cancellation of your reservation #{idReserva}";

                
                string saludo = idioma == "es"
                    ? $"Hola <strong>{SecurityElement.Escape(nombreCliente)}</strong>,"
                    : $"Hello <strong>{SecurityElement.Escape(nombreCliente)}</strong>,";

                string mensajePrincipal = idioma == "es"
                    ? $"Tu gurú <strong>{SecurityElement.Escape(nombreGuru)}</strong> lamenta informarte que tu reserva <strong>#{SecurityElement.Escape(idReserva)}</strong> ha sido cancelada debido a circunstancias imprevistas."
                    : $"Your guru <strong>{SecurityElement.Escape(nombreGuru)}</strong> regrets to inform you that your reservation <strong>#{SecurityElement.Escape(idReserva)}</strong> has been cancelled due to unforeseen circumstances.";

                string mensajeAccion = idioma == "es"
                    ? "Si deseas más información, puedes contactarnos escribiendo en nuestro sitio web en el menú de Mensajes o respondiendo a este correo."
                    : "If you need more information, you can contact us by writing on our website in the Messages menu or replying to this email.";

                string textoBoton = idioma == "es" ? "VER MIS MENSAJES" : "SEE MY MESSAGES";

                string mensajeDespedida = idioma == "es"
                    ? "Gracias por tu comprensión.<br/>Equipo de Atención al Cliente"
                    : "Thank you for your understanding.<br/>Customer Service Team";

                
                string htmlBody = $@"
                                    <html>
                                        <body style='font-family: Arial, sans-serif; color: #333;'>
                                            <h2>{(idioma == "es" ? "Reserva Cancelada" : "Reservation Cancelled")}</h2>

                                            <p>{saludo}</p>

                                            <p>{mensajePrincipal}</p>

                                            <p style='background-color: #f9f9f9; padding: 15px; border-left: 5px solid #e53935; margin-top: 10px; border-radius: 5px;'>
                                                {mensajeAccion}<br/>
                                                <a href='{linkWeb}' style='color: #e53935; font-weight: bold; text-decoration: none;'>{textoBoton}</a>
                                            </p>

                                            <p style='margin-top: 20px;'>{mensajeDespedida}</p>
                                        </body>
                                    </html>";


                // Enviar el correo
                var fromAddress = new MailAddress(ConfigInit.Row["UsuarioCorreo"].ToString(), ConfigInit.Row["NombrePlatForm"].ToString());
                var toAddress = new MailAddress(email);
                string fromPassword = ConfigInit.Row["CodigodeInstagram"].ToString();
                string subjectFinal = subject;

                var smtp = new SmtpClient
                {
                    Host = ConfigInit.Row["proveedorCorreo"].ToString(),
                    Port = Convert.ToInt32(ConfigInit.Row["puerto"]),
                    EnableSsl = Convert.ToBoolean(ConfigInit.Row["EnableSsl"]),
                    Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
                };

                using var message = new MailMessage(fromAddress, toAddress)
                {
                    Subject = subjectFinal,
                    Body = htmlBody,
                    IsBodyHtml = true
                };

                smtp.Send(message);
                return true;
            }
            catch (Exception ex)
            {
                MethodsCompile.SaveLogCorreo("Fallo en envío CANCELACION", ex.Message, email);
                Console.WriteLine($"Error al enviar mensaje: {ex.Message}");
                return false;
            }
 
        }
        public static bool NotificarReactivacionReserva(string email, string nombreCliente, string idReserva, string nombreGuru, string idioma, string linkWeb)
        {
            try
            {
                Result ConfigInit = ObtenerDatos();

                string subject = idioma == "es"
                    ? $"Tu reserva #{idReserva} ha sido reactivada"
                    : $"Your reservation #{idReserva} has been reactivated";

                string saludo = idioma == "es"
                    ? $"Hola <strong>{SecurityElement.Escape(nombreCliente)}</strong>,"
                    : $"Hello <strong>{SecurityElement.Escape(nombreCliente)}</strong>,";

                string mensajePrincipal = idioma == "es"
                    ? $"¡Buenas noticias! Tu gurú <strong>{SecurityElement.Escape(nombreGuru)}</strong> ha reactivado tu reserva <strong>#{SecurityElement.Escape(idReserva)}</strong>. Esta continuará en la fecha y hora previamente programadas."
                    : $"Good news! Your guru <strong>{SecurityElement.Escape(nombreGuru)}</strong> has reactivated your reservation <strong>#{SecurityElement.Escape(idReserva)}</strong>. It will continue on the date and time previously agreed upon.";

                string mensajeAccion = idioma == "es"
                    ? "Si deseas más detalles, puedes revisar tus mensajes o comunicarte directamente desde el sitio web."
                    : "If you'd like more details, you can check your messages or contact us directly from the website.";

                string textoBoton = idioma == "es" ? "IR A MIS MENSAJES" : "GO TO MY MESSAGES";

                string mensajeDespedida = idioma == "es"
                    ? "Gracias por seguir confiando en nosotros.<br/>Equipo de Atención al Cliente"
                    : "Thank you for continuing to trust us.<br/>Customer Service Team";

                string htmlBody = $@"
            <html>
                <body style='font-family: Arial, sans-serif; color: #333;'>
                    <h2>{(idioma == "es" ? "Reserva Reactivada" : "Reservation Reactivated")}</h2>

                    <p>{saludo}</p>

                    <p>{mensajePrincipal}</p>

                    <p style='background-color: #f1f8e9; padding: 15px; border-left: 5px solid #43a047; margin-top: 10px; border-radius: 5px;'>
                        {mensajeAccion}<br/>
                        <a href='{linkWeb}' style='color: #43a047; font-weight: bold; text-decoration: none;'>{textoBoton}</a>
                    </p>

                    <p style='margin-top: 20px;'>{mensajeDespedida}</p>
                </body>
            </html>";

                var fromAddress = new MailAddress(ConfigInit.Row["UsuarioCorreo"].ToString(), ConfigInit.Row["NombrePlatForm"].ToString());
                var toAddress = new MailAddress(email);
                string fromPassword = ConfigInit.Row["CodigodeInstagram"].ToString();

                var smtp = new SmtpClient
                {
                    Host = ConfigInit.Row["proveedorCorreo"].ToString(),
                    Port = Convert.ToInt32(ConfigInit.Row["puerto"]),
                    EnableSsl = Convert.ToBoolean(ConfigInit.Row["EnableSsl"]),
                    Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
                };

                using var message = new MailMessage(fromAddress, toAddress)
                {
                    Subject = subject,
                    Body = htmlBody,
                    IsBodyHtml = true
                };

                smtp.Send(message);
                return true;
            }
            catch (Exception ex)
            {
                MethodsCompile.SaveLogCorreo("Fallo en envío REACTIVACIÓN", ex.Message, email);
                Console.WriteLine($"Error al enviar mensaje: {ex.Message}");
                return false;
            }
        }


        public static bool NotificarSolicitudOpinionReserva(
       string email,
       string nombreCliente,
       string idReserva,
       string nombreGuru,
       string idioma,
       string linkOpinion,
       string nombreTourIngles,
       string nombreTourEspanol)
        {
            try
            {
                Result ConfigInit = ObtenerDatos();

                // Determinar nombre del tour según idioma
                string nombreTour = idioma == "es" ? nombreTourEspanol : nombreTourIngles;

                string subject = idioma == "es"
                    ? $"¿Cómo estuvo tu experiencia en el tour \"{nombreTour}\"?"
                    : $"How was your experience on the \"{nombreTour}\" tour?";

                string saludo = idioma == "es"
                    ? $"Hola <strong>{SecurityElement.Escape(nombreCliente)}</strong>,"
                    : $"Hello <strong>{SecurityElement.Escape(nombreCliente)}</strong>,";

                string mensajePrincipal = idioma == "es"
                    ? $"Queremos agradecerte por haber realizado el tour <strong>{SecurityElement.Escape(nombreTour)}</strong> junto a <strong>{SecurityElement.Escape(nombreGuru)}</strong>. Nos encantaría conocer tu opinión sobre esta experiencia."
                    : $"We want to thank you for joining the <strong>{SecurityElement.Escape(nombreTour)}</strong> tour with <strong>{SecurityElement.Escape(nombreGuru)}</strong>. We'd love to hear your thoughts about the experience.";

                string mensajeAccion = idioma == "es"
                    ? "Tu opinión es muy valiosa para nosotros y para futuros viajeros. Puedes compartirla haciendo clic en el siguiente botón."
                    : "Your feedback is very valuable to us and future travelers. You can share it by clicking the button below.";

                string textoBoton = idioma == "es" ? "DEJAR MI OPINIÓN" : "LEAVE MY FEEDBACK";

                string mensajeDespedida = idioma == "es"
                    ? "Gracias nuevamente por confiar en nosotros.<br/>Equipo de Atención al Cliente"
                    : "Thank you again for choosing us.<br/>Customer Service Team";

                string htmlBody = $@"
            <html>
                <body style='font-family: Arial, sans-serif; color: #333;'>
                    <h2>{(idioma == "es" ? "Gracias por tu tour" : "Thank you for your tour")}</h2>

                    <p>{saludo}</p>

                    <p>{mensajePrincipal}</p>

                    <p>{mensajeAccion}</p>

                    <p style='background-color: #e3f2fd; padding: 15px; border-left: 5px solid #1e88e5; margin-top: 10px; border-radius: 5px;'>
                        <a href='{linkOpinion}' style='color: #1e88e5; font-weight: bold; text-decoration: none;'>{textoBoton}</a>
                    </p>

                    <p style='margin-top: 20px;'>{mensajeDespedida}</p>
                </body>
            </html>";

                var fromAddress = new MailAddress(ConfigInit.Row["UsuarioCorreo"].ToString(), ConfigInit.Row["NombrePlatForm"].ToString());
                var toAddress = new MailAddress(email);
                string fromPassword = ConfigInit.Row["CodigodeInstagram"].ToString();

                var smtp = new SmtpClient
                {
                    Host = ConfigInit.Row["proveedorCorreo"].ToString(),
                    Port = Convert.ToInt32(ConfigInit.Row["puerto"]),
                    EnableSsl = Convert.ToBoolean(ConfigInit.Row["EnableSsl"]),
                    Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
                };

                using var message = new MailMessage(fromAddress, toAddress)
                {
                    Subject = subject,
                    Body = htmlBody,
                    IsBodyHtml = true
                };

                smtp.Send(message);
                return true;
            }
            catch (Exception ex)
            {
                MethodsCompile.SaveLogCorreo("Fallo en envío SOLICITUD OPINIÓN", ex.Message, email);
                Console.WriteLine($"Error al enviar mensaje: {ex.Message}");
                return false;
            }
        }


        public static List<Dictionary<string, object>> ConvertDataTableToJsonTopurs(DataTable toursTable, DataTable itinerariosTable, DataTable fechasMasiTable, DataTable imtTables, DataTable commentsTable, DataTable asksTable, DataTable fechasMasiTable2)
        {
            var result = new List<Dictionary<string, object>>();

            var fotosTour = imtTables.AsEnumerable()
                .GroupBy(row => row["id_tour"])
                .ToDictionary(
                    g => g.Key.ToString(),
                    g => g.Select(row => new Dictionary<string, object>
                    {
                { "RUTA", row["RUTA"] },
                { "nombre_Img", row["nombre_Img"] }
                    }).ToList()
                );

            var itinerariosDict = itinerariosTable.AsEnumerable()
                .GroupBy(row => row["id_tour"])
                .ToDictionary(
                    g => g.Key.ToString(),
                    g => g.Select(row => new Dictionary<string, object>
                    {
                { "id", row["id"] },
                { "lugar", row["lugar"] },
                { "hora", row["hora"] },
                { "descripcion", row["descripcion"] }
                    }).ToList()
                );

            var fechasMasiDict = fechasMasiTable.AsEnumerable()
            .GroupBy(row => new { id_tour = row["id_tour"].ToString(), fecha = row["fecha"].ToString() })
            .GroupBy(g => g.Key.id_tour)
            .ToDictionary(
                g => g.Key,
                g => g.Select(subGroup => new Dictionary<string, object>
                {
                    { "fecha", subGroup.Key.fecha },
                    { "horarios", subGroup
                        .Select(row => new {
                            hora = row["horario"].ToString(),
                            idioma = row["idioma"].ToString(),
                            id_guia = row["id_guia"].ToString(),
                            guia = row["id_guia"].ToString()
                        })
                        .Distinct()
                        .Select(h => new Dictionary<string, string> {
                            { "hora", h.hora },
                            { "idioma", h.idioma },
                            { "id_guia", h.id_guia},
                            { "guia", h.guia }
                        })
                        .ToList()
                    }
                }).ToList()
            );

            var fechasMasiDict2 = fechasMasiTable2.AsEnumerable()
    .GroupBy(row => row["id_tour"].ToString())
    .ToDictionary(
        g => g.Key,
        g => g
            .Select(row => new Dictionary<string, object>
            {
                { "fecha", row["fecha"].ToString() },
                { "hora", row["horario"].ToString() },
                { "idioma", row["idioma"].ToString() },
                { "id_guia", row["id_guia"].ToString() },
                { "guia", row["id_guia"].ToString() },
                { "activo", Convert.ToBoolean(row["activo"]) }
            })
            .Distinct()
            .ToList()
    );


            var coments = commentsTable.AsEnumerable()
                 .GroupBy(row => row["id_tour"])
                 .ToDictionary(
                     g => g.Key.ToString(),
                     g => g.Select(row => new Dictionary<string, object>
                     {
                { "NombreClient", row["NombreClient"] },
                { "ImagePath", row["ImagePath"] },
                { "OpinionText", row["OpinionText"] },
                { "NombreTour", row["NombreTour"] },
                { "rating", row["rating"] },
                { "picture", row["picture"] }
                     }).ToList()
                 );

            var preguntasFrecuentes = asksTable.AsEnumerable()
            .GroupBy(row => row["id_tour"].ToString())
            .ToDictionary(
                g => g.Key,
                g => g.Select(row => new Dictionary<string, object>
                {
                    { "code", row["id_pregunta"] },
                    { "name", row["name"] },
                    { "nameING", row["nameING"] }
                }).ToList()
            );


            foreach (DataRow row in toursTable.Rows)
            {
                var rowDict = new Dictionary<string, object>();

                string idTour = row["id"].ToString();

                foreach (DataColumn col in toursTable.Columns)
                {
                    string colName = col.ColumnName;

                    if (colName == "duracion")
                    {
                        rowDict[colName] = new Dictionary<string, string>
                {
                    { "name", row[col].ToString() }
                };
                    }
                    else if (colName == "PuntodeEncuentro_Lugar")
                    {
                        rowDict["puntoEncuentro"] = new Dictionary<string, object>
                {
                    { "place", row[col] },
                            { "url", row["PuntodeEncuentro_Url"] },
                            { "description", row["PuntodeEncuentro_Descripcion"] }
                };

                    }
                    else
                    {
                        rowDict[colName] = row[col];
                    }
                }

                rowDict["itinerario"] = itinerariosDict.ContainsKey(idTour) ? itinerariosDict[idTour] : new List<Dictionary<string, object>>();
                rowDict["fechasMasi2"] = fechasMasiDict2.ContainsKey(idTour) ? fechasMasiDict2[idTour] : new List<Dictionary<string, object>>();
                rowDict["fechasMasi"] = fechasMasiDict.ContainsKey(idTour) ? fechasMasiDict[idTour] : new List<Dictionary<string, object>>();
                rowDict["fotosTour"] = fotosTour.ContainsKey(idTour) ? fotosTour[idTour] : new List<Dictionary<string, object>>();
                rowDict["coments"] = coments.ContainsKey(idTour) ? coments[idTour] : new List<Dictionary<string, object>>();
                rowDict["preguntasFrecuentes"] = preguntasFrecuentes.ContainsKey(idTour) ? preguntasFrecuentes[idTour] : new List<Dictionary<string, object>>();


                result.Add(rowDict);
            }

            return result;
        }

        public static List<Dictionary<string, object>> ConvertDataTableToJson(DataTable table)
        {
            var result = new List<Dictionary<string, object>>();
            foreach (DataRow row in table.Rows)
            {
                var rowDict = new Dictionary<string, object>();
                foreach (DataColumn col in table.Columns)
                {
                    rowDict[col.ColumnName] = row[col];
                }
                result.Add(rowDict);
            }
            return result;
        }
        public static List<Dictionary<string, object>> ConvertDataTableToJson1(DataTable reservasTable, DataTable mensajesTable)
        {
            var mensajesList = new List<Dictionary<string, object>>();

            foreach (DataRow row in mensajesTable.Rows)
            {
                var mensaje = new Dictionary<string, object>
                {
                    ["texto"] = row["Mensaje"] == DBNull.Value ? null : row["Mensaje"],
                    ["created"] = row["created"] == DBNull.Value ? null : row["created"],
                    ["id_reserva"] = row["id_reserva"] == DBNull.Value ? null : row["id_reserva"],
                    ["idiomaselected"] = row["idiomaselected"] == DBNull.Value ? null : row["idiomaselected"],
                    ["isprop"] = row["isprop"] == DBNull.Value ? null : row["isprop"]
                };
                mensajesList.Add(mensaje);
            }

            var result = new List<Dictionary<string, object>>();

            foreach (DataRow row in reservasTable.Rows)
            {
                var rowDict = new Dictionary<string, object>();

                foreach (DataColumn col in reservasTable.Columns)
                {
                    rowDict[col.ColumnName] = row[col] == DBNull.Value ? null : row[col];
                }

                object reservaIdObj = row["id_reservaB"];
                if (reservaIdObj != DBNull.Value && int.TryParse(reservaIdObj.ToString(), out int reservaID))
                {
                    var mensajesRelacionados = mensajesList
                        .Where(m => m["id_reserva"] != null && int.TryParse(m["id_reserva"].ToString(), out int id) && id == reservaID)
                        .ToList();

                    rowDict["mensajeria"] = mensajesRelacionados;
                }
                else
                {
                    rowDict["mensajeria"] = new List<Dictionary<string, object>>();
                }

                result.Add(rowDict);
            }

            return result;
        }
    }
}

