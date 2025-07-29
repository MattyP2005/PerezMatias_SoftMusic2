using HerramientasdeProgramacion.API.Controllers.DTOs;
using HerramientasdeProgramacion.API.Data;
using HerramientasdeProgramacion.Modelos;
using Microsoft.AspNetCore.Mvc;

namespace HerramientasdeProgramacion.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AlbumesController : ControllerBase
    {
        private readonly SqlServerHdPDbContext _context;

        public AlbumesController(SqlServerHdPDbContext context)
        {
            _context = context;
        }

        // GET: api/Albumes
        [HttpGet]
        public IActionResult GetAlbumes()
        {
            var albumes = _context.Albumes.ToList();
            return Ok(albumes);
        }

        // GET: api/Albumes/5
        [HttpGet("{id}")]
        public IActionResult GetAlbum(int id)
        {
            var album = _context.Albumes.Find(id);
            if (album == null)
                return NotFound();
            return Ok(album);
        }

        // POST: api/Albumes
        [HttpPost]
        public async Task<IActionResult> CrearAlbum([FromBody] CreateAlbumDTOs dto)
        {
            var email = User.Identity?.Name;
            var artista = _context.Usuario.FirstOrDefault(u => u.Email == email);
            if (artista == null)
                return Unauthorized("Usuario no encontrado o no autorizado.");

            var album = new Album
            {
                Titulo = dto.Titulo,
                Descripcion = dto.Descripcion,
                FechaLanzamiento = DateTime.Now,
                ArtistaId = artista.Id
            };
            
            _context.Albumes.Add(album);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetAlbum), new { id = album.Id }, album);
        }

        // PUT: api/Albumes/5
        [HttpPut("{id}")]
        public async Task<IActionResult> ActualizarAlbum(int id, [FromBody] CreateAlbumDTOs dto)
        {
            var album = await _context.Albumes.FindAsync(id);
            if (album == null)
                return NotFound();
            album.Titulo = dto.Titulo;
            album.Descripcion = dto.Descripcion;
            album.FechaLanzamiento = dto.FechaLanzamiento;
            
            _context.Albumes.Update(album);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/Albumes/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> EliminarAlbum(int id)
        {
            var album = await _context.Albumes.FindAsync(id);
            if (album == null)
                return NotFound();
            _context.Albumes.Remove(album);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
