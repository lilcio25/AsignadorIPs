namespace ASIGNADORIPS.Models
{
    public class ConfiguracionCorreo
    {
        public int Id { get; set; }  // Siempre será 1 (solo una configuración)

        public string Remitente { get; set; } = "";
        public string HostSMTP { get; set; } = "";
        public int PuertoSMTP { get; set; }
        public string UsuarioSMTP { get; set; } = "";
        public string ClaveSMTP { get; set; } = "";
        public bool UsarSSL { get; set; }
    }
}
