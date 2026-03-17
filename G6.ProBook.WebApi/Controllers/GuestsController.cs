using Microsoft.AspNetCore.Mvc;
using G6.ProBook.WebApi.Models;     
using G6.ProBook.WebApi.Services;   
using G6.ProBook.WebApi.DTOs;       
using Google.Cloud.Firestore;

namespace G6.ProBook.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GuestsController : ControllerBase
    {
        private readonly CollectionReference _guestsCollection;
        private readonly ILogger<GuestsController> _logger;

        
        public GuestsController(FirebaseService firebaseService, ILogger<GuestsController> logger)
        {
            _logger = logger;
            
            _guestsCollection = firebaseService.GetCollection("Guests");
        }

        // 1. Crear un nuevo huésped
        [HttpPost]
        public async Task<IActionResult> Crear([FromBody] GuestDto.Create dto)
        {
            try
            {
                
                var nuevoHuesped = new Guest
                {
                    Nombre = dto.Nombre,
                    Email = dto.Email,
                    Identidad = dto.Identidad,
                    Telefono = dto.Telefono
                };

                
                DocumentReference docRef = await _guestsCollection.AddAsync(nuevoHuesped);

                return Ok(new { id = docRef.Id, mensaje = "Huésped registrado con éxito" });
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error al crear huésped");
                return BadRequest("No se pudo completar el registro.");
            }
        }

        // 2. Buscar huésped por su número de identidad
        [HttpGet("buscar/{identidad}")]
        public async Task<IActionResult> ObtenerPorIdentidad(string identidad)
        {
            try
            {
                // Hacemos la consulta a Firebase
                Query query = _guestsCollection.WhereEqualTo("Identidad", identidad);
                QuerySnapshot snapshot = await query.GetSnapshotAsync();

                if (snapshot.Documents.Count == 0)
                {
                    return NotFound("No existe un huésped con esa identidad.");
                }

                
                var doc = snapshot.Documents.First();
                var modelo = doc.ConvertTo<Guest>();

                var respuesta = new GuestDto.Read
                {
                    Id = doc.Id,
                    Nombre = modelo.Nombre,
                    Identidad = modelo.Identidad
                };

                return Ok(respuesta);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error al buscar");
                return StatusCode(500, "Error interno al consultar Firebase");
            }
        }
    }
}