using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RealEstateSearcher.Core.Models;
using RealEstateSearcher.Infrastructure;
using RealEstateSearcher.Services.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public Task<Property> AddPropertyAsync(string title, decimal price, int area, int floor, int totalFloors, string quarterName, string? buildingTypeName)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> DeletePropertyAsync(Guid id)
        {
            _logger.LogInformation("Deleting property {PropertyId}", id);

            var property = await _context.Properties
                .FindAsync(id));


            if (property == null) 
            {
                _logger.LogWarning("Property {PropertyId} not found", id);
                return false;
            } 

            var remove = _context.Remove(property);
            await  _context.SaveChangesAsync();

            _logger.LogInformation("Property {PropertyId} deleted successfully", id);
            return true;

        }

        public async Task<IEnumerable<Property>> GetAllPropertiesAsync()
        {
            return await _context.Properties
                            .AsNoTracking()
                            .Include(x => x.Quarter)
                            .Include(x=>x.BuildingType)
                            .OrderByDescending(x=>x.Price)
                            .ToListAsync();
        }

        public async Task<IEnumerable<QuarterAveragePrice>> GetAveragePricesByQuartersAsync(string quarterName)
        {
            var result = await _context.Properties
                .Include(x => x.Quarter)
                .GroupBy(x => x.Quarter.Name)
                .Select(x=>new QuarterAveragePrice
                {
                     Name = x.Key,
                      AveragePrice = x.Average(x=>x.Price),
                      PropertiesCount = x.Count()
                })
                .OrderByDescending(x=>x.AveragePrice)
                .ToListAsync();


            return result;
        }

        public Task<IEnumerable<Property>> GetPropertiesByBuildingTypeAsync(string buildingType)
        {
            throw new NotImplementedException();
        }

        public Task<Property?> GetPropertyByIdAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Property>> GetTop10PropertiesByQuarterAsync(string quarterName)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Property>> SearchByPriceRangeAsync(decimal minPrice, decimal maxPrice)
        {
            throw new NotImplementedException();
        }

        public Task<Property?> UpdatePropertyAsync(string title, decimal price, int area, int floor, int totalFloors, string quarterName, string? buildingTypeName)
        {
            throw new NotImplementedException();
        }
    }
}
