using HerramientasdeProgramacion.Consumer;
using HerramientasdeProgramacion.Modelos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace HerramientasdeProgramacion.MVC.Controllers
{
    [Authorize(Roles = "Artista,Admin")]
    public class AlbumesController : Controller
    {
        // GET: Albumes
        public ActionResult Index()
        {
            var userEmail = User.Identity?.Name;

            var albumes = Crud<Album>.GetAll();

            return View(albumes);
        }

        // GET: Albumes/Details/5
        public ActionResult Details(int id)
        {
            var album = Crud<Album>.GetById(id);
            if (album == null)
            {
                return NotFound();
            }
            return View(album);
        }

        // GET: Albumes/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Albumes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Album album)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var email = User.Identity?.Name;
            var artista = Crud<Usuario>.GetAll().FirstOrDefault(u => u.Email == email);
            if (artista == null)
            {
                return Unauthorized();
            }
            var albumes = new Album
            {
                Titulo = album.Titulo,
                ArtistaId = artista.Id
            };
            var createdAlbum = Crud<Album>.Create(album);
            return RedirectToAction(nameof(Index));
        }

        // GET: Albumes/Edit/5
        public ActionResult Edit(int id)
        {
            var album = Crud<Album>.GetById(id);
            if (album == null)
            {
                return NotFound();
            }
            return View(album);
        }

        // POST: Albumes/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, Album album)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var email = User.Identity?.Name;
            var artista = Crud<Usuario>.GetAll().FirstOrDefault(u => u.Email == email);
            if (artista == null)
            {
                return Unauthorized();
            }
            var existingAlbum = Crud<Album>.GetById(album.Id);
            if (existingAlbum == null)
            {
                return NotFound();
            }
            existingAlbum.Titulo = album.Titulo;
            existingAlbum.ArtistaId = artista.Id;
            Crud<Album>.Update(id, album);
            return RedirectToAction(nameof(Index));
        }

        // GET: Albumes/Delete/5
        public ActionResult Delete(int id)
        {
            var album = Crud<Album>.GetById(id);
            if (album == null)
            {
                return NotFound();
            }
            return View(album);
        }

        // POST: Albumes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id, Album album)
        {
            var albumes = Crud<Album>.GetById(id);
            if (album == null)
            {
                return NotFound();
            }
            Crud<Album>.Delete(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
