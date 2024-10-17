using boardCtrl.Models;
using boardCtrl.DTO;
using boardCtrl.Services;
using Microsoft.EntityFrameworkCore;
using boardCtrl.DATA;

public class UserService : IUserService
{

    // Campo privado para acceder al contexto de la base de datos
    private readonly boardCtrlContext _context;

    // Constructor que inyecta el contexto de la base de datos
    public UserService(boardCtrlContext context)
    {
        _context = context;
    }


    // Método para validar al usuario
    public async Task<UserDto?> ValidateUserAsync(string username, string password)
    {
        // Busca al usuario en la base de datos incluyendo el rol (si existe)
        var user = await _context.Users
            .Include(u => u.Role) // Carga la entidad 'Role' relacionada si es necesaria
            .SingleOrDefaultAsync(u => u.username == username); // Busca un unico usuario por nombre

        // Si el usuario se encuentra y la contraseña es correcta
        if (user != null && VerifyPassword(user, password))
        {
            // Crea un UserDto para retornar solo la informacion relevante (incluyendo el rol)
            var userDto = new UserDto
            {
                username = user.username,
                Role = user.Role != null ? new RoleDto
                {
                    roleId = user.Role.roleId,
                    roleName = user.Role.roleName
                } : null // Asegúrate de que el rol esté presente
            };
            return userDto;  // Retorna el UserDto con los datos necesarios
        }

        return null; // Retorna null si el usuario no se encuentra o la validacion falla
    }

    // Metodo para obtener un usuario por nombre de usuario
    public async Task<UserDto?> GetUserByUsernameAsync(string username)
    {
        // Busca al usuario en la base de datos
        var user = await _context.Users
            .Include(u => u.Role) // Incluye el rol si es necesario
            .SingleOrDefaultAsync(u => u.username == username); // Busca un unico usuario

        if (user == null)
        {
            return null; // Retorna null si no se encuentra el usuario
        }

        // Convierte el objeto User a UserDto para no exponer informacion sensible
        var userDto = new UserDto
        {
            username = user.username,
            Role = user.Role != null ? new RoleDto
            {
                roleId = user.Role.roleId,
                roleName = user.Role.roleName
            } : null // Asegúrate de que el rol esté presente
        };

        return userDto;
    }

    public async Task<UserDto?> GetUserByEmailAsync(string email)
    {
        // Busca al usuario en la base de datos
        var user = await _context.Users
            .Include(u => u.Role)
            .SingleOrDefaultAsync(u => u.email == email);
        if (user == null)
        {
            return null;
        }

        // Convierte el objeto User a UserDto para no exponer informacion sensible
        var userDto = new UserDto
        {
            username = user.username,
            Role = user.Role != null ? new RoleDto
            {
                roleId = user.Role.roleId,
                roleName = user.Role.roleName
            } : null // Asegurate de que el rol este presente
        };
        
        return userDto;
    }
    // Metodo para crear un nuevo usuario en la base de datos
    public async Task CreateUserAsync(User user)
    {
        if (string.IsNullOrEmpty(user.passwordHash))
        {
            throw new ArgumentException("La contraseña no puede estar vacía");
        }
        // Asegúrate de hashear la contraseña antes de guardarla
        user.passwordHash = HashPassword(user.passwordHash);
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
    }

    // Metodo para actualizar la contraseña de un usuario
    public async Task<User> UpdateUserPasswordAsync(string username, string newPassword)
    {
        // Busca al usuario por nombre de usuario
        var user = await _context.Users.FirstOrDefaultAsync(u => u.username == username);

        // Lanza una excepcion si no se encuentra el usuario
        if (user == null)
        {
            throw new KeyNotFoundException("Usuario no encontrado");
        }

        // Actualiza el hash de la contraseña con la nueva contraseña proporcionada
        user.passwordHash = HashPassword(newPassword); // Cambia a HashPassword
        _context.Users.Update(user); // Marca al usuario como actualizado
        await _context.SaveChangesAsync(); // Guarda los cambios

        return user; // Retorna el usuario actualizado
    }

    // Metodo privado para verificar la contraseña usando BCrypt
    private bool VerifyPassword(User user, string password)
    {
        // Compara la contraseña proporcionada con el hash almacenado
        return BCrypt.Net.BCrypt.Verify(password, user.passwordHash);
    }

    // Metodo privado para hashear la contraseña usando BCrypt
    private string HashPassword(string password)
    {
        // Implementa tu lógica de hashing aquí, por ejemplo:
        return BCrypt.Net.BCrypt.HashPassword(password); // Usa una librería como BCrypt.Net
    }
}