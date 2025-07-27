using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace HerramientasdeProgramacion.Modelos.ViewModels
{
    public class EditarPerfilViewModel
    {
        public int Id { get; set; }

        [Required]
        public string Nombre { get; set; }

        public string ImagenPerfilUrl { get; set; }

        [Display(Name = "Nueva imagen de perfil")]
        public IFormFile ImagenPerfil { get; set; }

        [Display(Name = "Eliminar imagen actual")]
        public bool EliminarImagen { get; set; }
    }
}
