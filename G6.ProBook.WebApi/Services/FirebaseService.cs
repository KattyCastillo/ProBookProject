
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Firestore;
using Google.Cloud.Firestore.V1;
using Grpc.Auth;
using Newtonsoft.Json;

namespace G6.ProBook.WebApi.Services
{
    public class FirebaseService
    {
        private readonly FirestoreDb _firebaseDb; //Instancia de la DB en base de datos
        private readonly ILogger<FirebaseService> _logger;

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

                //Paso #1 obtener la ruta del archivo de configuración con las credenciales
                var credentialPath = Path.Combine(AppContext.BaseDirectory, "Config", "firebase-credentials.json");

                //Paso #2 Validar que exista el archivo
                if (!File.Exists(credentialPath)) {
                    throw new FileNotFoundException($"Archivo de credenciales no encontrado en: {credentialPath}");
                }

                //Paso #3
                var projectId = GetProjectIdFromCredentials(credentialPath);

                var projectIdString = GetProjectIdFromCredentials(credentialPath);

                Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", credentialPath);

                //Paso #3 Inicializar Firebase Admin SDK
                // GooogleCredential.FromFile 
                // Firebase.Create
                if (FirebaseApp.DefaultInstance == null)
                {
                    FirebaseApp.Create(
                        new AppOptions {
                            Credential = GoogleCredential.FromFile(credentialPath)
                        }
                    );
                }

                //Crea el cliente en Firebase
                var firebaseClientBuilder = new FirestoreClientBuilder
                {
                    ChannelCredentials = GoogleCredential.FromFile(credentialPath)
                    .CreateScoped("")
                    .ToChannelCredentials()
                };

                // Crear el buil 

                var firebaseClient = firebaseClientBuilder.Build();
                _firebaseDb = FirestoreDb.Create(projectId, firebaseClient);
                Console.WriteLine("Conexión a Firebase iniciada correctamente");


            }
            catch (Exception e)
            {
                Console.WriteLine($"Error al iniciar Firebase:{e.InnerException }");

                Console.WriteLine(e);
                throw;
            }
        }

        private string GetProjectIdFromCredentials(string credentialsPath)
        {
            //Primero: Leer el archivo JSON como string
            //File.ReadAllText: lee todo el contenido del archivo en memoria
            var json = File.ReadAllText(credentialsPath);

            //Segundo: Parseamos el JSON (convertimos a un objeto dinamico)
            //JsonConvert.DeserializeObject: convierte el string JSON a un objeto C#
            //dynamic: Tipo flexible que permite acceder a propiedades en tiempo de ejecucion
            dynamic credentials = JsonConvert.DeserializeObject(json);

            //Tercero: Extraer y devolver el project id
            //credentials["project_id"]: acceder a la propiedad id del JSON
            return credentials["project_id"];
        }
    
        public CollectionReference GetCollection(string collentionName)
        {
            return _firebaseDb.Collection(path: collentionName);
        }
    }
}
