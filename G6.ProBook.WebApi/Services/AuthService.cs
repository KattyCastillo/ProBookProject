using G6.ProBook.WebApi.DTOs;
using G6.ProBook.WebApi.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Security.Claims;
using BCrypt.Net;
using System.Security.Cryptography;
using Google.Cloud.Firestore;

namespace G6.ProBook.WebApi.Services
{
    //Servicio de autenticacion de usuario
    public class AuthService : IAuthService
    {

        private readonly IConfiguration _configuration;
        private readonly FirebaseService _firebaseService;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            FirebaseService firebaseService,
            IConfiguration configuration,
            ILogger<AuthService> logger)
        {
            _firebaseService = firebaseService;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<User> Register(RegisterDto registerDto)
        {
            try
            {
                if (string.IsNullOrEmpty(registerDto.Email) || string.IsNullOrEmpty(registerDto.Password))
                {
                    throw new ArgumentException("Email y password son requeridos");
                }

                if (registerDto.Password.Length < 6)
                {
                    throw new ArgumentException("Password debe contener al menos 6 caracteres");
                }

                var userCollection = _firebaseService.GetCollection("users");

                if (userCollection == null)
                {
                    throw new InvalidOperationException("No se pudo obtener la colección de usuarios");
                }

                var query = await userCollection.WhereEqualTo("Email", registerDto.Email).GetSnapshotAsync();

                if (query.Count > 0)
                {
                    throw new ArgumentException("El email ya esta registrado");
                }

                // Hashear la contraseña con BCrypt
                var passwordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password);

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

                // Guardar el usuario en Firestore usando Dictionary
                var userData = new Dictionary<string, object>
                {
                    { "Id", newUser.Id },
                    { "Email", newUser.Email },
                    { "Fullname", newUser.Fullname },
                    { "Role", newUser.Role },
                    { "ProfilePictureUrl", newUser.ProfilePictureUrl },
                    { "HasReserved", newUser.HasReserved },
                    { "CreatedAt", newUser.CreatedAt },
                    { "LastLogin", newUser.LastLogin },
                    { "IsActive", newUser.IsActive },
                    { "PasswordHash", passwordHash }  // Guardar hash, NO la contraseña
                };

                await userCollection.Document(newUser.Id).SetAsync(userData);

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
                if (string.IsNullOrWhiteSpace(loginDto.Email) ||
                    string.IsNullOrWhiteSpace(loginDto.Password))
                {
                    throw new ArgumentException("Email y contraseña son requeridos");
                }

                var usersCollection = _firebaseService.GetCollection("users");

                if (usersCollection == null)
                {
                    throw new InvalidOperationException("No se pudo obtener la colección de usuarios");
                }

                var query = await usersCollection
                    .WhereEqualTo("Email", loginDto.Email)
                    .GetSnapshotAsync();

                if (query.Count == 0)
                {
                    throw new InvalidOperationException("Email o contraseña incorrectos");
                }

                var userDoc = query.Documents[0];
                var userDict = userDoc.ToDictionary();

                // Obtener el hash de contraseña guardado
                var passwordHash = userDict["PasswordHash"].ToString();

                // Validar la contraseña contra el hash con BCrypt
                if (!BCrypt.Net.BCrypt.Verify(loginDto.Password, passwordHash))
                {
                    throw new InvalidOperationException("Email o contraseña incorrectos");
                }

                // Convertir el diccionario a objeto User
                var user = new User
                {
                    Id = userDict["Id"].ToString(),
                    Email = userDict["Email"].ToString(),
                    Fullname = userDict["Fullname"].ToString(),
                    Role = userDict["Role"].ToString(),
                    ProfilePictureUrl = userDict["ProfilePictureUrl"].ToString(),
                    CreatedAt = ((Timestamp)userDict["CreatedAt"]).ToDateTime(),
                    LastLogin = ((Timestamp)userDict["LastLogin"]).ToDateTime(),
                    ReservationTimestamp = userDict.ContainsKey("ReservationTimestamp") ? ((Timestamp)userDict["ReservationTimestamp"]).ToDateTime() : DateTime.MinValue,
                    IsActive = (bool)userDict["IsActive"],
                    HasReserved = (bool)userDict["HasReserved"]
                };

                var token = GenerateJwtToken(user);

                // Actualizar LastLogin
                await usersCollection.Document(user.Id).UpdateAsync(
                    new Dictionary<string, object>
                    {
                        { "LastLogin", DateTime.UtcNow }
                    }
                );

                return (user, token);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error en Login: {ex.Message}");
                throw;
            }

        }

