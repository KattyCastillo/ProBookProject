using G6.ProBook.WebApi.DTOs;
using G6.ProBook.WebApi.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace G6.ProBook.WebApi.Services
{
    //Servicio de autenticacion de usuario
    public class AuthService : IAuthService
    {

        private readonly IConfiguration _configuration;
        private readonly FirebaseService _firebaseService;

        public async Task<User> Register(RegisterDto registerDto)
        {
            try
            {
                if(string.IsNullOrEmpty(registerDto.Email) || string.IsNullOrEmpty(registerDto.Password))
                {
                    throw new ArgumentException("Email y password son requeridos");
                }

                if(registerDto.Password.Length <0)
                {
                    throw new ArgumentException("Password debe contener al menos 6 caracteres");
                }

                var userCollection = _firebaseService.GetCollection("users");

                var query = await userCollection.WhereEqualTo("Email", registerDto.Email).GetSnapshotAsync();

                if(query.Count > 0)
                {
                    throw new ArgumentException("El email ya esta registrado");
                }

                var newUser = new User()
                {
                    Id = Guid.NewGuid().ToString(),
                    Email = registerDto.Email,
                    Fullname = registerDto.FullName,
                    Role = "huesped",
                    CreatedAt = DateTime.UtcNow,
                    LastLogin = DateTime.UtcNow,
                    IsActive = true,
                    HasReserved = false,
                };

                await userCollection.Document(newUser.Id).SetAsync(newUser);

                return newUser;

            }
            catch (Exception e)
            {
                Console.WriteLine($"Error al registrar usuario: {e.Message}");
                throw;
            }
        }

        public async Task<(User user, string token)> Login(LoginDto loginDto)
        {
            try
            {
                if (string.IsNullOrEmpty(loginDto.Email) || string.IsNullOrEmpty(loginDto.Password))
                {
                    throw new ArgumentException("Email y password son requeridos");
                }

                var userCollection = _firebaseService.GetCollection("users");

                var query = await userCollection.WhereEqualTo("Email", loginDto.Email).GetSnapshotAsync();

                if (query.Count == 0)
                {
                    throw new ArgumentException("Email o password incorrecto");
                }

                var userDoc = query.Documents[0];

                var user = userDoc.ConvertTo<User>();

                var token = GenerateJwtToken(user);

                await userCollection.Document(user.Id).UpdateAsync(
                        new Dictionary<string, object>
                        {
                            {"LastLogin", DateTime.UtcNow }
                        }
                    );

                return (user, token);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error al logearse: {e.Message}");
                throw;
            }

        }

        public async Task<bool> ValidateToken(string token)
        {
            try
            {
                var secretKey = _configuration["Jwt:SecretKey"];

                if(string.IsNullOrEmpty(secretKey))
                {
                    return false;
                }

                var tokenHandler = new JwtSecurityTokenHandler();

                var key= Encoding.ASCII.GetBytes(secretKey);

                tokenHandler.ValidateToken(token, new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                }
                , out SecurityToken validateToken);

                return true;
            }
            catch (Exception e) {
                Console.WriteLine(e.Message);
                throw;
            }
        }

        public string GenerateJwtToken(User user)
        {
            throw new NotImplementedException();
        }

        public Task<User?> GetUserById(string userId)
        {
            throw new NotImplementedException();
        }

        
    }
}
