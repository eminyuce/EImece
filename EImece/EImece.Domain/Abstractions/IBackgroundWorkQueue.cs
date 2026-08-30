using System;
using System.Threading;

namespace EImece.Domain.Abstractions
{
    /// <summary>
    /// Pure domain abstraction for queueing background work
    /// without directly depending on System.Web.Hosting.HostingEnvironment.
    /// </summary>
    public interface IBackgroundWorkQueue
    {
        void QueueBackgroundWorkItem(Action<CancellationToken> workItem);
    }
}
