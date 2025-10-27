using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RealEstateSearcher.Core.Models
{
    public class BuildingType
    {
        public BuildingType()
        {
            this.Id = Guid.NewGuid();
    
        }
        [Key]
        public Guid Id { get; set; }
        [Required]
        [MaxLength(50)]
        public string Name { get; set; } = string.Empty;
        public virtual ICollection<Property> Properties { get; set; } = new HashSet<Property>();
    }
}
