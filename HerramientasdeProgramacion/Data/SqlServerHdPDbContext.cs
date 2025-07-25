using HerramientasdeProgramacion.Modelos;
using Microsoft.EntityFrameworkCore;

namespace HerramientasdeProgramacion.API.Data
{
    public class SqlServerHdPDbContext : DbContext
    {
        public SqlServerHdPDbContext(DbContextOptions<SqlServerHdPDbContext> options) : base(options) { }

        public DbSet<Usuario> Usuarios { get; set; }

        public DbSet<Cancion> Canciones { get; set; }

        public DbSet<PlayList> PlayLists { get; set; }

        public DbSet<PlayListCancion> PlayListsCanciones { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Historial → Usuario y Canción
            modelBuilder.Entity<Historial>()
                .HasOne(h => h.Usuario)
                .WithMany()
                .HasForeignKey(h => h.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Historial>()
                .HasOne(h => h.Cancion)
                .WithMany()
                .HasForeignKey(h => h.CancionId)
                .OnDelete(DeleteBehavior.Restrict);

            // Cancion → Usuario
            modelBuilder.Entity<Cancion>()
                .HasOne(c => c.Usuario)
                .WithMany()
                .HasForeignKey(c => c.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);

            // PlaylistCancion (composite key)
            modelBuilder.Entity<PlayListCancion>()
                .HasKey(pc => new { pc.PlaylistId, pc.CancionId });

            modelBuilder.Entity<PlayListCancion>()
                .HasOne(pc => pc.PlayList)
                .WithMany(p => p.Canciones)
                .HasForeignKey(pc => pc.PlaylistId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PlayListCancion>()
                .HasOne(pc => pc.Cancion)
                .WithMany()
                .HasForeignKey(pc => pc.CancionId)
                .OnDelete(DeleteBehavior.Restrict);

            // Playlist → Usuario
            modelBuilder.Entity<PlayList>()
                .HasOne(p => p.Usuario)
                .WithMany()
                .HasForeignKey(p => p.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);

            // FormaPago
            modelBuilder.Entity<FormaPago>()
                .HasOne(fp => fp.Usuario)
                .WithMany()
                .HasForeignKey(fp => fp.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);

            // Usuario → FormasPago
            // Relación 1 Usuario -> muchas FormasPago
            modelBuilder.Entity<FormaPago>()
                .HasOne(fp => fp.Usuario)
                .WithMany(u => u.FormasPagos)
                .HasForeignKey(fp => fp.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);

            // Álbum → Artista (usuario)
            modelBuilder.Entity<Album>()
                .HasOne(a => a.Artista)
                .WithMany()
                .HasForeignKey(a => a.ArtistaId);

            // Cancion → Álbum
            modelBuilder.Entity<AlbumCancion>()
                .HasKey(ac => new { ac.AlbumId, ac.CancionId });

            modelBuilder.Entity<AlbumCancion>()
                .HasOne(ac => ac.Album)
                .WithMany(a => a.AlbumesCanciones)
                .HasForeignKey(ac => ac.AlbumId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<AlbumCancion>()
                .HasOne(ac => ac.Cancion)
                .WithMany(c => c.AlbumesCanciones)
                .HasForeignKey(ac => ac.CancionId)
                .OnDelete(DeleteBehavior.Restrict);

        }

        public DbSet<Historial> Historiales { get; set; }

        public DbSet<Album> Albumes { get; set; }

        public DbSet<FormaPago> FormasPagos { get; set; }

        public DbSet<Artista> Artistas { get; set; }

        public DbSet<AlbumCancion> AlbumesCanciones { get; set; }

        public DbSet<Usuario> Suscripciones { get; set; }
    }
}
