using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElCentre.Core.DTO
{
    public record CategoryDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
    public record AddCategoryDTO
    {
        public string Name { get; set; }
    }
}
