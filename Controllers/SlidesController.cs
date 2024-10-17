using boardCtrl.DATA; // Importa el contexto de la base de datos
using boardCtrl.DTO; // Importa los DTOs utilizando para transferir datos
using boardCtrl.Models; // Importa los modelos que representan las entidades de la base de datos
using Microsoft.AspNetCore.Authorization; // Importa las funcionalidades para manejar autorizacion
using Microsoft.AspNetCore.Mvc; // Importa las funcionalidades para manejar controladores y acciones
using Microsoft.EntityFrameworkCore;
using System.Security.Claims; // Importa las funcionalidades para trabajar con Entity Framework Core

namespace boardCtrl.Controllers
{
    // Define la ruta base de la API y marca la clase como un controlador de API
    [Route("api/[controller]")]
    [ApiController]
    public class SlidesController : ControllerBase
    {
        // Inyeccion de dependencias del contexto de la base de datos
        public readonly boardCtrlContext _context;

        // Constructor que recibe el contexto de base de datos
        public SlidesController(boardCtrlContext context)
        {
            _context = context; // Asigna el contexto a la propiedad local
        }

        // Endpoint para obtener todos los Slides de la base de datos
        [HttpGet("FullSlides")] // Define la ruta para obtener todos los slides
        [Authorize(Roles = "Admin, User")] // Solo los roles 'Admin y 'User' pueden acceder a este endpoint
        public async Task<IActionResult> GetSlides([FromQuery] int? page, [FromQuery] int? pageSize)
        {
            int currentPage = page ?? 1; // Si no se especifica la pagina, se asume por defecto la pagina 1
            int recordsPerPage = pageSize ?? 5; // Por defecto, se muestran 5 registros por pagina

            if (recordsPerPage < 1) // Validacion para asegurar que el tamaño de pagina sea mayor a 0
            {
                return BadRequest("El tamaño de pagina debe ser mayor que cero.");
            }

            // Obtiene el numero total de registros en la tabla Slides
            int totalRecords = await _context.Slides.CountAsync();

            // Calcular el numero total de paginas basado en los registros y el tamaño de pagina
            int totalPages = (int)Math.Ceiling((double)totalRecords / recordsPerPage);

            if (currentPage > totalPages)
            {
                currentPage = totalPages;
            }

            // Consulta los slides con paginacion, incluyendo la entidad 'Board' y su categoria asociada
            var slides = await _context.Slides
                .Skip((currentPage - 1) * recordsPerPage) // Omite los registros de las paginas anteriores
                .Take(recordsPerPage) // Toma solo los registros de la pagina actual
                .Include(s => s.Board) // Incluye la entidad relacionada 'Board'
                .ThenInclude(b => b.Category) // Incluye la entidad relacionada 'Category' del 'Board'
                .Select(s => new SlideDto // Proyecta los resultados en el DTO SlideDto
                {
                    slideId = s.slideId, // Asigna el ID del slide
                    titleSlide = s.titleSlide, // Asigna la descripcion del slide
                    uRL = s.uRL, // Asigna la URL del slide
                    time = s.time, // Asigna el tiempo del slide
                    boardId = s.Board != null ? s.Board.boardId : default, // Si existe el board, se asigna su ID
                    Board = s.Board == null ? null : new BoardDto // Si no hay board, se asigna null, de lo contrario se crea un BoardDto
                    {
                        boardId = s.Board.boardId, // Asigna el ID del board
                        titleBoard = s.Board.titleBoard ?? string.Empty, // Si el titulo es nulo, se asigna una cadena vacia
                        categoryId = s.Board.Category == null ? null : new CategoryDto // Si no hay categoria, se asigna null
                        {
                            categoryId = s.Board.Category.categoryId, // Asigna el ID de la categoria
                            titleCategory = s.Board.Category.titleCategory ?? string.Empty // Asigna una descripcion predeterminada si es nulo
                        }
                    }
                })
                .ToListAsync(); // Convierte la consulta en una lista asincronicamente

            if (!slides.Any()) // Si no hay slides, devuelve un 404
            {
                return NotFound();
            }

            return Ok(slides); // Retorna los slides en formato JSON con un codigo 200
        }

        // Endpoint para obtener el slide por ID
        [HttpGet("Slide-id")] // Ruta para obtener un slide por su ID 
        [Authorize(Roles = "Admin, User")] // Solo los roles 'Admin' y 'User' pueden acceder
        public IActionResult GetSlideDetails(int id)
        {
            // Consulta para obtener un slide especifico por su ID, incluyendo la entidad 'Board'
            var slide = _context.Slides
                .Include(s => s.Board) // Incluir la relacion con 'Board'
                .Select(s => new SlideDto
                {
                    slideId = s.slideId, // Asigna el ID del slide
                    titleSlide = s.titleSlide, // Asigna la descripcion del slide
                    uRL = s.uRL, // Asigna la URL del slide
                    time = s.time, // Asigna el tiempo del slide
                    boardId = s.Board != null ? s.Board.boardId : default(int), // Asigna el ID del board si no es nulo
                    Board = s.Board != null ? new BoardDto // Si el board existe, crea un BoardDto
                    {
                        boardId = s.Board.boardId, // Asigna el ID del board
                        titleBoard = s.Board.titleBoard // Asigna el titulo del board
                    } : null // Si no hay board, se asigna null
                })
                .FirstOrDefault(s => s.slideId == id); // Filtro el Slide por ID

            if (slide == null) // Si no encuentra el slide, devuelve un 404
            {
                return NotFound("No se encontro el slide");
            }

            return Ok(slide); // Devuelve el slide en formato JSON con un codigo 200
        }

