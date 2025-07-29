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

        public string Descripcion { get; set; } // Descripción opcional del álbum

        public string PortadaUrl { get; set; } // URL o ruta relativa a la portada del álbum

        // Relación con Artista
        public int UsuarioId { get; set; }

        [ForeignKey("UsuarioId")]
        public Usuario Usuario { get; set; }

        // Canciones que contiene el álbum
        public ICollection<AlbumCancion> AlbumesCanciones { get; set; }
    }
}
