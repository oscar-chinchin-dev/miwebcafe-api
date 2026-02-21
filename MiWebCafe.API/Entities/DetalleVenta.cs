
using System.Collections.Generic;

namespace MiWebCafe.API.Entities
{
    public class DetalleVenta
    {
        public int DetalleVentaId { get; set; }

        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Subtotal { get; set; }

        // FK
        public int VentaId { get; set; }
        public Venta Venta { get; set; } = null!;

        public int ProductoId { get; set; }
        public Producto Producto { get; set; } = null!;
    }
}