using G6.ProBook.WebApi.DTOs;
using G6.ProBook.WebApi.Models;
//Creado por Obed Casaña - 62221429

namespace G6.ProBook.WebApi.Services
{
    public interface IRoomService
    {
        //Obtiene las habitaciones según el tipo. 
        Task<List<RoomDto>> GetAllRooms(string? type = null);

        //Obtiene las habitaciones por el ID.
        Task<RoomDto?> GetRoomById(string roomId);
        //Crea una habitación.
        Task<Room> CreateRoom(Room room, string adminId);
        //Actualiza una habitación.
        Task<Room> UpdateRoom(string roomId, Room room, string adminId);
        //Elimina una habitación.
        Task DeleteRoom(string roomId, string adminId);
        //Buscar por número de habitación o amenidades.
        Task<List<RoomDto>> SearchRoom(string searchTerm);
    }
}
