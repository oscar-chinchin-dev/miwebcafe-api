using System.ComponentModel.DataAnnotations;

namespace MiWebCafe.API.DTOs
{
    public class CerrarCajaDto
    {
        [Range(0, double.MaxValue, ErrorMessage = "El monto final declarado debe ser mayor o igual a cero")]
        public decimal? MontoFinalDeclarado { get; set; }
    }
}
