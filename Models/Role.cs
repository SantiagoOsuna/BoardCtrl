using System.Text.Json.Serialization;

namespace boardCtrl.Models
{
    public class Role
    {
        public int roleId { get; set; } // Identificador unico del rol
        public string? roleName { get; set; } // Nombre del rol
        public bool statusRole { get; set; } // Estado del rol (activo/inactivo)
        public string? createdRoleBy { get; set; } // Usuario que creo el rol
        public string? editedRoleBy { get; set; } // Usuario que modifico el rol
        public DateTime? createdRoleDate { get; set; } = DateTime.Now; // Fecha de creacion del rol
        [JsonIgnore]
        public DateTime? editedRoleDate { get; set; } // Fecha de la ultima modificacion (ignorando en JSON)

        [JsonIgnore]
        public ICollection<User>? Users { get; set; } // Coleccion de usuario asociados al rol (ignorado en JSON)
    }
}
