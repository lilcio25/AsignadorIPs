using System;

namespace ASIGNADORIPS.Models
{
    public class HistorialAccion
    {
        public int Id { get; set; }
        public string Usuario { get; set; }         // Nombre de usuario desde Session
        public string IP { get; set; }              // IP del cliente
        public string Accion { get; set; }          // Ej: "Inicio sesión", "Agregó Licencia"
        public DateTime Fecha { get; set; } = DateTime.Now;
    }
}
