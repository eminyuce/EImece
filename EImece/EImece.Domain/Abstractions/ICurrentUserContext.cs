using System;

namespace EImece.Domain.Abstractions
{
    /// <summary>
    /// Pure domain abstraction providing current user identity context
    /// without any dependency on System.Web, HttpContext, or web frameworks.
    /// </summary>
    public interface ICurrentUserContext
    {
        string GetCurrentUserId();
        bool IsAuthenticated { get; }
        bool IsInRole(string role);
    }
}
