using System.Text.Json.Serialization; // Importa para la serializacion JSON

namespace boardCtrl.Models
{
    public class Board
    {
        public int boardId { get; set; } // Identificador unico del tablero
        public required string titleBoard { get; set; } // Titulo del tablero
        public string? descriptionBoard { get; set; } // Descripcion del tablero
        public bool statusBoard { get; set; } // Estado del tablero (habilitado/inhabilitado)
        public int categoryId { get; set; }
        public string? createdBoardBy { get; set; } // Usuario que creo el tablero (opcional)
        public string? editedBoardBy { get; set; } // Usuario que modifico el tablero (opcional)
        
        [JsonIgnore] // Ignora esta propiedad en la serializacion JSON
        public DateTime? createdBoardDate { get; set; } // Fecha de creacion del tablero
        
        [JsonIgnore] // Ignora esta propiedad en la serializacion JSON
        public DateTime? editedBoardDate { get; set; } // Fecha de ultima modificacion del tablero
        
        [JsonIgnore]
        public virtual Category? Category { get; set; } // Categoria asociada al tablero (opcional)
        [JsonIgnore]
        public ICollection<Slide>? Slides { get; set; }
    }
}