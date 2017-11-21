using EImece.Domain.Entities;
using EImece.Domain.Services.IServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EImece.Domain.Repositories.IRepositories;
using NLog;

namespace EImece.Domain.Services
{
    public class BrowserNotificationFeedBackService :
        BaseEntityService<BrowserNotificationFeedBack>, IBrowserNotificationFeedBackService
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private IBrowserNotificationFeedBackRepository BrowserNotificationFeedBackRepository { get; set; }

        public BrowserNotificationFeedBackService(IBrowserNotificationFeedBackRepository repository) : base(repository)
        {
            BrowserNotificationFeedBackRepository = repository;
        }
    }
}

