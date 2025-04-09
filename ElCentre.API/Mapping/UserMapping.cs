using AutoMapper;
using ElCentre.Core.DTO;
using ElCentre.Core.Entities;

namespace ElCentre.API.Mapping
{
    public class UserMapping:Profile
    {
        public UserMapping()
        {
            CreateMap<AppUser, UserDTO>().ReverseMap();
        }
    }
}
