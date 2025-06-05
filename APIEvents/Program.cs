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
using APIEvents.Models; // ����� �� ���� ��� �����

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
            builder.Services.AddControllers(); // <-- ���� ����� �-API Controllers
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // ���� DbContext
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

            app.MapControllers();   // <- ���� ����� API Controllers
            app.MapRazorPages();    // <- ���� ������ �� �� Razor Pages

            app.Run();
        }
    }
}
