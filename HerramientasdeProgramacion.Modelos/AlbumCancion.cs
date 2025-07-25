using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HerramientasdeProgramacion.Modelos
{
    public class AlbumCancion
    {
        public int AlbumId { get; set; }
        public Album Album { get; set; }

        public int CancionId { get; set; }
        public Cancion Cancion { get; set; }
    }
}
