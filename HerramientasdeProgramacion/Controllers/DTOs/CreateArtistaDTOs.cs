using System.ComponentModel.DataAnnotations;

namespace HerramientasdeProgramacion.API.Controllers.DTOs
{
    public class CreateArtistaDTOs
    {
        [Required]
        public string Nombre { get; set; }

        public string Biografia { get; set; }

        public DateTime FechaNacimiento { get; set; }

        public string Pais { get; set; }

        public IFormFile Portada { get; set; }  // Subir imagen del artista
    }
}
