using System.ComponentModel.DataAnnotations;

namespace ASIGNADORIPS.Models
{
    public class Personal
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Nombre del Funcionario")]
        public string NombreCompleto { get; set; }

        [Required]
        [Display(Name = "Código del Funcionario")]
        public string CodigoFuncionario { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Correo Institucional")]
        public string CorreoFuncionario { get; set; }
    }
}

