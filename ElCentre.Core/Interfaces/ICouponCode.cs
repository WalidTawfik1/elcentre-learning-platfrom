using ElCentre.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElCentre.Core.Interfaces
{
    public interface ICouponCode : IGenericRepository<CouponCode>
    {
        public Task<decimal> GetFinalPrice(string code, string studentId, int courseId);
        public Task<bool> CouponCodeExists(string code);

    }
}
