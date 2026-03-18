using G6.ProBook.WebApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace G6.ProBook.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "manager")]
    public class ReportsController : ControllerBase
    {
        private readonly IReportService _reportService;
        private readonly ILogger<ReportsController> _logger;

        public ReportsController(IReportService reportService, ILogger<ReportsController> logger)
        {
            _reportService = reportService;
            _logger = logger;
        }

        /// <summary>
        /// GET /api/reports/dashboard
        /// Obtiene estadísticas generales del dashboard
        /// Incluye: total de habitaciones, noches reservadas, porcentaje de ocupación, ingresos
        /// </summary>
        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboardStats()
        {
            try
            {
                var stats = await _reportService.GetDashboardStats();
                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al obtener estadísticas del dashboard: {ex.Message}");
                return StatusCode(500, new { message = "Error al obtener estadísticas del dashboard" });
            }
        }

        /// <summary>
        /// GET /api/reports/revenue
        /// Obtiene el total de ingresos generados
        /// </summary>
        [HttpGet("revenue")]
        public async Task<IActionResult> GetTotalRevenue()
        {
            try
            {
                var revenue = await _reportService.GetTotalRevenue();
                return Ok(new { totalRevenue = revenue });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al obtener ingresos totales: {ex.Message}");
                return StatusCode(500, new { message = "Error al obtener ingresos totales" });
            }
        }

        /// <summary>
        /// GET /api/reports/occupancy
        /// Obtiene el porcentaje de ocupación actual del hotel
        /// </summary>
        [HttpGet("occupancy")]
        public async Task<IActionResult> GetOccupancyPercentage()
        {
            try
            {
                var occupancy = await _reportService.GetOccupancyPercentage();
                return Ok(new { occupancyPercentage = occupancy });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al calcular porcentaje de ocupación: {ex.Message}");
                return StatusCode(500, new { message = "Error al calcular porcentaje de ocupación" });
            }
        }

        /// <summary>
        /// GET /api/reports/occupancy-by-room-type
        /// Obtiene datos de ocupación agrupados por tipo de habitación
        /// Retorna: tipo de habitación, cantidad total, cantidad reservada y porcentaje
        /// </summary>
        [HttpGet("occupancy-by-room-type")]
        public async Task<IActionResult> GetOccupancyByRoomType()
        {
            try
            {
                var data = await _reportService.GetOccupancyByRoomType();
                return Ok(data);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al obtener ocupación por tipo: {ex.Message}");
                return StatusCode(500, new { message = "Error al obtener ocupación por tipo de habitación" });
            }
        }

        /// <summary>
        /// GET /api/reports/occupancy-trend
        /// Obtiene tendencia de ocupación en un período temporal
        /// Parámetro query (opcional): days = número de días a retroceder (default: 30)
        /// </summary>
        [HttpGet("occupancy-trend")]
        public async Task<IActionResult> GetOccupancyTrend([FromQuery] int days = 30)
        {
            try
            {
                if (days < 1 || days > 365)
                {
                    return BadRequest(new { message = "El número de días debe estar entre 1 y 365" });
                }

                var trend = await _reportService.GetOccupancyTrend(days);
                return Ok(trend);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al obtener tendencia de ocupación: {ex.Message}");
                return StatusCode(500, new { message = "Error al obtener tendencia de ocupación" });
            }
        }

        /// <summary>
        /// GET /api/reports/reservation-details
        /// Obtiene tabla detallada de todas las reservas
        /// Incluye: información de huésped, habitación, fechas e ingresos
        /// </summary>
        [HttpGet("reservation-details")]
        public async Task<IActionResult> GetReservationDetailsTable()
        {
            try
            {
                var details = await _reportService.GetReservationDetailsTable();
                return Ok(details);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al obtener tabla de detalles: {ex.Message}");
                return StatusCode(500, new { message = "Error al obtener tabla de detalles de reservas" });
            }
        }

        /// <summary>
        /// GET /api/reports/revenue-by-room-type
        /// Obtiene estadísticas de ingresos por tipo de habitación
        /// Incluye: cantidad de reservas, noches totales, ingresos totales y promedio
        /// </summary>
        [HttpGet("revenue-by-room-type")]
        public async Task<IActionResult> GetRevenueByRoomType()
        {
            try
            {
                var revenue = await _reportService.GetRevenueByRoomType();
                return Ok(revenue);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al obtener ingresos por tipo: {ex.Message}");
                return StatusCode(500, new { message = "Error al obtener ingresos por tipo de habitación" });
            }
        }

        /// <summary>
        /// GET /api/reports/total-nights
        /// Obtiene el total de noches reservadas
        /// </summary>
        [HttpGet("total-nights")]
        public async Task<IActionResult> GetTotalNightsBooked()
        {
            try
            {
                var nights = await _reportService.GetTotalNightsBooked();
                return Ok(new { totalNightsBooked = nights });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al calcular total de noches: {ex.Message}");
                return StatusCode(500, new { message = "Error al calcular total de noches reservadas" });
            }
        }
    }
}
