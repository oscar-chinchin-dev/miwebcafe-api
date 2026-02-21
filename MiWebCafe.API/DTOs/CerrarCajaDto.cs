using System.ComponentModel.DataAnnotations;

namespace MiWebCafe.API.DTOs
{
    public class CerrarCajaDto
    {
        [Range(0, double.MaxValue)]
        public decimal? MontoFinalDeclarado { get; set; }
    }
}
