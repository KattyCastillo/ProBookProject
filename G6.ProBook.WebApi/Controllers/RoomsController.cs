using G6.ProBook.WebApi.Models;
using G6.ProBook.WebApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace G6.ProBook.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RoomsController : ControllerBase
    {
        private readonly IRoomService _roomService;
        private readonly IAuthService _authService;
        private readonly ILogger<RoomsController> _logger;

        //Recibe IRoomService inyectado
        public RoomsController(IRoomService roomService, IAuthService authService, ILogger<RoomsController> logger)
        {
            _roomService = roomService;
            _authService = authService;
            _logger = logger;
        }

        /// <summary>
        /// GET /api/rooms
        /// Obtiene todas las habitaciones con filtro opcional por Tipo de Habitación
        /// Acceso: Público (sin autenticación)
        /// Parámetros query (opcional):
        /// - type: Filtrar por tipo de habitación específica.
        /// Ejemplo: GET /api/rooms?type=Duplex
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAllRooms([FromQuery] string? type = null)
        {
            try
            {
                var rooms = await _roomService.GetAllRooms(type);
                return Ok(rooms);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al obtener habitaciones: {ex.Message}");
                return StatusCode(500, new { message = "Error al obtener habitaciones" });
            }
        }

        /// <summary>
        /// GET /api/rooms/{roomId}
        /// Obtiene una habitación específica por su ID
        /// Acceso: Público (sin autenticación)
        /// Parámetro:
        /// - roomId: ID de la habitación
        /// </summary>
        [HttpGet("{roomId}")]
        public async Task<IActionResult> GetRoomById(string roomId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(roomId))
                {
                    return BadRequest(new { message = "El ID de la habitación es requerido" });
                }

                var room = await _roomService.GetRoomById(roomId);

                if (room == null)
                {
                    return NotFound(new { message = "Habitación no encontrada" });
                }
                return Ok(room);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al obtener habitación: {ex.Message}");
                return StatusCode(500, new { message = "Error al obtener habitación" });
            }
        }

        /// <summary>
        /// POST /api/rooms
        /// Crea una nueva habitación (Solo managers)
        /// Requiere autenticación y rol "manager"
        /// Header requerido:
        /// - Authorization: Bearer {token}
        /// Cuerpo esperado (JSON):
        /// {
        ///   "number": "302-A",
        ///   "type": "Suite Presidencial",
        ///   "capacity": 4,
        ///   "amenities": ["Wi-Fi", "Minibar", "Jacuzzi"],
        ///   "basePrice": 150.50,
        ///   "photoUrl": "https://..."
        /// }
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "manager")]
        public async Task<IActionResult> CreateRoom([FromBody] Room room)
        {
            try
            {
                if (room == null)
                {
                    return BadRequest(new { message = "El cuerpo de la petición es requerido" });
                }

                if (string.IsNullOrWhiteSpace(room.Number))
                {
                    return BadRequest(new { message = "El número de habitación es requerido" });
                }

                if (room.Capacity <= 0)
                {
                    return BadRequest(new { message = "La capacidad debe ser mayor a 0" });
                }

                if (room.BasePrice <= 0)
                {
                    return BadRequest(new { message = "El precio base debe ser mayor a 0" });
                }

                // Extraer ID del manager desde el JWT
                var managerId = User.FindFirst("sub")?.Value ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(managerId))
                {
                    return StatusCode(403, new { message = "No se pudo identificar al usuario" });
                }

                var createdRoom = await _roomService.CreateRoom(room, managerId);

                _logger.LogInformation($"Habitación creada: {createdRoom.Number} por manager: {managerId}");

                return Created($"/api/rooms/{createdRoom.Id}", createdRoom);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al crear la habitación: {ex.Message}");
                return StatusCode(500, new { message = "Error al crear la habitación" });
            }
        }

        /// <summary>
        /// PUT /api/rooms/{roomId}
        /// Edita una habitación existente (Solo managers)
        /// Requiere autenticación y rol "manager"
        /// Header requerido:
        /// - Authorization: Bearer {token}
        /// Parámetro:
        /// - roomId: ID de la habitación a editar
        /// Cuerpo: Mismo formato que CreateRoom
        /// </summary>
        [HttpPut("{roomId}")]
        [Authorize(Roles = "manager")]
        public async Task<IActionResult> UpdateRoom(string roomId, [FromBody] Room room)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(roomId))
                {
                    return BadRequest(new { message = "El ID de la habitación es requerido" });
                }

                if (room == null)
                {
                    return BadRequest(new { message = "El cuerpo de la petición es requerido" });
                }

                // Extraer ID del manager desde el JWT
                var managerId = User.FindFirst("sub")?.Value ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(managerId))
                {
                    return StatusCode(403, new { message = "No se pudo identificar al usuario" });
                }

                var updatedRoom = await _roomService.UpdateRoom(roomId, room, managerId);

                _logger.LogInformation($"Habitación actualizada: {roomId} por manager: {managerId}");

                return Ok(updatedRoom);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al actualizar habitación: {ex.Message}");
                return StatusCode(500, new { message = "Error al actualizar habitación" });
            }
        }

        /// <summary>
        /// DELETE /api/rooms/{roomId}
        /// Elimina una habitación (Solo managers)
        /// Requiere autenticación y rol "manager"
        /// Header requerido:
        /// - Authorization: Bearer {token}
        /// Parámetro:
        /// - roomId: ID de la habitación a eliminar
        /// Respuesta exitosa (204): No content
        /// </summary>
        [HttpDelete("{roomId}")]
        [Authorize(Roles = "manager")]
        public async Task<IActionResult> DeleteRoom(string roomId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(roomId))
                {
                    return BadRequest(new { message = "El ID de la habitación es requerido" });
                }

                // Extraer ID del manager desde el JWT
                var managerId = User.FindFirst("sub")?.Value ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(managerId))
                {
                    return StatusCode(403, new { message = "No se pudo identificar al usuario" });
                }

                await _roomService.DeleteRoom(roomId, managerId);

                _logger.LogInformation($"Habitación eliminada: {roomId} por manager: {managerId}");

                return NoContent(); // 204
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al eliminar habitación: {ex.Message}");
                return StatusCode(500, new { message = "Error al eliminar habitación" });
            }
        }

        /// <summary>
        /// GET /api/rooms/search/{searchTerm}
        /// Busca habitaciones por número
        /// Acceso: Público (sin autenticación)
        /// Parámetro:
        /// - searchTerm: Término a buscar
        /// Ejemplo: GET /api/rooms/search/B-17
        /// </summary>
        [HttpGet("search/{searchTerm}")]
        public async Task<IActionResult> SearchRoom(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                {
                    return BadRequest(new { message = "El término de búsqueda es requerido" });
                }

                var results = await _roomService.SearchRoom(searchTerm);
                return Ok(results);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al buscar habitación: {ex.Message}");
                return StatusCode(500, new { message = "Error al buscar habitación" });
            }
        }
    }
}
