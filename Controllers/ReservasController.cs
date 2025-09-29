using APISietemasdereservas.Models.Request;
using Google.Apis.Auth;
using J_W.Estructura;
using J_W.Herramientas;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Differencing;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
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
    //[Route("api/[controller]")]
    [EnableCors("CorsApi")]
    [ApiController]
    [Authorize]
    public class ReservasController : ControllerBase
    {
        private IConfiguration _config;
        private readonly string connectionString;
        private readonly string rutasFileSaves;
        private readonly string rutasFileImages;
        private readonly string rutasFileTags;
        private readonly string xsltEnviarReserva;
        private readonly string xsltReservaCanceled;

        public ReservasController(IConfiguration config)
        {
            _config = config;
            connectionString = config
                .GetSection("ConnectionStrings")
                .GetSection("dbConnection")
                .Value;
            rutasFileSaves = config.GetSection("Configuracion").GetSection("rutaImages").Value;
            rutasFileImages = config.GetSection("Configuracion").GetSection("rutaImages").Value;
            xsltEnviarReserva = config.GetSection("Configuracion").GetSection("xsltEnviarReserva").Value;
            xsltReservaCanceled = config.GetSection("Configuracion").GetSection("xsltReservaCanceled").Value;
        }
        Dbase dbase = new Dbase();

        [HttpPost]
        [Route("api/tours/v1.0/CrearNuevoGuia")]
        public async Task<object> CrearNuevoGuia(
            IFormFile[] files,
            [FromForm] string idiomas,
            [FromForm] string nombre,
            [FromForm] string experiencia,
            [FromForm] int id_interno,
            [FromForm] string rutaAnterior,
            [FromForm] string correo,
            [FromForm] string id_user
        )
        {
            Dictionary<string, object> resultado = new Dictionary<string, object>();
            dbase.Conexion = connectionString;

            if ((id_interno == 0 && files == null) || (id_interno == 0 && files.Length == 0))
            {
                return BadRequest("Debe seleccionar un archivo.");
            }

            resultado = await MethodsLoadArchs.MethodsLoadArchs.upload_files_to_multiplatam(files, rutasFileSaves);

            string ruat = MethodsCompile.ObtenerPrimeraUrl(resultado);

            return dbase
                .Procedure(
                    "[GS].[ST_CreateGuia]",
                    "@idiomas:VARCHAR:100",
                    idiomas,
                    "@nombre:VARCHAR:100",
                    nombre,
                    "@experiencia:VARCHAR:1000",
                    experiencia,
                    "@rutaImagen:VARCHAR:1000",
                    ruat,
                    "@NombreImagen:VARCHAR:1000",
                    ruat,
                    "@correo:VARCHAR:100",
                    correo,
                    "@id_user:VARCHAR:100",
                    id_user,
                    "@id:VARCHAR:100",
                    id_interno
                )
                .RunScalar();
        }

        [HttpPost]
        [Route("api/tours/v1.0/CrearNuevoProveedor")]
        public async Task<object> CrearNuevoProveedor(
         IFormFile[] files,
         [FromForm] string nombre,
         [FromForm] string experiencia,
         [FromForm] int id_interno,
         [FromForm] int cantTours,
         [FromForm] string rutaAnterior,
         [FromForm] string correo,
         [FromForm] string id_user
     )
        {
            dbase.Conexion = connectionString;
            string uniqueFileName = "";
            string filePath = "";

            Dictionary<string, object> asa = new Dictionary<string, object>();

            if ((id_interno == 0 && files == null) || (id_interno == 0 && files.Length == 0))
            {
                return BadRequest("Debe seleccionar un archivo.");
            }

            asa = await MethodsLoadArchs.MethodsLoadArchs.upload_files_to_multiplatam(files, rutasFileSaves);

            string ruat = MethodsCompile.ObtenerPrimeraUrl(asa);

            return dbase
                .Procedure(
                    "[GS].[ST_CreateProveedor]",
                    "@nombre:VARCHAR:100",
                    nombre,
                    "@experiencia:VARCHAR:1000",
                    experiencia,
                    "@rutaImagen:VARCHAR:1000",
                    ruat,
                    "@cantTours:INT",
                    cantTours,
                    "@NombreImagen:VARCHAR:100",
                    uniqueFileName,
                    "@correo:VARCHAR:100",
                    correo,
                    "@id_user:VARCHAR:100",
                    id_user,
                    "@id:VARCHAR:100",
                    id_interno
                )
                .RunScalar();
        }

        [HttpPost]
        [Route("api/tours/v1.0/listGuias")]
        public IActionResult listGuias([FromForm] string id_user)
        {
            try
            {
                dbase.Conexion = connectionString;

                Result result = dbase.Procedure("[GS].[ST_ListGuia]", "@id_user:VARCHAR:10",
                    id_user).RunData();

                if (result?.Data?.Tables.Count == 0 || result.Data.Tables[0].Rows.Count == 0)
                {
                    return Ok(
                        new
                        {
                            Error = false,
                            Message = "No se encontraron datos en la tabla.",
                            Data = "",
                        }
                    );
                }

                var data = MethodsCompile.ConvertDataTableToJson(result.Data.Tables[0]);

                return Ok(new { Error = false, Data = data });
            }
            catch (Exception ex)
            {
                return StatusCode(
                    500,
                    new
                    {
                        Error = false,
                        Message = "Ocurrió un error al listar las carpetas.",
                        Details = ex.Message,
                    }
                );
            }
        }

        [HttpPost]
        [Route("api/tours/v1.0/listPregunFrecs")]
        public IActionResult listPregunFrecs([FromForm] string id_user)
        {
            try
            {
                dbase.Conexion = connectionString;

                Result result = dbase.Procedure("[GS].[ST_listPregunFrecs]", "@id_user:VARCHAR:10",
                    id_user).RunData();

                if (result?.Data?.Tables.Count == 0 || result.Data.Tables[0].Rows.Count == 0)
                {
                    return Ok(
                        new
                        {
                            Error = false,
                            Message = "No se encontraron datos en la tabla.",
                            Data = "",
                        }
                    );
                }

                var data = MethodsCompile.ConvertDataTableToJson(result.Data.Tables[0]);

                return Ok(new { Error = false, Data = data });
            }
            catch (Exception ex)
            {
                return StatusCode(
                    500,
                    new
                    {
                        Error = false,
                        Message = "Ocurrió un error al listar las carpetas.",
                        Details = ex.Message,
                    }
                );
            }
        }

        [HttpPost]
        [Route("api/tours/v1.0/ListProve")]
        public IActionResult listProve()
        {
            try
            {
                dbase.Conexion = connectionString;

                Result result = dbase.Procedure("[GS].[ST_ListProve]").RunData();

                if (result?.Data?.Tables.Count == 0 || result.Data.Tables[0].Rows.Count == 0)
                {
                    return Ok(
                        new
                        {
                            Error = false,
                            Message = "No se encontraron datos en la tabla.",
                            Data = "",
                        }
                    );
                }

                var data = MethodsCompile.ConvertDataTableToJson(result.Data.Tables[0]);

                return Ok(new { Error = false, Data = data });
            }
            catch (Exception ex)
            {
                return StatusCode(
                    500,
                    new
                    {
                        Error = false,
                        Message = "Ocurrió un error al listar las carpetas.",
                        Details = ex.Message,
                    }
                );
            }
        }


        [HttpPost]
        [Route("api/tours/v1.0/listProveList")]
        public IActionResult listProveList([FromForm] string id_user)
        {
            try
            {
                dbase.Conexion = connectionString;

                Result result = dbase.Procedure("[GS].[ST_ListProve]", "@id_user:VARCHAR:10", id_user).RunData();

                if (result?.Data?.Tables.Count == 0 || result.Data.Tables[0].Rows.Count == 0)
                {
                    return Ok(
                        new
                        {
                            Error = false,
                            Message = "No se encontraron datos en la tabla.",
                            Data = "",
                        }
                    );
                }

                var data = MethodsCompile.ConvertDataTableToJson(result.Data.Tables[0]);

                return Ok(new { Error = false, Data = data });
            }
            catch (Exception ex)
            {
                return StatusCode(
                    500,
                    new
                    {
                        Error = false,
                        Message = "Ocurrió un error al listar las carpetas.",
                        Details = ex.Message,
                    }
                );
            }
        }

        [HttpPost]
        [Route("api/tours/v1.0/EliminarImg")]
        public object EliminarImg([FromForm] string ruta, [FromForm] string TourID)
        {
            dbase.Conexion = connectionString;
            if (System.IO.File.Exists(ruta))
            {
                System.IO.File.Delete(ruta);
            }
            return dbase
                .Procedure("[GS].[ST_DeleteImgTours]", "@ruta:VARCHAR:100", ruta, "@TourID:VARCHAR:100", TourID)
                .RunScalar();
        }


        [HttpPost]
        [Route("api/tours/v1.0/setStatesTours")]
        public object setStatesTours([FromForm] string id, [FromForm] bool state)
        {
            dbase.Conexion = connectionString;

            return dbase
                .Procedure("[GS].[ST_SetStateTours]", "@id:VARCHAR:100", id, "@state:BIT", state)
                .RunScalar();
        }

        [HttpPost]
        [Route("api/tours/v1.0/SetState")]
        public object SetState([FromForm] string id, [FromForm] bool state)
        {
            dbase.Conexion = connectionString;

            return dbase
                .Procedure("[GS].[ST_SetStateGuia]", "@id:VARCHAR:100", id, "@state:BIT", state)
                .RunScalar();
        }


        [HttpPost]
        [Route("api/tours/v1.0/SetStateProve")]
        public object SetStateProve([FromForm] string id, [FromForm] bool state)
        {
            dbase.Conexion = connectionString;

            return dbase
                .Procedure("[GS].[ST_SetStateProve]", "@id:VARCHAR:100", id, "@state:BIT", state)
                .RunScalar();
        }

        [HttpPost]
        [Route("api/tours/v1.0/EliminarGuia")]
        public object EliminarGuia([FromForm] int id, [FromForm] string rutaImgen)
        {
            dbase.Conexion = connectionString;

            if (System.IO.File.Exists(rutaImgen))
            {
                System.IO.File.Delete(rutaImgen);
            }
            return dbase.Procedure("[GS].[ST_EliminarGuia]", "@id:VARCHAR:100", id).RunScalar();
        }


        [HttpPost]
        [Route("api/tours/v1.0/EliminarProveedor")]
        public object EliminarProveedor([FromForm] int id, [FromForm] string rutaImgen)
        {
            dbase.Conexion = connectionString;

            if (System.IO.File.Exists(rutaImgen))
            {
                System.IO.File.Delete(rutaImgen);
            }
            return dbase.Procedure("[GS].[ST_EliminarProveedor]", "@id:VARCHAR:100", id).RunScalar();
        }


        [HttpPost]
        [Route("api/tours/v1.0/GuardarTour")]
        public async Task<IActionResult> GuardarTour([FromForm] TourRequest request, [FromForm] IFormFile[] archivos)
        {
            try
            {
                dbase.Conexion = connectionString;
                Dictionary<string, object> a = new Dictionary<string, object>();
                string ruat = "";
                if (archivos.Length != 0){

                    a = await MethodsLoadArchs.MethodsLoadArchs.upload_files_to_multiplatam(archivos, rutasFileSaves);
                    var urls = a["Urls"] as List<string>;
                    if (urls != null)
                    {
                        ruat = JsonConvert.SerializeObject(urls); // JSON limpio
                    }
                     
                }
                
                string fechasJson = request.FechasMasi;
                string idiomas = string.Join(",", request.Idioma);
                string itinerarioJson = request.Itinerario;
                string archivosJson = ruat;

                var resultado = dbase
                    .Procedure(
                        "[GS].[ST_GuardarTour]",
                        "@nombre:VARCHAR(100)",
                        request.Nombre,
                        "@nombre_ing:VARCHAR(100)",
                        request.Nombre_ing,
                        "@descripcion:TEXT",
                        request.Descripcion,
                        "@descripcion_ing:TEXT",
                        request.Descripcion_ing,
                        "@duracion:VARCHAR(50)",
                        request.Duracion,
                        "@CantParticipantes:INT",
                        request.CantParticipantes,
                        "@guia:BIGINT",
                        request.Guia,
                        "@fechasMasi:JSON",
                        fechasJson,
                        "@idioma:VARCHAR(100)",
                        idiomas,
                        "@itinerario:JSON",
                        itinerarioJson,
                        "@preguntasFrecu:JSON",
                        request.preguntasFrecu,
                        "@precio:DECIMAL",
                        request.Precio,
                        "@PuntoEncuentro_Place:VARCHAR(255)",
                        request.PuntoEncuentro_Place,
                        "@PuntoEncuentro_Descripcion:VARCHAR(255)",
                        request.PuntoEncuentro_Descripcion,
                        "@url:TEXT",
                        request.Url,
                        "@infoAdicional:TEXT",
                        request.InfoAdicional,
                        "@portadaTour:VARCHAR:500",
                       request.portadaTour,
                        "@id_interno:VARCHAR:10",
                        request.id_interno,
                        "@Id_Usuario:VARCHAR:500",
                        request.id_user,
                        "@archivos:JSON",
                        archivosJson,
                        "@free:BIT",
                        request.free
                    )
                    .RunScalar();

                return Ok(new { resultado });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error al guardar el tour", error = ex.Message });
            }
        }

        [HttpPost]
        [Route("api/tours/v1.0/listTours")]
        public IActionResult listTours([FromBody] TourINfotmation request)
        {
            try
            {
                dbase.Conexion = connectionString;

                Result result = dbase
                    .Procedure("[GS].[ST_ListTours]", "@id_tour:VARCHAR(100)", request.id_tour)
                    .RunData();

                if (result?.Data?.Tables.Count == 0 || result.Data.Tables[0].Rows.Count == 0)
                {
                    return Ok(
                        new
                        {
                            Error = false,
                            Message = "No se encontraron datos en la tabla.",
                            Data = "",
                        }
                    );
                }

                var data = MethodsCompile.ConvertDataTableToJsonTopurs(
                    result.Data.Tables[0],
                    result.Data.Tables[1],
                    result.Data.Tables[2],
                    result.Data.Tables[3],
                    result.Data.Tables[4],
                    result.Data.Tables[5],
                    result.Data.Tables[7]
                );

                return Ok(new { Error = false, Data = data });
            }
            catch (Exception ex)
            {
                return StatusCode(
                    500,
                    new
                    {
                        Error = false,
                        Message = "Ocurrió un error al listar las carpetas.",
                        Details = ex.Message,
                    }
                );
            }
        }
 
        [HttpPost]
        [Route("api/tours/v1.0/CrearReserva")]
        public IActionResult CrearReserva([FromBody] ReservaRequest reserva)
        {
            try
            {
                dbase.Conexion = connectionString;
                string otpCode = OTPManager.GenerateOTP();
                DateTime otpExpiration = DateTime.Now.AddMinutes(5);
                bool enviado = true;
                bool enviado2 = true;


                if (reserva.isconfirmOTP != "1")
                {
                    OTPManager.SendOTP_WithXslt(reserva.email, otpCode);
                }

                //Se COmenta a peticion del cliente
                Result resultado = dbase
                    .Procedure(
                        "[GS].[ST_CreateReserva]",
                        "@fechaSeleccionada:DATE",
                        reserva.FechaSeleccionada,
                        "@horaSeleccionada:VARCHAR",
                        reserva.HoraSeleccionada,
                        "@idiomaselected:VARCHAR",
                        reserva.idiomaselected,
                        "@numeroAdultos:INT",
                        reserva.NumeroAdultos,
                        "@numeroNinos:INT",
                        reserva.NumeroNinos,
                        "@telefono:VARCHAR",
                        reserva.Telefono,
                        "@aceptaTerminos:BIT",
                        reserva.AceptaTerminos,
                        "@id_user:VARCHAR",
                        reserva.id_user,
                        "@id_tour:INT",
                        reserva.id_tour,
                        "@id_reserva:VARCHAR:100",
                        reserva.id_reserva,
                        //"@email:VARCHAR:100", reserva.email,
                        "@OTP:INT",
                        otpCode,
                        "@OTP_Expiration:DATETIME",
                        otpExpiration
                    )
                    .RunRow();

                if (!resultado.Error)
                {
                    var email = reserva.email;
                    var nombreCliente = resultado.Row["nombreUsuario"].ToString();
                     
               
                    if (reserva.isconfirmOTP == "1")
                    {                        
                        var NombreTour = resultado.Row["NombreTour"].ToString();
                        var NombreTourEN = resultado.Row["NombreTourEN"].ToString();
                        var fecha = reserva.FechaSeleccionada;
                        var hora = reserva.HoraSeleccionada;
                        var idioma = reserva.idiomaselected; 
                        var LinkPagina = resultado.Row["LinkPagina"].ToString();
                        var puntodeE = resultado.Row["PuntoNombre"].ToString();
                        var puntodeE_Descp = resultado.Row["PuntoEN"].ToString();
                        var nombreGuia = resultado.Row["nombreGuia"].ToString();
                        var ECorreoGuia = resultado.Row["ECorreoGuia"].ToString();
                        var idReserva = resultado.Row["idReserva"].ToString();

                        enviado = MethodsCompile.NotificarConfirmacionReserva(
                            email,
                            nombreCliente,
                            NombreTour,
                            NombreTourEN,
                            fecha,
                            hora,
                            idioma,
                            puntodeE,
                            puntodeE_Descp,
                            LinkPagina
                        );
                        enviado2 = MethodsCompile.NotificarConfirmacionGuia(
                         idReserva,
                         ECorreoGuia,
                         nombreGuia,
                         nombreCliente,
                         NombreTour,
                         NombreTourEN,
                         fecha,
                         hora,
                         idioma,
                         puntodeE,
                         puntodeE_Descp,
                         LinkPagina
                     );
        
                    }
                }

                return Ok(
                    new
                    {
                        Error = false,
                        Message = resultado.Message,
                        data = resultado.Row,
                        IsMailSend = enviado,
                    }
                );
            }
            catch (Exception ex)
            {


                return StatusCode(
                    500,
                    new
                    {
                        Error = true,
                        Message = ex.Message,
                        data = ex.Message,
                    }
                );
            }
        }

        [HttpPost]
        [Route("api/tours/v1.0/listAgenda")]
        public IActionResult listAgenda([FromForm] string id_user)
        {
            try
            {
                dbase.Conexion = connectionString;

                Result result = dbase
                    .Procedure("[GS].[ST_ListAgenda]", "@id_user:VARCHAR:10", id_user)
                    .RunData();

                if (result?.Data?.Tables.Count == 0 || result.Data.Tables[0].Rows.Count == 0)
                {
                    return Ok(
                        new
                        {
                            Error = false,
                            Message = "No se encontraron datos en la tabla.",
                            Data = "",
                        }
                    );
                }

                var tours = result
                    .Data.Tables[0]
                    .AsEnumerable()
                    .Select(row =>
                    { 
                        string duracionStr = row.Field<string>("duracion");
                        int minutosDuracion = 0;

                        if (!string.IsNullOrEmpty(duracionStr) && duracionStr.Contains(":"))
                        {
                            var partes = duracionStr.Replace("h", "").Split(':');
                            int horas = int.Parse(partes[0]);
                            int minutos = int.Parse(partes[1]);
                            minutosDuracion = (horas * 60) + minutos;
                        }

                        DateTime end = Convert.ToDateTime(row.Field<string>("fecha")).AddMinutes(minutosDuracion);

                        return new
                        {
                            id = Convert.ToInt32(row.Field<decimal>("id")),
                            start = Convert.ToDateTime(row.Field<string>("fecha")),
                            end = end,
                            title = row.Field<string>("title"),
                            seats = row.Field<string>("seats"),
                            flag = row.Field<string>("flag"),
                            idioma = row.Field<string>("idioma"),
                            reservasCanceladas = row.Field<int>("reservasCanceladas"),
                            reservasConfirmadas = row.Field<int>("reservasConfirmadas"),
                            reservasPendientes = row.Field<int>("reservasPendientes"),
                            horario = row.Field<string>("horario"),
                            EstadoReserva = row.Field<string>("EstadoReserva"),
                            fechaVisble = row.Field<string>("fechaVisble"),
                        };
                    })
                    .ToList();

                return Ok(new { Error = false, Data = tours });
            }
            catch (Exception ex)
            {
                return StatusCode(
                    500,
                    new
                    {
                        Error = true,
                        Message = "Ocurrió un error al listar los tours.",
                        Details = ex.Message,
                    }
                );
            }
        }

        [HttpPost]
        [Route("api/tours/v1.0/ValidarOTP")]
        public IActionResult ValidarOTP([FromBody] OTPRequest request)
        {
            try
            {
                dbase.Conexion = connectionString;

                var resultado = dbase
                    .Procedure(
                        "[GS].[ST_ValidarOTP]",
                        "@id_reserva:VARCHAR",
                        request.reserva,
                        "@otp:INT",
                        request.OTP,
                        "@id_user:varchar:10",
                        request.id_user
                    )
                    .RunRow();

                int validacion = Convert.ToInt32(resultado.Row["isConfirmada"].ToString());

                if (validacion == 0)
                {
                    return Ok(new { Error = true, Message = "Código inválido o expirado" });
                }
                else
                {
                    var email = request.email;
                    var subject =
                        $"RESERVA DE TOUR {resultado.Row["ID"].ToString()} CONFIRMADA EL {resultado.Row["FechaSeleccionada"].ToString()} A LAS {resultado.Row["HoraSeleccionada"].ToString()}";

                    var xsltParams = new Dictionary<string, string>
                    {
                        { "Nombre", resultado.Row["nombreUsuario"].ToString() },
                        { "NombreTour", resultado.Row["NombreTour"].ToString() },
                        { "fecha", resultado.Row["FechaSeleccionada"].ToString()},
                        { "hora", resultado.Row["HoraSeleccionada"].ToString()},
                    };

                    OTPManager.SendEmailWithXsltTemplate(
                        email,
                        subject,
                        xsltParams,
                        xsltEnviarReserva
                    );
                }

                return Ok(new { Error = false, Message = "Código válido. Reserva confirmada." });
            }
            catch (Exception ex)
            {
                return StatusCode(
                    500,
                    new
                    {
                        Error = true,
                        Message = "Error al validar el código",
                        data = ex.Message,
                    }
                );
            }
        }

        [HttpPost]
        [Route("api/tours/v1.0/ReenviarOTP")]
        public IActionResult ReenviarOTP([FromBody] OTPRequest request)
        {
            try
            {
                dbase.Conexion = connectionString;

                string nuevoOTP = OTPManager.GenerateOTP();
                DateTime nuevaExpiracion = DateTime.Now.AddMinutes(5);

                dbase
                    .Procedure(
                        "[GS].[ST_ReenviarOTP]",
                        "@id_reserva:VARCHAR",
                        request.reserva,
                        "@otp:INT",
                        nuevoOTP,
                        "@nueva_expiracion:DATETIME",
                        nuevaExpiracion
                    )
                    .RunData();

                // Enviar correo
                if (!string.IsNullOrEmpty(request.email))
                {
                    OTPManager.SendOTP_WithXslt(request.email, nuevoOTP);
                }

                return Ok(new { Error = false, Message = "Código reenviado con éxito" });
            }
            catch (Exception ex)
            {
                return StatusCode(
                    500,
                    new
                    {
                        Error = true,
                        Message = "Error al reenviar el código",
                        data = ex.Message,
                    }
                );
            }
        }

        [HttpPost]
        [Route("api/tours/v1.0/listReservaClientes")]
        public IActionResult listReservaClientes([FromBody] UserReservas request)
        {
            try
            {
                dbase.Conexion = connectionString;

                Result result = dbase
                    .Procedure(
                        "[GS].[ST_ListReservasClientes]",
                        "@id_user:VARCHAR:10",
                        request.id_user,
                        "@menuReserva:VARCHAR:10",
                        request.menuReserva
                    )
                    .RunData();

                if (result?.Data?.Tables.Count == 0 || result.Data.Tables[0].Rows.Count == 0)
                {
                    return Ok(
                        new
                        {
                            Error = false,
                            Message = "No se encontraron datos en la tabla.",
                            Data = "",
                        }
                    );
                }

                var data = MethodsCompile.ConvertDataTableToJson1(
                    result.Data.Tables[0],
                    result.Data.Tables[1]
                );

                return Ok(new { Error = false, Data = data });
            }
            catch (Exception ex)
            {
                return StatusCode(
                    500,
                    new
                    {
                        Error = false,
                        Message = "Ocurrió un error al listar las carpetas.",
                        Details = ex.Message,
                    }
                );
            }
        }

        [HttpPost]
        [Route("api/tours/v1.0/listAsk")]
        public IActionResult listAsk()
        {
            try
            {
                dbase.Conexion = connectionString;

                Result result = dbase.Procedure("[GS].[ST_ListPreguntasFrecuentes]").RunData();

                if (result?.Data?.Tables.Count == 0 || result.Data.Tables[0].Rows.Count == 0)
                {
                    return Ok(
                        new
                        {
                            Error = false,
                            Message = "No se encontraron datos en la tabla.",
                            Data = "",
                        }
                    );
                }

                var data = MethodsCompile.ConvertDataTableToJson(result.Data.Tables[0]);

                return Ok(new { Error = false, Data = data });
            }
            catch (Exception ex)
            {
                return StatusCode(
                    500,
                    new
                    {
                        Error = false,
                        Message = "Ocurrió un error al listar las carpetas.",
                        Details = ex.Message,
                    }
                );
            }
        }

        [HttpPost]
        [Route("api/tours/v1.0/listPersonasReservas")]
        public IActionResult listPersonasReservas([FromBody] ListRqest request)
        {
            try
            {
                dbase.Conexion = connectionString;

                Result result = dbase
                    .Procedure(
                        "[GS].[ST_ReservaListPersonas]",
                        "@id_tour:INT",
                        request.id_tour,
                        "@fechaSelect:VARCHAR:10",
                        request.fechaSelect,
                        "@idiomaselect:VARCHAR:10",
                        request.idiomaselect,
                        "@horaselect:VARCHAR:5",
                        request.horaselect
                    )
                    .RunData();

                if (result?.Data?.Tables.Count == 0 || result?.Data?.Tables[0].Rows.Count == 0)
                {
                    return Ok(
                        new
                        {
                            Error = false,
                            Message = "No se encontraron datos en la tabla.",
                            Data = "",
                        }
                    );
                }

                var data = MethodsCompile.ConvertDataTableToJson(result.Data.Tables[0]);

                return Ok(new { Error = false, Data = data });
            }
            catch (Exception ex)
            {
                return StatusCode(
                    500,
                    new
                    {
                        Error = false,
                        Message = "Ocurrió un error al listar las carpetas.",
                        Details = ex.Message,
                    }
                );
            }
        }

        public class TokenRequest
        {
            public string Credential { get; set; }
        }

        [HttpPost]
        [Route("api/tours/v1.0/google-login")]
        public async Task<IActionResult> GoogleLogin([FromBody] TokenRequest request)
        {
            try
            {
                dbase.Conexion = connectionString;
                var payload = await GoogleJsonWebSignature.ValidateAsync(request.Credential);

                // Extrae datos del usuario
                var email = payload.Email;
                var name = payload.Name;
                var picture = payload.Picture;

                Result result = dbase
                    .Procedure(
                        "[GS].[ST_UserCreateRegisterGoogle]",
                        "@email:VARCHAR:1000",
                        email,
                        "@name:VARCHAR:1000",
                        name,
                        "@picture:VARCHAR:1000",
                        picture
                    )
                    .RunData();

                if (!result.Error)
                {
                    if (result?.Data?.Tables?.Count == 0 || result?.Data?.Tables[0].Rows.Count == 0)
                    {
                        return Ok(new { Error = false, Data = "Vacio" });
                    }
                    else
                    {
                        var data = MethodsCompile.ConvertDataTableToJson(result.Data.Tables[0]);
                        return Ok(new { Error = false, Data = data });
                    }
                }
                else
                {
                    return StatusCode(200, new { Error = true, Message = result.Message });
                }
            }
            catch (Exception ex)
            {
                return Unauthorized(new { message = "Token inválido", error = ex.Message });
            }
        }

        [HttpPost]
        [Route("api/tours/v1.0/CancelarReserva")]
        public IActionResult CancelarReserva([FromBody] CancelReserva request)
        {
            try
            {
                dbase.Conexion = connectionString;

                Result resultado = dbase
                    .Procedure(
                        "[GS].[ST_CancelarReserva]",
                        "@id_reserva:VARCHAR:1000",
                        request.id_reserva,
                        "@selectedMotivo:VARCHAR:1000",
                        request.selectedMotivo
                    )
                    .RunRow();
                bool enviado = true;

                if (!resultado.Error)
                {
                    var email = resultado.Row["MailUSer"].ToString();
                    var Nombre = resultado.Row["NameUser"].ToString();
                    var NombreTour = resultado.Row["nombretour"].ToString();
                    var NombreTourEN = resultado.Row["NombreTourEN"].ToString();
                    var fecha = resultado.Row["fecha"].ToString();
                    var hora = resultado.Row["hora"].ToString();    
                    var idioma = resultado.Row["idioma"].ToString();
                    var guru = resultado.Row["GUIA"].ToString();
                    var LinkPagina = resultado.Row["LinkPagina"].ToString();
                    var emailGuru = resultado.Row["emailGuru"].ToString();


                    enviado = MethodsCompile.NotificarCancelacionReserva_USUARIO(
                        email, 
                        Nombre,
                        NombreTour,
                        NombreTourEN,
                        fecha,
                        hora,
                        idioma,
                        LinkPagina
                    );
 
                    enviado = MethodsCompile.NotificarCancelacionReserva_GUIA(
                        request.id_reserva,
                        emailGuru,
                        guru,
                        Nombre,
                        NombreTour,
                        NombreTourEN,
                        fecha,
                        hora,
                        idioma
                    );
                }

                return Ok(new { Error = resultado.Error, Message = "Cancelacion Realizada" });
            }
            catch (Exception ex)
            {
                return StatusCode(
                    500,
                    new
                    {
                        Error = true,
                        Message = "Error al cancelar la reserva",
                        data = ex.Message,
                    }
                );
            }
        }

        [HttpPost]
        [Route("api/tours/v1.0/Encriptador")]
        public IActionResult Encriptador([FromBody] UserLogIn model)
        {
            var encriptado = EncryptionHelper.Encriptar(model.user + model.contrase);
            return Ok(new { Error = false, Data = encriptado });
        }

        [HttpPost]
        [Route("api/tours/v1.0/Logn")]
        public IActionResult LogIn([FromBody] UserLogIn model)
        {
            try
            {
                var encriptado = EncryptionHelper.Encriptar(model.user + model.contrase);

                dbase.Conexion = connectionString;
                Result result = dbase
                    .Procedure("[GS].[ST_SessionToken]", "@id:VARCHAR:1000", encriptado)
                    .RunData();

                if (!result.Error)
                {
                    if (result?.Data?.Tables?.Count == 0 || result?.Data?.Tables[0].Rows.Count == 0)
                    {
                        return Ok(new { Error = false, Data = "Vacio" });
                    }
                    else
                    {
                        var data = MethodsCompile.ConvertDataTableToJson(result.Data.Tables[0]);
                        return Ok(new { Error = false, Data = data });
                    }
                }
                else
                {
                    return StatusCode(200, new { Error = true, Message = result.Message });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(
                    500,
                    new
                    {
                        Error = false,
                        Message = "Ocurrió un error al listar las carpetas.",
                        Details = ex.Message,
                    }
                );
            }
        }

        [HttpPost]
        [Route("api/tours/v1.0/GuardarOpinion")]
        public async Task<IActionResult> GuardarOpinion(
            [FromForm] int rating,
            [FromForm] string opinionText,
            [FromForm] string id_Tour,
            [FromForm] IFormFile[] photos,
            [FromForm] int userId
        )
        {
            string imagePath = null;
            string uniqueFileName = null;
            Dictionary<string, object> asa = new Dictionary<string, object>();
            dbase.Conexion = connectionString;

            if(photos.Length != 0)
            {
                asa = await MethodsLoadArchs.MethodsLoadArchs.upload_files_to_multiplatam(photos, rutasFileSaves);

                imagePath = JsonConvert.SerializeObject(asa["Urls"]);
            }
             
            try
            {
                var result = dbase
                    .Procedure(
                        "[GS].[ST_InsertarOpinion]",
                        "@Rating:INT",
                        rating,
                        "@OpinionText:VARCHAR:1000",
                        opinionText,
                        "@ImagePath:VARCHAR:-1",
                        imagePath,
                        "@id_Tour:VARCHAR:10",
                        id_Tour,
                        "@UserId:INT",
                        userId
                    )
                    .RunScalar();

                return Ok(new { message = result.Message, data = result.Error });
            }
            catch (Exception ex)
            {
                if (!string.IsNullOrEmpty(imagePath) && System.IO.File.Exists(imagePath))
                {
                    try
                    {
                        System.IO.File.Delete(imagePath);
                    }
                    catch (Exception deleteEx)
                    {
                        return StatusCode(
                            500,
                            new
                            {
                                message = "Error al enviar la opinión y al intentar eliminar la imagen.",
                                error = ex.Message,
                                deleteError = deleteEx.Message,
                            }
                        );
                    }
                }
                return StatusCode(
                    500,
                    new { message = "Error al enviar la opinión", error = ex.Message }
                );
            }
        }

        [HttpPost]
        [Route("api/tours/v1.0/guardar-configuracion")]
        public async Task<IActionResult> GuardarConfiguracion([FromForm] ConfiguracionModel model)
        {
            string uniqueFileName = null;
            string imagePath = null;

            if (model.ImagenPrincipal != null && model.ImagenPrincipal.Length > 0)
            {
                if (!Directory.Exists(rutasFileSaves))
                    Directory.CreateDirectory(rutasFileSaves);

                uniqueFileName = $"{Guid.NewGuid()}_{model.ImagenPrincipal.FileName}";
                imagePath = Path.Combine(rutasFileSaves, uniqueFileName);

                using (var stream = new FileStream(imagePath, FileMode.Create))
                {
                    await model.ImagenPrincipal.CopyToAsync(stream);
                }
            }

            try
            {
                dbase.Conexion = connectionString;

                var result = dbase
                    .Procedure(
                        "[GS].[ST_GuardarConfiguracion]",
                        "@Titulo:VARCHAR:300",
                        model.Titulo,
                        "@Descripcion:VARCHAR:1000",
                        model.Descripcion,
                        "@TituloEN:VARCHAR:300",
                        model.TituloEN,
                        "@DescripcionEN:VARCHAR:1000",
                        model.DescripcionEN,
                        "@ImagenPrincipal:VARCHAR:1000",
                        uniqueFileName ?? "",
                        "@Terminos:VARCHAR:8000",
                        model.Terminos,
                        "@Telefono:VARCHAR:50",
                        model.Telefono,
                        "@Email:VARCHAR:150",
                        model.Email,
                        "@Direccion:VARCHAR:500",
                        model.Direccion,
                        "@Facebook:VARCHAR:150",
                        model.Facebook,
                        "@Instagram:VARCHAR:150",
                        model.Instagram,
                        "@WhatsApp:VARCHAR:150",
                        model.WhatsApp,
                        "@UsuarioCorreo:VARCHAR:150",
                        model.UsuarioCorreo,
                        "@ClaveCorreo:VARCHAR:150",
                        model.ClaveCorreo,
                        "@LinkPagina:VARCHAR:300",
                        model.LinkPagina,
                        "@MensajeReserva:VARCHAR:300",
                        model.MensajeReserva,
                        "@MensajeCancelacion:VARCHAR:300",
                        model.MensajeCancelacion,
                        "@Faq:VARCHAR:1000",
                        model.Faq
                    )
                    .RunScalar();

                return Ok(new { mensaje = result.Message, error = result.Error });
            }
            catch (Exception ex)
            {
                return StatusCode(
                    500,
                    new
                    {
                        mensaje = "Error interno al guardar la configuración.",
                        error = ex.Message,
                    }
                );
            }
        }

        [HttpPost]
        [Route("api/tours/v1.0/ObtenerConfiguracionSystem")]
        public IActionResult ObtenerConfiguracionSystem()
        {
            try
            {
                dbase.Conexion = connectionString;

                Result result = dbase.Procedure("[GS].[ST_ObtenerConfig]").RunData();

                if (result?.Data?.Tables.Count == 0 || result.Data.Tables[0].Rows.Count == 0)
                {
                    return Ok(
                        new
                        {
                            Error = false,
                            Message = "No se encontraron datos en la tabla.",
                            Data = "",
                        }
                    );
                }

                var data = MethodsCompile.ConvertDataTableToJson(result.Data.Tables[0]);

                return Ok(new { Error = false, Data = data });
            }
            catch (Exception ex)
            {
                return StatusCode(
                    500,
                    new
                    {
                        Error = false,
                        Message = "Ocurrió un error al listar las carpetas.",
                        Details = ex.Message,
                    }
                );
            }
        }

        [HttpPost]
        [Route("api/tours/v1.0/GuardarPQRS")]
        public IActionResult GuardarPQRS(
            [FromForm] string name = null,
            [FromForm] string email = null,
            [FromForm] string message = null,
            [FromForm] string id_tour = null,
            [FromForm] string id_reserva = null,
            [FromForm] string id_user = null
        )
        {
            dbase.Conexion = connectionString;

            try
            {
                var result = dbase
                    .Procedure(
                        "[GS].[ST_GuardarPQRS]",
                        "@name:VARCHAR:1000",
                        name,
                        "@email:VARCHAR:1000",
                        email,
                        "@message:VARCHAR:100",
                        message,
                        "@id_tour:VARCHAR:100",
                        id_tour,
                        "@id_reserva:VARCHAR:100",
                        id_reserva,
                        "@id_user:VARCHAR:100",
                        id_user
                    )
                    .RunScalar();

                return Ok(new { message = result.Message, data = result.Error });
            }
            catch (Exception ex)
            {
                return StatusCode(
                    500,
                    new { message = "Error al enviar la opinión", error = ex.Message }
                );
            }
        }

        [HttpPost]
        [Route("api/tours/v1.0/MarcrMjsLeido")]
        public IActionResult MarcrMjsLeido(
            [FromForm] string id_user = null,
            [FromForm] string id_reserva = null
        )
        {
            dbase.Conexion = connectionString;

            try
            {
                var result = dbase
                    .Procedure(
                        "[GS].[ST_MarcrMjsLeido]",
                        "@id_user:VARCHAR:100",
                        id_user,
                        "@id_reserva:VARCHAR:100",
                        id_reserva
                    )
                    .RunScalar();

                return Ok(new { message = result.Message, data = result.Error });
            }
            catch (Exception ex)
            {
                return StatusCode(
                    500,
                    new { message = "Error al enviar la opinión", error = ex.Message }
                );
            }
        }

        [HttpPost]
        [Route("api/tours/v1.0/CancelarReservaAdmin")]
        public IActionResult CancelarReservaAdmin([FromBody] ListRqest request)
        {
            try
            {
                dbase.Conexion = connectionString;

                Result result = dbase
                    .Procedure(
                        "[GS].[ST_CancelarReservaAdmin]",
                        "@id_tour:INT",
                        request.id_tour,
                        "@fechaSelect:VARCHAR:10",
                        request.fechaSelect,
                        "@idiomaselect:VARCHAR:10",
                        request.idiomaselect,
                        "@id_user:VARCHAR:10",
                        request.id_user,
                        "@horaselect:VARCHAR:5",
                        request.horaselect
                    )
                    .RunData();

                var table = result.Data.Tables[0];

                if (table.Rows.Count > 0)
                {
                    foreach (DataRow row in table.Rows)
                    {
                        var Email = row["Email"].ToString();
                        var Mensaje = row["mensaje"].ToString();
                        var NombreCliente = row["Nombre"].ToString();
                        var Id_reserva = row["Id_reserva"].ToString();
                        var NombreGuru = row["NombreGuru"].ToString();
                        var Idioma = row["Idioma"].ToString();
                        var LinkWeb = row["LinkWeb"].ToString();

                        MethodsCompile.NotificarCancelacionReserva(
                            Email,
                            NombreCliente,
                            Id_reserva,
                            NombreGuru,
                            Idioma,
                            LinkWeb
                        );
                    }
                }
                return Ok(new { message = result.Message, data = result.Error });
            }
            catch (Exception ex)
            {
                return StatusCode(
                    500,
                    new
                    {
                        Error = false,
                        Message = "Ocurrió un error al listar las carpetas.",
                        Details = ex.Message,
                    }
                );
            }
        }


        [HttpPost]
        [Route("api/tours/v1.0/ActivarReserva")]
        public IActionResult ActivarReserva([FromBody] ListRqest request)
        {
            try
            {
                dbase.Conexion = connectionString;

                Result result = dbase
                    .Procedure(
                        "[GS].[ST_ActivarReservaAdmin]",
                        "@id_tour:INT",
                        request.id_tour,
                        "@fechaSelect:VARCHAR:10",
                        request.fechaSelect,
                        "@idiomaselect:VARCHAR:10",
                        request.idiomaselect,
                        "@id_user:VARCHAR:10",
                        request.id_user,
                        "@horaselect:VARCHAR:5",
                        request.horaselect
                    )
                    .RunData();

                var table = result.Data.Tables[0];

                if (table.Rows.Count > 0)
                {
                    foreach (DataRow row in table.Rows)
                    {
                        var Email = row["Email"].ToString();
                        var Mensaje = row["mensaje"].ToString();
                        var NombreCliente = row["Nombre"].ToString();
                        var Id_reserva = row["Id_reserva"].ToString();
                        var NombreGuru = row["NombreGuru"].ToString();
                        var Idioma = row["Idioma"].ToString();
                        var LinkWeb = row["LinkWeb"].ToString();

                        MethodsCompile.NotificarReactivacionReserva(
                            Email,
                            NombreCliente,
                            Id_reserva,
                            NombreGuru,
                            Idioma,
                            LinkWeb
                        );
                    }
                }
                return Ok(new { message = result.Message, data = result.Error });
            }
            catch (Exception ex)
            {
                return StatusCode(
                    500,
                    new
                    {
                        Error = false,
                        Message = "Ocurrió un error al listar las carpetas.",
                        Details = ex.Message,
                    }
                );
            }
        }

        [HttpPost]
        [Route("api/tours/v1.0/NotificarSolicitudOpinion")]
        public IActionResult NotificarSolicitudOpinion([FromBody] ListRqest request)
        {
            try
            {
                dbase.Conexion = connectionString;

                Result result = dbase
                    .Procedure(
                        "[GS].[ST_NotificarSolicitudOpinion]",
                        "@id_tour:INT",
                        request.id_tour,
                        "@fechaSelect:VARCHAR:10",
                        request.fechaSelect,
                        "@idiomaselect:VARCHAR:10",
                        request.idiomaselect,
                        "@id_user:VARCHAR:10",
                        request.id_user,
                        "@horaselect:VARCHAR:5",
                        request.horaselect
                    )
                    .RunData();

                var table = result.Data.Tables[0];

                if (table.Rows.Count > 0)
                {
                    foreach (DataRow row in table.Rows)
                    {
                        var Email = row["Email"].ToString();
                        var Mensaje = row["mensaje"].ToString();
                        var NombreCliente = row["Nombre"].ToString();
                        var Id_reserva = row["Id_reserva"].ToString();
                        var NombreGuru = row["NombreGuru"].ToString();
                        var Idioma = row["Idioma"].ToString();
                        var LinkWeb = row["LinkWeb"].ToString();
                        var Nombre_Espaniol = row["Nombre_Espaniol"].ToString();
                        var Nombre_Ingles = row["Nombre_Ingles"].ToString();

                        MethodsCompile.NotificarSolicitudOpinionReserva(
                            Email,
                            NombreCliente,
                            Id_reserva,
                            NombreGuru,
                            Idioma,
                            LinkWeb,
                            Nombre_Ingles,
                            Nombre_Espaniol
                        );
                    }
                }
                return Ok(new { message = result.Message, data = result.Error });
            }
            catch (Exception ex)
            {
                return StatusCode(
                    500,
                    new
                    {
                        Error = false,
                        Message = "Ocurrió un error al listar las carpetas.",
                        Details = ex.Message,
                    }
                );
            }
        }

        [HttpPost]
        [Route("api/tours/v1.0/NotificarClientes")]
        public IActionResult NotificarClientes([FromBody] ListRqest request)
        {
            try
            {
                dbase.Conexion = connectionString;

                Result result = dbase
                    .Procedure(
                        "[GS].[ST_NotificarClientes]",
                        "@id_tour:INT",
                        request.id_tour,
                        "@fechaSelect:VARCHAR:10",
                        request.fechaSelect,
                        "@idiomaselect:VARCHAR:10",
                        request.idiomaselect,
                        "@horaselect:VARCHAR:5",
                        request.horaselect,
                        "@mensaje:VARCHAR(MAX)",
                        request.mensaje,
                        "@id_user:VARCHAR:8",
                        request.id_user
                    )
                    .RunData();

                var table = result.Data.Tables[0];

                if (table.Rows.Count > 0)
                {
                    foreach (DataRow row in table.Rows)
                    {
                        var Email = row["Email"].ToString();
                        var Mensaje = row["mensaje"].ToString();
                        var NombreCliente = row["Nombre"].ToString();
                        var Id_reserva = row["Id_reserva"].ToString();
                        var NombreGuru = row["NombreGuru"].ToString();
                        var Idioma = row["Idioma"].ToString();
                        var LinkWeb = row["LinkWeb"].ToString();

                        MethodsCompile.MensajePorDifusion(
                            Email,
                            Mensaje,
                            NombreCliente,
                            Id_reserva,
                            NombreGuru,
                            Idioma,
                            LinkWeb
                        );
                    }
                }
                var data = MethodsCompile.ConvertDataTableToJson(table);

                return Ok(new { Error = false, Data = data });
            }
            catch (Exception ex)
            {
                return StatusCode(
                    500,
                    new
                    {
                        Error = true,
                        Message = "Ocurrió un error al listar las carpetas.",
                        Details = ex.Message,
                    }
                );
            }
        }

        [HttpPost]
        [Route("api/tours/v1.0/RegisterClient")]
        public IActionResult RegisterClient([FromBody] UserRegister model)
        {
            try
            { 
                var encriptado = EncryptionHelper.Encriptar(model.user + model.contrase);

                dbase.Conexion = connectionString;
                Result result = dbase
                    .Procedure(
                        "[GS].[ST_UserRegister]",
                        "@user:VARCHAR:100",
                        model.user,
                        "@contrase:VARCHAR:100",
                        model.contrase,
                        "@email:VARCHAR:100",
                        model.email,
                        "@nombre:VARCHAR:100",
                        model.nombre,
                        "@ciudad:VARCHAR:100",
                        model.ciudad,
                        "@claveEncriptada:VARCHAR:1000",
                        encriptado
                    )
                    .RunData();

                if (!result.Error)
                {
                    return Ok(new { Error = false, Message = "Usuario registrado correctamente." });
                }
                else
                {
                    return StatusCode(200, new { Error = true, Message = result.Message });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(
                    500,
                    new
                    {
                        Error = true,
                        Message = "Ocurrió un error al registrar el usuario.",
                        Details = ex.Message,
                    }
                );
            }
        }

        [HttpPost]
        [Route("api/tours/v1.0/ResetPassaword")]
        public IActionResult ResetPassaword([FromBody] ResetUserRegister model)
        {
            try
            {
                string CODEOTP = OTPManager.GenerateOTP();
                dbase.Conexion = connectionString;
                Result result = dbase
                    .Procedure(
                        "[GS].[ST_PassWordResetUserRegister]",
                        "@email:VARCHAR:100", model.email,
                        "@OTPUSER:VARCHAR:100" , CODEOTP
                    )
                    .RunRow();

                if (!result.Error)
                {
                    OTPManager.SendOTP_WithXslt(model.email, CODEOTP);
                    return Ok(new { Error = false, data= result});
                }
                else
                {
                    return StatusCode(200, new { Error = true, Message = result.Message });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(
                    500,
                    new
                    {
                        Error = true,
                        Message = "Ocurrió un error al registrar el usuario.",
                        Details = ex.Message,
                    }
                );
            }
        }

        [HttpPost]
        [Route("api/tours/v1.0/VerifyOtp")]
        public IActionResult VerifyOtp([FromBody] resert model)
        {
            try
            {
                dbase.Conexion = connectionString;

                var encriptado = EncryptionHelper.Encriptar(model.email + model.password);
                Result result = dbase
                    .Procedure(
                        "[GS].[ST_PassWordResetUserRegister]",
                        "@email:VARCHAR:100", model.email,
                        "@OTPUSER:VARCHAR:100", model.otpcode,
                        "@OP:VARCHAR:100", "MOD",
                        "@newPass:VARCHAR:1000", encriptado
                    )
                    .RunRow();

                if (!result.Error)
                { 
                    return Ok(new { Error = false, data = result });
                }
                else
                {
                    return StatusCode(200, new { Error = true, Message = result.Message });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(
                    500,
                    new
                    {
                        Error = true,
                        Message = "Ocurrió un error al registrar el usuario.",
                        Details = ex.Message,
                    }
                );
            }
        }


        [HttpPost]
        [Route("api/tours/v1.0/CrearPreguntasFrecuentes")]
        public IActionResult CrearPreguntasFrecuentes(
            [FromForm] string pregunta_Esp = null,
            [FromForm] string pregunta_Ing = null,
            [FromForm] string res_Esp = null,
            [FromForm] string res_Ing = null,
            [FromForm] string id_interno = null,
            [FromForm] string id_user = null
        )
        {
            dbase.Conexion = connectionString;

            try
            {
                var result = dbase
                    .Procedure(
                        "[GS].[ST_GuardarPreguntas]",
                        "@pregunta_Esp:VARCHAR:100",
                        pregunta_Esp,
                        "@pregunta_Ing:VARCHAR:100",
                        pregunta_Ing,
                        "@res_Esp:VARCHAR:100",
                        res_Esp,
                        "@res_Ing:VARCHAR:100",
                        res_Ing,
                        "@id_interno:VARCHAR:100",
                        id_interno,
                        "@id_user:VARCHAR:100",
                        id_user
                    )
                    .RunScalar();

                return Ok(new { message = result.Message, error = result.Error });
            }
            catch (Exception ex)
            {
                return StatusCode(
                    500,
                    new { message = "Error al enviar la opinión", error = ex.Message }
                );
            }
        }



    }
}
