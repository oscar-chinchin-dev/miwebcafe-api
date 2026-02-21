using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using miwebcafe.API.Data;
using MiWebCafe.API.Entities;
using MiWebCafe.API.DTOs;

/// <summary>
/// Controlador para la gestión del catálogo de productos.
/// Permite administrar precios, stock y la relación con categorías.
/// </summary>
[ApiController]
[Route("api/productos")]
public class ProductosController : ControllerBase
{
    private readonly AppDbContext _context;

    public ProductosController(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Obtiene la lista completa de productos.
    /// </summary>
    /// <remarks>
    /// Utiliza .Include() para realizar un Join con la tabla de Categorías
    /// y proyecta los resultados a ProductoDto para el consumo del frontend.
    /// </remarks>
    [Authorize(Roles = "Admin,Cajero")]
    [HttpGet]
    public async Task<ActionResult<List<ProductoDto>>> Get()
    {
        var productos = await _context.Productos
            .Include(p => p.Categoria)
            .Select(p => new ProductoDto
            {
                ProductoId = p.ProductoId,
                Nombre = p.Nombre,
                Precio = p.Precio,
                Activo = p.Activo,
                CategoriaId = p.CategoriaId,
                CategoriaNombre = p.Categoria.Nombre,
                Stock = p.Stock
            })
            .ToListAsync();

        return Ok(productos);
    }

    /// <summary>
    /// Obtiene un producto específico mediante su ID.
    /// </summary>
    /// <param name="id">ID numérico del producto.</param>
    [Authorize(Roles = "Admin,Cajero")]
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProductoDto>> GetById(int id)
    {
        var producto = await _context.Productos
            .Include(p => p.Categoria)
            .Where(p => p.ProductoId == id)
            .Select(p => new ProductoDto
            {
                ProductoId = p.ProductoId,
                Nombre = p.Nombre,
                Precio = p.Precio,
                Activo = p.Activo,
                CategoriaId = p.CategoriaId,
                CategoriaNombre = p.Categoria.Nombre,
                Stock = p.Stock
            })
            .FirstOrDefaultAsync();

        if (producto == null) return NotFound();

        return Ok(producto);
    }

    /// <summary>
    /// Registra un nuevo producto en el sistema.
    /// </summary>
    /// <remarks>
    /// Realiza validaciones de negocio: el precio debe ser positivo y 
    /// la categoría asignada debe existir en la base de datos.
    /// </remarks>
    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult> Post(ProductoCreateDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        // Validación de precio: impide registros con valores inconsistentes
        if (dto.Precio <= 0)
            return BadRequest("El precio debe ser mayor a 0");

        // Validación de Integridad: asegura que el ProductoId apunte a una categoría real
        var categoriaExiste = await _context.Categorias
            .AnyAsync(c => c.CategoriaId == dto.CategoriaId);

        if (!categoriaExiste)
            return BadRequest("La categoría no existe");

        var producto = new Producto
        {
            Nombre = dto.Nombre,
            Precio = dto.Precio,
            CategoriaId = dto.CategoriaId,
            Activo = dto.Activo
        };

        _context.Productos.Add(producto);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = producto.ProductoId }, null);
    }

    /// <summary>
    /// Actualiza la información de un producto existente.
    /// </summary>
    /// <param name="id">ID del producto a editar.</param>
    /// <param name="dto">Datos actualizados del producto.</param>
    [Authorize(Roles = "Admin")]
    [HttpPut("{id}")]
    public async Task<ActionResult> Put(int id, ProductoCreateDto dto)
    {
        var producto = await _context.Productos.FindAsync(id);
        if (producto == null) return NotFound();

        if (dto.Precio <= 0)
            return BadRequest("El precio debe ser mayor a 0");

        var categoriaExiste = await _context.Categorias
            .AnyAsync(c => c.CategoriaId == dto.CategoriaId);

        if (!categoriaExiste)
            return BadRequest("La categoría no existe");

        // Actualización de propiedades del objeto rastreado por EF Core
        producto.Nombre = dto.Nombre;
        producto.Precio = dto.Precio;
        producto.CategoriaId = dto.CategoriaId;
        producto.Activo = dto.Activo;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>
    /// Elimina un producto de forma permanente.
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        var producto = await _context.Productos.FindAsync(id);
        if (producto == null) return NotFound();

        _context.Productos.Remove(producto);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}

