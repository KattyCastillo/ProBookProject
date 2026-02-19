using FirebaseAdmin.Auth;
using G6.ProBook.WebApi.Models;
using G6.ProBook.WebApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace G6.ProBook.WebApi.Controllers
{
    [ApiController]
    [Route(template: "api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly FirebaseService _firebaseService;
        private readonly ILogger _logger;
        public AuthController(FirebaseService firebaseService, ILogger<TestController> logger)
        {
            _firebaseService = firebaseService;
            _logger = logger;
        }

        [HttpPost("signup")]
        public async Task<IActionResult> CreateUser(string correo, string password, string nombre)
        {
            try
            {
                _logger.LogInformation("Creando usuario...");

                var userRecord = await FirebaseAuth.DefaultInstance.CreateUserAsync(
                new UserRecordArgs
                {
                    Email = correo,
                    Password = password,
                    DisplayName = nombre
                });

                var uid = userRecord.Uid;

                //Crear usuario en firestore
                var newUser = new User
                {
                    Id = uid,
                    Email = correo,
                    Nombre = nombre,
                    FechaCreacion = DateTime.UtcNow,
                    hasReserved = false,
                    Rol = "huesped"
                };

                await _firebaseService.AgregarUserFirestore(newUser);

                return Ok(new
                {
                    success = true,
                    message = "Usuario creado",
                    usuario = newUser,
                    timeStamp = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error en creacion: {ex.Message}");


                return StatusCode(500, new
                {
                    success = false,
                    message = "Error del servidor",
                    erro = ex.Message
                });
            }
        }

    }
}
