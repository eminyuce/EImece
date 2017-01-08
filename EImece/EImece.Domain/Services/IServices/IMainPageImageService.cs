using EImece.Domain.Entities;
using EImece.Domain.Models.FrontModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EImece.Domain.Services.IServices
{
    public interface IMainPageImageService : IBaseContentService<MainPageImage>
    {
        void DeleteMainPageImage(int id);
        MainPageViewModel GetMainPageViewModel(int language);
    }
}
