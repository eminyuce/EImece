using AutoMapper;
using EImece.Domain.Entities;
using EImece.Domain.Helpers.Extensions;
using EImece.Domain.Models.DTOs;

namespace EImece.Domain.Services
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMapProductCategory();
        }

        private void CreateMapProductCategory()
        {
            CreateMap<ProductCategory, ProductCategoryDto>()

                            .ForMember(d => d.MainImageUrl,
                                o => o.MapFrom(s => s.GetCroppedImageUrl(s.MainImageId, 100, 100, false, false)))

                             .ForMember(d => d.MainImageThumbnailUrl,
                                o => o.MapFrom(s => s.GetCroppedImageUrl(s.MainImageId, 100, 100, true, false)))

                            .ForMember(d => d.DetailPageUrl,
                                o => o.MapFrom(s => s.DetailPageUrl))

                            .ForMember(d => d.SeoUrl,
                                o => o.MapFrom(s => s.GetSeoUrl()))

                            .ForMember(d => d.DiscountPercentage,
                                o => o.MapFrom(s => s.DiscountPercantage))

                            .ForMember(d => d.Children, o => o.Ignore());
        }
    }
}