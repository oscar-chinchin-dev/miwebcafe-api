using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using miwebcafe.API.Data;
using MiWebCafe.API.DTOs;
using MiWebCafe.API.Entities;

/// <summary>
/// Controlador encargado de gestionar las categorías de los productos en el sistema.
/// Permite organizar el catálogo y es fundamental para la integridad referencial de los productos.
/// </summary>
[ApiController]
[Route("api/categorias")]
public class CategoriasController : ControllerBase
{
    private readonly AppDbContext _context;

    public CategoriasController(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Recupera el listado de todas las categorías registradas.
    /// </summary>
    /// <remarks>
    /// Accesible para Administradores y Cajeros. 
    /// Realiza una proyección directa a CategoriaDto para optimizar la transferencia de datos.
    /// </remarks>
    [Authorize(Roles = "Admin,Cajero")]
    [HttpGet]
    public async Task<ActionResult<List<CategoriaDto>>> Get()
    {
        var categorias = await _context.Categorias
            .Select(c => new CategoriaDto
            {
                CategoriaId = c.CategoriaId,
                Nombre = c.Nombre,
                Activo = c.Activo
            })
            .ToListAsync();

        return Ok(categorias);
    }

    /// <summary>
    /// Crea una nueva categoría de producto.
    /// </summary>
    /// <param name="dto">Objeto con la información básica de la categoría (Nombre).</param>
    /// <returns>La categoría recién creada y su ubicación en la API.</returns>
    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult> Post(CategoriaCreateDto dto)
    {
        // Validación manual del estado del modelo para asegurar datos íntegros
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var categoria = new Categoria
        {
            Nombre = dto.Nombre
        };

        _context.Categorias.Add(categoria);
        await _context.SaveChangesAsync();

        // Retorno siguiendo el estándar REST 201 Created
        return CreatedAtAction(nameof(Get), new { id = categoria.CategoriaId }, categoria);
    }

    /// <summary>
    /// Actualiza los datos de una categoría existente.
    /// </summary>
    /// <param name="id">Identificador único de la categoría a modificar.</param>
    /// <param name="dto">Nuevos valores para el nombre y el estado de activación.</param>
    [Authorize(Roles = "Admin")]
    [HttpPut("{id}")]
    public async Task<ActionResult> Put(int id, CategoriaCreateDto dto)
    {
        // Búsqueda del registro original en la base de datos
        var categoria = await _context.Categorias.FindAsync(id);
        if (categoria == null) return NotFound();

        // Mapeo manual de campos actualizables
        categoria.Nombre = dto.Nombre;
        categoria.Activo = dto.Activo;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>
    /// Elimina físicamente una categoría de la base de datos.
    /// </summary>
    /// <param name="id">ID de la categoría a remover.</param>
    /// <remarks>
    /// Solo permitido para el rol Admin. Requiere precaución por posibles dependencias con productos.
    /// </remarks>
    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        var categoria = await _context.Categorias.FindAsync(id);
        if (categoria == null) return NotFound();

        _context.Categorias.Remove(categoria);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
