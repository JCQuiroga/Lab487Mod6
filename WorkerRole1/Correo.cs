using System.IO;

namespace WorkerRole1
{
    public class Correo
    {
        public string Origen { get; set; }
        public string Destino { get; set; }
        public string Asunto { get; set; }
        public string Contenido { get; set; }
    }
}