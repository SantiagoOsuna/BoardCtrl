using boardCtrl.DATA; // Importa el contexto de la base de datos
using boardCtrl.DTO; // Importa los DTOs utilizados para transferir datos
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
    public class CategoriesController : ControllerBase
    {
        // Inyeccion de dependencia del contexto de la base de datos
        public readonly boardCtrlContext _context;

        // Constructor que recibe el contexto de la base de datos
        public CategoriesController(boardCtrlContext context)
        {
            _context = context;
        }

        // Endpoint para obtener una lista de categorias con paginacion
        [HttpGet]
        [Route("FullCategories")]
        [Authorize(Roles = "Admin, User")]
        public async Task<IActionResult> Get([FromQuery] int? page, [FromQuery] int? pageSize)
        {
            int currentPage = page ?? 1;
            int recordsPerPage = pageSize ?? 5;

            if (recordsPerPage < 1)
            {
                return BadRequest("El tamaño de página debe ser mayor que cero.");
            }

            int total_records = await _context.Categories.CountAsync();
            int total_pages = (int)Math.Ceiling((double)total_records / recordsPerPage);

            if (currentPage > total_pages && total_pages > 0)
            {
                currentPage = total_pages;
            }

            var categories = await _context.Categories
                .Skip((currentPage - 1) * recordsPerPage)
                .Take(recordsPerPage)
                .Select(c => new CategoryDto
                {
                    categoryId = c.categoryId,
                    titleCategory = c.titleCategory,
                    statusCategory = c.statusCategory
                })
                .ToListAsync();

            if (!categories.Any())
            {
                return NotFound("No hay categorías.");
            }

            var response = new
            {
                items = categories,
                totalPages = total_pages
            };

            return Ok(response);
        }
        // Endpoint para obtener los detalles de una categoria especifica por su ID
        [HttpGet("Categories-id/{id}")]
        [Authorize(Roles = "Admin, User")] // Permite acceso a usuarios con rol Admin o User
        public IActionResult GetCategoryDetails(int id)
        {
            // Busca la categoria por ID
            var category = _context.Categories.Find(id);
            // Retorna un error 404 si no encuentra la categoria
            if (category == null)
            {
                return NotFound("No se encontro la categoria"); // Retorna 404 si no se encuentra la categoria
            }
            return Ok(category); // Retorna la categoria en formato JSON
        }

        // Endpoint para actualizar una categoria existente 
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")] // Solo permite acceso a usuarios con rol Admin
        public async Task<IActionResult> UpdateCategories(int id, CategoryDto categoryDto)
        {
            // Verifica si el ID proporcionado coincide con el ID de la categoria
            var username = User.FindFirst(ClaimTypes.Name)?.Value;
            if (id != categoryDto.categoryId)
            {
                return NotFound();
            }

            // Si no se encuentra la categoria, retorna un codigo 404 (Not Found)
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound("No se encontro la categoria");
            }

            // Actualiza las propiedades de la categoria con los valores proporcionados en el DTO
            category.titleCategory = categoryDto.titleCategory; // Actualiza la descripcion
            category.statusCategory = categoryDto.statusCategory; // Actualiza el estado de la categoria
            category.editedCategoryBy = username;
            category.editedCategoryDate = DateTime.UtcNow; // Establece la fecha actual como fecha de modificacion

            _context.Entry(category).State = EntityState.Modified;
            // Intente guardar los cambios en la base de datos
            try
            {
                await _context.SaveChangesAsync();
            }
            // Captura excepciones de concurrencia, por ejemplo, cuando dos usuarios intentan la misma entidad al tiempo
            catch (DbUpdateConcurrencyException)
            {
                if (!CategoryExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw; // Lanta la excepcion si ocurrio otro tipo de error
                }
            }

            return NoContent();
        }
        // Endpoint para crear una nueva categoria
        [HttpPost("Create")]
        [Authorize(Roles = "Admin")] // Solo permite acceso a usuarios con rol Admin
        public IActionResult CreateCategory([FromBody] Category category)
        {
            var username = User.FindFirst(ClaimTypes.Name)?.Value;
            // Valida que la categoria no sea nula
            if (category == null)
            {
                return BadRequest("Error en la solicitud"); // Retorna 400 si el objeto categoria es nulo
            }

            // Asigna valores de creacion y modificacion
            category.createdCategoryBy = Environment.MachineName; // Usa el nombre del computador
            category.createdCategoryDate = DateTime.UtcNow; // Establece la Fecha de creacion en UTC

            _context.Categories.Add(category); // Agrega la nueva categoria a la base de datos
            _context.SaveChanges(); // Guardar los cambios en la base de datos

            // Retorna 201 (creado) con la nueva categoria
            return CreatedAtAction(nameof(Get), new { id = category.categoryId }, category); // Retorna 201 (creado)
        }
        // Endpoint para eliminar una categoria por el ID
        [HttpDelete("delete/{id}")]
        [Authorize(Roles = "Admin")]
        public IActionResult DeleteCategories(int id)
        {
            var category = _context.Categories.Find(id);

            if (category == null)
            {
                return NotFound("No se encontro la categoria");
            }

            _context.Categories.Remove(category);
            _context.SaveChanges();

            return NoContent();
        }
        // Metodo DELETE (o Toggle Status) para activar o desactivar una categoria
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")] // Solo los usuarios con rol "Admin" pueden usar este endpoint
        public async Task<IActionResult> ToggleBoardsStatus(int id, [FromQuery] bool? activate = null)
        {
            // Busca la categoria por su ID
            var category = await _context.Categories.FindAsync(id);

            // Si no se encuentra la categoria, retorna un error 404 (Not Found)
            if (category == null)
            {
                return NotFound("No es encontro la categoria");
            }

            // Si se proporciona el parametro activate, establece el estado de la categoria segun el valor de activate
            // Si no se proporciona, alterna el estado actual del tablero
            if (activate.HasValue)
            {
                category.statusCategory = activate.Value; // True o false segun el parametro
            }
            else
            {
                // Si no se proporciona, simplemente alterna el estado actual
                category.statusCategory = !category.statusCategory;
            }

            // Actualiza la informacion del usuario que realizo la modificacion
            category.editedCategoryBy = User.FindFirst(ClaimTypes.Name)?.Value;
            category.editedCategoryDate = DateTime.Now;

            // Marca el objeto como modificado en el contexto de datos
            _context.Entry(category).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            // Retorna un codigo 204 (No Content) para indicar que la operacion fue exitosa
            return NoContent();
        }

        private bool CategoryExists(int id)
        {
            return _context.Categories.Any(e => e.categoryId == id);
        }
    }
}