using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElCentre.Core.Sharing
{
    public class CourseParams
    {
        public int pagenum { get; set; } = 1;
        public int Maxpagesize { get; set; } = 6;
        private int _pagesize = 3;

        public int pagesize
        {
            get { return _pagesize; }
            set { _pagesize = value > Maxpagesize ? Maxpagesize : value; }
        }

        public string? sort { get; set; }
        public int? categoryId { get; set; }
        public string? search { get; set; }
        public decimal? minPrice { get; set; }
        public decimal? maxPrice { get; set; }
    }
}
