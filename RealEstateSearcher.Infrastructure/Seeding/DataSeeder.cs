using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RealEstateSearcher.Core.Models;
using RealEstateSearcher.Infrastructure;
using RealEstateSearcher.Infrastructure.Seeding;
using System.Globalization;

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
            _logger.LogError("File not found: {Path}", path);
            return;
        }

        var json = File.ReadAllText(path);

        var options = new JsonSerializerSettings
        {
            Culture = CultureInfo.InvariantCulture,
            Formatting = Formatting.Indented,
        };

        var jsonProperties = JsonConvert.DeserializeObject<List<PropertyDto>>(json, options);

        if (jsonProperties == null || !jsonProperties.Any())
        {
            _logger.LogWarning("No data in JSON file.");
            return;
        }

        // 1. Добавяме квартали
        var uniqueQuarters = jsonProperties
            .Select(p => p.Quarter)
            .Where(q => !string.IsNullOrWhiteSpace(q))
            .Distinct()
            .ToList();

        var quarterDict = new Dictionary<string, Quarter>();
        foreach (var quarterName in uniqueQuarters)
        {
            var quarter = new Quarter { Name = quarterName };
            _context.Quarters.Add(quarter);
            quarterDict[quarterName] = quarter;
        }
        _context.SaveChanges();
        _logger.LogInformation("Added {Count} quarters", quarterDict.Count);

        // 2. Добавяме типове сгради
        var uniqueBuildingTypes = jsonProperties
            .Select(p => p.BuildingType)
            .Where(bt => !string.IsNullOrWhiteSpace(bt))
            .Distinct()
            .ToList();

        var buildingTypeDict = new Dictionary<string, BuildingType>();
        foreach (var typeName in uniqueBuildingTypes)
        {
            var buildingType = new BuildingType { Name = typeName };
            _context.BuildingTypes.Add(buildingType);
            buildingTypeDict[typeName] = buildingType;
        }
        _context.SaveChanges();
        _logger.LogInformation("Added {Count} building types", buildingTypeDict.Count);

        // 3. Добавяме имоти
        var properties = new List<Property>();
        foreach (var dto in jsonProperties)
        {
            if (!quarterDict.ContainsKey(dto.Quarter))
                continue;

            var property = new Property
            {
                Title = dto.Title ?? "Без заглавие",
                Price = dto.Price,
                Area = dto.Area,
                Floor = dto.Floor,
                TotalFloors = dto.TotalFloors,
                QuarterId = quarterDict[dto.Quarter].Id,
                BuildingTypeId = !string.IsNullOrWhiteSpace(dto.BuildingType) && buildingTypeDict.ContainsKey(dto.BuildingType)
                    ? buildingTypeDict[dto.BuildingType].Id
                    : null
            };

            properties.Add(property);
        }

        _context.Properties.AddRange(properties);
        _context.SaveChanges();
        _logger.LogInformation("Added {Count} properties", properties.Count);

        _logger.LogInformation("Seeding completed successfully!");
    }
}