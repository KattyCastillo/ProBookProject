namespace G6.ProBook.WebApi.DTOs
{
    //Creado por Obed Casaña - 62221429
    public class RoomDto
    {
        public string Number { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public int Capacity { get; set; }
        public List<string> Amenities { get; set; } = new List<string>();
        public decimal BasePrice { get; set; }
        public string PhotoUrl { get; set; } = string.Empty;
    }
}
