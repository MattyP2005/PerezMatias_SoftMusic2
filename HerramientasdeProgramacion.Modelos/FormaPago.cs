using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HerramientasdeProgramacion.Modelos
{
    public class FormaPago
    {
        public int Id { get; set; }

        [Required]
        public string Tipo { get; set; } // Ej: "Tarjeta", "PayPal", "Crédito"

        [Required]
        [Display(Name = "Detalles de Pago")]
        public string Detalles { get; set; } // Ej: número enmascarado

        public DateTime FechaRegistro { get; set; }

        // Usuario dueño de esta forma de pago
        public int UsuarioId { get; set; }

        public Usuario Usuario { get; set; }
    }
}
