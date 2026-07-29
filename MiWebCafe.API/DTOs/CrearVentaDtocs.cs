using System.ComponentModel.DataAnnotations;

namespace MiWebCafe.API.DTOs
{
    public class CrearVentaDto
    {
        [Required(ErrorMessage = "La lista de detalles es obligatoria")]
        [MinLength(1, ErrorMessage = "La venta debe tener al menos un producto")]
        public List<DetalleVentaDto> Detalles { get; set; } = new();
    }
}
