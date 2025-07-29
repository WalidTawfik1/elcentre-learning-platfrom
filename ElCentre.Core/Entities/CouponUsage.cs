using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElCentre.Core.Entities
{
    public class CouponUsage
    {
        [Key]
        public int Id { get; set; }
        public int CouponId { get; set; }
        public string UserId { get; set; }
        public DateTime UsedOn { get; set; } = DateTime.Now;
    }
}
