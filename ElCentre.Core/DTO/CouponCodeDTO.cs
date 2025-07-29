using System.ComponentModel.DataAnnotations.Schema;

namespace ElCentre.Core.DTO
{
    public class AddCouponCodeDTO
    {
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