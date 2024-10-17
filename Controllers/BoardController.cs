using boardCtrl.DATA; // Importa el contexto de la base de datos
using boardCtrl.DTO; // Importa los DTOs utilizados para transferir datos
using boardCtrl.Models; // Importa los modelos que representan las entidades de la base de datos
using Microsoft.AspNetCore.Authorization; // Importa las funcionalidades para manejar controladores y acciones
using Microsoft.AspNetCore.Mvc; // Importa las funcionalidades para manejar controladores y acciones
using Microsoft.EntityFrameworkCore; // Importa las funcionalidades para trabajar con Entity Framework Core
using System.Security.Claims; // Importa las funcionalidades para manejar claims en JWT

namespace boardCtrl.Controllers
{
    // Define que este es un controlador de API y establece la ruta base para las peticiones HTTP
    [Route("api/[controller]")]
    [ApiController]
    public class BoardController : ControllerBase
    {
        // Inyeccion de dependencia del contexto de la base de datos
        public readonly boardCtrlContext _context;

        // Constructor que recibe el contexto de la base de datos
        public BoardController(boardCtrlContext context)
        {
            _context = context;
        }
        // Endpoint para visualizar todos los boards con paginacion
        [HttpGet("FullBoards")]
        [Authorize(Roles = "Admin, User")] // Permite acceso a usuarios con rol Admin o User
        public async Task<IActionResult> Get([FromQuery] int? page, [FromQuery] int? pagesize)
        {
            int currentpage = page ?? 1; // Si no se proporciona la pagina, usamos 1 como valor predeterminado
            int recordsPerPage = pagesize ?? 5; // Definir el numero de registros por pagina

            if (recordsPerPage < 1)
            {
                return BadRequest("El tamaño de pagina debe ser mayor que cero."); // Retorna 400 si el tamaño de la pagina es menor que 1
            }

            // Obtener el numero total de registros
            int total_records = await _context.Boards.CountAsync();

            // Calcula el numero total de paginas basado en el numero total de registros y el tamaño de la pagina
            int total_pages = (int)Math.Ceiling((double)total_records / recordsPerPage); // Calcula el total de paginas

            // Verificar si la pagina solicitada excede al numero total de paginas, ajusta a la ultima pagina
            if (currentpage > total_pages && total_pages > 0)
            {
                currentpage = total_pages;
            }

            // Obtiene todos los tableros con su categoria
            var board = _context.Boards
                .Skip((currentpage - 1) * recordsPerPage) // Omite los registros de las paginas anteriores
                .Take(recordsPerPage) // Toma solo los registros de la pagina actual
                .Include(b => b.Category) // Incluye la categoria relacionada
                .ToList();

            var username = User.FindFirst(ClaimTypes.NameIdentifier)?.Value; // Obtiene el nombre del usuario actual

            // Transforma los datos de Board a BoardDto
            var boardDtos = board.Select(b => new BoardDto
            {
                boardId = b.boardId,
                titleBoard = b.titleBoard,
                statusBoard = b.statusBoard,
                categoryId = b.Category != null ? new CategoryDto
                {
                    categoryId = b.Category.categoryId,
                    titleCategory = b.Category.titleCategory
                } : new CategoryDto(), // Asigna el nombre del usuario actual como creador
                createdBoardBy = b.createdBoardBy = username, // Asigna el nombre del usuario actual como creador
                editedBoardBy = b.editedBoardBy = username, // Asigna el nombre del usuario actual como modificador

            }).ToList();

            // Retorna un error 404 si no se encuentran tableros
            if (!boardDtos.Any())
            {
                return NotFound("No se encontraron tableros");
            }

            // Retorna la lista de tableros en formato JSON
            return Ok(boardDtos);
        }
        // Endpoint para obtener los detalles de un tablero especifico por su ID
        [HttpGet("Board-id/{id}")]
        [Authorize(Roles = "Admin, User")] // Permite acceso a usuarios con rol Admin o User
        public IActionResult GetBoardDetails(int id)
        {
            // Busca el tablero por ID y su categoria asociada
            var Board = _context.Boards
                .Include(b => b.Category)
                .FirstOrDefault(b => b.boardId == id);

            if (Board == null)
            {
                return NotFound("No se encontro el tablero"); // Retorna 404 si no encuentra el tablero
            }

            // Mapea los datos del tablero al DTO BoardDto
            var boardDto = new BoardDto
            {
                boardId = Board.boardId,
                titleBoard = Board.titleBoard,
                statusBoard = Board.statusBoard,
                categoryId = new CategoryDto()
                {
                    categoryId = Board.Category?.categoryId ?? 0, // Asigna si la categoria es nula
                    titleCategory = Board.Category?.titleCategory ?? "Sin descripcion", // Asigna "Sin descripcion" si Board.Category es null
                },

                createdBoardBy = Board.createdBoardBy ?? "", // Asigna el nombre del usuario actual como creador

                editedBoardBy = Board.editedBoardBy ?? "", // Asigna el nombre del usuario actual como modificador

                createdBoardDate = (DateTime)Board.createdBoardDate, // Asegura que la fecha sea no nula

                editedBoardDate = (DateTime)Board.editedBoardDate // Asegura que la fecha sea no nula
            };

            // Retorna el DTO del tablero en formato JSON
            return Ok(boardDto);
        }

