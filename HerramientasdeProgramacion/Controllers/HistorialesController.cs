using HerramientasdeProgramacion.API.Data;
using HerramientasdeProgramacion.Modelos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HerramientasdeProgramacion.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class HistorialesController : ControllerBase
    {
        private readonly SqlServerHdPDbContext _context;
        private readonly IWebHostEnvironment _env;

        public HistorialesController(SqlServerHdPDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // 
        [HttpPost("{cancionId}")]
        public IActionResult RegistrarReproduccion(int cancionId)
        {
            var email = User.Identity?.Name;
            var usuario = _context.Usuario.FirstOrDefault(u => u.Email == email);

            if (usuario == null)
                return Unauthorized();

            var cancion = _context.Canciones.Find(cancionId);
            if (cancion == null)
                return NotFound("Canción no encontrada.");

            var registro = new Historial
            {
                UsuarioId = usuario.Id,
                CancionId = cancionId,
                FechaHora = DateTime.UtcNow
            };

            _context.Historiales.Add(registro);
            _context.SaveChanges();

            return Ok("Reproducción registrada.");
        }

        // 
        [HttpGet]
        public IActionResult VerHistorial()
        {
            var email = User.Identity?.Name;
            var usuario = _context.Usuario
                .Include(u => u.Historiales)
                .ThenInclude(h => h.Cancion)
                .FirstOrDefault(u => u.Email == email);

            if (usuario == null)
                return Unauthorized();

            var historial = usuario.Historiales
                .OrderByDescending(h => h.FechaHora)
                .Select(h => new
                {
                    Cancion = h.Cancion.Titulo,
                    h.FechaHora
                }).ToList();

            return Ok(historial);
        }
    }
}
