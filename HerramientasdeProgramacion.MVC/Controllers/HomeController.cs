using HerramientasdeProgramacion.API.Data;
using HerramientasdeProgramacion.Modelos.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HerramientasdeProgramacion.MVC.Controllers
{
    [Authorize(Roles = "Usuario,Artista,Admin")]
    public class HomeController : Controller
    {
        private readonly SqlServerHdPDbContext _context;

        public HomeController(SqlServerHdPDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var model = new HomeViewModel
            {
                Albums = _context.Albumes
                    .Include(a => a.AlbumesCanciones)
                        .ThenInclude(ac => ac.Cancion)
                            .ThenInclude(c => c.Artista)
                    .ToList(),

                PlaylistsPublicas = _context.PlayLists
                    .Where(p => p.EsPublica)
                    .Include(p => p.Canciones)
                        .ThenInclude(pc => pc.Cancion)
                            .ThenInclude(c => c.Artista)
                    .ToList(),

                CancionesSueltas = _context.Canciones
                    .Include(c => c.Artista)
                    .Where(c => !c.AlbumesCanciones.Any())
                    .OrderByDescending(c => c.FechaSubida)
                    .Take(20) // Limitar a las 20 canciones más recientes
                    .ToList(),
            };

            return View(model);
        }
    }
}
