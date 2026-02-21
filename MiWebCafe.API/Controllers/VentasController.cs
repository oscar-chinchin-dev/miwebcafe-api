using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using miwebcafe.API.Data;
using MiWebCafe.API.DTOs;
using MiWebCafe.API.Entities;
using System.Security.Claims;

namespace MiWebCafe.API.Controllers
{
    /// <summary>
    /// Controlador principal para la gestión del ciclo de vida de las ventas.
    /// Maneja transacciones atómicas, control de stock y reportes financieros.
    /// </summary>
    
    [ApiController]
    [Route("api/ventas")]
    public class VentasController : ControllerBase
    {
        private readonly AppDbContext _context;

        public VentasController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Registra una venta completa procesando múltiples productos en una sola transacción.
        /// </summary>
        /// <remarks>
        /// Incluye validación de caja abierta, verificación de stock y actualización de inventario.
        /// Utiliza una transacción de base de datos para asegurar la integridad de los datos.
        /// </remarks>
        

        [Authorize(Roles = "Admin,Cajero")]
        [HttpPost]
        public async Task<IActionResult> RegistrarVenta([FromBody] CrearVentaDto dto)
        {
            if (dto.Detalles == null || !dto.Detalles.Any())
                return BadRequest("La venta debe tener al menos un producto");

            // Inicio de transacción para asegurar que si falla un detalle, no se guarde nada

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Obtención del cajero actual desde el Token

                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

                // Verificación de que el usuario tenga un turno de caja activo para poder vender

                var cajaAbierta = await _context.CierresCaja
                .Where(c => c.UsuarioId == userId && c.Estado == "ABIERTA")
                .OrderByDescending(c => c.FechaApertura)
                .FirstOrDefaultAsync();

                if (cajaAbierta == null)
                    return BadRequest("No hay caja ABIERTA. Debe abrir caja antes de vender.");

                var venta = new Venta
                {
                    Fecha = DateTime.UtcNow,
                    UsuarioId = userId,
                    Total = 0,
                    CierreCajaId = cajaAbierta.CierreCajaId,
                    Estado = "ABIERTA"
                };

                _context.Ventas.Add(venta);
                await _context.SaveChangesAsync();

                decimal total = 0;

                // Procesamiento individual de cada producto en el detalle de la venta

                foreach (var item in dto.Detalles)
                {
                    var producto = await _context.Productos.FindAsync(item.ProductoId);
                    if (producto == null)
                        return NotFound($"Producto {item.ProductoId} no existe");

                    // Regla de Negocio: No permitir ventas que superen el stock disponible

                    if (producto.Stock < item.Cantidad)
                        return BadRequest($"Stock insuficiente para {producto.Nombre}");

                    // Actualización de inventario en tiempo real

                    producto.Stock -= item.Cantidad;

                    var subtotal = producto.Precio * item.Cantidad;
                    total += subtotal;

                    _context.DetalleVentas.Add(new DetalleVenta
                    {
                        VentaId = venta.VentaId,
                        ProductoId = producto.ProductoId,
                        Cantidad = item.Cantidad,
                        PrecioUnitario = producto.Precio,
                        Subtotal = subtotal
                    });
                }

                venta.Total = total;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync(); // Confirmación de cambios en la base de datos

                return Ok(new { venta.VentaId, venta.Total });
            }
            catch
            {
                // En caso de error, se revierten todos los cambios (incluyendo el stock)
                await transaction.RollbackAsync();
                throw;
            }
        }

        /// <summary>
        /// Obtiene el historial general de ventas (Solo administradores).
        /// </summary>
        
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<ActionResult> GetVentas()
        {
            var ventas = await _context.Ventas
                .Include(v => v.Usuario)
                .Select(v => new
                {
                    v.VentaId,
                    v.Fecha,
                    v.Total,
                    Cajero = v.Usuario.Nombre
                })
                .OrderByDescending(v => v.Fecha)
                .ToListAsync();

            return Ok(ventas);
        }

        /// <summary>
        /// Recupera el detalle completo de una venta específica, incluyendo productos.
        /// </summary>
        
        [Authorize(Roles = "Admin,Cajero")]
        [HttpGet("{id}")]
        public async Task<ActionResult> GetVenta(int id)
        {
            var venta = await _context.Ventas
                .Include(v => v.Usuario)
                .Include(v => v.Detalles)
                    .ThenInclude(d => d.Producto)
                .FirstOrDefaultAsync(v => v.VentaId == id);

            if (venta == null)
                return NotFound();

            return Ok(new
            {
                ventaId = venta.VentaId,
                fecha = venta.Fecha,
                total = venta.Total,
                usuarioNombre = venta.Usuario.Nombre,
                detalles = venta.Detalles.Select(d => new
                {
                    productoNombre = d.Producto.Nombre,
                    cantidad = d.Cantidad,
                    precio = d.PrecioUnitario,
                    subtotal = d.Subtotal
                })
            });
        }

        /// <summary>
        /// Permite incrementar manualmente el stock de un producto (Abastecimiento).
        /// </summary>
        