        [HttpGet("List-Slide-by-board")]
        [Authorize(Roles = "Admin, User")] // Tanto usuarios como administrador pueden visualizar a gusto

        public IActionResult GetSlidesByBoard(int boardId)
        {
            var slides = _context.Slides
                .Include(s => s.Board)
                .ThenInclude(b => b.Category)
                .Where(s => s.Board.boardId == boardId)
                .ToList();
            // Verificar si no se encontraron slides
            if (!slides.Any())
            {
                return NotFound("No se encontraron slides para el tablero con ID {boardId}.");
            }
            var SlideDto = slides.Select(s => new
            {
                slideId = s.slideId,
                titleSlide = s.titleSlide,
                uRL = s.uRL,
                time = s.time,
                statusSlide = s.statusSlide,
                Board = new
                {
                    boardId = s.Board.boardId,
                    titleBoard = s.Board.titleBoard,
                    descriptionBoard = s.Board.descriptionBoard,
                    statusBoard = s.Board.statusBoard,
                    Category = new
                    {
                        categoryId = s.Board.Category.categoryId, // Acceso a categoryId a traves de Board
                        titleCategory = s.Board.Category.titleCategory,
                        createdCategoryBy = s.Board.Category.createdCategoryBy,
                        editedCategoryBy = s.Board.Category.editedCategoryBy,
                        createdCategoryDate = s.Board.Category.createdCategoryDate,
                        editedCategoryDate = s.Board.Category.editedCategoryDate,
                        statusCategory = s.Board.Category.statusCategory
                    }
                }
            }).ToList();
            return Ok(SlideDto);
        }

        // Endpoint para actualizar el slide
        [HttpPut("Update")] // Ruta para actualizar un slide por su ID
        [Authorize(Roles = "Admin")] // Solo los usuarios con rol 'Admin' pueden acceder
        public IActionResult UpdateSlide(int id, [FromBody] Slide Slides)
        {
            // Valida que el objeto Slide y el ID coincidan
            if (Slides == null || id != Slides.slideId)
            {
                return BadRequest("Datos invalidos"); // Si no coinciden, devuelve un error 400
            }

            // Busca el slide existente en la base de datos
            var existingSlide = _context.Slides.Find(id);
            if (existingSlide == null) // Si no se encuentra, devuelve un 404 
            {
                return NotFound("No encuentra el slide");
            }

            var username = User.FindFirst(ClaimTypes.Name)?.Value;
            // Actualiza los campos del slide existente con los nuevos valores
            existingSlide.titleSlide = Slides.titleSlide;
            existingSlide.uRL = Slides.uRL;
            existingSlide.statusSlide = Slides.statusSlide;
            existingSlide.time = Slides.time;
            existingSlide.editedSlideBy = username; // Asigna el nombre del computador que modifica el slide
            existingSlide.editedSlideDate = DateTime.UtcNow; // Asigna la fecha actual de modificacion

            // Actualiza el slide en la base de datos
            _context.Slides.Update(existingSlide);
            _context.SaveChanges(); // Guarda los cambios en la base de datos

            return NoContent(); // Devuelve un 204 (sin contenido) tras la actualizacion exitosa 
        }

        // Endpoint para crear un nuevo slide
        [HttpPost("Create")] // Ruta para crear un nuevo slide
        [Authorize(Roles = "Admin")] // Solo los usuarios con rol 'Admin' pueden acceder
        public IActionResult CreateSlide([FromBody] Slide Slides)
        {
            if (Slides == null) // Verifica si el objeto Slide es nulo
            {
                return BadRequest("Datos de slide null"); // Si es nulo, devuelve un error 400
            }

            var username = User.FindFirst(ClaimTypes.Name)?.Value;
            //Asignar valores de auditoria al nuevo slide
            Slides.createdSlideBy = username; // Asigna el nombre del computador que crea el slide
            Slides.createdSlideDate = DateTime.UtcNow; // Asigna la fecha actual de creacion

            // Añade el nuevo slide a la base de datos
            _context.Slides.Add(Slides);
            _context.SaveChanges(); // Guarda los cambios en la base de datos

            // Devuelve un 201 (creado) junto con el ID del nuevo slide
            return CreatedAtAction(nameof(GetSlideDetails), new
            {
                id = Slides.slideId // Devuelve el ID del slide recien creado
            }, Slides);
        }
        // Metodo DELETE (o Toggle Status) para activar o desactivar un slide
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")] // Solo los usuarios con rol "Admin" pueden usar este endpoint
        public async Task<IActionResult> ToggleSlidesStatus(int id, [FromQuery] bool? activate = null)
        {
            // Busca el slide por su ID
            var slide = await _context.Slides.FindAsync(id);

            // Si no se encuentra el tablero, retorna un error 404 (Nor Found)
            if (slide == null)
            {
                return NotFound();
            }

            // Si se proporciona el parametro activate, establece el estado del slide segun el valor de activate
            // Si no se proporciona, alteral el estado actual del slide
            if (activate.HasValue)
            {
                slide.statusSlide = activate.Value; // True o false segun el parametro
            }
            else
            {
                // Si no se proporciona, simplemente alterna el estado actual
                slide.statusSlide = !slide.statusSlide;
            }

            // Actualiza la informacion del usuario que realizo la modificacion
            slide.editedSlideBy = User.FindFirst(ClaimTypes.Name)?.Value;
            slide.editedSlideDate = DateTime.Now;

            // Marca el objeto como modificado en el contexto de datos
            _context.Entry(slide).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            // Retorna un codigo 204 (No Content) para indicar que la operacion fue exitosa
            return NoContent();

        }
    }
}