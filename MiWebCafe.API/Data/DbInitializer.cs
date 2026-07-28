using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MiWebCafe.API.Configuration;
using MiWebCafe.API.Entities;

namespace miwebcafe.API.Data;

public sealed class DbInitializer
{
    private readonly AppDbContext _context;
    private readonly InitialUsersOptions _initialUsers;
    private readonly PasswordHasher<Usuario> _passwordHasher = new();

    public DbInitializer(
        AppDbContext context,
        IOptions<InitialUsersOptions> initialUsers)
    {
        _context = context;
        _initialUsers = initialUsers.Value;
    }

    public async Task SeedAsync()
    {
        await CreateUserIfMissingAsync(_initialUsers.Admin, "Admin");
        await CreateUserIfMissingAsync(_initialUsers.Cajero, "Cajero");
    }

    private async Task CreateUserIfMissingAsync(InitialUserOptions userOptions, string role)
    {
        var userExists = await _context.Usuarios
            .AnyAsync(user => user.Email == userOptions.Email);

        if (userExists)
        {
            return;
        }

        var user = new Usuario
        {
            Nombre = userOptions.Nombre,
            Email = userOptions.Email,
            Rol = role,
            Activo = true
        };

        user.PasswordHash = _passwordHasher.HashPassword(user, userOptions.Password);

        _context.Usuarios.Add(user);
        await _context.SaveChangesAsync();
    }
}
