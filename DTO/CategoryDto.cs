namespace boardCtrl.DTO
{
    public class CategoryDto
    {
        public int categoryId { get; set; } // Identificador unico de la cateogira
        public string? titleCategory { get; set; } // Descripcion de la categoria (opcional)
        public bool statusCategory { get; set; } // Estado de la categoria (activo/inactivo)
        public string? createdCategoryBy { get; set; } // Usuario que creo esta categoria
        public string? editedCategoryBy { get; set; } // Usuario que modifico esta categoria
        public DateTime? createdCategoryDate { get; set; } = DateTime.Now; // Fecha de creacion (valor por defecto: ahora)
        public DateTime? editedCategoryDate { get; set; } // Fecha de ultima modificacion (opcional)
    }
}