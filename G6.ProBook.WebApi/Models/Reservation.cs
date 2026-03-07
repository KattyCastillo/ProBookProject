namespace G6.ProBook.WebApi.Models
{
    //Modelo realizado por Obed Casaña 
    //Modelo de reservacion
    public class Reservation
    {
        public string Id { get; set; } = string.Empty;
        public string RoomId { get; set; } = string.Empty;
        public string RoomNumber { get; set; } = string.Empty;
        public string RoomType { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public int Nights { get; set; }
        public double TotalCost { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
