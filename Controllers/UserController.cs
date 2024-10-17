using boardCtrl.DATA; // Importa el contexto de la base de datos
using boardCtrl.DTO; // Importa los DTOs utilizando para transferir datos
using boardCtrl.Models; // Importa los modelos que representan las entidades de la base de datos
using Microsoft.AspNetCore.Authorization; // Importa las funcionalidades para manejar autorizacion
using Microsoft.AspNetCore.Mvc; // Importa las funcionalidades para manejar controladores y acciones
using Microsoft.EntityFrameworkCore;
using System.Security.Claims; // Importa las funcionalidades para trabajar con Entity Framework Core

namespace boardCtrl.Controllers
{
    // Define la ruta base para las solicitudes HTTP y marca esta clase como un controlador de API
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        // Inyeccion de dependencia del contexto de la base de datos
        public readonly boardCtrlContext _context;

        // Constructor que recibe el contexto de la base de datos
        public UserController(boardCtrlContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        // Endpoint para obtener una lista de usuarios con paginacion
        [HttpGet("FullUsers")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetUsersAsync([FromQuery] int? page, [FromQuery] int? pageSize)
        {
            // Determina la pagina actual y el tamaño de la pagina, con valores predeterminados
            int currentPage = page ?? 1; // Si no se proporciona la pagina, se usa 1 como valor predeterminado
            int recordsPerPage = pageSize ?? 5; // Si no se proporciona el tamaño de la pagina, se usa 5 como valor predeterminado

            // Valida que el tamaño de la pagina sea mayor que cero
            if (recordsPerPage < 1)
            {
                return BadRequest("El tamaño de pagina debe ser mayor que cero."); // Retorna 400 si el tamaño de la pagina es menor que 1
            }

            // Obtener el numero total de usuarios en la base de datos
            int totalRecords = await _context.Users.CountAsync(); // Obtiene el numero total de registros

            int totalPages = (int)Math.Ceiling((double)totalRecords / recordsPerPage); // Calcular el numero total de paginas

            // Verificar si la pagina solicitada excede al numero total de paginas, ajusta a la ultima pagina
            if (currentPage > totalPages && totalPages > 0)
            {
                currentPage = totalPages;
            }

            // Obtiene la lista de Users con paginacion
            var users = await _context.Users
                .Skip((currentPage - 1) * recordsPerPage) // Saltar a la pagina actual
                .Take(recordsPerPage) // Tomar para el numero de registros por pagina
                .Include(s => s.Role) // Incluir la relación Role
                .Select(s => new UserDto
                {
                    // Mapea cada usuario a un UserDto, que se usa para transferir los datos al cliente
                    userId = s.userId,
                    username = s.username,
                    statusUser = s.statusUser,
                    roleId = s.Role != null ? s.Role.roleId : default(int), // Si el usuario tiene rol, asigna el ID del rol
                    Role = s.Role != null ? new RoleDto
                    {
                        roleId = s.Role.roleId,
                        roleName = s.Role.roleName,
                    } : null // Si no tiene rol, retorna null para el DTO del rol
                })
                .ToListAsync(); // Ejecuta la consulta de manera asincrona

            if (!users.Any())
            {
                return NotFound("No se encontro el usuario"); // Retorna 404 si no se encuentran usuarios
            }

            // Retorna la respuesta con la lista de usuarios, el total de paginas y la pagina actual
            return Ok(new
            {
                total_pages = totalPages, // Total de paginas calculadas
                current_page = currentPage, // Pagina actual
                records = users // Lista de usuarios
            });
        }

        // Endpoint para obtener los detalles de un usuario especifico por su ID
        [HttpGet("User-id")]
        [Authorize(Roles = "Admin")] // Solo accesible por el rol 'Admin'
        public IActionResult GetUserDetails(int id)
        {
            // Consulta a la base de datos para obtener el usuario y su rol asociado
            var user = _context.Users
                .Include(s => s.Role) // Incluir la relacion Role en la consulta
                .Select(s => new UserDto
                {
                    userId = s.userId,
                    username = s.username,
                    statusUser = s.statusUser,
                    roleId = s.Role != null ? s.Role.roleId : default(int),
                    Role = s.Role != null ? new RoleDto
                    {
                        roleId = s.Role.roleId,
                        roleName = s.Role.roleName
                    } : null // Si no tiene rol, retorna null para el DTO del rol
                })
                .FirstOrDefault(s => s.userId == id); // Obtiene el usuario que coincide con el ID proporcionado

            if (user == null)
            {
                return NotFound("No se encontro el usuario"); // Retorna 404 si el usuario no es encontrado
            }

            return Ok(user); // Retorna el usuario encontrado
        }

        // Endpoint para actualizar un usuario existente
        [HttpPut("Update")]
        [Authorize(Roles = "Admin")]
        public IActionResult UpdateUser(int id, [FromBody] User user)
        {
            // Verifica que los datos del usuario y el ID sean validos
            if (user == null || id != user.userId)
            {
                return BadRequest("Datos de usuario invalidos."); // Retorna un error 400 si los datos son incorrectos
            }

            var existingUser = _context.Users.Find(id); // Busca el usuario en la base de datos
            if (existingUser == null)
            {
                return NotFound("No se encontro el usuario"); // Retorna 404 si no se encuentra el usuario
            }

            var username = User.FindFirst(ClaimTypes.Name)?.Value;
            // Actualiza las propiedades permitidas del usuario
            existingUser.username = user.username;
            existingUser.roleId = user.roleId;
            existingUser.statusUser = user.statusUser;
            existingUser.editedUserBy = username; // Asigna el nombre de la maquina para registrar quien modifico
            existingUser.editedUserDate = DateTime.UtcNow; // Actualiza la fecha de modificacion

            _context.Users.Update(existingUser); // Actualiza el usuario en la base de datos
            _context.SaveChanges(); // Guarda los cambios

            return NoContent(); // Retorna 204 (sin contenido) indicando que la actualizacion fue exitosa
        }

        // Endpoint para crear un nuevo usuario
        [HttpPost("Create")]
        [Authorize(Roles = "Admin")] // Solo accesible por el rol 'Admin'
        public IActionResult CreateUser([FromBody] User user)
        {
            if (user == null)
            {
                return BadRequest("Datos del usuario son nulos"); // Retorna 400 si los datos del usuario son invalidos
            }

            // Verifica que el RoleId proporcionado sea valido
            var role = _context.Roles.FirstOrDefault(r => r.roleId == user.roleId);
            if (role == null)
            {
                return BadRequest("RoleId invalido"); // Retorna 400 si el RoleId es invalido
            }

            // Verifica que el nombre de usuario no exista ya en la base de datos
            bool usernameExists = _context.Users.Any(u => u.username == user.username);
            if (usernameExists)
            {
                return Conflict("Usuario con este username ya existe"); // Retorna 400 si ya existe un usuario con el mismo nombre
            }

            var username = User.FindFirst(ClaimTypes.Name)?.Value;
            // Asignar valores adicionales de auditoria
            user.createdUserBy = username; // Asigna el nombre de la maquina que creo el usuario
            user.createdUserDate = DateTime.UtcNow; // Asigna la fecha de creacion

            _context.Users.Add(user); // Agrega el nuevo usuario a la base de datos
            _context.SaveChanges(); // Guarda los cambios en la base de datos

            // Mapea el nuevo usuario a un UserDto para la respuesta
            var userDto = new UserDto
            {
                userId = user.userId,
                username = user.username,
                roleId = user.roleId,
                statusUser = user.statusUser,
                createdUserBy = user.createdUserBy,
                editedUserBy = user.editedUserBy,
                createdUserDate = user.createdUserDate,
                editedUserDate = user.editedUserDate
            };
            return CreatedAtAction(nameof(GetUserDetails), new { id = user.userId }, userDto);
        }

        // Endpoint para elimianr un usuario por el ID
        [HttpDelete("delete/{id}")]
        [Authorize(Roles = "Admin")]
        public IActionResult DeleteUser(int id)
        {
            var user = _context.Users.Find(id);

            if (user == null)
            {
                return NotFound("No se encontro el usuario.");
            }

            _context.Users.Remove(user);
            _context.SaveChanges();

            return NoContent();
        }
        // Metodo DELETE (o Toggle Status) para activar o desactivar un usuario
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")] // Solo los usuarios con rol "Admin" pueden usar este metodo
        public async Task<IActionResult> ToggleUsersStatus(int id, [FromQuery] bool? activate = null)
        {
            // Busca el usuario por su ID
            var user = await _context.Users.FindAsync(id);

            // Si no se encuentra el usuario, retorna un error 404 (Not Found)
            if (user == null)
            {
                return NotFound("No se encontro el usuario");
            }

            // Si se proporciona el parametro activate, establece el estado del usuario segun el valor de activate
            // Si no se proporciona, alterna el estado actual del usuario
            if (activate.HasValue)
            {
                user.statusUser = activate.Value; // True o false segun el parametro
            }
            else
            {
                // Si no se proporciona, simplemente alterna el estado actual
                user.statusUser = !user.statusUser;
            }

            // Actualiza la informacion del usuario que realizo la modificacion
            user.editedUserBy = User.FindFirst(ClaimTypes.Name)?.Value;
            user.editedUserDate = DateTime.Now;

            // Marca el objeto como modificado en el contexto de datos
            _context.Entry(user).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            // Retorna un codigo 204 (No Content) para indicar que la operacion fue exitosa
            return NoContent();
        }
    }
}