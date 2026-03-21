using Microsoft.EntityFrameworkCore;
using TerrariaCompendium.Areas.Identity.Data;
using TerrariaCompendium.Data;

namespace TerrariaCompendium
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var connectionString = builder.Configuration.GetConnectionString("TerrariaCompendiumContextConnection") ?? throw new InvalidOperationException("Connection string 'TerrariaCompendiumContextConnection' not found.");;

            builder.Services.AddDbContext<TerrariaCompendiumContext>(options => options.UseSqlServer(connectionString));

            builder.Services.AddDefaultIdentity<TerrariaCompendiumUser>().AddEntityFrameworkStores<TerrariaCompendiumContext>();

            builder.Services.AddControllersWithViews();

            var app = builder.Build();

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseRouting();

            app.UseAuthorization();

            app.MapStaticAssets();
            app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}").WithStaticAssets();
            app.MapRazorPages();

            app.Run();
        }
    }
}
