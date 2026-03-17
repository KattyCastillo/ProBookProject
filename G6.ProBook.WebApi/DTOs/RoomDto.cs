namespace G6.ProBook.WebApi.DTOs
{
    //Creado por Obed Casaña - 62221429
    //Se envían sólo los datos necesarios, por eso no se envían CreatedAt, CreatedBy, reservationCount
    public class RoomDto
    {
        public string Id { get; set; } = string.Empty;
        public string Number { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public int Capacity { get; set; }
        public List<string> Amenities { get; set; } = new List<string>();
        public double BasePrice { get; set; }
        public string PhotoUrl { get; set; } = string.Empty;
    }
}
