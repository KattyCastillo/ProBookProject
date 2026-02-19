using G6.ProBook.WebApi.Models;
using FirebaseAdmin;
using FirebaseAdmin.Auth;
using G6.ProBook.WebApi.Controllers;
using Google.Cloud.Firestore.V1;

namespace G6.ProBook.WebApi.Services
{
   

    public class AuthService
    {
        private readonly FirebaseService _firebaseService;
        private readonly ILogger _logger;

        public AuthService(FirebaseService firebaseService, ILogger<TestController> logger)
        {
            _firebaseService = firebaseService;
            _logger = logger;
        }

    }
}
