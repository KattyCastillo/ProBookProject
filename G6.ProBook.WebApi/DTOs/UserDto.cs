namespace G6.ProBook.WebApi.DTOs
{
    public class UserDto
    {

        public string Id { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = "user";
        public string ProfilePictureUrl { get; set; } = string.Empty;
        public bool HasReserved { get; set; } = false;
        public DateTime ReservationTimestamp { get; set; } = DateTime.MinValue;
    }

    /// <summary>
    /// RegisterDto es lo que recibira el backend cuando alguien se registra
    /// </summary>
    public class RegisterDto
    {
        public string Email { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;

        public string FullName { get; set; } = string.Empty;
    }

    public class LoginDto
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
    /// <summary>
    /// AuthResponse es la informacion que recibimos de nuestra peticion desde el FE
    /// Contiene el token que el FE recibe para futuras peticiones
    /// </summary>
    public class AuthResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;

        public string Token { get; set; } = string.Empty;
        public UserDto User { get; set; } = new UserDto();
    }
}