using HerramientasdeProgramacion.API.Data;
using HerramientasdeProgramacion.Modelos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HerramientasdeProgramacion.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PlayListsController : ControllerBase
    {
        private readonly SqlServerHdPDbContext _context;

        public PlayListsController(SqlServerHdPDbContext context)
        {
            _context = context;
        }

        // 
        [HttpPost]
        public IActionResult CrearPlaylist([FromBody] string nombre)
        {
            var email = User.Identity?.Name;
            var usuario = _context.Usuarios.FirstOrDefault(u => u.Email == email);

            if (usuario == null)
                return Unauthorized();

            var yaExiste = _context.PlayLists.Any(p => p.UsuarioId == usuario.Id && p.Nombre == nombre);
            if (yaExiste)
                return Conflict("Ya tienes una playlist con ese nombre.");

            var playlist = new PlayList
            {
                Nombre = nombre,
                FechaCreacion = DateTime.UtcNow,
                UsuarioId = usuario.Id
            };

            _context.PlayLists.Add(playlist);
            _context.SaveChanges();

            return Ok("Playlist creada.");
        }

        // 
        [HttpGet]
        public IActionResult ObtenerPlaylists()
        {
            var email = User.Identity?.Name;
            var usuario = _context.Usuarios.FirstOrDefault(u => u.Email == email);

            if (usuario == null)
                return Unauthorized();

            var playlists = _context.PlayLists
                .Where(p => p.UsuarioId == usuario.Id)
                .Select(p => new
                {
                    p.Id,
                    p.Nombre,
                    Canciones = p.Canciones.Select(pc => pc.Cancion.Titulo)
                })
                .ToList();

            return Ok(playlists);
        }

        //
        [HttpPost("{playlistId}/agregar")]
        public IActionResult AgregarCancion(int playlistId, [FromBody] int cancionId)
        {
            var email = User.Identity?.Name;
            var usuario = _context.Usuarios.FirstOrDefault(u => u.Email == email);

            var playlist = _context.PlayLists.FirstOrDefault(p => p.Id == playlistId && p.UsuarioId == usuario.Id);
            if (playlist == null)
                return NotFound("Playlist no encontrada.");

            var yaExiste = _context.PlayListsCanciones.Any(pc => pc.PlaylistId == playlistId && pc.CancionId == cancionId);
            if (yaExiste)
                return Conflict("La canción ya está en la playlist.");

            _context.PlayListsCanciones.Add(new PlayListCancion
            {
                PlaylistId = playlistId,
                CancionId = cancionId
            });

            _context.SaveChanges();
            return Ok("Canción agregada.");
        }

        // 
        [HttpPost("{playlistId}/quitar")]
        public IActionResult QuitarCancion(int playlistId, [FromBody] int cancionId)
        {
            var cancion = _context.PlayListsCanciones.FirstOrDefault(pc =>
                pc.PlaylistId == playlistId && pc.CancionId == cancionId);

            if (cancion == null)
                return NotFound("La canción no está en la playlist.");

            _context.PlayListsCanciones.Remove(cancion);
            _context.SaveChanges();

            return Ok("Canción quitada.");
        }
    }
}