        public async Task<bool> ValidateToken(string token)
        {
            try
            {
                var secretKey = _configuration["Jwt:SecretKey"];

                if (string.IsNullOrEmpty(secretKey))
                {
                    return false;
                }

                var tokenHandler = new JwtSecurityTokenHandler();

                var key = Encoding.ASCII.GetBytes(secretKey);

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
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw;
            }
        }

        public string GenerateJwtToken(User user)
        {
            try
            {
                var secretKey = _configuration["Jwt:SecretKey"];
                var issuer = _configuration["Jwt:Issuer"];
                var audience = _configuration["Jwt:Audience"];

                if (string.IsNullOrEmpty(secretKey))
                {
                    throw new InvalidOperationException("JWT SecretKey no configurado");
                }

                var key = Encoding.ASCII.GetBytes(secretKey);

                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new[]
                    {
                        new Claim("sub", user.Id),
                        new Claim("email", user.Email),
                        new Claim("name", user.Fullname),
                        new Claim("role", user.Role)
                    }),
                    Expires = DateTime.UtcNow.AddHours(24),
                    Issuer = issuer,
                    Audience = audience,
                    SigningCredentials = new SigningCredentials(
                        new SymmetricSecurityKey(key),
                        SecurityAlgorithms.HmacSha256Signature)
                };

                var tokenHandler = new JwtSecurityTokenHandler();
                var token = tokenHandler.CreateToken(tokenDescriptor);

                return tokenHandler.WriteToken(token);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al generar token: {ex.Message}");
                throw;
            }
        
        }

        public async Task<User?> GetUserById(string userId)
        {
            try
            {
                var usersCollection = _firebaseService.GetCollection("users");
                var doc = await usersCollection.Document(userId).GetSnapshotAsync();

                if (!doc.Exists)
                {
                    return null;
                }

                var userDict = doc.ToDictionary();

                var user = new User
                {
                    Id = userDict["Id"].ToString(),
                    Email = userDict["Email"].ToString(),
                    Fullname = userDict["Fullname"].ToString(),
                    Role = userDict["Role"].ToString(),
                    ProfilePictureUrl = userDict["ProfilePictureUrl"].ToString(),
                    CreatedAt = ((Timestamp)userDict["CreatedAt"]).ToDateTime(),
                    LastLogin = ((Timestamp)userDict["LastLogin"]).ToDateTime(),
                    IsActive = (bool)userDict["IsActive"],
                    HasReserved = (bool)userDict["HasReserved"],
                    ReservationTimestamp = userDict.ContainsKey("ReservationTimestamp") ? ((Timestamp)userDict["ReservationTimestamp"]).ToDateTime() : DateTime.MinValue
                };

                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al obtener usuario: {ex.Message}");
                return null;
            }
        }

        public async Task<List<User>?> GetAllGuests()
        {
            try
            {
                var usersCollection = _firebaseService.GetCollection("users");
                var query = usersCollection.WhereEqualTo("Role", "huesped");
                var snapshot = await query.GetSnapshotAsync();

                var guests = new List<User>();

                foreach (var doc in snapshot.Documents)
                {
                    var userDict = doc.ToDictionary();
                    var guest = new User
                    {
                        Id = userDict["Id"].ToString(),
                        Email = userDict["Email"].ToString(),
                        Fullname = userDict["Fullname"].ToString(),
                        Role = userDict["Role"].ToString(),
                        ProfilePictureUrl = userDict["ProfilePictureUrl"].ToString(),
                        CreatedAt = ((Timestamp)userDict["CreatedAt"]).ToDateTime(),
                        LastLogin = ((Timestamp)userDict["LastLogin"]).ToDateTime(),
                        IsActive = (bool)userDict["IsActive"],
                        HasReserved = (bool)userDict["HasReserved"],
                        ReservationTimestamp = userDict.ContainsKey("ReservationTimestamp") ? ((Timestamp)userDict["ReservationTimestamp"]).ToDateTime() : DateTime.MinValue
                    };
                    guests.Add(guest);
                }

                return guests;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al obtener huéspedes: {ex.Message}");
                return null;
            }
        }

    }
}