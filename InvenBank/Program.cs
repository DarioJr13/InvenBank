using FluentValidation;
using FluentValidation.AspNetCore;
using InvenBank.API.Configuration;
using InvenBank.API.Middleware;
using InvenBank.API.Repositories.Implementations;
using InvenBank.API.Repositories.Interfaces;
using InvenBank.API.Services;
using InvenBank.API.Services.Implementations;
using InvenBank.API.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Data;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ===============================================
// CONFIGURACI�N DE SERILOG
// ===============================================

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "InvenBank.API")
    .CreateLogger();

builder.Host.UseSerilog();

// ===============================================
// CONFIGURACI�N DE SERVICIOS
// ===============================================

var services = builder.Services;
var configuration = builder.Configuration;

// Configuraciones fuertemente tipadas
services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));
services.Configure<CorsSettings>(configuration.GetSection(CorsSettings.SectionName));
services.Configure<SwaggerSettings>(configuration.GetSection(SwaggerSettings.SectionName));
services.Configure<ApiSettings>(configuration.GetSection(ApiSettings.SectionName));

// Configuraci�n de base de datos
services.AddScoped<IDbConnection>(sp =>
{
    var connectionString = configuration.GetConnectionString("DefaultConnection");
    return new SqlConnection(connectionString);
});

// Servicios de autenticaci�n
services.AddScoped<IJwtService, JwtService>();
services.AddScoped<IPasswordHashService, PasswordHashService>();

// AutoMapper
services.AddAutoMapper(typeof(Program));

services.AddScoped<ICategoryRepository, CategoryRepository>();
services.AddScoped<ISupplierRepository, SupplierRepository>();

// Services
services.AddScoped<ICategoryService, CategoryService>();
services.AddScoped<ISupplierService, SupplierService>();

// Validators
services.AddValidatorsFromAssemblyContaining<Program>();

// FluentValidation
services.AddFluentValidationAutoValidation()
        .AddFluentValidationClientsideAdapters();

// Controllers
services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.WriteIndented = true;
    });

// ===============================================
// CONFIGURACI�N JWT
// ===============================================

var jwtSettings = configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>();

services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.RequireHttpsMetadata = false; // Solo para desarrollo
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings!.Issuer,
        ValidAudience = jwtSettings.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
        ClockSkew = TimeSpan.Zero
    };

    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            Log.Warning("Autenticaci�n JWT fallida: {Message}", context.Exception.Message);
            return Task.CompletedTask;
        },
        OnTokenValidated = context =>
        {
            Log.Information("Token JWT validado para usuario: {UserId}",
                context.Principal?.FindFirst("userId")?.Value);
            return Task.CompletedTask;
        }
    };
});

// ===============================================
// CONFIGURACI�N CORS
// ===============================================

var corsSettings = configuration.GetSection(CorsSettings.SectionName).Get<CorsSettings>();

services.AddCors(options =>
{
    options.AddPolicy("InvenBankCorsPolicy", policy =>
    {
        policy.WithOrigins(corsSettings!.AllowedOrigins)
              .WithMethods(corsSettings.AllowedMethods)
              .WithHeaders(corsSettings.AllowedHeaders);

        if (corsSettings.AllowCredentials)
        {
            policy.AllowCredentials();
        }
    });
});

// ===============================================
// CONFIGURACI�N SWAGGER
// ===============================================

var swaggerSettings = configuration.GetSection(SwaggerSettings.SectionName).Get<SwaggerSettings>();

services.AddEndpointsApiExplorer();
services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = swaggerSettings!.Title,
        Version = swaggerSettings.Version,
        Description = swaggerSettings.Description,
        Contact = new OpenApiContact
        {
            Name = swaggerSettings.ContactName,
            Email = swaggerSettings.ContactEmail
        }
    });

    // Configuraci�n JWT en Swagger
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = @"JWT Authorization header usando el esquema Bearer. 
                        Ingresa 'Bearer' [espacio] y luego tu token en el campo de abajo.
                        Ejemplo: 'Bearer 12345abcdef'",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header,
            },
            new List<string>()
        }
    });

    // Incluir comentarios XML
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
});

// ===============================================
// CONFIGURACI�N ADICIONAL
// ===============================================

// Compresi�n de respuestas
services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
});

// Health Checks
services.AddHealthChecks()
    .AddSqlServer(configuration.GetConnectionString("DefaultConnection")!);

// ===============================================
// CONSTRUCCI�N DE LA APLICACI�N
// ===============================================

var app = builder.Build();

// ===============================================
// CONFIGURACI�N DEL PIPELINE
// ===============================================

// Logging de requests (solo en desarrollo)
if (app.Environment.IsDevelopment())
{
    app.UseRequestLogging();
}

// Manejo global de errores
app.UseErrorHandling();

// Swagger (solo en desarrollo)
//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI(options =>
//    {
//        options.SwaggerEndpoint("/swagger/v1/swagger.json", $"{swaggerSettings!.Title} {swaggerSettings.Version}");
//        options.RoutePrefix = string.Empty; // Swagger en la ra�z
//        options.DocumentTitle = swaggerSettings.Title;
//    });
//}
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", $"{swaggerSettings!.Title} {swaggerSettings.Version}");
    options.RoutePrefix = "swagger"; // Swagger en /swagger
    options.DocumentTitle = swaggerSettings.Title;
    options.DefaultModelsExpandDepth(-1); // Ocultar modelos por defecto
});

// HTTPS Redirection
app.UseHttpsRedirection();

// Compresi�n
app.UseResponseCompression();

// CORS
app.UseCors("InvenBankCorsPolicy");

// Autenticaci�n y Autorizaci�n
app.UseAuthentication();
app.UseAuthorization();

// Health Checks
app.MapHealthChecks("/health");

// Controllers
app.MapControllers();

// ===============================================
// LOGGING DE INICIO
// ===============================================

Log.Information("Iniciando InvenBank API...");
Log.Information("Entorno: {Environment}", app.Environment.EnvironmentName);
Log.Information("JWT Issuer: {Issuer}", jwtSettings!.Issuer);
Log.Information("CORS Origins: {Origins}", string.Join(", ", corsSettings!.AllowedOrigins));

try
{
    Log.Information("InvenBank API iniciada exitosamente");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "La aplicaci�n fall� al iniciar");
}
finally
{
    Log.CloseAndFlush();
}