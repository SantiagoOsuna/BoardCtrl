namespace boardCtrl.DTO
{
    public class RegisterDto
    {
        public required string Username { get; set; } // Nombre de usuario para el nuevo registro
        public required string Password { get; set; } // Contraseña para el nuevo usuario
        public required string Email { get; set; } // Email para el nuevo usuario
        public int RoleId { get; set; }
    }
}
