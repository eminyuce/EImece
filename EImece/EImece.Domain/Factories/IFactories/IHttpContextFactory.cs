using System;
using System.Web;

namespace EImece.Domain.Factories.IFactories
{
    public interface IHttpContextFactory
    {
        HttpContextBase Create();

        String GetCurrentUserId();
    }
}