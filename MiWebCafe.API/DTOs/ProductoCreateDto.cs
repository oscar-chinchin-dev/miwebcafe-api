using System.ComponentModel.DataAnnotations;

namespace MiWebCafe.API.DTOs
{
    public class ProductoCreateDto
    {
        [Required(ErrorMessage = "El nombre es obligatorio")]
        public string Nombre { get; set; } = null!;

        [Range(1, int.MaxValue, ErrorMessage = "El precio debe ser mayor a 0")]
        public decimal Precio { get; set; }

        [Required(ErrorMessage = "Debe seleccionar una categoría")]
        public int CategoriaId { get; set; }

        public bool Activo { get; set; } = true;
    }
}
