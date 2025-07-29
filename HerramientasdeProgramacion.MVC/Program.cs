using HerramientasdeProgramacion.API.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

namespace HerramientasdeProgramacion.MVC
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddDbContext<SqlServerHdPDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("SqlServerConnection")));

            builder.Services.AddHttpClient();
            // Add services to the container.
            builder.Services.AddControllersWithViews();

            // Agregar autenticaci�n con cookies
            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/Usuario/Login";  
                    options.AccessDeniedPath = "/Usuario/AccesoDenegado"; 
                });

            builder.Services.AddAuthorization();
            builder.Services.AddSession();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Usuario}/{action=Login}/{id?}");

            app.Run();
        }
    }
}
