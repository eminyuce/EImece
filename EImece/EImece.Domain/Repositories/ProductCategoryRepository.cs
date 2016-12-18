using EImece.Domain.Entities;
using EImece.Domain.Repositories.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EImece.Domain.DbContext;
using EImece.Domain.GenericRepositories;

namespace EImece.Domain.Repositories
{
    public class ProductCategoryRepository : BaseRepository<ProductCategory, int>, IProductCategoryRepository
    {
        public ProductCategoryRepository(IEImeceContext dbContext) : base(dbContext)
        {
        }

        //Recursion method for recursively get all child nodes
        private void GetTreeview(List<ProductCategory> list, ProductCategory current, ref List<ProductCategory> returnList)
        {
            //get child of current item
            var childs = list.Where(a => a.ParentId == current.Id).OrderBy(r => r.Position).ToList();
            current.Childrens = new List<ProductCategory>();
            current.Childrens.AddRange(childs);
            foreach (var i in childs)
            {
                GetTreeview(list, i, ref returnList);
            }
        }
       
        public List<ProductCategory> BuildTree()
        {
            List<ProductCategory> list = GetAll().ToList();
            List<ProductCategory> returnList = new List<ProductCategory>();
            //find top levels items
            var topLevels = list.Where(a => a.ParentId == 0).OrderBy(r => r.Position).ToList();
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
            GC.SuppressFinalize(this);
        }

        public int SaveOrEdit(ProductCategory item)
        {
            return BaseEntityRepository.SaveOrEdit(this, item);
        }

        public int DeleteItem(ProductCategory item)
        {
            return BaseEntityRepository.DeleteItem(this, item);
        }
    }
}
