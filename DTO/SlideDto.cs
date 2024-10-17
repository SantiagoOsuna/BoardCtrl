namespace boardCtrl.DTO
{
    public class SlideDto
    {
        public int slideId { get; set; } // Identificador unico del slide
        public string? titleSlide { get; set; } // Descripcion del contenido del slide
        public string? uRL { get; set; } // URL asociado al slide (imagen, archivo, etc.)
        public int time { get; set; } // Duracion en segundos que el slide se muestra
        public int boardId { get; set; } // Relacion con el ID del tablero al que pertenece
        public bool atatusSlide { get; set; } // Estado del slide (activo/inactivo)
        public string? createdSlideBy { get; set; } // Usuario que creo el slide
        public string? editedSlideBy { get; set; } // Usuario que modifico el slide
        public DateTime? createdSlideDate { get; set; } = DateTime.Now; // Fecha de creacion (valor por defecto: ahora)
        public DateTime? editedSlideDate { get; set; } // Fecha de ultima modificacion
        public BoardDto? Board { get; set; } // Relacion con el DTO de Board (tablero asociado al slide)
    }
}