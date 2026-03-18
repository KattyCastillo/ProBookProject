namespace G6.ProBook.WebApi.DTOs
{
    /// <summary>
    /// DTO para estadísticas del dashboard
    /// </summary>
    public class DashboardStatsDto
    {
        public int TotalRooms { get; set; }
        public long TotalNightsBooked { get; set; }
        public double OccupancyPercentage { get; set; }
        public decimal TotalRevenue { get; set; }
        public int TotalReservations { get; set; }
        public int ActiveReservations { get; set; }
    }

    /// <summary>
    /// DTO para ocupación por tipo de habitación
    /// </summary>
    public class RoomTypeReportDto
    {
        public string RoomType { get; set; } = string.Empty;
        public int TotalRooms { get; set; }
        public int ReservedRooms { get; set; }
        public double OccupancyPercentage { get; set; }
        public int ReservationCount { get; set; }
    }

    /// <summary>
    /// DTO para tendencia de ocupación temporal
    /// </summary>
    public class OccupancyTrendDto
    {
        public DateTime Date { get; set; }
        public double OccupancyPercentage { get; set; }
        public int ReservationsCount { get; set; }
    }

    /// <summary>
    /// DTO para tabla detallada de reservas
    /// </summary>
    public class ReservationReportDto
    {
        public string ReservationId { get; set; } = string.Empty;
        public string GuestName { get; set; } = string.Empty;
        public string GuestEmail { get; set; } = string.Empty;
        public string RoomNumber { get; set; } = string.Empty;
        public string RoomType { get; set; } = string.Empty;
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public int Nights { get; set; }
        public decimal TotalCost { get; set; }
        public DateTime ReservationDate { get; set; }
    }

    /// <summary>
    /// DTO para ingresos por tipo de habitación
    /// </summary>
    public class RevenueByRoomTypeDto
    {
        public string RoomType { get; set; } = string.Empty;
        public int ReservationCount { get; set; }
        public long TotalNights { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal AverageRevenuePerReservation { get; set; }
    }
}
