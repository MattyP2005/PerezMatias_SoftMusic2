using System.ComponentModel.DataAnnotations;

namespace HerramientasdeProgramacion.Modelos.ViewModels
{
    public class MetodoPagoViewModel
    {
        public int UsuarioId { get; set; }

        [Required]
        [Display(Name = "Tipo de tarjeta")]
        public string TipoTarjeta { get; set; }

        [Required]
        [Display(Name = "Número de tarjeta")]
        [CreditCard(ErrorMessage = "Número de tarjeta inválido")]
        public string NumeroTarjeta { get; set; }

        [Required]
        [Display(Name = "Nombre del titular")]
        public string NombreTitular { get; set; }

        [Required]
        [Display(Name = "Expira (MM/AA)")]
        [RegularExpression(@"^(0[1-9]|1[0-2])\/\d{2}$", ErrorMessage = "Formato debe ser MM/AA")]
        public string FechaExpiracion { get; set; }

        [Required]
        [Display(Name = "CVV")]
        [StringLength(4, MinimumLength = 3, ErrorMessage = "CVV inválido")]
        public string CVV { get; set; }
    }
}
