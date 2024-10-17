using boardCtrl.DTO; // Importa los DTOs utilizados para transferir datos
using boardCtrl.Models; // Importa los modelos que representan las entidades de la base de datos
using boardCtrl.Services; // Importa los servicios que gestionan la logica de negocios
using Microsoft.AspNetCore.Mvc; // Importa las funcionalidades para manejar controladores y acciones
using Microsoft.IdentityModel.Tokens; // Importa las funcionalidades para crear y validar tokens JWT
using System.IdentityModel.Tokens.Jwt; // Importa las funcionalidades para trabajar con JWT
using System.Security.Claims; // Importa las funcionalidades para manejar claims en JWT
using System.Text; // Importa las funcionalidades para manejar codificacion de texto

namespace boardCtrl.Controllers
{
    // Defina la ruta base para las solicitudes HTTP y marca esta clase como un controlador de API
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly IConfiguration _configuration; // Configuracion de la aplicacion
        private readonly IUserService _userService; // Servicio para gestionar usuarios
        private readonly IRoleService _roleService; // Servicio para gestionar roles

        // Constructor que recibe la configuracion y los servicios necesarios para el controlador
        public AuthenticationController(IConfiguration configuration, IUserService userService, IRoleService roleService)
        {
            _configuration = configuration;
            _userService = userService;
            _roleService = roleService;
        }

        // Endpoint para cambiar la contraseña de un usuario (requiere autenticacion)
        [HttpPost("ApplyPassword")]
        public async Task<IActionResult> ApplyPassword([FromBody] ChangePasswordDto dto)
        {
            // Valida que los datos enviados son correctos
            if (dto == null || string.IsNullOrEmpty(dto.Username) || string.IsNullOrEmpty(dto.NewPassword))
            {
                return BadRequest("Datos inválidos");
            }

            try
            {
                // Actualiza la contraseña del usuario y verifica si el usuario existe
                var user = await _userService.UpdateUserPasswordAsync(dto.Username, dto.NewPassword);

                if (user == null)
                {
                    return NotFound("Usuario no encontrado"); // Retorna un error 404 si el usuario no fue encontrado
                }

                // Retorna un mensaje de exito
                return Ok("Contraseña actualizada exitosamente");
            }
            catch (KeyNotFoundException)
            {
                return NotFound("Usuario no encontrado"); // Retorna un error 404 si el usuario no fue encontrado
            }
        }

        // Endpoint para realizar el inicio de sesion (no requiere autenticacion)
        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] Login user)
        {
            // Valida que los datos de inicio de sesion sean correctos
            if (user == null || string.IsNullOrEmpty(user.User) || string.IsNullOrEmpty(user.Password))
            {
                return BadRequest("Solicitud de usuario inválida");
            }

            // Valida las credenciales del usuario
            var userFromDb = await _userService.ValidateUserAsync(user.User, user.Password);

            // Retorna un error 401 si las credenciales son incorrectas
            if (userFromDb == null)
            {
                return Unauthorized("Credenciales incorrectas");
            }

            // Asegurarse de que el usuario tiene un rol asignado
            if (userFromDb.Role == null)
            {
                return BadRequest("El usuario no tiene un rol asignado.");
            }

            // Obtener el rol del usuario
            string userRole = userFromDb.Role?.roleName;

            // Verificar si la clave secreta JWT esta configurada correctamente
            var secretKeyString = _configuration["JWT:Secret"];
            if (string.IsNullOrEmpty(secretKeyString))
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Clave secreta de JWT no está configurada.");
            }

            // Crear la clave y credenciales de firma para el token
            var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKeyString));
            var signinCredentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);

            // Crear los claims, incluyendo el rol correcto del usuario
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.User),
                new Claim(ClaimTypes.Role, userRole)
            };

            // Configurar las opciones del token JWT
            var tokenOptions = new JwtSecurityToken(
                issuer: _configuration["JWT:ValidIssuer"],
                audience: _configuration["JWT:ValidAudience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(60),
                signingCredentials: signinCredentials
            );

            // Generar el token JWT
            var tokenString = new JwtSecurityTokenHandler().WriteToken(tokenOptions);

            // Retorna el token JWT en la respuesta
            return Ok(new
            {
                Message = "Inicio de sesión realizado correctamente.",
                Token = tokenString,
                userName = user.User
            });
        }
        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerdto)
        {
            // Valida que los datos del registro sean correctos
            if (registerdto == null || string.IsNullOrEmpty(registerdto.Username) || string.IsNullOrEmpty(registerdto.Password) || string.IsNullOrEmpty(registerdto.Email))
            {
                return BadRequest(new { message = "Datos inválidos. Asegurate de proporcionar nombre de usuario, contraseña y correo electronico." });
            }

            try
            {
                // Verifica si el usuario ya existe
                var existingUser = await _userService.GetUserByUsernameAsync(registerdto.Username);
                if (existingUser != null)
                {
                    return Conflict(new { message = "El usuario ya existe. Por favor, elige otro." }); // Retorna un error 409 si el usuario ya existe
                }

                var existingEmail = await _userService.GetUserByEmailAsync(registerdto.Email);
                if (existingEmail != null)
                {
                    return Conflict(new { message = "El correo electronico ya está en uso." }); // Retorna un error 409 si el email ya esta registrado
                }

                // Crea un nuevo usuario
                var newUser = new User
                {
                    username = registerdto.Username,
                    passwordHash = registerdto.Password,
                    email = registerdto.Email, // Agrega esta línea
                    roleId = registerdto.RoleId, // Asignar el RoleId desde el DTO
                    statusUser = true, // Puedes establecer el estado como activo por defecto
                    createdUserBy = Environment.MachineName, // Aquí puedes usar el sistema o el usuario que crea el registro
                    createdUserDate = DateTime.Now // Fecha de creación
                };

                // Guarda el nuevo usuario en la base de datos
                await _userService.CreateUserAsync(newUser);

                return Ok("Usuario registrado exitosamente");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Error al registrar el usuario: El correo ya esta en uso."});
            }
        }
    }
}