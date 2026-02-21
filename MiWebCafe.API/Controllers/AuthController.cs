using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using miwebcafe.API.Data;
using MiWebCafe.API.DTOs.Auth;
using MiWebCafe.API.Entities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MiWebCafe.API.Controllers
{
    /// <summary>
    /// Controlador encargado de gestionar los procesos de autenticación y seguridad.
    /// Provee endpoints para el inicio de sesión y generación de tokens JWT.
    /// </summary>
    
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;

        public AuthController(AppDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        /// <summary>
        /// Valida las credenciales del usuario y retorna un token de acceso.
        /// </summary>
        /// <param name="dto">Objeto de transferencia de datos con Email y Password.</param>
        /// <returns>Resultado de la acción con el token y datos básicos del usuario.</returns>
        


        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            // Buscamos el usuario por email asegurándonos de que esté activo en el sistema.

            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Email == dto.Email && u.Activo);

            // Importante: No especificamos si el correo existe o no por razones de seguridad (evita enumeración de usuarios).

            if (usuario == null)
                return Unauthorized("Credenciales incorrectas");

            // Utilizamos PasswordHasher de Identity para verificar el hash almacenado contra el texto plano recibido.

            var hasher = new PasswordHasher<Usuario>();
            var result = hasher.VerifyHashedPassword(usuario, usuario.PasswordHash, dto.Password);

            if (result == PasswordVerificationResult.Failed)
                return Unauthorized("Credenciales incorrectas");

            // Una vez autenticado, generamos el JWT para manejar la sesión stateless.

            var token = GenerarToken(usuario);

            return Ok(new
            {
                token,
                usuario.Nombre,
                usuario.Rol
            });
        }

        /// <summary>
        /// Genera un token JWT (JSON Web Token) firmado con las claims del usuario.
        /// </summary>
        /// <param name="usuario">Entidad del usuario autenticado.</param>
        /// <returns>String que representa el token JWT.</returns>
        
        private string GenerarToken(Usuario usuario)
        {
            // Definimos los Claims: información de identidad que viajará en el payload del token.

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.UsuarioId.ToString()),
                new Claim(ClaimTypes.Email, usuario.Email),
                new Claim(ClaimTypes.Role, usuario.Rol)
            };

            // Obtenemos la llave secreta desde la configuración (User-Secrets o Variables de entorno en producción).

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_config["Jwt:Key"]!)
            );

            // Definimos el algoritmo de firma (HMAC SHA256 es el estándar para JWT).

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Construcción del objeto del token con tiempos de expiración y emisores definidos.

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(
                    int.Parse(_config["Jwt:ExpireMinutes"]!)
                ),
                signingCredentials: creds
            );

            // Serializamos el token a string.

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
