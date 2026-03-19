using G6.ProBook.WebApi.DTOs;
using G6.ProBook.WebApi.Models;
using G6.ProBook.WebApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace G6.ProBook.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ReservationsController : ControllerBase
    {
        private readonly ILogger<RoomsController> _logger;
        private readonly IReservationService _reservationService;

        public ReservationsController(
            ILogger<RoomsController> logger, 
            IReservationService reservationService)
        {
            _logger = logger;
            _reservationService = reservationService;
        }

        /// <summary>
        /// GET /api/reservations
        /// Obtiene todas las reservaciones (solo para managers)
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "manager")]
        public async Task<IActionResult> GetAllReservations()
        {
            try
            {
                var reservations = await _reservationService.GetAllReservations();
                return Ok(reservations);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al obtener reservaciones: {ex.Message}");
                return StatusCode(500, new { message = "Error al obtener reservaciones" });
            }
        }

        /// <summary>
        /// POST /api/reservations
        /// Crea una nueva reservación (solo para guests/huéspedes)
        /// El usuario solo puede crear reservación para su propio ID
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "huesped")]
        public async Task<IActionResult> CreateReservation([FromBody] CreateReservationDto createReservationDto)
        {
            try
            {
                if (createReservationDto == null)
                {
                    return BadRequest(new { message = "El cuerpo de la petición es requerido" });
                }

                if (string.IsNullOrWhiteSpace(createReservationDto.UserID))
                {
                    return BadRequest(new { message = "El ID de usuario es requerido" });
                }

                if (string.IsNullOrWhiteSpace(createReservationDto.RoomID))
                {
                    return BadRequest(new { message = "El ID de habitacion es requerido" });
                }

                if (createReservationDto.CheckInDate == DateTime.MinValue)
                {
                    return BadRequest(new { message = "Fecha de check in es requerida" });
                }

                if (createReservationDto.CheckOutDate == DateTime.MinValue)
                {
                    return BadRequest(new { message = "Fecha de check out es requerida" });
                }

                // Validar que el usuario solo cree reservaciones para sí mismo
                var userIdFromToken = User.FindFirst("sub")?.Value ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdFromToken) || userIdFromToken != createReservationDto.UserID)
                {
                    return StatusCode(403, new { message = "No puedes crear reservaciones para otros usuarios" });
                }

                var createdReservation = await _reservationService.CreateReservation(createReservationDto);

                _logger.LogInformation($"Reservacion creada por usuario: {createReservationDto.UserID}");

                return Created($"/api/reservations/{createdReservation.Id}", createdReservation);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al crear reservacion: {ex.Message}");
                return StatusCode(500, new { message = "Error al crear reservacion" });
            }
        }

        /// <summary>
        /// GET /api/reservations/{reservationId}
        /// Obtiene una reservación específica
        /// Huéspedes solo pueden ver sus propias reservaciones
        /// Managers pueden ver todas
        /// </summary>
        [HttpGet("{reservationId}")]
        public async Task<IActionResult> GetReservationById(string reservationId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(reservationId))
                {
                    return BadRequest(new { message = "El ID de la reservacion es requerido" });
                }

                var reservacion = await _reservationService.GetReservationById(reservationId);

                if (reservacion == null)
                {
                    return NotFound(new { message = "Reservacion no encontrada" });
                }

                // Validar acceso: Huéspedes solo ven sus propias reservaciones
                var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
                var userIdFromToken = User.FindFirst("sub")?.Value ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

                if (userRole == "huesped" && reservacion.UserID != userIdFromToken)
                {
                    return StatusCode(403, new { message = "No tienes permiso para ver esta reservación" });
                }

                return Ok(reservacion);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al obtener la reservacion: {ex.Message}");
                return StatusCode(500, new { message = "Error al obtener la reservacion" });
            }
        }

        /// <summary>
        /// GET /api/reservations/room/{roomId}
        /// Obtiene reservaciones de una habitación específica
        /// Solo accesible para managers
        /// </summary>
        [HttpGet("room/{roomId}")]
        [Authorize(Roles = "manager")]
        public async Task<IActionResult> GetReservationsByRoomId(string roomId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(roomId))
                {
                    return BadRequest(new { message = "El ID de la habitacion es requerido" });
                }

                var reservaciones = await _reservationService.GetReservationsByRoomId(roomId);
                return Ok(reservaciones);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al obtener reservaciones: {ex.Message}");
                return StatusCode(500, new { message = "Error al obtener reservaciones" });
            }
        }

        /// <summary>
        /// GET /api/reservations/user/{userId}
        /// Obtiene reservaciones de un usuario específico
        /// Huéspedes solo ven sus propias reservaciones
        /// Managers pueden ver todas
        /// </summary>
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetReservationsByUserId(string userId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userId))
                {
                    return BadRequest(new { message = "El ID de usuario es requerido" });
                }

                // Validar acceso: Huéspedes solo ven sus propias reservaciones
                var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
                var userIdFromToken = User.FindFirst("sub")?.Value ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

                if (userRole == "huesped" && userId != userIdFromToken)
                {
                    return StatusCode(403, new { message = "No tienes permiso para ver las reservaciones de otros usuarios" });
                }

                var reservaciones = await _reservationService.GetReservationsByUserId(userId);
                return Ok(reservaciones);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al obtener reservaciones: {ex.Message}");
                return StatusCode(500, new { message = "Error al obtener reservaciones" });
            }
        }

        /// <summary>
        /// GET /api/reservations/available
        /// Obtiene habitaciones disponibles para un rango de fechas
        /// Accesible para guests para buscar disponibilidad
        /// </summary>
        [HttpGet("available")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAvailableRooms([FromQuery] DateTime checkInDate, [FromQuery] DateTime checkOutDate)
        {
            try
            {
                if (checkInDate == DateTime.MinValue || checkOutDate == DateTime.MinValue)
                {
                    return BadRequest(new { message = "Las fechas de check-in y check-out son requeridas" });
                }

                if (checkInDate >= checkOutDate)
                {
                    return BadRequest(new { message = "La fecha de check-out debe ser posterior a la de check-in" });
                }

                var rooms = await _reservationService.GetAvailableRooms(checkInDate, checkOutDate);
                return Ok(rooms);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al obtener habitaciones disponibles: {ex.Message}");
                return StatusCode(500, new { message = "Error al obtener habitaciones disponibles" });
            }
        }
    }
}
