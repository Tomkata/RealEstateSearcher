using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RealEstateSearcher.Core.Models;
using RealEstateSearcher.Infrastructure;
using RealEstateSearcher.Infrastructure.Dtos;
using System.Globalization;
using Formatting = Newtonsoft.Json.Formatting;

public class DatabaseSeeder
{
    private readonly RealEstateDbContext _context;
    private readonly ILogger<DatabaseSeeder> _logger;

    public DatabaseSeeder(RealEstateDbContext context, ILogger<DatabaseSeeder> logger)
    {
        _context = context;
        _logger = logger;
    }

    public void Seed()
    {
        // Проверка дали вече има данни
        if (_context.Properties.Any())
        {
            _logger.LogInformation("Database already has data. Skipping seed.");
            return;
        }

        var path = @"C:\Users\HP\Desktop\Programming\LeetCode\RealEstateSearcher\RealEstateSearcher\imoti.json";

        if (!File.Exists(path))
        {
            _logger.LogError($"File not found: {path}");
            return;
        }

        var json = File.ReadAllText(path);

        var options = new JsonSerializerSettings()
        {
            Culture = CultureInfo.InvariantCulture,
            Formatting = Formatting.Indented
        };

        var jsonProperties = JsonConvert.DeserializeObject<ICollection<PropertyDto>>(json, options);

        if (jsonProperties == null || !jsonProperties.Any())
        {
            _logger.LogWarning("No data found in JSON file.");
            return;
        }

        _logger.LogInformation($"Found {jsonProperties.Count} properties in JSON");

        // Quarter Insert
        var uniqueQuarters = jsonProperties
            .Select(x => x.Quarter)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct()
            .ToList();

        var quarterDic = new Dictionary<string, Quarter>();

        foreach (var quarterName in uniqueQuarters)
        {
            var quarter = new Quarter() { Name = quarterName };
            _context.Quarters.Add(quarter);
            quarterDic[quarterName] = quarter;
        }

        _context.SaveChanges();
        _logger.LogInformation($"Added {uniqueQuarters.Count} quarters");

        // Building Type Insert
        var uniqueBuildingType = jsonProperties
            .Select(x => x.BuildingType)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct()
            .ToList();

        var buildingTypeDic = new Dictionary<string, BuildingType>();

        foreach (var buildingTypeName in uniqueBuildingType)
        {
            var buildingType = new BuildingType() { Name = buildingTypeName };
            _context.BuildingTypes.Add(buildingType);
            buildingTypeDic[buildingTypeName] = buildingType;
        }

        _context.SaveChanges();
        _logger.LogInformation($"Added {uniqueBuildingType.Count} building types");

        // Properties Insert
        var properties = new List<Property>();
        int imageCount = 0;

        foreach (var dto in jsonProperties)
        {
            if (!quarterDic.ContainsKey(dto.Quarter))
            {
                _logger.LogWarning("Quarter not found: {Quarter}", dto.Quarter);
                continue;
            }

            var property = new Property
            {
                Title = dto.Title ?? "Без заглавие",
                Price = dto.Price,
                Area = dto.Area,
                Floor = dto.Floor,
                TotalFloors = dto.TotalFloors > 0 ? dto.TotalFloors : 1,
                QuarterId = quarterDic[dto.Quarter].Id,
                BuildingTypeId = !string.IsNullOrWhiteSpace(dto.BuildingType) && buildingTypeDic.ContainsKey(dto.BuildingType)
                    ? buildingTypeDic[dto.BuildingType].Id
                    : null,
                ImageUrl = dto.ImageUrl ?? dto.Images?.FirstOrDefault(),  // Главна снимка
                Images = new List<PropertyImage>()
            };

            // ✅ Добави всички снимки с правилното property name: ImageUrl
            if (dto.Images != null && dto.Images.Any())
            {
                int order = 0;
                foreach (var imgUrl in dto.Images)
                {
                    if (!string.IsNullOrWhiteSpace(imgUrl))
                    {
                        property.Images.Add(new PropertyImage
                        {
                            ImageUrl = imgUrl,  
                            Order = order++,
                            PropertyId = property.Id
                        });
                        imageCount++;
                    }
                }
            }

            properties.Add(property);
        }

        _context.Properties.AddRange(properties);
        _context.SaveChanges();

        _logger.LogInformation($"✅ Added {properties.Count} properties");
        _logger.LogInformation($"📸 Added {imageCount} images");
        _logger.LogInformation("Seeding completed successfully!");
    }
}