using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RealEstateSearcher.Services.Dtos
{
    public class QuarterAveragePrice
    {
        public string Name { get; set; }
        public decimal AveragePrice { get; set; }
        public int PropertiesCount { get; set; }
    }
}
