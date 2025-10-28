using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RealEstateSearcher.Services.Interfaces;
using RealEstateSearcher.Services.Services;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace RealEstateSearcher.Web.Controllers
{
    public class PropertiesController : Controller
    {
        private readonly IPropertyService _propertyService;
        private readonly ILogger<PropertiesController> _logger;
        public PropertiesController(IPropertyService propertyService,
                                    ILogger<PropertiesController> logger)
        {
            this._propertyService = propertyService;
            this._logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var properties = _propertyService.GetAllPropertiesAsync();

            return View(properties);
        }

        public async Task<IActionResult> Details(Guid id)
        {
            var property = _propertyService.GetPropertyByIdAsync(id);

            if (property == null)
            {
                _logger.LogWarning("Property {PropertyId} not found", id);
                return NotFound();
            }

            return View(property);
        }

        public async Task<IActionResult> Create()
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
                var property = _propertyService.AddPropertyAsync(
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
            var property = _propertyService.GetPropertyByIdAsync(id);
            if (property == null)
            {
                _logger.LogWarning("Property {PropertyId} not found", id);
                return NotFound();
            }
            return View(property);
        }

        [HttpPost]
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
               var property =  await _propertyService.UpdatePropertyAsync(
                    id, title, price, area, floor, totalFloors, quarterName, buildingTypeName);

                if (property == null)
                {
                    return NotFound();
                }   

                return RedirectToAction(nameof(Details),new {id }); 
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error updating property { PropertyId}", id);
                ModelState.AddModelError("", "Възникна грешка при актуализирането на имота");
                return View();
            }
        }

       
        public async Task<IActionResult> Delete(Guid id)
        {
            var property = _propertyService.GetPropertyByIdAsync(id);

            if (property == null)
            {
                _logger.LogWarning("Property {PropertyId} not found", id);
                return NotFound();
            }

            return View(property);
        }

        [HttpPost,ActionName("Delete")]
        [ValidateAntiForgeryToken]//For security
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            try
            {
                var result = await _propertyService.DeletePropertyAsync(id);
                if (!result)
                {
                    return NotFound();
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting property {PropertyId}", id);
                return RedirectToAction(nameof(Details), new { id });
            }
        }

        public async Task<IActionResult> Search(Guid id)
        {
            var property = _propertyService.GetPropertyByIdAsync(id);

            if (property == null)
            {
                _logger.LogWarning("Property {PropertyId} not found", id);
                return NotFound();
            }

            return View(property);
        }
        public IActionResult Search()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Search(decimal? minPrice, decimal? maxPrice, string? quarterName)
        {
            IEnumerable<RealEstateSearcher.Core.Models.Property> properties;

            if (minPrice.HasValue && maxPrice.HasValue)
            {
                properties = await _propertyService.SearchByPriceRangeAsync(minPrice.Value, maxPrice.Value);
            }
            else if (!string.IsNullOrWhiteSpace(quarterName))
            {
                properties = await _propertyService.GetPropertiesByQuarterAsync(quarterName);
            }
            else
            {
                properties = await _propertyService.GetAllPropertiesAsync();
            }

            return View("Index",properties);
        }

    }
}
