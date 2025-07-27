using System.ComponentModel.DataAnnotations;

namespace HerramientasdeProgramacion.API.Controllers.DTOs
{
    public class CreateAlbumDTOs
    {
        [Required]
        public string Titulo { get; set; }

        [Required]
        public DateTime FechaLanzamiento { get; set; }

        [Required]
        public string Descripcion { get; set; }

        [Required]
        public int ArtistaId { get; set; }

        [Required]
        public string Portada { get; set; } // URL de la portada del álbum
    }
}
