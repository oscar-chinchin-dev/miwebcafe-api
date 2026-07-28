using Microsoft.EntityFrameworkCore;
using miwebcafe.API.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using MiWebCafe.API.Configuration;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendDev", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// --- Configuración de Servicios (Inyección de Dependencias) ---

// Configuración de controladores con política de nombres CamelCase para compatibilidad con Frontend (Angular/React)
builder.Services.AddControllers().AddJsonOptions(opt =>
{
    opt.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
});

// Configuración de la base de datos principal mediante SQL Server
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);

builder.Services
    .AddOptions<JwtOptions>()
    .BindConfiguration(JwtOptions.SectionName)
    .Validate(options => options.IsValid(), "La configuración JWT es inválida.")
    .ValidateOnStart();

builder.Services
    .AddOptions<InitialUsersOptions>()
    .BindConfiguration(InitialUsersOptions.SectionName)
    .Validate(options => options.IsValid(), "La configuración de usuarios iniciales es inválida.")
    .ValidateOnStart();

builder.Services.AddScoped<DbInitializer>();

// Soporte para OpenAPI (Documentación de la API)
builder.Services.AddOpenApi();

// --- Configuración de Seguridad JWT ---
var jwtSettings = builder.Configuration
    .GetRequiredSection(JwtOptions.SectionName)
    .Get<JwtOptions>()!;
var key = Encoding.UTF8.GetBytes(jwtSettings.Key);

builder.Services.AddAuthentication(options =>
{
    // Establece JWT como el esquema de autenticación por defecto
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    // Definición de reglas para validar el token recibido
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true, // Verifica que el token no haya expirado
        ValidateIssuerSigningKey = true, // Valida la firma del servidor
        ValidIssuer = jwtSettings.Issuer,
        ValidAudience = jwtSettings.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(key)
    };
});

// Configuración de CORS: Permite que el frontend (puerto 4200) se comunique con esta API


var app = builder.Build();

// --- Configuración del Pipeline de solicitudes (Middleware) ---



if (app.Environment.IsDevelopment())
{
    // Habilita la interfaz de OpenAPI solo en entorno de desarrollo
    app.MapOpenApi();
}

// app.UseHttpsRedirection(); // Comentado para desarrollo local si no se usa SSL

// El orden aquí es CRÍTICO: CORS -> Autenticación -> Autorización
app.UseCors("FrontendDev");

app.UseAuthentication(); // ¿Quién es el usuario?
app.UseAuthorization();  // ¿A qué tiene permiso?

app.MapControllers();

// --- Inicialización de Datos (Seeding) ---
// Ejecuta la creación de usuarios base al iniciar la aplicación si no existen
using (var scope = app.Services.CreateScope())
{
    var initializer = scope.ServiceProvider.GetRequiredService<DbInitializer>();
    await initializer.SeedAsync();
}

app.Run();
