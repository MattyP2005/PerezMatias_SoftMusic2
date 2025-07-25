using System.ComponentModel.DataAnnotations;

namespace HerramientasdeProgramacion.API.Controllers.DTOs
{
    public class CreateCancionDTOs
    {
        [Required]
        public string Titulo { get; set; }

        public string Genero { get; set; }

        [Required]

        public string Url { get; set; } // Donde se aloja la canción (puede ser local o simulada)

        public DateTime FechaSubida { get; set; }

        [Required]
        public int ArtistaId { get; set; } // ID del artista (usuario) que sube la canción
    }
}
