using G6.ProBook.WebApi.DTOs;
using G6.ProBook.WebApi.Models;
using Google.Cloud.Firestore;

namespace G6.ProBook.WebApi.Services
{
    public class ReservationService : IReservationService
    {
        private readonly FirebaseService _firebaseService;
        private readonly ILogger<ReservationService> _logger;
        private readonly AuthService _authService;
        private readonly RoomService _roomService;
        public ReservationService
            (
            FirebaseService firebaseService,
            ILogger<ReservationService> logger,
            AuthService authService,
            RoomService roomService
            ) 
        { 
            _firebaseService = firebaseService;
            _logger = logger;
            _authService = authService;
            _roomService = roomService;
        }
        public async Task<Reservation> CreateReservation(CreateReservationDto createReservationDto)
        {
            try
            {
                // Validar que los campos requeridos no estén vacíos
                if (string.IsNullOrWhiteSpace(createReservationDto.UserID))
                {
                    throw new ArgumentException("El ID de usuario es requerido");
                }

                if (string.IsNullOrWhiteSpace(createReservationDto.RoomID))
                {
                    throw new ArgumentException("El ID de habitacion es requerido");
                }

                // Generar ID si no lo tiene
                if (string.IsNullOrWhiteSpace(createReservationDto.Id))
                {
                    createReservationDto.Id = Guid.NewGuid().ToString();
                }

                //Revisar si el usuario existe
                var usuario = await _authService.GetUserById(createReservationDto.UserID);
                if(usuario ==  null)
                {
                    throw new InvalidOperationException("Usuario no existe");
                }

                //Revisar si la habitacion existe
                var habitacion = await _roomService.GetRoomById(createReservationDto.RoomID);
                if(habitacion == null)
                {
                    throw new InvalidOperationException("Habitacion no existe");
                }

                //Revisar que CheckOutDate sea mayor a CheckInDate 
                if(createReservationDto.CheckInDate >= createReservationDto.CheckOutDate)
                {
                    throw new InvalidOperationException("La fecha de check out no puede ser menor o igual a la fecha de check in");
                }

                //Revisar que el usuario no tenga otra reservacion
                if ((bool)usuario.hasReserved)
                {
                    throw new InvalidOperationException("Usuario ya tiene reservacion");
                }

                //Revisar si la habitacion esta disponible en las fechas
                var reseravationCollection = _firebaseService.GetCollection("reservations");
                if(reseravationCollection == null)
                {
                    throw new InvalidOperationException("No se pudo obtener la colleccion de reservaciones");
                }

                var query = await reseravationCollection
                    .WhereEqualTo("RoomID", createReservationDto.RoomID)
                    .GetSnapshotAsync();

                bool reserved = false;
                if (query.Count > 0)
                {
                    var reservationDocs = query.Documents;
                    foreach (var doc in reservationDocs)
                    {
                        var reservationDict = doc.ToDictionary();
                        if (
                            ((Timestamp)reservationDict["CheckInDate"]).ToDateTime() <= createReservationDto.CheckOutDate
                            && createReservationDto.CheckOutDate <= ((Timestamp)reservationDict["CheckOutDate"]).ToDateTime()
                            )
                        {
                            reserved = true;
                            break;
                        }
                    }

                }

                if(reserved)
                {
                    throw new InvalidOperationException("Habitacion ya esta reservada");
                }

                var reservacion = new Reservation
                {
                    Id = Guid.NewGuid().ToString(),
                    UserID = createReservationDto.UserID,
                    UserName = usuario.Fullname,
                    RoomID = createReservationDto.RoomID,
                    RoomType = habitacion.Type,
                    RoomNumber = habitacion.Number,
                    CheckInDate = createReservationDto.CheckInDate,
                    CheckOutDate = createReservationDto.CheckOutDate,
                    Nights = (createReservationDto.CheckOutDate - createReservationDto.CheckInDate).Days,
                    TotalCost = createReservationDto.TotalCost,
                    Timestamp = DateTime.UtcNow
                };

                var usersCollection = _authService.GetCollection("users");

                usuario.hasReserved = true;

                await usersCollection.Document(usuario.Id).SetAsync(usuario);

                await reseravationCollection.Document(reservacion.Id).SetAsync(reservacion);

                Console.WriteLine($"Reservacion creada");
                return reservacion;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al crear reservacion: {ex.Message}");
                throw;
            }
        }

