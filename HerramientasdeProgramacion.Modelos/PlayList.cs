using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HerramientasdeProgramacion.Modelos
{
    public class PlayList
    {
        public int Id { get; set; }

        [Required]
        public string Nombre { get; set; }

        public DateTime FechaCreacion { get; set; }

        // Artista dueño de la playlist
        public int ArtistaId { get; set; }

        public Usuario Artista { get; set; }

        // Indica si la playlist es pública o privada
        public bool EsPublica { get; set; } = false; // Por defecto, privada

        // Relación con canciones
        public ICollection<PlayListCancion> Canciones { get; set; }
    }
}
