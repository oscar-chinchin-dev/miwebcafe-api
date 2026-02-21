using Microsoft.EntityFrameworkCore;
using MiWebCafe.API.Entities;

namespace miwebcafe.API.Data
{
    /// <summary>
    /// Clase de contexto de la base de datos. 
    /// Actúa como el puente principal entre las entidades de C# y la base de datos SQL.
    /// </summary>
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        // --- DbSets: Representan las tablas de la base de datos ---

        /// <summary> Tabla de usuarios y credenciales del sistema. </summary>
        public DbSet<Usuario> Usuarios { get; set; }

        /// <summary> Tabla de categorías para la organización de productos. </summary>
        public DbSet<Categoria> Categorias { get; set; }

        /// <summary> Tabla de productos (mapeo de catálogo e inventario). </summary>
        public DbSet<Producto> Productos { get; set; }

        /// <summary> Encabezado de las transacciones de venta. </summary>
        public DbSet<Venta> Ventas { get; set; }

        /// <summary> Detalle línea por línea de cada producto vendido. </summary>
        public DbSet<DetalleVenta> DetalleVentas { get; set; }

        /// <summary> Registros de apertura y cierre de turnos de caja. </summary>
        public DbSet<CierreCaja> CierresCaja { get; set; }

        /// <summary>
        /// Configuración avanzada del modelo mediante Fluent API.
        /// </summary>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // --- Configuración de Relaciones ---

            modelBuilder.Entity<Venta>()
                .HasOne(v => v.CierreCaja) // Una venta pertenece a un cierre de caja específico.
                .WithMany()                // Un cierre de caja puede agrupar múltiples ventas.
                .HasForeignKey(v => v.CierreCajaId)
                // REGLA DE INTEGRIDAD: Se usa NoAction para evitar rutas de eliminación en cascada circulares.
                // Esto asegura que no se borren ventas accidentalmente al manipular registros de caja.
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
