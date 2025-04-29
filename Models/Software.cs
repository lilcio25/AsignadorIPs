using System;
using System.ComponentModel.DataAnnotations;

namespace ASIGNADORIPS.Models
{
    public class Software
    {
        public int Id { get; set; }

        [Required]
        public string Nombre { get; set; }

        public string Descripcion { get; set; }

        public string OrdenCompra { get; set; }

        public int CantidadLicencias { get; set; }

        public int LicenciasDisponibles { get; set; }

        public decimal CostoTotal { get; set; }

        [DataType(DataType.Date)]
        public DateTime FechaAdquisicion { get; set; }

        [DataType(DataType.Date)]
        public DateTime FechaExpiracion { get; set; }

        // ✅ Nuevos campos configurables
        public bool Notificar { get; set; } = false;

        public int DiasAntesNotificar { get; set; } = 0;

        public string CorreosNotificacion { get; set; } = "";  // << importante

    }
}

