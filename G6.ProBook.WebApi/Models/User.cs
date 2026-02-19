using Google.Cloud.Firestore;

namespace G6.ProBook.WebApi.Models
{
    //Modelo de User
    [FirestoreData]
    public class User
    {
        // Id es la clave primaria
        // Identificador unico de cada usuario
        [FirestoreProperty]
        public string Id { get; set; }

        // Nombre del usuario
        [FirestoreProperty]
        public string Nombre { get; set; }

        // Rol: gerente o huesped
        [FirestoreProperty]
        public string Rol { get; set; }

        // indica si ya realizó una reserva
        [FirestoreProperty]
        public bool hasReserved { get; set; }

        // Correo del cliente
        [FirestoreProperty]
        public string Email { get; set; }

        //Fecha que se creo el usuario
        [FirestoreProperty]
        public DateTime FechaCreacion { get; set; }
    }
}
