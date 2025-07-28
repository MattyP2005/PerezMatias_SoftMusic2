using HerramientasdeProgramacion.API.Data;
using HerramientasdeProgramacion.Modelos;
using HerramientasdeProgramacion.Modelos.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace HerramientasdeProgramacion.MVC.Controllers
{
    public class UsuariosController : Controller
    {
        private readonly SqlServerHdPDbContext _context;

        public UsuariosController(SqlServerHdPDbContext context)
        {
            _context = context;
        }

        // Vista de inicio de sesión
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // Procesa el inicio de sesión
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var usuario = _context.Usuarios
                .FirstOrDefault(u => u.Email == model.Email && u.PasswordHash == model.Password);

            if (usuario == null)
            {
                ModelState.AddModelError("", "Credenciales incorrectas");
                return View(model);
            }

            // Iniciar sesión usando cookie (simple)
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, usuario.Email),
                new Claim("Rol", usuario.Rol),
                new Claim(ClaimTypes.Role, usuario.Rol),
                new Claim("Plan", usuario.Plan)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            return RedirectToAction("Index", "Home");
        }

        // Vista del cierre de sesión
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            return RedirectToAction("Login");
        }

        // Procesa el cierre de sesión
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult LogoutConfirmed()
        {
            HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.Session.Clear();
            TempData["Mensaje"] = "Has cerrado sesión correctamente.";
            return RedirectToAction("Login", "Usuarios");
        }

        // Vista de registro
        [HttpGet]
        public IActionResult Registrar()
        {
            return View();
        }

        // Procesa el registro
        [HttpPost]
        public IActionResult Registrar(RegistroViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var usuario = new Usuario
            {
                Nombre = model.Nombre,
                Email = model.Email,
                PasswordHash = model.Password,
                ImagenPerfilUrl = "/img/sin-perfil.png",
                Plan = model.Plan ?? "Gratis",
                Rol = "Usuarios" // Asignar rol por defecto
            };

            _context.Usuarios.Add(usuario);
            _context.SaveChanges();

            if (model.Plan == "Premium")
            {
                return RedirectToAction("ElegirSubPlan", "Usuarios", new { id = usuario.Id });
            }

            return RedirectToAction("Index", "Home");
        }

        // Vista de selección de sub-plan
        [HttpGet]
        public IActionResult ElegirSubPlan(int id)
        {
            var model = new ElegirSubPlanViewModel { UsuarioId = id };
            return View(model);
        }

        // Procesa la selección de sub-plan
        [HttpPost]
        public IActionResult ElegirSubPlan(ElegirSubPlanViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var usuario = _context.Usuarios.Find(model.UsuarioId);
            if (usuario == null)
                return NotFound();

            // Guardamos el sub-plan como parte del Plan
            usuario.Plan = "Premium-" + model.SubPlan;
            _context.SaveChanges();

            // Guardamos info en TempData para mostrarla en la vista de pago
            TempData["SubPlanInfo"] = $"Premium - {model.SubPlan} (${(model.SubPlan == "Personal" ? "9.99" :
                model.SubPlan == "Familiar" ? "3.5" :
                model.SubPlan == "Empresarial" ? "35" : "0")}/mes)";

            return RedirectToAction("MetodoPago", "Usuarios", new { id = usuario.Id });
        }

        // Vista de métodos de pago
        [HttpGet]
        public IActionResult MetodoPago(int id)
        {
            var model = new MetodoPagoViewModel { UsuarioId = id };
            return View(model);
        }

        // Procesa el método de pago
        [HttpPost]
        public async Task<IActionResult> MetodoPago(MetodoPagoViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var usuario = _context.Usuarios.Find(model.UsuarioId);
            if (usuario == null)
                return NotFound();

            var formaPago = new FormaPago
            {
                UsuarioId = usuario.Id,
                Tipo = model.TipoTarjeta,
                Detalles = $"**** **** **** {model.NumeroTarjeta[^4..]}", // muestra solo los últimos 4 dígitos
                FechaRegistro = DateTime.Now
            };

            _context.FormasPagos.Add(formaPago);
            _context.SaveChanges();

            TempData["Mensaje"] = $"Gracias por suscribirte al plan {usuario.Plan}!";

            // Iniciar sesión automáticamente (si aún no está logueado)
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, usuario.Email),
                new Claim("Rol", usuario.Rol),
                new Claim("Plan", usuario.Plan),
                new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            };

            var identity = new ClaimsIdentity(claims, "login");
            await HttpContext.SignInAsync(new ClaimsPrincipal(identity));

            return RedirectToAction("MiPerfil", "Usuarios");

        }


        // Vista de perfil del usuario
        [Authorize(Roles = "Usuario,Admin,Artista")]
        public IActionResult MiPerfil()
        {
            var email = User.Identity?.Name;

            if (string.IsNullOrEmpty(email))
                return Unauthorized();

            var usuario = _context.Usuarios
                .Include(u => u.FormaPago)
                .Include(u => u.Albumes)
                .Include(u => u.Historiales)
                    .ThenInclude(h => h.Cancion)
                        .ThenInclude(c => c.Artista)
                .FirstOrDefault(u => u.Email == email);

            if (usuario == null)
                return NotFound();

            return View(usuario);
        }

        // Cambiar plan de suscripción
        [Authorize(Roles = "Usuario,Admin,Artista")]
        [HttpGet]
        public IActionResult CambiarPlan()
        {
            var email = User.Identity?.Name;
            var usuario = _context.Usuarios.FirstOrDefault(u => u.Email == email);

            if (usuario == null)
                return NotFound();

            if (usuario.Plan == "Gratis")
            {
                usuario.Plan = "Premium";
                _context.SaveChanges();
                return RedirectToAction("MetodoPago", "Usuarios", new { id = usuario.Id });
            }
            else
            {
                // Si tenía una forma de pago, la eliminamos al pasar a Gratis
                var pago = _context.FormasPagos.FirstOrDefault(p => p.UsuarioId == usuario.Id);
                if (pago != null)
                {
                    _context.FormasPagos.Remove(pago);
                }

                usuario.Plan = "Gratis";
                _context.SaveChanges();
                return RedirectToAction("MiPerfil");
            }
        }

        // Editar perfil del usuario
        [HttpGet]
        public IActionResult EditarPerfil()
        {
            var email = User.Identity?.Name;

            if (string.IsNullOrEmpty(email))
                return Unauthorized();

            var usuario = _context.Usuarios.FirstOrDefault(u => u.Email == email);
            if (usuario == null)
                return NotFound();

            var model = new EditarPerfilViewModel
            {
                Id = usuario.Id,
                Nombre = usuario.Nombre,
                ImagenPerfilUrl = usuario.ImagenPerfilUrl
            };

            return View(model);
        }

        // Procesa la edición del perfil
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarPerfil(EditarPerfilViewModel model, IFormFile imagenPerfil)
        {
            if (!ModelState.IsValid)
                return View(model);

            var email = User.Identity?.Name;
            var usuario = _context.Usuarios.FirstOrDefault(u => u.Email == email);

            if (usuario == null)
                return NotFound();

            usuario.Nombre = model.Nombre;

            // Eliminar imagen si se marca la casilla
            if (model.EliminarImagen && !string.IsNullOrEmpty(usuario.ImagenPerfilUrl))
            {
                var rutaImagen = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", usuario.ImagenPerfilUrl.TrimStart('/'));
                if (System.IO.File.Exists(rutaImagen))
                    System.IO.File.Delete(rutaImagen);

                usuario.ImagenPerfilUrl = null;
            }

            // Subir nueva imagen
            if (model.ImagenPerfil != null && model.ImagenPerfil.Length > 0)
            {
                var carpeta = "perfiles";
                var extension = Path.GetExtension(model.ImagenPerfil.FileName);
                var nombreArchivo = Guid.NewGuid().ToString() + extension;
                var ruta = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", carpeta, nombreArchivo);

                using (var stream = new FileStream(ruta, FileMode.Create))
                {
                    await model.ImagenPerfil.CopyToAsync(stream);
                }

                usuario.ImagenPerfilUrl = "/" + carpeta + "/" + nombreArchivo;
            }

            _context.SaveChanges();
            TempData["Mensaje"] = "Perfil actualizado correctamente";

            return RedirectToAction("MiPerfil");
        }

        // Eliminar cuenta del usuario
        [HttpGet]
        public IActionResult EliminarCuenta()
        {
            var email = User.Identity?.Name;
            if (string.IsNullOrEmpty(email))
                return Unauthorized();
            var usuario = _context.Usuarios.FirstOrDefault(u => u.Email == email);
            if (usuario == null)
                return NotFound();
            return View(usuario);
        }

        // Procesa la eliminación de la cuenta
        [Authorize(Roles = "Usuario,Admin,Artista")]
        [HttpPost, ActionName("EliminarCuenta")]
        public async Task<IActionResult> ConfirmarEliminarCuenta(string motivo)
        {
            var email = User.Identity?.Name;
            var usuario = _context.Usuarios
                .Include(u => u.FormaPago)
                .Include(u => u.Historiales)
                .Include(u => u.PlayLists)
                .Include(u => u.Albumes)
                .FirstOrDefault(u => u.Email == email);

            if (usuario == null)
                return NotFound();

            // Eliminar forma de pago
            if (usuario.FormaPago != null && usuario.FormaPago.Id > 0)
            {
                _context.FormasPagos.Remove(usuario.FormaPago);
            }

            // Eliminar historial
            if (usuario.Historiales != null && usuario.Historiales.Any())
            {
                _context.Historiales.RemoveRange(usuario.Historiales);
            }

            // Eliminar playlists
            if (usuario.PlayLists != null && usuario.PlayLists.Any())
            {
                _context.PlayLists.RemoveRange(usuario.PlayLists);
            }

            // Eliminar álbumes (si el usuario es artista o crea álbumes)
            if (usuario.Albumes != null && usuario.Albumes.Any())
            {
                _context.Albumes.RemoveRange(usuario.Albumes);
            }

            // Finalmente, eliminar el usuario
            _context.Usuarios.Remove(usuario);
            await _context.SaveChangesAsync();

            await HttpContext.SignOutAsync();
            TempData["MensajeEliminacion"] = "Tu cuenta fue eliminada con éxito.";
            return RedirectToAction("Login");
        }
    }
}
