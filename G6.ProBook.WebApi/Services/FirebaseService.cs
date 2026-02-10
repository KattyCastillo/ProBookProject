namespace G6.ProBook.WebApi.Services
{
    using FirebaseAdmin;
    using Google.Apis.Auth.OAuth2;
    using Google.Cloud.Firestore;
    using Microsoft.AspNetCore.Builder.Extensions;
    using Newtonsoft.Json;
    public class FirebaseService
    {
        //_firestoreDb : Instancia de la base de datos FS
        //Privada porque solo este service la maneja
        private readonly FirestoreDb _firestoreDb;


        // _logger para registrar eventos como errores
        //Nos permite ver que esta pasando
        private readonly ILogger<FirebaseService> _logger;

        //Constructor: se ejecuta cuando arranca
        //Recibe un Ilogger inyectado por ASP.NET Core
        public FirebaseService(ILogger<FirebaseService> logger)
        {
            _logger = logger;

            try
            {
                //Paso 1: Obtener la ruta del archivo red
                //AppContext.BaseDirectory: Directorio raiz de la app
                //Path.Combine: Une las rutas de forma segura
                var credentialsPath = Path.Combine(
                    AppContext.BaseDirectory,
                    "Config",
                    "firebase-credentials.json"
                    );

                //Paso 2: Validar que el archivo existe
                //Si no, lanzar exception
                if (!File.Exists(credentialsPath))
                {
                    throw new FileNotFoundException(
                        $"Archivos de credenciales no encontrado en: {credentialsPath}"
                    );
                }

                //Paso 3: Inicializar Firebase Admin SDK
                //GoogleCredentials.FromFile:
                //FirebaseApp.Create:
                if (FirebaseApp.DefaultInstance == null)
                {
                    FirebaseApp.Create(new AppOptions
                    {
                        Credential = GoogleCredential.FromFile(credentialsPath)
                    });
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}
