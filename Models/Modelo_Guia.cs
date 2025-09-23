using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Web;

namespace APISietemasdereservas.Models.Request
{
    public class Modelo_Guia
    {

        [JsonPropertyName("nombre")]
        public string nombre { get; set; }
        [JsonPropertyName("experiencia")]
        public string experiencia { get; set; }
        [JsonPropertyName("idiomas")]
        public string idiomas { get; set; }
        [JsonPropertyName("id_interno")]
        public string id_interno { get; set; }


    }

    public class MailRequest
    {
        public string Correo { get; set; }
        public string Password { get; set; }
        public string HostImap { get; set; }
        public int Puerto { get; set; }

        public List<string> RemitentesFiltro { get; set; } // Cambio aquí
        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
    }

}
