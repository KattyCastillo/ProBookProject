using G6.ProBook.WebApi.DTOs;
using G6.ProBook.WebApi.Models;

namespace G6.ProBook.WebApi.Services
{
    public interface IRoomService
    {
        Task<List<RoomDto>> GetAllRooms(string? roomType = null);

        Task<RoomDto?> GetRoomById (string roomId);

        Task<Room> CreatRoom (Room room, string adminId);

        Task<Room> UpdateRoom (string  roomId, Room room, string adminId);

        Task DeleteRoom(string roomId, string adminId);

        Task<List<RoomDto>> SearchRooms(string searchTerm);
    }
}
