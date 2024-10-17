using boardCtrl.DATA; // Importa el contexto de datos de la aplicacion
using boardCtrl.Models; // Importa los modelos de datos, como 'Role'
using Microsoft.EntityFrameworkCore; // Importa las herramientas de Entity Framework

namespace boardCtrl.Services
{
    // Clase 'RoleService' que implementa la interfaz 'IRoleService'
    public class RoleService : IRoleService
    {
        // Campo para manejar el contexto de la base de datos
        private readonly boardCtrlContext _context;

        // Constructor que inyecta el contexto de la base de datos
        public RoleService(boardCtrlContext context)
        {
            _context = context;
        }
        // Metodo que obtiene un rol por su ID de forma asincrona
        // Retorna un objeto 'Role' si encuentra el rol, o 'null' si no lo encuentra
        public async Task<Role?> GetRoleByIdAsync(int roleId)
        {
            // Realiza una busqueda en la tabla de roles usando el ID proporcionado
            return await _context.Roles.FirstOrDefaultAsync(r => r.roleId == roleId);
        }
    }
}