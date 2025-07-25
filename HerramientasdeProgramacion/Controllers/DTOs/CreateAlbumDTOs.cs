using System.ComponentModel.DataAnnotations;

namespace HerramientasdeProgramacion.API.Controllers.DTOs
{
    public class CreateAlbumDTOs
    {
        [Required]
        public string Titulo { get; set; }
    }
}
