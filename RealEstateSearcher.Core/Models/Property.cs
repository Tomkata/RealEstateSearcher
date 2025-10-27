using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RealEstateSearcher.Core.Models
{
    public class Property
    {
        public Property()
        {
            this.Id = Guid.NewGuid();
        }
        public Guid Id { get; set; }
        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = null!;
        [Range(0, double.MaxValue)]
        public decimal Price { get; set; }
        [Required]
        [Range(1, 500)]
        public int Area { get; set; }

        [NotMapped]
        public decimal PriceByArea => this.Price / this.Area;
        [Range(0, 100)]
        public int Floor { get; set; }
        [Range(0, 100)]

        public int TotalFloors { get; set; }

        [ForeignKey(nameof(Quarter))]
        public Guid QuarterId { get; set; }
        public Quarter Quarter { get; set; }

        [ForeignKey(nameof(BuildingType))]
        public Guid? BuildingTypeId { get; set; }
        public BuildingType BuildingType { get; set; }

    }
}
