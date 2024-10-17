using System.Text.Json.Serialization;

namespace boardCtrl.DTO
{
    public class BoardDto
    {
        public int boardId { get; set; } // Identificador unico del tablero
        public required string titleBoard { get; set; } // Titulo del tablero
        public bool statusBoard { get; set; } // Estado del tablero (activo/inactivo)
        public CategoryDto? categoryId { get; set; } // Categoria asociada al tablero (DTO de Category)
        [JsonIgnore]
        public string? createdBoardBy { get; set; } // Usuario que creo este registro
        public string? editedBoardBy { get; set; } // Usuario que modifico este registro
        [JsonIgnore]
        public DateTime? createdBoardDate { get; set; } // Fecha de creacion del registro (ignorado en JSON)
        [JsonIgnore]
        public DateTime? editedBoardDate { get; set; } // Fecha de la ultima modificacion (ignorado en JSON)
    }
}