        [Authorize(Roles = "Admin")]
        [HttpPut("stock/{id}")]
        public async Task<IActionResult> ReponerStock(int id, [FromBody] int cantidad)
        {
            if (cantidad <= 0)
                return BadRequest("Cantidad inválida");

            var producto = await _context.Productos.FindAsync(id);
            if (producto == null)
                return NotFound();

            producto.Stock += cantidad;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// Genera un reporte consolidado de las ventas del día actual.
        /// </summary>
        
        [Authorize(Roles = "Admin")]
        [HttpGet("reporte-diario")]
        public async Task<IActionResult> ReporteDiario()
        {
            var hoy = DateTime.UtcNow.Date;
            var mañana = hoy.AddDays(1);

            var ventas = await _context.Ventas
                .Include(v => v.Usuario)
                .Where(v => !v.Anulada && v.Fecha >= hoy && v.Fecha < mañana)
                .Select(v => new
                {
                    v.VentaId,
                    v.Fecha,
                    v.Total,
                    Cajero = v.Usuario.Nombre
                })
                .ToListAsync();

            var totalDia = ventas.Sum(v => v.Total);

            return Ok(new
            {
                Fecha = hoy.ToString("yyyy-MM-dd"),
                CantidadVentas = ventas.Count,
                TotalVendido = totalDia,
                Ventas = ventas
            });
        }

        /// <summary>
        /// Reporte de ventas filtrado por un rango de fechas personalizado.
        /// </summary>
        
        [Authorize(Roles = "Admin")]
        [HttpGet("reporte-rango")]
        public async Task<IActionResult> ReporteRango(
    [FromQuery] DateTime desde,
    [FromQuery] DateTime hasta)
        {
            if (desde > hasta)
                return BadRequest("La fecha 'desde' no puede ser mayor que 'hasta'");

            var hastaFin = hasta.Date.AddDays(1);

            var ventas = await _context.Ventas
                .Include(v => v.Usuario)
                .Where(v => !v.Anulada && v.Fecha >= desde.Date && v.Fecha < hastaFin)
                .Select(v => new
                {
                    v.VentaId,
                    v.Fecha,
                    v.Total,
                    Cajero = v.Usuario.Nombre
                })
                .OrderBy(v => v.Fecha)
                .ToListAsync();

            return Ok(new
            {
                Desde = desde.ToString("yyyy-MM-dd"),
                Hasta = hasta.ToString("yyyy-MM-dd"),
                CantidadVentas = ventas.Count,
                TotalVendido = ventas.Sum(v => v.Total),
                Ventas = ventas
            });
        }

        /// <summary>
        /// Marca una venta como anulada para fines contables.
        /// </summary>
        

        [Authorize(Roles = "Admin")]
        [HttpPut("anular/{id}")]
        public async Task<IActionResult> AnularVenta(int id)
        {
            var venta = await _context.Ventas.FindAsync(id);

            if (venta == null)
                return NotFound("Venta no existe");

            if (venta.Anulada)
                return BadRequest("La venta ya está anulada");

            venta.Anulada = true;
            venta.FechaAnulacion = DateTime.UtcNow;
            venta.Total = 0; // Se resetea el total para no afectar reportes financieros

            await _context.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// Permite añadir un producto adicional a una venta ya iniciada.
        /// </summary>
        
        [Authorize(Roles = "Admin,Cajero")]
        [HttpPost("detalle")]
        public async Task<IActionResult> AgregarDetalle([FromBody] AgregarDetalleVentaDto dto)
        {
            var venta = await _context.Ventas
                .Include(v => v.Detalles)
                .FirstOrDefaultAsync(v => v.VentaId == dto.VentaId);

            if (venta == null)
                return NotFound("Venta no existe");

            var producto = await _context.Productos.FindAsync(dto.ProductoId);

            if (producto == null)
                return NotFound("Producto no existe");

            if (producto.Stock < dto.Cantidad)
                return BadRequest("Stock insuficiente");

            // Lógica para acumular cantidad si el producto ya está en la lista de detalles

            var detalleExistente = venta.Detalles
                .FirstOrDefault(d => d.ProductoId == dto.ProductoId);

            if (detalleExistente != null)
            {
                detalleExistente.Cantidad += dto.Cantidad;
            }
            else
            {
                venta.Detalles.Add(new DetalleVenta
                {
                    ProductoId = dto.ProductoId,
                    Cantidad = dto.Cantidad,
                    PrecioUnitario = producto.Precio
                });
            }

            await _context.SaveChangesAsync();
            return Ok();
        }

        /// <summary>
        /// Finaliza el proceso de venta cambiando su estado a CONFIRMADA.
        /// </summary>
        // [Authorize(Roles = "Admin,Cajero")]

        [HttpPost("{id}/confirmar")]
        public async Task<IActionResult> ConfirmarVenta(int id)
        {
            Console.WriteLine(">>> ENTRÓ A ConfirmarVenta");

            var venta = await _context.Ventas.FindAsync(id);
            if (venta == null)
                return NotFound("Venta no encontrada");

            venta.Estado = "CONFIRMADA";
            venta.Fecha = DateTime.Now;

            await _context.SaveChangesAsync();
            return Ok(new
            {
                venta.VentaId,
                venta.Fecha,
                venta.Total,
                venta.Estado,
                venta.Anulada
            });
        }
    }
}
