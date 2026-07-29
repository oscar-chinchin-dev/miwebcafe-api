using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MiWebCafe.API.Entities
{
    public class Producto
    {
        public int ProductoId { get; set; }

        public string Nombre { get; set; } = null!;
        public decimal Precio { get; set; }
        public bool Activo { get; set; } = true;
        public int Stock { get; set; }

        /// <summary>
        /// Token de concurrencia optimista.
        /// SQL Server gestiona este valor automáticamente (rowversion/timestamp).
        /// EF Core lo usa para detectar actualizaciones concurrentes sobre el mismo registro.
        /// </summary>
        [Timestamp]
        public byte[] RowVersion { get; set; } = null!;

        // FK
        public int CategoriaId { get; set; }
        public Categoria Categoria { get; set; } = null!;

        // Relaciones
        public ICollection<DetalleVenta> DetalleVentas { get; set; } = new List<DetalleVenta>();
    }
}