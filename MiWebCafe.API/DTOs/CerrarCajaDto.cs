using System.ComponentModel.DataAnnotations;

namespace MiWebCafe.API.DTOs
{
    public class CerrarCajaDto
    {
        [Required(ErrorMessage = "El monto final declarado es obligatorio")]
        [Range(0, double.MaxValue, ErrorMessage = "El monto final declarado debe ser mayor o igual a cero")]
        public decimal MontoFinalDeclarado { get; set; }
    }
}
