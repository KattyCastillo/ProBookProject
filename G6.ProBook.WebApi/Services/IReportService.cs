using G6.ProBook.WebApi.DTOs;

namespace G6.ProBook.WebApi.Services
{
    public interface IReportService
    {
        /// <summary>
        /// Obtiene estadísticas generales de reservas y ocupación
        /// Incluye: total de habitaciones, noches reservadas, porcentaje de ocupación, ingresos
        /// </summary>
        Task<DashboardStatsDto> GetDashboardStats();

        /// <summary>
        /// Obtiene el total de ingresos generados por período
        /// </summary>
        Task<decimal> GetTotalRevenue();

        /// <summary>
        /// Obtiene el porcentaje de ocupación actual del hotel
        /// </summary>
        Task<double> GetOccupancyPercentage();

        /// <summary>
        /// Obtiene datos de ocupación agrupados por tipo de habitación
        /// Retorna: tipo de habitación y cantidad de reservas
        /// </summary>
        Task<List<RoomTypeReportDto>> GetOccupancyByRoomType();

        /// <summary>
        /// Obtiene tendencia de ocupación en un período temporal
        /// Retorna: fecha y porcentaje de ocupación para esa fecha
        /// </summary>
        Task<List<OccupancyTrendDto>> GetOccupancyTrend(int days = 30);

        /// <summary>
        /// Obtiene tabla de resultados detallada de todas las reservas
        /// Incluye información de huésped, habitación, fechas e ingresos
        /// </summary>
        Task<List<ReservationReportDto>> GetReservationDetailsTable();

        /// <summary>
        /// Obtiene estadísticas de ingresos por tipo de habitación
        /// </summary>
        Task<List<RevenueByRoomTypeDto>> GetRevenueByRoomType();

        /// <summary>
        /// Obtiene total de noches reservadas
        /// </summary>
        Task<long> GetTotalNightsBooked();
    }
}
