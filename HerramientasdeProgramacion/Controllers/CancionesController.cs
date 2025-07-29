using HerramientasdeProgramacion.API.Controllers.DTOs;
using HerramientasdeProgramacion.API.Data;
using HerramientasdeProgramacion.API.Services;
using HerramientasdeProgramacion.Modelos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HerramientasdeProgramacion.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CancionesController : ControllerBase
    {
        private readonly SqlServerHdPDbContext _context;
        private readonly IWebHostEnvironment _env;
        public CancionesController(SqlServerHdPDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }
        // GET: api/Canciones
        [HttpGet]
        public IActionResult GetCanciones()
        {
            var canciones = _context.Canciones.ToList();
            return Ok(canciones);
        }
        // GET: api/Canciones/5
        [HttpGet("{id}")]
        public IActionResult GetCancion(int id)
        {
            var cancion = _context.Canciones.Select(c => new
            {
                c.Id,
                c.Titulo,
                c.Duracion,
                c.Genero,
                Artista = c.Usuario.Email
            }).ToList();
            
            if (cancion == null)
                return NotFound();
            return Ok(cancion);
        }
        // POST: api/Canciones
        [Authorize]
        [Route("CrearCancion")]
        [HttpPost]
        public async Task<IActionResult> CrearCancion([FromForm] CreateCancionDTOs dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var email = User.Identity?.Name;
            var usuario = _context.Usuario.SingleOrDefault(u => u.Email == email);

            if (usuario == null)
                return Unauthorized("Usuario no válido.");

            // Validar nombre único
            var yaExiste = _context.Canciones.Any(c => c.Titulo == dto.Titulo);
            if (yaExiste)
                return Conflict("Ya existe una canción con ese nombre.");

            // Validar extensión del archivo
            var ext = Path.GetExtension(dto.Url.FileName).ToLower();
            if (ext != ".mp3")
                return BadRequest("Solo se permiten archivos .mp3");

            // Guardar el archivo en disco
            var nombreArchivo = Guid.NewGuid() + ".mp3";
            var rutaCarpeta = Path.Combine(_env.WebRootPath, "canciones");

            if (!Directory.Exists(rutaCarpeta))
                Directory.CreateDirectory(rutaCarpeta);

            var rutaCompleta = Path.Combine(rutaCarpeta, nombreArchivo);

            using (var stream = new FileStream(rutaCompleta, FileMode.Create))
            {
                await dto.Url.CopyToAsync(stream);
            }

            // Crear objeto Cancion
            var cancion = new Cancion
            {
                Titulo = dto.Titulo,
                Genero = dto.Genero,
                Url = "/canciones/" + nombreArchivo, // Ruta pública
                FechaSubida = DateTime.UtcNow,
                UsuarioId = usuario.Id,
                ArtistaId = dto.ArtistaId
            };

            _context.Canciones.Add(cancion);
            await _context.SaveChangesAsync();

            return Ok(new { Mensaje = "Canción subida correctamente", cancion.Url });
        }

        // PUT: api/Canciones/5
        [HttpPut("{id}")]
        public async Task<IActionResult> ActualizarCancion(int id, [FromForm] CreateCancionDTOs dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var email = User.Identity?.Name;
            var usuario = _context.Usuario.FirstOrDefault(u => u.Email == email);
            if (usuario == null)
                return Unauthorized();

            var cancion = _context.Canciones.FirstOrDefault(c => c.Id == id);
            if (cancion == null)
                return NotFound("Canción no encontrada.");

            // Solo dueño o admin puede modificar
            if (usuario.Rol != "Admin" && cancion.UsuarioId != usuario.Id)
                return Forbid("No tienes permiso para actualizar esta canción.");

            // ✅ Si hay archivo nuevo, reemplazarlo
            if (dto.Url != null)
            {
                var ext = Path.GetExtension(dto.Url.FileName).ToLower();
                if (ext != ".mp3")
                    return BadRequest("Solo se permiten archivos .mp3");

                // Eliminar anterior si existe
                if (!string.IsNullOrWhiteSpace(cancion.Url))
                {
                    var rutaVieja = Path.Combine(_env.WebRootPath, cancion.Url.TrimStart('/'));
                    if (System.IO.File.Exists(rutaVieja))
                        System.IO.File.Delete(rutaVieja);
                }

                // Guardar nuevo
                var nuevoNombre = Guid.NewGuid().ToString() + ".mp3";
                var nuevaRuta = Path.Combine(_env.WebRootPath, "canciones");

                if (!Directory.Exists(nuevaRuta))
                    Directory.CreateDirectory(nuevaRuta);

                var rutaCompleta = Path.Combine(nuevaRuta, nuevoNombre);
                using (var stream = new FileStream(rutaCompleta, FileMode.Create))
                {
                    await dto.Url.CopyToAsync(stream);
                }

                cancion.Url = "/canciones/" + nuevoNombre;
            }

            // Actualizar datos
            cancion.Titulo = dto.Titulo ?? cancion.Titulo;
            cancion.Genero = dto.Genero ?? cancion.Genero;

            _context.Canciones.Update(cancion);
            await _context.SaveChangesAsync();

            return Ok("Canción actualizada correctamente.");
        }

        // GET: api/CancionApi/{id}
        [Authorize]
        [HttpGet("descargar/{id}")]
        public IActionResult DescargarCancion(int id)
        {
            var email = User.Identity?.Name;
            var usuario = _context.Usuario.FirstOrDefault(u => u.Email == email);

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

        // DELETE: api/Canciones/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> EliminarCancion(int id)
        {
            var email = User.Identity?.Name;
            var usuario = _context.Usuario.FirstOrDefault(u => u.Email == email);
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
