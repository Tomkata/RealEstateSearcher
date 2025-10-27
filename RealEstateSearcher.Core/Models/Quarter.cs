using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RealEstateSearcher.Core.Models
{
    public class Quarter
    {
        public Quarter()
        {
            this.Id = Guid.NewGuid();
        }

        [Key]
        public Guid Id { get; set; }
        [Required]
        [StringLength(80,MinimumLength =5)]
        public string Name { get; set; }

        public virtual ICollection<Property> Properties { get; set; } = new HashSet<Property>();    
    }
}
