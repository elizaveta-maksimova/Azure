using AutoMapper;
using MVC_WebApplication.AutoMapper.Profiles;

namespace MVC_WebApplication.AutoMapper
{
    public class AutoMapperConfig
    {
        public static void Configure()
        {
            Mapper.Initialize(cfg =>
            {
                cfg.AddProfile(new PresentationLayerProfile());
            });
        }
    }
}