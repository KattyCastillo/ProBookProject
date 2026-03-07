namespace G6.ProBook.WebApi.DTOs
{
    public class RoomDto
    {
        public string Id { get; set; } = string.Empty;

        public int RoomNumber { get; set; }

        public string RoomType { get; set; } = string.Empty;

        public int RoomCapacity { get; set; }

        public string RoomAmenities { get; set; } = string.Empty;

        public List<string>? RoomPicturesUrl { get; set; }
    }
}
