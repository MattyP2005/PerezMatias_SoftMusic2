using HerramientasdeProgramacion.API.Data;
using HerramientasdeProgramacion.Modelos;
using HerramientasdeProgramacion.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using HerramientasdeProgramacion.API.Controllers.DTOs;

namespace HerramientasdeProgramacion.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsuariosController : ControllerBase
    {
        private readonly SqlServerHdPDbContext _context;
        private readonly IConfiguration _config;
        private readonly IWebHostEnvironment _env;

        public UsuariosController(SqlServerHdPDbContext context, IConfiguration config, IWebHostEnvironment env)
        {
            _context = context;
            _config = config;
            _env = env;
        }

        // GET: api/UsuarioApi
        [HttpGet]
        public IActionResult ObtenerUsuarios()
        {
            var usuarios = _context.Usuarios.ToList();
            return Ok(usuarios);
        }

        // GET: api/UsuarioApi/protegido
        [Authorize]
        [HttpGet("protegido")]
        public IActionResult VerSoloAutenticados()
        {
            return Ok("Solo puedes ver esto si tienes un token válido.");
        }

        // POST: api/UsuarioApi
        [HttpPost]
        public IActionResult CrearUsuario([FromBody] CreateUsuarioDTOs dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var usuario = new Usuario
            {
                Email = dto.Email,
                PasswordHash = dto.PasswordHash,
                Rol = dto.Rol,
                Plan = dto.Plan
            };

            _context.Usuarios.Add(usuario);
            _context.SaveChanges();

            return Ok("Usuario creado");
        }

        /* POST: api/UsuarioApi/login
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginDTO login)
        {
            var usuario = _context.Usuarios.SingleOrDefault(u =>
                u.Email == login.Email && u.PasswordHash == login.PasswordHash);

            if (usuario == null)
                return Unauthorized("Credenciales inválidas.");

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, usuario.Email),
                new Claim(ClaimTypes.Role, usuario.Rol)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);


            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(2),
                signingCredentials: creds
            );

            return Ok(new
            {
                token = new JwtSecurityTokenHandler().WriteToken(token)
            });
        }*/

        // POST: api/UsuarioApi/login
        [Authorize]
        [HttpGet("plan")]
        public IActionResult VerPlan()
        {
            var email = User.Identity?.Name;
            var usuario = _context.Usuarios.FirstOrDefault(u => u.Email == email);

            if (usuario == null)
                return Unauthorized();

            var plan = usuario.Plan;

            var info = new
            {
                Plan = plan,
                PrecioMensual = PlanServices.GetPrecioMensual(plan),
                MaxDispositivos = PlanServices.GetMaxDispositivos(plan),
                PermiteDescargas = PlanServices.PermiteDescargas(plan)
            };

            return Ok(info);
        }

        // DELETE: api/UsuarioApi/5
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public IActionResult EliminarUsuario(int id)
        {
            var usuario = _context.Usuarios
                .Include(u => u.PlayLists)
                .Include(u => u.FormasPagos)
                .FirstOrDefault(u => u.Id == id);

            if (usuario == null)
                return NotFound("Usuario no encontrado.");

            // 1. Eliminar historial
            var historial = _context.Historiales.Where(h => h.UsuarioId == id);
            _context.Historiales.RemoveRange(historial);

            // 2. Eliminar canciones del usuario
            var canciones = _context.Canciones.Where(c => c.UsuarioId == id).ToList();
            foreach (var c in canciones)
            {
                // Eliminar archivo físico
                var ruta = Path.Combine(_env.WebRootPath, c.Url.TrimStart('/'));
                if (System.IO.File.Exists(ruta))
                    System.IO.File.Delete(ruta);
            }
            _context.Canciones.RemoveRange(canciones);

            // 4. Eliminar playlists del usuario
            var playlists = _context.PlayLists
                .Include(p => p.Canciones)
                .Where(p => p.UsuarioId == id)
                .ToList();

            foreach (var p in playlists)
            {
                _context.PlayListsCanciones.RemoveRange(p.Canciones);
            }
            _context.PlayLists.RemoveRange(playlists);

            // 5. Eliminar formas de pago
            var formasPago = _context.FormasPagos.Where(f => f.UsuarioId == id);
            _context.FormasPagos.RemoveRange(formasPago);

            // 6. Finalmente, el usuario
            _context.Usuarios.Remove(usuario);

            _context.SaveChanges();

            return Ok("Usuario y todos sus datos han sido eliminados correctamente.");
        }
    }
}
