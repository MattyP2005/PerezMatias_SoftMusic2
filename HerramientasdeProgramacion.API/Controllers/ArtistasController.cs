using HerramientasdeProgramacion.API.Controllers.DTOs;
using HerramientasdeProgramacion.API.Data;
using HerramientasdeProgramacion.Modelos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HerramientasdeProgramacion.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ArtistasController : ControllerBase
    {
        private readonly SqlServerHdPDbContext _context;
        private readonly IWebHostEnvironment _env;
        public ArtistasController(SqlServerHdPDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // GET: api/Artistas
        [HttpGet]
        public IActionResult GetArtistas()
        {
            var artistas = _context.Artistas.ToList();
            return Ok(artistas);
        }

        // GET: api/Artistas/5
        [HttpGet("{id}")]
        public IActionResult GetArtista(int id)
        {
            var artista = _context.Usuario.Find(id);
            if (artista == null || artista.Rol != "Artista")
                return NotFound("Artista no encontrado o no es un artista.");
            return Ok(artista);
        }

        // POST: api/Artistas
        //[Authorize(Roles = "Admin,Artista")]
        [HttpPost]
        [Route("crear")]
        public async Task<IActionResult> CrearArtista([FromForm] CreateArtistaDTOs dto)
        {
            var artista = new Artista
            {
                Nombre = dto.Nombre,
                Biografia = dto.Biografia,
                FechaNacimiento = dto.FechaNacimiento,
                Pais = dto.Pais,
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

                var baseUrl = $"{Request.Scheme}://{Request.Host}";
                artista.PortadaUrl = $"{baseUrl}/portadas/{nombreArchivo}";
            }
            _context.Artistas.Add(artista);
            await _context.SaveChangesAsync();

            return Ok(artista);
        }

        // PUT: api/Artistas/5
        [HttpPut("{id}")]
        public async Task<IActionResult> ActualizarArtista(int id, [FromBody] CreateArtistaDTOs dto)
        {
            var artista = await _context.Artistas.FindAsync(id);
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            
            if (artista == null)
                return NotFound("Artista no encontrado o no es un artista.");
            artista.Nombre = dto.Nombre;
            artista.Biografia = dto.Biografia;
            artista.FechaNacimiento = dto.FechaNacimiento;
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

        // DELETE: api/Artistas/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> EliminarArtista(int id)
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
