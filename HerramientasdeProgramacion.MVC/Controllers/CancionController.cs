using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using HerramientasdeProgramacion.API.Data;
using HerramientasdeProgramacion.Modelos;

namespace HerramientasdeProgramacion.MVC.Controllers
{
    public class CancionController : Controller
    {
        private readonly SqlServerHdPDbContext _context;
        private readonly IWebHostEnvironment _env;

        public CancionController(SqlServerHdPDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // GET: Canciones
        public async Task<IActionResult> Index()
        {
            var userEmail = User.Identity?.Name;
            var canciones = _context.Canciones
                .Include(c => c.Artista) // relación con Usuario
                .Where(c => c.Artista.Email == userEmail)
                .OrderByDescending(c => c.FechaSubida)
                .ToList();

            return View(canciones);
        }

        // GET: Canciones/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cancion = await _context.Canciones
                .Include(c => c.Album)
                .Include(c => c.Artista)
                .Include(c => c.Usuario)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (cancion == null)
            {
                return NotFound();
            }

            return View(cancion);
        }

        // GET: Canciones/Create
        public IActionResult Create()
        {
            ViewData["AlbumId"] = new SelectList(_context.Albumes, "Id", "Descripcion");
            ViewData["ArtistaId"] = new SelectList(_context.Artistas, "Id", "Biografia");
            ViewData["UsuarioId"] = new SelectList(_context.Usuario, "Id", "Email");
            return View();
        }

        // POST: Canciones/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string Titulo, string Genero, IFormFile AudioFile)
        {
            if (AudioFile == null || AudioFile.Length == 0)
                return BadRequest("Debes subir un archivo de audio.");

            // 🔒 Validar extensión del archivo
            var extension = Path.GetExtension(AudioFile.FileName).ToLower();
            if (extension != ".mp3")
                return BadRequest("Solo se permiten archivos .mp3");

            // Guardar archivo con nombre único
            var fileName = Guid.NewGuid().ToString() + extension;
            var path = Path.Combine(_env.WebRootPath, "audio", fileName);

            using (var stream = new FileStream(path, FileMode.Create))
            {
                AudioFile.CopyTo(stream);
            }

            // Obtener usuario autenticado
            var email = User.Identity?.Name;
            var usuario = _context.Usuario.FirstOrDefault(u => u.Email == email);

            if (usuario == null)
                return Unauthorized();

            var cancion = new Cancion
            {
                Titulo = Titulo,
                Genero = Genero,
                Url = "/audio/" + fileName,
                FechaSubida = DateTime.UtcNow,
                UsuarioId = usuario.Id
            };

            // Validar nombre único (opcional)
            var existe = _context.Canciones.Any(c => c.Titulo == Titulo);
            if (existe)
                return Conflict("Ya existe una canción con ese título.");

            _context.Canciones.Add(cancion);
            _context.SaveChanges();

            return RedirectToAction("Index", "Home");
        
        }

        // GET: Canciones/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cancion = await _context.Canciones.FindAsync(id);
            if (cancion == null)
            {
                return NotFound();
            }
            ViewData["AlbumId"] = new SelectList(_context.Albumes, "Id", "Descripcion", cancion.AlbumId);
            ViewData["ArtistaId"] = new SelectList(_context.Artistas, "Id", "Biografia", cancion.ArtistaId);
            ViewData["UsuarioId"] = new SelectList(_context.Usuario, "Id", "Email", cancion.UsuarioId);
            return View(cancion);
        }

        // POST: Canciones/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Titulo,Genero,Url,FechaSubida,ArtistaId,UsuarioId,AlbumId,Reproducciones,Duracion")] Cancion cancion)
        {
            if (id != cancion.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(cancion);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CancionExists(cancion.Id))
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
            ViewData["AlbumId"] = new SelectList(_context.Albumes, "Id", "Descripcion", cancion.AlbumId);
            ViewData["ArtistaId"] = new SelectList(_context.Artistas, "Id", "Biografia", cancion.ArtistaId);
            ViewData["UsuarioId"] = new SelectList(_context.Usuario, "Id", "Email", cancion.UsuarioId);
            return View(cancion);
        }

        // GET: Canciones/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cancion = await _context.Canciones
                .Include(c => c.Album)
                .Include(c => c.Artista)
                .Include(c => c.Usuario)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (cancion == null)
            {
                return NotFound();
            }

            return View(cancion);
        }

        // POST: Canciones/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var cancion = await _context.Canciones.FindAsync(id);
            if (cancion != null)
            {
                _context.Canciones.Remove(cancion);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CancionExists(int id)
        {
            return _context.Canciones.Any(e => e.Id == id);
        }
    }
}
