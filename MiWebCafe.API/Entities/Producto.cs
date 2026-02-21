using System.Collections.Generic;

namespace MiWebCafe.API.Entities
{
    public class Producto
    {
        public int ProductoId { get; set; }

        public string Nombre { get; set; } = null!;
        public decimal Precio { get; set; }
        public bool Activo { get; set; } = true;
        public int Stock { get; set; }

        // FK
        public int CategoriaId { get; set; }
        public Categoria Categoria { get; set; } = null!;

        // Relaciones
        public ICollection<DetalleVenta> DetalleVentas { get; set; } = new List<DetalleVenta>();
    }
}