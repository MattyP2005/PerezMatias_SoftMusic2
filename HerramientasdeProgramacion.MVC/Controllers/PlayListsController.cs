using HerramientasdeProgramacion.API.Data;
using HerramientasdeProgramacion.Modelos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace HerramientasdeProgramacion.MVC.Controllers
{
    public class PlayListsController : Controller
    {
        private readonly SqlServerHdPDbContext _context;

        public PlayListsController(SqlServerHdPDbContext context)
        {
            _context = context;
        }

        // GET: PlayLists
        public async Task<IActionResult> Index()
        {
            var playlists = _context.PlayLists
                .Include(p => p.Usuario)
                .Include(p => p.Canciones)
                    .ThenInclude(pc => pc.Cancion)
                        .ThenInclude(c => c.Artista)
                .Where(p => p.Usuario.Email == User.Identity.Name)
                .ToList();

            return View(playlists);
        }

        // GET: PlayLists/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var playlist = await _context.PlayLists
                .Include(p => p.Usuario)
                .Include(p => p.Canciones)
                    .ThenInclude(pc => pc.Cancion)
                        .ThenInclude(c => c.Artista)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (playlist == null)
                return NotFound();

            return View(playlist);
        }

        // GET: PlayLists/Create
        public IActionResult Create()
        {
            var userEmail = User.Identity?.Name;
            var usuario = _context.Usuarios.FirstOrDefault(u => u.Email == userEmail);

            if (usuario == null)
                return Unauthorized();

            if (!usuario.Plan.StartsWith("Premium"))
            {
                TempData["Error"] = "Solo los usuarios con plan Premium pueden crear playlists.";
                return RedirectToAction("Index");
            }

            return View();
        }

        // POST: PlayLists/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string nombre)
        {
            var userEmail = User.Identity?.Name;
            var usuario = _context.Usuarios.FirstOrDefault(u => u.Email == userEmail);
            if (usuario == null) return Unauthorized();

            if (!usuario.Plan.StartsWith("Premium"))
            {
                TempData["Error"] = "Solo los usuarios con plan Premium pueden crear playlists.";
                return RedirectToAction("Index");
            }

            var playlist = new PlayList
            {
                Nombre = nombre,
                UsuarioId = usuario.Id,
                FechaCreacion = DateTime.UtcNow,
                EsPublica = true
            };

            _context.PlayLists.Add(playlist);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        // GET: Agregar Canciones a Playlist
        public IActionResult AgregarCanciones(int id)
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

        // POST: Agregar Canciones a Playlist
        [HttpPost]
        public IActionResult AgregarCanciones(int playlistId, int[] cancionesSeleccionadas)
        {
            foreach (var cancionId in cancionesSeleccionadas)
            {
                var existe = _context.PlayListsCanciones.Any(pc => pc.PlaylistId == playlistId && pc.CancionId == cancionId);
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
            return RedirectToAction("Index");
        }

        // GET: PlayLists/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var playList = await _context.PlayLists.FindAsync(id);
            if (playList == null)
            {
                return NotFound();
            }
            ViewData["UsuarioId"] = new SelectList(_context.Usuarios, "Id", "Email", playList.UsuarioId);
            return View(playList);
        }

        // POST: PlayLists/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nombre,FechaCreacion,UsuarioId,EsPublica")] PlayList playList)
        {
            if (id != playList.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(playList);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PlayListExists(playList.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["UsuarioId"] = new SelectList(_context.Usuarios, "Id", "Email", playList.UsuarioId);
            return View(playList);
        }

        // GET: PlayLists/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var playList = await _context.PlayLists
                .Include(p => p.Usuario)
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
