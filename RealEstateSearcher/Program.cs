using Microsoft.EntityFrameworkCore;
using RealEstateSearcher.Infrastructure;
using RealEstateSearcher.Infrastructure.Dtos;
using RealEstateSearcher.Services.Interfaces;
using RealEstateSearcher.Services.Services;


var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<RealEstateDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IPropertyService, PropertyService>();

var app = builder.Build();

// Seed данните при първо стартиране
await SeedDatabaseAsync(app);

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

// Метод за Database Seeding
static async Task SeedDatabaseAsync(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();

    try
    {
        logger.LogInformation(" Starting database initialization...");

        var context = services.GetRequiredService<RealEstateDbContext>();

        // Прилагаме миграции
        logger.LogInformation("Applying database migrations...");
        await context.Database.MigrateAsync();
        logger.LogInformation(" Migrations applied successfully");

        // Seed данните
        logger.LogInformation("Starting data seeding...");
        var seederLogger = services.GetRequiredService<ILogger<DatabaseSeeder>>();
        var seeder = new DatabaseSeeder(context, seederLogger);
        seeder.Seed();
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred during database initialization");
        // Не спираме апликацията, само логваме грешката
    }
} 