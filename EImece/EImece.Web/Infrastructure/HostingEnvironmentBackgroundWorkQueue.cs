using EImece.Domain.Abstractions;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Hosting;

namespace EImece.Web.Infrastructure
{
    public class HostingEnvironmentBackgroundWorkQueue : IBackgroundWorkQueue
    {
        public void QueueBackgroundWorkItem(Action<CancellationToken> workItem)
        {
            if (workItem == null) throw new ArgumentNullException(nameof(workItem));

            if (HostingEnvironment.IsHosted)
            {
                HostingEnvironment.QueueBackgroundWorkItem(workItem);
            }
            else
            {
                Task.Run(() => workItem(CancellationToken.None));
            }
        }
    }
}
