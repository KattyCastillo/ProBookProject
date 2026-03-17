using Google.Cloud.Firestore;

namespace G6.ProBook.WebApi.Models
{
    //Modelo realizado por Obed Casaña 
    //Modelo de habitacion
    [FirestoreData]
    public class Room
    {
            [FirestoreDocumentId]
            public string Id { get; set; } = string.Empty;
            [FirestoreProperty]    
            public string Number { get; set; } = string.Empty;
            [FirestoreProperty]
            public string Type { get; set; } = string.Empty;
            [FirestoreProperty]
            public int Capacity { get; set; }
            [FirestoreProperty]
            public double BasePrice { get; set; }
            [FirestoreProperty]
            public List<string> Amenities { get; set; } = new List<string>();
            [FirestoreProperty]
            public string PhotoUrl { get; set; } = string.Empty;
            [FirestoreProperty]
            public int reservationCount { get; set; }
            [FirestoreProperty]
            public DateTime CreatedAt { get; set; }
            [FirestoreProperty]
            public string CreatedBy { get; set; } = string.Empty;
    }
}
