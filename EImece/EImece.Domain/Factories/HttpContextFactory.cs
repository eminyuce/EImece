using EImece.Domain.Factories.IFactories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace EImece.Domain.Factories
{
    public class HttpContextFactory
     : IHttpContextFactory
    {
        public HttpContextBase Create()
        {
            return new HttpContextWrapper(HttpContext.Current);
        }
    }
}
