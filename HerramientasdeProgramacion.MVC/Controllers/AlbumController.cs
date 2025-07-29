using HerramientasdeProgramacion.API.Data;
using HerramientasdeProgramacion.Modelos;
using HerramientasdeProgramacion.Modelos.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace HerramientasdeProgramacion.MVC.Controllers
{
    //[Authorize(Roles = "Usuario,Artista,Admin")]
    public class AlbumController : Controller
    {
        private readonly SqlServerHdPDbContext _context;

        public AlbumController(SqlServerHdPDbContext context)
        {
            _context = context;
        }

        // GET: Albumes
        public async Task<IActionResult> Index()
        {
            var Email = User.Identity?.Name;

            var albumes = _context.Albumes
                .Include(ac => ac.AlbumesCanciones)
                    .ThenInclude(ac => ac.Cancion)
                        .ThenInclude(c => c.Artista)
                .Include(a => a.Usuario)
                .Where(a => a.Usuario.Email == Email)
                .ToList();

            return View(albumes);
        }

        // GET: Albumes/Details/5
        public async Task<IActionResult> Ver(int id)
        {
            var userEmail = User.Identity?.Name;
            var album = _context.Albumes
                .Include(a => a.AlbumesCanciones)
                    .ThenInclude(ac => ac.Cancion)
                        .ThenInclude(c => c.Artista)
                .Include(a => a.Usuario)
                .FirstOrDefault(a => a.Id == id && a.Usuario.Email == userEmail);

            if (album == null)
                return NotFound();

            return View(album);
        }

        // GET: Albumes/Create
        [HttpGet]
        public IActionResult Crear()
        {
            return View();
        }

        // POST: Albumes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        public async Task<IActionResult> Crear(string titulo, DateTime fechaLanzamiento, string descripcion)
        {
            var userEmail = User.Identity?.Name;
            var usuario = _context.Usuario.FirstOrDefault(u => u.Email == userEmail);

            var album = new Album
            {
                Titulo = titulo,
                FechaLanzamiento = fechaLanzamiento,
                Descripcion = descripcion,
                PortadaUrl = "/images/default-album-cover.png",
                UsuarioId = usuario.Id,
            };

            _context.Albumes.Add(album);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        // GET: Album/AgregarCancion/5
        public IActionResult AgregarCancion(int id)
        {
            var userEmail = User.Identity?.Name;

            var album = _context.Albumes
                .Include(a => a.Usuario)
                .FirstOrDefault(a => a.Id == id && a.Usuario.Email == userEmail);

            if (album == null)
                return NotFound();

            // 🔥 Carga todas las canciones disponibles (sin filtrar por usuario)
            var canciones = _context.Canciones.ToList();

            var viewModel = new AgregarCancionViewModel
            {
                AlbumId = album.Id,
                AlbumTitulo = album.Titulo,
                Canciones = canciones.Select(c => new CancionSeleccionada
                {
                    CancionId = c.Id,
                    Titulo = c.Titulo,
                    Seleccionada = false
                }).ToList()
            };

            return View(viewModel);
        }

        // POST: Album/AgregarCanciones
        [HttpPost]
        public IActionResult AgregarCancion(AgregarCancionViewModel model)
        {
            var userEmail = User.Identity?.Name;

            // Validación: evitar nulo
            if (model.Canciones == null || model.Canciones.Count == 0)
            {
                // Recargar canciones si se vuelve a mostrar la vista
                var cancionesDisponibles = _context.Canciones
                    .Where(c => c.Artista.Email == userEmail)
                    .Select(c => new CancionSeleccionada
                    {
                        CancionId = c.Id,
                        Titulo = c.Titulo,
                        Seleccionada = false
                    }).ToList();

                model.Canciones = cancionesDisponibles;

                ModelState.AddModelError("", "Debes seleccionar al menos una canción.");
                return View(model);
            }

            var album = _context.Albumes
                .Include(a => a.Usuario)
                .Include(a => a.AlbumesCanciones)
                .FirstOrDefault(a => a.Id == model.AlbumId && a.Usuario.Email == userEmail);

            if (album == null) return NotFound();

            foreach (var c in model.Canciones.Where(c => c.Seleccionada))
            {
                var yaExiste = _context.AlbumesCanciones
                    .Any(ac => ac.AlbumId == album.Id && ac.CancionId == c.CancionId);

                if (!yaExiste)
                {
                    _context.AlbumesCanciones.Add(new AlbumCancion
                    {
                        AlbumId = album.Id,
                        CancionId = c.CancionId
                    });
                }
            }

            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        // GET: Albumes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var album = await _context.Albumes.FindAsync(id);
            if (album == null)
            {
                return NotFound();
            }
            ViewData["ArtistaId"] = new SelectList(_context.Usuario, "Id", "Email", album.UsuarioId);
            return View(album);
        }

        // POST: Albumes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Titulo,FechaLanzamiento,Descripcion,PortadaUrl,ArtistaId")] Album album)
        {
            if (id != album.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(album);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AlbumExists(album.Id))
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
            ViewData["ArtistaId"] = new SelectList(_context.Usuario, "Id", "Email", album.UsuarioId);
            return View(album);
        }

        // GET: Albumes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var album = await _context.Albumes
                .Include(a => a.Usuario)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (album == null)
            {
                return NotFound();
            }

            return View(album);
        }

        // POST: Albumes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var album = await _context.Albumes.FindAsync(id);
            if (album != null)
            {
                _context.Albumes.Remove(album);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AlbumExists(int id)
        {
            return _context.Albumes.Any(e => e.Id == id);
        }
    }
}
