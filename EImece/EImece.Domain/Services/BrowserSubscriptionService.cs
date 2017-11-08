using EImece.Domain.Entities;
using EImece.Domain.Services.IServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EImece.Domain.Repositories.IRepositories;
using EImece.Domain.Models.Enums;
using EImece.Domain.Models.HelperModels;
using Ninject;
using EImece.Domain.Helpers;
using NLog;
using System.Linq.Expressions;
using GenericRepository.EntityFramework.Enums;
using System.Data.Entity.Validation;


namespace EImece.Domain.Services
{
    public class BrowserSubscriptionService : BaseEntityService<BrowserSubscription>, IBrowserSubscriptionService
    {
        public IBrowserSubscriptionRepository BrowserSubscriptionRepository { get; set; }
        public BrowserSubscriptionService(IBrowserSubscriptionRepository browserSubscriptionRepository) : base(browserSubscriptionRepository)
        {
            BrowserSubscriptionRepository = browserSubscriptionRepository;
        }

        public BrowserSubscription GetSubscriptionsByPublicApiKey(string applicationServerPublicKey)
        {
           return BrowserSubscriptionRepository.FindBy(r => r.PublicKey.Equals(applicationServerPublicKey, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
        }
    }
}
