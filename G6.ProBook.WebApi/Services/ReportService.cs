using G6.ProBook.WebApi.DTOs;
using G6.ProBook.WebApi.Models;
using Google.Cloud.Firestore;

namespace G6.ProBook.WebApi.Services
{
    public class ReportService : IReportService
    {
        private readonly FirebaseService _firebaseService;
        private readonly ILogger<ReportService> _logger;
        private readonly IRoomService _roomService;
        private readonly IReservationService _reservationService;
        private readonly IAuthService _authService;

        public ReportService(
            FirebaseService firebaseService,
            ILogger<ReportService> logger,
            IRoomService roomService,
            IReservationService reservationService,
            IAuthService authService)
        {
            _firebaseService = firebaseService;
            _logger = logger;
            _roomService = roomService;
            _reservationService = reservationService;
            _authService = authService;
        }

        public async Task<DashboardStatsDto> GetDashboardStats()
        {
            try
            {
                var totalRooms = await GetTotalRooms();
                var totalNights = await GetTotalNightsBooked();
                var occupancyPercentage = await GetOccupancyPercentage();
                var totalRevenue = await GetTotalRevenue();
                var allReservations = await _reservationService.GetAllReservations() ?? new List<ReservationDto>();
                var activeReservations = allReservations.Count(r => r.CheckOutDate >= DateTime.UtcNow);

                return new DashboardStatsDto
                {
                    TotalRooms = totalRooms,
                    TotalNightsBooked = totalNights,
                    OccupancyPercentage = occupancyPercentage,
                    TotalRevenue = totalRevenue,
                    TotalReservations = allReservations.Count,
                    ActiveReservations = activeReservations
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al obtener estadísticas del dashboard: {ex.Message}");
                throw;
            }
        }

        public async Task<decimal> GetTotalRevenue()
        {
            try
            {
                var reservations = await _reservationService.GetAllReservations() ?? new List<ReservationDto>();
                return (decimal)reservations.Sum(r => r.TotalCost);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al calcular ingresos totales: {ex.Message}");
                throw;
            }
        }

        public async Task<double> GetOccupancyPercentage()
        {
            try
            {
                var totalRooms = await GetTotalRooms();
                if (totalRooms == 0) return 0;

                var reservations = await _reservationService.GetAllReservations() ?? new List<ReservationDto>();
                var currentDate = DateTime.UtcNow;

                // Contar habitaciones actualmente ocupadas
                var occupiedRooms = reservations
                    .Where(r => r.CheckInDate <= currentDate && r.CheckOutDate > currentDate)
                    .Select(r => r.RoomID)
                    .Distinct()
                    .Count();

                return (double)occupiedRooms / totalRooms * 100;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al calcular porcentaje de ocupación: {ex.Message}");
                throw;
            }
        }

        public async Task<List<RoomTypeReportDto>> GetOccupancyByRoomType()
        {
            try
            {
                var allRooms = await _roomService.GetAllRooms();
                var reservations = await _reservationService.GetAllReservations() ?? new List<ReservationDto>();
                var currentDate = DateTime.UtcNow;

                var roomsByType = allRooms.GroupBy(r => r.Type);
                var result = new List<RoomTypeReportDto>();

                foreach (var typeGroup in roomsByType)
                {
                    var roomIds = typeGroup.Select(r => r.Id).ToList();
                    var reservationsForType = reservations.Where(r => roomIds.Contains(r.RoomID)).ToList();
                    var occupiedRooms = reservationsForType
                        .Where(r => r.CheckInDate <= currentDate && r.CheckOutDate > currentDate)
                        .Select(r => r.RoomID)
                        .Distinct()
                        .Count();

                    var occupancyPercentage = typeGroup.Count() > 0
                        ? (double)occupiedRooms / typeGroup.Count() * 100
                        : 0;

                    result.Add(new RoomTypeReportDto
                    {
                        RoomType = typeGroup.Key,
                        TotalRooms = typeGroup.Count(),
                        ReservedRooms = occupiedRooms,
                        OccupancyPercentage = occupancyPercentage,
                        ReservationCount = reservationsForType.Count
                    });
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al obtener ocupación por tipo de habitación: {ex.Message}");
                throw;
            }
        }

        public async Task<List<OccupancyTrendDto>> GetOccupancyTrend(int days = 30)
        {
            try
            {
                var reservations = await _reservationService.GetAllReservations() ?? new List<ReservationDto>();
                var allRooms = await _roomService.GetAllRooms();
                var totalRooms = allRooms.Count;

                if (totalRooms == 0) return new List<OccupancyTrendDto>();

                var result = new List<OccupancyTrendDto>();
                var currentDate = DateTime.UtcNow;

                for (int i = days - 1; i >= 0; i--)
                {
                    var date = currentDate.AddDays(-i).Date;

                    var occupiedRooms = reservations
                        .Where(r => r.CheckInDate.Date <= date && r.CheckOutDate.Date > date)
                        .Select(r => r.RoomID)
                        .Distinct()
                        .Count();

                    var occupancyPercentage = (double)occupiedRooms / totalRooms * 100;

                    var reservationsCount = reservations
                        .Where(r => r.CheckInDate.Date <= date && r.CheckOutDate.Date > date)
                        .Count();

                    result.Add(new OccupancyTrendDto
                    {
                        Date = date,
                        OccupancyPercentage = occupancyPercentage,
                        ReservationsCount = reservationsCount
                    });
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al obtener tendencia de ocupación: {ex.Message}");
                throw;
            }
        }

        public async Task<List<ReservationReportDto>> GetReservationDetailsTable()
        {
            try
            {
                var reservations = await _reservationService.GetAllReservations() ?? new List<ReservationDto>();
                var result = new List<ReservationReportDto>();

                foreach (var reservation in reservations)
                {
                    var user = await _authService.GetUserById(reservation.UserID);

                    result.Add(new ReservationReportDto
                    {
                        ReservationId = reservation.Id,
                        GuestName = user?.Fullname ?? reservation.UserName,
                        GuestEmail = user?.Email ?? string.Empty,
                        RoomNumber = reservation.RoomNumber,
                        RoomType = reservation.RoomType,
                        CheckInDate = reservation.CheckInDate,
                        CheckOutDate = reservation.CheckOutDate,
                        Nights = reservation.Nights,
                        TotalCost = (decimal)reservation.TotalCost,
                        ReservationDate = reservation.Timestamp
                    });
                }

                return result.OrderByDescending(r => r.ReservationDate).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al obtener tabla de detalles de reservas: {ex.Message}");
                throw;
            }
        }

        public async Task<List<RevenueByRoomTypeDto>> GetRevenueByRoomType()
        {
            try
            {
                var reservations = await _reservationService.GetAllReservations() ?? new List<ReservationDto>();
                var groupedByType = reservations.GroupBy(r => r.RoomType);

                var result = new List<RevenueByRoomTypeDto>();

                foreach (var typeGroup in groupedByType)
                {
                    var totalRevenue = typeGroup.Sum(r => (decimal)r.TotalCost);
                    var count = typeGroup.Count();
                    var totalNights = typeGroup.Sum(r => r.Nights);

                    result.Add(new RevenueByRoomTypeDto
                    {
                        RoomType = typeGroup.Key,
                        ReservationCount = count,
                        TotalNights = totalNights,
                        TotalRevenue = totalRevenue,
                        AverageRevenuePerReservation = count > 0 ? totalRevenue / count : 0
                    });
                }

                return result.OrderByDescending(r => r.TotalRevenue).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al obtener ingresos por tipo de habitación: {ex.Message}");
                throw;
            }
        }

        public async Task<long> GetTotalNightsBooked()
        {
            try
            {
                var reservations = await _reservationService.GetAllReservations() ?? new List<ReservationDto>();
                return reservations.Sum(r => r.Nights);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al calcular total de noches: {ex.Message}");
                throw;
            }
        }

        private async Task<int> GetTotalRooms()
        {
            try
            {
                var rooms = await _roomService.GetAllRooms();
                return rooms.Count;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al obtener total de habitaciones: {ex.Message}");
                throw;
            }
        }
    }
}
