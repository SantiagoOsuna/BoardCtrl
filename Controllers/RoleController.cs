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
    public class RoleController : ControllerBase
    {
        // Inyeccion de dependencia del contexto de la base de datos
        public readonly boardCtrlContext _context;

        // Constructor que recibe el contexto de la base de datos
        public RoleController(boardCtrlContext context)
        {
            _context = context;
        }

        // Endpoint para obtener una lista de Roles con paginacion
        [HttpGet("FullRoles")]
        [Authorize(Roles = "Admin")] // Solo los usuarios con rol Admin puede acceder
        public async Task<IActionResult> GetRoles([FromQuery] int? page, [FromQuery] int? pageSize)
        {
            // Determina la pagina actual y el tamaño de la pagina, con valores predeterminados
            int currentPage = page ?? 1; // Si no se proporciona la pagina, se usa 1 como valor predeterminado
            int recordsPerPage = pageSize ?? 5; // Si no se proporciona el tamaño de la pagina, se usa 5 como valor predeterminado

            // Valida que el tamaño de la pagina sea mayor que cero
            if (recordsPerPage < 1)
            {
                return BadRequest("El tamaño de pagina debe ser mayor que cero."); // Retorna 400 si el tamaño de la pagina es menor que 1
            }

            // Obtener el numero total de roles en la base de datos
            int totalRecords = await _context.Roles.CountAsync(); // Obtiene el numero total de registros

            int totalPages = (int)Math.Ceiling((double)totalRecords / recordsPerPage); // Calcular el numero total de paginas

            // Verificar si la pagina solicitada excede al numero total de paginas, ajusta a la ultima pagina
            if (currentPage > totalPages && totalPages > 0)
            {
                currentPage = totalPages;
            }

            // Obtiene la lista de roles con paginacion 
            var roles = await _context.Roles
                .Include(r => r.Users) // Incluye la coleccion de usuarios
                .Skip((currentPage - 1) * recordsPerPage) // Omite los registros de las paginas anteriores
                .Take(recordsPerPage) // Toma solo los registros de la pagina actual
                .Select(r => new RoleDto
                {
                    roleId = r.roleId,
                    roleName = r.roleName,
                    statusRole = r.statusRole,
                    // Mapeo de la coleccion de usuarios a la propiedad Users de RoleDto
                    Users = r.Users != null ? r.Users.Select(u => new UserDto
                    {
                        userId = u.userId,
                        username = u.username,
                        roleId = u.roleId // Clave foranea en UserDto
                    }).ToList() : new List<UserDto>() // Lista vacia si es nulo
                })
                .ToListAsync(); // Cambia a ToListAsync para la operacion asincronica

            if (!roles.Any())
            {
                return NotFound("No se encontraron los roles"); // Retorna 404 si no encuentra roles
            }

            // Retorna los roles con la paginacion
            return Ok(new
            {
                total_pages = totalPages, // Total de paginas
                current_page = currentPage, // Pagina actual
                records = roles // Registros de roles
            });
        }

        // Endpoint para obtener el role por ID
        [HttpGet("Role-id/{id}")]
        [Authorize(Roles = "Admin")] // Solo Admin puede acceder
        public IActionResult GetRoleDetails(int id)
        {
            // Busca el rol por ID, incluyendo sus usuarios
            var role = _context.Roles
                .Include(r => r.Users) // Incluye la coleccion de usuarios
                .Select(r => new RoleDto
                {
                    roleId = r.roleId,
                    roleName = r.roleName,
                    statusRole = r.statusRole,
                    Users = r.Users != null ? r.Users.Select(u => new UserDto
                    {
                        userId = u.userId,
                        username = u.username,
                        roleId = u.roleId // El ID del rol al que pertenece el usuario
                    }).ToList() : new List<UserDto>() // Convertimos la coleccion a lista vacia si es nula
                })
                .FirstOrDefault(r => r.roleId == id); // Filtra por RoleId

            if (role == null)
            {
                return NotFound(); // Retorna 404 si no encuentra el rol
            }

            return Ok(role); // Retorna el rol en formato JSON
        }

        // Endpoint para editar el Role por el ID
        [HttpPut("Update")]
        [Authorize(Roles = "Admin")]
        public IActionResult UpdateRole(int id, [FromBody] Role role)
        {
            // Valida si los datos son correctos
            if (role == null || id != role.roleId)
            {
                return BadRequest("Datos invalidos"); // Retorna 400 si los datos no son validos
            }

            // Busca el rol existente por ID
            var existingRole = _context.Roles.Find(id);
            if (existingRole == null)
            {
                return NotFound("No encuentra el rol"); // Retorna 404 si no encuentra el rol
            }

            var username = User.FindFirst(ClaimTypes.Name)?.Value;
            // Actualiza los valores del rol
            existingRole.roleName = role.roleName;
            existingRole.editedRoleBy = username; // Nombre del computador que realiza la modificacion
            existingRole.editedRoleDate = DateTime.UtcNow; // Fecha de modificacion actual

            // Guarda los cambios
            _context.Roles.Update(existingRole);
            _context.SaveChanges(); // Guarda en la base de datos

            return NoContent(); // Retorna 204 (sin contenido) tras la actualizacion exitosa
        }

        // Endpoint para crear un nuevo role
        [HttpPost("Create")]
        [Authorize(Roles = "Admin")] // Solo Admin puede crear roles
        public IActionResult CreateRole([FromBody] Role role)
        {
            // Valida si los datos son nulos
            if (role == null)
            {
                return BadRequest("Datos del rol null"); // Retorna 400 si el rol es nulo
            }

            var username = User.FindFirst(ClaimTypes.Name)?.Value;
            // Asigna valores de auditoría
            role.createdRoleBy = username; // Nombre del computador que crea el rol
            role.createdRoleDate = DateTime.UtcNow; // Fecha de creacion actual

            // Añade el rol a la base de datos
            _context.Roles.Add(role);
            _context.SaveChanges(); // Guarda en la base de datos

            // Mapeo a DTO y retorna la nueva entidad creada
            var roleDto = new RoleDto
            {
                roleId = role.roleId,
                roleName = role.roleName,
                statusRole = role.statusRole,
                createdRoleBy = role.createdRoleBy,
                editedRoleBy = role.editedRoleBy,
                createdRoleDate = role.createdRoleDate,
                editedRoleDate = role.editedRoleDate
            };
            return CreatedAtAction(nameof(GetRoleDetails), new { id = role.roleId }, roleDto); // Retorna 201 (creado)
        }
        // Metodo DELETE (o Toggle Status) para activar o desactivar un rol
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")] // Solo los usuarios con rol "Admin" pueden usar este endpoint
        public async Task<IActionResult> ToggleRolesStatus(int id, [FromQuery] bool? activate = null)
        {
            // Busca el rol por su ID
            var role = await _context.Roles.FindAsync(id);

            // Si no se encuentra el rol, retorna un error 404 (Not Found)
            if (role == null)
            {
                return NotFound("No se encontro el rol");
            }

            // Si se proporciona el parametro activate, establece el estado del rol segun el valor de activate
            // Si no se proporcionar, alterna el estado actual del rol
            if (activate.HasValue)
            {
                role.statusRole = activate.Value; // True o false segun el parametro
            }
            else
            {
                // Si no se proporciona, simplemente alterna el estado actual
                role.statusRole = !role.statusRole;
            }

            // Actualiza la informacion del usuario que realizo la modificacion
            role.editedRoleBy = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            role.editedRoleDate = DateTime.Now;

            // Marca el objeto como modificado en el contexto de datos
            _context.Entry(role).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            // Retorna un codigo 204 (No Content) para indicar que la operacion fue exitosa
            return NoContent();
        }
    }
}