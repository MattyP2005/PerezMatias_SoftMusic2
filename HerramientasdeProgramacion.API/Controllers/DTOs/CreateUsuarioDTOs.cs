using System.ComponentModel.DataAnnotations;

namespace HerramientasdeProgramacion.API.Controllers.DTOs
{
    public class CreateUsuarioDTOs
    {
        [Required]
        public string Nombre { get; set; }
        [Required]
        public string Email { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        [Required]
        public string Rol { get; set; }

        [Required]
        public string Plan { get; set; }
    }
}
