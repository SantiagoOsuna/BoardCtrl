using System.Text.Json.Serialization;

namespace boardCtrl.Models
{
    public class Slide
    {
        public int slideId { get; set; } // Identificador unico de la diapositiva
        public string? titleSlide { get; set; } // Descripcion de la diapositiva
        public string? uRL { get; set; } // URL de la imagen o contenido de la diapositiva
        public int time { get; set; } // Tiempo (en segundos) que la diapositiva debe mostrarse
        public int boardId { get; set; } //Identificador del tablero al que pertenece la diapositiva
        public bool statusSlide { get; set; } // Estado de la diapositiva (activa/inactiva)
        public string? createdSlideBy { get; set; } // Usuario que creo la diapositiva
        public string? editedSlideBy { get; set; } // Usuario que modifico la diapositiva
        [JsonIgnore]
        public DateTime? createdSlideDate { get; set; } = DateTime.Now; // Fecha de creacion de la diapositiva (ignorando en JSON)
        [JsonIgnore]
        public DateTime? editedSlideDate { get; set; } // Fecha de la ultima modificacion (ignorando en JSON)
        [JsonIgnore]
        public Board? Board { get; set; } // Tablero asociado a la diapositiva (ignorando en JSON)
    }
}