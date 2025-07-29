using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ElCentre.Core.Sharing
{
    public class CourseParams
    {
        public int pagenum { get; set; } = 1;
        [JsonIgnore]
        public int Maxpagesize { get; set; } = 50;
        private int _pagesize = 16;

        public int pagesize
        {
            get { return _pagesize; }
            set { _pagesize = value > Maxpagesize ? Maxpagesize : value; }
        }

        public string? sort { get; set; }
        public int? categoryId { get; set; }
        public string? language { get; set; }
        public string? search { get; set; }
        public decimal? minPrice { get; set; }
        public decimal? maxPrice { get; set; }
    }
}
