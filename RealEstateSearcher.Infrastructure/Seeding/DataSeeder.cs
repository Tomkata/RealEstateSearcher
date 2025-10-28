using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RealEstateSearcher.Core.Models;
using RealEstateSearcher.Infrastructure;
using RealEstateSearcher.Infrastructure.Dtos;
using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Xml;
using System.Xml.Schema;
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
            Formatting =  Formatting.Indented
        };

        var jsonProperties = JsonConvert.DeserializeObject<ICollection<PropertyDto>>(json,options);

        if (!jsonProperties.Any() || jsonProperties == null)
        {
            _logger.LogWarning("No data found in JSON file.");
            return;
        }

        //Quarter Insert
        var uniqueQuarters = jsonProperties
            .Select(x => x.Quarter)
            .Where(x=>!string.IsNullOrWhiteSpace(x))
            .Distinct()
            .ToList();


        var quarterDic = new Dictionary<string, Quarter>();

        foreach (var quarterName in uniqueQuarters)
        {
            var queater = new Quarter() { Name = quarterName };

            _context.Quarters.Add(queater);

            quarterDic[quarterName] = queater;
        }
        _context.SaveChanges();

        //BuildingType Insert

        var uniqueBuildingType = jsonProperties
            .Select(x => x.BuildingType)
            .Where(x => !string.IsNullOrWhiteSpace(x) && x != null)
            .Distinct()
            .ToList();

        var buildingTypeDic = new Dictionary<string, BuildingType>();

        foreach (var buildingTypeName in uniqueBuildingType)
        {
            var buildingType = new BuildingType() { Name =  buildingTypeName };

            _context.BuildingTypes.Add(buildingType);

            buildingTypeDic[buildingTypeName] = buildingType;
        }
        _context.SaveChanges();

        var properties = new List<Property>();

        foreach (var dto in jsonProperties)
        {
            if (!quarterDic.ContainsKey(dto.Quarter))
            {
                _logger.LogWarning("Quarter not found: {Quarter}", dto.Quarter);
                continue; 
            }

            properties.Add(new Property
            {
                 Title = dto.Title ?? "Без заглавие",
                 Price = dto.Price,
                 Area = dto.Area,
                 Floor = dto.Floor,
                 QuarterId = quarterDic[dto.Quarter].Id,
                 BuildingTypeId = !string.IsNullOrWhiteSpace(buildingTypeDic[dto.BuildingType].Name) 
                 && buildingTypeDic.ContainsKey(dto.BuildingType)  
                 ? buildingTypeDic[dto.BuildingType].Id
                 : null
            });
        }

        _context.Properties.AddRange(properties);
        _context.SaveChanges();
        _logger.LogInformation("Added {Count} properties", properties.Count);

        _logger.LogInformation("Seeding completed successfully!");

    }
}