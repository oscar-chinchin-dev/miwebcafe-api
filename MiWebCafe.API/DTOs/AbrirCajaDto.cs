using System.ComponentModel.DataAnnotations;

namespace MiWebCafe.API.DTOs
{
    public class AbrirCajaDto
    {
        [Required(ErrorMessage = "El monto inicial es obligatorio")]
        [Range(0, double.MaxValue, ErrorMessage = "El monto inicial debe ser mayor o igual a cero")]
        public decimal MontoInicial { get; set; }
    }
}