        [HttpGet("list-boards-by-category")] // Visualiza todos los tableros conectados a una categoria
        [Authorize(Roles = "Admin, User")] // Tanto usuarios como el administrador pueden hacer uso de este endpoint
        public IActionResult GetBoardsByCategory(int categoryId, int pageNumber = 1, int pageSize = 20)
        {
            // Calcula el número de tableros a omitir
            var skip = (pageNumber - 1) * pageSize;

            // Obtener la categoría correspondiente
            var category = _context.Categories
                .Where(c => c.categoryId == categoryId)
                .Select(c => new
                {
                    categoryId = c.categoryId,
                    titleCategory = c.titleCategory
                })
                .FirstOrDefault();

            // Si la categoría no existe, retorna un error
            if (category == null)
            {
                return NotFound($"Categoría con ID {categoryId} no encontrada.");
            }

            // Obtener los tableros de la categoría
            var boards = _context.Boards
                .Include(b => b.Category) // Incluye la categoría relacionada al board
                .Where(b => b.Category.categoryId == categoryId) // Filtra los boards por el categoryId
                .Skip(skip) // Omitir los tableros según el número de página
                .Take(pageSize) // Tomar solo la cantidad de tableros definida por PageSize
                .ToList();

            // Crear el DTO de tableros
            var boardDtos = boards.Select(b => new
            {
                boardId = b.boardId,
                titleBoard = b.titleBoard,
                descriptionBoard = b.descriptionBoard,
                statusBoard = b.statusBoard,
                createdBoardBy = b.createdBoardBy,
                editedBoardBy = b.editedBoardBy,
                createdBoardDate = b.createdBoardDate,
                editedBoardDate = b.editedBoardDate,
                Category = new
                {
                    categoryId = b.Category.categoryId,
                    titleCategory = b.Category.titleCategory,
                    createdCategoryBy = b.Category.createdCategoryBy,
                    editedCategoryBy = b.Category.editedCategoryBy,
                    createdCategoryDate = b.Category.createdCategoryDate,
                    editedCategoryDate = b.Category.editedCategoryDate,
                    statusCategory = b.Category.statusCategory
                }
            }).ToList();

            // Obtener el total de tableros para la categoría para calcular la cantidad de páginas
            var totalBoards = _context.Boards.Count(b => b.Category.categoryId == categoryId);
            var totalPages = (int)Math.Ceiling((double)totalBoards / pageSize);

            // Respuesta 200 con los tableros de la categoría y el nombre de la categoría
            return Ok(new
            {
                Category = new
                {
                    categoryId = category.categoryId,
                    titleCategory = category.titleCategory
                },
                Boards = boardDtos,
                TotalBoards = totalBoards,
                TotalPages = totalPages,
                CurrentPage = pageNumber
            });
        }


