using G6.ProBook.WebApi.Models;
using G6.ProBook.WebApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace G6.ProBook.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RoomsController : ControllerBase
    {
        private readonly IRoomService _roomService;
        private readonly ILogger<RoomsController> _logger;

        //Recibe IRoomService inyectado
        public RoomsController(IRoomService roomService, ILogger<RoomsController> logger)
        {
            _roomService = roomService;
            _logger = logger;
        }

        //GET /api/room
        //Obtiene todas las habitaciones con filtro opcional por Tipo de Habitación
        //Parámetros query (opcional)
        //type: Filtrar por tipo de habitación específica.
        //Ejemplo: GET /api/room?type=Duplex
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
        /*
         * GET /api/room/{roomID}
         * Obtiene una habitación específica por su ID
         * Parámetro:
         * roomID: ID de la película
         */
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
        /*
         * POST /api/room
         * Crea una nueva habitación (Sólo el administrador)
         * Header requerido:
         * Authorization: Bearer {token}
         * Cuerpo esperado (JSON):
         * {
                "number": "302-A",
                "type": "Suite Presidencial",
                "capacity": 4,
                "amenities": [
                "Wi-Fi de alta velocidad",
                "Minibar premium",
                "Vista al mar",
                "Jacuzzi"
                ],
                "basePrice": 150.50,
                "photoUrl": "https..."
            }
         */
        [HttpPost]
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

                // TODO: Validar que el usuario es admin (requiere token parsing)
                var adminId = "admin_123"; // Por ahora, hardcodeado

                var createdRoom = await _roomService.CreateRoom(room, adminId);

                _logger.LogInformation($"Habitación creada: {createdRoom.Number}");

                return Created($"/api/room/{createdRoom.Id}", createdRoom);
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
        /*
         * PUT /api/room/{roomId}
         * Edita una habitación existente (únicamente el administrador)
         * Header requerido:
         * Authorization: Bearer {token}
         * Parámetro:
         * roomId: ID de la película a editar
         * Cuerpo: Mismo formato que CreateRoom
         * Respuesta exitosa (200): La habitación actualizada
         * Errores:
         * 404: Habitación no encontrada
         * 401: No autenticado
         * 403: No es administrador
         */
        [HttpPut("{roomId}")]
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

                // TODO: Validar que el usuario es admin
                var adminId = "admin_123"; // Por ahora, hardcodeado

                var updatedRoom = await _roomService.UpdateRoom(roomId, room, adminId);

                _logger.LogInformation($"Habitación actualizada: {roomId}");

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
        /// DELETE /api/room/{roomId}
        /// Elimina una habitación (SOLO ADMIN)
        /// Header requerido:
        ///   Authorization: Bearer {token}
        /// Parámetro:
        ///   roomId: ID de la habitación a eliminar
        /// 
        /// Respuesta exitosa (204): No content
        /// 
        /// Errores:
        /// 404: Habitación no encontrada
        /// 400: Habitación tiene reservaciones
        /// 401: No autenticado
        /// 403: No es administrador
        /// </summary>
        [HttpDelete("{roomId}")]
        public async Task<IActionResult> DeleteRoom(string roomId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(roomId))
                {
                    return BadRequest(new { message = "El ID de la habitación es requerido" });
                }

                // TODO: Validar que el usuario es admin
                var adminId = "admin_123"; // Por ahora, hardcodeado

                await _roomService.DeleteRoom(roomId, adminId);

                _logger.LogInformation($"Habitación eliminada: {roomId}");

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
        /// GET /api/room/search/{searchTerm}
        /// 
        /// Busca habitación por número
        /// 
        /// Parámetro:
        ///   searchTerm: Lo que el usuario busca
        ///   Ejemplo: GET /api/room/search/B-17
        /// 
        /// Respuesta exitosa (200):
        /// [
        ///   {
        ///     "id": "room_001",
        ///     "number": "B-17",
        ///     ...
        ///   }
        /// ]
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
