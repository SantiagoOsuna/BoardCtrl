using boardCtrl.DATA; // Importa el contexto de datos
using boardCtrl.Models; // Importa los modelos de datos
using boardCtrl.Services; // Importa los servicios
using Microsoft.AspNetCore.Authentication.JwtBearer; // Importa la autenticacion JWT
using Microsoft.EntityFrameworkCore; // Importa Entity Framework Core
using Microsoft.IdentityModel.Tokens; // Importa los tokens de autenticacion
using Microsoft.OpenApi.Models; // Importa configuraciones de Swagger
using System.Text; // Importa las utilidades de codificacion

namespace boardCtrl
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Configura la aplicacion web
            var builder = WebApplication.CreateBuilder(args);

            // Añade servicios para controladores de API y genera documentacion swagger
            builder.Services.AddControllers(); // Habilita el soporte para controladores de API
            builder.Services.AddEndpointsApiExplorer(); // Explore y expone los endpoints de la API
            builder.Services.AddSwaggerGen(c =>
            {
                // Configura Swagger para documentar la API
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "BoardsCTRL", Version = "v1" });

                // Define el esquema de seguridad para autenticar usando tokens JWT
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header, // Ubica el token en el encabezado de la solicitud
                    Description = "Enter your token in the format 'Bearer {token}'",
                    Name = "Authorization", // Define el encabezado donde se envia el token
                    Type = SecuritySchemeType.ApiKey, // Define el tipo de autenticacion
                    Scheme = "Bearer" // Especifica el esquema de seguridad utilizado
                });

                // Configura el requisito de seguridad para usar tokens JWT en Swagger
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new string[] { }
                    }
                });
            });

            // Configura el contexto de base de datos usando SQL Server y define un timeout de 100 segundos
            builder.Services.AddDbContext<boardCtrlContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"), sqlOptions =>
                {
                    sqlOptions.CommandTimeout(100); // Establece el tiempo maximo de espera para las consultas SQL
                }));

            // Lee y configura los ajustes de JWT desde el archivo de configuracion
            var jwtSection = builder.Configuration.GetSection("JWT");
            builder.Services.Configure<JWTTOKENResponse>(jwtSection);

            // Obtiene las configuraciones JWT
            var jwtSettings = jwtSection.Get<JWTTOKENResponse>();

            // Verifica si las configuraciones de JWT estan correctas
            if (jwtSettings == null)
            {
                throw new InvalidOperationException("JWT settings are not configured correctly.");
            }

            if (string.IsNullOrEmpty(jwtSettings.Secret))
            {
                throw new InvalidOperationException("JWT secret is not configured.");
            }
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowReactApp",
                    policy =>
                    {
                        policy.WithOrigins("http://localhost:3000") // Direccion de tu app de React
                            .AllowAnyMethod() // Permite todos los metodos HTTP (GET, POST, PUT, etc.)
                            .AllowAnyHeader() // Permite todo los encabezados
                            .AllowCredentials(); // Permite el uso de credenciales.
                    });
            });
            // Configura la autenticacion usando JWT
            builder.Services.AddAuthentication(options =>
            {
                // Define el esquema de autenticacion predeterminado
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                // Configura la validacion de los tokens JWT
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    // Valida los parametros del token JWT
                    ValidateIssuer = true, // Valida el emisor del token
                    ValidateAudience = true, // Valida la audiencia del token
                    ValidateLifetime = true, // Valida que el token no haya expirado
                    ValidateIssuerSigningKey = true, // Valida la clave de firma del token
                    ValidIssuer = jwtSettings.ValidIssuer, // Emisor valido del token
                    ValidAudience = jwtSettings.ValidAudience, // Audiencia valida del token
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret)) // Llave de firma simetrica
                };

                // Configura como se recibe el token JWT
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var token = context.Request.Headers["Authorization"].FirstOrDefault();
                        if (!string.IsNullOrEmpty(token))
                        {
                            // Elimina el prefijo 'Bearer ' del token si esta presente
                            if (token.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                            {
                                context.Token = token.Substring("Bearer ".Length).Trim();
                            }
                            else
                            {
                                context.Token = token;
                            }
                        }
                        return Task.CompletedTask;
                    }
                };
            });

            // Configura la autorizacion
            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("RequireAdminRole", policy => policy.RequireRole("Admin"));
                options.AddPolicy("RequireUserRole", policy => policy.RequireRole("User"));
            });

            // Registra los servicios de usuario y rol
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<IRoleService, RoleService>();

            var app = builder.Build();

            // Configura la aplicación segun el entorno
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger(); // Habilita Swagger en desarrollo
                app.UseSwaggerUI(); // Habilita la interfaz de usuario de Swagger
            }

            app.UseCors("AllowReactApp");
            app.UseHttpsRedirection(); // Redirige a HTTPS
            app.UseAuthentication(); // Añade la autenticacion
            app.UseAuthorization(); // Añade la autorizacion
            app.MapControllers(); // Mapea los controladores
            app.Run(); // Ejecuta la aplicacion
        }
    }
}