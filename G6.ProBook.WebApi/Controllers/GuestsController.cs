using G6.ProBook.WebApi.DTOs;
using G6.ProBook.WebApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace G6.ProBook.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "manager")]
    public class GuestsController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IReservationService _reservationService;
        private readonly ILogger<GuestsController> _logger;

        public GuestsController(
            IAuthService authService,
            IReservationService reservationService,
            ILogger<GuestsController> logger)
        {
            _authService = authService;
            _reservationService = reservationService;
            _logger = logger;
        }

        /// <summary>
        /// GET /api/guests
        /// Obtiene lista de todos los huéspedes registrados (solo para managers)
        /// Respuesta incluye información básica del huésped y si tienen reservaciones
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAllGuests()
        {
            try
            {
                var guests = await _authService.GetAllGuests();

                if (guests == null)
                {
                    return Ok(new List<UserDto>());
                }

                var guestDtos = guests.Select(g => new UserDto
                {
                    Id = g.Id,
                    Email = g.Email,
                    FullName = g.Fullname,
                    Role = g.Role,
                    HasReserved = g.HasReserved,
                    ReservationTimestamp = g.ReservationTimestamp
                }).ToList();

                return Ok(guestDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al obtener lista de huéspedes: {ex.Message}");
                return StatusCode(500, new { message = "Error al obtener lista de huéspedes" });
            }
        }

        /// <summary>
        /// GET /api/guests/{guestId}
        /// Obtiene información completa de un huésped específico (solo para managers)
        /// Incluye datos personales y datos de su reservación si existe
        /// </summary>
        [HttpGet("{guestId}")]
        public async Task<IActionResult> GetGuestById(string guestId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(guestId))
                {
                    return BadRequest(new { message = "El ID de huésped es requerido" });
                }

                var guest = await _authService.GetUserById(guestId);

                if (guest == null)
                {
                    return NotFound(new { message = "Huésped no encontrado" });
                }

                // Obtener reservaciones del huésped
                var reservations = await _reservationService.GetReservationsByUserId(guestId) ?? new List<ReservationDto>();

                var guestDto = new UserDto
                {
                    Id = guest.Id,
                    Email = guest.Email,
                    FullName = guest.Fullname,
                    Role = guest.Role,
                    HasReserved = guest.HasReserved,
                    ReservationTimestamp = guest.ReservationTimestamp
                };

                var response = new
                {
                    guest = guestDto,
                    reservations = reservations
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al obtener información del huésped: {ex.Message}");
                return StatusCode(500, new { message = "Error al obtener información del huésped" });
            }
        }

        /// <summary>
        /// GET /api/guests/with-reservations
        /// Obtiene lista de huéspedes que tienen reservaciones activas
        /// </summary>
        [HttpGet("filter/with-reservations")]
        public async Task<IActionResult> GetGuestsWithReservations()
        {
            try
            {
                var allReservations = await _reservationService.GetAllReservations() ?? new List<ReservationDto>();

                if (allReservations.Count == 0)
                {
                    return Ok(new List<object>());
                }

                // Obtener IDs únicos de usuarios con reservaciones
                var guestIds = allReservations.Select(r => r.UserID).Distinct();

                var guestsWithReservations = new List<object>();

                foreach (var guestId in guestIds)
                {
                    var guest = await _authService.GetUserById(guestId);
                    if (guest != null)
                    {
                        var guestReservations = allReservations.Where(r => r.UserID == guestId).ToList();

                        guestsWithReservations.Add(new
                        {
                            guest = new UserDto
                            {
                                Id = guest.Id,
                                Email = guest.Email,
                                FullName = guest.Fullname,
                                Role = guest.Role
                            },
                            reservationCount = guestReservations.Count,
                            latestReservation = guestReservations.OrderByDescending(r => r.Timestamp).FirstOrDefault()
                        });
                    }
                }

                return Ok(guestsWithReservations.OrderByDescending(g => ((dynamic)g).reservationCount));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al obtener huéspedes con reservaciones: {ex.Message}");
                return StatusCode(500, new { message = "Error al obtener huéspedes con reservaciones" });
            }
        }
    }
}

