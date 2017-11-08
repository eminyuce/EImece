using EImece.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EImece.Domain.Helpers;
using EImece.Domain.Models.Enums;
using EImece.Domain.Models.HelperModels;

namespace EImece.Domain.Services.IServices
{
    public interface IBrowserSubscriptionService : IBaseEntityService<BrowserSubscription>
    {
        BrowserSubscription GetSubscriptionsByPublicApiKey(string applicationServerPublicKey);
    }
}
