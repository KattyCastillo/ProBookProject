using G6.ProBook.WebApi.DTOs;
using G6.ProBook.WebApi.Models;


namespace G6.ProBook.WebApi.Services
{
using Google.Cloud.Firestore;
    public class RoomService:IRoomService
    {
        /// <summary>
        /// RoomService implementa la gestión de habitaciones
        /// Permite obtener, crear, editar y eliminar habitaciones
        /// Solo administradores pueden crear, editar y eliminar
        /// </summary>
        private readonly FirebaseService _firebaseService;
        /// <summary>
        /// Constructor: Recibe FirebaseService inyectado
        /// Se usa para acceder a Firestore
        /// </summary>
        public RoomService(FirebaseService firebaseService)
        {
            _firebaseService = firebaseService;
        }
        /// <summary>
        /// GetAllRooms: Obtiene todas las habitaciones (con filtro opcional por tipo)
        /// 
        /// Proceso:
        /// 1. Obtener la colección "rooms" de Firestore
        /// 2. Si se especifica type, filtrar
        /// 3. Convertir documentos a RoomDto
        /// 4. Devolver lista
        /// </summary>
        public async Task<List<RoomDto>> GetAllRooms(string? type = null)
        {
            try
            {
                var roomsCollection = _firebaseService.GetCollection("rooms");

                Query query = roomsCollection;

                // Si se especifica género, filtrar por él
                if (!string.IsNullOrWhiteSpace(type))
                {
                    query = query.WhereEqualTo("Type", type);
                }

                // Obtener snapshot (lectura de datos)
                var snapshot = await query.GetSnapshotAsync();

                // Convertir cada documento a MovieDto
                var rooms = new List<RoomDto>();
                foreach (var doc in snapshot.Documents)
                {
                    var room = doc.ConvertTo<Room>();
                    rooms.Add(ConvertToDto(room));
                }

                return rooms;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener habitaciones: {ex.Message}");
                throw;
            }
        }
        /// <summary>
        /// GetRoomById: Obtiene una habitación específica por su ID
        /// 
        /// Proceso:
        /// 1. Acceder al documento por ID
        /// 2. Verificar que existe
        /// 3. Convertir a RoomDto
        /// 4. Devolver
        /// </summary>
        public async Task<RoomDto?> GetRoomById(string roomId)
        {
            try
            {
                var roomsCollection = _firebaseService.GetCollection("rooms");
                var doc = await roomsCollection.Document(roomId).GetSnapshotAsync();

                // Si el documento no existe
                if (!doc.Exists)
                {
                    return null;
                }

                // Convertir a Room y luego a RoomDto
                var room = doc.ConvertTo<Room>();
                return ConvertToDto(room);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener habitación: {ex.Message}");
                return null;
            }
        }
        /// <summary>
        /// CreateRoom: Crea una nueva habitación (solo admin)
        /// 
        /// Proceso:
        /// 1. Validar que los datos requeridos existen
        /// 2. Generar ID si no lo tiene
        /// 3. Establecer CreatedAt y CreatedBy
        /// 4. Guardar en Firestore
        /// 5. Devolver la habitación creada
        /// </summary>
        public async Task<Room> CreateRoom(Room room, string adminId)
        {
            try
            {
                // Validar que los campos requeridos no estén vacíos
                if (string.IsNullOrWhiteSpace(room.Number))
                {
                    throw new ArgumentException("El número de habitación es requerido");
                }

                if (string.IsNullOrWhiteSpace(room.Type))
                {
                    throw new ArgumentException("El tipo de habitación es requerida");
                }

                // Generar ID si no lo tiene
                if (string.IsNullOrWhiteSpace(room.Id))
                {
                    room.Id = Guid.NewGuid().ToString();
                }

                // Establecer información de auditoría
                room.CreatedAt = DateTime.UtcNow;
                room.CreatedBy = adminId;

                // Inicializar contadores
                room.reservationCount = 0;

                // Guardar en Firestore
                var roomsCollection = _firebaseService.GetCollection("rooms");
                await roomsCollection.Document(room.Id).SetAsync(room);

                Console.WriteLine($"Habitación creada: {room.Number} ({room.Id})");
                return room;    
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al crear habitación: {ex.Message}");
                throw;
            }
        }
        /// <summary>
        /// UpdateRoom: Edita una habitación existente (solo admin)
        /// 
        /// Proceso:
        /// 1. Verificar que la habitación existe
        /// 2. Actualizar los campos permitidos
        /// 3. Guardar en Firestore
        /// 4. Devolver habitación actualizada
        /// </summary>
        public async Task<Room> UpdateRoom(string roomId, Room room, string adminId)
        {
            try
            {
                // Validar entrada
                if (string.IsNullOrWhiteSpace(roomId))
                {
                    throw new ArgumentException("El ID de la habitación es requerido");
                }

                var roomsCollection = _firebaseService.GetCollection("rooms");

                // Verificar que la habitación existe
                var existingDoc = await roomsCollection.Document(roomId).GetSnapshotAsync();
                if (!existingDoc.Exists)
                {
                    throw new InvalidOperationException($"Habitación con ID {roomId} no existe");
                }

                // Obtener la habitación existente para preservar campos de auditoría
                var existingRoom = existingDoc.ConvertTo<Room>();

                // Actualizar solo los campos permitidos del requerimiento
                existingRoom.Number = !string.IsNullOrWhiteSpace(room.Number) ? room.Number : existingRoom.Number;
                existingRoom.Type = !string.IsNullOrWhiteSpace(room.Type) ? room.Type : existingRoom.Type;
                // Para la capacidad y precio, validamos que sean mayores a cero
                existingRoom.Capacity = room.Capacity > 0 ? room.Capacity : existingRoom.Capacity;
                existingRoom.BasePrice = room.BasePrice > 0 ? room.BasePrice : existingRoom.BasePrice;
                // Las amenidades se actualizan si la nueva lista trae datos
                existingRoom.Amenities = (room.Amenities != null && room.Amenities.Count > 0)? room.Amenities: existingRoom.Amenities;
                existingRoom.PhotoUrl = !string.IsNullOrWhiteSpace(room.PhotoUrl) ? room.PhotoUrl : existingRoom.PhotoUrl;

                // No actualizar CreatedAt, CreatedBy, reservationCount
                // Esos se controlan automáticamente

                // Guardar cambios
                await roomsCollection.Document(roomId).SetAsync(existingRoom);

                Console.WriteLine($"Habitación actualizada: {roomId}");
                return existingRoom;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al actualizar Habitación: {ex.Message}");
                throw;
            }
        }
        /// <summary>
        /// DeleteRoom: Elimina una habitación (solo admin)
        /// 
        /// Consideración importante:
        /// Antes de eliminar, verificar que NO tiene reservaciones
        /// Si la elimina con reservaciones, quedan "huérfanas"
        /// 
        /// Proceso:
        /// 1. Verificar que la habitación existe
        /// 2. Verificar que no tiene reservaciones
        /// 3. Eliminar de Firestore
        /// </summary>
        public async Task DeleteRoom(string roomId, string adminId)
        {
            try
            {
                // Validar entrada
                if (string.IsNullOrWhiteSpace(roomId))
                {
                    throw new ArgumentException("El ID de habitación es requerido");
                }

                var roomsCollection = _firebaseService.GetCollection("rooms");
                var reservationsCollection = _firebaseService.GetCollection("reservations");

                // Verificar que la habitación existe
                var roomDoc = await roomsCollection.Document(roomId).GetSnapshotAsync();
                if (!roomDoc.Exists)
                {
                    throw new InvalidOperationException($"Habitación con ID {roomId} no existe");
                }

                // Verificar que no tiene reservaciones
                var reservationsQuery = await reservationsCollection
                    .WhereEqualTo("RoomId", roomId)
                    .GetSnapshotAsync();

                if (reservationsQuery.Count > 0)
                {
                    throw new InvalidOperationException(
                        $"No se puede eliminar. La habitación tiene {reservationsQuery.Count} reservaciones. " +
                        "Debe eliminar las reservaciones primero."
                    );
                }

                // Eliminar la habitación
                await roomsCollection.Document(roomId).DeleteAsync();

                Console.WriteLine($"Habitación eliminada: {roomId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al eliminar habitación: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// SearchRooms: Busca habitaciones por type (búsqueda simple)
        /// 
        /// Nota: Firestore no tiene búsqueda de texto completo nativa
        /// Esta es una búsqueda simple que obtiene todas las habitaciones
        /// y filtra en memoria (no es escalable para muchos datos)
        /// 
        /// Proceso:
        /// 1. Obtener todas las habitaciones
        /// 2. Filtrar por type (case-insensitive)
        /// 3. Devolver resultados
        /// </summary>
        public async Task<List<RoomDto>> SearchRoom(string searchTerm)
        {
            try
            {
                // Validar entrada
                if (string.IsNullOrWhiteSpace(searchTerm))
                {
                    return new List<RoomDto>();
                }

                // Obtener todas las habitaciones
                var allRooms = await GetAllRooms();

                // Filtrar por type que contiene el término de búsqueda
                var searchLower = searchTerm.ToLower();
                var results = allRooms
                    .Where(m => m.Type.ToLower().Contains(searchLower))
                    .ToList();

                return results;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al buscar habitación: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Método privado auxiliar: ConvertToDto
        /// 
        /// Convierte un Room (modelo interno) a RoomDto (lo que se envía al frontend)
        /// Es privado porque solo lo usa internamente RoomService
        /// </summary>
        private RoomDto ConvertToDto(Room room)
        {
            return new RoomDto
            {
                Id = room.Id,
                Number = room.Number,
                Type = room.Type,
                Capacity = room.Capacity,
                // Usamos .ToList() para asegurar una copia nueva de la lista
                Amenities = room.Amenities ?? new List<string>(),

                // Verifica que ambos sean decimal
                BasePrice = room.BasePrice,

                // Verifica que la propiedad se llame igual en el DTO
                PhotoUrl = room.PhotoUrl
            };
        }
    }
}
