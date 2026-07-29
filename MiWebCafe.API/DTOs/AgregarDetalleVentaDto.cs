using System.ComponentModel.DataAnnotations;

namespace MiWebCafe.API.DTOs
{
    public class AgregarDetalleVentaDto
    {
        [Required(ErrorMessage = "El VentaId es obligatorio")]
        [Range(1, int.MaxValue, ErrorMessage = "El id de la venta debe ser mayor a 0")]
        public int VentaId { get; set; }

        [Required(ErrorMessage = "El ProductoId es obligatorio")]
        [Range(1, int.MaxValue, ErrorMessage = "El id del producto debe ser mayor a 0")]
        public int ProductoId { get; set; }

        [Required(ErrorMessage = "La cantidad es obligatoria")]
        [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0")]
        public int Cantidad { get; set; }
    }
}
