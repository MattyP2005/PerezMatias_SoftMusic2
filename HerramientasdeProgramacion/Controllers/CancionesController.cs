using HerramientasdeProgramacion.API.Data;
using HerramientasdeProgramacion.Modelos;
using HerramientasdeProgramacion.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using HerramientasdeProgramacion.API.Controllers.DTOs;

namespace HerramientasdeProgramacion.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CancionesController : ControllerBase
    {
        private readonly SqlServerHdPDbContext _context;
        private readonly IWebHostEnvironment _env;

        public CancionesController(SqlServerHdPDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // GET: api/CancionApi
        [HttpGet]
        public IActionResult ObtenerCanciones()
        {
            var canciones = _context.Canciones
                .Select(c => new
                {
                    c.Id,
                    c.Titulo,
                    c.Genero,
                    Artista = c.Usuario.Email
                }).ToList();

            return Ok(canciones);
        }

        // GET: api/CancionApi/{id}
        [Authorize]
        [HttpGet("descargar/{id}")]
        public IActionResult DescargarCancion(int id)
        {
            var email = User.Identity?.Name;
            var usuario = _context.Usuarios.FirstOrDefault(u => u.Email == email);

            if (usuario == null)
                return Unauthorized();

            if (!PlanServices.PermiteDescargas(usuario.Plan))
                return Forbid("Tu plan no permite descargas.");

            if (usuario.Plan == "Free")
                return Forbid("Tu plan no permite descargas.");

            var cancion = _context.Canciones.Find(id);
            if (cancion == null)
                return NotFound("Canción no encontrada.");


            // Simular una descarga
            return Ok(new
            {
                Mensaje = "Descarga simulada.",
                cancion.Titulo,
                cancion.Url,
                PlanUsuario = usuario.Plan,
                DispositivosMaximos = PlanServices.GetMaxDispositivos(usuario.Plan),
                PrecioMensual = PlanServices.GetPrecioMensual(usuario.Plan)
            });
        }

        // POST: api/CancionApi
        [Authorize(Roles = "Artista,Admin")]
        [HttpPost]
        public IActionResult SubirCancion([FromBody] CreateCancionDTOs dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var email = User.Identity?.Name;
            var usuario = _context.Usuarios.SingleOrDefault(u => u.Email == email);

            if (usuario == null)
                return Unauthorized("Usuario no válido.");

            // Validar nombre único
            var yaExiste = _context.Canciones.Any(c => c.Titulo == dto.Titulo);
            if (yaExiste)
                return Conflict("Ya existe una canción con ese nombre.");

            // Crear objeto Cancion desde el DTO
            var cancion = new Cancion
            {
                Titulo = dto.Titulo,
                Genero = dto.Genero,
                Url = dto.Url,
                FechaSubida = DateTime.UtcNow,
                UsuarioId = usuario.Id,
                ArtistaId = dto.ArtistaId // opcional si manejas Artista como entidad separada
            };

            _context.Canciones.Add(cancion);
            _context.SaveChanges();

            return Ok("Canción subida correctamente.");
        }

        // 
        [Authorize(Roles = "Admin,Artista")]
        [HttpDelete("{id}")]
        public IActionResult EliminarCancion(int id)
        {
            var email = User.Identity?.Name;
            var usuario = _context.Usuarios.FirstOrDefault(u => u.Email == email);
            if (usuario == null)
                return Unauthorized();

            var cancion = _context.Canciones.FirstOrDefault(c => c.Id == id);
            if (cancion == null)
                return NotFound("Canción no encontrada.");

            // Solo permite eliminar si es el dueño o es Admin
            if (usuario.Rol != "Admin" && cancion.UsuarioId != usuario.Id)
                return Forbid("No tienes permiso para eliminar esta canción.");

            // Eliminar archivo del disco si existe
            var filePath = Path.Combine(_env.WebRootPath, cancion.Url.TrimStart('/'));
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }

            // Eliminar de la base de datos
            _context.Canciones.Remove(cancion);
            _context.SaveChanges();

            return Ok("Canción eliminada.");
        }
    }
}
