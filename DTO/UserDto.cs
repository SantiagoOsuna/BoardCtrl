using System.Text.Json.Serialization;

namespace boardCtrl.DTO
{
    public class UserDto
    {
        public int userId { get; set; } // Identificador unico del usuario
        public string? username { get; set; } // Nombre de usuario
        [JsonIgnore]
        public string? passwordHash { get; set; } 
        public int roleId { get; set; } // Clave foránea que relaciona al usuario con su rol
        public bool statusUser { get; set; } // Estado del usuario (activo/inactivo)
        public string? createdUserBy { get; set; } // Usuario que creo este registro
        public string? editedUserBy { get; set; } // Usuario que modifico este registro

        [JsonIgnore]
        public DateTime? createdUserDate { get; set; } = DateTime.Now; // Fecha de creacion (valor por defecto: ahora)
        [JsonIgnore]
        public DateTime? editedUserDate { get; set; } // Fecha de ultima modificacion

        // Relación muchos-a-uno: Un usuario tiene un solo rol
        [JsonIgnore]
        public RoleDto? Role { get; set; } // Rol asociado al usuario
    }
}