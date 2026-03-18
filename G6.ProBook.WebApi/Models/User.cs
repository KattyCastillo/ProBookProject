namespace G6.ProBook.WebApi.Models
{
    public class User
    {
        public string Id { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Fullname { get; set; } = string.Empty;

        public string Role { get; set; } = string.Empty;

        public string ProfilePictureUrl { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        public DateTime LastLogin { get; set; }

        public bool IsActive { get; set; } = true;

        public bool HasReserved { get; set; } = false;

        public DateTime ReservationTimestamp { get; set; }
    }
}
