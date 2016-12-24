using EImece.Domain.Entities;
using EImece.Domain.Services.IServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EImece.Domain.Services
{
    public class MenuService : BaseContentService<Menu>, IMenuService
    {
        public List<Menu> BuildTree()
        {
            return MenuRepository.BuildTree();
        }

        public override void SetCurrentRepository()
        {
            this.baseRepository = MenuRepository;
        }

    }
}