        // Endpoint para actualizar un tablero existente
        [HttpPut("Update")]
        [Authorize(Roles = "Admin")] // Solo permite acceso a usuarios con rol Admin
        public IActionResult UpdateBoard(int id, [FromBody] Board board)
        {
            // Valida que los datos sean correctos
            if (board == null || id != board.boardId)
            {
                return BadRequest("Hay errores en la solicitud"); // Retorna 400 si hay errores en la solicitud
            }

            // Busca el tablero existente por su ID
            var existingBoard = _context.Boards.Find(id);
            if (existingBoard == null)
            {
                return NotFound("No se encuentra el tablero"); // Retorna 404 si no se encuentra el tablero
            }

            var username = User.FindFirst(ClaimTypes.Name)?.Value; // Obtiene el nombre del usuario actual

            // Actualiza los detalles del tablero
            existingBoard.statusBoard = board.statusBoard;
            existingBoard.titleBoard = board.titleBoard;
            existingBoard.descriptionBoard = board.descriptionBoard;
            existingBoard.createdBoardBy = username;
            existingBoard.editedBoardBy = username; // Usa el nombre del equipo como "modificado por"

            existingBoard.editedBoardDate = DateTime.UtcNow; // Actualiza la fecha de modificacion

            _context.Boards.Update(existingBoard); // Actualiza en la base de datos
            _context.SaveChanges(); // Guarda los cambios en la base de datos

            return NoContent(); // Retorna 204 (sin contenido)
        }
        // Endpoint para crear un nuevo tablero
        [HttpPost("Create")]
        [Authorize(Roles = "Admin")] // Solo permite acceso a usuarios con rol Admin
        public IActionResult CreateBoard([FromBody] Board board)
        {
            // Valida que el objeto board no sea nulo
            if (board == null)
            {
                return BadRequest("Hay errores en la solicitud"); // Retorna 400 si el objeto es nulo
            }

            var username = User.FindFirst(ClaimTypes.Name)?.Value; // Obtiene el nombre del usuario actual

            // Establece los valores de creacion y modificacion
            board.createdBoardBy = username; // Usar el nombre del usuario como creador 
            board.createdBoardDate = DateTime.UtcNow; // Establece la fecha de creacion en UTC

            _context.Boards.Add(board); // Agrega el nuevo tablero a la base de datos
            _context.SaveChanges(); // Guarda los cambios en la base de datos

            // Retorna 201 Created con el objeto creado
            return CreatedAtAction(nameof(Get), new { id = board.boardId }, board); // Retorna 201 (creado)
        }
        // Metodo DELETE (o Toggle Status) para activar o descativar un tablero
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")] // Solo los usuarios con rol "Admin" pueden usar este metodo
        public async Task<IActionResult> TogglesBoardsStatus(int id, [FromQuery] bool? activate = null)
        {
            // Busca el tablero por su ID
            var board = await _context.Boards.FindAsync(id);

            // Si no se encuentra el tablero, retorna un error 404 (Not Found)
            if (board == null)
            {
                return NotFound("No se encontro el board");
            }

            // Si se proporciona el parametro activate, establece el estado del tablero segun el valor de activate
            if (activate.HasValue)
            {
                board.statusBoard = !board.statusBoard;
            }

            // Actualiza la informacion del usuario que realizo la modificacion
            board.editedBoardBy = User.FindFirst(ClaimTypes.Name)?.Value;
            board.editedBoardDate = DateTime.Now;

            // Marca el objeto como modificado en el contexto de datos
            _context.Entry(board).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            // Retorna un codigo 204 (No Content) para indicar que la operacion fue exitosa
            return NoContent();
        }
    }
}