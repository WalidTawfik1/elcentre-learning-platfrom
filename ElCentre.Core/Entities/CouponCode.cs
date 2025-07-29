using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElCentre.Core.Entities
{
    public class CouponCode
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string DiscountType { get; set; } = string.Empty; // e.g., "percentage" or "fixed"

        [Column(TypeName = "decimal(18,2)")]
        public decimal DiscountValue { get; set; }
        public DateOnly ExpirationDate { get; set; }
        public int UsageLimit { get; set; }
        public bool IsGlobal { get; set; } = false; // If true, the coupon can be used for any course
        public int? CourseId { get; set; }

    }
}
