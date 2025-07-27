using HerramientasdeProgramacion.Modelos;
using Newtonsoft.Json;
using System.Text;

namespace HerramientasdeProgramacion.Consumer
{
    public static class Crud<T>
    {
        private static string BaseUrl = "https://localhost:5001/api/";

        private static string GetEndpoint()
        {
            return typeof(T) switch
            {
                var t when t == typeof(Usuario) => "usuarios",
                var t when t == typeof(Cancion) => "canciones",
                var t when t == typeof(PlayList) => "playlists",
                var t when t == typeof(PlayListCancion) => "playlistcanciones",
                var t when t == typeof(Historial) => "historial",
                var t when t == typeof(Album) => "albumes",
                var t when t == typeof(FormaPago) => "formaspago",
                var t when t == typeof(Artista) => "artistas",
                var t when t == typeof(AlbumCancion) => "albumcanciones",
                _ => throw new Exception($"No hay endpoint definido para el tipo {typeof(T).Name}")
            };
        }

        private static string FullEndpoint => $"{BaseUrl}{GetEndpoint()}";

        public static List<T> GetAll()
        {
            using var client = new HttpClient();
            var response = client.GetAsync(FullEndpoint).Result;
            var content = response.Content.ReadAsStringAsync().Result;
            if (response.IsSuccessStatusCode)
            {
                return JsonConvert.DeserializeObject<List<T>>(content);
            }
            else
            {
                throw new Exception($"Error al obtener datos: {response.StatusCode} - {content}");
            }
        }

        public static T GetById(int id)
        {
            using (var client = new HttpClient())
            {
                var response = client.GetAsync($"{FullEndpoint}/{id}").Result;
                var content = response.Content.ReadAsStringAsync().Result;
                if (response.IsSuccessStatusCode)
                {
                    return JsonConvert.DeserializeObject<T>(content);
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return default; // Retorna null si no se encuentra el elemento
                }
                else
                {
                    throw new Exception($"Error al obtener el elemento con ID {id}: {response.StatusCode} - {content}");
                }
            } 
        }

        public static T Create(T item)
        {
            using (var client = new HttpClient())
            {
                var response = client.PostAsync(
                    FullEndpoint,
                    new StringContent(
                        JsonConvert.SerializeObject(item),
                        Encoding.UTF8,
                        "application/json"
                    )
                ).Result;

                if (response.IsSuccessStatusCode)
                {
                    var json = response.Content.ReadAsStringAsync().Result;
                    return JsonConvert.DeserializeObject<T>(json);
                }
                else
                {
                    var content = response.Content.ReadAsStringAsync().Result;
                    throw new Exception($"Error: {response.StatusCode} - {content}");
                }
            }
        }

        public static bool Update(int id, T item)
        {
            using (var client = new HttpClient())
            {
                var response = client.PutAsync(
                    $"{FullEndpoint}/{id}",
                    new StringContent(
                        JsonConvert.SerializeObject(item),
                        Encoding.UTF8,
                        "application/json"
                    )
                ).Result;

                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
                else
                {
                    var content = response.Content.ReadAsStringAsync().Result;
                    throw new Exception($"Error: {response.StatusCode} - {content}");
                }
            }
        }

        public static bool Delete(int id)
        {
            using (var client = new HttpClient())
            {
                var response = client.DeleteAsync($"{FullEndpoint}/{id}").Result;
                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
                else
                {
                    var content = response.Content.ReadAsStringAsync().Result;
                    throw new Exception($"Error: {response.StatusCode} - {content}");
                }
            }
        }
    }
}
