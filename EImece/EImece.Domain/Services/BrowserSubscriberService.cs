using EImece.Domain.Entities;
using EImece.Domain.Repositories.IRepositories;
using EImece.Domain.Services.IServices;
using Ninject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharkDev.Web.Controls.TreeView.Model;
using NLog;
using EImece.Domain.Models.FrontModels;
using System.Data.Entity.Validation;
using EImece.Domain.Helpers;
using EImece.Domain.Models.Enums;
using EImece.Domain.Helpers.Extensions;
using System.Linq.Expressions;

namespace EImece.Domain.Services
{
    public class BrowserSubscriberService : BaseEntityService<BrowserSubscriber>, IBrowserSubscriberService
    {
      
        public IBrowserSubscriberRepository BrowserSubscriberRepository { get; set; }
        public BrowserSubscriberService(IBrowserSubscriberRepository browserSubscriberRepository) : base(browserSubscriberRepository)
        {
            BrowserSubscriberRepository = browserSubscriberRepository;
        }

        public List<BrowserSubscriber> GetBrowserSubscribers()
        {
            Expression<Func<BrowserSubscriber, object>> includeProperty3 = r => r.BrowserSubscription;
            Expression<Func<BrowserSubscriber, object>>[] includeProperties = { includeProperty3 };
            var items = BrowserSubscriberRepository.GetAllIncluding(includeProperties);
            items = items.OrderBy(r => r.Position).ThenByDescending(r => r.UpdatedDate);
            return items.ToList();
        }
    }
}
