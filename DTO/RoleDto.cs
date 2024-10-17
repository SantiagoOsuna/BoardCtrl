using System.Text.Json.Serialization;

namespace boardCtrl.DTO
{
    public class RoleDto
    {
        public int roleId { get; set; } // Identificador unico del rol
        public string? roleName { get; set; } // Nombre del rol (ej: Admin, User)
        public bool statusRole { get; set; } // Estado del rol (activo/inactivo)
        public string? createdRoleBy { get; set; } // Usuario que creo el rol
        public string? editedRoleBy { get; set; } // Usuario que modifico el rol
        [JsonIgnore]
        public DateTime? createdRoleDate { get; set; } = DateTime.Now; // Fecha de ultima modificacion (ignorada en JSON)
        [JsonIgnore]
        public DateTime? editedRoleDate { get; set; } // Fecha de ultima modificacion (ignorada en JSON)
        [JsonIgnore]
        public ICollection<UserDto>? Users { get; set; } // Coleccion de usuarios asociados a este rol (ignoada en JSON)
    }
}