namespace boardCtrl.Models
{
    public class JWTTOKENResponse
    {
        public string? Token { get; set; }
        // Token JWT generado para autenticacion
        public string? ValidIssuer { get; set; } // Emisor valido del token JWT
        public string? ValidAudience { get; set; } // Audiencia valida del token JWT
        public string? Secret { get; set; } // Clave secreta usada para firmar el token JWT
    }
}