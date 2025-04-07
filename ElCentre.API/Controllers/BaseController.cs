using AutoMapper;
using ElCentre.Core.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ElCentre.API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class BaseController : ControllerBase
    {
        protected readonly IUnitofWork work;
        protected readonly IMapper mapper;

        public BaseController(IUnitofWork work, IMapper mapper)
        {
            this.work = work;
            this.mapper = mapper;
        }
    }
}
