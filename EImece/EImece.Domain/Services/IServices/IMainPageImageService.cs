using EImece.Domain.Entities;
using EImece.Domain.Models.FrontModels;
using System.Threading;
using System.Threading.Tasks;

namespace EImece.Domain.Services.IServices
{
    public interface IMainPageImageService : IBaseContentService<MainPageImage>
    {
        void DeleteMainPageImage(int id);

        Task DeleteMainPageImageAsync(int id);

        FooterViewModel GetFooterViewModel(int language);

        MainPageViewModel GetMainPageViewModel(int language);

        Task<MainPageViewModel> GetMainPageViewModelAsync(int language, CancellationToken cancellationToken = default(CancellationToken));
    }
}