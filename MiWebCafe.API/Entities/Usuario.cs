using System.Collections.Generic;

namespace MiWebCafe.API.Entities
{
    public class Usuario
    {
        public int UsuarioId { get; set; }

        public string Nombre { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
        public string Rol { get; set; } = null!; // Admin / Usuario
        public bool Activo { get; set; } = true;

        // Relaciones
        public ICollection<Venta> Ventas { get; set; } = new List<Venta>();
    }
}
