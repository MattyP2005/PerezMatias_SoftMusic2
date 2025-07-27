using HerramientasdeProgramacion.Consumer;
using HerramientasdeProgramacion.Modelos;
using HerramientasdeProgramacion.Modelos.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace HerramientasdeProgramacion.MVC.Controllers
{
    [Authorize(Roles = "Usuario,Admin")]
    public class UsuarioController : Controller
    {
        public IActionResult Index()
        {
            var usuarios = Crud<Usuario>.GetAll();
            return View(usuarios);
        }

        // Vista de inicio de sesión
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        /* Procesa el inicio de sesión
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

            var identity = new ClaimsIdentity(claims, "login");
            await HttpContext.SignInAsync(new ClaimsPrincipal(identity));

            return RedirectToAction("Index", "Home");
        }*/

        // GET: Usuario/Details/5
        public ActionResult Details(int id)
        {
            var usuarios = Crud<Usuario>.GetById(id);
            if (usuarios == null)
            {
                return NotFound();
            }
            return View(usuarios);
        }

        // GET: Usuario/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Usuario/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Usuario usuario)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                Crud<Usuario>.Create(usuario);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(usuario);
            }
        }

        // GET: Usuario/Edit/5
        public ActionResult Edit(int id)
        {
            var usuario = Crud<Usuario>.GetById(id);
            if (usuario == null)
            {
                return NotFound();
            }
            return View(usuario);
        }

        // POST: Usuario/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, Usuario usuario)
        {
            if (id != usuario.Id)
            {
                return BadRequest();
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                Crud<Usuario>.Update(id, usuario);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(usuario);
            }
        }

        // GET: Usuario/Delete/5
        public ActionResult Delete(int id)
        {
            var usuario = Crud<Usuario>.GetById(id);
            if (usuario == null)
            {
                return NotFound();
            }
            return View(usuario);
        }

        // POST: Usuario/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id, Usuario usuario)
        {
            var usuarios = Crud<Usuario>.GetById(id);
            if (usuario == null)
            {
                return NotFound();
            }
            try
            {
                Crud<Usuario>.Delete(id);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(usuario);
            }
        }
    }
}
