using ElCentre.Core.Entities;
using ElCentre.Core.Services;
using ElCentre.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElCentre.Infrastructure.Repositories.Services
{
    public class CouponService: ICouponService
    {
        private readonly ElCentreDbContext _context;

        public CouponService(ElCentreDbContext context)
        {
            _context = context;
        }

        public async Task<decimal> ApplyCouponAsync(string couponCode, decimal amount, string studentId, int courseId)
        {
            try
            {
                if (string.IsNullOrEmpty(couponCode))
                    return amount; // No coupon, return original amount

                var coupon = await _context.CouponCodes
                    .Where(c => c.Code == couponCode &&
                                c.ExpirationDate > DateOnly.FromDateTime(DateTime.Now) &&
                                c.UsageLimit > 0)
                    .FirstOrDefaultAsync();

                if (coupon == null)
                    throw new ArgumentException("Invalid or expired coupon code.");

                var isValidForStudent = await _context.CouponUsages
                    .AnyAsync(u => u.CouponId == coupon.Id && u.UserId == studentId);

                if (isValidForStudent)
                    throw new ArgumentException("Coupon code has already been used.");

                if (!coupon.IsGlobal && courseId != coupon.CourseId)
                    throw new ArgumentException("Coupon code is not valid for this course.");

                // Apply discount
                amount = ApplyDiscount(amount, coupon.DiscountType, coupon.DiscountValue);

                await _context.SaveChangesAsync();
                return amount;
            }
            catch
            {
                throw;
            }
        }

        public decimal ApplyDiscount(decimal amount, string discountType, decimal discountValue)
        {
            switch (discountType.ToLower())
            {
                case "percentage":
                    amount -= amount * (discountValue / 100);
                    break;

                case "fixed":
                    amount -= discountValue;
                    break;

                case "setprice":
                    amount = discountValue;
                    break;

                case "free":
                    amount = 0;
                    break;

                default:
                    throw new ArgumentException("Invalid discount type");
            }

            // Ensure amount is not negative
            return amount < 0 ? 0 : amount;
        }
    }
}
