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
    public class BrowserNotificationService : BaseEntityService<BrowserNotification>, IBrowserNotificationService
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private IBrowserNotificationRepository BrowserNotificationRepository { get; set; }

        public BrowserNotificationService(IBrowserNotificationRepository repository) : base(repository)
        {
            BrowserNotificationRepository = repository;
        }
    }
}

