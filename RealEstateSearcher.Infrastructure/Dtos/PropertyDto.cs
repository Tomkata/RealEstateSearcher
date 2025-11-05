namespace RealEstateSearcher.Infrastructure.Dtos
{
    public class PropertyDto
    {
        public string Title { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Area { get; set; }
        public int Floor { get; set; }
        public int TotalFloors { get; set; }
        public string Quarter { get; set; } = string.Empty;
        public string? BuildingType { get; set; }
        public string? ImageUrl { get; set; }
        public List<string>? Images { get; set; }
    }
}