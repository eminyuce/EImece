using EImece.Domain.Entities;
using EImece.Domain.Models.FrontModels;

namespace EImece.Domain.Services.IServices
{
    public interface IMainPageImageService : IBaseContentService<MainPageImage>
    {
        void DeleteMainPageImage(int id);

        FooterViewModel GetFooterViewModel(int language);

        MainPageViewModel GetMainPageViewModel(int language);
    }
}