using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RealEstateSearcher.Core.Models
{
    public class Property
    {
        public Property()
        {
            Id = Guid.NewGuid();
            Images = new HashSet<PropertyImage>();
        }

        [Key]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Range(0, double.MaxValue)]
        public decimal Price { get; set; }

        [Range(1, 10000)]
        [Required]
        public int Area { get; set; }

        [NotMapped]
        public decimal PriceByArea =>  Price / Area ;

        [Range(0, 200)]
        public int Floor { get; set; }

        [Range(0, 200)]
        public int TotalFloors { get; set; }

        [Required]
        public Guid QuarterId { get; set; }

        [ForeignKey(nameof(QuarterId))]
        public Quarter Quarter { get; set; } = null!;

        public Guid? BuildingTypeId { get; set; }

        [ForeignKey(nameof(BuildingTypeId))]
        public BuildingType? BuildingType { get; set; }

        public string? ImageUrl { get; set; } 

        public ICollection<PropertyImage> Images { get; set; } 

        [MaxLength(1000)]
        public string? Description { get; set; }

        [MaxLength(100)]
        public string? SourceLink { get; set; } 
    }
}
