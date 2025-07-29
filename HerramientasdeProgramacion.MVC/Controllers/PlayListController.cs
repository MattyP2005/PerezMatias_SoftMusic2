using HerramientasdeProgramacion.API.Data;
using HerramientasdeProgramacion.Modelos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace HerramientasdeProgramacion.MVC.Controllers
{
    public class PlayListController : Controller
    {
        private readonly SqlServerHdPDbContext _context;

        public PlayListController(SqlServerHdPDbContext context)
        {
            _context = context;
        }

        // GET: PlayLists
        public async Task<IActionResult> Index()
        {
            var playlists = _context.PlayLists
                .Include(p => p.Artista)
                .Include(p => p.Canciones)
                    .ThenInclude(pc => pc.Cancion)
                        .ThenInclude(c => c.Artista)
                .Where(p => p.Artista.Email == User.Identity.Name)
                .ToList();

            return View(playlists);
        }

        // GET: PlayLists/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var playlist = await _context.PlayLists
                .Include(p => p.Artista)
                .Include(p => p.Canciones)
                    .ThenInclude(pc => pc.Cancion)
                        .ThenInclude(c => c.Artista)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (playlist == null)
                return NotFound();

            return View(playlist);
        }

        // GET: PlayLists/Create
        [Authorize(Roles = "Admin,Artista")]
        public IActionResult Create()
        {
            var userEmail = User.Identity?.Name;
            var usuario = _context.Usuario.FirstOrDefault(u => u.Email == userEmail);

            if (usuario == null)
                return Unauthorized();

            return View();
        }

        // POST: PlayLists/Create
        [HttpPost]
        [Authorize(Roles = "Admin,Artista")]
        public async Task<IActionResult> Create(string nombre)
        {
            var userEmail = User.Identity?.Name;
            var usuario = _context.Usuario.FirstOrDefault(u => u.Email == userEmail);

            if (usuario == null)
                return Unauthorized();

            var playlist = new PlayList
            {
                Nombre = nombre,
                ArtistaId = usuario.Id,
                FechaCreacion = DateTime.UtcNow,
                EsPublica = true
            };

            _context.PlayLists.Add(playlist);
            await _context.SaveChangesAsync();

            return RedirectToAction("AgregarCancion", new { id = playlist.Id });
        }

        // GET: Agregar Canciones a Playlist
        public IActionResult AgregarCancion(int id)
        {
            var playlist = _context.PlayLists
                .Include(p => p.Canciones)
                .ThenInclude(pc => pc.Cancion)
                .FirstOrDefault(p => p.Id == id);

            if (playlist == null)
                return NotFound();

            ViewBag.CancionesDisponibles = _context.Canciones.ToList();
            return View(playlist);
        }

        // POST: PlayLists/AgregarCancion
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        public IActionResult AgregarCancion(int playlistId, int[] cancionesSeleccionadas)
        {
            foreach (var cancionId in cancionesSeleccionadas)
            {
                var existe = _context.PlayListsCanciones
                    .Any(pc => pc.PlaylistId == playlistId && pc.CancionId == cancionId);

                if (!existe)
                {
                    _context.PlayListsCanciones.Add(new PlayListCancion
                    {
                        PlaylistId = playlistId,
                        CancionId = cancionId
                    });
                }
            }

            _context.SaveChanges();

            TempData["MensajeExito"] = "🎉 Canciones agregadas correctamente.";
            return RedirectToAction("Details", new { id = playlistId });
        }

        // GET: PlayLists/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var playList = await _context.PlayLists
                .Include(p => p.Artista)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (playList == null)
            {
                return NotFound();
            }

            return View(playList);
        }

        // POST: PlayLists/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var playlist = _context.PlayLists
                .Include(p => p.Canciones)
                .FirstOrDefault(p => p.Id == id);

            if (playlist == null)
                return NotFound();

            // Eliminar relaciones con canciones
            _context.PlayListsCanciones.RemoveRange(playlist.Canciones);

            // Eliminar playlist
            _context.PlayLists.Remove(playlist);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        // POST: Quitar Canción de Playlist
        [HttpPost]
        public IActionResult QuitarCancion(int playlistId, int cancionId)
        {
            var relacion = _context.PlayListsCanciones
                .FirstOrDefault(pc => pc.PlaylistId == playlistId && pc.CancionId == cancionId);

            if (relacion == null)
                return NotFound();

            _context.PlayListsCanciones.Remove(relacion);
            _context.SaveChanges();

            return RedirectToAction("Ver", new { id = playlistId });
        }

        private bool PlayListExists(int id)
        {
            return _context.PlayLists.Any(e => e.Id == id);
        }
    }
}
