using EImece.Domain.Entities;
using EImece.Domain.Services.IServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EImece.Domain.Repositories.IRepositories;

namespace EImece.Domain.Services
{
    public class SubscriberService : BaseEntityService<Subscriber>, ISubscriberService
    {

        private ISubscriberRepository SubscriberRepository { get; set; }
        public SubscriberService(ISubscriberRepository
            repository) : base(repository)
        {
            SubscriberRepository = repository;
        }
    }
}
