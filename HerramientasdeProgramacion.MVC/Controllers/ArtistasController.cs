using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HerramientasdeProgramacion.API.Data;
using HerramientasdeProgramacion.Modelos;

namespace HerramientasdeProgramacion.MVC.Controllers
{
    public class ArtistasController : Controller
    {
        private readonly SqlServerHdPDbContext _context;
        private readonly IWebHostEnvironment _env;

        public ArtistasController(SqlServerHdPDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // GET: Artistas
        public async Task<IActionResult> Index()
        {
            var artistas = await _context.Artistas.ToListAsync();
            return View(artistas);
        }

        // GET: Artistas/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            var artista = await _context.Artistas
                .Include(a => a.Canciones)
                    .ThenInclude(c => c.AlbumesCanciones)
                        .ThenInclude(ac => ac.Album)
                .FirstOrDefaultAsync(a => a.Id == id);
            if (artista == null)
                return NotFound();
            return View(artista);
        }

        // GET: Artistas/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Artistas/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Artista artista, IFormFile portada)
        {
            if (portada != null && portada.Length > 0)
            {
                var nombreArchivo = Guid.NewGuid().ToString() + Path.GetExtension(portada.FileName);
                var rutaCarpeta = Path.Combine(_env.WebRootPath, "portadas");

                if (!Directory.Exists(rutaCarpeta))
                    Directory.CreateDirectory(rutaCarpeta);

                var rutaCompleta = Path.Combine(rutaCarpeta, nombreArchivo);

                using (var stream = new FileStream(rutaCompleta, FileMode.Create))
                {
                    await portada.CopyToAsync(stream);
                }

                artista.PortadaUrl = "/portadas/" + nombreArchivo;
            }

            _context.Artistas.Add(artista);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult SubirPortada(int id)
        {
            var artista = _context.Artistas.FirstOrDefault(a => a.Id == id);
            if (artista == null) return NotFound();

            return View(artista);
        }

        [HttpPost]
        public IActionResult SubirPortada(int id, IFormFile PortadaFile)
        {
            var artista = _context.Artistas.Find(id);
            if (artista == null) return NotFound();

            if (PortadaFile == null || PortadaFile.Length == 0)
                return BadRequest("Archivo inválido.");

            var extension = Path.GetExtension(PortadaFile.FileName).ToLower();
            if (extension != ".jpg" && extension != ".jpeg" && extension != ".png")
                return BadRequest("Solo se permiten imágenes .jpg y .png");

            var nombreArchivo = $"artista_{id}{extension}";
            var ruta = Path.Combine(_env.WebRootPath, "portadas", nombreArchivo);

            using (var stream = new FileStream(ruta, FileMode.Create))
            {
                PortadaFile.CopyTo(stream);
            }

            artista.PortadaUrl = "/portadas/" + nombreArchivo;
            _context.SaveChanges();

            return RedirectToAction("SubirPortada", new { id });
        }

        // GET: Artistas/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var artista = await _context.Artistas.FindAsync(id);
            if (artista == null)
            {
                return NotFound();
            }
            return View(artista);
        }

        // POST: Artistas/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nombre,PortadaUrl,Biografia,FechaNacimiento,Pais,Email")] Artista artista)
        {
            if (id != artista.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(artista);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ArtistaExists(artista.Id))
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
            return View(artista);
        }

        // GET: Artistas/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var artista = await _context.Artistas
                .FirstOrDefaultAsync(m => m.Id == id);
            if (artista == null)
            {
                return NotFound();
            }

            return View(artista);
        }

        // POST: Artistas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var artista = await _context.Artistas.FindAsync(id);
            if (artista != null)
            {
                _context.Artistas.Remove(artista);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ArtistaExists(int id)
        {
            return _context.Artistas.Any(e => e.Id == id);
        }
    }
}
