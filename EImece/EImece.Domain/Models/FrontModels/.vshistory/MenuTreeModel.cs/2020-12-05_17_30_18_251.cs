using EImece.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EImece.Domain.Models.FrontModels
{
    public class MenuTreeModel
    {
        public Menu Menu { get; set; }

        public MenuTreeModel()
        {

        }

        public MenuTreeModel(Menu r)
        {
            this.Menu = r;
        }
        public MenuTreeModel(Menu r, int level)
        {
            this.Menu = r;
            this.TreeLevel = level;
        }

        public int Id
        {
            get
            {
                return Menu.Id;
            }
        }
        public string Name
        {
            get
            {
                return Menu.Name;
            }
        }

        public int TreeLevel { get; set; }
        public List<MenuTreeModel> Childrens { get; set; }
        public MenuTreeModel Parent { get; set; }

        public string TextWithArrow
        {
            get
            {
                return string.Format("{1}{0}", Menu.Name, ProduceArrow());
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
