using System.Collections.Generic;

namespace ASIGNADORIPS.Models.ViewModels
{
    public class UsuarioSoftwareViewModel
    {
        public List<Usuario> Usuarios { get; set; }
        public List<Software> Softwares { get; set; }
        public List<UsuarioSoftware> Asignaciones { get; set; }
    }
}
