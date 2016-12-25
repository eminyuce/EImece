using EImece.Domain.Entities;
using EImece.Domain.Repositories.IRepositories;
using EImece.Domain.Services.IServices;
using Ninject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EImece.Domain.Services
{
    public class MenuService : BaseContentService<Menu>, IMenuService
    {
        private IMenuRepository MenuRepository { get; set; }
        public MenuService(IMenuRepository repository)
        {
            MenuRepository = repository;
            this.baseRepository = repository;
        }

        public List<Menu> BuildTree()
        {
            return MenuRepository.BuildTree();
        }
         

    }
}
