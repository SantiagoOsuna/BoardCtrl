using boardCtrl.DTO; // Importa el espacio de nombres que contiene los objetos de transferencia de datos (DTO), como 'UserDto'
using boardCtrl.Models; // Importa el espacio de nombres que contiene los modelos, como 'User'

namespace boardCtrl.Services
{
    // Interfaz que define las operaciones relacionadas con los usuarios
    public interface IUserService
    {
        // Metodo que obtiene un usuario por su nombre de usuario de forma asíncrona
        // Devuelve un objeto 'UserDto' que contiene los detalles del usuario, o 'null' si no se encuentra
        Task<UserDto?> GetUserByUsernameAsync(string username);

        // Metodo que valida las credenciales del usuario de forma asíncrona
        // Devuelve un 'UserDto' si las credenciales son válidas o 'null' si no lo son

        Task<UserDto?> GetUserByEmailAsync(string email);

        Task<UserDto?> ValidateUserAsync(string username, string password);

        // Metodo que crea un nuevo usuario de forma asíncrona
        // No devuelve nada (Task), solo realiza la acción de crear el usuario en la base de datos
        Task CreateUserAsync(User user);

        // Metodo que actualiza la contraseña de un usuario de forma asíncrona
        // Devuelve el objeto 'User' actualizado si la operación fue exitosa, o 'null' si no se encontro el usuario
        Task<User> UpdateUserPasswordAsync(string username, string newPassword);
    }
}