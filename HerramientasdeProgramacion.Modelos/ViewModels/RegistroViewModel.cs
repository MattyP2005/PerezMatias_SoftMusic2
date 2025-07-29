using System.ComponentModel.DataAnnotations;

namespace HerramientasdeProgramacion.Modelos.ViewModels
{
    public class RegistroViewModel
    {
        [Required]
        public string Nombre { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        public string Plan { get; set; } // "Gratis" o "Premium"

        [Required]
        public string Rol { get; set; } // "Usuario", "Artista", "Admin"
    }
}
