using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ASIGNADORIPS.Models
{
    public class UsuarioSoftware
    {
        [Key]
        public int Id { get; set; }

        public int PersonalId { get; set; }

        [ForeignKey("PersonalId")]
        public Personal Personal { get; set; }

        public int SoftwareId { get; set; }

        [ForeignKey("SoftwareId")]
        public Software Software { get; set; }
    }
}

