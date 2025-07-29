using HerramientasdeProgramacion.API.Data;
using HerramientasdeProgramacion.Modelos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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

        // GET: api/PlayLists
        [HttpGet]
        public IActionResult GetPlayLists()
        {
            var playLists = _context.PlayLists.ToList();
            return Ok(playLists);
        }

        // GET: api/PlayLists/5
        [HttpGet("{id}")]
        public IActionResult GetPlayList(int id)
        {
            var playList = _context.PlayLists.Find(id);
            if (playList == null)
                return NotFound();
            return Ok(playList);
        }

        // POST: api/PlayLists
        [HttpPost]
        public async Task<IActionResult> CrearPlayList([FromBody] string nombre)
        {
            var email = User.Identity?.Name;
            var usuario = _context.Usuario.FirstOrDefault(u => u.Email == email);

            if (usuario == null)
                return Unauthorized();

            var yaExiste = _context.PlayLists.Any(p => p.ArtistaId == usuario.Id && p.Nombre == nombre);
            if (yaExiste)
                return Conflict("Ya tienes una playlist con ese nombre.");

            var playlist = new PlayList
            {
                Nombre = nombre,
                FechaCreacion = DateTime.UtcNow,
                ArtistaId = usuario.Id
            };

            _context.PlayLists.Add(playlist);
            _context.SaveChanges();

            return Ok("Playlist creada.");
        }

        // POST: api/PlayLists/5/agregar
        [HttpPost("{id}/agregar")]
        public async Task<IActionResult> AgregarCancionAPlayList(int id, [FromBody] int cancionId)
        {
            var playList = await _context.PlayLists.FindAsync(id);
            if (playList == null)
                return NotFound();
            var cancion = await _context.Canciones.FindAsync(cancionId);
            if (cancion == null)
                return NotFound("Canción no encontrada.");
            if (_context.PlayListsCanciones.Any(pc => pc.PlaylistId == id && pc.CancionId == cancionId))
                return Conflict("La canción ya está en la playlist.");
            var playListCancion = new PlayListCancion
            {
                PlaylistId = id,
                CancionId = cancionId
            };
            _context.PlayListsCanciones.Add(playListCancion);
            await _context.SaveChangesAsync();
            return Ok("Canción agregada a la playlist.");
        }

        // PUT: api/PlayLists/5
        [HttpPut("{id}")]
        public async Task<IActionResult> ActualizarPlayList(int id, string nombre)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var playList = await _context.PlayLists.FindAsync(id);
            if (playList == null)
                return NotFound();
            playList.Nombre = nombre;


            _context.PlayLists.Update(playList);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/PlayLists/5/canciones/10
        [HttpDelete("{id}/canciones/{cancionId}")]
        public async Task<IActionResult> EliminarCancionDePlayList(int id, int cancionId)
        {
            var playList = await _context.PlayLists.FindAsync(id);
            if (playList == null)
                return NotFound();
            var playListCancion = await _context.PlayListsCanciones
                .FirstOrDefaultAsync(pc => pc.PlaylistId == id && pc.CancionId == cancionId);
            if (playListCancion == null)
                return NotFound("La canción no está en la playlist.");
            _context.PlayListsCanciones.Remove(playListCancion);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/PlayLists/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> EliminarPlayList(int id)
        {
            var playList = await _context.PlayLists.FindAsync(id);
            if (playList == null)
                return NotFound();
            _context.PlayLists.Remove(playList);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
