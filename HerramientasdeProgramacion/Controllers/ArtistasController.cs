using HerramientasdeProgramacion.API.Controllers.DTOs;
using HerramientasdeProgramacion.API.Data;
using HerramientasdeProgramacion.Modelos;
using Microsoft.AspNetCore.Mvc;

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
            var artistas = _context.Usuarios.ToList();
            return Ok(artistas);
        }

        // GET: api/Artistas/5
        [HttpGet]
        [Route("{id}")]
        public IActionResult GetArtista(int id)
        {
            var artista = _context.Usuarios.Find(id);
            if (artista == null || artista.Rol != "Artista")
                return NotFound("Artista no encontrado o no es un artista.");
            return Ok(artista);
        }

        // POST: api/Artistas
        [HttpGet]
        [Route("crear")]
        public async Task<IActionResult> CrearArtista([FromBody] CreateArtistaDTOs dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var artista = new Artista
            {
                Nombre = dto.Nombre,
                Biografia = dto.Biografia,
                FechaNacimiento = dto.FechaNacimiento,
                Pais = dto.Pais,
            };

            if (dto.PortadaUrl != null)
            {
                var nombreArchivo = Guid.NewGuid().ToString() + Path.GetExtension(dto.PortadaUrl.FileName);
                var rutaCarpeta = Path.Combine(_env.WebRootPath, "portadas");

                if (!Directory.Exists(rutaCarpeta))
                    Directory.CreateDirectory(rutaCarpeta);

                var rutaCompleta = Path.Combine(rutaCarpeta, nombreArchivo);

                using (var stream = new FileStream(rutaCompleta, FileMode.Create))
                {
                    await dto.PortadaUrl.CopyToAsync(stream);
                }

                artista.PortadaUrl = "/portadas/" + nombreArchivo;
            }
            _context.Artistas.Add(artista);
            await _context.SaveChangesAsync();

            return Ok(artista);
        }

        // PUT: api/Artistas/5
        [HttpPut("{id}")]
        public async Task<IActionResult> ActualizarArtista(int id, [FromBody] CreateArtistaDTOs dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var artista = _context.Artistas.Find(id);
            if (artista == null)
                return NotFound("Artista no encontrado o no es un artista.");
            artista.Nombre = dto.Nombre;
            artista.Biografia = dto.Biografia;
            artista.FechaNacimiento = dto.FechaNacimiento;
            artista.Pais = dto.Pais;

            if (dto.PortadaUrl != null)
            {
                if (!string.IsNullOrWhiteSpace(artista.PortadaUrl))
                {
                    var rutaVieja = Path.Combine(_env.WebRootPath, artista.PortadaUrl.TrimStart('/'));
                    if (System.IO.File.Exists(rutaVieja))
                        System.IO.File.Delete(rutaVieja);
                }

                var nombreArchivo = Guid.NewGuid().ToString() + Path.GetExtension(dto.PortadaUrl.FileName);
                var rutaCompleta = Path.Combine(_env.WebRootPath, "portadas", nombreArchivo);

                using (var stream = new FileStream(rutaCompleta, FileMode.Create))
                {
                    await dto.PortadaUrl.CopyToAsync(stream);
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
            var artista = _context.Usuarios.Find(id);
            if (artista == null || artista.Rol != "Artista")
                return NotFound("Artista no encontrado o no es un artista.");
            _context.Usuarios.Remove(artista);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
