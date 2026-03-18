using G6.ProBook.WebApi.DTOs;
using G6.ProBook.WebApi.Models;

namespace G6.ProBook.WebApi.Services
{
    public interface IReservationService
    {
        Task<Reservation> CreateReservation(CreateReservationDto createReservationDto);

        Task<List<RoomDto>?> GetAvailableRooms(DateTime checkInDate, DateTime checkOutDate);

        Task<ReservationDto?> GetReservationById(string reservationId);

        Task<List<ReservationDto>?> GetReservationsByUserId(string userId);

        Task<List<ReservationDto>?> GetReservationsByRoomId(string roomId);

        Task<List<ReservationDto>?> GetAllReservations();
    }
}
