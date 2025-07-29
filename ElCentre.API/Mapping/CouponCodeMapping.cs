using AutoMapper;
using ElCentre.Core.DTO;
using ElCentre.Core.Entities;

namespace ElCentre.API.Mapping
{
    public class CouponCodeMapping:Profile
    {
        public CouponCodeMapping()
        {
            CreateMap<AddCouponCodeDTO, CouponCode>().ReverseMap();
        }
    }
}
