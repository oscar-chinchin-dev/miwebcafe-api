using System.ComponentModel.DataAnnotations;

namespace MiWebCafe.API.DTOs
{
    public class VentaDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "La fecha es obligatoria")]
        public DateTime? Fecha { get; set; }

        [Required(ErrorMessage = "El total es obligatorio")]
        [Range(0, double.MaxValue, ErrorMessage = "El total debe ser mayor o igual a cero")]
        public decimal Total { get; set; }

        [Required(ErrorMessage = "El ClienteId es obligatorio")]
        [Range(1, int.MaxValue, ErrorMessage = "El id del cliente debe ser mayor a 0")]
        public int ClienteId { get; set; }
    }
}
