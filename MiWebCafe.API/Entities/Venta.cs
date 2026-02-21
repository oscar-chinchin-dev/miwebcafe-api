using System;
using System.Collections.Generic;

namespace MiWebCafe.API.Entities
{
    public class Venta
    {
        public int VentaId { get; set; }

        public DateTime Fecha { get; set; }
        public decimal Total { get; set; }
        public bool Anulada { get; set; } = false;
        public DateTime? FechaAnulacion { get; set; }
        public string Estado { get; set; } = "ABIERTA";

        // FK
        public int UsuarioId { get; set; }
        public Usuario Usuario { get; set; } = null!;
        public int? CierreCajaId { get; set; }
        public CierreCaja? CierreCaja { get; set; }

        // Relaciones
        public ICollection<DetalleVenta> Detalles { get; set; } = new List<DetalleVenta>();
    }
}