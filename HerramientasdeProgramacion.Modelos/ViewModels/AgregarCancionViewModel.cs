namespace HerramientasdeProgramacion.Modelos.ViewModels
{
    public class AgregarCancionViewModel
    {
        public int AlbumId { get; set; }
        public string AlbumTitulo { get; set; }

        public List<CancionSeleccionada> Canciones { get; set; }
    }

    public class CancionSeleccionada
    {
        public int CancionId { get; set; }
        public string Titulo { get; set; }
        public bool Seleccionada { get; set; }
    }
}
