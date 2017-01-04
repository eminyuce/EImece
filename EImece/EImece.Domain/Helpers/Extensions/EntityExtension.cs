using EImece.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EImece.Domain.Helpers.Extensions
{
    public static class EntityExtension
    {
        public static String GetSeoUrl(this BaseEntity entity)
        {
            return String.Format("{0}-{1}", GeneralHelper.GetUrlSeoString(entity.Name), entity.Id);
        }
    }
}
