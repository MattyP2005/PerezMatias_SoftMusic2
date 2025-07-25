using HerramientasdeProgramacion.Consumer;
using HerramientasdeProgramacion.Modelos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HerramientasdeProgramacion.MVC.Controllers
{
    public class ArtistasController : Controller
    {
        [Authorize(Roles = "Artista,Admin")]
        public IActionResult Index()
        {
            var artistas = Crud<Artista>.GetAll();
            return View(artistas);
        }

        // GET: Artistas/Details/5
        public IActionResult Details(int id)
        {
            var artista = Crud<Artista>.GetById(id);
            if (artista == null)
            {
                return NotFound();
            }
            return View(artista);
        }

        // GET: Artistas/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Artistas/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Artista artista)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                Crud<Artista>.Create(artista);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(artista);
            }
        }

        // GET: Artistas/Edit/5
        public ActionResult Edit(int id)
        {
            var artista = Crud<Artista>.GetById(id);
            return View(artista);
        }

        // POST: Artistas/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, Artista artista)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                Crud<Artista>.Update(id, artista);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(artista);
            }
        }

        // GET: Artistas/Delete/5
        public ActionResult Delete(int id)
        {
            var artista = Crud<Artista>.GetById(id);
            return View(artista);
        }

        // POST: Artista/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, Artista artista)
        {
            try
            {
                Crud<Artista>.Delete(id);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(artista);
            }
        }
    }
}
