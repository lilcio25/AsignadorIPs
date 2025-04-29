namespace ASIGNADORIPS.Models.ViewModels
{
    public class AsociacionLicenciasViewModel
    {
        public List<Personal> Personal { get; set; }
        public List<Software> Softwares { get; set; }
        public List<UsuarioSoftware> Asignaciones { get; set; }
    }
}
