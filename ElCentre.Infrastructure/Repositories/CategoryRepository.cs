using ElCentre.Core.Entities;
using ElCentre.Core.Interfaces;
using ElCentre.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElCentre.Infrastructure.Repositories
{
    public class CategoryRepository:GenericRepository<Category>, ICategoryRepository
    {
        public CategoryRepository(ElCentreDbContext context) : base(context)
        {
        }
    }

}
