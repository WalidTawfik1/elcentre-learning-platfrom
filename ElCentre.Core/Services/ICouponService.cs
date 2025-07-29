using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElCentre.Core.Services
{
    public interface ICouponService
    {
        public Task<decimal> ApplyCouponAsync(string couponCode, decimal amount, string studentId, int courseId);
        public decimal ApplyDiscount(decimal amount, string discountType, decimal discountValue);
    }
}
