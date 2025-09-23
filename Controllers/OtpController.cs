using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Xml.Xsl;
using System.Xml;
using System.Security;
using J_W.Estructura;
using APISietemasdereservas.Controllers;
using Microsoft.Extensions.Configuration;

public class OTPManager
{
    // Almacenamiento temporal (puedes reemplazar por DB o caché distribuida)
    private static Dictionary<string, (string Code, DateTime Expiration)> otpStore = new();
    static IConfigurationRoot GetConfiguration()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

        return builder.Build();

    }
    // Genera un código OTP de n dígitos (por defecto 6)
    public static string GenerateOTP(int length = 6)
    {
        var random = new Random();
        string otp = "";
        for (int i = 0; i < length; i++)
            otp += random.Next(0, 10).ToString();
        return otp;
    }
    public static string TransformXsltFromString(string xmlString, string xsltPath)
    {
        var xslt = new XslCompiledTransform();
        xslt.Load(xsltPath);

        using var stringReader = new StringReader(xmlString);
        using var xmlReader = XmlReader.Create(stringReader);

        using var stringWriter = new StringWriter();
        xslt.Transform(xmlReader, null, stringWriter);

        return stringWriter.ToString();
    }
    public static bool SendEmailWithXsltTemplate(string email, string subject, Dictionary<string, string> xsltParameters, string xsltFilePath, string rootElementName = "top", string innerElementName = "DataInfo")
    {
        try
        {
            Result ConfigInit = MethodsCompile.ObtenerDatos();
            // Construir el XML con los parámetros recibidos
            // Crear la lista de parámetros para el XSLT
            var xsltArgs = new XsltArgumentList();
            xsltArgs.AddParam("url", "", "https://youtube.com");  // Aquí agregas tu nuevo parámetro

            var xmlDoc = new XmlDocument();
            var root = xmlDoc.CreateElement(rootElementName);
            var dataInfo = xmlDoc.CreateElement(innerElementName);

            foreach (var kvp in xsltParameters)
            {
                var attr = xmlDoc.CreateAttribute(kvp.Key);
                attr.Value = SecurityElement.Escape(kvp.Value ?? string.Empty);
                dataInfo.Attributes.Append(attr);
            }

            root.AppendChild(dataInfo);
            xmlDoc.AppendChild(root);

            // Cargar la plantilla XSLT
            var xslt = new XslCompiledTransform();
            xslt.Load(xsltFilePath);

            // Generar el HTML usando la transformación
            string htmlBody;
            using (var stringWriter = new StringWriter())
            using (var xmlWriter = XmlWriter.Create(stringWriter, xslt.OutputSettings))
            {
                xslt.Transform(xmlDoc, xsltArgs, xmlWriter);

                htmlBody = stringWriter.ToString();
            }

            //// Configurar el correo 
            string emai1l = ConfigInit.Row["UsuarioCorreo"].ToString();
            string nombreplatrom = ConfigInit.Row["NombrePlatForm"].ToString();
            string CodigodeInstagram = ConfigInit.Row["CodigodeInstagram"].ToString();
            string proveedorCorreo = ConfigInit.Row["proveedorCorreo"].ToString();
            var puerto = Convert.ToInt32(ConfigInit.Row["puerto"]);
            bool EnableSsl = Convert.ToBoolean(ConfigInit.Row["EnableSsl"]);

            var fromAddress = new MailAddress(emai1l, nombreplatrom);
            var toAddress = new MailAddress(email);
            string fromPassword = CodigodeInstagram; 

            var smtp = new SmtpClient
            {
                Host = proveedorCorreo,
                Port = puerto,
                EnableSsl = EnableSsl,
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
            MethodsCompile.SaveLogCorreo("Fallo en envío de OTP", ex.Message, email);
            Console.WriteLine($"Error al enviar correo: {ex.Message}");
            return false;
        }
    }



    public static bool SendOTP_WithXslt(string email, string otpCode)
    {
        try
        {
            
            Result ConfigInit = MethodsCompile.ObtenerDatos();

            
            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml($"<top><DataInfo otp='{otpCode}'/></top>");

            
            var xslt = new XslCompiledTransform();
            var configuracion = GetConfiguration();
            var xsltEnviarOTP = configuracion.GetSection("Configuracion").GetSection("xsltEnviarOTP").Value;

            xslt.Load(xsltEnviarOTP);

            
            string htmlBody;
            using (var stringWriter = new StringWriter())
            using (var xmlWriter = XmlWriter.Create(stringWriter, xslt.OutputSettings))
            {
                xslt.Transform(xmlDoc, xmlWriter);
                htmlBody = stringWriter.ToString();
            }

            string emai1l = ConfigInit.Row["UsuarioCorreo"].ToString();
            string nombreplatrom = ConfigInit.Row["NombrePlatForm"].ToString();
            string CodigodeInstagram = ConfigInit.Row["CodigodeInstagram"].ToString();
            string proveedorCorreo = ConfigInit.Row["proveedorCorreo"].ToString();
            var puerto = Convert.ToInt32(ConfigInit.Row["puerto"]);
            bool EnableSsl = Convert.ToBoolean(ConfigInit.Row["EnableSsl"]);
            
            var fromAddress = new MailAddress(emai1l, nombreplatrom);
            var toAddress = new MailAddress(email);
            string fromPassword = CodigodeInstagram;
            string subjectFinal = "Codigo OTP | Code OTP";

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
            MethodsCompile.SaveLogCorreo("Fallo en envío de OTP", ex.Message, email);
            Console.WriteLine($"Error al enviar OTP: {ex.Message}");
            return false;
        }
    }
    public static bool ConfirmacionReservaMail(string email, string Nombre, string NombreTour, string fecha, string hora, string idreserva)
    {
        try
        {
            Result ConfigInit = MethodsCompile.ObtenerDatos();
            // 1. Crear XML con el OTP
            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml($"<top><DataInfo  Nombre='{Nombre}'NombreTour='{NombreTour}' fecha='{fecha}' hora='{hora}'/></top>");

            // 2. Cargar el XSLT
            var xslt = new XslCompiledTransform();
 
            var configuracion = GetConfiguration();
            var xsltEnviarOTP = configuracion.GetSection("Configuracion").GetSection("xsltEnviarReserva").Value;
            xslt.Load(xsltEnviarOTP);

            // 3. Aplicar transformación
            string htmlBody;
            using (var stringWriter = new StringWriter())
            using (var xmlWriter = XmlWriter.Create(stringWriter, xslt.OutputSettings))
            {
                xslt.Transform(xmlDoc, xmlWriter);
                htmlBody = stringWriter.ToString();
            }

            // 4. Enviar correo 
            string emai1l = ConfigInit.Row["UsuarioCorreo"].ToString();
            string nombreplatrom = ConfigInit.Row["NombrePlatForm"].ToString();
            string CodigodeInstagram = ConfigInit.Row["CodigodeInstagram"].ToString();
            string proveedorCorreo = ConfigInit.Row["proveedorCorreo"].ToString();
            var puerto = Convert.ToInt32(ConfigInit.Row["puerto"]);
            bool EnableSsl = Convert.ToBoolean(ConfigInit.Row["EnableSsl"]);

            var fromAddress = new MailAddress(emai1l, nombreplatrom);
            var toAddress = new MailAddress(email);
            string fromPassword = CodigodeInstagram;
            string subject = $"RESERVA DE TOUR {idreserva} CONFIRMADA EL {fecha} A LAS {hora}";

            var smtp = new SmtpClient
            {
                Host = proveedorCorreo,
                Port = puerto,
                EnableSsl = EnableSsl,
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
            MethodsCompile.SaveLogCorreo("Fallo en envío de OTP", ex.Message, email);
            Console.WriteLine($"Error al enviar OTP: {ex.Message}");
            return false;
        }
    }


    // Valida el OTP ingresado por el usuario
    public static bool ValidateOTP(string email, string inputCode)
    {
        if (!otpStore.ContainsKey(email))
            return false;

        var (storedCode, expiration) = otpStore[email];

        if (DateTime.Now > expiration)
        {
            otpStore.Remove(email); // Eliminar si está expirado
            return false;
        }

        if (storedCode == inputCode)
        {
            otpStore.Remove(email); // Eliminar después de usarlo
            return true;
        }

        return false;
    }
}
