using Google.Cloud.Firestore;

namespace G6.ProBook.WebApi.Models
{
    //Modelo de reservacion
    [FirestoreData]
    public class Reservation
    {
        [FirestoreDocumentId]
        public string Id { get; set; } = string.Empty;
        [FirestoreProperty]
        public string UserID { get; set; } = string.Empty;
        [FirestoreProperty]
        public string UserName { get; set; } = string.Empty;
        [FirestoreProperty]
        public string RoomID { get; set; } = string.Empty;
        [FirestoreProperty]
        public string RoomType { get; set; } = string.Empty;
        [FirestoreProperty]
        public string RoomNumber { get; set; } = string.Empty;
        [FirestoreProperty]
        public DateTime CheckInDate { get; set; }
        [FirestoreProperty]
        public DateTime CheckOutDate { get; set; }
        [FirestoreProperty]
        public int Nights { get; set; }
        [FirestoreProperty]
        public double TotalCost { get; set; }
        [FirestoreProperty]
        public DateTime Timestamp { get; set; }
    }
}
