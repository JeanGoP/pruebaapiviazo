using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace APISietemasdereservas.Models
{

    public class Operacional
    {
        [Required(ErrorMessage = "El campo clase no puede estar vacío.")]
        public string Clase { get; set; }

        [Required(ErrorMessage = "El campo Metodo no puede estar vacío.")]
        public string Metodo { get; set; }

        [Required(ErrorMessage = "El campo Params no puede estar vacío.")]
        public string Params { get; set; }

        [Required(ErrorMessage = "El campo id_user no puede estar vacío.")]
        public string id_user { get; set; }

        [Required(ErrorMessage = "El campo token no puede estar vacío.")]
        public string token { get; set; }
        public string ruta { get; set; }

    }
}
 
