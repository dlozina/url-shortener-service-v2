using AutoMapper;
using Shortener.Service.DTO;
using Shortener.Service.Model;

namespace Shortener.Service.Profiles
{
    public class UrlProfile : Profile
    {
        public UrlProfile()
        {
            CreateMap<UrlData, UrlDataDto>().ReverseMap();
        }
    }
}