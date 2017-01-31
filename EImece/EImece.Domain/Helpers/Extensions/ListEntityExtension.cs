using EImece.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EImece.Domain.Helpers.Extensions
{
    public static class ListEntityExtension
    {
        public static String GetListItemText(this List entity)
        {
            var Items = entity.ListItems;
            if (Items != null && Items.Any())
            {
                return
              string.Join("\n", Items.Select(i => i.Name == i.Value ? i.Name : string.Format("{0}, {1}", i.Name, i.Value)).ToList());
            }
            else
            {
                return String.Empty;
            }
        }

        public static void SetListItems(this List entity, String value)
        {
            var strItems = value.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            var Items = new List<ListItem>();
            int order = 1;
            foreach (var sItem in strItems)
            {

                var itemValues = sItem.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                var text = itemValues[0];
                var val = text;
                if (itemValues.Length > 1 && !string.IsNullOrEmpty(itemValues[1]))
                {
                    val = itemValues[1];
                }

                Items.Add(new ListItem() { Id=0, Name = text, Value = val, Position = order++, ListId = entity.Id });

            }

            entity.ListItems = Items;
        }
    }
}
