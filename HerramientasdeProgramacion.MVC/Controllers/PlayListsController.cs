using HerramientasdeProgramacion.Consumer;
using HerramientasdeProgramacion.Modelos;
using Microsoft.AspNetCore.Mvc;

namespace HerramientasdeProgramacion.MVC.Controllers
{
    public class PlayListsController : Controller
    {
        public IActionResult Index()
        {
            var playLists = Crud<PlayList>.GetAll();
            return View(playLists);
        }

        // GET: PlayLists/Details/5
        public IActionResult Details(int id)
        {
            var playList = Crud<PlayList>.GetById(id);
            if (playList == null)
            {
                return NotFound();
            }
            return View(playList);
        }

        // GET: PlayLists/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: PlayLists/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(PlayList playList)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                Crud<PlayList>.Create(playList);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(playList);
            }
        }

        // GET: PlayLists/Edit/5
        public ActionResult Edit(int id)
        {
            var playList = Crud<PlayList>.GetById(id);
            if (playList == null)
            {
                return NotFound();
            }
            return View(playList);
        }

        // POST: PlayLists/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, PlayList playList)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                Crud<PlayList>.Update(id, playList);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(playList);
            }
        }

        // GET: PlayLists/Delete/5
        public ActionResult Delete(int id)
        {
            var playList = Crud<PlayList>.GetById(id);
            if (playList == null)
            {
                return NotFound();
            }
            return View(playList);
        }

        // POST: PlayLists/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            try
            {
                Crud<PlayList>.Delete(id);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View();
            }
        }

        // GET: Agregar Canción a Playlist
        public IActionResult AgregarCancion(int playlistId)
        {
            var playlist = Crud<PlayList>.GetById(playlistId);
            if (playlist == null)
            {
                return NotFound();
            }
            var canciones = Crud<Cancion>.GetAll();
            ViewBag.PlaylistId = playlistId;
            return View(canciones);
        }

        // POST: Quitar Canción de Playlist
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AgregarCancion(int playlistId, int cancionId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var playListCancion = new PlayListCancion
                {
                    PlaylistId = playlistId,
                    CancionId = cancionId
                };
                Crud<PlayListCancion>.Create(playListCancion);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View();
            }
        }

        // GET: Quitar Canción de Playlist
        public IActionResult QuitarCancion(int playlistId, int cancionId)
        {
            var playListCancion = Crud<PlayListCancion>.GetAll()
                .FirstOrDefault(plc => plc.PlaylistId == playlistId && plc.CancionId == cancionId);
            if (playListCancion == null)
            {
                return NotFound();
            }
            return View(playListCancion);
        }

        // POST: Quitar Canción de Playlist
        [HttpPost, ActionName("QuitarCancion")]
        [ValidateAntiForgeryToken]
        public IActionResult QuitarCancionConfirmed(int playlistId, int cancionId)
        {
            try
            {
                var playListCancion = Crud<PlayListCancion>.GetAll()
                    .FirstOrDefault(plc => plc.PlaylistId == playlistId && plc.CancionId == cancionId);
                if (playListCancion != null)
                {
                    Crud<PlayListCancion>.Delete(playListCancion.PlaylistId);
                }
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View();
            }
        }
    }
}
