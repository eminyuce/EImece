using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace EImece.Domain.Factories.IFactories
{
    public interface IHttpContextFactory
    {
        HttpContextBase Create();
        String GetCurrentUserId();
    }
}
