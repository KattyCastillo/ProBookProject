namespace G6.ProBook.WebApi.Models
{
    //Modelo realizado por Obed Casaña 
    //Modelo de habitacion
    public class Room
    {
            public string Id { get; set; } = string.Empty;

            public string Number { get; set; } = string.Empty;

            public string Type { get; set; } = string.Empty;

            public int Capacity { get; set; }

            public decimal BasePrice { get; set; }

            public List<string> Amenities { get; set; } = new List<string>();

            public string PhotoUrl { get; set; } = string.Empty;

            public int reservationCount { get; set; }

            public DateTime CreatedAt { get; set; }

            public string CreatedBy { get; set; } = string.Empty;
    }
}
