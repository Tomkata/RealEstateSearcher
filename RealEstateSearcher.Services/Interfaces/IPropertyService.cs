
using RealEstateSearcher.Services.Dtos;
using Property = RealEstateSearcher.Core.Models.Property;

namespace RealEstateSearcher.Services.Interfaces  
{
    public interface IPropertyService
    {
        Task<IEnumerable<Property>> GetAllPropertiesAsync();
        Task<Property?> GetPropertyByIdAsync(Guid id);
        Task<IEnumerable<Property>> GetTop10PropertiesByQuarterAsync(string quarterName);
        Task<IEnumerable<Property>> SearchByPriceRangeAsync(decimal minPrice, decimal maxPrice);
        Task<IEnumerable<Property>> GetPropertiesByBuildingTypeAsync(string buildingType);
        Task<IEnumerable<QuarterAveragePrice>> GetAveragePricesByQuartersAsync();
        Task<Property> AddPropertyAsync(
            string title,
            decimal price,
            int area,
            int floor,
            int totalFloors,
            string quarterName,
            string? buildingTypeName);

        Task<Property?> UpdatePropertyAsync(
            Guid id,
            string title,
            decimal price,
            int area,
            int floor,
            int totalFloors,
            string quarterName,
            string? buildingTypeName);

        Task<bool> DeletePropertyAsync(Guid id);
    }
}
