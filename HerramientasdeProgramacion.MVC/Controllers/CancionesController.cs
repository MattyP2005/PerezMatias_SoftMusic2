using HerramientasdeProgramacion.Consumer;
using HerramientasdeProgramacion.Modelos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HerramientasdeProgramacion.MVC.Controllers
{
    [Authorize(Roles = "Artista,Admin")]
    public class CancionesController : Controller
    {
        public IActionResult Index()
        {
            var canciones = Crud<Cancion>.GetAll();
            return View(canciones);
        }

        // GET: Canciones/Details/5
        public ActionResult Details(int id)
        {
            var canciones = Crud<Cancion>.GetById(id);
            return View(canciones);
        }

        // GET: Canciones/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Canciones/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Cancion cancion)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                Crud<Cancion>.Create(cancion);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(cancion);
            }
        }

        // GET: Canciones/Edit/5
        public ActionResult Edit(int id)
        {
            var cancion = Crud<Cancion>.GetById(id);
            return View(cancion);
        }

        // POST: Canciones/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id,  Cancion cancion)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                Crud<Cancion>.Update(id, cancion);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(cancion);
            }
        }

        // GET: Canciones/Delete/5
        public ActionResult Delete(int id)
        {
            var cancion = Crud<Cancion>.GetById(id);
            return View(cancion);
        }

        // POST: Canciones/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, Cancion cancion)
        {
            try
            {
                Crud<Cancion>.Delete(id);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(cancion);
            }
        }
    }
}
