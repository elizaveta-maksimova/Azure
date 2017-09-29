using AutoMapper;
using MVC_WebApplication.BL.Models;
using MVC_WebApplication.Models;

namespace MVC_WebApplication.AutoMapper.Profiles
{
    public class PresentationLayerProfile : Profile
    {
        public PresentationLayerProfile()
        {
            RegisterProductMapping();
        }

        public void RegisterProductMapping()
        {
            CreateMap<Product, ProductEntity>();
        }
    }
}