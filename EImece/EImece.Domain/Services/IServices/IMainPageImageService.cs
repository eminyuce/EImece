using EImece.Domain.Entities;
using EImece.Domain.Models.DTOs.Storefront;
using EImece.Domain.Models.FrontModels;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EImece.Domain.Services.IServices
{
    public interface IMainPageImageService : IBaseContentService<MainPageImage>
    {
        #region Storefront Read Methods (LINQ Projection, AsNoTracking, Main Entity Activation)

        Task<List<StorefrontBannerDto>> GetStorefrontMainPageBannersAsync(int language, CancellationToken cancellationToken = default(CancellationToken));
        List<StorefrontBannerDto> GetStorefrontMainPageBanners(int language);

        #endregion

        void DeleteMainPageImage(int id);

        Task DeleteMainPageImageAsync(int id);

        FooterViewModel GetFooterViewModel(int language);

        MainPageViewModel GetMainPageViewModel(int language);

        Task<MainPageViewModel> GetMainPageViewModelAsync(int language, CancellationToken cancellationToken = default(CancellationToken));
    }
}