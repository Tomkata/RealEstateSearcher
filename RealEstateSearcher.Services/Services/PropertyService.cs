using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RealEstateSearcher.Core.Models;
using RealEstateSearcher.Infrastructure;
using RealEstateSearcher.Services.Dtos;
using RealEstateSearcher.Services.Interfaces;

namespace RealEstateSearcher.Services.Services
{
    public class PropertyService : IPropertyService
    {
        private readonly RealEstateDbContext _context;
        private readonly ILogger<PropertyService> _logger;

        public PropertyService(RealEstateDbContext context, ILogger<PropertyService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Property> AddPropertyAsync(
            string title,
            decimal price,
            int area,
            int floor,
            int totalFloors,
            string quarterName,
            string? buildingTypeName)
        {
            _logger.LogInformation("Adding new property: {Title} in {Quarter}", title, quarterName);

            var quarter = await _context.Quarters
                .FirstOrDefaultAsync(x => x.Name == quarterName);

            if (quarter == null)
            {
                _logger.LogInformation("Quarter {QuarterName} not found, creating new one", quarterName);
                quarter = new Quarter { Name = quarterName };
                _context.Quarters.Add(quarter);
                await _context.SaveChangesAsync();
            }

            Guid? buildingTypeId = null;
            if (!string.IsNullOrWhiteSpace(buildingTypeName))
            {
                var buildingType = await _context.BuildingTypes
                    .FirstOrDefaultAsync(x => x.Name == buildingTypeName);

                if (buildingType == null)
                {
                    _logger.LogInformation("BuildingType {BuildingType} not found, creating new one", buildingTypeName);
                    buildingType = new BuildingType { Name = buildingTypeName };
                    _context.BuildingTypes.Add(buildingType);
                    await _context.SaveChangesAsync();
                }

                buildingTypeId = buildingType.Id;
            }

            var property = new Property
            {
                Title = title,
                Price = price,
                Area = area,
                Floor = floor,
                TotalFloors = totalFloors,
                QuarterId = quarter.Id,
                BuildingTypeId = buildingTypeId
            };

            await _context.Properties.AddAsync(property);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Property {PropertyId} created successfully", property.Id);

            await _context.Entry(property)
                .Reference(p => p.Quarter)
                .LoadAsync();

            if (buildingTypeId.HasValue)
            {
                await _context.Entry(property)
                    .Reference(p => p.BuildingType)
                    .LoadAsync();
            }

            return property;
        }

        public async Task<bool> DeletePropertyAsync(Guid id)
        {
            _logger.LogInformation("Deleting property {PropertyId}", id);

            var property = await _context.Properties.FindAsync(id);

            if (property == null)
            {
                _logger.LogWarning("Property {PropertyId} not found", id);
                return false;
            }

            _context.Properties.Remove(property);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Property {PropertyId} deleted successfully", id);
            return true;
        }

        public async Task<IEnumerable<Property>> GetAllPropertiesAsync()
        {
            return await _context.Properties
                .AsNoTracking()
                .Include(x => x.Quarter)
                .Include(x => x.BuildingType)
                .Include(x => x.Images) 
                .OrderByDescending(x => x.Price)
                .ToListAsync();
        }

        public async Task<IEnumerable<QuarterAveragePrice>> GetAveragePricesByQuartersAsync()
        {
            _logger.LogInformation("Calculating average prices by quarters");

            var result = await _context.Properties
                .AsNoTracking()
                .Include(x => x.Quarter)
                .GroupBy(x => x.Quarter.Name)
                .Select(x => new QuarterAveragePrice
                {
                    Name = x.Key,
                    AveragePrice = x.Average(p => p.Price),
                    PropertiesCount = x.Count()
                })
                .OrderByDescending(x => x.AveragePrice)
                .ToListAsync();

            return result;
        }

        public async Task<IEnumerable<Property>> GetPropertiesByBuildingTypeAsync(string buildingType)
        {
            var properties = await _context.Properties
                .AsNoTracking()
                .Include(x => x.BuildingType)
                .Include(x => x.Quarter)
                .Include(x => x.Images) 
                .Where(x => x.BuildingType != null && x.BuildingType.Name == buildingType)
                .OrderByDescending(x => x.Price)
                .ToListAsync();

            if (!properties.Any())
            {
                _logger.LogWarning("No properties found in building type: {BuildingType}", buildingType);
            }

            return properties;
        }

        public async Task<Property?> GetPropertyByIdAsync(Guid id)
        {
            var property = await _context.Properties
                .AsNoTracking()
                .Include(x => x.Quarter)
                .Include(x => x.BuildingType)
                .Include(x => x.Images)  
                .FirstOrDefaultAsync(x => x.Id == id);

            if (property == null)
            {
                _logger.LogWarning("Property with ID {PropertyId} not found", id);
            }

            return property;
        }

        public async Task<IEnumerable<Property>> GetTop10PropertiesByQuarterAsync(string quarterName)
        {
            var properties = await _context.Properties
                .AsNoTracking()
                .Include(x => x.Quarter)
                .Include(x => x.BuildingType)
                .Include(x => x.Images)  
                .Where(x => x.Quarter.Name == quarterName)
                .OrderByDescending(x => x.Price)
                .Take(10)
                .ToListAsync();

            if (!properties.Any())
            {
                _logger.LogWarning("No properties found in quarter: {QuarterName}", quarterName);
            }

            return properties;
        }

        public async Task<PagedResult<Property>> SearchByPriceRangePagedAsync(
            int minPrice,
            int maxPrice,
            int pageNumber,
            int pageSize)
        {
            _logger.LogInformation("Searching by price range: {Min}-{Max}, page {Page}",
                minPrice, maxPrice, pageNumber);

            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 12;
            if (pageSize > 100) pageSize = 100;

            var totalCount = await _context.Properties
                .Where(x => x.Price >= minPrice && x.Price <= maxPrice)
                .CountAsync();

            var properties = await _context.Properties
                .AsNoTracking()
                .Include(x => x.Quarter)
                .Include(x => x.BuildingType)
                .Include(x => x.Images)
                .Where(x => x.Price >= minPrice && x.Price <= maxPrice && maxPrice > minPrice)
                .OrderBy(x => x.Price)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<Property>
            {
                Items = properties,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount
            };
        }

        public async Task<IEnumerable<Property>> SearchByPriceRangeAsync(decimal minPrice, decimal maxPrice)
        {
            var properties = await _context.Properties
                .AsNoTracking()
                .Include(x => x.Quarter)
                .Include(x => x.BuildingType)
                .Include(x => x.Images)  
                .Where(x => x.Price >= minPrice && x.Price <= maxPrice)
                .OrderBy(x => x.Price)
                .ToListAsync();

            if (!properties.Any())
            {
                _logger.LogWarning("No properties found in price range: {MinPrice}-{MaxPrice}", minPrice, maxPrice);
            }

            return properties;
        }

        public async Task<Property?> UpdatePropertyAsync(
            Guid id,
            string title,
            decimal price,
            int area,
            int floor,
            int totalFloors,
            string quarterName,
            string? buildingTypeName)
        {
            var property = await _context.Properties
                .Include(p => p.Quarter)
                .Include(p => p.BuildingType)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (property == null)
            {
                _logger.LogWarning("Property {PropertyId} not found", id);
                return null;
            }

            if (property.Quarter.Name != quarterName)
            {
                var quarter = await _context.Quarters
                    .FirstOrDefaultAsync(x => x.Name == quarterName);

                if (quarter == null)
                {
                    quarter = new Quarter { Name = quarterName };
                    await _context.Quarters.AddAsync(quarter);
                    await _context.SaveChangesAsync();
                }

                property.QuarterId = quarter.Id;
            }

            if (!string.IsNullOrWhiteSpace(buildingTypeName))
            {
                var buildingType = await _context.BuildingTypes
                    .FirstOrDefaultAsync(x => x.Name == buildingTypeName);

                if (buildingType == null)
                {
                    buildingType = new BuildingType { Name = buildingTypeName };
                    await _context.BuildingTypes.AddAsync(buildingType);
                    await _context.SaveChangesAsync();
                }

                property.BuildingTypeId = buildingType.Id;
            }
            else
            {
                property.BuildingTypeId = null;
            }

            property.Title = title;
            property.Area = area;
            property.Price = price;
            property.Floor = floor;
            property.TotalFloors = totalFloors;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Property {PropertyId} updated successfully", id);

            await _context.Entry(property).Reference(x => x.Quarter).LoadAsync();
            if (property.BuildingTypeId.HasValue)
            {
                await _context.Entry(property).Reference(x => x.BuildingType).LoadAsync();
            }

            return property;
        }

        public async Task<IEnumerable<Property>> GetPropertiesByQuarterAsync(string quarterName)
        {
            var properties = await _context.Properties
                .AsNoTracking()
                .Include(x => x.Quarter)
                .Include(x => x.BuildingType)
                .Include(x => x.Images)  
                .Where(x => x.Quarter.Name == quarterName)
                .OrderByDescending(x => x.Price)
                .ToListAsync();

            if (!properties.Any())
            {
                _logger.LogWarning("No properties found in quarter: {QuarterName}", quarterName);
            }

            return properties;
        }

        public async Task<PagedResult<Property>> GetPropertiesPagedAsync(int pageNumber = 1, int pageSize = 12)
        {
            _logger.LogInformation("Getting properties page {PageNumber} with size {PageSize}", pageNumber, pageSize);

            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 12;
            if (pageSize > 100) pageSize = 100;

            var totalCount = await _context.Properties.CountAsync();

            var properties = await _context.Properties
                .AsNoTracking()
                .Include(x => x.Quarter)
                .Include(x => x.BuildingType)
                .Include(x => x.Images) 
                .OrderBy(x => x.Price)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<Property>
            {
                Items = properties,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount
            };
        }
    }
}