using System.ComponentModel.DataAnnotations;

namespace MiWebCafe.API.DTOs
{
    public class CategoriaCreateDto
    {
        [Required(ErrorMessage = "El nombre es obligatorio")]
        [MaxLength(50, ErrorMessage = "Máximo 50 caracteres")]
        public string Nombre { get; set; } = null!;

        public bool Activo { get; set; } = true;
    }
}
