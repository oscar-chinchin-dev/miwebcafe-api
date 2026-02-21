using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using miwebcafe.API.Data;
using MiWebCafe.API.DTOs;
using MiWebCafe.API.Entities;
using System.Security.Claims;

namespace MiWebCafe.API.Controllers
{
    [ApiController]
    [Route("api/caja")]
    public class CajaController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CajaController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Permite a un administrador o cajero abrir un nuevo turno de caja.
        /// </summary>
        [Authorize(Roles = "Admin,Cajero")]
        [HttpPost("abrir")]
        public async Task<IActionResult> AbrirCaja([FromBody] AbrirCajaDto dto)
        {
            // Obtención del ID del usuario autenticado a partir de los Claims del Token JWT
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            // Validación: Verifica que el usuario no tenga ya una caja en estado "ABIERTA"
            var yaAbierta = await _context.CierresCaja
                .AnyAsync(c => c.UsuarioId == userId && c.Estado == "ABIERTA");

            if (yaAbierta)
                return BadRequest("Ya existe una caja ABIERTA para este usuario.");

            // Instancia de la entidad CierreCaja con los valores iniciales
            var caja = new CierreCaja
            {
                UsuarioId = userId,
                FechaApertura = DateTime.UtcNow,
                MontoInicial = dto.MontoInicial,
                Estado = "ABIERTA",
                TotalVentas = 0,
                CantidadVentas = 0
            };

            _context.CierresCaja.Add(caja);
            await _context.SaveChangesAsync();

            return Ok(new { caja.CierreCajaId, caja.FechaApertura, caja.MontoInicial, caja.Estado });
        }

        /// <summary>
        /// Recupera la información de la caja que el usuario actual tiene abierta.
        /// </summary>
        [Authorize(Roles = "Admin,Cajero")]
        [HttpGet("actual")]
        public async Task<IActionResult> CajaActual()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            // Busca el último registro de caja abierta para el usuario en sesión
            var caja = await _context.CierresCaja
                .Where(c => c.UsuarioId == userId && c.Estado == "ABIERTA")
                .OrderByDescending(c => c.FechaApertura)
                .FirstOrDefaultAsync();

            if (caja == null)
                return Ok(null);

            return Ok(new
            {
                caja.CierreCajaId,
                caja.FechaApertura,
                caja.MontoInicial,
                caja.Estado
            });
        }

        /// <summary>
        /// Proceso de cierre de caja donde se calcula el arqueo (esperado vs declarado).
        /// </summary>
        [Authorize(Roles = "Admin,Cajero")]
        [HttpPost("{cierreCajaId}/cerrar")]
        public async Task<IActionResult> CerrarCaja(int cierreCajaId, [FromBody] CerrarCajaDto dto)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            // Localiza la caja específica asegurando que pertenezca al usuario que intenta cerrarla
            var caja = await _context.CierresCaja
                .FirstOrDefaultAsync(c => c.CierreCajaId == cierreCajaId && c.UsuarioId == userId);

            if (caja == null)
                return NotFound("Caja no encontrada.");

            if (caja.Estado != "ABIERTA")
                return BadRequest("La caja ya está cerrada.");

            // Recopila todas las ventas vinculadas a este cierre de caja que no hayan sido anuladas
            var ventas = await _context.Ventas
                .Where(v => v.CierreCajaId == cierreCajaId && !v.Anulada)
                .ToListAsync();

            // Cálculo de arqueo de caja
            var esperado = caja.MontoInicial + caja.TotalVentas;
            var declarado = caja.MontoFinalDeclarado ?? 0;
            var diferencia = declarado - esperado;

            // Actualización de los totales y estado final de la caja
            caja.CantidadVentas = ventas.Count;
            caja.TotalVentas = ventas.Sum(v => v.Total);
            caja.MontoFinalDeclarado = dto.MontoFinalDeclarado;
            caja.FechaCierre = DateTime.UtcNow;
            caja.Estado = "CERRADA";

            await _context.SaveChangesAsync();

            return Ok(new
            {
                caja.CierreCajaId,
                caja.Estado,
                caja.FechaApertura,
                caja.FechaCierre,
                caja.MontoInicial,
                caja.MontoFinalDeclarado,
                caja.CantidadVentas,
                caja.TotalVentas,
                Esperado = esperado,
                Diferencia = diferencia
            });
        }

        /// <summary>
        /// Genera un resumen detallado de una caja cerrada, incluyendo datos del cajero y diferencias.
        /// </summary>
        [Authorize(Roles = "Admin,Cajero")]
        [HttpGet("{cierreCajaId}/resumen")]
        public async Task<IActionResult> Resumen(int cierreCajaId)
        {
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);

            var isAdmin = User.IsInRole("Admin");

            // Los administradores pueden ver cualquier resumen, los cajeros solo el propio
            var caja = await _context.CierresCaja.Include(c => c.Usuario)
            .FirstOrDefaultAsync(c => c.CierreCajaId == cierreCajaId && (isAdmin || c.UsuarioId == userId));

            if (caja == null) return NotFound("Cierre no encontrado");

            // Obtiene ventas confirmadas para el resumen financiero
            var ventas = await _context.Ventas
                .Where(v => v.CierreCajaId == cierreCajaId && !v.Anulada)
                .ToListAsync();

            var totalVentas = ventas.Sum(v => v.Total);
            var cantidadVentas = ventas.Count;

            var esperado = caja.MontoInicial + totalVentas;
            var declarado = caja.MontoFinalDeclarado ?? 0;
            var diferencia = declarado - esperado;

            return Ok(new
            {
                caja.CierreCajaId,
                Cajero = caja.Usuario.Nombre,
                caja.FechaApertura,
                caja.FechaCierre,
                caja.Estado,
                caja.MontoInicial,
                TotalVentas = totalVentas,
                CantidadVentas = cantidadVentas,
                caja.MontoFinalDeclarado,
                Esperado = esperado,
                Diferencia = diferencia
            });
        }

        /// <summary>
        /// Lista todos los cierres de caja registrados (Solo Administradores).
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpGet("cierres")]
        public async Task<IActionResult> GetCierres()
        {
            // Proyección de datos para el listado administrativo con información del usuario
            var cierres = await _context.CierresCaja
                .Include(c => c.Usuario)
                .OrderByDescending(c => c.FechaApertura)
                .Select(c => new
                {
                    c.CierreCajaId,
                    Cajero = c.Usuario.Nombre,
                    c.FechaApertura,
                    c.FechaCierre,
                    c.Estado,
                    c.MontoInicial,
                    c.TotalVentas,
                    c.CantidadVentas,
                    c.MontoFinalDeclarado
                })
                .ToListAsync();

            return Ok(cierres);
        }
    }
}