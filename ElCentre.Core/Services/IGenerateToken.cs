using ElCentre.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElCentre.Core.Services
{
    public interface IGenerateToken
    {
       string GetAndCreateToken(AppUser customer);
    }
}
