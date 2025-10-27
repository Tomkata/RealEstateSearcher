namespace RealEstateSearcher.Infrastructure.Seeding
{
    public class PropertyDto
    {
        public string? Title { get; set; }
        public string Quarter { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Area { get; set; }
        public int Floor { get; set; }
        public int TotalFloors { get; set; }
        public string? BuildingType { get; set; }
    }
}