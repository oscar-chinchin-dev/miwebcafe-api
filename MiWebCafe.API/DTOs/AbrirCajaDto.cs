using System.ComponentModel.DataAnnotations;

namespace MiWebCafe.API.DTOs
{
    public class AbrirCajaDto
    {
        [Range(0, double.MaxValue)]
        public decimal MontoInicial { get; set; }
    }
}
