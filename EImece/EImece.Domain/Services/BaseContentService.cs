using EImece.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EImece.Domain.Services
{
   
    public abstract class BaseContentService<T> : BaseEntityService<T> where T : BaseContent
    {
      
    }
}
