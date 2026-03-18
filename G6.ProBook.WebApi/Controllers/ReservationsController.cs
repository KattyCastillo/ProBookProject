using G6.ProBook.WebApi.DTOs;
using G6.ProBook.WebApi.Models;
using G6.ProBook.WebApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace G6.ProBook.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReservationsController : ControllerBase
    {
        private readonly ILogger<RoomsController> _logger;
        private readonly IReservationService _reservationService;

        public ReservationsController(ILogger<RoomsController> logger, IReservationService reservationService)
        {
            _logger = logger;
            _reservationService = reservationService;
        }

        [HttpGet]
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

        [HttpPost]
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


                var createdReservation = await _reservationService.CreateReservation(createReservationDto);

                _logger.LogInformation($"Reservacion creada");

                return Created($"/api/reservation/{createdReservation.Id}", createdReservation);
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

                return Ok(reservacion);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al obtener la reservacion: {ex.Message}");
                return StatusCode(500, new { message = "Error al obtener la reservacion" });
            }
        }

        [HttpGet("room/{roomId}")]
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

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetReservationsByUserId(string userId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userId))
                {
                    return BadRequest(new { message = "El ID de usuario es requerido" });
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

        [HttpGet("available")]
        public async Task<IActionResult> GetAvailableRooms(DateTime checkInDate, DateTime checkOutDate)
        {
            try
            {
                var rooms = await _reservationService.GetAvailableRooms(checkInDate, checkOutDate);
                return Ok(rooms);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al obtener reservaciones: {ex.Message}");
                return StatusCode(500, new { message = "Error al obtener reservaciones" });
            }
        }
    }
}