        public async Task<List<ReservationDto>?> GetAllReservations()
        {
            try
            {
                var reseravationCollection = _firebaseService.GetCollection("reservations");

                Query query = reseravationCollection;

                // Obtener snapshot (lectura de datos)
                var snapshot = await query.GetSnapshotAsync();

                // Convertir cada documento a MovieDto
                var reservations = new List<ReservationDto>();
                foreach (var doc in snapshot.Documents)
                {
                    var reservation = doc.ConvertTo<Reservation>();
                    reservations.Add(ConvertToDto(reservation));
                }

                return reservations;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener películas: {ex.Message}");
                throw;
            }
        }

        public async Task<List<Room>?> GetAvailableRooms(DateTime checkInDate, DateTime checkOutDate)
        {
            try
            {
                var roomList = await _roomService.GetAllRooms();
                var reseravationList = await GetAllReservations();

                List<Room> availableRooms = new List<Room>();

                foreach(var room in roomList)
                {
                    bool reserved = false;

                    foreach(var reservation in reseravationList)
                    {
                        if(room.Id == reservation.RoomID)
                        {
                            if(checkInDate <= reservation.CheckOutDate && reservation.CheckOutDate <= checkInDate)
                            {
                                reserved = true;
                            }
                        }
                    }

                    if (!reserved)
                    {
                        availableRooms.Add(room);
                    }
                }

                return availableRooms;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al verificar disponibilidad: {ex.Message}");
                throw;
            }

        }

        public async Task<ReservationDto?> GetReservationById(string reservationId)
        {
            try
            {
                var reseravationCollection = _firebaseService.GetCollection("reservations");

                var doc = await reseravationCollection.Document(reservationId).GetSnapshotAsync();

                if(!doc.Exists)
                {
                    return null;
                }

                var reservation = doc.ConvertTo<Reservation>();
                return ConvertToDto(reservation);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener películas: {ex.Message}");
                throw;
            }
        }

        public async Task<List<ReservationDto>?> GetReservationsByRoomId(string roomId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(roomId))
                {
                    throw new ArgumentException("El ID de habitacion es requerido");
                }
                var reseravationCollection = _firebaseService.GetCollection("reservations");

                var query = reseravationCollection
                    .WhereEqualTo("RoomID", roomId)
                    .OrderByDescending("Timestamp");

                // Obtener snapshot (lectura de datos)
                var snapshot = await query.GetSnapshotAsync();

                // Convertir cada documento a MovieDto
                var reservations = new List<ReservationDto>();
                foreach (var doc in snapshot.Documents)
                {
                    var reservation = doc.ConvertTo<Reservation>();
                    reservations.Add(ConvertToDto(reservation));
                }

                return reservations;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener películas: {ex.Message}");
                throw;
            }
        }

        public async Task<List<ReservationDto>?> GetReservationsByUserId(string userId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userId))
                {
                    throw new ArgumentException("El ID de usuario es requerido");
                }
                var reseravationCollection = _firebaseService.GetCollection("reservations");

                var query = reseravationCollection
                    .WhereEqualTo("RoomID", userId)
                    .OrderByDescending("Timestamp");

                // Obtener snapshot (lectura de datos)
                var snapshot = await query.GetSnapshotAsync();

                // Convertir cada documento a MovieDto
                var reservations = new List<ReservationDto>();
                foreach (var doc in snapshot.Documents)
                {
                    var reservation = doc.ConvertTo<Reservation>();
                    reservations.Add(ConvertToDto(reservation));
                }

                return reservations;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener películas: {ex.Message}");
                throw;
            }
        }

        private ReservationDto ConvertToDto(Reservation reservation)
        {
            return new ReservationDto
            {
                Id = reservation.Id,
                UserID = reservation.UserID,
                RoomID = reservation.RoomID,
                RoomType = reservation.RoomType,
                RoomNumber = reservation.RoomNumber,
                CheckInDate = reservation.CheckInDate,
                CheckOutDate = reservation.CheckOutDate
            };
        }
    }
}
