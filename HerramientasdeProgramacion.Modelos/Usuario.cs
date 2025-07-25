using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HerramientasdeProgramacion.Modelos
{
    public class Usuario
    {
        public int Id { get; set; }

        [Required]
        public string Email { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        [Required]
        public string Rol { get; set; }     // "Usuario", "Admin", "Artista"

        [Required]
        public string Plan { get; set; }    // "Free", "Personal", etc.

        // Opcionales para el perfil
        public string Nombre { get; set; }   // Nombre real o nickname

        public string? ImagenPerfilUrl { get; set; }  // URL o ruta relativa a la imagen de perfil

        public FormaPago? FormaPago { get; set; }

        public ICollection<PlayList> PlayLists { get; set; }

        public ICollection<Historial> Historiales { get; set; }

        public ICollection<FormaPago> FormasPagos { get; set; }

        public List<Album> Albumes { get; set; }
    }
}
