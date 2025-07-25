using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HerramientasdeProgramacion.Modelos
{
    public class Historial
    {
        public int Id { get; set; }

        public int UsuarioId { get; set; }

        public Usuario Usuario { get; set; }

        public int CancionId { get; set; }

        public Cancion Cancion { get; set; }

        public DateTime FechaHora { get; set; }
    }
}
