using boardCtrl.Models;

namespace boardCtrl.Services
{
    // Interfaz que define las operaciones relacionadas con los roles
    public interface IRoleService
    {
        // Método que obtiene un rol basado en su ID de forma asíncrona
        // Devuelve un objeto de tipo 'Role', o 'null' si no se encuentra
        // Se espera que este método sea implementado por una clase concreta
        Task<Role?> GetRoleByIdAsync(int roleId); // 'roleId' es el parámetro que representa el ID del rol a buscar
    }
}