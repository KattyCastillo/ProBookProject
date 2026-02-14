using G6.ProBook.WebApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace G6.ProBook.WebApi.Controllers
{
    /// <summary>
    /// Controller de prueba, verificar conexión a Firebase
    /// </summary>
    /// 

    [ApiController]
    [Route(template:"api/[controller]")]
    public class TestController : ControllerBase
    {
        private readonly FirebaseService _firebaseService;
        private readonly ILogger _logger;

        public TestController(FirebaseService firebaseService, ILogger<TestController> logger) { 
            _firebaseService = firebaseService;
            _logger = logger;
        }

        [HttpGet(template:"firebase")]
        public async Task<ActionResult> TestFirebaseConnection() {
            try
            {
                _logger.LogInformation("Iniciando pruab de conexión a Firebase");

                //Obtenemos la colección test
                var testCollection = _firebaseService.GetCollection("test");

                //intentar leer un documento
                var snapshot = await testCollection.Limit(1).GetSnapshotAsync();

                return Ok(new
                {
                    success = true,
                    message = "Conexión Exitosa",
                    documentInTest = snapshot.Count(),
                    timeStamp = DateTime.Now
                }); 
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error en prueba: {ex.Message}");


                return StatusCode(500,new
                {
                    success = false,
                    message = "Error del servidor",
                    erro = ex.Message
                });
            }
        }

        [HttpGet(template: "health")]
        public IActionResult TestFirebaseDatabase() {
            return Ok(new
            {
                message = "API Corriendo",
                timeStamp = DateTime.Now
            });
        }
    }
}
