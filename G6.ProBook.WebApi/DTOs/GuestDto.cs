namespace G6.ProBook.WebApi.DTOs
{
    public class GuestDto
    {
        public class Create
        {
            public string Nombre { get; set; }
            public string Email { get; set; }
            public string Identidad { get; set; }
            public string Telefono { get; set; }
        }

        public class Read
        {
            public string Id { get; set; }
            public string Nombre { get; set; }
            public string Identidad { get; set; }
        }
    }
}