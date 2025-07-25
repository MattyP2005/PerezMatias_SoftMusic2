using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HerramientasdeProgramacion.Modelos
{
    public class PlayListCancion
    {
        public int PlaylistId { get; set; }
        public PlayList PlayList { get; set; }

        public int CancionId { get; set; }
        public Cancion Cancion { get; set; }
    }
}
