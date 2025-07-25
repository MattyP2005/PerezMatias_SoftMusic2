using HerramientasdeProgramacion.API.Controllers.DTOs;
using HerramientasdeProgramacion.API.Data;
using HerramientasdeProgramacion.Modelos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HerramientasdeProgramacion.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ArtistasController : ControllerBase
    {
        private readonly SqlServerHdPDbContext _context;
        private readonly IWebHostEnvironment _env;

        public ArtistasController(SqlServerHdPDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // GET: api/ArtistaApi
        [HttpGet]
        public IActionResult GetArtistas()
        {
            var artistas = _context.Artistas.ToList();
            return Ok(artistas);
        }

        // GET: api/ArtistaApi/5
        [HttpGet("{id}")]
        public IActionResult GetArtista(int id)
        {
            var artista = _context.Artistas.Find(id);
            if (artista == null)
                return NotFound();

            return Ok(artista);
        }

        // POST: api/ArtistaApi
        [HttpPost]
        public async Task<IActionResult> CrearArtista([FromForm] CreateArtistaDTOs dto)
        {
            var artista = new Artista
            {
                Nombre = dto.Nombre,
                Biografia = dto.Biografia,
                Pais = dto.Pais
            };

            if (dto.Portada != null)
            {
                var nombreArchivo = Guid.NewGuid().ToString() + Path.GetExtension(dto.Portada.FileName);
                var rutaCarpeta = Path.Combine(_env.WebRootPath, "portadas");

                if (!Directory.Exists(rutaCarpeta))
                    Directory.CreateDirectory(rutaCarpeta);

                var rutaCompleta = Path.Combine(rutaCarpeta, nombreArchivo);

                using (var stream = new FileStream(rutaCompleta, FileMode.Create))
                {
                    await dto.Portada.CopyToAsync(stream);
                }

                artista.PortadaUrl = "/portadas/" + nombreArchivo;
            }

            _context.Artistas.Add(artista);
            await _context.SaveChangesAsync();

            return Ok(artista);
        }

        // PUT: api/ArtistaApi/5
        [HttpPut("{id}")]
        public async Task<IActionResult> EditarArtista(int id, [FromForm] CreateArtistaDTOs dto)
        {
            var artista = await _context.Artistas.FindAsync(id);
            if (artista == null)
                return NotFound();

            artista.Nombre = dto.Nombre;
            artista.Biografia = dto.Biografia;
            artista.Pais = dto.Pais;

            if (dto.Portada != null)
            {
                if (!string.IsNullOrWhiteSpace(artista.PortadaUrl))
                {
                    var rutaVieja = Path.Combine(_env.WebRootPath, artista.PortadaUrl.TrimStart('/'));
                    if (System.IO.File.Exists(rutaVieja))
                        System.IO.File.Delete(rutaVieja);
                }

                var nombreArchivo = Guid.NewGuid().ToString() + Path.GetExtension(dto.Portada.FileName);
                var rutaCompleta = Path.Combine(_env.WebRootPath, "portadas", nombreArchivo);

                using (var stream = new FileStream(rutaCompleta, FileMode.Create))
                {
                    await dto.Portada.CopyToAsync(stream);
                }

                artista.PortadaUrl = "/portadas/" + nombreArchivo;
            }

            await _context.SaveChangesAsync();

            return Ok(artista);
        }

        // DELETE: api/ArtistaApi/5
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public IActionResult EliminarArtista(int id)
        {
            var artista = _context.Artistas
                .Include(a => a.Canciones)
                .FirstOrDefault(a => a.Id == id);

            if (artista == null)
                return NotFound("Artista no encontrado.");

            // 1. Eliminar canciones y archivos físicos
            foreach (var cancion in artista.Canciones)
            {
                // Eliminar historial de esta canción
                var historial = _context.Historiales.Where(h => h.CancionId == cancion.Id);
                _context.Historiales.RemoveRange(historial);

                // Eliminar de playlists
                var enPlaylists = _context.PlayListsCanciones.Where(pc => pc.CancionId == cancion.Id);
                _context.PlayListsCanciones.RemoveRange(enPlaylists);

                // Eliminar archivo físico
                var ruta = Path.Combine(_env.WebRootPath, cancion.Url.TrimStart('/'));
                if (System.IO.File.Exists(ruta))
                    System.IO.File.Delete(ruta);
            }

            _context.Canciones.RemoveRange(artista.Canciones);

            // 2. Eliminar portada del artista (si existe)
            if (!string.IsNullOrWhiteSpace(artista.PortadaUrl))
            {
                var rutaPortada = Path.Combine(_env.WebRootPath, artista.PortadaUrl.TrimStart('/'));
                if (System.IO.File.Exists(rutaPortada))
                    System.IO.File.Delete(rutaPortada);
            }

            // 3. Eliminar artista
            _context.Artistas.Remove(artista);

            _context.SaveChanges();

            return Ok("Artista eliminado junto con sus canciones, álbumes y portada.");
        }
    }
}
