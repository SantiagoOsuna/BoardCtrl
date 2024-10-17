namespace boardCtrl.DTO
{
    public class ChangePasswordDto
    {
        // Nombre de usuario del que se desea cambiar la contraseña
        public required string Username { get; set; }
        // Nueva contraseña que se desea asignar
        public required string NewPassword { get; set; }
    }
}