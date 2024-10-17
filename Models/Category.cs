using System.Text.Json.Serialization;

namespace boardCtrl.Models
{
    public class Category
    {
        public int categoryId { get; set; } // Identificador unico de la categoria
        public string? titleCategory { get; set; } // Descripcion de la categoria (opcional)
        public bool statusCategory { get; set; } // Estado de la categoria (activo/inactivo)
        public string? createdCategoryBy { get; set; } // Usuario que creo la categoria
        public string? editedCategoryBy { get; set; } // Usuario que modifico la categoria
        [JsonIgnore]
        public DateTime? createdCategoryDate { get; set; } = DateTime.Now; // Fecha de creacion de la categoria, valor por defecto es la fecha actual
        [JsonIgnore]
        public DateTime? editedCategoryDate { get; set; } // Fecha de ultima modificacion de la categoria (opcional)
    }
}
