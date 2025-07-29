using HerramientasdeProgramacion.API.Data;
using HerramientasdeProgramacion.Modelos.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HerramientasdeProgramacion.MVC.Controllers
{
    [Authorize(Roles = "Usuario,Artista,Admin")]
    public class HomeController : Controller
    {
        private readonly SqlServerHdPDbContext _context;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public HomeController(SqlServerHdPDbContext context, IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        public IActionResult Index()
        {
            var model = new HomeViewModel
            {
                Albums = _context.Albumes
                    .Include(a => a.AlbumesCanciones)
                        .ThenInclude(ac => ac.Cancion)
                            .ThenInclude(c => c.Usuario)
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
                    .Take(20)
                    .ToList(),
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> DescargarCancion(int id)
        {
            var apiBaseUrl = _configuration["ApiBaseUrl"];
            if (string.IsNullOrEmpty(apiBaseUrl))
                return StatusCode(500, "ApiBaseUrl no configurado.");

            var client = _httpClientFactory.CreateClient();
            client.BaseAddress = new Uri(apiBaseUrl);

            // Obtener cookie de autenticación de la petición actual y añadirla a HttpClient
            if (Request.Headers.TryGetValue("Cookie", out var cookie))
            {
                client.DefaultRequestHeaders.Add("Cookie", cookie.ToString());
            }
            else
            {
                return Unauthorized("No se encontró cookie de autenticación.");
            }

            var response = await client.GetAsync($"/api/cancionapi/descargar/{id}");

            if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                return Forbid("Tu plan no permite descargar esta canción.");

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                return Unauthorized("No autorizado para descargar la canción.");

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return NotFound("Canción no encontrada.");

            if (!response.IsSuccessStatusCode)
                return StatusCode((int)response.StatusCode, "Error al descargar la canción.");

            var contentDisposition = response.Content.Headers.ContentDisposition;
            var contentType = response.Content.Headers.ContentType?.ToString() ?? "application/octet-stream";
            var fileName = contentDisposition?.FileNameStar ?? contentDisposition?.FileName ?? $"cancion_{id}.mp3";

            var stream = await response.Content.ReadAsStreamAsync();

            return File(stream, contentType, fileName);
        }
    }
}