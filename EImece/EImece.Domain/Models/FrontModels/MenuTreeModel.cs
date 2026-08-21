using EImece.Domain.Models.DTOs.Storefront;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EImece.Domain.Models.FrontModels
{
    public class MenuTreeModel
    {
        public StorefrontMenuDto Menu { get; set; }

        /// <summary>
        /// True when this node or any descendant matches the current request (for nav active CSS).
        /// </summary>
        public bool IsActiveBranch
        {
            get
            {
                if (Menu != null && !string.IsNullOrEmpty(Menu.IsPageActived))
                {
                    return true;
                }

                return Childrens != null && Childrens.Any(c => c != null && c.IsActiveBranch);
            }
        }

        public string ActiveCssClass
        {
            get { return IsActiveBranch ? "active current" : string.Empty; }
        }

        public MenuTreeModel()
        {
        }

        public MenuTreeModel(StorefrontMenuDto r)
        {
            this.Menu = r;
        }

        public MenuTreeModel(StorefrontMenuDto r, int level)
        {
            this.Menu = r;
            this.TreeLevel = level;
        }

        public int Id
        {
            get
            {
                return Menu != null ? Menu.Id : 0;
            }
        }

        public string Name
        {
            get
            {
                return Menu != null ? Menu.Name : string.Empty;
            }
        }

        public int TreeLevel { get; set; }
        public List<MenuTreeModel> Childrens { get; set; }
        public MenuTreeModel Parent { get; set; }

        public string TextWithArrow
        {
            get
            {
                return string.Format("{1}{0}", Menu != null ? Menu.Name : string.Empty, ProduceArrow());
            }
        }

        public string ProduceArrow()
        {
            var builder = new StringBuilder();
            int count = TreeLevel - 1;
            if (count > 0)
            {
                for (int i = 0; i < count; i++)
                {
                    builder.Append(" — ");
                }
                builder.Append("> ");
            }
            return builder.ToString();
        }
    }
}