using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

public class CancelReserva
{
    public string id_reserva { get; set; }
    public string selectedMotivo { get; set; }
}

public class ListRqest
{
    public int id_tour { get; set; }
    public string horaselect { get; set; }
    public string idiomaselect { get; set; }
    public string fechaSelect { get; set; }

    public string? mensaje { get; set; }
    public string? id_user { get; set; }
}
public class OTPRequest
{
    public string reserva { get; set; }
    public int OTP { get; set; }

    public string email { get; set; }
    public string? id_user { get; set; }
}
public class TourINfotmation
{
    public string? id_tour { get; set; }
}
public class UserReservas
{
    public string id_user { get; set; }
    public string? menuReserva { get; set; }
}

public class TourRequest
{
    public string Descripcion { get; set; }
    public string Descripcion_ing { get; set; }
    public string Duracion { get; set; }
    public  string FechasMasi { get; set; }
    public int Guia { get; set; }
    public int CantParticipantes { get; set; }
    public List<string> Idioma { get; set; }
    public string InfoAdicional { get; set; }
    public string Itinerario { get; set; }
    public string preguntasFrecu { get; set; }
    public string Nombre { get; set; }
    public string Nombre_ing { get; set; }
    public decimal Precio { get; set; }
    public string PuntoEncuentro_Place { get; set; }
    public string PuntoEncuentro_Descripcion { get; set; }
    public string Place { get; set; }
    public string id_user { get; set; }
    public string id_interno { get; set; }
    public string portadaTour { get; set; }

    public string Url { get; set; }
    public string id_cuidad { get; set; }
    public bool free { get; set; }
}
public class ConfiguracionModel
{
    public string Titulo { get; set; }
    public string Descripcion { get; set; }
    public string TituloEN { get; set; }
    public string DescripcionEN { get; set; }
    public IFormFile? ImagenPrincipal { get; set; }
    public string Terminos { get; set; }
    public string Telefono { get; set; }
    public string Email { get; set; }
    public string Direccion { get; set; }
    public string Facebook { get; set; }
    public string Instagram { get; set; }
    public string WhatsApp { get; set; }
    public string UsuarioCorreo { get; set; }
    public string ClaveCorreo { get; set; }
    public string LinkPagina { get; set; }
    public string MensajeReserva { get; set; }
    public string MensajeCancelacion { get; set; }
    public string Faq { get; set; }
}


public class ReservaRequest
{
    public string FechaSeleccionada { get; set; }
    public string HoraSeleccionada { get; set; }
    public string idiomaselected { get; set; }
    public int NumeroAdultos { get; set; }

    public int NumeroNinos { get; set; }
    public string Telefono { get; set; }
    public bool AceptaTerminos { get; set; }
    public bool isInvited { get; set; }
    public string nombreuser { get; set; }
    public string id_user { get; set; }
    public int id_tour { get; set; }
    public string email { get; set; }
    public string isconfirmOTP { get; set; }
    public string id_reserva { get; set; }
}

public class FechaHorario
{
    public string Fecha { get; set; }
    public List<string> Horarios { get; set; }
}

public class ItinerarioItem
{
    public long Id { get; set; }
    public string Lugar { get; set; }
    public string Hora { get; set; }
    public string Descripcion { get; set; }
}

public class AgendaDto
{
    public long Id { get; set; }
    public string TourID { get; set; }
    public string Fecha { get; set; }
    public string FechaVisible { get; set; }
    public string Horario { get; set; }
    public string Hora { get; set; }
    public string Duracion { get; set; }
    public string Title { get; set; }
    public string CantidadInscrito { get; set; }
    public int CantidadMaxima { get; set; }
    public string Seats { get; set; }
    public string Flag { get; set; }
    public string Idioma { get; set; }
    public int ReservasConfirmadas { get; set; }
    public int ReservasPendientes { get; set; }
    public int ReservasCanceladas { get; set; }
    public string EstadoReserva { get; set; }

    public List<AgendaOrderDto> Orders { get; set; } = new();
}
public class AgendaOrderDto
{
    public string Id { get; set; }
    public string ProductCode { get; set; }
    public string Date { get; set; }
    public int Adultos { get; set; }
    public int Ninos { get; set; }
    public string Customer { get; set; }
    public string Telefono { get; set; }
    public string Status { get; set; }
}
