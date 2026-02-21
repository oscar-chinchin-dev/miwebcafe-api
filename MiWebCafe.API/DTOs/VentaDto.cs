using System.ComponentModel.DataAnnotations;

namespace MiWebCafe.API.DTOs
{
    public class VentaDto
    {
        public int Id { get; set; }

        [Required]
        public DateTime? Fecha { get; set; }

        [Required]
        public decimal Total { get; set; }

        [Required]
        public int ClienteId { get; set; }
    }
}
