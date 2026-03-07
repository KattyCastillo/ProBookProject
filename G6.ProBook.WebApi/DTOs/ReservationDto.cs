namespace G6.ProBook.WebApi.DTOs
{
    public class ReservationDto
    {
        public string UserID { get; set; } = string.Empty;

        public string RoomID { get; set; } = string.Empty;

        public string RoomType { get; set; } = string.Empty;

        public int RoomNumber { get; set; }

        public DateTime CheckInDate { get; set; }

        public DateTime CheckOutDate { get; set; }
    }

    public class CreateReservationDto
    {
        public string UserID { get; set; } = string.Empty;

        public string RoomID { get; set; } = string.Empty;

        public string RoomType { get; set; } = string.Empty;

        public int RoomNumber { get; set; }

        public DateTime CheckInDate { get; set; }

        public DateTime CheckOutDate { get; set; }

        public double TotalCost { get; set; }

    }
}
