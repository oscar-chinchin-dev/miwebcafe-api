using Microsoft.AspNetCore.Identity;
using MiWebCafe.API.Entities;

namespace miwebcafe.API.Data
{
    /// <summary>
    /// Clase encargada de la alimentación inicial (Seeding) de la base de datos.
    /// Garantiza que el sistema cuente con usuarios operativos desde el primer despliegue.
    /// </summary>
    public static class DbInitializer
    {
        /// <summary>
        /// Crea el usuario con privilegios de Administrador si la tabla de usuarios está vacía.
        /// </summary>
        /// <param name="context">Contexto de la base de datos para realizar la persistencia.</param>
        public static void SeedAdmin(AppDbContext context)
        {
            // Verificación de existencia: Evita duplicar datos en reinicios del servidor.
            if (context.Usuarios.Any())
                return;

            // Uso de PasswordHasher de ASP.NET Core Identity para el manejo seguro de credenciales (Hashing).
            var passwordHasher = new PasswordHasher<Usuario>();

            var admin = new Usuario
            {
                Nombre = "Administrador",
                Email = "admin@miwebcafe.cl",
                Rol = "Admin",
                Activo = true
            };

            // Se almacena el hash, nunca la contraseña en texto plano.
            admin.PasswordHash = passwordHasher.HashPassword(admin, "Admin123*");

            context.Usuarios.Add(admin);
            context.SaveChanges();
        }

        /// <summary>
        /// Crea un usuario con rol de Cajero si el correo específico no está registrado.
        /// </summary>
        public static void SeedCajero(AppDbContext context)
        {
            // Validación específica por Email para asegurar la presencia del rol de Cajero.
            if (context.Usuarios.Any(u => u.Email == "cajero@miwebcafe.cl"))
                return;

            var hasher = new PasswordHasher<Usuario>();

            var cajero = new Usuario
            {
                Nombre = "Cajero",
                Email = "cajero@miwebcafe.cl",
                Rol = "Cajero",
                Activo = true
            };

            cajero.PasswordHash = hasher.HashPassword(cajero, "Cajero123*");

            context.Usuarios.Add(cajero);
            context.SaveChanges();
        }
    }
}
