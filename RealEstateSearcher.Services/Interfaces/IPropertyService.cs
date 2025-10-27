using Microsoft.EntityFrameworkCore.Metadata.Internal;
using RealEstateSearcher.Infrastructure;
using RealEstateSearcher.Services.Dtos;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Property = RealEstateSearcher.Core.Models.Property;

namespace RealEstateSearcher.Services.Services
{
    public interface IPropertyService
    {
        Task<IEnumerable<Property>> GetAllPropertiesAsync();

        Task<Property?> GetPropertyByIdAsync(Guid id);

         Task<Property> AddPropertyAsync(string title,
             decimal price,
             int area,
             int floor,
             int totalFloors,
             string quarterName, 
             string? buildingTypeName);

        Task<Property?> UpdatePropertyAsync(string title,
             decimal price,
             int area,
             int floor,
             int totalFloors,
             string quarterName,
             string? buildingTypeName);

        Task<bool> DeletePropertyAsync(Guid Id);


        Task<IEnumerable<Property>> GetTop10PropertiesByQuarterAsync(string quarterName);

        Task<IEnumerable<QuarterAveragePrice>> GetAveragePricesByQuartersAsync(string quarterName);

        Task<IEnumerable<Property>> SearchByPriceRangeAsync(decimal minPrice, decimal maxPrice);

        Task<IEnumerable<Property>> GetPropertiesByBuildingTypeAsync(string buildingType);
    }
}
