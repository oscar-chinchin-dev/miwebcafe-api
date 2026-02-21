using System.ComponentModel.DataAnnotations;

namespace MiWebCafe.API.DTOs
{
    public class CategoriaDto
    {
        
            public int CategoriaId { get; set; }
            public string Nombre { get; set; } = null!;
            public bool Activo { get; set; }
        }
    }
