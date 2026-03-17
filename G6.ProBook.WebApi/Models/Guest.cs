using Google.Cloud.Firestore;

namespace G6.ProBook.WebApi.Models
{
    [FirestoreData]
    public class Guest
    {
        [FirestoreProperty]
        public string Nombre { get; set; }

        [FirestoreProperty]
        public string Email { get; set; }

        [FirestoreProperty]
        public string Identidad { get; set; }

        [FirestoreProperty]
        public string Telefono { get; set; }
    }
}