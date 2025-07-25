using HerramientasdeProgramacion.API.Controllers.DTOs;
using HerramientasdeProgramacion.API.Data;
using HerramientasdeProgramacion.Modelos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HerramientasdeProgramacion.API.Controllers
{
    [ApiController]
    [Route("api[controller]")]
    public class AlbumesController : ControllerBase
    {
        private readonly SqlServerHdPDbContext _context;
        private readonly IWebHostEnvironment _env;

        public AlbumesController(SqlServerHdPDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // GET: api/AlbumApi
        [HttpGet]
        public IActionResult GetAll()
        {
            var albums = _context.Albumes
                .Include(a => a.Artista)
                .Select(a => new
                {
                    a.Id,
                    a.Titulo,
                })
                .ToList();

            return Ok(albums);
        }

        // GET: api/AlbumApi/5
        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var album = _context.Albumes
                .Include(a => a.Artista)
                .Where(a => a.Id == id)
                .Select(a => new
                {
                    a.Id,
                    a.Titulo,
                })
                .FirstOrDefault();

            if (album == null)
                return NotFound();

            return Ok(album);
        }

        // POST: api/AlbumApi
        [HttpPost]
        public IActionResult Create([FromBody] CreateAlbumDTOs dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var email = User.Identity?.Name;
            var artista = _context.Usuarios.FirstOrDefault(u => u.Email == email);

            if (artista == null)
                return Unauthorized();

            var album = new Album
            {
                Titulo = dto.Titulo,
                FechaLanzamiento = DateTime.Now, // asignación automática
                ArtistaId = artista.Id           // se asocia al usuario logueado
            };

            _context.Albumes.Add(album);
            _context.SaveChanges();

            return CreatedAtAction(nameof(GetById), new { id = album.Id }, new { album.Id });
        }

        // PUT: api/AlbumApi/5
        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] CreateAlbumDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var album = _context.Albumes.FirstOrDefault(a => a.Id == id);
            if (album == null)
                return NotFound();

            // Solo se puede cambiar el título
            album.Titulo = dto.Titulo;

            _context.SaveChanges();

            return NoContent();
        }

        // DELETE: api/AlbumApi/5
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var album = _context.Albumes.Find(id);
            if (album == null)
                return NotFound();

            _context.Albumes.Remove(album);
            _context.SaveChanges();

            return NoContent();
        }
    }
}
