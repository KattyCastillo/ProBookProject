namespace G6.ProBook.WebApi.Models
{
    //Modelo de reservacion
    public class Reservation
    {
        public string Id { get; set; } = string.Empty;

        public string UserID { get; set; } = string.Empty;

        public string UserName { get; set; } = string.Empty;

        public string RoomID { get; set; } = string.Empty;

        public string RoomType { get; set; } = string.Empty;

        public int RoomNumber { get; set; }

        public DateTime CheckInDate { get; set; }

        public DateTime CheckOutDate { get; set; }

        public int Nights { get; set; }

        public double TotalCost { get; set; }

        public DateTime Timestamp { get; set; }
    }
}
