using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HerramientasdeProgramacion.Modelos
{
    public class Album
    {
        public int Id { get; set; }

        [Required]
        public string Titulo { get; set; }

        public DateTime FechaLanzamiento { get; set; }

        // Relación con Artista
        public int ArtistaId { get; set; }

        [ForeignKey("ArtistaId")]
        public Usuario Artista { get; set; }

        // Canciones que contiene el álbum
        public ICollection<AlbumCancion> AlbumesCanciones { get; set; }
    }
}
