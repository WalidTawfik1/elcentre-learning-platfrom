using ElCentre.Core.Entities;
using ElCentre.Core.Interfaces;
using ElCentre.Core.Services;
using ElCentre.Infrastructure.Data;
using ElCentre.Infrastructure.Repositories.Services;
using Microsoft.EntityFrameworkCore;
using Polly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ElCentre.Infrastructure.Repositories
{
    public class CouponCodeRepository : GenericRepository<CouponCode>, ICouponCode
    {
        private readonly ElCentreDbContext context;
        private readonly ICouponService couponService;
        public CouponCodeRepository(ElCentreDbContext context, ICouponService couponService) : base(context)
        {
            this.context = context;
            this.couponService = couponService;
        }

        public async Task<decimal> GetFinalPrice(string code, string studentId, int courseId)
        {
            var coupon = await context.CouponCodes
                .Where(c => c.Code == code &&
                c.ExpirationDate > DateOnly.FromDateTime(DateTime.Now) &&
                c.UsageLimit > 0)
                .FirstOrDefaultAsync();
            if(coupon == null)
            {
                throw new ArgumentException("Invalid or expired coupon code.");
            }
            var isValidForStudent = await context.CouponUsages
                   .AnyAsync(u => u.CouponId == coupon.Id && u.UserId == studentId);

            if (isValidForStudent)
                throw new ArgumentException("Coupon code has already been used.");

            var course = await context.Courses.FindAsync(courseId);

            if (course == null)
            {
                throw new ArgumentException("Course not found.");
            }
            var amount = course.Price;

            if (!coupon.IsGlobal && courseId != coupon.CourseId)
                throw new ArgumentException("Coupon code is not valid for this course.");

            var totalAmount = couponService.ApplyDiscount(amount, coupon.DiscountType, coupon.DiscountValue);

            return totalAmount;
        }

        public async Task<bool> CouponCodeExists(string code)
        {
            return await context.CouponCodes
                .Where(c => c.Code == code)
                .AnyAsync();
        }
    }
}
