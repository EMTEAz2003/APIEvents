//namespace APIEvents;

//public class Program
//{
//    public static void Main(string[] args)
//    {
//        var builder = WebApplication.CreateBuilder(args);
//        builder.AddServiceDefaults();

//        // Add services to the container.
//        builder.Services.AddRazorPages();

//        var app = builder.Build();

//        app.MapDefaultEndpoints();

//        // Configure the HTTP request pipeline.
//        if (!app.Environment.IsDevelopment())
//        {
//            app.UseExceptionHandler("/Error");
//        }
//        app.UseStaticFiles();

//        app.UseRouting();

//        app.UseAuthorization();

//        app.MapRazorPages();

//        app.Run();
//    }
//}
using Microsoft.EntityFrameworkCore;
using APIEvents.Models; // תעדכן אם צריך לפי הנתיב

namespace APIEvents
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.AddServiceDefaults();

            // Add services to the container.
            builder.Services.AddRazorPages();
            builder.Services.AddControllers(); // <-- הוסף תמיכה ב-API Controllers
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // הגדר DbContext
            builder.Services.AddDbContext<EventsContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("EventsDb")));

            var app = builder.Build();

            app.MapDefaultEndpoints();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
            }

            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.MapControllers();   // <- אפשר להריץ API Controllers
            app.MapRazorPages();    // <- אפשר להמשיך גם עם Razor Pages

            app.Run();
        }
    }
}
