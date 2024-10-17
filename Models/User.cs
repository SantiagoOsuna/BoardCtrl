using System.Text.Json.Serialization;

namespace boardCtrl.Models
{
    public class User
    {
        public int userId { get; set; } // Identifiacdor unico del usuario
        public string? username { get; set; } // Nombre de usuario
        [JsonIgnore]
        public string? passwordHash { get; set; }
        public string? email { get; set; }
        public int roleId { get; set; } // Identificador del rol asociado
        public bool statusUser { get; set; } // Estado del usuario (activo/inactivo)
        public string? createdUserBy { get; set; } // Usuario que creo este registro
        public string? editedUserBy { get; set; } // Usuario que modifico este registro
        
        [JsonIgnore]
        public DateTime? createdUserDate { get; set; } = DateTime.Now; // Fecha de creacion del registro (ignorado en JSON)
        
        [JsonIgnore]
        public DateTime? editedUserDate { get; set; } // Fecha de la ultima modificacion (ignorado en JSON)
        [JsonIgnore]
        public Role? Role { get; set; } // Rol asociado al usuario (ignorado en JSON)
    }
}