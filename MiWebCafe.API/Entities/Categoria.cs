using System.Collections.Generic;

namespace MiWebCafe.API.Entities
{
    public class Categoria
    {
        public int CategoriaId { get; set; }

        public string Nombre { get; set; } = null!;
        public string? Descripcion { get; set; }
        public bool Activo { get; set; }

        // Relaciones
        public ICollection<Producto> Productos { get; set; } = new List<Producto>();
    }
}
