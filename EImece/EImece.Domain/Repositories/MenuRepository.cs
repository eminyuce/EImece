using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EImece.Domain.DbContext;
using EImece.Domain.Entities;
using EImece.Domain.Repositories.IRepositories;
using GenericRepository;
using EImece.Domain.GenericRepositories;
using System.Data.Entity;

namespace EImece.Domain.Repositories
{
    public class MenuRepository : BaseContentRepository<Menu>, IMenuRepository
    {
        public MenuRepository(IEImeceContext dbContext) : base(dbContext)
        {
           
        }

        private void GetTreeview(List<Menu> list, Menu current, ref List<Menu> returnList)
        {
            //get child of current item
            var childs = list.Where(a => a.ParentId == current.Id).OrderBy(r => r.Position).ToList();
            current.Childrens = new List<Menu>();
            current.Childrens.AddRange(childs);
            foreach (var i in childs)
            {
                GetTreeview(list, i, ref returnList);
            }
        }

        public List<Menu> BuildTree()
        {
            List<Menu> list = GetAll().ToList();
            List<Menu> returnList = new List<Menu>();
            //find top levels items
            var topLevels = list.Where(a => a.ParentId == 0).OrderBy(r=>r.Position).ToList();
            returnList.AddRange(topLevels);
            foreach (var i in topLevels)
            {
                GetTreeview(list, i, ref returnList);
            }
            return returnList;
        }

        public void Dispose()
        {
            Dispose(true);
        }

        public int SaveOrEdit(Menu item)
        {
            return BaseEntityRepository.SaveOrEdit(this,item);
        }
        public int DeleteItem(Menu item)
        {
            return BaseEntityRepository.DeleteItem(this, item);
        }
        public List<Menu> GetActiveBaseEntities(bool? isActive)
        {
            return BaseEntityRepository.GetActiveBaseEntities(this, isActive);
        }

        public List<Menu> GetActiveBaseContents(bool? isActive, int language)
        {
            return BaseContentRepository.GetActiveBaseContents(this, isActive, language);
        }
    }
}
