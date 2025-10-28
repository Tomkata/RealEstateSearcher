using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RealEstateSearcher.Core.Models;
using RealEstateSearcher.Services.Dtos;
using RealEstateSearcher.Services.Interfaces;

namespace RealEstateSearcher.Web.Controllers
{
    public class PropertiesController : Controller
    {
        private readonly IPropertyService _propertyService;
        private readonly ILogger<PropertiesController> _logger;

        public PropertiesController(
            IPropertyService propertyService,
            ILogger<PropertiesController> logger)
        {
            _propertyService = propertyService;
            _logger = logger;
        }

        public async Task<IActionResult> Index(int page = 1, int pageSize = 12)
        {
            _logger.LogInformation("Loading properties list, page {Page}", page);
            var pagedResult = await _propertyService.GetPropertiesPagedAsync(page, pageSize);
            return View(pagedResult);
        }
        public async Task<IActionResult> Details(Guid id)
        {
            _logger.LogInformation("Loading property details for {PropertyId}", id);
            var property = await _propertyService.GetPropertyByIdAsync(id);

            if (property == null)
            {
                _logger.LogWarning("Property {PropertyId} not found", id);
                return NotFound();
            }

            return View(property);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            string title,
            decimal price,
            int area,
            int floor,
            int totalFloors,
            string quarterName,
            string? buildingTypeName)
        {
            try
            {
                _logger.LogInformation("Creating new property: {Title}", title);

                var property = await _propertyService.AddPropertyAsync(
                    title,
                    price,
                    area,
                    floor,
                    totalFloors,
                    quarterName,
                    buildingTypeName);

                _logger.LogInformation("Property {PropertyId} created successfully", property.Id);

                return RedirectToAction(nameof(Details), new { id = property.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating property");
                ModelState.AddModelError("", "Възникна грешка при създаването на имота");
                return View();
            }
        }

    
        public async Task<IActionResult> Edit(Guid id)
        {
            _logger.LogInformation("Loading property for edit: {PropertyId}", id);

            // ✅ Добавен await
            var property = await _propertyService.GetPropertyByIdAsync(id);

            if (property == null)
            {
                _logger.LogWarning("Property {PropertyId} not found", id);
                return NotFound();
            }

            return View(property);
        }

     
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            Guid id,
            string title,
            decimal price,
            int area,
            int floor,
            int totalFloors,
            string quarterName,
            string? buildingTypeName)
        {
            try
            {
                _logger.LogInformation("Updating property {PropertyId}", id);

                var property = await _propertyService.UpdatePropertyAsync(
                    id, title, price, area, floor, totalFloors, quarterName, buildingTypeName);

                if (property == null)
                {
                    _logger.LogWarning("Property {PropertyId} not found", id);
                    return NotFound();
                }

                _logger.LogInformation("Property {PropertyId} updated successfully", id);

                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating property {PropertyId}", id);
                ModelState.AddModelError("", "Възникна грешка при актуализирането на имота");
                return View();
            }
        }

  
        public async Task<IActionResult> Delete(Guid id)
        {
            _logger.LogInformation("Loading property for delete: {PropertyId}", id);

          
            var property = await _propertyService.GetPropertyByIdAsync(id);

            if (property == null)
            {
                _logger.LogWarning("Property {PropertyId} not found", id);
                return NotFound();
            }

            return View(property);
        }

        // POST: /Properties/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            try
            {
                _logger.LogInformation("Deleting property {PropertyId}", id);

                var result = await _propertyService.DeletePropertyAsync(id);

                if (!result)
                {
                    _logger.LogWarning("Property {PropertyId} not found", id);
                    return NotFound();
                }

                _logger.LogInformation("Property {PropertyId} deleted successfully", id);

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting property {PropertyId}", id);
                return RedirectToAction(nameof(Details), new { id });
            }
        }

        public async Task<IActionResult> Search(
            decimal? minPrice,
            decimal? maxPrice,
            string? quarterName,
            string? buildingType,
            int page = 1,
            int pageSize = 12)
        {
            if (!minPrice.HasValue && !maxPrice.HasValue &&
                string.IsNullOrWhiteSpace(quarterName) &&
                string.IsNullOrWhiteSpace(buildingType))
            {
                return View();
            }

            _logger.LogInformation("Searching properties with filters");

            PagedResult<Property> pagedResult;

            // Търсене по цена
            if (minPrice.HasValue && maxPrice.HasValue)
            {
                _logger.LogInformation("Searching by price range: {Min}-{Max}", minPrice, maxPrice);

                pagedResult = await _propertyService.SearchByPriceRangePagedAsync(
                    (int)minPrice.Value,
                    (int)maxPrice.Value,
                    page,
                    pageSize);

                ViewBag.SearchType = "price";
                ViewBag.MinPrice = minPrice;
                ViewBag.MaxPrice = maxPrice;
            }
            // Търсене по квартал
            // Търсене по квартал
            else if (!string.IsNullOrWhiteSpace(quarterName))
            {
                var allProperties = await _propertyService.GetPropertiesByQuarterAsync(quarterName);

                pagedResult = new PagedResult<Property>
                {
                    Items = allProperties
                        .OrderBy(p => p.Price)  
                        .Skip((page - 1) * pageSize)
                        .Take(pageSize)
                        .ToList(),
                    PageNumber = page,
                    PageSize = pageSize,
                    TotalCount = allProperties.Count()
                };

                ViewBag.SearchType = "quarter";
                ViewBag.QuarterName = quarterName;
            }
            // Търсене по тип сграда
            else if (!string.IsNullOrWhiteSpace(buildingType))
            {
                var allProperties = await _propertyService.GetPropertiesByBuildingTypeAsync(buildingType);

                pagedResult = new PagedResult<Property>
                {
                    Items = allProperties
                        .OrderBy(p => p.Price)  
                        .Skip((page - 1) * pageSize)
                        .Take(pageSize)
                        .ToList(),
                    PageNumber = page,
                    PageSize = pageSize,
                    TotalCount = allProperties.Count()
                };

                ViewBag.SearchType = "buildingType";
                ViewBag.BuildingType = buildingType;
            }
            else
            {
                pagedResult = await _propertyService.GetPropertiesPagedAsync(page, pageSize);
                ViewBag.SearchType = "all";
            }

            return View("SearchResults", pagedResult);
        }
    }
}